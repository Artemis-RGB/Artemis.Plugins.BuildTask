# Artemis.Plugins.BuildTask
MSBuild task that copies the just-built plugin binaries to the Artemis plugin directory. Useful for instantly debugging the plugins without cluttering the project file.

# Usage

```xml
<ItemGroup>
    <PackageReference Include="Artemis.Plugins.BuildTask" Version="1.0.0" />
</ItemGroup>
```

# Configuration

By default, the task will run if the build is done in Visual Studio or JetBrains Rider. 
This behaviour can be overriden in the csproj file with :
```xml
<PropertyGroup>
    <EnablePluginCopy Condition="$(SomeUsefulProperty)">true</EnablePluginCopy>
</PropertyGroup>
```

or via the dotnet command line interface:
`dotnet build -p:EnablePluginCopy=true`

# Note

This cannot be used yet as it hasn't been published to NuGet due to a package id conflict.