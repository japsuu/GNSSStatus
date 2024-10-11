const apiKey = "WQNA71V5DYQRO3BV"; // Public read API key
const channelId = "2691494"; // ThingSpeak channel ID
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
    const json = await response.json();

    // Construct an array of GNSS data from the JSON response
    const data = { feeds: json.feeds.map(feed => {
        // The data is stored in multiple fields, so we need to combine them into one JSON object
        const f1 = feed.field1;
        const f2 = feed.field2;
        const f3 = feed.field3;
        const f4 = feed.field4;
        const gnssData = Object.assign({}, JSON.parse(f1), JSON.parse(f2), JSON.parse(f3), JSON.parse(f4));

        // Convert the hhmmss.ss(ss) TimeUtc to hh:mm:ss
        const timeUtc = gnssData.TimeUtc;
        const time = `${timeUtc.slice(0, 2)}:${timeUtc.slice(2, 4)}:${timeUtc.slice(4, 6)}`;

        return {
          gnss: gnssData,
          time: time,
        }
    })};
    console.log('Data:', data);


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

    if (feed.gnss.DeltaZ !== undefined) {
      deltaZData.push(feed.gnss.DeltaZ);
      labels.push(feed.time);
    }
  });

  deltaZChart.data.labels = labels;
  deltaZChart.data.datasets[0].data = deltaZData;
  deltaZChart.update();
}

// Update text data
function updateTextData(data) {
  const latestFeed = data.feeds[data.feeds.length - 1];

  document.getElementById('TimeUtc').textContent = latestFeed.time;
  document.getElementById('FixType').textContent = latestFeed.gnss.FixType;
  document.getElementById('SatellitesInUse').textContent = latestFeed.gnss.SatellitesInUse;
  document.getElementById('PDop').textContent = latestFeed.gnss.PDop;
  document.getElementById('ErrorLatitude').textContent = latestFeed.gnss.ErrorLatitude;
  document.getElementById('ErrorLongitude').textContent = latestFeed.gnss.ErrorLongitude;
  document.getElementById('ErrorAltitude').textContent = latestFeed.gnss.ErrorAltitude;
}

// Fetch data every 15 seconds without refreshing the page
setInterval(fetchData, 15000);
fetchData();
