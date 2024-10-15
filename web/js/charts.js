
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
    ctx.strokeStyle = 'green'; // Color of the reference line
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

function updateGraph(data, dataKey, chart, pointsPerGraph, autoScaleX, showOnlyRtkFix) {
  const feeds = data.feeds;
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

      // Skip points that are not RTKFix if showOnlyRtkFix is true
      if (showOnlyRtkFix && parseInt(feed.gnss.FixType) !== 4) {
        return;
      }

      const pData = feed.gnss[dataKey];
      const pLabel = feed.datetime.toTimeString().slice(0, 8);
      const pColor = getPointColor(feed.gnss.FixType);

      dataPoints.push(pData);
      pointLabels.push(pLabel);
      pointColors.push(pColor);
      pointRadius.push(0);

      const lastIndex = dataPoints.length - 1;
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

  if (!autoScaleX) {
    for (let i = 0; i < pointsPerGraph - pointCount; i++) {
      dataPoints.push(null);
      pointLabels.push('');
      pointColors.push('black');
      pointRadius.push(0);
    }
  }

  if (maxIndex !== -1) {
    pointRadius[maxIndex] = 5;
    pointColors[maxIndex] = 'red';
  }
  if (minIndex !== -1) {
    pointRadius[minIndex] = 5;
    pointColors[minIndex] = 'red';
  }

  chart.data.labels = pointLabels;
  chart.data.datasets[0].data = dataPoints;
  chart.data.datasets[0].pointBackgroundColor = pointColors;
  chart.data.datasets[0].pointRadius = pointRadius;
  chart.update();
}

function updateFixTypeChart(data, fixTypeChart) {
  const feeds = data.feeds;
  const fixTypeDurations = { 0: 0, 1: 0, 2: 0 };

  for (let i = 1; i < feeds.length; i++) {
    const currentFeed = feeds[i];
    const previousFeed = feeds[i - 1];
    const duration = (currentFeed.datetime - previousFeed.datetime) / 1000;
    const fixType = parseInt(previousFeed.gnss.FixType);

    if (fixType === 4) {
      fixTypeDurations[2] += duration;
    }
    else if (fixType === 5) {
      fixTypeDurations[1] += duration;
    }
    else {
      fixTypeDurations[0] += duration;
    }
  }

  fixTypeChart.data.datasets[0].data = [
    fixTypeDurations[0],
    fixTypeDurations[1],
    fixTypeDurations[2]
  ];
  fixTypeChart.update();
}

// Export the necessary functions and variables
export { createChart, referenceLinePlugin, updateGraph, updateFixTypeChart };
