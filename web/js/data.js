
const apiKey = "WQNA71V5DYQRO3BV";
const dataFetchUrl = (startDate) => `https://api.thingspeak.com/channels/2691494/feeds.json?api_key=${apiKey}&start=${startDate}`;

async function fetchData(startDate) {
  const response = await fetch(dataFetchUrl(startDate));
  const json = await response.json();

  return {
    feeds: json.feeds.map(feed => {
      const f1 = feed.field1;
      const f2 = feed.field2;
      const f3 = feed.field3;
      const f4 = feed.field4;
      const gnssData = Object.assign({}, JSON.parse(f1), JSON.parse(f2), JSON.parse(f3), JSON.parse(f4));

      const timeUtc = gnssData.TimeUtc;
      const date = feed.created_at.split('T')[0];
      const time = `${timeUtc.slice(0, 2)}:${timeUtc.slice(2, 4)}:${timeUtc.slice(4, 6)}`;

      const datetime = new Date(`${date}T${time}Z`);

      return {
        gnss: gnssData,
        datetime: datetime
      };
    }),
    lastEntryId: json.channel.last_entry_id
  };
}

/*
  Constructs an array of data points from the data object.
 */
function dataToCsv(data) {
  const feeds = data.feeds;

  // Define the header row
  const header = [
    'DateUTC', 'DateLocal', 'FixType', 'SatellitesInUse', 'RoverX', 'RoverY', 'RoverZ', 'DeltaZ', 'DeltaXY', 'PDop', 'HDop', 'VDop', 'ErrorLatitude', 'ErrorLongitude', 'ErrorAltitude', 'BaseRoverDistance'
  ];

  // Map the data to CSV format
  const csvData = feeds.map(feed => {
    const dateUtc = feed.datetime.toISOString();
    let localDate = new Date(feed.datetime);
    localDate.setHours(localDate.getHours() - (localDate.getTimezoneOffset() / 60));
    const dateLocal = localDate.toISOString();
    const fixType = feed.gnss.FixType;
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

    return [dateUtc, dateLocal, fixType, satellitesInUse, roverX, roverY, roverZ, deltaZ, deltaXY, pDop, hDop, vDop, errorLatitude, errorLongitude, errorAltitude, baseRoverDistance];
  });

  // Prepend the header row to the CSV data
  csvData.unshift(header);

  return csvData;
}

export { fetchData, dataToCsv };
