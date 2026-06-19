const getProp = (obj, ...keys) => {
    if (!obj) return null;

    for (const key of keys) {
        if (Object.prototype.hasOwnProperty.call(obj, key)) {
            return obj[key];
        }
    }

    return null;
};

const getResponseData = (response) => {
    return getProp(response, "data", "Data");
};