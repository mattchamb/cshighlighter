/// <reference path="../scripts/react.d.ts" />

module Components {
    export var SourceStats = React.createClass({
        displayName: 'SourceStats',
        render: function () {
            var file = this.props.file;

            //var totalMembers = 0;
            //for (var i = 0; i < file.typeDecls.length; i++) {
            //    var decl = file.typeDecls[i]; 
            //    totalMembers += decl.Members.length;
            //}

            var referencedSymbols = 0;
            for (var i = 0; i < file.codeTokens.length; i++) {
                var token = file.codeTokens[i];
                if (token.symbolId) {
                    referencedSymbols++;
                }
            }

            return (
                React.DOM.div(null,
                    //React.DOM.div(null, "Types defined: ", file.typeDecls.length),
                    //React.DOM.div(null, "Members defined: ", totalMembers),
                    React.DOM.div(null, "Classified spans: ", file.codeTokens.length),
                    React.DOM.div(null, "Referenced symbols: ", referencedSymbols)
                    )
                ); 
        }
    });
}
