
/// <reference path="./Dispatcher.ts" />

declare var $: any; //jQuery

module App {

    var dispatcher = App.dispatcher;

    export interface ISolutionData {
        path: string;
        projects: IProjectData[];
    }

    export interface IProjectData {
        path: string;
        sourceIds: number[];
    }

    export interface ISourceFileData {
        sourceId: number;
        path: string;
        codeTokens: ICodeToken[];
    }

    export interface ICodeToken {
        kind: string;
        text: string;
        symbolId?: number;
    }

    export interface ISymbolData {
        symbolId: number;
        displayText: string;
        locations: ISourceLocation[];
    }

    export interface ISourceLocation {
        sourceId: number;
        line?: number;
        col?: number;
        assembly?: string;
    }

    export var StoreEvents = {
        RecievedData: "ProjectDataReceived"
    };

    export class ProjectDataStore {

        private hasData: boolean; 

        public solutionData: ISolutionData;
        private sourceData: ISourceFileData[];
        private symbolData: ISymbolData[];

        constructor() {
            this.hasData = false;
            this.getData();
        }

        public getFile(fileId) : ISourceFileData {
            for (var i = 0; i < this.sourceData.length; i++) {
                var file = this.sourceData[i]
			    if (file.sourceId == fileId)
                    return file;
            }
            throw "Shit fucked up.";
        }

        public getSymbolInfo(symbolId: number) {
            return this.symbolData[symbolId]
        }

        private setData(sourceData, solutionData, symbolData) {
            this.solutionData = solutionData;
            this.sourceData = sourceData;
            this.symbolData = symbolData;
            this.hasData = true;
            dispatcher.trigger(StoreEvents.RecievedData, null);
        }

        private getData() {
            var self = this;
            $.getJSON("Source.json", function (sourceData) {
                $.getJSON("Symbols.json", function (symbolData) {
                    $.getJSON("Solution.json", function (solutionData) {
                        self.setData(sourceData, solutionData, symbolData);
                    });
                });
            });
        }
    }

    export var project = new ProjectDataStore();
}
