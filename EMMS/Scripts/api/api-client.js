const STATUS_MESSAGES = {
    400: "The request is invalid.",
    401: "You are not authenticated or your session has expired.",
    403: "You do not have permission to perform this action.",
    404: "The requested resource was not found.",
    405: "The HTTP method is not allowed for this endpoint.",
    409: "A conflict occurred while processing your request.",
    413: "The request payload exceeds the maximum allowed size.",
    415: "The request media type is not supported.",
    422: "The submitted data is invalid.",
    429: "Too many requests. Please try again later.",
    500: "An internal server error occurred.",
    502: "A bad gateway error occurred.",
    503: "The service is temporarily unavailable or overloaded.",
    504: "The server did not respond within the allowed time."
};

const ApiClient = {

    // --------------------------------------------------
    // URL & QUERY STRING
    // --------------------------------------------------
    buildUrl(url) {
        const appRoot =
            document.querySelector('meta[name="app-root"]')?.content?.trim() || "/";

        // Normalize the application root path.
        const rootPath = `/${appRoot.replace(/^\/+|\/+$/g, "")}`;

        // Ensure the application root always ends with a slash.
        const basePath = rootPath === "/" ? "/" : `${rootPath}/`;

        const baseUrl = new URL(basePath, window.location.origin);

        // Resolve the requested URL relative to the application root.
        return new URL(String(url).replace(/^\/+/, ""), baseUrl).toString();
    },


    buildQueryString(params) {
        if (!params || typeof params !== "object") {
            return "";
        }

        const query = new URLSearchParams();

        Object.entries(params).forEach(([key, value]) => {
            if (value === null || value === undefined) {
                return;
            }

            const values = Array.isArray(value) ? value : [value];

            values.forEach(item => {
                if (item !== null && item !== undefined) {
                    query.append(key, String(item));
                }
            });
        });

        return query.toString();
    },

    buildRequestUrl(url, params = null) {
        const requestUrl = new URL(this.buildUrl(url));
        const queryString = this.buildQueryString(params);

        if (queryString) {
            new URLSearchParams(queryString).forEach((value, key) => {
                requestUrl.searchParams.append(key, value);
            });
        }

        return requestUrl.toString();
    },

    // --------------------------------------------------
    // HEADERS & SECURITY
    // --------------------------------------------------

    getAntiForgeryToken() {
        return (
            document.querySelector('meta[name="csrf-form-token"]')?.content ||
            document.querySelector(
                'input[name="__RequestVerificationToken"]'
            )?.value ||
            ""
        );
    },

    requiresAntiForgeryToken(method) {
        return ["POST", "PUT", "PATCH", "DELETE"]
            .includes(String(method).toUpperCase());
    },

    buildHeaders(isJson = true, customHeaders = {}, csrfHeader = false) {
        const headers = new Headers(customHeaders);

        if (isJson && !headers.has("Content-Type")) {
            headers.set("Content-Type", "application/json");
        }

        if (csrfHeader) {
            const token = this.getAntiForgeryToken();

            if (token && !headers.has("RequestVerificationToken")) {
                headers.set("RequestVerificationToken", token);
            }
        }

        return headers;
    },

    // --------------------------------------------------
    // CORE REQUEST
    // --------------------------------------------------

    //async send(method, url, options = {}) {
    //    const {
    //        params = null,
    //        data,
    //        headers = {},
    //        signal,
    //        isJson = true,
    //    } = options;

    //    const requestOptions = {
    //        method,
    //        credentials: "same-origin",
    //        headers: this.buildHeaders(isJson, headers),
    //        signal
    //    };

    //    if (data !== undefined) {
    //        requestOptions.body = isJson
    //            ? JSON.stringify(data)
    //            : data;
    //    }

    //    const response = await fetch(
    //        this.buildRequestUrl(url, params),
    //        requestOptions
    //    );

    //    return this.handleResponse(response);
    //},

    async send(method, url, options = {}) {
        const {
            params = null,
            data,
            headers = {},
            signal,
            isJson = true
        } = options;

        const normalizedMethod =
            String(method).toUpperCase();

        const requiresToken =
            this.requiresAntiForgeryToken(normalizedMethod);

        /*
         * JSON:
         * Gửi token qua RequestVerificationToken header.
         *
         * FormData/URLSearchParams:
         * Gửi token trong __RequestVerificationToken.
         */
        const requestHeaders = this.buildHeaders(
            isJson,
            headers,
            requiresToken && isJson
        );

        let requestBody = data;

        if (
            requiresToken &&
            !isJson
        ) {
            const token = this.getAntiForgeryToken();

            if (!token) {
                throw new Error(
                    "Anti-forgery token was not found."
                );
            }

            if (requestBody instanceof URLSearchParams) {
                requestBody.set(
                    "__RequestVerificationToken",
                    token
                );
            }
            else if (requestBody instanceof FormData) {
                requestBody.set(
                    "__RequestVerificationToken",
                    token
                );
            }
            else if (
                typeof requestBody === "string"
            ) {
                const formParams =
                    new URLSearchParams(requestBody);

                formParams.set(
                    "__RequestVerificationToken",
                    token
                );

                requestBody = formParams;
            }
            else if (
                requestBody === undefined ||
                requestBody === null
            ) {
                const formParams =
                    new URLSearchParams();

                formParams.set(
                    "__RequestVerificationToken",
                    token
                );

                requestBody = formParams;
            }
            else {
                throw new Error(
                    "Form request data must be FormData, " +
                    "URLSearchParams, or a form-urlencoded string."
                );
            }
        }

        const requestOptions = {
            method: normalizedMethod,
            credentials: "same-origin",
            headers: requestHeaders,
            signal
        };

        if (
            requestBody !== undefined &&
            !["GET", "HEAD"].includes(normalizedMethod)
        ) {
            requestOptions.body = isJson
                ? JSON.stringify(requestBody)
                : requestBody;
        }

        const response = await fetch(
            this.buildRequestUrl(url, params),
            requestOptions
        );

        return this.handleResponse(response);
    },

    // --------------------------------------------------
    // HTTP METHODS
    // --------------------------------------------------

    get(url, params = null, options = {}) {
        return this.send("GET", url, {
            ...options,
            params,
            isJson: false
        });
    },

    post(url, data, options = {}) {
        return this.send("POST", url, {
            ...options,
            data,
            isJson: true
        });
    },

    put(url, data, options = {}) {
        return this.send("PUT", url, {
            ...options,
            data,
            isJson: true
        });
    },

    patch(url, data, options = {}) {
        return this.send("PATCH", url, {
            ...options,
            data,
            isJson: true
        });
    },

    delete(url, params = null, options = {}) {
        return this.send("DELETE", url, {
            ...options,
            params,
            isJson: false
        });
    },

    postForm(url, formData, options = {}) {
        return this.send("POST", url, {
            ...options,
            data: formData,
            isJson: false
        });
    },

    // --------------------------------------------------
    // RESPONSE HANDLER
    // --------------------------------------------------

    async handleResponse(response) {
        // Handle responses without a body.
        if (response.status === 204 || response.status === 205) {
            return null;
        }

        const body = await response.text();

        let data = null;

        if (body) {
            try {
                data = JSON.parse(body);
            } catch {
                data = body;
            }
        }

        if (!response.ok) {
            const serverMessage =
                typeof data === "object" && data !== null
                    ? data.message || data.Message || data.title
                    : typeof data === "string" &&
                        !data.trim().startsWith("<")
                        ? data.trim()
                        : null;

            const defaultMessage =
                STATUS_MESSAGES[response.status] ||
                (response.status >= 500
                    ? `Lỗi hệ thống (${response.status}).`
                    : `Yêu cầu không hợp lệ (${response.status}).`);

            const error = new Error(
                serverMessage || defaultMessage
            );

            error.status = response.status;
            error.data = data;
            error.response = response;

            throw error;
        }

        return data;
    },

    // --------------------------------------------------
    // FILE DOWNLOAD
    // --------------------------------------------------

    async download(url, params = null, options = {}) {
        const response = await fetch(
            this.buildRequestUrl(url, params),
            {
                method: "GET",
                credentials: "same-origin",
                headers: this.buildHeaders(false, options.headers || {}),
                signal: options.signal
            }
        );

        if (!response.ok) {
            return this.handleResponse(response);
        }

        const blob = await response.blob();

        const disposition =
            response.headers.get("Content-Disposition");

        const fileName = this.getFileName(disposition);

        const downloadUrl = URL.createObjectURL(blob);
        const link = document.createElement("a");

        link.href = downloadUrl;
        link.download = fileName;

        document.body.appendChild(link);
        link.click();
        link.remove();

        // Delay revocation to ensure the browser starts the download.
        setTimeout(() => URL.revokeObjectURL(downloadUrl), 1000);

        return {
            fileName,
            size: blob.size,
            contentType: blob.type
        };
    },

    getFileName(disposition) {
        if (!disposition) {
            return "download";
        }

        // Support UTF-8 encoded filenames.
        const encodedMatch = disposition.match(
            /filename\*\s*=\s*UTF-8''([^;]+)/i
        );

        if (encodedMatch) {
            try {
                return decodeURIComponent(
                    encodedMatch[1].trim().replace(/^["']|["']$/g, "")
                );
            } catch {
                // Fall back to the regular filename.
            }
        }

        // Support regular filenames.
        const regularMatch = disposition.match(
            /filename\s*=\s*(?:"([^"]+)"|([^;]+))/i
        );

        return regularMatch
            ? (regularMatch[1] || regularMatch[2]).trim()
            : "download";
    },

    // --------------------------------------------------
    // REQUEST WRAPPER
    // --------------------------------------------------

    async request(callback) {
        Popup.show("Loading...");

        try {
            return await callback();
        } catch (error) {
            Popup.error(error.message);
            throw error;
        } finally {
            Popup.hide();
        }
    }
};













//const ApiClient = {
//    get: async function (url, params = null) {

//        const queryString = this.buildQueryString(params);

//        const response = await fetch(queryString ? `${url}?${queryString}` : url);

//        return this.handleResponse(response);
//    },

//    post: async function (url, data) {

//        const response = await fetch(url, {
//            method: "POST",
//            headers: {
//                "Content-Type": "application/json"
//            },
//            body: JSON.stringify(data)
//        });

//        return this.handleResponse(response);
//    },

//    postForm: async function (url, formData) {
//        const response = await fetch(url, {
//            method: "POST",
//            body: formData
//        });

//        return this.handleResponse(response);
//    },

//    handleResponse: async function (response) {

//        if (!response.ok) {

//            let message =`HTTP ${response.status}`;

//            try {

//                const errorData = await response.json();

//                message = errorData.Message || errorData.message || message;

//            } catch (e) {

//            }

//            throw new Error(message);
//        }

//        return await response.json();
//    },

//    buildQueryString: function (params) {

//        if (!params) return "";

//        const query = new URLSearchParams();

//        const filtered = Object.entries(params)
//            .forEach(([key, value]) => {

//                if (value === undefined || value === null) {
//                    return;
//                }

//                if (Array.isArray(value) || value instanceof Array) {
//                    value.forEach(item =>
//                        query.append(key, item)
//                    );
//                }
//                else {
//                    query.append(key, value);
//                }
//            });

//        return query.toString();
//    },

//    request: async function(callback) {

//        Popup.show("Loading...");

//        try {

//            return await callback();

//        } catch (e) {

//            Popup.error(e.message);

//            throw e;

//        } finally {

//            Popup.hide();
//        }

//        // Cach su dung:
//        // const result = await ApiClient.request(
//        //     () => ApiClient.get("/api/fixture/list")
//        // );
//    }
//}

//// if (response.status === 401) {
////     window.location.href = '@Url.Action("Index", "Auth")';
////     return null;
//// }


