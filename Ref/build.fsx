#r "System.Xml.Linq.dll"
#r "System.Management.Automation"
#r @"packages\Fake\tools\FakeLib.dll"
#r @"packages\WTG.DevTools.Common\lib\net451\WTG.DevTools.Common.VisualStudio.dll"

open System.Xml.Linq
open System.IO
open System
open Fake
open System.Collections.Generic
open WTG.DevTools.Common

let xnn s = XName.Get(s, "http://wisetechglobal.com/DevTools/Build.xsd")
let xn s = XName.Get(s)
let xd = XDocument.Load(@"build.xml")

let getFolder relativePath = Path.Combine(currentDirectory, relativePath)
let msBuildLoc = VisualStudioHelpers.GetMSBuildPath(new System.Version("15.0"), Architecture.x64);

let deploy (args: IDictionary<string, string>) =
    let argsString = args |> Seq.map (fun x -> String.Format(@"-{0}:""{1}""", x.Key, x.Value)) |> String.concat " "
    Shell.Exec("powershell.exe", String.Format("-NonInteractive -ExecutionPolicy ByPass -File Deploy.ps1 {0}", argsString), getFolder @"bin\Tools\")


Target "DeployTest" (fun _ ->
    let args = dict["sourcePath", getFolder ""; "binPath", getFolder "bin"; "stagingTaskPath", @"RefDbRepoDeployTest\\"; "publishProfile","Test"; 
        "stagingDestination", @"\\SYDSP-SWEB-1.sand.wtg.zone\StagingDeployTest\\"; "stagingServer", "SYDSP-SWEB-1.sand.wtg.zone"; 
        "dbWriterPassword", ""; "dbReaderPassword", ""; "msbuild", msBuildLoc]
    let result = deploy args
    if result > 0 then failwithf "Deployment Error"
)

Target "DeployUAT" (fun _ ->
    let args = dict["sourcePath", getFolder ""; "binPath", getFolder "bin"; "stagingTaskPath", @"RefDbRepo\\"; "publishProfile", "UAT"; 
        "stagingDestination", @"\\SYDSP-SWEB-1.sand.wtg.zone\Staging\\"; "stagingServer", "SYDSP-SWEB-1.sand.wtg.zone"; 
        "dbWriterPassword", ""; "dbReaderPassword", ""; "msbuild", msBuildLoc]
    let result = deploy args
    if result > 0 then failwithf "Deployment Error"
)

Target "DeployPRO" (fun _ ->
    let args = dict["sourcePath", getFolder ""; "binPath", getFolder "bin"; "stagingTaskPath", @"RefDbRepo\\"; "publishProfile", "PRO"; 
        "stagingDestination", @"\\SYDWP-SAPP-6.wisecloud.zone\RefDbRepo\\"; "stagingServer", "SYDWP-SAPP-6.wisecloud.zone"; 
        "dbWriterPassword", ""; "dbReaderPassword", ""; "msbuild", msBuildLoc; "deliveryServiceDeployUsername", "SYDWP-SWEB-1\RefDbRepo_WebDeploy";
        "updateServiceDeployUsername", @"PROD\TV"; "stagingDeployUsername", @"PROD\TV"; "deliveryServiceDeployPassword", "";
        "updateServiceDeployPassword", ""; "stagingDeployPassword", ""; "dbPassword", ""]
    let result = deploy args
    if result > 0 then failwithf "Deployment Error"
)

Target "Paket" (fun _ -> 
    Paket.Restore(fun p -> 
        p.ToolPath = @"ThirdParty\Paket\Paket.exe" |> ignore
        p
    )
)

Target "Clean" (fun _ ->
    CleanDir @"bin" 
)

Target "CopyPackages" (fun _ ->
    let files = xd.Descendants(xnn "Filename")
    let destFile (node:XElement) = @"bin\" + node.Value
    let sourceFile (node:XElement) = node.Attribute(xn "CopyFrom").Value + @"\" + Path.GetFileName (destFile node)
    let copyFile node =
        CreateDir(Path.GetDirectoryName(destFile node))
        CopyFile (destFile node) (sourceFile node)
    files |> Seq.iter copyFile
    let directories = xd.Descendants(xnn "Directory")
    let destDir (node: XElement) = @"bin\" + node.Value
    let srcDir (node: XElement) = node.Attribute(xn "CopyFrom").Value
    let copyDir node = CopyDir (destDir node) (srcDir node) (fun _ -> true)
    directories |> Seq.iter copyDir
)

let solutions = xd.Descendants(xnn "Solution") |> Seq.map (fun node -> node.Attribute(xn "Filename").Value)

Target "DebugBuild" (fun _ ->
    solutions
        |> MSBuildDebug "" "Build"
        |> Log "DebugBuild-Output:"
)

Target "ReleaseBuild" (fun _ ->
    solutions
        |> MSBuildRelease "" "Build"
        |> Log "ReleaseBuild-Output:"
)

"Paket", "Clean"
    ==> "CopyPackages"

"CopyPackages"
    ==> "DebugBuild", "ReleaseBuild"

"ReleaseBuild"
    ==> "DeployTest", "DeployUAT"

RunTargetOrDefault "DebugBuild"
