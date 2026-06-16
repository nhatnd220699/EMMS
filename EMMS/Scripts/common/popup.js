(function (window) {

    if (window.Popup)
        return;

    // Inject CSS
    const style = document.createElement("style");

    style.textContent = `
        .popup {
            position: fixed;
            top: 20px;
            left: 50%;
            transform: translateX(-50%) scale(0.95);
            background: #333;
            color: white;
            padding: 12px 20px;
            border-radius: 10px;
            display: flex;
            align-items: center;
            gap: 10px;
            z-index: 99999;
            opacity: 0;
            transition: all .3s ease;
            pointer-events: none;
        }

        .popup.show {
            opacity: 1;
            transform: translateX(-50%) scale(1);
        }

        .popup.success {
            background: #2e7d32;
        }

        .popup.error {
            background: #c62828;
        }

        .popup-spinner {
            width: 16px;
            height: 16px;
            border: 3px solid #ccc;
            border-top: 3px solid #fff;
            border-radius: 50%;
            animation: popup-spin 1s linear infinite;
        }

        @keyframes popup-spin {
            100% {
                transform: rotate(360deg);
            }
        }
    `;

    document.head.appendChild(style);

    // Create DOM when page ready
    let popup;
    let text;
    let spinner;
    let timeout;

    function initialize() {

        if (document.getElementById("global-popup"))
            return;

        popup = document.createElement("div");

        popup.id = "global-popup";
        popup.className = "popup";

        popup.innerHTML = `
            <div class="popup-spinner"></div>
            <span></span>
        `;

        document.body.appendChild(popup);

        spinner = popup.querySelector(".popup-spinner");
        text = popup.querySelector("span");
    }

    function ensureReady() {

        if (!popup)
            initialize();
    }

    const Popup = {

        show(message) {

            ensureReady();

            clearTimeout(timeout);

            popup.className = "popup show";

            spinner.style.display = "block";

            text.textContent = message;
        },

        success(message, delay = 1500) {

            ensureReady();

            clearTimeout(timeout);

            popup.className = "popup show success";

            spinner.style.display = "none";

            text.textContent = message;

            timeout = setTimeout(
                Popup.hide,
                delay
            );
        },

        error(message, delay = 2000) {

            ensureReady();

            clearTimeout(timeout);

            popup.className = "popup show error";

            spinner.style.display = "none";

            text.textContent = message;

            timeout = setTimeout(
                Popup.hide,
                delay
            );
        },

        warning(message, delay = 2000) {

            ensureReady();

            clearTimeout(timeout);

            popup.className = "popup show";

            spinner.style.display = "none";

            text.textContent = message;

            timeout = setTimeout(
                Popup.hide,
                delay
            );
        },

        hide() {

            if (!popup)
                return;

            popup.classList.remove("show");
        }
    };

    window.Popup = Popup;

    if (document.readyState === "loading") {

        document.addEventListener(
            "DOMContentLoaded",
            initialize
        );

    } else {

        initialize();
    }

})(window);