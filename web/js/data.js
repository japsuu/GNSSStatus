import { getFixTypeName } from './utils.js';

const apiKey = "WQNA71V5DYQRO3BV";
const dataFetchUrl = (startDate, endDate) => `https://api.thingspeak.com/channels/2691494/feeds.json?api_key=${apiKey}&start=${startDate}&end=${endDate}`;

async function fetchData(startDate) {
  // End date is 24 hours after the start date
  const day = 60 * 60 * 24 * 1000;
  let endDate = new Date(startDate.getTime() + day);
  endDate = endDate.toISOString().slice(0, 19) + 'Z';
  startDate = startDate.toISOString().slice(0, 19) + 'Z';

  const url = dataFetchUrl(startDate, endDate);

  const response = await fetch(url);
  const json = await response.json();

  let availableRovers = [];
  let feedsByRoverId = {};

  json.feeds.map(feed => {
    const f1 = feed.field1;
    const f2 = feed.field2;
    const f3 = feed.field3;
    const f4 = feed.field4;
    const gnssData = Object.assign({}, JSON.parse(f1), JSON.parse(f2), JSON.parse(f3), JSON.parse(f4));

    // Read the RoverId from the gnssData
    let roverId = gnssData.RoverId;

    if (roverId === undefined) {
      // console.warn('Encountered a data feed without a RoverId, setting to unknown');
      roverId = 'unknown';
    }

    // Add the roverId to availableRovers
    if (!availableRovers.includes(roverId)) {
      availableRovers.push(roverId);
    }
    // Add a new feed to feedsByRoverId
    if (!feedsByRoverId[roverId]) {
      feedsByRoverId[roverId] = [];
    }

    const timeUtc = gnssData.TimeUtc;
    const date = feed.created_at.split('T')[0];
    const time = `${timeUtc.slice(0, 2)}:${timeUtc.slice(2, 4)}:${timeUtc.slice(4, 6)}`;

    const datetime = new Date(`${date}T${time}Z`);

    feedsByRoverId[roverId].push({
      gnss: gnssData,
      datetime: datetime
    });
  });

  return {
    feeds: feedsByRoverId,
    lastEntryId: json.channel.last_entry_id,
    availableRovers: availableRovers
  };
}

/*
  Constructs an array of data points from the data object.
 */
function dataToCsv(data) {

  // Define the header row
  const header = [
    'RoverId', 'DateUTC', 'DateLocal', 'FixType', 'SatellitesInUse', 'RoverX', 'RoverY', 'RoverZ', 'DeltaZ', 'DeltaXY', 'IonoPercentage', 'PDop', 'HDop', 'VDop', 'ErrorLatitude', 'ErrorLongitude', 'ErrorAltitude', 'BaseRoverDistance'
  ];

  const feedsMap = data.feeds;
  const feeds = [];

  // Flatten the feeds object into an array
  Object.keys(feedsMap).forEach(roverId => {
    feedsMap[roverId].forEach(feed => {
      feeds.push(feed);
    });
  });

  // Map the data to CSV format
  const csvData = feeds.map(feed => {
    const roverId = feed.gnss.RoverId;
    const dateUtc = feed.datetime.toISOString();
    let localDate = new Date(feed.datetime);
    localDate.setHours(localDate.getHours() - (localDate.getTimezoneOffset() / 60));
    const dateLocal = localDate.toISOString();
    const fixType = getFixTypeName(feed.gnss.FixType);
    const satellitesInUse = feed.gnss.SatellitesInUse;
    const roverX = feed.gnss.RoverX;
    const roverY = feed.gnss.RoverY;
    const roverZ = feed.gnss.RoverZ;
    const deltaZ = feed.gnss.DeltaZ;
    const deltaXY = feed.gnss.DeltaXY;
    const pDop = feed.gnss.PDop;
    const hDop = feed.gnss.HDop;
    const vDop = feed.gnss.VDop;
    const errorLatitude = feed.gnss.ErrorLatitude;
    const errorLongitude = feed.gnss.ErrorLongitude;
    const errorAltitude = feed.gnss.ErrorAltitude;
    const baseRoverDistance = feed.gnss.BaseRoverDistance;
    const iono = feed.gnss.IonoPercentage;

    return [roverId, dateUtc, dateLocal, fixType, satellitesInUse, roverX, roverY, roverZ, deltaZ, deltaXY, iono, pDop, hDop, vDop, errorLatitude, errorLongitude, errorAltitude, baseRoverDistance];
  });

  // Prepend the header row to the CSV data
  csvData.unshift(header);

  return csvData;
}

export { fetchData, dataToCsv };
