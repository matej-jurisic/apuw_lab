dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings; `
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:"Html"; `
Start-Process "coverage-report\index.html"
