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



function renderStuff(sourceFiles, codeData, symbolData) {

	var Store = StateStore();

	var Trivia = React.createClass({
		render: function() {
			return this.props.data;
		}
	});


	var SymbolInfo = React.createClass({
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
				<div className="symbolInfo">
					<div>{this.props.text}</div>
					<div>{locationText}</div>
				</div>
			);
		}
	});

	function renderToken(token) {
		var className = tokenClassName(token.kind);
		if (!token.symbolId) {
			return <span className={className}>{token.text}</span>;
	    }
	    var symbolInfo = symbolData[token.symbolId];
		return (
			<span className="hasSymbolInfo">
				<span className={className}>{token.text}</span>
				<SymbolInfo text={symbolInfo.displayText} locations={symbolInfo.locations} />
			</span>
		);
	};

	var CodeSection = React.createClass({
	    render: function() {
			var tokenSpans = [];
			var tokens = this.props.file.codeTokens;
			for(var i = 0; i < tokens.length; i++) {
				var token = tokens[i];
				tokenSpans.push(renderToken(token));
			}
			return (
				<div className="codeSection">
					<pre>
						<code className="formattedCode">
							{tokenSpans}
						</code>
					</pre>
				</div>
			);
	    }
	});

	var selectedFileChanged = null;
	var selectedProjectChanged = null;

	var ProjectFileList = React.createClass({
		render: function() {

			var projId = this.props.projectId;

			var toSourceFileListing = map(function(x) {
				var path = sourceFiles[x.sourceId].path;
				var fileClicked = function() {
					 selectedFileChanged(x.sourceId);
				}
				return <div className="" onClick={fileClicked}>{path}</div>;
			});
			var projectClicked = function() {
				selectedProjectChanged(projId);
			};
			return (
				<div onClick={projectClicked} className="projectLevel">
					{toSourceFileListing(this.props.files)}
				</div>
			);
		}
	});

	var ProjectList = React.createClass({
		render: function() {
			var toFileList = mapi(function(i, x) {
				return (
					<div key={x.path} className="projectLevel">
						<div>{x.path}</div>
						<ProjectFileList projectId={i} files={x.files} />
					</div>
				);
			});
			return <div>{ toFileList(this.props.projects) }</div>;
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

	var SourceStats = React.createClass({
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
				<div>
					<div>Types defined: {file.typeDecls.length}</div>
					<div>Members defined: {totalMembers}</div>
					<div>Classified spans: {file.codeTokens.length}</div>
					<div>Referenced symbols: {referencedSymbols}</div>
				</div>
			);
		}
	});

	var SolutionExplorer = React.createClass({

		getInitialState: function() {
			return Store.getState();
		},

		componentWillMount: function() {
			selectedFileChanged = this.selectedFileChanged;
			selectedProjectChanged = this.selectedProjectChanged;
		},

	    render: function() {
	    	var solutionPath = codeData.path;
	    	var projects = codeData.projects;

	    	var currProj = projects[this.state.currentProjectId];
	    	var currFile = getFile(currProj, this.state.currentSourceId);

			return (
				<div>
					<div className="solutionExplorer">
						<div>{solutionPath}</div>
						<ProjectList projects={projects} />
					</div>
					<div className="codePanel">
						<h3>{sourceFiles[this.state.currentSourceId].path}</h3>
						<SourceStats file={ currFile } />
						<CodeSection file={ currFile } />
					</div>
				</div>
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
        <SolutionExplorer />,
        document.getElementById("explorerContainer")
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

