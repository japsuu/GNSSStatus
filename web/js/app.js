// User configuration
// ------------------------------------------------------------
const refreshInterval = 15; // seconds
//const oldDataWarningThreshold = 120; // seconds

// Constants
// ------------------------------------------------------------
const siteRefreshDate = new Date(); // Date when the site was last refreshed
const pointsPerGraph = 24 * 60 * 60 / refreshInterval;
console.log('Points per graph:', pointsPerGraph);

// Data fetch start UTC date in format YYYY-MM-DD%20HH:NN:SS
const dayStartLocal = new Date(siteRefreshDate.getFullYear(), siteRefreshDate.getMonth(), siteRefreshDate.getDate(), 0, 0, 0, 0);
const dataStartUtc = dayStartLocal.toISOString().slice(0, 19) + 'Z';
const utcOffset = siteRefreshDate.getTimezoneOffset() / 60;
console.log('Day start UTC:', dataStartUtc);

const apiKey = "WQNA71V5DYQRO3BV"; // Public read API key
const dataFetchUrl = `https://api.thingspeak.com/channels/2691494/feeds.json?api_key=${apiKey}&start=${dataStartUtc}`;

// Only used if autoScaleY is true
const deltaZChartDefaultMinY = -0.03;
const deltaZChartDefaultMaxY = 0.03;
const deltaXYChartDefaultMinY = 0;
const deltaXYChartDefaultMaxY = 0.03;

const referenceLinePlugin = {
  id: 'referenceLine',
  beforeDraw: (chart) => {
    const ctx = chart.ctx;
    const yScale = chart.scales.y;

    if (!yScale) {
      return; // Exit if yScale is not defined
    }

    const yValue = 0; // Y-value where the reference line should be drawn
    const yRange = 0.02; // Range for the highlighted area

    // Get the pixel values for the Y-value and the range
    const yPixel = yScale.getPixelForValue(yValue);
    const yPixelMin = yScale.getPixelForValue(yValue - yRange);
    const yPixelMax = yScale.getPixelForValue(yValue + yRange);

    // Draw the red background
    ctx.save();
    ctx.beginPath();
    ctx.rect(chart.chartArea.left, chart.chartArea.top, chart.chartArea.right - chart.chartArea.left, chart.chartArea.bottom - chart.chartArea.top);
    ctx.clip();
    ctx.fillStyle = 'rgba(255, 0, 0, 0.2)'; // Color of the red background with transparency
    ctx.fillRect(chart.chartArea.left, chart.chartArea.top, chart.chartArea.right - chart.chartArea.left, chart.chartArea.bottom - chart.chartArea.top);

    // Draw the green "safe" area
    ctx.fillStyle = 'rgba(0, 255, 0, 0.2)'; // Color of the green "safe" area with transparency
    ctx.fillRect(chart.chartArea.left, yPixelMax, chart.chartArea.right - chart.chartArea.left, yPixelMin - yPixelMax);

    // Draw the reference line
    ctx.beginPath();
    ctx.moveTo(chart.chartArea.left, yPixel);
    ctx.lineTo(chart.chartArea.right, yPixel);
    ctx.strokeStyle = 'green'; // Color of the reference line
    ctx.lineWidth = 2; // Width of the reference line
    ctx.stroke();
    ctx.restore();
  }
};

// Register the plugin with ChartJS
Chart.register(referenceLinePlugin);

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
      pointRadius: 0,
      pointHitRadius: 10,
      fill: false,
      pointHoverRadius: 8
    }]
  },
  options: {
    responsive: true,
    scales: {
      x: {display: true, title: {display: true, text: `Time (UTC+${-utcOffset})`}},
      y: {display: true, min: deltaZChartDefaultMinY, max: deltaZChartDefaultMaxY, title: {display: true, text: 'DeltaZ (m)'}}
    },
    plugins: {
      referenceLine: true,
      tooltip: {
        callbacks: {
          label: function(context) {
            const index = context.dataIndex;
            const value = context.formattedValue;
            const fixType = getFixTypeName(latestData.feeds[index].gnss.FixType);
            const utcTime = latestData.feeds[index].datetime.toUTCString().slice(17, 25);
            return [`Value: ${value} m`, `FixType: ${fixType}`, `UTC: ${utcTime}`];
          }
        }
      }
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
      pointRadius: 0,
      pointHitRadius: 10,
      fill: false,
      pointHoverRadius: 8
    }]
  },
  options: {
    responsive: true,
    scales: {
      x: {display: true, title: {display: true, text: `Time (UTC+${-utcOffset})`}},
      y: {display: true, min: deltaXYChartDefaultMinY, max: deltaXYChartDefaultMaxY, title: {display: true, text: 'DeltaXY (m)'}}
    },
    plugins: {
      referenceLine: true,
      tooltip: {
        callbacks: {
          label: function(context) {
            const index = context.dataIndex;
            const value = context.formattedValue;
            const fixType = getFixTypeName(latestData.feeds[index].gnss.FixType);
            const utcTime = latestData.feeds[index].datetime.toUTCString().slice(17, 25);
            return [`Value: ${value} m`, `FixType: ${fixType}`, `UTC: ${utcTime}`];
          }
        }
      }
    }
  }
});

const fixTypeChartCtx = document.getElementById('fixTypeChart').getContext('2d');
const fixTypeChart = new Chart(fixTypeChartCtx, {
  type: 'pie',
  data: {
    labels: [getFixTypeName(0), getFixTypeName(1), getFixTypeName(2), getFixTypeName(3), getFixTypeName(4), getFixTypeName(5), getFixTypeName(6)],
    datasets: [{
      data: [],
      backgroundColor: ['red', 'red', 'red', 'red', 'green', 'yellow', 'red']
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
        text: 'GNSS Fix Types Duration (seconds)'
      }
    }
  }
});

const autoScaleXCheckbox = document.getElementById('autoScaleXCheckbox');
const autoScaleYCheckbox = document.getElementById('autoScaleYCheckbox');

// Variables
// ------------------------------------------------------------
let lastDataFetchTime;
let lastNewDataReceiveTime;
let latestEntryId = 0;
let latestData;
let autoScaleX = false;
let autoScaleY = false;

async function fetchData() {
  console.log('----- Fetching data -----');
  try {
    const response = await fetch(dataFetchUrl);
    const json = await response.json();

    lastDataFetchTime = new Date();
    const lastEntryId = json.channel.last_entry_id;
    // console.log('Entry ID:', lastEntryId, 'Cached entry ID:', latestEntryId);

    if (latestEntryId >= lastEntryId) {
      // console.log('No new data received');
      return;
    }

    latestEntryId = lastEntryId;

    const data = {
      feeds: json.feeds.map(feed => {
        const f1 = feed.field1;
        const f2 = feed.field2;
        const f3 = feed.field3;
        const f4 = feed.field4;
        const gnssData = Object.assign({}, JSON.parse(f1), JSON.parse(f2), JSON.parse(f3), JSON.parse(f4));

        const timeUtc = gnssData.TimeUtc;
        const date = feed.created_at.split('T')[0]; // Extract the date part from the created_at field
        const time = `${timeUtc.slice(0, 2)}:${timeUtc.slice(2, 4)}:${timeUtc.slice(4, 6)}`;

        const datetime = new Date(`${date}T${time}Z`); // Combine date and time

        return {
          gnss: gnssData,
          datetime: datetime
        }
      })
    };

    console.log('New data received:', data);
    lastNewDataReceiveTime = new Date();
    latestData = data;

    refreshInterface();

  } catch (error) {
    console.error('Error fetching data:', error);
  }
}

function refreshInterface(){
  updateGraph(latestData, 'DeltaZ', deltaZChart);
  updateGraph(latestData, 'DeltaXY', deltaXYChart);
  updateTextData(latestData);
  updateFixTypeChart(latestData);
}

function getPointColor(fixType){
  fixType = parseInt(fixType);

  // Quality 4 = green, 5 = yellow, others = red
  if (fixType === 4) {
    return 'green';
  }
  if (fixType === 5) {
    return 'yellow';
  }

  return 'red';
}

function getFixTypeName(fixType) {

  fixType = parseInt(fixType);

  switch (fixType) {
    case 0:
      return 'No Fix';
    case 1:
      return 'GPS Fix';
    case 2:
      return 'Differential GPS Fix';
    case 3:
      return 'Not Applicable';
    case 4:
      return 'RTK Fix';
    case 5:
      return 'RTK Float';
    case 6:
      return 'INS Dead Reckoning';
    default:
      return 'Unknown';
  }
}

function updateGraph(data, dataKey, chart) {
  const feeds = data.feeds;

  // Init with pointsPerGraph empty points
  const dataPoints = [];
  const pointLabels = [];
  const pointColors = [];
  const pointRadius = [];

  let maxIndex = -1;
  let minIndex = -1;
  let maxValue = -Infinity;
  let minValue = Infinity;

  let pointCount = 0;

  feeds.forEach(feed => {
    if (feed.gnss[dataKey] !== undefined) {
      const pData = feed.gnss[dataKey];
      const pLabel = feed.datetime.toTimeString().slice(0, 8);
      const pColor = getPointColor(feed.gnss.FixType);

      dataPoints.push(pData);
      pointLabels.push(pLabel);
      pointColors.push(pColor);
      pointRadius.push(0);

      const lastIndex = dataPoints.length - 1;
      // Check for max and min values
      if (pData > maxValue) {
        maxValue = pData;
        maxIndex = lastIndex;
      }
      if (pData < minValue) {
        minValue = pData;
        minIndex = lastIndex;
      }

      pointCount++;
    }
  });

  // If autoScaleX is false, fill the graph with all the remaining empty points
  let count = pointsPerGraph - pointCount;
  if (!autoScaleX) {
    for (let i = 0; i < count; i++) {
      dataPoints.push(null);
      pointLabels.push('');
      pointColors.push('black');
      pointRadius.push(0);
    }
  }

  // Highlight the highest and lowest points
  if (maxIndex !== -1) {
    pointRadius[maxIndex] = 5; // Increase the radius for the highest point
    pointColors[maxIndex] = 'red'; // Change color for the highest point
  }
  if (minIndex !== -1) {
    pointRadius[minIndex] = 5; // Increase the radius for the lowest point
    pointColors[minIndex] = 'red'; // Change color for the lowest point
  }

  chart.data.labels = pointLabels;
  chart.data.datasets[0].data = dataPoints;
  chart.data.datasets[0].pointBackgroundColor = pointColors;
  chart.data.datasets[0].pointRadius = pointRadius;
  chart.update();
}

function updateFixTypeChart(data) {
  const feeds = data.feeds;
  const fixTypeDurations = { 0: 0, 1: 0, 2: 0, 3: 0, 4: 0, 5: 0, 6: 0 };

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
    fixTypeDurations[0],
    fixTypeDurations[1],
    fixTypeDurations[2],
    fixTypeDurations[3],
    fixTypeDurations[4],
    fixTypeDurations[5],
    fixTypeDurations[6]
  ];
  fixTypeChart.update();
}

function updateTextData(data) {
  const latestFeed = data.feeds[data.feeds.length - 1];

  document.getElementById('TimeUtc').textContent = latestFeed.datetime.toTimeString();
  document.getElementById('FixType').textContent = getFixTypeName(latestFeed.gnss.FixType);
  document.getElementById('SatellitesInUse').textContent = latestFeed.gnss.SatellitesInUse;
  document.getElementById('SatellitesInView').textContent = latestFeed.gnss.SatellitesInView;
  document.getElementById('PDop').textContent = latestFeed.gnss.PDop;
  document.getElementById('HDop').textContent = latestFeed.gnss.HDop;
  document.getElementById('VDop').textContent = latestFeed.gnss.VDop;
  document.getElementById('ErrorLatitude').textContent = latestFeed.gnss.ErrorLatitude;
  document.getElementById('ErrorLongitude').textContent = latestFeed.gnss.ErrorLongitude;
  document.getElementById('ErrorAltitude').textContent = latestFeed.gnss.ErrorAltitude;
  document.getElementById('BaseRoverDistance').textContent = `${latestFeed.gnss.BaseRoverDistance} m`;
}

/*function updateTimeToRefresh() {
  if (!lastDataFetchTime)
    return;

  const now = new Date();
  let refreshIn = Math.round((lastDataFetchTime.getTime() + refreshInterval * 1000 - now.getTime()) / 1000);
  const timeElement = document.getElementById('TimeToNextRefresh');

  if (refreshIn <= 0)
    refreshIn = 0;

  timeElement.textContent = `Refreshing in ${refreshIn}...`;
}*/

/*function updateOldDataWarning() {
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
}*/

//setInterval(updateTimeToRefresh, 1000);
//setInterval(updateOldDataWarning, 1000);

autoScaleXCheckbox.checked = autoScaleX;
autoScaleYCheckbox.checked = autoScaleY;

// Add an event listener to the stretch checkbox
autoScaleXCheckbox.addEventListener('change', () => {
  autoScaleX = autoScaleXCheckbox.checked;
  refreshInterface();
});

// Add an event listener to the auto-scale Y checkbox
autoScaleYCheckbox.addEventListener('change', () => {
  const autoScale = autoScaleYCheckbox.checked;
  autoScaleY = autoScale;

  if (autoScale) {
    deltaZChart.options.scales.y.min = undefined;
    deltaZChart.options.scales.y.max = undefined;
    deltaXYChart.options.scales.y.min = 0;
    deltaXYChart.options.scales.y.max = undefined;
  } else {
    deltaZChart.options.scales.y.min = deltaZChartDefaultMinY;
    deltaZChart.options.scales.y.max = deltaZChartDefaultMaxY;
    deltaXYChart.options.scales.y.min = deltaXYChartDefaultMinY;
    deltaXYChart.options.scales.y.max = deltaXYChartDefaultMaxY;
  }

  deltaZChart.update();
  deltaXYChart.update();
});

fetchData();

setInterval(fetchData, refreshInterval * 1000);
