import {createChart, updateFixTypeChart, updateGraph} from './charts.js';
import {dataToCsv, fetchData} from './data.js';
import {downloadCSV, getFixTypeName} from './utils.js';

// User configuration
// ------------------------------------------------------------
const refreshInterval = 15; // seconds
const defaultChartYRange = 0.03;
const oldDataWarningThreshold = 5 * 60; // seconds

// Constants
// ------------------------------------------------------------
const siteRefreshDate = new Date(); // Date when the site was last refreshed
const pointsPerGraph = 24 * 60 * 60 / refreshInterval;
console.log('Points per graph:', pointsPerGraph);
// Data fetch start UTC date in format YYYY-MM-DD%20HH:NN:SS
const dayStartLocal = new Date(siteRefreshDate.getFullYear(), siteRefreshDate.getMonth(), siteRefreshDate.getDate(), 0, 0, 0, 0);
const utcOffset = siteRefreshDate.getTimezoneOffset() / 60;

const returnValueIfSkip = (segmentCtx, value) => segmentCtx.p0.skip || segmentCtx.p1.skip ? value : undefined;
const tryGetLineFixColor = function (segmentCtx) {
  const p0Index = segmentCtx.p0DataIndex;
  const p1Index = segmentCtx.p1DataIndex;

  const p0FixType = segmentCtx.chart.data.datasets[0].fixType[p0Index];
  const p1FixType = segmentCtx.chart.data.datasets[0].fixType[p1Index];

  // Both RTK Fix
  if (p0FixType === 1 && p1FixType === 1) {
    return 'green';
  }

  // Both RTK Float
  if (p0FixType === 2 && p1FixType === 2) {
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
    legend: {
      display: false
    },
    tooltip: {
      callbacks: {
        label: function (context) {
          const index = context.dataIndex;
          const value = context.formattedValue;
          const fixType = getFixTypeName(context.chart.data.datasets[0].fixType[index]);
          const utcTime = context.chart.data.datasets[0].datetime[index].toUTCString().slice(17, 25);
          let iono = context.chart.data.datasets[0].ionos[index];
          iono = iono === undefined ? 'N/A' : iono;
          return [`Value: ${value} m`, `FixType: ${fixType}`, `UTC: ${utcTime}`, `Iono: ${iono} %`];
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
    legend: {
      display: false
    },
    tooltip: {
      callbacks: {
        label: function (context) {
          const index = context.dataIndex;
          const value = context.formattedValue;
          const fixType = getFixTypeName(context.chart.data.datasets[0].fixType[index]);
          const utcTime = context.chart.data.datasets[0].datetime[index].toUTCString().slice(17, 25);
          let iono = context.chart.data.datasets[0].ionos[index];
          iono = iono === undefined ? 'N/A' : iono;
          return [`Value: ${value} m`, `FixType: ${fixType}`, `UTC: ${utcTime}`, `Iono: ${iono} %`];
        }
      }
    }
  }
});

const fixTypeChartCtx = document.getElementById('fixTypeChart').getContext('2d');
const fixTypeChart = createChart(fixTypeChartCtx, 'pie', {
  labels: [getFixTypeName(0), getFixTypeName(1), getFixTypeName(2)],
  datasets: [{
    data: [],
    backgroundColor: ['red', 'green', 'yellow'],
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
      text: 'GNSS Fix Types Duration (%)'
    },
    tooltip: {
      callbacks: {
        label: function (context) {
          const value = context.formattedValue;
          return `${value} %`;
        }
      }
    }
  }
});

const autoScaleXCheckbox = document.getElementById('autoScaleXCheckbox');
const autoScaleYCheckbox = document.getElementById('autoScaleYCheckbox');
const manualYRangeInput = document.getElementById('manualYRangeInput');
const showOnlyRtkFixCheckbox = document.getElementById('showOnlyRtkFixCheckbox');
const showThresholdInput = document.getElementById('showThresholdInput');
const displayModeDropdown = document.getElementById('displayModeDropdown');
const selectedRoverDropdown = document.getElementById('selectedRoverDropdown');
const availableRoversContainer = document.getElementById('availableRoversContainer');
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
// Possible values: 'startOfDay', 'last24Hours', 'last6Hours', 'last1Hours', 'last10Minutes'.
let displayMode = 'startOfDay';
let latestEntryId = 0;
let latestData;
let latestDataReceiveTime;
// The IDs of the rovers that are currently available
let availableRovers = [];
let selectedRover = "unknown";
// The current translations
let translations;

async function forceRefreshData() {
  latestEntryId = 0;
  await refreshData();
}

async function refreshData() {
  let dataStart = dayStartLocal;
  const now = new Date();

  switch (displayMode) {
    case 'startOfDay':
      dataStart = dayStartLocal;
      break;
    case 'last24Hours':
      dataStart = new Date(now.getTime() - 24 * 60 * 60 * 1000);
      break;
    case 'last6Hours':
      dataStart = new Date(now.getTime() - 6 * 60 * 60 * 1000);
      break;
    case 'last1Hours':
      dataStart = new Date(now.getTime() - 1 * 60 * 60 * 1000);
      break;
    case 'last10Minutes':
      dataStart = new Date(now.getTime() - 10 * 60 * 1000);
      break;
    default:
      console.error('Invalid display mode:', displayMode);
      return;
  }

  const data = await fetchData(dataStart);

  if (data.availableRovers.length === 0) {
    console.log('No rovers available');
    return;
  }

  if (latestEntryId >= data.lastEntryId) {
    console.log('No new data received');
    return;
  }

  // Update the available rovers
  availableRovers = data.availableRovers;

  // Check that the currently selected rover is still available.
  // If not, select the first available rover.
  if (selectedRover && !availableRovers.includes(selectedRover)) {
    selectedRover = availableRovers.length > 0 ? availableRovers[0] : "unknown";
  }

  latestData = data;
  latestDataReceiveTime = new Date();
  latestEntryId = data.lastEntryId;
  console.log('New data received:', latestData);
  refreshInterface();
  onAvailableRoversChanged();
}

function refreshInterface() {
  updateGraph(latestData.feeds[selectedRover], 'DeltaZ', deltaZChart, pointsPerGraph, autoScaleX, showOnlyRtkFix, showThreshold);
  updateGraph(latestData.feeds[selectedRover], 'DeltaXY', deltaXYChart, pointsPerGraph, autoScaleX, showOnlyRtkFix, showThreshold);
  updateTextData(latestData.feeds[selectedRover]);
  updateFixTypeChart(latestData.feeds[selectedRover], fixTypeChart);
}

function updateTextData(feeds) {
  const latestFeed = feeds[feeds.length - 1];

  const deltaZ = latestFeed.gnss.DeltaZ.toFixed(3);
  const deltaXY = latestFeed.gnss.DeltaXY.toFixed(3);
  const ionoRaw = latestFeed.gnss.IonoPercentage;
  const iono = ionoRaw === undefined ? 'N/A' : ionoRaw;
  let roverId = latestFeed.gnss.RoverId;
  if (roverId === undefined) {
    roverId = 'unknown';
  }

  // Deltas pruned to 3 decimal places
  document.getElementById('DeltaZ').textContent = `${deltaZ} m`;
  document.getElementById('DeltaZTitle').textContent = `${deltaZ * 1000} mm`;
  document.getElementById('DeltaXY').textContent = `${deltaXY} m`;
  document.getElementById('DeltaXYTitle').textContent = `${deltaXY * 1000} mm`;
  document.getElementById('Ionosphere').textContent = `${iono} %`;

  document.getElementById('RoverId').textContent = roverId;
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

function updateOldDataWarning() {
  if (!latestDataReceiveTime)
    return;
  const now = new Date();
  const secondsAgo = Math.floor((now - latestDataReceiveTime) / 1000);
  const warningPopup = document.getElementById('warning-popup');
  if (secondsAgo > oldDataWarningThreshold) {
    warningPopup.classList.remove('hidden');
  } else {
    warningPopup.classList.add('hidden');
  }
}

function onAvailableRoversChanged() {
  selectedRoverDropdown.innerHTML = availableRovers.map(roverId => `<option value="${roverId}">${roverId}</option>`).join('');
  selectedRoverDropdown.value = selectedRover;

  document.getElementById('availableRoversTableBody').innerHTML = availableRovers.map(roverId => getRoverListEntry(roverId)).join('');
}

function getRoverListEntry(roverId) {
  const feeds = latestData.feeds[roverId];
  if (!feeds || feeds.length === 0) {
    return `<tr><td colspan="5"><b>${roverId}</b>: No data</td></tr>`;
  }

  const latestFeed = feeds[feeds.length - 1];
  const deltaZ = latestFeed.gnss.DeltaZ.toFixed(3) * 1000;
  const deltaXY = latestFeed.gnss.DeltaXY.toFixed(3) * 1000;
  const ionoRaw = latestFeed.gnss.IonoPercentage;
  const iono = ionoRaw === undefined ? 'N/A' : ionoRaw;
  const baseDistance = latestFeed.gnss.BaseRoverDistance.toFixed(3);
  const time = latestFeed.datetime.toTimeString();

  return `
    <tr>
      <td><b>${roverId}</b></td>
      <td>${deltaZ}</td>
      <td>${deltaXY}</td>
      <td>${iono}</td>
      <td>${baseDistance}</td>
      <td>${time}</td>
    </tr>
  `;
}

function applyTranslations() {
  // Title
  document.querySelector('title').textContent = translations.title;
  document.querySelector('h1').textContent = translations.title;

  // Available rovers
  document.getElementById('availableRoversTableTitle').textContent = translations.availableRoversTableTitle;
  document.getElementById('availableRoversTableId').textContent = translations.availableRoversTableId;
  document.getElementById('availableRoversTableDeltaZ').textContent = `${translations.availableRoversTableDeltaZ} (mm)`;
  document.getElementById('availableRoversTableDeltaXY').textContent = `${translations.availableRoversTableDeltaXY} (mm)`;
  document.getElementById('availableRoversTableIono').textContent = `${translations.availableRoversTableIono} (%)`;
  document.getElementById('availableRoversTableBaseDistance').textContent = `${translations.availableRoversTableBaseDistance} (m)`;
  document.getElementById('availableRoversTableTime').textContent = translations.availableRoversTableTime;

  // Settings
  document.querySelector('label[for="showOnlyRtkFixCheckbox"]').textContent = translations.settingsShowOnlyRtkFixed;
  document.querySelector('label[for="autoScaleYCheckbox"]').textContent = translations.settingsAutoScaleY;
  document.querySelector('label[for="displayModeDropdown"]').textContent = translations.settingsDisplayMode;
  // Display mode options
  document.querySelector('option[value="startOfDay"]').textContent = translations.settingsDisplayModeStartOfDay;
  document.querySelector('option[value="last24Hours"]').textContent = translations.settingsDisplayModeLast24Hours;
  document.querySelector('option[value="last6Hours"]').textContent = translations.settingsDisplayModeLast6Hours;
  document.querySelector('option[value="last1Hours"]').textContent = translations.settingsDisplayModeLast1Hours;
  document.querySelector('option[value="last10Minutes"]').textContent = translations.settingsDisplayModeLast10Minutes;
  // Selected rover
  document.querySelector('label[for="selectedRoverDropdown"]').textContent = translations.settingsSelectedRover;
  // Download section
  document.querySelector('label[for="datePicker"]').textContent = translations.settingsSelectDate;
  document.getElementById('downloadButton').textContent = translations.settingsDownloadCsv;

  // Data
  document.getElementById('dataTitle').textContent = translations.dataTitle;
  document.getElementById('dataRoverId').textContent = translations.dataRoverId;
  document.getElementById('dataTime').textContent = translations.dataTime;
  document.getElementById('dataBaseDistance').textContent = translations.dataBaseDistance;
  document.getElementById('dataFixType').textContent = translations.dataFixType;
  document.getElementById('dataSatellitesInUse').textContent = translations.dataSatellitesInUse;
  document.getElementById('dataPDop').textContent = translations.dataPDop;
  document.getElementById('dataHDop').textContent = translations.dataHDop;
  document.getElementById('dataVDop').textContent = translations.dataVDop;
  document.getElementById('dataErrorLatitude').textContent = translations.dataErrorLatitude;
  document.getElementById('dataErrorLongitude').textContent = translations.dataErrorLongitude;
  document.getElementById('dataErrorAltitude').textContent = translations.dataErrorAltitude;
  document.getElementById('dataDeltaZ').textContent = translations.dataDeltaZ;
  document.getElementById('dataDeltaXY').textContent = translations.dataDeltaXY;
  document.getElementById('dataIonosphere').textContent = translations.dataIonosphere;
}

async function loadAvailableTranslations() {

  const response = await fetch(`locales/translations.json`);
  const availableTranslations = await response.json();

  for (let i = 0; i < availableTranslations.length; i++) {
    console.log('Discovered translation:', availableTranslations[i].language);
  }

  // Automatically populate the languageSwitcher dropdown with the available translations
  const languageSwitcher = document.getElementById('languageSwitcher');
  languageSwitcher.innerHTML = availableTranslations.map(translation => `<option value="${translation.id}">${translation.language}</option>`).join('');

  // Load the default translation
  await loadTranslation(availableTranslations[0].id);
}

async function loadTranslation(lang){
  const response = await fetch(`locales/${lang}.json`);
  translations = await response.json();
  applyTranslations();
}

autoScaleXCheckbox.checked = autoScaleX;
autoScaleYCheckbox.checked = autoScaleY;
manualYRangeInput.value = manualYRange;
showOnlyRtkFixCheckbox.checked = showOnlyRtkFix;
showThresholdInput.value = showThreshold;
displayModeDropdown.value = displayMode;
datePicker.value = siteRefreshDate.toISOString().slice(0, 10);

autoScaleXCheckbox.addEventListener('change', () => {
  autoScaleX = autoScaleXCheckbox.checked;
  refreshInterface();
});

autoScaleYCheckbox.addEventListener('change', () => {
  autoScaleY = autoScaleYCheckbox.checked;

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

displayModeDropdown.addEventListener('change', async () => {
  displayMode = displayModeDropdown.value;
  autoScaleX = displayMode !== 'startOfDay';

  await forceRefreshData();
});

selectedRoverDropdown.addEventListener('change', () => {
  selectedRover = selectedRoverDropdown.value;
  refreshInterface();
});

downloadButton.addEventListener('click', async () => {
  const selectedDate = new Date(datePicker.value);
  const dayStartLocal = new Date(selectedDate.getFullYear(), selectedDate.getMonth(), selectedDate.getDate(), 0, 0, 0, 0);

  if (isNaN(dayStartLocal.getTime())) {
    alert('Please select a valid date.');
    return;
  }

  notification.classList.remove('hidden');
  notification.textContent = 'Downloading data...';

  try {
    const data = await fetchData(dayStartLocal);
    const csvData = dataToCsv(data);
    downloadCSV(csvData, `gnss_data_${dayStartLocal.toISOString().slice(0, 19) + 'Z'}.csv`);
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

document.getElementById('languageSwitcher').addEventListener('change', (event) => {
  loadTranslation(event.target.value);
});

loadAvailableTranslations().then(r => refreshData());
setInterval(refreshData, refreshInterval * 1000);
setInterval(updateOldDataWarning, 5000);
