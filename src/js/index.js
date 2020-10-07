document.addEventListener("DOMContentLoaded", function(){
    init();
});

function init(){
    /* Code common to both OpenFin and browser to go above.
     Then the specific code for OpenFin and browser only to be
     targeted in the try/catch block below.
     */
    try{
        fin.desktop.main(function(){
            initWithOpenFin();
        })
    }catch(err){
        initNoOpenFin();
    }
}

async function addListener() {
    const w = fin.Window.getCurrentSync()
    const event = "close-requested"
    await w.on(event, () => { window.fin.Application.getCurrentSync().close(true); });
    console.log("Listener count", w.listenerCount(event))
}

function initWithOpenFin(){
    console.log("OpenFin is available");
    // Your OpenFin specific code to go here...
    addListener();
}

function initNoOpenFin(){
    alert("OpenFin is not available - you are probably running in a browser.");
    // Your browser-only specific code to go here...
}
