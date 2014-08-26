
/// <reference path="./SymbolInfo.ts" />
/// <reference path="../scripts/Util.ts" />
/// <reference path="../scripts/ProjectStore.ts" />
/// <reference path="../scripts/react.d.ts" />

module Components {

    function renderToken(token: App.ICodeToken) {
        var className = Util.tokenClassName(token.kind);
        var text = token.text;
        if (className == "") {
            text = token.text.replace(/\t/g, "    ");
        }
        
        if (!token.symbolId) {
            return React.DOM.span({ className: className }, text);
        }
        var symbolInfo = App.project.getSymbolInfo(token.symbolId);
        return (
            React.DOM.span({ className: "hasSymbolInfo" },
                React.DOM.span({ className: className }, text),
                SymbolInfo({ text: symbolInfo.displayText, locations: symbolInfo.locations })
                )
            );
    }

    export var CodeSection = React.createClass({
        displayName: 'CodeSection',
        render: function () {
            var tokenSpans = [];
            var groups = [];
            var tokens = this.props.file.codeTokens;
            for (var i = 0; i < tokens.length; i++) {
                var token = tokens[i];
                tokenSpans.push(renderToken(token));
                if (tokenSpans.length == 200) {
                    groups.push(React.DOM.span(null, tokenSpans));
                    tokenSpans = [];
                }
            }
            if (tokenSpans.length > 0) {
                groups.push(React.DOM.span(null, tokenSpans));
            }
            return React.DOM.div({ className: "codeSection" },
                React.DOM.pre(null,
                    React.DOM.code({ className: "formattedCode" },
                        groups
                        )
                    )
                );
        }
    });
}