SlowMonkey allows you to use the same kind of config transformations that are used in ASP.NET projects with executable and class library projects. It is inspired by Sayed Hashimi's [SlowCheetah](https://github.com/sayedihashimi/slow-cheetah) package but is designed to work with Mono's xbuild as well as .NET's MSBuild.

## How to use

1) Install the SlowMonkey NuGet package

2) Add an app.config file to your project if it does not already have one.

3) Add app.Debug.config, app.Release.config, etc files to your project with the desired config transforms. See http://msdn.microsoft.com/en-us/library/dd465326%28v=vs.110%29.aspx for documentation on the transform syntax.

4) Build. The transform for the current build config, if present, will be applied.

SlowMonkey has the explicit goal of working on both Mono and .NET. I have tested on Mono 3.2.8. It *probably* works on Mono 2.10.8.1 but you may have to replace

```
  <Import Project="..\packages\SlowMonkey.1.0.0\build\SlowMonkey.targets" Condition="Exists('..\packages\SlowMonkey.1.0.0\build\SlowMonkey.targets')" />
  <Target Name="EnsureNuGetPackageBuildImports" BeforeTargets="PrepareForBuild">
    <PropertyGroup>
      <ErrorText>This project references NuGet package(s) that are missing on this computer. Enable NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is {0}.</ErrorText>
    </PropertyGroup>
    <Error Condition="!Exists('..\packages\SlowMonkey.1.0.0\build\SlowMonkey.targets')" Text="$([System.String]::Format('$(ErrorText)', '..\packages\SlowMonkey.1.0.0\build\SlowMonkey.targets'))" />
  </Target>
```

with

```
<Import Project="..\packages\SlowMonkey.1.0.0\build\SlowMonkey.targets" />
```

in your .csproj after the package installs.

## Rationale

SlowCheetah works great with .NET but makes some assumptions that are not true on Linux (such as the existence of a LocalAppData environment variable and the use of backslashes in paths), uses MSBuild features that are not available in Mono (such as ItemDefinitionGroups - https://bugzilla.xamarin.com/show_bug.cgi?id=10017), uses MSBuild features that are buggy in Mono (https://bugzilla.xamarin.com/show_bug.cgi?id=24211, https://bugzilla.xamarin.com/show_bug.cgi?id=24366), and uses the stock Microsoft.Web.XmlTransform.dll, which is affected by Mono bugs https://bugzilla.xamarin.com/show_bug.cgi?id=19426 and https://bugzilla.xamarin.com/show_bug.cgi?id=19447. SlowCheetah has a lot of extra features like support for ClickOnce projects and uses a lot of advanced MSBuild features. Rather than forking SlowCheetah and attempting to port all those features to Mono, SlowMonkey keeps it simple with just plain old executable and class library support. No Visual Studio addin or manually adding TransformOnBuild metadata to the .csproj is required with SlowMonkey, just install the NuGet package and build.

Thanks to Sayed Hashimi for creating SlowCheetah, for [pressuring Microsoft to release Microsoft.Web.Xdt under Apache 2.0](https://nuget.codeplex.com/discussions/405195) so it could be used by NuGet, and for writing his excellent book on MSBuild, without which I wouldn't know how to write SlowMonkey.

Thanks to Matt Ward for [patching XDT to work with Mono](https://github.com/mrward/xdt), saving me the trouble of figuring out why the stock XDT throws InvalidDataExceptions under Mono.