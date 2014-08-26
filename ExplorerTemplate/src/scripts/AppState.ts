module App {
    function StateStore() {
        var state = {
            currentProjectId: 0,
            currentSourceId: 0,
            hasProjectData: false,
        };
        return {
            getState: function () {
                return state;
            },

            setProjectId: function (id) {
                state.currentProjectId = id;
            },

            setSourceId: function (id) {
                state.currentSourceId = id;
            },

            setHasProjectData: function (hasData) {
                state.hasProjectData = hasData;
            }
        };
    }
    export var stateStore = StateStore();
} 