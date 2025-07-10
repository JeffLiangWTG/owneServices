using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefMessagingBussCarrierInfoForm))]
	class RefMessagingBussCarrierInfoFormTest : ZFormBasherTest
	{
		public void TestImplementation()
		{
			using (var form = new RefMessagingBussCarrierInfoFormForTest(Factory.New<RefMessagingBussCarrierInfo>()))
			{
				AssertEquals(expected: false, form.AllowNewForTest);
				AssertEquals(expected: false, form.SupportsEDocsForTest);
				AssertEquals(expected: false, form.ShowNotesTabForTest);
				AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.ReferenceFiles, form.SectionCode);
			}
		}

		public void TestControlBinding()
		{
			var refMessagingBussCarrierInfo = Factory.NewWithValidTestData<RefMessagingBussCarrierInfo>();
			using (var form = new RefMessagingBussCarrierInfoForm(refMessagingBussCarrierInfo))
			{
				form.Show();

				AssertEquals($"Carrier - {refMessagingBussCarrierInfo.ZMC_CarrierCode} - {refMessagingBussCarrierInfo.ZMC_CarrierName}", form.FormCaption);
				AssertEquals(refMessagingBussCarrierInfo.ZMC_CarrierCode, form.FindSingle<ZTextBox>("ZMC_CarrierCodeTextBox").Text);
				AssertEquals(refMessagingBussCarrierInfo.ZMC_CarrierName, form.FindSingle<ZTextBox>("ZMC_CarrierNameTextBox").Text);
				AssertEquals(refMessagingBussCarrierInfo.ZMC_CountryCode, form.FindSingle<ZTextBox>("ZMC_CountryCodeTextBox").Text);
				AssertEquals(refMessagingBussCarrierInfo.Package.ZMP_PackageName, form.FindSingle<ZTextBox>("ZMP_PackageNameTextBox").Text);
			}
		}

		protected override Form GetFormToBashCore() => new RefMessagingBussCarrierInfoForm(Factory.New<RefMessagingBussCarrierInfo>())
		{
			ControllerID = ControllerIDs.RefMessagingBussCarrierInfo
		};
	}

	class RefMessagingBussCarrierInfoFormForTest : RefMessagingBussCarrierInfoForm
	{
		public RefMessagingBussCarrierInfoFormForTest(RefMessagingBussCarrierInfo refMessagingBussCarrierInfo)
			: base(refMessagingBussCarrierInfo)
		{
		}

		public bool AllowNewForTest => base.AllowNew;

		public bool SupportsEDocsForTest => base.SupportsEDocs;

		public bool ShowNotesTabForTest => base.ShowNotesTab;
	}
}
