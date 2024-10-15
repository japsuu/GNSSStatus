import { createChart, referenceLinePlugin, updateGraph, updateFixTypeChart } from './charts.js';
import { fetchData, dataToCsv } from './data.js';
import { getFixTypeName, downloadCSV } from './utils.js';

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

// Only used if autoScaleY is true
const deltaZChartDefaultMinY = -0.03;
const deltaZChartDefaultMaxY = 0.03;
const deltaXYChartDefaultMinY = 0;
const deltaXYChartDefaultMaxY = 0.03;

// Initialize charts
const deltaZChartCtx = document.getElementById('deltaZChart').getContext('2d');
const deltaZChart = createChart(deltaZChartCtx, 'line', {
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
}, {
  responsive: true,
  scales: {
    x: { display: true, title: { display: true, text: `Time (UTC+${-utcOffset})` } },
    y: { display: true, min: deltaZChartDefaultMinY, max: deltaZChartDefaultMaxY, title: { display: true, text: 'DeltaZ (m)' } }
  },
  plugins: {
    referenceLine: true,
    tooltip: {
      callbacks: {
        label: function (context) {
          const index = context.dataIndex;
          const value = context.formattedValue;
          const fixType = getFixTypeName(context.chart.data.datasets[0].fixType[index]);
          const utcTime = context.chart.data.datasets[0].datetime[index].toUTCString().slice(17, 25);
          return [`Value: ${value} m`, `FixType: ${fixType}`, `UTC: ${utcTime}`];
        }
      }
    }
  }
});

const deltaXYChartCtx = document.getElementById('deltaXYChart').getContext('2d');
const deltaXYChart = createChart(deltaXYChartCtx, 'line', {
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
}, {
  responsive: true,
  scales: {
    x: { display: true, title: { display: true, text: `Time (UTC+${-utcOffset})` } },
    y: { display: true, min: deltaXYChartDefaultMinY, max: deltaXYChartDefaultMaxY, title: { display: true, text: 'DeltaXY (m)' } }
  },
  plugins: {
    referenceLine: true,
    tooltip: {
      callbacks: {
        label: function (context) {
          const index = context.dataIndex;
          const value = context.formattedValue;
          const fixType = getFixTypeName(context.chart.data.datasets[0].fixType[index]);
          const utcTime = context.chart.data.datasets[0].datetime[index].toUTCString().slice(17, 25);
          return [`Value: ${value} m`, `FixType: ${fixType}`, `UTC: ${utcTime}`];
        }
      }
    }
  }
});

const fixTypeChartCtx = document.getElementById('fixTypeChart').getContext('2d');
const fixTypeChart = createChart(fixTypeChartCtx, 'pie', {
  labels: [getFixTypeName(0), getFixTypeName(5), getFixTypeName(4)],
  datasets: [{
    data: [],
    backgroundColor: ['red', 'yellow', 'green'],
    // Remove the border around the pie chart segments
    borderColor: 'white',
    borderWidth: 0
  }]
}, {
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
});

const autoScaleXCheckbox = document.getElementById('autoScaleXCheckbox');
const autoScaleYCheckbox = document.getElementById('autoScaleYCheckbox');
const showOnlyRtkFixCheckbox = document.getElementById('showOnlyRtkFixCheckbox');
const downloadButton = document.getElementById('downloadButton');
const datePicker = document.getElementById('datePicker');
const notification = document.getElementById('notification');

// Variables
// ------------------------------------------------------------
let autoScaleX = false;
let autoScaleY = false;
let showOnlyRtkFix = false;
let latestEntryId = 0;
let latestData;

async function refreshData() {
  const data = await fetchData(dataStartUtc);

  if (latestEntryId >= data.lastEntryId) {
    console.log('No new data received');
    return;
  }

  latestData = data;
  latestEntryId = data.lastEntryId;
  console.log('New data received:', latestData);
  refreshInterface();
}

function refreshInterface() {
  updateGraph(latestData, 'DeltaZ', deltaZChart, pointsPerGraph, autoScaleX, showOnlyRtkFix);
  updateGraph(latestData, 'DeltaXY', deltaXYChart, pointsPerGraph, autoScaleX, showOnlyRtkFix);
  updateTextData(latestData);
  updateFixTypeChart(latestData, fixTypeChart);
}

function updateTextData(data) {
  const latestFeed = data.feeds[data.feeds.length - 1];

  document.getElementById('TimeUtc').textContent = latestFeed.datetime.toTimeString();
  document.getElementById('FixType').textContent = getFixTypeName(latestFeed.gnss.FixType);
  document.getElementById('SatellitesInUse').textContent = latestFeed.gnss.SatellitesInUse;
  document.getElementById('PDop').textContent = latestFeed.gnss.PDop;
  document.getElementById('HDop').textContent = latestFeed.gnss.HDop;
  document.getElementById('VDop').textContent = latestFeed.gnss.VDop;
  document.getElementById('ErrorLatitude').textContent = latestFeed.gnss.ErrorLatitude;
  document.getElementById('ErrorLongitude').textContent = latestFeed.gnss.ErrorLongitude;
  document.getElementById('ErrorAltitude').textContent = latestFeed.gnss.ErrorAltitude;
  document.getElementById('BaseRoverDistance').textContent = `${latestFeed.gnss.BaseRoverDistance} m`;
}

autoScaleXCheckbox.checked = autoScaleX;
autoScaleYCheckbox.checked = autoScaleY;
showOnlyRtkFixCheckbox.checked = showOnlyRtkFix;
datePicker.value = siteRefreshDate.toISOString().slice(0, 10);

autoScaleXCheckbox.addEventListener('change', () => {
  autoScaleX = autoScaleXCheckbox.checked;
  refreshInterface();
});

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

showOnlyRtkFixCheckbox.addEventListener('change', () => {
  showOnlyRtkFix = showOnlyRtkFixCheckbox.checked;
  refreshInterface();
});

downloadButton.addEventListener('click', async () => {
  const selectedDate = new Date(datePicker.value);
  const dayStartLocal = new Date(selectedDate.getFullYear(), selectedDate.getMonth(), selectedDate.getDate(), 0, 0, 0, 0);
  const dataStartUtc = dayStartLocal.toISOString().slice(0, 19) + 'Z';

  if (isNaN(dayStartLocal.getTime())) {
    alert('Please select a valid date.');
    return;
  }

  notification.classList.remove('hidden');
  notification.textContent = 'Downloading data...';

  try {
    const data = await fetchData(dataStartUtc);
    const csvData = dataToCsv(data);
    downloadCSV(csvData, `gnss_data_${dataStartUtc}.csv`);
    notification.textContent = 'Download complete.';
  } catch (error) {
    console.error('Error downloading data:', error);
    alert('Failed to download data. Please try again.');
    notification.textContent = 'Download failed.';
  } finally {
    setTimeout(() => {
      notification.classList.add('hidden');
    }, 3000);
  }
});

refreshData();
setInterval(refreshData, refreshInterval * 1000);
