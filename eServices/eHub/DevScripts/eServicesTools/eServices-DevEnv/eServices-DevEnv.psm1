Using Module eServices-TFS

class GLOBALSETTINGS
{
    hidden static $Environments = @{
        "$/eServices/eHub" = @{
            "Bin" = "$/eServices/eHub/Bin";
            "SignatureKey" = "$/eServices/eHub/EDI-Release-private.snk";
            "CommonAssemblyInfo" = "$/eServices/eHub/CommonAssemblyInfo{0}.cs";
            "DeploymentDirectory" = "$/eServices/eHub/Bin/Deployment/{0}";
            "DeploymentCommonScript" = "$/eServices/eHub/Bin/Deployment/Common.ps1";
            "Packages" = "$/eServices/eHub/packages";
        };
    }

    hidden static $Deployments = @{
        "Application" = @{

        };
        "Database" = @{

        };
        "Database2010" = @{

        };
        "Database2015" = @{

        };
        "Library" = @{

        };
        "NuGet" = @{

        };
        "Registry" = @{

        };
        "WebService" = @{
            "Deploy.proj" = "DeploymentProjectPath";
            "Package.proj" = "PackageProjectPath";
            "Common.ps1" = "CommonPowershellFunctionsFilePath";
			"ConfigFile" = "Web.config";
        };
        "WebSphereMQ" = @{

        };
        "WindowsService" = @{

        };
    }

    hidden static $ProjectType = @{
        "{06A35CCD-C46D-44D5-987B-CF40FF872267}" = @{ "Description" = "Deployment Merge Module";						"DeploymentType" = ""};									
        "{14822709-B5A1-4724-98CA-57A101D1B079}" = @{ "Description" = "Workflow (C#)";									"DeploymentType" = ""};									
        "{20D4826A-C6FA-45DB-90F4-C717570B9F32}" = @{ "Description" = "Legacy (2003) Smart Device (C#)";				"DeploymentType" = ""};									
        "{2150E333-8FDC-42A3-9474-1A3956D46DE8}" = @{ "Description" = "Solution Folder";								"DeploymentType" = ""};									
        "{2DF5C3F4-5A5F-47a9-8E94-23B4456F55E2}" = @{ "Description" = "XNA (XBox)";										"DeploymentType" = ""};									
        "{32F31D43-81CC-4C15-9DE6-3FC5453562B6}" = @{ "Description" = "Workflow Foundation";							"DeploymentType" = ""};									
        "{349C5851-65DF-11DA-9384-00065B846F21}" = @{ "Description" = "Web Application (incl. MVC 5)";					"DeploymentType" = "WebService"};									
        "{3AC096D0-A1C2-E12C-1390-A8335801FDAB}" = @{ "Description" = "Test";											"DeploymentType" = ""};									
        "{3D9AD99F-2412-4246-B90B-4EAA41C64699}" = @{ "Description" = "Windows Communication Foundation (WCF)";			"DeploymentType" = ""};									
        "{3EA9E505-35AC-4774-B492-AD1749C4943A}" = @{ "Description" = "Deployment Cab";									"DeploymentType" = ""};									
        "{4D628B5B-2FBC-4AA6-8C16-197242AEB884}" = @{ "Description" = "Smart Device (C#)";								"DeploymentType" = ""};									
        "{4F174C21-8C12-11D0-8340-0000F80270F8}" = @{ "Description" = "Database (other project types)";					"DeploymentType" = ""};									
        "{54435603-DBB4-11D2-8724-00A0C9A8B90C}" = @{ "Description" = "Visual Studio 2015 Installer Project Extension";	"DeploymentType" = ""};									
        "{593B0543-81F6-4436-BA1E-4747859CAAE2}" = @{ "Description" = "SharePoint (C#)";								"DeploymentType" = ""};									
        "{603C0E0B-DB56-11DC-BE95-000D561079B0}" = @{ "Description" = "ASP.NET MVC 1.0";								"DeploymentType" = "WebService"};									
        "{60DC8134-EBA5-43B8-BCC9-BB4BC16C2548}" = @{ "Description" = "Windows Presentation Foundation (WPF)";			"DeploymentType" = ""};									
        "{68B1623D-7FB9-47D8-8664-7ECEA3297D4F}" = @{ "Description" = "Smart Device (VB.NET)";							"DeploymentType" = ""};									
        "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}" = @{ "Description" = "Project Folders";								"DeploymentType" = ""};									
        "{6BC8ED88-2882-458C-8E55-DFD12B67127B}" = @{ "Description" = "MonoTouch";										"DeploymentType" = ""};									
        "{6D335F3A-9D43-41b4-9D22-F6F17C4BE596}" = @{ "Description" = "XNA (Windows)";									"DeploymentType" = ""};									
        "{76F1466A-8B6D-4E39-A767-685A06062A39}" = @{ "Description" = "Windows Phone 8/8.1 Blank/Hub/Webview App";		"DeploymentType" = ""};									
        "{786C830F-07A1-408B-BD7F-6EE04809D6DB}" = @{ "Description" = "Portable Class Library";							"DeploymentType" = ""};									
        "{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}" = @{ "Description" = "ASP.NET 5";										"DeploymentType" = "WebService"};									
        "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}" = @{ "Description" = "C++";											"DeploymentType" = ""};									
        "{978C614F-708E-4E1A-B201-565925725DBA}" = @{ "Description" = "Deployment Setup";								"DeploymentType" = ""};									
        "{A1591282-1198-4647-A2B1-27E5FF5F6F3B}" = @{ "Description" = "Silverlight";									"DeploymentType" = ""};									
        "{A5A43C5B-DE2A-4C0C-9213-0A381AF9435A}" = @{ "Description" = "Universal Windows Class Library";				"DeploymentType" = ""};									
        "{A860303F-1F3F-4691-B57E-529FC101A107}" = @{ "Description" = "Visual Studio Tools for Applications (VSTA)";	"DeploymentType" = ""};									
        "{A9ACE9BB-CECE-4E62-9AA4-C7E7C5BD2124}" = @{ "Description" = "Database";										"DeploymentType" = ""};									
        "{AB322303-2255-48EF-A496-5904EB18DA55}" = @{ "Description" = "Deployment Smart Device Cab";					"DeploymentType" = ""};									
        "{B69E3092-B931-443C-ABE7-7E7B65F2A37F}" = @{ "Description" = "Micro Frmework";									"DeploymentType" = ""};									
        "{BAA0C2D2-18E2-41B9-852F-F413020CAA33}" = @{ "Description" = "Visual Studio Tools for Office (VSTO)";			"DeploymentType" = ""};									
        "{BC8A1FFA-BEE3-4634-8014-F334798102B3}" = @{ "Description" = "Windows Store Apps (Metro Apps)";				"DeploymentType" = ""};									
        "{BF6F8E12-879D-49E7-ADF0-5503146B24B8}" = @{ "Description" = "C# in Dynamics 2012 AX AOT";						"DeploymentType" = ""};									
        "{C089C8C0-30E0-4E22-80C0-CE093F111A43}" = @{ "Description" = "Windows Phone 8/8.1 App (C#)";					"DeploymentType" = ""};									
        "{C252FEB5-A946-4202-B1D4-9916A0590387}" = @{ "Description" = "Visual Database Tools";							"DeploymentType" = ""};									
        "{CB4CE8C6-1BDB-4DC7-A4D3-65A1999772F8}" = @{ "Description" = "Legacy (2003) Smart Device (VB.NET)";			"DeploymentType" = ""};									
        "{D399B71A-8929-442a-A9AC-8BEC78BB2433}" = @{ "Description" = "XNA (Zune)";										"DeploymentType" = ""};									
        "{D59BE175-2ED0-4C54-BE3D-CDAA9F3214C8}" = @{ "Description" = "Workflow (VB.NET)";								"DeploymentType" = ""};									
        "{DB03555F-0C8B-43BE-9FF9-57896B3C5E56}" = @{ "Description" = "Windows Phone 8/8.1 App (VB.NET)";				"DeploymentType" = ""};									
        "{E24C65DC-7377-472B-9ABA-BC803B73C61A}" = @{ "Description" = "Web Site";										"DeploymentType" = ""};									
        "{E3E379DF-F4C6-4180-9B81-6769533ABE47}" = @{ "Description" = "ASP.NET MVC 4.0";								"DeploymentType" = "WebService"};									
        "{E53F8FEA-EAE0-44A6-8774-FFD645390401}" = @{ "Description" = "ASP.NET MVC 3.0";								"DeploymentType" = "WebService"};									
        "{E6FDF86B-F3D1-11D4-8576-0002A516ECE8}" = @{ "Description" = "J#";												"DeploymentType" = ""};									
        "{EC05E597-79D4-47f3-ADA0-324C4F7C7484}" = @{ "Description" = "SharePoint (VB.NET)";							"DeploymentType" = ""};									
        "{EFBA0AD7-5A72-4C68-AF49-83D382785DCF}" = @{ "Description" = "Xamarin.Android / Mono for Android";				"DeploymentType" = ""};									
        "{F135691A-BF7E-435D-8960-F99683D2D49C}" = @{ "Description" = "Distributed System";								"DeploymentType" = ""};									
        "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}" = @{ "Description" = "VB.NET";											"DeploymentType" = ""};									
        "{F2A71F9B-5D33-465A-A702-920D77279786}" = @{ "Description" = "F#";												"DeploymentType" = ""};									
        "{F5B4F3BC-B597-4E2B-B552-EF5D8A32436F}" = @{ "Description" = "MonoTouch Binding";								"DeploymentType" = ""};									
        "{F85E285D-A4E0-4152-9332-AB1D724D3325}" = @{ "Description" = "ASP.NET MVC 2.0";								"DeploymentType" = "WebService"};									
        "{F8810EC1-6754-47FC-A15F-DFABD2E3FA90}" = @{ "Description" = "SharePoint Workflow";							"DeploymentType" = ""};									
        "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}" = @{ "Description" = "C#";												"DeploymentType" = ""};	
    }

    hidden static $Packages = @{
        "Common.Logging" = @{
            "Include" = "Common.Logging, Version={0}, Culture=neutral, PublicKeyToken=af08829b84f0328e, processorArchitecture=MSIL";
            "HintPath" = "Common.Logging\lib\net{0}\Common.Logging.dll";
        };
        "Common.Logging.Core" = @{
            "Include" = "Common.Logging.Core, Version={0}, Culture=neutral, PublicKeyToken=af08829b84f0328e, processorArchitecture=MSIL";
            "HintPath" = "Common.Logging.Core\lib\net{0}\Common.Logging.Core.dll";
        };
        "Common.Logging.Log4Net1211" = @{
            "Include" = "Common.Logging.Log4Net1211, Version={0}, Culture=neutral, PublicKeyToken=af08829b84f0328e, processorArchitecture=MSIL";
            "HintPath" = "Common.Logging.Log4Net1211\lib\net{0}\Common.Logging.Log4Net1211.dll";
        };
        "log4net" = @{
            "Include" = "log4net, Version={0}, Culture=neutral, PublicKeyToken=669e0ddf0bb1aa2a, processorArchitecture=MSIL";
            "HintPath" = "log4net\lib\net{0}-full\log4net.dll";
        };
    }

    static [string] GetEnvironmentVariable([TFS] $tfs, [string] $branch, [string] $variableName, [Array] $injections)
    {
        return $tfs.GetLocalItem([GLOBALSETTINGS]::Environments[$branch][$variableName]) -f $injections;
    }

    static [string] GetDeploymentVariable([string] $deploymentType, [string] $variableName)
    {
        return [GLOBALSETTINGS]::Deployments[$deploymentType][$variableName]
    }

    static [PSCustomObject] GetProjectType([string] $GUID)
    {
        return [PSCustomObject]([GLOBALSETTINGS]::ProjectType[$GUID])
    }

    static [PSCustomObject] GetPackage([string] $packageName)
    {
        return [PSCustomObject]([GLOBALSETTINGS]::Packages[$packageName])
    }
}