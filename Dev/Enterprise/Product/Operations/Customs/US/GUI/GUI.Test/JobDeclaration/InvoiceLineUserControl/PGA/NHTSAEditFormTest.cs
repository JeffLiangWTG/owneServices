using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(NHTSAEditForm))]
	sealed class NHTSAEditFormTest : ZFormBasherTest
	{
		public void TestControlsVisibilityForProduct()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = ZGuid.NewZGuid().ToGuid().ToString("N");
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var header = pivot.NHTSALines.AddNew();
			header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.REI;
			using (var form = new NHTSAEditForm(header))
			{
				form.Show();
				AssertEquals(false, form.ElecImageSubmittedCheckBox.Visible);
				AssertEquals(false, form.TravelDocumentGroupBox.Visible);
				AssertEquals(false, form.PGAContantNameTextBox.Visible);
				AssertEquals(false, form.PGAContactPhoneTextBox.Visible);
				AssertEquals(false, form.PGAContactEmailTextBox.Visible);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<NHTSAHeader>();
			header.B7_ParentID = ZGuid.NewZGuid();
			header.B7_ParentTableCode = "CI";
			Factory.Save();
			return new NHTSAEditForm(header);
		}
	}
}
