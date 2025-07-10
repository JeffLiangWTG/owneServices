using System.Drawing.Imaging;
using System.Windows.Forms;
using CargoWise.BrandManager;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using WTG.ROPE.Model;
using static Enterprise.MasterFiles.GUI.Test.ImageBitmapTestHelper;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(ConfirmOrganizationForm))]
	public class ConfirmOrganizationFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var companyLookupModel = new CompanyLookupModel(new[]
			{
				new ResponseCompanyItem { Name = "TestCompany", Address = "TEST ADDRESS 1", City = "SAA1", PostCode = "1231234", StateCode = "AAA", Country = "CCC", Score = 50, Registered = true, Identifiers = new Identifier[] { new Identifier() { ID = "12345689", Type = IdentifierType.DUNS } } },
				new ResponseCompanyItem { Name = "TestCompany", Address = "TEST ADDRESS 2", City = "SAA2", PostCode = "123123444", StateCode = "AAAb", Country = "CCCd", Score = 90, Registered = false, Identifiers = new Identifier[] { new Identifier() { ID = "56789", Type = IdentifierType.DUNS } } },
				new ResponseCompanyItem { Name = "TestCompany", Address = "TEST ADDRESS 3", City = "SAA3", PostCode = "1231235555", StateCode = "AAAc", Country = "CCCdd", Score = 50, Registered = true, Identifiers = new Identifier[] { new Identifier() { ID = "23459", Type = IdentifierType.DUNS } } }
			});

			return new ConfirmOrganizationForm(companyLookupModel);
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "cityStatePostInfoRadioButton";
		}

		public void TestClose_ButtonClick()
		{
			var companyLookupModel = new CompanyLookupModel(new[] { new ResponseCompanyItem { Name = "TestCompany", Address = "TEST ADDRESS 1", City = "SAA1", PostCode = "1231234", StateCode = "AAA", Country = "CCC", Score = 50, Registered = true, Identifiers = new Identifier[] { new Identifier() { ID = "12345689", Type = IdentifierType.DUNS } } } });

			using (var form = new ConfirmOrganizationForm(companyLookupModel))
			{
				var isClosed = false;
				form.Closed += (sender, args) =>
				{
					isClosed = true;
				};

				form.Show();
				var closeButton = form.Controls.Find("cancelButton", true)[0] as ZButton;
				closeButton.PerformClick();
				Assert(isClosed);
			}
		}

		public void TestIconAndText()
		{
			var companyLookupModel = new CompanyLookupModel(new[] { new ResponseCompanyItem { Name = "TestCompany", Address = "TEST ADDRESS 1", City = "SAA1", PostCode = "1231234", StateCode = "AAA", Country = "CCC", Score = 50, Registered = true, Identifiers = new Identifier[] { new Identifier() { ID = "12345689", Type = IdentifierType.DUNS } } } });

			using (var form = new ConfirmOrganizationForm(companyLookupModel))
			{
				AssertEquals("Confirm the organization", form.Text);

				var bitsExpected = ConvertBitMapToByteArray(BrandingFactory.Instance.ProductIcon.ToBitmap(), ImageFormat.Bmp);
				var bitsActual = ConvertBitMapToByteArray(form.Icon.ToBitmap(), ImageFormat.Bmp);
				AssertArrayEqualsByElements(bitsExpected, bitsActual);
			}
		}

		[RequiresSTA]
		public void TestConfirmButton_Click()
		{
			var companyLookupModel = new CompanyLookupModel(new[] { new ResponseCompanyItem { Name = "TestCompany", Address = "TEST ADDRESS 1", City = "SAA1", PostCode = "1231234", StateCode = "AAA", Country = "CCC", Score = 50, Registered = true, Identifiers = new Identifier[] { new Identifier() { ID = "12345689", Type = IdentifierType.DUNS } } } });

			using (var form = new ConfirmOrganizationForm(companyLookupModel))
			{
				var isClosed = false;
				form.Closed += (sender, args) =>
				{
					isClosed = true;
				};
				Assert(!form.NeedToGetReport);
				form.Show();

				var confirmButton = form.Controls.Find("confirmButton", true)[0] as ZButton;
				confirmButton.PerformClick();
				Assert(isClosed);
				Assert(form.NeedToGetReport);
			}
		}
	}
}
