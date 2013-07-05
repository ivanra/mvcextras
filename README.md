# MvcExtras

A few extra bits of functionality to be used with Microsoft ASP.NET MVC framework.

## Prerequisites

* Visual Studio 2012
* (optional) [NuGet Package Manager Visual Studio Extension][nuget-extension]
* (optional) [xUnit Test Runner Visual Studio Extension][xunit-test-runner]

## Building the project

The project is using [NuGet][nuget-site] package manager, so the necessary dependencies will be restored/downloaded from the official NuGet repositories on first build.
Make sure that restoring packages during build is enabled in your environment. Check out [NuGet site][nuget-restore-on-build] for more details.

For building binaries, and running unit tests, from the command line, use `build.cmd` script. For the list of available build targets, use `build.cmd --help`.


## License

This code is Copyright (c) 2012-2013 Ivan Radanovic &lt;ivanra at gmail&gt; and distributed under MIT license. See `LICENSE` file for more information.


[nuget-site]: http://nuget.org "NuGet Package Manager"
[nuget-restore-on-build]: http://docs.nuget.org/docs/workflows/using-nuget-without-committing-packages#Enabling_Package_Restore_During_Build "NuGet Restore On Build"
[nuget-extension]: http://visualstudiogallery.msdn.microsoft.com/27077b70-9dad-4c64-adcf-c7cf6bc9970c "NuGet Package Manager Visual Studio Extension" 
[xunit-test-runner]: http://visualstudiogallery.msdn.microsoft.com/463c5987-f82b-46c8-a97e-b1cde42b9099 "xUnit Test Runner Visual Studio Extension" 
[mvcextras-csvfileresult]: README.CsvFileResult.md "CsvFileResult"
