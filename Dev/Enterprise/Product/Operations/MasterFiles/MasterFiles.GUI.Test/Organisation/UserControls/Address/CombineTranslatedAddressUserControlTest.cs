using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class CombineTranslatedAddressUserControlTest : TestCaseWithFactory
	{
		OrgAddress GetNewAddressForTesting(OrgHeader org)
		{
			var address = org.Addresses.AddNew();
			address.OA_Code = Guid.NewGuid().ToString().Substring(0, 25);
			return address;
		}

		public void TestAddressFormFieldsMatchCurrentEntity()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			var testAddress = GetNewAddressForTesting(testHeader);
			testAddress.OA_Address1 = "test1";
			testAddress.OA_Address1 = "test2";
			testAddress.OA_City = "test3";
			testAddress.OA_State = "RRR";
			testAddress.OA_PostCode = "1234";
			Application.DoEvents();
			using (var form = new ZForm())
			using (var testControl = new TestCombineTranslatedAddressesControl())
			{
				form.Controls.Add(testControl);
				testControl.SetDataBinding(testAddress, "");
				form.Show();

				Assert(string.Equals(testAddress.Address1, testControl.OA_Address1BoundTextBox_Exposed.Text, StringComparison.OrdinalIgnoreCase));
				Assert(string.Equals(testAddress.Address2, testControl.OA_Address2BoundTextBox_Exposed.Text, StringComparison.OrdinalIgnoreCase));
				Assert(string.Equals(testAddress.City, testControl.OA_CityBoundTextBox_Exposed.Text, StringComparison.OrdinalIgnoreCase));
				Assert(string.Equals(testAddress.State, testControl.OA_StateBoundDropEdit_Exposed.Text, StringComparison.OrdinalIgnoreCase));
				Assert(string.Equals(testAddress.Postcode, testControl.OA_PostCodeBoundTextBox_Exposed.Text, StringComparison.OrdinalIgnoreCase));
			}
		}

		public void TestFormIsReadOnlyOnLoadWhenPropertyIsSet()
		{
			Env.Security.OrgAddressModify.IsAllowed = true;
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			var testAddress = GetNewAddressForTesting(testHeader);
			testAddress.SetReadOnlyIncludingChildren(true);

			using (var form = new ZForm())
			using (var docAddress = new TestCombineTranslatedAddressesControl())
			{
				docAddress.SetDataBinding(testAddress, "");
				form.Controls.Add(docAddress);
				form.Show();
				Application.DoEvents();
				Assert(docAddress.OA_Address1BoundTextBox_Exposed.ReadOnly);
				Assert(docAddress.OA_Address2BoundTextBox_Exposed.ReadOnly);
				Assert(docAddress.OA_CityBoundTextBox_Exposed.ReadOnly);
				Assert(docAddress.OA_StateBoundDropEdit_Exposed.ReadOnly);
				Assert(docAddress.OA_PostCodeBoundTextBox_Exposed.ReadOnly);
			}
		}

		public void TestAddressValidationisHiddenWhenPropertyIsSet()
		{
			Env.Security.OrgAddressModify.IsAllowed = true;
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			var testAddress = GetNewAddressForTesting(testHeader);

			using (var form = new ZForm())
			using (var docAddress = new TestCombineTranslatedAddressesControl())
			{
				docAddress.SetDataBinding(testAddress, "");
				docAddress.ValidateAddressButtonVisibility = false;
				form.Controls.Add(docAddress);
				form.Show();
				Application.DoEvents();
				Assert(!docAddress.ValidateButton.Visible);
			}
		}
	}
}
