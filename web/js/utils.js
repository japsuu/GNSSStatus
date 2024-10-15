
function getPointColor(fixType) {
  fixType = parseInt(fixType);

  if (fixType === 4) {
    return 'green';
  }
  if (fixType === 5) {
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
      return 'GPS Fix';
    case 2:
      return 'Differential GPS Fix';
    case 3:
      return 'Not Applicable';
    case 4:
      return 'RTK Fix';
    case 5:
      return 'RTK Float';
    case 6:
      return 'INS Dead Reckoning';
    default:
      return 'Unknown';
  }
}

export { getPointColor, getFixTypeName };
