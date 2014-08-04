/** @jsx React.DOM */

var Trivia = React.createClass({
	render: function() {
		return this.props.data;
	}
});

function tokenClassName(tokenKind) {
	switch(tokenKind) {
		case "Unformatted":
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
		default:
		    return "";
	}
};

var SymbolInfo = React.createClass({
	render: function() {
		return (
			<div className="symbolInfo">
				<div>{this.props.text}</div>
				<div>{this.props.location}</div>
			</div>
		);
	}
});

function renderToken(toolTips, token) {
	var className = tokenClassName(token.kind);
	if(!token.tipId) {
		return <span className={className}>{token.text}</span>;
    }
	var tip = toolTips[token.tipId];
	return (
		<span className="hasSymbolInfo">
			<span className={className}>{token.text}</span><SymbolInfo text={tip.text} location={tip.location} />
		</span>
	);
};

var CodeSection = React.createClass({
    render: function() {
		var asdf = [];
		var tokens = this.props.data.codeTokens;
		var tooltips = this.props.data.toolTips;
		for(var i = 0; i < tokens.length; i++) {
			var token = tokens[i];
			asdf.push(renderToken(tooltips, token));
		}
		return (
			<pre>
				<code className="formattedCode">
					{asdf}
				</code>
			</pre>
		);
    }
});
$.getJSON("Data.json", function(data) {
            
    React.renderComponent(
        <CodeSection data={data} />,
        document.getElementById('code')
    );
});