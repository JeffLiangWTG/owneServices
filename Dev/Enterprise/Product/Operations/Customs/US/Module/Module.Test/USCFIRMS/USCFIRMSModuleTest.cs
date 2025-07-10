using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCFIRMSModule))]
	sealed class USCFIRMSModuleTest : USCFilterGridModuleTest
	{
		public void TestCopyFilterGridHyperlinkToClipboardMenuItemIsAbsent()
		{
			using (var fIRMmodule = new USCFIRMSModule())
			{
				AssertNull(fIRMmodule.FormActionMenu.FindByText("Copy Hyperlinks to Clipboard", true));
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.FIRMS;

		protected override LicenceCheckpoint ExpectedLicenceCheckpoint => Env.Licence.Broker;
	}
}
