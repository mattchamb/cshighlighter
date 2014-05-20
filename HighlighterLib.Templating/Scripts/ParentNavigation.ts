function projectMessageDispatcher(projectId: string) {
    return function () {
        var message = {
            originProject: projectId,
            destinationPath: this.pathname
        };
        window.postMessage(message, "*");
    };
}