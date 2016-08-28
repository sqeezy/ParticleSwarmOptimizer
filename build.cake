var target = Argument("target", "Default");

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
        XBuild("ParticleSwarmOptimizer.sln",new XBuildSettings {
          Configuration = "Release"
        }.WithProperty("POSIX","True"));
    }
    else
    {
        MSBuild("ParticleSwarmOptimizer.sln", new MSBuildSettings {
          Configuration = "Release"
        });
    }
  });

Task("Default")
  .IsDependentOn("Build");

RunTarget(target);