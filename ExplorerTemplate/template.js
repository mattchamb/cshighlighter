/** @jsx React.DOM */

"use strict";

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



function renderStuff(sourceFiles, codeData, symbolData) {

	var Trivia = React.createClass({displayName: 'Trivia',
		render: function() {
			return this.props.data;
		}
	});


	var SymbolInfo = React.createClass({displayName: 'SymbolInfo',
		render: function() {
			var location = this.props.locations[0];
			var locationText = "";
			if (location) {
				if(location.assembly)
				{
					locationText = location.assembly;
				} else {
					locationText = sourceFiles[location.sourceId].path;
				}
			}

			return (
				React.DOM.div({className: "symbolInfo"}, 
					React.DOM.div(null, this.props.text), 
					React.DOM.div(null, locationText)
				)
			);
		}
	});

	function renderToken(token) {
		var className = tokenClassName(token.kind);
		if(!token.symbolId) {
			return React.DOM.span({className: className}, token.text);
	    }
	    var symbolInfo = symbolData[token.symbolId];
		return (
			React.DOM.span({className: "hasSymbolInfo"}, 
				React.DOM.span({className: className}, token.text), 
				SymbolInfo({text: symbolInfo.displayText, locations: symbolInfo.locations})
			)
		);
	};

	var CodeSection = React.createClass({displayName: 'CodeSection',
	    render: function() {
			var tokenSpans = [];
			var tokens = this.props.file.codeTokens;
			for(var i = 0; i < tokens.length; i++) {
				var token = tokens[i];
				tokenSpans.push(renderToken(token));
			}
			return (
				React.DOM.pre(null, 
					React.DOM.code({className: "formattedCode"}, 
						tokenSpans
					)
				)
			);
	    }
	});

	var selectedFileChanged = null;
	var selectedProjectChanged = null;

	var ProjectFileList = React.createClass({displayName: 'ProjectFileList',
		render: function() {

			var projId = this.props.projectId;

			var toSourceFileListing = map(function(x) {
				var path = sourceFiles[x.sourceId].path;
				var fileClicked = function() {
					 selectedFileChanged(x.sourceId);
				}
				return React.DOM.div({className: "", onClick: fileClicked}, path);
			});
			var projectClicked = function() {
				selectedProjectChanged(projId);
			};
			return (
				React.DOM.div({onClick: projectClicked, className: "projectLevel"}, 
					toSourceFileListing(this.props.files)
				)
			);
		}
	});

	var ProjectList = React.createClass({displayName: 'ProjectList',
		render: function() {
			var toFileList = mapi(function(i, x) {
				return (
					React.DOM.div({key: x.path, className: "projectLevel"}, 
						React.DOM.div(null, x.path), 
						ProjectFileList({projectId: i, files: x.files})
					)
				);
			});
			return React.DOM.div(null,  toFileList(this.props.projects) );
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

	var _state = {
		currentProjectId: 0,
		currentFileId: 0
	};

	var SolutionExplorer = React.createClass({displayName: 'SolutionExplorer',

		getInitialState: function() {
			
			return {
				currentProjectId: 0,
				currentFileId: 0
			};
		},

		componentWillMount: function() {
			var self = this;
			selectedFileChanged = function(sourceId) {
				var newState = {
					currentProjectId: _state.currentProjectId,
					currentFileId: sourceId
				};
				_state = newState;
				self.setState(newState);
			};
			selectedProjectChanged = function(projectId) {
				var newState = {
					currentProjectId: projectId,
					currentFileId: _state.currentFileId
				};
				_state = newState;
				self.setState(newState);
			};
		},

	    render: function() {
	    	var solutionPath = codeData.path;
	    	var projects = codeData.projects;

	    	var currProj = projects[this.state.currentProjectId];
	    	var currFile = getFile(currProj, this.state.currentFileId);

			return (
				React.DOM.div(null, 
					React.DOM.div({className: "solutionExplorer"}, 
						React.DOM.div(null, solutionPath), 
						ProjectList({projects: projects})
					), 
					React.DOM.div({className: "codePanel"}, 
						CodeSection({file: currFile })
					)
				)
			);
	    }
	});
	
    React.renderComponent(
        SolutionExplorer(null),
        document.getElementById("SolutionExplorer")
    );
}

$.getJSON("Data2SourceFiles.json", function(sourceFiles) {
	$.getJSON("Data2.json", function(codeData) {
		$.getJSON("Data2Symbols.json", function(symbolData) {
			renderStuff(sourceFiles, codeData, symbolData);
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

