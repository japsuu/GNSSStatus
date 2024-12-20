
import { getFixTypeName, getPointColor } from './utils.js';

// Reference line plugin
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
    ctx.strokeStyle = 'black'; // Color of the reference line
    ctx.lineWidth = 2; // Width of the reference line
    ctx.stroke();
    ctx.restore();
  }
};

// Register the plugin with ChartJS
Chart.register(referenceLinePlugin);

// Function to create a chart
function createChart(ctx, type, data, options) {
  return new Chart(ctx, {
    type: type,
    data: data,
    options: options
  });
}

function updateGraph(feeds, dataKey, chart, pointsPerGraph, autoScaleX, showOnlyRtkFix, showThreshold) {
  const dataPoints = [];
  const pointLabels = [];
  const pointColors = [];
  const pointRadii = [];
  const fixTypes = [];
  const dateTimes = [];
  const ionos = [];

  let maxIndex = -1;
  let minIndex = -1;
  let maxValue = -Infinity;
  let minValue = Infinity;

  let pointCount = 0;
  feeds.forEach(feed => {
    if (feed.gnss[dataKey] !== undefined) {

      // FixType as an integer
      let pFixType = parseInt(feed.gnss.FixType);
      // Wanted data point
      let pData = feed.gnss[dataKey];
      // Label for the point: time in HH:MM format
      let pLabel = feed.datetime.toTimeString().slice(0, 5);
      // Color for the point based on FixType
      let pColor = getPointColor(pFixType);
      // Datetime for the point
      let pDatetime = feed.datetime;
      // Ionosphere percentage
      let pIono = feed.gnss.IonoPercentage;

      pointCount++;

      // Skip points that are not RTKFix if showOnlyRtkFix is true
      const isUnwantedFix = showOnlyRtkFix && pFixType !== 1;
      const isOverThreshold = Math.abs(pData) > showThreshold;
      const isNullPoint = isUnwantedFix || isOverThreshold;

      if (isNullPoint){
        pData = null;
        pLabel = '';
        pColor = 'black';
        pDatetime = null;
        pIono = null;
      }

      dataPoints.push(pData);
      pointLabels.push(pLabel);
      pointColors.push(pColor);
      pointRadii.push(0);
      fixTypes.push(pFixType);
      dateTimes.push(pDatetime);
      ionos.push(pIono);

      // Null points are not considered for min/max
      if (isNullPoint)
        return;

      const lastIndex = dataPoints.length - 1;
      if (pData > maxValue) {
        maxValue = pData;
        maxIndex = lastIndex;
      }
      if (pData < minValue) {
        minValue = pData;
        minIndex = lastIndex;
      }
    }
  });

  if (!autoScaleX) {
    for (let i = 0; i < pointsPerGraph - pointCount; i++) {
      dataPoints.push(null);
      pointLabels.push('');
      pointColors.push('black');
      pointRadii.push(0);
      fixTypes.push(-1);
      dateTimes.push(null);
      ionos.push(null);
    }
  }

  if (maxIndex !== -1) {
    pointRadii[maxIndex] = 5;
    pointColors[maxIndex] = 'red';
  }
  if (minIndex !== -1) {
    pointRadii[minIndex] = 5;
    pointColors[minIndex] = 'red';
  }

  chart.data.labels = pointLabels;
  chart.data.datasets[0].data = dataPoints;
  chart.data.datasets[0].pointBackgroundColor = pointColors;
  chart.data.datasets[0].pointRadius = pointRadii;
  chart.data.datasets[0].fixType = fixTypes;
  chart.data.datasets[0].datetime = dateTimes;
  chart.data.datasets[0].ionos = ionos;
  chart.update();
}

function updateFixTypeChart(feeds, fixTypeChart) {
  const fixTypeDurations = { 0: 0, 1: 0, 2: 0 };

  for (let i = 1; i < feeds.length; i++) {
    const currentFeed = feeds[i];
    const previousFeed = feeds[i - 1];
    const duration = (currentFeed.datetime - previousFeed.datetime) / 1000;
    const fixType = parseInt(previousFeed.gnss.FixType);

    if (fixType === 1) {
      fixTypeDurations[1] += duration;
    }
    else if (fixType === 2) {
      fixTypeDurations[2] += duration;
    }
    else {
      fixTypeDurations[0] += duration;
    }
  }

  const fixTypePercentages = { 0: 0, 1: 0, 2: 0 };
  const totalDuration = fixTypeDurations[0] + fixTypeDurations[1] + fixTypeDurations[2];

  for (let i = 0; i < 3; i++) {
    fixTypePercentages[i] = (fixTypeDurations[i] / totalDuration) * 100;
  }

  fixTypeChart.data.datasets[0].data = [
    fixTypePercentages[0],
    fixTypePercentages[1],
    fixTypePercentages[2]
  ];
  fixTypeChart.update();
}

// Export the necessary functions and variables
export { createChart, updateGraph, updateFixTypeChart };
