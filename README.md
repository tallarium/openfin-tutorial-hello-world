# OpenFin Tests
nunit tests for testing the parts of OpenFin framework that we depend on.

OpenFin provide a (pretty inactive) project demonstrating running tests in JavaScript (https://github.com/openfin/hello-openfin-selenium-example)

## Testing runtimes
### Selenium.WebDriver.ChromeDriver
The version of Selenium.WebDriver.ChromeDriver needs to match the version of Chrome used by the OpenFin runtime defined in app.json

e.g. OpenFin runtime 14.78.46.2 requires Selenium.WebDriver.ChromeDriver 78

You can select the correct version of Selenium.WebDriver.ChromeDriver in the nuget package manager.

## Limitations
Currently each nunit test needs to run independently to ensure they run successfully.
