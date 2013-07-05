@echo off

if /i _%1 == _-h goto Help
if /i _%1 == _--help goto Help

if not exist %~dp0bin mkdir %~dp0bin

if _%1 == _ goto BuildDefault

%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild MvcExtras.msbuild /m /t:%* /p:Platform="Any CPU" /flp:LogFile=bin\msbuild.log;Verbosity=Normal
if errorlevel 1 goto BuildFailed
goto BuildSuccess

:BuildDefault
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild MvcExtras.msbuild /m /p:Platform="Any CPU" /flp:LogFile=bin\msbuild.log;Verbosity=Normal
if errorlevel 1 goto BuildFailed
goto BuildSuccess

:Help
echo Available build targets:
for /f delims^=^"^ tokens^=2 %%i in ('findstr /c:"Target Name" MvcExtras.msbuild') do echo;  %%i
goto :EOF

:BuildFailed
echo;
echo **** FAILED ****
exit /b 1

:BuildSuccess
echo;
echo **** SUCCESSFUL ****
