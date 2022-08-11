# NetWebAssemblyTSTypeGenerator

## Usage

Replace your csproj file.

example
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
    <RuntimeIdentifier>browser-wasm</RuntimeIdentifier>
    <WasmMainJSPath>main.mts</WasmMainJSPath>
    <OutputType>Exe</OutputType>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>

    <!-- Required -->
    <JSPortOverrideTypeDefinitionOutputDir>__REPLACE_ME_YOUR_PROJECT_TYPES_DIR_</JSPortOverrideTypeDefinitionOutputDir>
    <!-- Optional -->
    <JSPortOverrideTargetProjectInheritRuntimeType>true</JSPortOverrideTargetProjectInheritRuntimeType>
  </PropertyGroup>

  <ItemGroup>
    <WasmExtraFilesToDeploy Include="app-support.mjs" />

    <PackageReference Include="NetWebAssemblyTSTypeGenerator" Version="*" OutputItemType="Analyzer" />

    <!-- Required -->
    <CompilerVisibleProperty Include="JSPortOverrideTypeDefinitionOutputDir" />
    <!-- Optional -->
    <CompilerVisibleProperty Include="JSPortOverrideTargetProjectInheritRuntimeType" />
  </ItemGroup>
</Project>
```