const FixtureService = {
    getList(filter) {

        return ApiClient.get(`/api/fixtures?filter=${queryString}`);
    }
}