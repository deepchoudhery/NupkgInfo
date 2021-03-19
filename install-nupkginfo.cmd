set VERSION=1.0.0
set SRC_DIR=%cd%
set NUPKG=bin\Debug\

call dotnet pack NupkgInfo.sln

call dotnet tool uninstall -g NupkgInfo
call cd %SRC_DIR%/%NUPKG% 
call dotnet tool install -g NupkgInfo --add-source %SRC_DIR%\%NUPKG% --version %VERSION%
cd %SRC_DIR%
