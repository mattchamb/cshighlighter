/// <reference path="../scripts/react.d.ts" />
/// <reference path="../scripts/ProjectStore.ts" />

module Components {
    export var SymbolInfo = React.createClass({
        displayName: 'SymbolInfo',
        render: function () {
            var location = this.props.locations[0];
            var locationText = "";
            if (location) {
                if (location.assembly) {
                    locationText = location.assembly;
                } else {
                    locationText = App.project.getFile(location.sourceId).path;
                }
            }

            return React.DOM.div({ className: "symbolInfo" },
                React.DOM.div(null, this.props.text),
                React.DOM.div(null, locationText));
        }
    });
}