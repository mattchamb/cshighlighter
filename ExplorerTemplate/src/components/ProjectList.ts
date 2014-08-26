/// <reference path="./ProjectFileList.ts" />
/// <reference path="../scripts/Util.ts" />
/// <reference path="../scripts/react.d.ts" />

module Components {
    export var ProjectList = React.createClass({
        displayName: 'ProjectList',
        render: function () {
            var toFileList = Util.mapi(function (i, x) {
                return React.DOM.div({ key: x.path, className: "projectLevel" },
                    React.DOM.div(null, x.path),
                    ProjectFileList({ projectId: i, files: x.sourceIds }));
            });
            return React.DOM.div(null, toFileList(this.props.projects));
        }
    });
}
