var App;
(function (App) {
    var EventDispatcher = (function () {
        function EventDispatcher() {
            this.events = {};
        }
        EventDispatcher.prototype.register = function (event, callback) {
            if (!this.events[event]) {
                this.events[event] = [];
            }
            this.events[event].push(callback);
        };

        EventDispatcher.prototype.remove = function (event, callback) {
            var listeners = this.events[event];
            if (!listeners) {
                throw "No event listeners added for " + event;
            }
            this.events[event] = listeners.splice(listeners.indexOf(callback), 1);

            if (this.events[event].length = 0) {
                delete this.events[event];
            }
        };

        EventDispatcher.prototype.trigger = function (event, arg) {
            var listeners = this.events[event];
            if (!listeners) {
                return;
            }

            for (var i = 0; i < listeners.length; i++) {
                var l = listeners[i];
                l(arg);
            }
        };
        return EventDispatcher;
    })();
    App.EventDispatcher = EventDispatcher;

    App.dispatcher = new EventDispatcher();
})(App || (App = {}));
/// <reference path="./Dispatcher.ts" />

var App;
(function (App) {
    var dispatcher = App.dispatcher;

    App.StoreEvents = {
        RecievedData: "ProjectDataReceived"
    };

    var ProjectDataStore = (function () {
        function ProjectDataStore() {
            this.hasData = false;
            this.getData();
        }
        ProjectDataStore.prototype.getFile = function (fileId) {
            for (var i = 0; i < this.sourceData.length; i++) {
                var file = this.sourceData[i];
                if (file.sourceId == fileId)
                    return file;
            }
            throw "Shit fucked up.";
        };

        ProjectDataStore.prototype.getSymbolInfo = function (symbolId) {
            return this.symbolData[symbolId];
        };

        ProjectDataStore.prototype.setData = function (sourceData, solutionData, symbolData) {
            this.solutionData = solutionData;
            this.sourceData = sourceData;
            this.symbolData = symbolData;
            this.hasData = true;
            dispatcher.trigger(App.StoreEvents.RecievedData, null);
        };

        ProjectDataStore.prototype.getData = function () {
            var self = this;
            $.getJSON("Source.json", function (sourceData) {
                $.getJSON("Symbols.json", function (symbolData) {
                    $.getJSON("Solution.json", function (solutionData) {
                        self.setData(sourceData, solutionData, symbolData);
                    });
                });
            });
        };
        return ProjectDataStore;
    })();
    App.ProjectDataStore = ProjectDataStore;

    App.project = new ProjectDataStore();
})(App || (App = {}));
/// <reference path="../scripts/react.d.ts" />
/// <reference path="../scripts/ProjectStore.ts" />
var Components;
(function (Components) {
    Components.SymbolInfo = React.createClass({
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

            return React.DOM.div({ className: "symbolInfo" }, React.DOM.div(null, this.props.text), React.DOM.div(null, locationText));
        }
    });
})(Components || (Components = {}));
var Util;
(function (Util) {
    function tokenClassName(tokenKind) {
        switch (tokenKind) {
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
    }
    Util.tokenClassName = tokenClassName;
    ;

    function mapi(f) {
        return function (seq) {
            var result = [];
            for (var i = 0; i < seq.length; i++) {
                result.push(f(i, seq[i]));
            }
            return result;
        };
    }
    Util.mapi = mapi;

    function map(f) {
        return function (seq) {
            var result = [];
            for (var i = 0; i < seq.length; i++) {
                result.push(f(seq[i]));
            }
            return result;
        };
    }
    Util.map = map;
})(Util || (Util = {}));
/// <reference path="./SymbolInfo.ts" />
/// <reference path="../scripts/Util.ts" />
/// <reference path="../scripts/ProjectStore.ts" />
/// <reference path="../scripts/react.d.ts" />
var Components;
(function (Components) {
    function renderToken(token) {
        var className = Util.tokenClassName(token.kind);
        var text = token.text;
        if (className == "") {
            text = token.text.replace(/\t/g, "    ");
        }

        if (!token.symbolId) {
            return React.DOM.span({ className: className }, text);
        }
        var symbolInfo = App.project.getSymbolInfo(token.symbolId);
        return (React.DOM.span({ className: "hasSymbolInfo" }, React.DOM.span({ className: className }, text), Components.SymbolInfo({ text: symbolInfo.displayText, locations: symbolInfo.locations })));
    }

    Components.CodeSection = React.createClass({
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
            return React.DOM.div({ className: "codeSection" }, React.DOM.pre(null, React.DOM.code({ className: "formattedCode" }, groups)));
        }
    });
})(Components || (Components = {}));
/// <reference path="../scripts/Dispatcher.ts" />
/// <reference path="../scripts/Util.ts" />
/// <reference path="../scripts/ProjectStore.ts" />
/// <reference path="../scripts/react.d.ts" />
var Components;
(function (Components) {
    var dispatcher = App.dispatcher;

    Components.ProjectFileList = React.createClass({
        displayName: 'ProjectFileList',
        render: function () {
            var props = this.props;
            var projId = this.props.projectId;

            var toSourceFileListing = Util.map(function (sourceId) {
                var path = App.project.getFile(sourceId).path;
                var fileClicked = function () {
                    dispatcher.trigger("selectedFileChanged", sourceId);
                };
                return React.DOM.div({ className: "", onClick: fileClicked }, path);
            });
            var projectClicked = function () {
                dispatcher.trigger("selectedProjectChanged", projId);
            };
            return React.DOM.div({ onClick: projectClicked, className: "projectLevel" }, toSourceFileListing(this.props.files));
        }
    });
})(Components || (Components = {}));
/// <reference path="./ProjectFileList.ts" />
/// <reference path="../scripts/Util.ts" />
/// <reference path="../scripts/react.d.ts" />
var Components;
(function (Components) {
    Components.ProjectList = React.createClass({
        displayName: 'ProjectList',
        render: function () {
            var toFileList = Util.mapi(function (i, x) {
                return React.DOM.div({ key: x.path, className: "projectLevel" }, React.DOM.div(null, x.path), Components.ProjectFileList({ projectId: i, files: x.sourceIds }));
            });
            return React.DOM.div(null, toFileList(this.props.projects));
        }
    });
})(Components || (Components = {}));
var App;
(function (App) {
    function StateStore() {
        var state = {
            currentProjectId: 0,
            currentSourceId: 0,
            hasProjectData: false
        };
        return {
            getState: function () {
                return state;
            },
            setProjectId: function (id) {
                state.currentProjectId = id;
            },
            setSourceId: function (id) {
                state.currentSourceId = id;
            },
            setHasProjectData: function (hasData) {
                state.hasProjectData = hasData;
            }
        };
    }
    App.stateStore = StateStore();
})(App || (App = {}));
/// <reference path="../scripts/react.d.ts" />
var Components;
(function (Components) {
    Components.SourceStats = React.createClass({
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

            return (React.DOM.div(null, React.DOM.div(null, "Classified spans: ", file.codeTokens.length), React.DOM.div(null, "Referenced symbols: ", referencedSymbols)));
        }
    });
})(Components || (Components = {}));
/// <reference path="../scripts/ProjectStore.ts" />
/// <reference path="../scripts/Dispatcher.ts" />
/// <reference path="../scripts/AppState.ts" />
/// <reference path="./CodeSection.ts" />
/// <reference path="./ProjectList.ts" />
/// <reference path="./SourceStats.ts" />
/// <reference path="../scripts/react.d.ts" />
var Components;
(function (Components) {
    var dispatcher = App.dispatcher;
    var Store = App.stateStore;
    var project = App.project;

    Components.SolutionExplorer = React.createClass({
        displayName: 'SolutionExplorer',
        getInitialState: function () {
            return Store.getState();
        },
        componentWillMount: function () {
            dispatcher.register("selectedFileChanged", this.selectedFileChanged);
            dispatcher.register("selectedProjectChanged", this.selectedProjectChanged);
            dispatcher.register(App.StoreEvents.RecievedData, this.recievedData);
        },
        componentWillUnmount: function () {
            dispatcher.remove("selectedFileChanged", this.selectedFileChanged);
            dispatcher.remove("selectedProjectChanged", this.selectedProjectChanged);
            dispatcher.remove(App.StoreEvents.RecievedData, this.recievedData);
        },
        render: function () {
            if (!this.state.hasProjectData) {
                return React.DOM.div(null, "Downloading project data.");
            }

            var solutionPath = project.solutionData.path;
            var projects = project.solutionData.projects;

            var currProj = projects[this.state.currentProjectId];
            var currFile = project.getFile(this.state.currentSourceId);

            return (React.DOM.div(null, React.DOM.div({ className: "solutionExplorer" }, React.DOM.div(null, solutionPath), Components.ProjectList({ projects: projects })), React.DOM.div({ className: "codePanel" }, React.DOM.h3(null, project.getFile(this.state.currentSourceId).path), Components.SourceStats({ file: currFile }), Components.CodeSection({ file: currFile }))));
        },
        selectedFileChanged: function (sourceId) {
            Store.setSourceId(sourceId);
            this.setState(Store.getState());
        },
        selectedProjectChanged: function (projectId) {
            Store.setProjectId(projectId);
            this.setState(Store.getState());
        },
        recievedData: function () {
            Store.setHasProjectData(true);
            this.setState(Store.getState());
        }
    });
})(Components || (Components = {}));
/// <reference path="../components/SolutionExplorer.ts" />
React.renderComponent(Components.SolutionExplorer(null), document.getElementById("explorerContainer"));
//# sourceMappingURL=combined.js.map
