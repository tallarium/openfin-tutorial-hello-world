# OpenFin Tests
## Bug
App name never meets requirments
- `npm run server`
- `openfin -l -c http://localhost:9070/app.json`
- `rvm.log` contains `WARNING | VERBOSE | App name: ; does not meet requirements for a valid app name` error

## Testsnunit tests for testing the parts of OpenFin framework that we depend on.

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
