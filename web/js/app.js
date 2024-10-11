const apiKey = "WQNA71V5DYQRO3BV"; // Public read API key
const channelId = "2691494"; // ThingSpeak channel ID
const maxResults = 288; // 15 sec intervals over 24 hours: 24 * 60 * 60 / 15
const dataFetchUrl = `https://api.thingspeak.com/channels/${channelId}/feeds.json?api_key=${apiKey}&results=${maxResults}`;

const refreshInterval = 15000; // milliseconds
const oldDataWarningThreshold = 60; // seconds

const deltaZChartCtx = document.getElementById('deltaZChart').getContext('2d');
const deltaZChart = new Chart(deltaZChartCtx, {
  type: 'line',
  data: {
    labels: [],
    datasets: [{
      label: 'DeltaZ',
      data: [],
      pointBackgroundColor: [],
      borderColor: 'black',
      backgroundColor: 'black',
      borderWidth: 2,
      pointBorderWidth: 0,
      pointRadius: 5,
      pointHitRadius: 10,
      fill: false
    }]
  },
  options: {
    responsive: true,
    scales: {
      x: {display: true, title: {display: true, text: 'Time (UTC)'}},
      y: {display: true, title: {display: true, text: 'DeltaZ (m)'}}
    }
  }
});

const deltaXYChartCtx = document.getElementById('deltaXYChart').getContext('2d');
const deltaXYChart = new Chart(deltaXYChartCtx, {
  type: 'line',
  data: {
    labels: [],
    datasets: [{
      label: 'DeltaXY',
      data: [],
      pointBackgroundColor: [],
      borderColor: 'black',
      backgroundColor: 'black',
      borderWidth: 2,
      pointBorderWidth: 0,
      pointRadius: 5,
      pointHitRadius: 10,
      fill: false
    }]
  },
  options: {
    responsive: true,
    scales: {
      x: {display: true, title: {display: true, text: 'Time (UTC)'}},
      y: {display: true, min:0, title: {display: true, text: 'DeltaXY (m)'}}
    }
  }
});

const fixTypeChartCtx = document.getElementById('fixTypeChart').getContext('2d');
const fixTypeChart = new Chart(fixTypeChartCtx, {
  type: 'pie',
  data: {
    labels: ['No Fix', '2D Fix', '3D Fix', 'DGPS Fix', 'RTK Fix'],
    datasets: [{
      data: [],
      backgroundColor: ['red', 'orange', 'yellow', 'green', 'blue']
    }]
  },
  options: {
    responsive: true,
    plugins: {
      legend: {
        position: 'top',
      },
      title: {
        display: true,
        text: 'GNSS Fix Types Duration'
      }
    }
  }
});

let lastDataFetchTime;
let lastNewDataReceiveTime;

async function fetchData() {
  try {
    const response = await fetch(dataFetchUrl);
    const json = await response.json();
    lastDataFetchTime = new Date();

    const data = {
      feeds: json.feeds.map(feed => {
        const f1 = feed.field1;
        const f2 = feed.field2;
        const f3 = feed.field3;
        const f4 = feed.field4;
        const gnssData = Object.assign({}, JSON.parse(f1), JSON.parse(f2), JSON.parse(f3), JSON.parse(f4));

        const timeUtc = gnssData.TimeUtc;
        const time = `${timeUtc.slice(0, 2)}:${timeUtc.slice(2, 4)}:${timeUtc.slice(4, 6)}`;

        let datetime = new Date();
        let [hours, minutes, seconds] = time.split(':');
        datetime.setUTCHours(hours);
        datetime.setUTCMinutes(minutes);
        datetime.setUTCSeconds(seconds);

        return {
          gnss: gnssData,
          time: time,
          datetime: datetime
        }
      })
    };

    const lastEntry = data.feeds[data.feeds.length - 1];
    lastNewDataReceiveTime = lastEntry.datetime;

    updateGraph(data, 'DeltaZ', deltaZChart);
    updateGraph(data, 'DeltaXY', deltaXYChart);
    updateTextData(data);
    updateFixTypeChart(data);
  } catch (error) {
    console.error('Error fetching data:', error);
  }
}

function updateGraph(data, dataKey, chart) {
  const feeds = data.feeds;
  const interval = 15 * 60 * 1000; // 15 minutes in milliseconds
  const dataPoints = [];
  const pointLabels = [];
  const pointColors = [];

  let intervalStart = feeds[0].datetime.getTime();
  let intervalEnd = intervalStart + interval;
  let intervalData = [];
  let intervalFixTypes = new Set();

  feeds.forEach(feed => {
    const feedTime = feed.datetime.getTime();

    if (feedTime >= intervalStart && feedTime < intervalEnd) {
      intervalData.push(feed.gnss[dataKey]);
      intervalFixTypes.add(feed.gnss.FixType);
    } else {
      if (intervalData.length > 0) {
        const avgValue = intervalData.reduce((sum, value) => sum + value, 0) / intervalData.length;
        dataPoints.push(avgValue);

        const startDate = new Date(intervalStart).toISOString().slice(11, 16); // HH:mm format
        const endDate = new Date(intervalEnd).toISOString().slice(11, 16); // HH:mm format
        pointLabels.push(`${startDate} - ${endDate}`);
        pointColors.push(determineIntervalColor(intervalFixTypes));
      }

      intervalStart = intervalEnd;
      intervalEnd = intervalStart + interval;
      intervalData = [feed.gnss[dataKey]];
      intervalFixTypes = new Set([feed.gnss.FixType]);
    }
  });

  // Handle the last interval
  if (intervalData.length > 0) {
    const avgValue = intervalData.reduce((sum, value) => sum + value, 0) / intervalData.length;
    dataPoints.push(avgValue);

    const startDate = new Date(intervalStart).toISOString().slice(11, 16); // HH:mm format
    // Cannot use intervalEnd, as it might be past the last feed's time
    const endDate = new Date(feeds[feeds.length - 1].datetime).toISOString().slice(11, 16); // HH:mm format
    pointLabels.push(`${startDate} - ${endDate}`);
    pointColors.push(determineIntervalColor(intervalFixTypes));
  }

  chart.data.labels = pointLabels;
  chart.data.datasets[0].data = dataPoints;
  chart.data.datasets[0].pointBackgroundColor = pointColors;
  chart.update();
}

function updateFixTypeChart(data) {
  const feeds = data.feeds;
  const fixTypeDurations = { 1: 0, 2: 0, 3: 0, 4: 0, 5: 0 };

  for (let i = 1; i < feeds.length; i++) {
    const currentFeed = feeds[i];
    const previousFeed = feeds[i - 1];
    const duration = (currentFeed.datetime - previousFeed.datetime) / 1000; // duration in seconds
    const fixType = previousFeed.gnss.FixType;

    if (fixTypeDurations[fixType] !== undefined) {
      fixTypeDurations[fixType] += duration;
    }
  }

  fixTypeChart.data.datasets[0].data = [
    fixTypeDurations[1],
    fixTypeDurations[2],
    fixTypeDurations[3],
    fixTypeDurations[4],
    fixTypeDurations[5]
  ];
  fixTypeChart.update();
}

function updateTextData(data) {
  const latestFeed = data.feeds[data.feeds.length - 1];

  document.getElementById('TimeUtc').textContent = latestFeed.datetime.toTimeString();
  document.getElementById('FixType').textContent = latestFeed.gnss.FixType;
  document.getElementById('SatellitesInUse').textContent = latestFeed.gnss.SatellitesInUse;
  document.getElementById('SatellitesInView').textContent = latestFeed.gnss.SatellitesInView;
  document.getElementById('PDop').textContent = latestFeed.gnss.PDop;
  document.getElementById('HDop').textContent = latestFeed.gnss.HDop;
  document.getElementById('VDop').textContent = latestFeed.gnss.VDop;
  document.getElementById('ErrorLatitude').textContent = latestFeed.gnss.ErrorLatitude;
  document.getElementById('ErrorLongitude').textContent = latestFeed.gnss.ErrorLongitude;
  document.getElementById('ErrorAltitude').textContent = latestFeed.gnss.ErrorAltitude;
}

function updateTimeToRefresh() {
  if (!lastDataFetchTime)
    return;

  const now = new Date();
  const refreshIn = Math.round((lastDataFetchTime.getTime() + refreshInterval - now.getTime()) / 1000);
  const timeElement = document.getElementById('TimeToNextRefresh');

  timeElement.textContent = `Refreshing in ${refreshIn}...`;
}

function updateTimeAgo() {
  if (!lastNewDataReceiveTime)
    return;

  const now = new Date();
  const secondsAgo = Math.floor((now - lastNewDataReceiveTime) / 1000);
  const warningPopup = document.getElementById('warning-popup');

  if (secondsAgo > oldDataWarningThreshold) {
    warningPopup.classList.remove('hidden');
  } else {
    warningPopup.classList.add('hidden');
  }
}

function determineIntervalColor(fixTypes) {
  if (fixTypes.size === 1) {
    const fixType = [...fixTypes][0];
    if (fixType === 4) return 'green';
    if (fixType === 5) return 'yellow';
    return 'red';
  }
  return 'blue';
}

setInterval(updateTimeToRefresh, 1000);
setInterval(updateTimeAgo, 1000);
setInterval(fetchData, refreshInterval);
fetchData();
