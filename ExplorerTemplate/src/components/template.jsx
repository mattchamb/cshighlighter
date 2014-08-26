/** @jsx React.DOM */

"use strict";

function StateStore() {
	var state = {
		currentProjectId: 0,
		currentSourceId: 0
	};
	return {
		getState: function() {
			return state;
		},

		setProjectId: function(id) {
			state.currentProjectId = id;
		},

		setSourceId: function(id) {
			state.currentSourceId = id;
		}
	};
};

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



function mapi(f) {
	return function(seq) {
		var result = [];
		for(var i = 0; i < seq.length; i++) {
			result.push(f(i, seq[i]));
		}
		return result;
	};
}


function map(f) {
	return function(seq) {
		var result = [];
		for(var i = 0; i < seq.length; i++) {
			result.push(f(seq[i]));
		}
		return result;
	};
}



function renderStuff(dispatcher, sourceFiles, codeData, symbolData) {

	var Store = StateStore();

	var Trivia = React.createClass({
		render: function() {
			return this.props.data;
		}
	});


	


	function getFile(project, fileId) {
		for (var i = 0; i < project.files.length; i++) {
			var file = project.files[i]
			if (file.sourceId == fileId)
				return file;
		}
		throw "Shit fucked up.";
	}



	
	
    React.renderComponent(
        <SolutionExplorer />,
        document.getElementById("explorerContainer")
    );
}

$.getJSON("Data2SourceFiles.json", function (sourceFiles) {
        $.getJSON("Data2.json", function (codeData) {
            $.getJSON("Data2Symbols.json", function (symbolData) {
                var dispatcher = new Dispatcher();
                renderStuff(dispatcher, sourceFiles, codeData, symbolData);
            });
        });
    });
/*
var SymbolBrowser = React.createClass({
    render: function() {
		return (
			<div>
			</div>
		);
    }
});

$.getJSON("Data2Symbols.json", function(data) {
    React.renderComponent(
        <SymbolBrowser data={data} />,
        document.getElementById('code')
    );
});*/

