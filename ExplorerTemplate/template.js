/** @jsx React.DOM */

var Trivia = React.createClass({displayName: 'Trivia',
	render: function() {
		return this.props.data;
	}
});

function tokenClassName(tokenKind) {
	switch(tokenKind) {
		case "Literal":
			return "";
		case "Keyword":
			return "keyword";
		case "Identifier":
			return "identifier";
		case "TypeDecl":
			return "namedTypeDecl";
		case "TypeRef":
			return "namedTypeRef";
		case "StringLiteral":
			return "stringLiteral";
		case "NumericLiteral":
			return "numericLiteral";
		case "CharLiteral":
			return "characterLiteral";
		case "LocalDecl":
			return "localDecl";
		case "LocalRef":
			return "localRef";
		case "FieldDecl":
			return "fieldDecl";
		case "FieldRef":
			return "fieldRef";
		case "ParamDecl":
			return "paramDecl";
		case "ParamRef":
			return "paramRef";
		case "PropDecl":
			return "propDecl";
		case "PropRef":
			return "propRef";
		case "MethodDecl":
			return "methodDecl";
		case "MethodRef":
			return "methodRef";
		case "EnumMemberDecl":
			return "enumMemberDecl";
		case "EnumMemberRef":
			return "enumMemberRef";
		case "SemanticError":
			return "semanticError";
		case "Trivia":
			return "";
		case "Comment":
			return "comment";
		case "Region":
			return "region";
		case "DisabledText":
			return "disabled";
	}
};

var SymbolInfo = React.createClass({displayName: 'SymbolInfo',
	render: function() {
		return (
			React.DOM.div({className: "symbolInfo"}, 
				React.DOM.div(null, this.props.text), 
				React.DOM.div(null, this.props.location)
			)
		);
	}
});

function renderToken(toolTips, token) {
	var className = tokenClassName(token.kind);
	if(token.tipId == -1) {
		return React.DOM.span({className: className}, token.text);
    }
	var tip = toolTips[token.tipId];
	return (
		React.DOM.span({className: "hasSymbolInfo"}, 
			React.DOM.span({className: className}, token.text), SymbolInfo({text: tip.text, location: tip.location})
		)
	);
};

var CodeSection = React.createClass({displayName: 'CodeSection',
    render: function() {
		var asdf = [];
		var tokens = this.props.data.codeTokens;
		var tooltips = this.props.data.toolTips;
		for(var i = 0; i < tokens.length; i++) {
			var token = tokens[i];
			asdf.push(renderToken(tooltips, token));
		}
		return (
			React.DOM.pre(null, 
				React.DOM.code({className: "formattedCode"}, 
					asdf
				)
			)
		);
    }
});
$.getJSON("Data.json", function(data) {
            
    React.renderComponent(
        CodeSection({data: data}),
        document.getElementById('code')
    );
});