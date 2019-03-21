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

function runMyAsset(alias) {
    return new Promise((resolve, reject) => {
        fin.desktop.System.launchExternalProcess({
            alias,
            listener: function (result) {
                console.log('the exit code', result.exitCode);
                resolve();
            }
        }, function (payload) {
            console.log('Success:', payload.uuid);
        }, function (error) {
            console.log('Error:', error);
            reject(error);
        });

    })
}

function downloadV2() {
    const appAsset = {
        "src": "http://localhost:9070/assets/assetv2.zip",
        "alias": "myAsset2",
        "version": "2.0",
        "target": "echo.vbs"
    };
    return new Promise((resolve, reject) => {
        fin.desktop.System.downloadAsset(appAsset, progress => {
            //Print progress as we download the asset.
            const downloadedPercent = Math.floor((progress.downloadedBytes / progress.totalBytes) * 100);
            console.log(`Downloaded ${downloadedPercent}%`);
        }, () => {
            //asset download complete, launch
            resolve(appAsset);
        }, (reason, error) => {
            //Failed the download.
            console.log(reason, error);
            reject(reason);
        });
    })
}

async function initWithOpenFin(){
    alert("OpenFin is available");
    await runMyAsset("myAsset");
    await downloadV2();
    await runMyAsset("myAsset2");
    // Your OpenFin specific code to go here...
}

function initNoOpenFin(){
    alert("OpenFin is not available - you are probably running in a browser.");
    // Your browser-only specific code to go here...
}
