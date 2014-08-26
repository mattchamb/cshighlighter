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
		var text = token.text;
		if (className == "") {
			text = token.text.replace(/\t/g, "    ");
		}

		if (!token.symbolId) {
			return React.DOM.span({className: className}, text);
	    }
	    var symbolInfo = symbolData[token.symbolId];
		return (
			React.DOM.span({className: "hasSymbolInfo"}, 
				React.DOM.span({className: className}, text), 
				SymbolInfo({text: symbolInfo.displayText, locations: symbolInfo.locations})
			)
		);
	};

	var CodeSection = React.createClass({displayName: 'CodeSection',
	    render: function() {
			var tokenSpans = [];
			var groups = [];
			var tokens = this.props.file.codeTokens;
			for(var i = 0; i < tokens.length; i++) {
				var token = tokens[i];
				tokenSpans.push(renderToken(token));
				if(tokenSpans.length == 200) {
					groups.push(React.DOM.span(null, tokenSpans));
					tokenSpans = [];
				}
			}
			if (tokenSpans.length > 0) {
				groups.push(React.DOM.span(null, tokenSpans));
			}
			return (
				React.DOM.div({className: "codeSection"}, 
					React.DOM.pre(null, 
						React.DOM.code({className: "formattedCode"}, 
							groups
						)
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
					dispatcher.trigger("selectedFileChanged", x.sourceId);
				}
				return React.DOM.div({className: "", onClick: fileClicked}, path);
			});
			var projectClicked = function() {
				dispatcher.trigger("selectedProjectChanged", projId);
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

	var SourceStats = React.createClass({displayName: 'SourceStats',
		render: function() {
			var file = this.props.file;

			var totalMembers = 0;
			for(var i = 0; i < file.typeDecls.length; i++) {
				var decl = file.typeDecls[i];
				totalMembers += decl.Members.length;
			}

			var referencedSymbols = 0;
			for(var i = 0; i < file.codeTokens.length; i++) {
				var token = file.codeTokens[i];
				if(token.symbolId) {
					referencedSymbols++;
				}
			}

			return (
				React.DOM.div(null, 
					React.DOM.div(null, "Types defined: ", file.typeDecls.length), 
					React.DOM.div(null, "Members defined: ", totalMembers), 
					React.DOM.div(null, "Classified spans: ", file.codeTokens.length), 
					React.DOM.div(null, "Referenced symbols: ", referencedSymbols)
				)
			);
		}
	});

	var SolutionExplorer = React.createClass({displayName: 'SolutionExplorer',

		getInitialState: function() {
			return Store.getState();
		},

		componentWillMount: function() {
			dispatcher.addListener("selectedFileChanged", this.selectedFileChanged);
			dispatcher.addListener("selectedProjectChanged", this.selectedProjectChanged);
		},

		componentWillUnmount: function() {
			dispatcher.removeListener("selectedFileChanged", this.selectedFileChanged);
			dispatcher.removeListener("selectedProjectChanged", this.selectedProjectChanged);
		},

	    render: function() {
	    	var solutionPath = codeData.path;
	    	var projects = codeData.projects;

	    	var currProj = projects[this.state.currentProjectId];
	    	var currFile = getFile(currProj, this.state.currentSourceId);

			return (
				React.DOM.div(null, 
					React.DOM.div({className: "solutionExplorer"}, 
						React.DOM.div(null, solutionPath), 
						ProjectList({projects: projects})
					), 
					React.DOM.div({className: "codePanel"}, 
						React.DOM.h3(null, sourceFiles[this.state.currentSourceId].path), 
						SourceStats({file: currFile }), 
						CodeSection({file: currFile })
					)
				)
			);
	    }, 

	    selectedFileChanged: function(sourceId) {
			Store.setSourceId(sourceId);
			this.setState(Store.getState());
		},

		selectedProjectChanged: function(projectId) {
			Store.setProjectId(projectId);
			this.setState(Store.getState());
		}
	});
	
    React.renderComponent(
        SolutionExplorer(null),
        document.getElementById("explorerContainer")
    );
}

$.getJSON("Data2SourceFiles.json", function(sourceFiles) {
	$.getJSON("Data2.json", function(codeData) {
		$.getJSON("Data2Symbols.json", function(symbolData) {
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

