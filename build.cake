#tool "nuget:?package=xunit.runner.console"

var target = Argument("target", "Default");
var solutionPath = "ParticleSwarmOptimizer.sln";

Task("Clean")
  .Does(()=>
  {
    CleanDirectories("bin/**");
  });

Task("NuGetRestore")
  .IsDependentOn("Clean")
  .Does(()=>
  {
    NuGetRestore("./");
  });

Task("Build")
  .IsDependentOn("NuGetRestore")
  .Does(()=>
  {
    if(IsRunningOnUnix())
    {
        XBuild(solutionPath, new XBuildSettings {
          Configuration = "Release"
        }.WithProperty("POSIX","True"));
    }
    else
    {
        MSBuild(solutionPath, new MSBuildSettings {
          Configuration = "Release"
        });
    }
  });

  Task("Tests")
    .IsDependentOn("Build")
    .Does(()=>
    {
      XUnit2("**/bin/**/*.Tests.*.dll");
    });

Task("Default")
  .IsDependentOn("Tests");

RunTarget(target);