const w = fin.Window.getCurrentSync();
console.log("Listener count: ", w.listenerCount("close-requested"))
w.removeAllListeners();
console.log("Listener count: ", w.listenerCount("close-requested"))
