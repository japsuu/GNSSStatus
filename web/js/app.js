const apiKey = "WQNA71V5DYQRO3BV"; // Replace with your ThingSpeak API Key
const channelId = "2691494"; // Replace with your Channel ID
const maxResults = 288; // 15 sec intervals over 24 hours: 24 * 60 * 60 / 15

const deltaZChartCtx = document.getElementById('deltaZChart').getContext('2d');

// Set up Chart.js for DeltaZ graph
const deltaZChart = new Chart(deltaZChartCtx, {
  type: 'line',
  data: {
    labels: [],
    datasets: [{
      label: 'DeltaZ',
      data: [],
      borderColor: 'rgba(75, 192, 192, 1)',
      borderWidth: 2,
      fill: false
    }]
  },
  options: {
    responsive: true,
    scales: {
      x: { display: true, title: { display: true, text: 'Time (UTC)' } },
      y: { display: true, title: { display: true, text: 'DeltaZ (m)' } }
    }
  }
});

// Fetch data from ThingSpeak and update UI
async function fetchData() {
  const url = `https://api.thingspeak.com/channels/${channelId}/feeds.json?api_key=${apiKey}&results=${maxResults}`;

  try {
    const response = await fetch(url);
    const data = await response.json();
    updateGraph(data);
    updateTextData(data);
  } catch (error) {
    console.error('Error fetching data:', error);
  }
}

// Update DeltaZ graph
function updateGraph(data) {
  const feeds = data.feeds;
  const deltaZData = [];
  const labels = [];

  feeds.forEach(feed => {

    // The field1 json data has all double quotes replaced with pipes,
    // so we need to replace them back before parsing the JSON data
    const jsonData = feed.field1.replace(/\|/g, '"');

    console.log(jsonData);

    const gnssData = JSON.parse(jsonData);
    if (gnssData.DeltaZ !== undefined) {
      deltaZData.push(gnssData.DeltaZ);
      labels.push(gnssData.TimeUtc);
    }
  });

  deltaZChart.data.labels = labels;
  deltaZChart.data.datasets[0].data = deltaZData;
  deltaZChart.update();
}

// Update text data
function updateTextData(data) {
  const latestFeed = data.feeds[data.feeds.length - 1];
  const gnssData = JSON.parse(latestFeed.field1);

  document.getElementById('TimeUtc').textContent = gnssData.TimeUtc;
  document.getElementById('FixType').textContent = gnssData.FixType;
  document.getElementById('SatellitesInUse').textContent = gnssData.SatellitesInUse;
  document.getElementById('PDop').textContent = gnssData.PDop;
  document.getElementById('ErrorLatitude').textContent = gnssData.ErrorLatitude;
  document.getElementById('ErrorLongitude').textContent = gnssData.ErrorLongitude;
  document.getElementById('ErrorAltitude').textContent = gnssData.ErrorAltitude;
}

// Fetch data every 15 seconds without refreshing the page
setInterval(fetchData, 15000);
fetchData();
