/// <reference path="../scripts/ProjectStore.ts" />
/// <reference path="../scripts/Dispatcher.ts" />
/// <reference path="../scripts/AppState.ts" />
/// <reference path="./CodeSection.ts" />
/// <reference path="./ProjectList.ts" />
/// <reference path="./SourceStats.ts" />
/// <reference path="../scripts/react.d.ts" />

module Components {
    var dispatcher = App.dispatcher;
    var Store = App.stateStore;
    var project = App.project;

    export var SolutionExplorer = React.createClass({
        displayName: 'SolutionExplorer',

        getInitialState: function () {
            return Store.getState();
        },

        componentWillMount: function () {
            dispatcher.register("selectedFileChanged", this.selectedFileChanged);
            dispatcher.register("selectedProjectChanged", this.selectedProjectChanged);
            dispatcher.register(App.StoreEvents.RecievedData, this.recievedData);
        },

        componentWillUnmount: function () {
            dispatcher.remove("selectedFileChanged", this.selectedFileChanged);
            dispatcher.remove("selectedProjectChanged", this.selectedProjectChanged);
            dispatcher.remove(App.StoreEvents.RecievedData, this.recievedData);

        },

        render: function () {
            if (!this.state.hasProjectData) {
                return React.DOM.div(null, "Downloading project data.");
            }

            var solutionPath = project.solutionData.path;
            var projects = project.solutionData.projects;

            var currProj = projects[this.state.currentProjectId];
            var currFile = project.getFile(this.state.currentSourceId);

            return (
                React.DOM.div(null,
                    React.DOM.div({ className: "solutionExplorer" },
                        React.DOM.div(null, solutionPath),
                        ProjectList({ projects: projects })
                        ),
                    React.DOM.div({ className: "codePanel" },
                        React.DOM.h3(null, project.getFile(this.state.currentSourceId).path),
                        SourceStats({ file: currFile }),
                        CodeSection({ file: currFile })
                        )
                    )
                );
        },

        selectedFileChanged: function (sourceId) {
            Store.setSourceId(sourceId);
            this.setState(Store.getState());
        },

        selectedProjectChanged: function (projectId) {
            Store.setProjectId(projectId);
            this.setState(Store.getState());
        },

        recievedData: function () {
            Store.setHasProjectData(true);
            this.setState(Store.getState());
        }
    });
}