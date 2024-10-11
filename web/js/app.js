const apiKey = "WQNA71V5DYQRO3BV"; // Public read API key
const channelId = "2691494"; // ThingSpeak channel ID
const maxResults = 288; // 15 sec intervals over 24 hours: 24 * 60 * 60 / 15
const dataFetchUrl = `https://api.thingspeak.com/channels/${channelId}/feeds.json?api_key=${apiKey}&results=${maxResults}`;

const refreshInterval = 15000; // milliseconds
const oldDataWarningThreshold = 60; // seconds

const deltaZChartCtx = document.getElementById('deltaZChart').getContext('2d');

// Set up Chart.js for DeltaZ graph
const deltaZChart = new Chart(deltaZChartCtx, {
  type: 'line',
  data: {
    labels: [],
    datasets: [{
      label: 'DeltaZ',
      data: [],
      pointBackgroundColor: [],
      borderColor: [],
      //borderColor: 'rgba(75, 192, 192, 1)',
      borderWidth: 2,
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

let lastDataFetchTime;
let lastNewDataReceiveTime;

async function fetchData() {

  try {
    const response = await fetch(dataFetchUrl);
    const json = await response.json();
    lastDataFetchTime = new Date();

    // Construct an array of GNSS data from the JSON response
    const data = {
      feeds: json.feeds.map(feed => {
        // The data is stored in multiple fields, so we need to combine them into one JSON object
        const f1 = feed.field1;
        const f2 = feed.field2;
        const f3 = feed.field3;
        const f4 = feed.field4;
        const gnssData = Object.assign({}, JSON.parse(f1), JSON.parse(f2), JSON.parse(f3), JSON.parse(f4));

        // Convert the hhmmss.ss(ss) TimeUtc to hh:mm:ss
        const timeUtc = gnssData.TimeUtc;
        const time = `${timeUtc.slice(0, 2)}:${timeUtc.slice(2, 4)}:${timeUtc.slice(4, 6)}`;

        // Construct a proper Date object from the TimeUtc string
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
    console.log('Data:', data);

    const lastEntry = data.feeds[data.feeds.length - 1];
    lastNewDataReceiveTime = lastEntry.datetime;

    updateDeltaZGraph(data);
    updateTextData(data);
  } catch (error) {
    console.error('Error fetching data:', error);
  }
}

function updateDeltaZGraph(data) {
  const feeds = data.feeds;
  const deltaZData = [];
  const labels = [];
  const pointColors = [];

  feeds.forEach(feed => {

    if (feed.gnss.DeltaZ !== undefined) {
      deltaZData.push(feed.gnss.DeltaZ);
      labels.push(feed.time);
      pointColors.push(getPointColor(feed.gnss));
    }
  });

  deltaZChart.data.labels = labels;
  deltaZChart.data.datasets[0].data = deltaZData;
  deltaZChart.data.datasets[0].pointBackgroundColor = pointColors;
  deltaZChart.data.datasets[0].borderColor = pointColors;
  deltaZChart.update();
}

function getPointColor(gnss){
  // Quality 4 = green, 5 = yellow, others = red
  if (gnss.FixType === 4) {
    return 'green';
  }
  if (gnss.FixType === 5) {
    return 'yellow';
  }

  return 'red';
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

setInterval(updateTimeToRefresh, 1000);
setInterval(updateTimeAgo, 1000);
setInterval(fetchData, refreshInterval);
fetchData();
