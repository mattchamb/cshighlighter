module App {
    export class EventDispatcher {

        private events;

        public constructor() {
            this.events = {}; 
        }

        public register(event: string, callback) {
            if (!this.events[event]) {
                this.events[event] = [];
            }
            this.events[event].push(callback); 
        } 

        public remove(event, callback) {
            var listeners = this.events[event];
            if (!listeners) {
                throw "No event listeners added for " + event;
            }
            this.events[event] = listeners.splice(listeners.indexOf(callback), 1);

            if (this.events[event].length = 0) {
                delete this.events[event];
            }
        }

        public trigger(event, arg) {
            var listeners = this.events[event];
            if (!listeners) {
                return;
            }

            for (var i = 0; i < listeners.length; i++) { 
                var l = listeners[i];
                l(arg);
            }
        }
    }

    export var dispatcher = new EventDispatcher();
}
