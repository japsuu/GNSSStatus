import { createChart, updateGraph, updateFixTypeChart } from './charts.js';
import { fetchData, dataToCsv } from './data.js';
import { getFixTypeName, downloadCSV } from './utils.js';

// User configuration
// ------------------------------------------------------------
const refreshInterval = 15; // seconds
const defaultChartYRange = 0.03;
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

const returnValueIfSkip = (segmentCtx, value) => segmentCtx.p0.skip || segmentCtx.p1.skip ? value : undefined;
const tryGetLineFixColor = function (segmentCtx) {
  const p0Index = segmentCtx.p0DataIndex;
  const p1Index = segmentCtx.p1DataIndex;

  const p0FixType = segmentCtx.chart.data.datasets[0].fixType[p0Index];
  const p1FixType = segmentCtx.chart.data.datasets[0].fixType[p1Index];

  // Both RTK Fix
  if (p0FixType === 4 && p1FixType === 4) {
    return 'green';
  }

  // Both RTK Float
  if (p0FixType === 5 && p1FixType === 5) {
    return 'yellow';
  }

  // Other
  return 'red';
};

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
    pointHoverRadius: 8,
    segment: {
      borderColor: ctx => returnValueIfSkip(ctx, 'rgb(0,0,0,0.2)') || tryGetLineFixColor(ctx),
      borderDash: ctx => returnValueIfSkip(ctx, [6, 6]),
    },
    spanGaps: true
  }]
}, {
  responsive: true,
  scales: {
    x: { display: true, title: { display: true, text: `Time (UTC+${-utcOffset})` } },
    y: { display: true, min: -defaultChartYRange, max: defaultChartYRange, title: { display: true, text: 'DeltaZ (m)' } }
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
    pointHoverRadius: 8,
    segment: {
      borderColor: ctx => returnValueIfSkip(ctx, 'rgb(0,0,0,0.2)') || tryGetLineFixColor(ctx),
      borderDash: ctx => returnValueIfSkip(ctx, [6, 6]),
    },
    spanGaps: true
  }]
}, {
  responsive: true,
  scales: {
    x: { display: true, title: { display: true, text: `Time (UTC+${-utcOffset})` } },
    y: { display: true, min: 0, max: defaultChartYRange, title: { display: true, text: 'DeltaXY (m)' } }
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
const manualYRangeInput = document.getElementById('manualYRangeInput');
const showOnlyRtkFixCheckbox = document.getElementById('showOnlyRtkFixCheckbox');
const showThresholdInput = document.getElementById('showThresholdInput');
const downloadButton = document.getElementById('downloadButton');
const datePicker = document.getElementById('datePicker');
const notification = document.getElementById('notification');

// Variables
// ------------------------------------------------------------
let autoScaleX = false;
let autoScaleY = false;
let manualYRange = defaultChartYRange;
let showOnlyRtkFix = false;
let showThreshold = 100;
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
  updateGraph(latestData, 'DeltaZ', deltaZChart, pointsPerGraph, autoScaleX, showOnlyRtkFix, showThreshold);
  updateGraph(latestData, 'DeltaXY', deltaXYChart, pointsPerGraph, autoScaleX, showOnlyRtkFix, showThreshold);
  updateTextData(latestData);
  updateFixTypeChart(latestData, fixTypeChart);
}

function updateTextData(data) {
  const latestFeed = data.feeds[data.feeds.length - 1];

  const deltaZ = latestFeed.gnss.DeltaZ.toFixed(3);
  const deltaXY = latestFeed.gnss.DeltaXY.toFixed(3);
  const ionoRaw = latestFeed.gnss.Ionosphere;
  const iono = ionoRaw === undefined ? 'N/A' : ionoRaw;

  // Deltas pruned to 3 decimal places
  document.getElementById('DeltaZ').textContent = `${deltaZ} m`;
  document.getElementById('DeltaZTitle').textContent = `${deltaZ} m`;
  document.getElementById('DeltaXY').textContent = `${deltaXY} m`;
  document.getElementById('DeltaXYTitle').textContent = `${deltaXY} m`;
  document.getElementById('Ionosphere').textContent = `${iono} %`;

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

function updateGraphRanges(){
  if (autoScaleY) {
    deltaZChart.options.scales.y.min = undefined;
    deltaZChart.options.scales.y.max = undefined;
    deltaXYChart.options.scales.y.min = 0;
    deltaXYChart.options.scales.y.max = undefined;
  } else {
    deltaZChart.options.scales.y.min = -manualYRange;
    deltaZChart.options.scales.y.max = manualYRange;
    deltaXYChart.options.scales.y.min = 0;
    deltaXYChart.options.scales.y.max = manualYRange;
  }

  deltaZChart.update();
  deltaXYChart.update();
}

autoScaleXCheckbox.checked = autoScaleX;
autoScaleYCheckbox.checked = autoScaleY;
manualYRangeInput.value = manualYRange;
showOnlyRtkFixCheckbox.checked = showOnlyRtkFix;
showThresholdInput.value = showThreshold;
datePicker.value = siteRefreshDate.toISOString().slice(0, 10);

autoScaleXCheckbox.addEventListener('change', () => {
  autoScaleX = autoScaleXCheckbox.checked;
  refreshInterface();
});

autoScaleYCheckbox.addEventListener('change', () => {
  const autoScale = autoScaleYCheckbox.checked;
  autoScaleY = autoScale;

  updateGraphRanges()
});

manualYRangeInput.addEventListener('change', () => {
  manualYRange = parseFloat(manualYRangeInput.value);
  autoScaleYCheckbox.checked = false;
  autoScaleY = false;
  updateGraphRanges()
});

showOnlyRtkFixCheckbox.addEventListener('change', () => {
  showOnlyRtkFix = showOnlyRtkFixCheckbox.checked;
  refreshInterface();
});

showThresholdInput.addEventListener('change', () => {
  showThreshold = parseFloat(showThresholdInput.value);
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
