using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(OrgSupplierPartForm))]
	public sealed class OrgSupplierPartFormTest : ZFormBasherTest
	{
		public void TestOP_DescCaption()
		{
			var part = Factory.New<OrgSupplierPart>();
			using (var form = new OrgSupplierPartForm(part))
			{
				form.Show();
				AssertEquals("English Desc.", form.OP_DescBoundTextBox.GetExtension<LabelCaptionRenderer>().Caption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			return new OrgSupplierPartForm(part);
		}
	}
}
