var target = Argument("target", "Default");

var bepInExVersion = XmlPeek("./Directory.Build.props", "/Project/PropertyGroup/BepInExVersion");
var packageVersion = XmlPeek("./Build/Build.csproj", "/Project/PropertyGroup/PackageVersion");

var distDir = Directory("./dist");
var extractDir = distDir + Directory("BepInEx");
var downloadUrl = $"https://github.com/BepInEx/BepInEx/releases/download/v{bepInExVersion}/BepInEx_win_x64_{bepInExVersion}.zip";
var zipFile = distDir + File($"BepInEx_win_x64_{bepInExVersion}.zip");

void EnsureClean(DirectoryPath dir)
{
    if (DirectoryExists(dir)) CleanDirectory(dir);
    else CreateDirectory(dir);
}

Task("Clean")
    .Does(() =>
{
    EnsureClean(distDir);
});

Task("DownloadBepInEx")
    .IsDependentOn("Clean")
    .Does(() =>
{
    Information($"Downloading BepInEx v{bepInExVersion}...");
    if (!FileExists(zipFile))
    {
        DownloadFile(downloadUrl, zipFile);
        Information($"Downloaded to: {zipFile}");
    }
    else
    {
        Information("File already exists, skipping download.");
    }
});

Task("ExtractBepInEx")
    .IsDependentOn("DownloadBepInEx")
    .Does(() =>
{
    Information($"Extracting BepInEx to {extractDir}...");
    EnsureClean(extractDir);
    Unzip(zipFile, extractDir);
    Information("Extraction completed.");
});

Task("Configure")
    .IsDependentOn("ExtractBepInEx")
    .Does(() =>
{
    Information("Configuring BepInEx for Goblin Cleanup...");

    var doorstopConfig = extractDir + File("doorstop_config.ini");
    if (FileExists(doorstopConfig))
    {
        var path = doorstopConfig.ToString();
        var content = System.IO.File.ReadAllText(path);
        content = content.Replace("dll_search_path_override=", "dll_search_path_override=UnstrippedCorlib");
        System.IO.File.WriteAllText(path, content);
        Information("Patched doorstop_config.ini: dll_search_path_override = UnstrippedCorlib");
    }

    var unstrippedTarget = extractDir + Directory("UnstrippedCorlib");
    CreateDirectory(unstrippedTarget);
    CopyDirectory("./UnstrippedCorlib", unstrippedTarget);
    Information("Copied UnstrippedCorlib DLLs");
});

Task("PrintVersion")
    .Does(() =>
{
    Information(packageVersion);
});

Task("Build")
    .IsDependentOn("Configure")
    .Does(() =>
{
    Information($"Building with package version {packageVersion}...");
    var exitCode = StartProcess("dotnet", new ProcessSettings
    {
        Arguments = $"tcli build --package-version {packageVersion}",
        WorkingDirectory = Directory(".")
    });
    if (exitCode != 0)
    {
        throw new Exception($"dotnet tcli build failed with exit code {exitCode}");
    }
    Information("Build completed successfully.");
});
Task("Prepare-UnstrippedCorlib")
    .Does(() => PrepareUnstrippedCorlib.Run(Context,
        Argument("unity-version", XmlPeek("./Build/Build.csproj", "/Project/PropertyGroup/UnityVersion")),
        Argument("unity-hub", XmlPeek("./Build/Build.csproj", "/Project/PropertyGroup/UnityHubDir")),
        Argument("game-path", XmlPeek("./Build/Build.csproj", "/Project/PropertyGroup/GameDir"))));

Task("Default")
    .IsDependentOn("Build");

RunTarget(target);
