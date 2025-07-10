$Source = @"
using System;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;

public class Program
{
    public static void Deploy(string binPath)
    {
        AppDomain.CurrentDomain.AssemblyResolve += (s, eArgs) => Assembly.LoadFile(Path.Combine(binPath, eArgs.Name.Split(',')[0] + ".dll"));

        var filename = Path.Combine(binPath, "TestEnvironment", "ProtectedData.dat");
        var drName = Path.GetDirectoryName(filename);
        var drInfo = new DirectoryInfo(drName);

        Directory.CreateDirectory(drName);

        var drSecurity = drInfo.GetAccessControl();
        var everyOne = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
        drSecurity.AddAccessRule(new FileSystemAccessRule(everyOne, FileSystemRights.FullControl, AccessControlType.Allow));

        drInfo.SetAccessControl(drSecurity);

        var testBedAssembly = Assembly.Load("Cargowise.DataProtection.TestFramework");
        var testBedType = testBedAssembly.GetType("CargoWise.DataProtection.TestFramework.DataProtectionTestBed");
        var testBed = Activator.CreateInstance(testBedType, new object[] { filename });
        testBedType.GetMethod("GenerateInitialProtectedDataForTestExecutionEnvironment").Invoke(testBed, new object[] { });
    }
}
"@

$binPath = Join-Path "$PSScriptRoot" "bin"

Write-Host "Initializing Test Environment Protected Data for $binPath"

Add-Type -TypeDefinition $Source

[Program]::Deploy($binPath)

$net8BinPath = [IO.Path]::Combine("$PSScriptRoot", "bin", "net8.0")
Write-Host "Initializing Test Enironment Protected Data for $net8BinPath"
[Program]::Deploy($net8BinPath)

Write-Host "Test Environment Protected Data Initialization Completed."