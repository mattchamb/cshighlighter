/// <reference path="../scripts/Dispatcher.ts" />
/// <reference path="../scripts/Util.ts" />
/// <reference path="../scripts/ProjectStore.ts" />
/// <reference path="../scripts/react.d.ts" />

module Components {
    var dispatcher = App.dispatcher;

    export var ProjectFileList = React.createClass({
        displayName: 'ProjectFileList',
        render: function () {
            var props = this.props;
            var projId = this.props.projectId;

            var toSourceFileListing = Util.map(function (sourceId) {
                var path = App.project.getFile(sourceId).path;
                var fileClicked = function () {
                    dispatcher.trigger("selectedFileChanged", sourceId);
                }
			    return React.DOM.div({ className: "", onClick: fileClicked }, path);
            });
            var projectClicked = function () {
                dispatcher.trigger("selectedProjectChanged", projId);
            };
            return React.DOM.div({ onClick: projectClicked, className: "projectLevel" },
                    toSourceFileListing(this.props.files));
        }
    });
}