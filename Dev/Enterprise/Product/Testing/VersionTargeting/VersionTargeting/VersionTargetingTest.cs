using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using CargoWise.BuildTools;
using NUnit.Framework;
using WTG.DevTools.Common.Win32;

namespace Enterprise.VersionTargeting.Testing
{
	class VersionTargettingTest : TestCase
	{
		static class CompatibilityIdentifiers
		{
			public static readonly Guid WindowsVista = new Guid("e2011457-1546-43c5-a5fe-008deee3d3f0");
			public static readonly Guid Windows7 = new Guid("35138b9a-5d96-4fbd-8e2d-a2440225f93a");
			public static readonly Guid Windows8 = new Guid("4a2f28e3-53b9-4441-ba9c-d69d4a4a6e38");
			public static readonly Guid Windows81 = new Guid("1f676c76-80e1-4239-95bb-83d0f6d0da78");
			public static readonly Guid Windows10 = new Guid("8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a");
		}

		public void TestAllApplicationsTargetWindows7()
		{
			CombineAssertions("All first-party applications should list Windows 7 in the compatibility section of their application manifest" + InstructionsOnHowToFix, () =>
			{
				TestApplicationTargetting(ApplicationsToTest, "Windows 7", CompatibilityIdentifiers.Windows7);
			});
		}

		public void TestAllApplicationsTargetWindows8()
		{
			CombineAssertions("All first-party applications should list Windows 8 in the compatibility section of their application manifest" + InstructionsOnHowToFix, () =>
			{
				TestApplicationTargetting(ApplicationsToTest, "Windows 8", CompatibilityIdentifiers.Windows8);
			});
		}

		public void TestAllApplicationsTargetWindows8_1()
		{
			CombineAssertions("All first-party applications should list Windows 8.1 in the compatibility section of their application manifest" + InstructionsOnHowToFix, () =>
			{
				TestApplicationTargetting(ApplicationsToTest, "Windows 8.1", CompatibilityIdentifiers.Windows81);
			});
		}

		public void TestAllApplicationsTargetWindows10()
		{
			CombineAssertions("All first-party applications should list Windows 10 in the compatibility section of their application manifest" + InstructionsOnHowToFix, () =>
			{
				TestApplicationTargetting(ApplicationsToTest, "Windows 10", CompatibilityIdentifiers.Windows10);
			});
		}

		public void TestWindowsVistaCompatibility()
		{
			CombineAssertions("Only Print Server should list Windows Vista in the compatibility section of it's application manifest" + InstructionsOnHowToFix, () =>
			{
				var applicationsSupportedOnVista = ApplicationsToTest.Intersect(VistaCompatibleApplications);
				var applicationsNotSupportedOnVista = ApplicationsToTest.Except(VistaCompatibleApplications);
				var applicationsSupportedOnVistaThatDoNotExist = VistaCompatibleApplications.Except(ApplicationsToTest);

				TestApplicationTargetting(applicationsSupportedOnVista, "Windows Vista", CompatibilityIdentifiers.WindowsVista);
				TestApplicationTargetting(applicationsNotSupportedOnVista, "Windows Vista", CompatibilityIdentifiers.WindowsVista, shouldTarget: false);

				var listOfNonexistentApplications = string.Join(Environment.NewLine, applicationsSupportedOnVistaThatDoNotExist);
				Assert(string.Format("All applications listed in VersionTargettingTest.VistaCompatibleApplications should exist in Build.xml. The following applications are listed but do not exist: {0}{1}", Environment.NewLine, listOfNonexistentApplications), !applicationsSupportedOnVistaThatDoNotExist.Any());
			});
		}

		static void TestApplicationTargetting(IEnumerable<string> applicationsToTest, string operatingSystemName, Guid compatibilityIdentifier, bool shouldTarget = true)
		{
			foreach (var application in applicationsToTest)
			{
				try
				{
					var manifest = ApplicationManifest.Load(application);
					AssertNotEquals(string.Format("'{0}' should have an application manifest.", application), null, manifest);

					if (shouldTarget)
					{
						Assert(string.Format("'{0}' should contain the application targeting identifier for {1}.", application, operatingSystemName), manifest.SupportedOperatingSystemIdentifiers.Contains(compatibilityIdentifier));
					}
					else
					{
						Assert(string.Format("'{0}' should not contain the application targeting identifier for {1}.", application, operatingSystemName), !manifest.SupportedOperatingSystemIdentifiers.Contains(compatibilityIdentifier));
					}
				}
				catch (Win32Exception ex)
				{
					Assert(string.Format("Unable to test '{0}', failed with {1}: {2}", application, ex.GetType().FullName, ex.Message), false);
				}
			}
		}

		IEnumerable<string> ApplicationsToTest
		{
			get
			{
				if (applicationsToTest == null)
				{
					applicationsToTest = BuildXml.Instance
						.GetAllAssembliesToBuild(deployedToClientsOnly: false) // Some binaries are not deployed, but are packed into MSIs.
						.Where(p => Path.GetExtension(p).Equals(".exe", StringComparison.OrdinalIgnoreCase))
						.Except(ExclusionsList)
						.OrderBy(x => x, StringComparer.OrdinalIgnoreCase);
				}
				return applicationsToTest;
			}
		}
		IEnumerable<string> applicationsToTest;

		static readonly string[] VistaCompatibleApplications = new string[]
		{
			// Remote Printing
			"RemotePrinting.Client.Configurator.exe",
			"RemotePrinting.Client.Desktop.exe",
			"RemotePrinting.Client.Service.exe",
			"RemotePrinting.Client.Setup.exe",
		};

		// DO NOT ADD EXCLUSIONS FOR THE SAKE OF ADDING EXCLUSIONS.
		// THE ONLY EXCLUSIONS LISTED HERE SHOULD BE APPLICATIONS THAT DO NOT RUN
		// UNDER NORMAL WINDOWS (e.g. Windows CE applications), OR THAT NEVER MAKE
		// THEIR WAY TO A TESTING OR LIVE CLIENT ENVIRONMENT.
		//
		// Please keep this list in alphabetical order (by subsection).
		static readonly string[] ExclusionsList =
		[
			// Development Tools
			"AnalyzersUnitTestGenerator.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\AnalyzersUnitTestGenerator.exe",
			"AnalyzersRunner.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\AnalyzersRunner.exe",
			"AssemblyMetaDataExtractor.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\AssemblyMetaDataExtractor.exe",
			"BulkCodeGenerator.exe",
			"CargoWise.Bi.SsisGenerator.exe",
			"CodeContractsUnitTestGenerator.exe",
			"CopyDocumentXmls.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\CopyDocumentXmls.exe",
			"MessageBuilderMappingFileGenerator.exe",
			"Enterprise.CodeAnalysis.UnitTestGenerator.exe",
			@"net8.0\Enterprise.CodeAnalysis.UnitTestGenerator.exe",
			"Enterprise.CodeSniffer.CodeSnifferUnitTestGenerator.exe",
			"Enterprise.DataTransfer.Native.Generator.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\Enterprise.DataTransfer.Native.Generator.exe",
			"Enterprise.DbUpgrader.SsisDevelopmentDeploy.exe",
			"Enterprise.MasterFiles.Services.KeysGenerator.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\Enterprise.MasterFiles.Services.KeysGenerator.exe",
			"Enterprise.PackageBuilder.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\Enterprise.PackageBuilder.exe",
			"Enterprise.Rating.Web.SelfHost.exe",
			"Enterprise.ResourceStrings.Cmd.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\Enterprise.ResourceStrings.Cmd.exe",
			"ExcelComparator.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\ExcelComparator.exe",
			"DbUsage.exe",
			"GenerateDbUpgraderResources.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\GenerateDbUpgraderResources.exe",
			"Generator.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\Generator.exe",
			"GeneratorConsistencyTestGenerator.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\GeneratorConsistencyTestGenerator.exe",
			"GlowModelTestRunner.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\GlowModelTestRunner.exe",
			"GlowUpgradeTaskRunner.exe",
			"LoadSolution.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\LoadSolution.exe",
			"MasterFiles.Business.SpecTesting.Regenerate.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\MasterFiles.Business.SpecTesting.Regenerate.exe",
			"ResourceStringsSpellCheckTestGenerator.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\ResourceStringsSpellCheckTestGenerator.exe",
			"SqlSecurity.SpecTesting.Regenerate.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\SqlSecurity.SpecTesting.Regenerate.exe",
			"TestRunner.exe",
			"VsnetUrlHandler.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\WebDeployBuilder.exe",
			"ZRSGenerator.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\ZRSGenerator.exe",

			// ILMerge Input
			"CargoWiseOne.Start.Unmerged.exe",
			"CargoWise.RemoteDesktopServices.Upgrader.Unmerged.exe",
			"CargoWiseServerSetup.Unmerged.exe",

			// One-Off Use Tools
			"cwStlClientCollector.exe",

			// Sample Clients
			"AccountingTransactionExportSampleClient.exe",
			"AccountingWebServiceSampleClient.exe",
			"InvoicePaymentSampleClient.exe",

			// Test Executables
			"Enterprise.Customs.GB.Ccsuk.TestProgramExe.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\Enterprise.Customs.GB.Ccsuk.TestProgramExe.exe",
			"Enterprise.DataTransfer.Native.TestClient.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\Enterprise.DataTransfer.Native.TestClient.exe",
			"Enterprise.RemoteDesktopServices.TestClientHost.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\Enterprise.RemoteDesktopServices.TestClientHost.exe",
			"Enterprise.ServiceManager.TestServiceProvider.exe",
			"ServiceManager.Common.Test.TestProcessAnyCpu.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\ServiceManager.Common.Test.TestProcessAnyCpu.exe",
			"MockProgram.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\MockProgram.exe",
			"MockUnsignedProgram.exe",
			$@"{CommonAssemblyInfo.CWNetCoreSubfolder}\MockUnsignedProgram.exe",

			// Windows CE
			"Enterprise.LocalTransport.Mobile.Client.exe",
			"Enterprise.Warehouse.RF.exe",
			"Enterprise.Warehouse.RF.VoicePicking.exe",
			"Enterprise.Warehouse.RF.VoicePicking.Console.exe",
			"EW.RF.CP.Load.CS.exe",
			"EW.RF.Load.CS.exe",
			"EW.RF.VP.Load.CS.exe",
			"LocalTransport.Mobile.Client.Load.CS.exe",
		];

		const string InstructionsOnHowToFix = @"

To add the default manifest for CargoWise One, go to Project -> Add Existing and select the ""main.manifest"" file in the root Enterprise (C:\dev) folder. Click the down arrow on the Add button, and select Add as link. " +
@"
Then, in the Application pane of the Project settings, enter the relative path from your project to the manifest file.

If your executable has different requirements, right click the project in the solution explorer and select 'Add -> New Item'. Then you can select 'Application Manifest File' from the list.
After the file has been created, open it up and you will see a bunch of sections like the following:

<!-- Windows 8.1 -->
<!-- <supportedOS Id=""{1f676c76-80e1-4239-95bb-83d0f6d0da78}"" /> -->

Uncomment the OSs you wish to support, or comment out the ones you do not support. Also, you must add the following line for windows 10 support if it is not already present:
<!-- Windows 10 -->
<supportedOS Id=""{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}"" />

In the (rare) case your project should be excluded from this rule (dev tools, items that will never go to a client, or things that dont use windows, you can add them to the exclusion list in VersionTargettingTest.cs
";
	}
}
