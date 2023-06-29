# BrokenNuGetReference-PackageIdInCommonBuildDemo
Demonstrates the broken package reference that results from defining the PackageId of a referenced project in a *.props-file of another NuGet package.

Issues  
https://github.com/dotnet/sdk/issues/33528  
https://github.com/NuGet/Home/issues/12544

## Steps to reproduce
1. clone the repo
2. execute _3_Demo\nuget pack.ps1_ (or the dotnet pack command that is inside of it)
3. Extract the _MyCompany.MyProject.Extended.nuspec_ from the package _3_Demo\MyProject.Extended\bin\Release\MyCompany.MyProject.Extended.1.0.0.nupkg_ that was just created.
4. Open the _MyCompany.MyProject.Extended.nuspec_ in an editor and have a look at the dependencies section.
5. You will see the following dependencies:

        <dependency id="MyCompany.MyProject.A_SetInProject" version="1.0.0" exclude="Build,Analyzers" />

        <dependency id="MyCompany.MyProject.B_SetInDirectoryBuildProps" version="1.0.0" exclude="Build,Analyzers" />

        <dependency id="MyProject.C_SetInNuGetPackage" version="1.0.0" exclude="Build,Analyzers" />
6. The fist two dependencies are fine, but the third one is missing the _MyCompany._-Prefix

## Description
### Background (Why am I doing this? / What do I want to achieve?)
I'm trying to get a consistent build for multiple applications/projects/repositories in our company.  
To do so I was creating a separate NuGet package containing some .props/.targets files. For example I'm trying to accomplish:
- Company name as prefix for assembly name, root namespace, package id.  
(So the project name can be shorter in solution explorer)
- Consistent versioning in Jenkins
- Consistent settings for Nullable Reference Types, ImplicitUsings, NeutralLanguage, License, ...
- ...

Of course the target projects could still overwrite the settings, but I'm trying to define common default settings.

I noticed a problem setting the PackageId when there are multiple projects in a solution with project references amongst each other.  
This is why I created this demo repository with simplified code of what I want to achieve.

### Repository Contents
The repository contains 3 directories. Unless you're interested in how I set up all of this you can ignore directories 1 and 2 and jump straight to directory 3 that contains the actual demo.

- 1_CommonBuild  
  Contains an example project how I created my common build package.  
  Basically it contains a project that packs a props and a targets file to a NuGet package. The contained props file sets the prefix _MyCompany._ to the PackageId.

  Regarding this bug you can take this as granted unless you want to see how I created this package.  
  The _nuget pack and push locally.ps1_ pushes the created nupkg to the second folder.

- 2_LocalNuGetRepo  
  Contains the resulting nupkg from _1_CommonBuild_.  
  The solution in _3_Demo_ is configured to use this directory as local NuGet feed.  
  Typically this wouldn't be committed to the git repo, but for demonstration purposes I thought it would be easier this way.

- 3_Demo  
  This directory contains the actual demo solution.  
  For details see next chapter.

### Description of the solution in 3_Demo
The solution consists of 4 projects. The first three projects _MyProject.A_SetInProject_, _MyProject.B_SetInDirectoryBuildProps_ and _MyProject.C_SetInNuGetPackage_ show 3 different ways of setting the PackageId. The fourth project _MyProject.Extended_ depends on all three other projects and demonstrates, that the dependencies to A and B are fine, but the dependency to C is broken in the resulting NuGet package.

- MyProject.A_SetInProject  
  Sets the PackageId directly in the csproj file.  
  Works like expected.

- MyProject.B_SetInDirectoryBuildProps  
  Sets the PackageId in a local Directory.Build.props file.  
  Works like expected.

- MyProject.C_SetInNuGetPackage  
  The PackageId is set in the NuGet package _MyCompany.CommonBuild_.  
  The resulting NuGet package for this project actually is totally fine. **So regarding this project itself everything works like expected.**

- MyProject.Extended  
  **This is where the bug appears.**  
  This project has references to the above 3 projects. During design and compile time everything works like expected.  
  But when the project is packed, the dependency to project C is broken. It is missing the _MyCompany._ prefix. The dependencies to project A and B are fine.

## Remarks
- The demo was created with Visual Studio 2022 (17.6.3) and .Net 7.0.304.

- Complete broken nuspec file of project _MyProject.Extended_:

        <?xml version="1.0" encoding="utf-8"?>
        <package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
        <metadata>
            <id>MyCompany.MyProject.Extended</id>
            <version>1.0.0</version>
            <authors>MyCompany.MyProject.Extended</authors>
            <description>Package Description</description>
            <dependencies>
            <group targetFramework=".NETStandard2.0">
                <dependency id="MyCompany.MyProject.A_SetInProject" version="1.0.0" exclude="Build,Analyzers" />
                <dependency id="MyCompany.MyProject.B_SetInDirectoryBuildProps" version="1.0.0" exclude="Build,Analyzers" />
                <dependency id="MyProject.C_SetInNuGetPackage" version="1.0.0" exclude="Build,Analyzers" />
            </group>
            </dependencies>
        </metadata>
        </package>

- Log of packing the solution in _3_Demo_:

        MSBuild version 17.6.3+07e294721 for .NET
        Wiederherzustellende Projekte werden ermittelt...
        "C:\Data\Dev\BrokenNuGetReference-PackageIdInCommonBuildDemo\3_Demo\MyProject.Extended\MyProject.Extended.csproj" wiederhergestellt (in "247 ms").
        "C:\Data\Dev\BrokenNuGetReference-PackageIdInCommonBuildDemo\3_Demo\MyProject.C_SetInNuGetPackage\MyProject.C_SetInNuGetPackage.csproj" wiederhergestellt (in "247 ms").
        2 von 4 Projekten sind für die Wiederherstellung auf dem neuesten Stand.
        ==MyProject.B_SetInDirectoryBuildProps Properties Begin==
        ==CommonBuild_Properties Begin (MyProject.C_SetInNuGetPackage)==
        RootNamespace = 'MyCompany.MyProject.B_SetInDirectoryBuildProps'
        RootNamespace = 'MyCompany.MyProject.C_SetInNuGetPackage'
        AssemblyName = 'MyCompany.MyProject.B_SetInDirectoryBuildProps'
        AssemblyName = 'MyCompany.MyProject.C_SetInNuGetPackage'
        PackageId = 'MyCompany.MyProject.C_SetInNuGetPackage'
        ==CommonBuild_Properties End (MyProject.C_SetInNuGetPackage)==
        PackageId = 'MyCompany.MyProject.B_SetInDirectoryBuildProps'
        ==MyProject.B_SetInDirectoryBuildProps Properties End==
        MyProject.C_SetInNuGetPackage -> C:\Data\Dev\BrokenNuGetReference-PackageIdInCommonBuildDemo\3_Demo\MyProject.C_SetInNuGetPackage\bin\Release\netstandard2.0\MyCompany.MyProject.C_SetInNuGetPackage.dll
        MyProject.B_SetInDirectoryBuildProps -> C:\Data\Dev\BrokenNuGetReference-PackageIdInCommonBuildDemo\3_Demo\MyProject.B_SetInDirectoryBuildProps\bin\Release\netstandard2.0\MyCompany.MyProject.B_SetInDirectoryBuild
        Props.dll
        MyProject.A_SetInProject -> C:\Data\Dev\BrokenNuGetReference-PackageIdInCommonBuildDemo\3_Demo\MyProject.A_SetInProject\bin\Release\netstandard2.0\MyCompany.MyProject.A_SetInProject.dll
        ==MyProject.B_SetInDirectoryBuildProps Properties Begin==
        RootNamespace = 'MyCompany.MyProject.B_SetInDirectoryBuildProps'
        AssemblyName = 'MyCompany.MyProject.B_SetInDirectoryBuildProps'
        PackageId = 'MyCompany.MyProject.B_SetInDirectoryBuildProps'
        ==MyProject.B_SetInDirectoryBuildProps Properties End==
        ==CommonBuild_Properties Begin (MyProject.C_SetInNuGetPackage)==
        RootNamespace = 'MyCompany.MyProject.C_SetInNuGetPackage'
        AssemblyName = 'MyCompany.MyProject.C_SetInNuGetPackage'
        PackageId = 'MyCompany.MyProject.C_SetInNuGetPackage'
        ==CommonBuild_Properties End (MyProject.C_SetInNuGetPackage)==
        ==MyProject.B_SetInDirectoryBuildProps Properties Begin==
        RootNamespace = 'MyCompany.MyProject.B_SetInDirectoryBuildProps'
        AssemblyName = 'MyCompany.MyProject.B_SetInDirectoryBuildProps'
        PackageId = 'MyCompany.MyProject.B_SetInDirectoryBuildProps'
        ==MyProject.B_SetInDirectoryBuildProps Properties End==
        ==CommonBuild_Properties Begin (MyProject.C_SetInNuGetPackage)==
        RootNamespace = 'MyCompany.MyProject.C_SetInNuGetPackage'
        AssemblyName = 'MyCompany.MyProject.C_SetInNuGetPackage'
        PackageId = 'MyCompany.MyProject.C_SetInNuGetPackage'
        ==CommonBuild_Properties End (MyProject.C_SetInNuGetPackage)==
        The package MyCompany.MyProject.A_SetInProject.1.0.0 is missing a readme. Go to https://aka.ms/nuget/authoring-best-practices/readme to learn why package readmes are important.
        Das Paket "C:\Data\Dev\BrokenNuGetReference-PackageIdInCommonBuildDemo\3_Demo\MyProject.A_SetInProject\bin\Release\MyCompany.MyProject.A_SetInProject.1.0.0.nupkg" wurde erfolgreich erstellt.
        The package MyCompany.MyProject.B_SetInDirectoryBuildProps.1.0.0 is missing a readme. Go to https://aka.ms/nuget/authoring-best-practices/readme to learn why package readmes are important.
        Das Paket "C:\Data\Dev\BrokenNuGetReference-PackageIdInCommonBuildDemo\3_Demo\MyProject.B_SetInDirectoryBuildProps\bin\Release\MyCompany.MyProject.B_SetInDirectoryBuildProps.1.0.0.nupkg" wurde erfolgreich erstellt.
        The package MyCompany.MyProject.C_SetInNuGetPackage.1.0.0 is missing a readme. Go to https://aka.ms/nuget/authoring-best-practices/readme to learn why package readmes are important.
        Das Paket "C:\Data\Dev\BrokenNuGetReference-PackageIdInCommonBuildDemo\3_Demo\MyProject.C_SetInNuGetPackage\bin\Release\MyCompany.MyProject.C_SetInNuGetPackage.1.0.0.nupkg" wurde erfolgreich erstellt.
        MyProject.Extended -> C:\Data\Dev\BrokenNuGetReference-PackageIdInCommonBuildDemo\3_Demo\MyProject.Extended\bin\Release\netstandard2.0\MyCompany.MyProject.Extended.dll
        The package MyCompany.MyProject.Extended.1.0.0 is missing a readme. Go to https://aka.ms/nuget/authoring-best-practices/readme to learn why package readmes are important.
        Das Paket "C:\Data\Dev\BrokenNuGetReference-PackageIdInCommonBuildDemo\3_Demo\MyProject.Extended\bin\Release\MyCompany.MyProject.Extended.1.0.0.nupkg" wurde erfolgreich erstellt.
