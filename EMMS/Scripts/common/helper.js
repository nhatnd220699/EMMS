const RequestRunner = {
    run: async function (options, callback) {

        const { lockKey,
            loadMessage
        } = options;

        if (lockKey) {

            if (!ActionLock.acquire(lockKey))
                return;
        }

        if (loadMessage) {
            Popup.show(loadMessage);
        }

        try {

            return await callback();

        } catch (e) {

            Popup.error(e.message);
            throw e;

        } finally {

            if (loadMessage) {
                Popup.hide()
            }

            if (lockKey) {
                ActionLock.release(lockKey);
            }

        }
    }
}