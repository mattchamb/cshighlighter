/// <reference path="./typings/knockout/knockout.d.ts" />

class FileIndexViewModel {
    constructor (public test: number) {
        var a = test * 5;
    }
    greet() {
        var v = "asdf";
        
        return this.test;
    }
}