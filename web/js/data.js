
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

export { fetchData };
