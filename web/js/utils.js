
function getPointColor(fixType) {
  fixType = parseInt(fixType);

  if (fixType === 1) {
    return 'green';
  }
  if (fixType === 2) {
    return 'yellow';
  }

  return 'red';
}

function getFixTypeName(fixType) {
  fixType = parseInt(fixType);

  switch (fixType) {
    case 0:
      return 'No Fix';
    case 1:
      return 'RTK Fix';
    case 2:
      return 'RTK Float';
    default:
      return 'Unknown';
  }
}

function downloadCSV(dataArray, filename) {
  // Convert array to CSV string
  const csvContent = dataArray.map(e => e.join(",")).join("\n");

  // Create a blob from the CSV string
  const blob = new Blob([csvContent], { type: 'text/csv' });

  // Create a link element
  const link = document.createElement("a");

  // Create a URL for the blob
  const url = URL.createObjectURL(blob);

  // Set download attribute with a filename
  link.setAttribute("href", url);
  link.setAttribute("download", filename);

  // Append the link to the document body (invisible)
  document.body.appendChild(link);

  // Programmatically trigger the click
  link.click();

  // Remove the link from the document
  document.body.removeChild(link);
}

export { getPointColor, getFixTypeName, downloadCSV };
