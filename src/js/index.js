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
};

async function testFetch() {
    const response = await fetch("https://postman-echo.com/post", {
        method: 'POST',
        mode: 'no-cors',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ a: 1, b: 2 }),
    });
    console.log("fetch was successful");
}
function initWithOpenFin(){
    alert("OpenFin is available");

    testFetch();
    // Your OpenFin specific code to go here...
}

function initNoOpenFin(){
    alert("OpenFin is not available - you are probably running in a browser.");
    // Your browser-only specific code to go here...
}
