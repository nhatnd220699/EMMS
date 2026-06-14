const ApiClient = {
    get: async function (url, params = null) {

        const queryString = this.buildQueryString(params);

        const response = await fetch(queryString ? `${url}?${queryString}` : url);

        return this.handleResponse(response);
    },

    post: async function (url, data) {

        const response = await fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(data)
        });

        return this.handleResponse(response);
    },

    postForm: async function (url, formData) {
        const response = await fetch(url, {
            method: "POST",
            body: formData
        });

        return this.handleResponse(response);
    },

    handleResponse: async function (response) {

        if (!response.ok) {

            let message =`HTTP ${response.status}`;

            try {

                const errorData = await response.json();

                console.log(errorData)

                message = errorData.message || errorData.error || message;

            } catch (e) {

            }

            throw new Error(message);
        }

        return await response.json();
    },

    buildQueryString: function (params) {

        if (!params) return "";

        const query = new URLSearchParams();

        const filtered = Object.entries(params)
            .forEach(([key, value]) => {

                if (value === undefined && value === null) return;

                if (Array.isArray(value) || value instanceof Array) {
                    value.forEach(item =>
                        query.append(key, item)
                    );
                }
                else {
                    query.append(key, value);
                }
            });

        return query.toString();
    },

    request: async function(callback) {

        Popup.show("Loading...");

        try {

            return await callback();

        } catch (e) {

            Popup.error(e.message);

            throw e;

        } finally {

            Popup.hide();
        }

        // Cach su dung:
        // const result = await ApiClient.request(
        //     () => ApiClient.get("/api/fixture/list")
        // );
    }
}

// if (response.status === 401) {
//     window.location.href = '@Url.Action("Index", "Login")';
//     return null;
// }


