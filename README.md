<h1 align="center">PlugUtopia</h1>
<h3 align="center">Develop, Connect and Deploy.</h3>

---
<p align="center">
<img alt="Logo Banner" src="https://raw.githubusercontent.com/pietroserrano/plugutopia/main/logo.png?sanitize=true"/>
<br/>

[![Nuget](https://img.shields.io/nuget/v/PlugUtopia.Plugin?label=PlugUtopia.Plugin)](https://www.nuget.org/packages/PlugUtopia.Plugin)
[![Nuget](https://img.shields.io/nuget/dt/PlugUtopia.Plugin?label=Download)](https://www.nuget.org/packages/PlugUtopia.Plugin)

[![GitHub Release](https://img.shields.io/github/v/release/pietroserrano/plugutopia?label=PlugUtopia.Plugin.Tools&filter=PlugUtopia.Plugin.Tools*)](https://github.com/pietroserrano/plugutopia/pkgs/nuget/PlugUtopia.Plugin.Tools)
[![GitHub Release](https://img.shields.io/github/v/release/pietroserrano/plugutopia?label=PlugUtopia.Console&filter=PlugUtopia-Console*)](https://github.com/pietroserrano/plugutopia/pkgs/container/plugutopia-console)

---

## Getting Started

docker-compose.yml example

```yaml
version: '3.5'

services:
  plugutopia-console:
    image: plugutopia-console
    
    volumes:
      - ./plugins:/app/plugins

    environment:
      - BotConfiguration__BotToken=<token>
      - BotConfiguration__Whitelist__0=<username1>
      - BotConfiguration__Whitelist__N=<usernameN>

```
or

```shell

docker build . -f Apps/PlugUtopia.Console/Dockerfile -t plugutopia-console

```

or

```shell

docker buildx build --platform linux/amd64 . -f Apps/PlugUtopia.Console/Dockerfile -t plugutopia-console

```

## How to create plugin

1. Install dotnet templatate

```bash
 dotnet new install PlugUtopia.Plugin.Templates
```

2. Create project

```csharp
mkdir path/to/proj/proj_name
dotnet new plugutopia-plugin -n proj_name  
```

3. Install the tool needed to create the manifest:
```bash
dotnet nuget add source -u [GithubUserName] -p [YourApiKey] -n gh-plugutopia https://nuget.pkg.github.com/pietroserrano/index.json
dotnet tool install -g PlugUtopia.Plugin.Tools
```

## How to add plugin infrastructure to existing app

1. Add the following statements to your project file:

```xml
<Target Name="PostBuild" AfterTargets="PostBuildEvent">
  <Exec Command="plugutopia-tools --generate-manifest $(OutDir) $([System.IO.Path]::Combine($(TargetDir), $(AssemblyName)))" />
</Target>
```

2. Create a new internal-manifest.json file to your project and add it as an embedded resource
```json
{
  "category": "",
  "changelog": "github.com/readme.md",
  "description": "plugin description",
  "name": "plugin name",
  "overview": "",
  "owner": "",
  "targetAbi": "1.0.0",
  "guid": ""
}
```

```xml
<ItemGroup>
  <None Remove="internal-manifest.json" />
  <EmbeddedResource Include="internal-manifest.json" />
</ItemGroup>
```

3. Install the tool needed to create the manifest:
```bash
dotnet nuget add source -u [GithubUserName] -p [YourApiKey] -n gh-plugutopia https://nuget.pkg.github.com/pietroserrano/index.json
dotnet tool install -g PlugUtopia.Plugin.Tools
```

4. Verify that, after compiling the project, the manifest.json file is created correctly and that it contains all the DLLs needed for the plugin to work.
