# OpenFin Tests
## Bug
3 Tests (`AppHasDefaultSize`, `ResizeWindow` and `RestoreSnapshot`) fail in our circleci pipeline on this branch.  
The reason seems to be that `await app.getChildWindows()` is returning too soon.  
On version `19.89.57.15` the tests pass successfully but on `19.89.59.24` they are broken.

Error call stack:  
```
OpenQA.Selenium.WebDriverException : javascript error: Cannot read property 'getBounds' of undefined (Session info: chrome=89.0.4389.90)  
   at OpenQA.Selenium.Remote.RemoteWebDriver.UnpackAndThrowOnError(Response errorResponse)  
   at OpenQA.Selenium.Remote.RemoteWebDriver.Execute(String driverCommandToExecute, Dictionary\`2 parameters)  
   at OpenQA.Selenium.Remote.RemoteWebDriver.ExecuteScriptCommand(String script, String commandName, Object[] args)  
   at OpenQA.Selenium.Remote.RemoteWebDriver.ExecuteScript(String script, Object[] args)  
   at OpenfinDesktop.OpenfinTests.getWindowBounds() in C:\Users\circleci\repo\openfin-csharp-api-test\openfin-csharp-api-test\OpenfinTests.cs:line 161  
   at OpenfinDesktop.OpenfinTests.AppHasDefaultSize() in C:\Users\circleci\repo\openfin-csharp-api-test\openfin-csharp-api-test\OpenfinTests.cs:line 313
```  

## Tests
nunit tests for testing the parts of OpenFin framework that we depend on.

OpenFin provide a (pretty inactive) project demonstrating running tests in JavaScript (https://github.com/openfin/hello-openfin-selenium-example)

## Requirements
### openfin-cli
```
npm install -g openfin-cli
```

## Testing runtimes
### Selenium.WebDriver.ChromeDriver
The version of Selenium.WebDriver.ChromeDriver needs to match the version of Chrome used by the OpenFin runtime defined in app.json

e.g. OpenFin runtime 14.78.46.2 requires Selenium.WebDriver.ChromeDriver 78

You can select the correct version of Selenium.WebDriver.ChromeDriver in the nuget package manager.

### Command line
You can run the nunit tests from the command line
```
%USERPROFILE%\.nuget\packages\NUnit.ConsoleRunner\3.11.1\tools\nunit3-console.exe openfin-csharp-api-test.sln
```

## Limitations
Currently each nunit test needs to run independently to ensure they run successfully.
