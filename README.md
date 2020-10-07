# OpenFin Tests

## Bug
`close-requested` event listener prevents application close even after the handler code is destroyed  (e.g. Change in url)
- Open the app
- Console log shows that there is one listener
- (It is possible to close the app)
- Click the "Go to page 2" link
- Console log shows that there are no listeners
- It is no longer possible to close the app/window

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
