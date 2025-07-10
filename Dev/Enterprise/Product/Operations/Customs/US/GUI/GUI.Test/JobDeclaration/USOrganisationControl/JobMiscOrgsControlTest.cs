using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.GUI
{
	sealed class JobMiscOrgsControlTest : TestCaseWithFactory
	{
		public void TestMIDParse()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TEST ORG DUMMY";
			org.OH_Code = "ORG" + new System.Random().Next(1000000).ToString();
			var mainAddress = org.MainAddress;
			mainAddress.OA_Address1 = "ADDRESS 1";
			mainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "MID234323");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var declarationControl = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				var miscOrgControl = declarationControl.JobMiscOrgsControl;
				AssertEquals(mainAddress.PK, miscOrgControl.InvoicerAddressControl.Parse("MID234323"));
			}
		}

		public void TestOrganizationVisibilityBetweenACSAndACE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var declarationControl = (USJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				var misOrgControl = declarationControl.JobMiscOrgsControl;
				AssertEquals("Invoicer is visible for ACS", true, misOrgControl.InvoicerAddressControl.Visible);
				AssertEquals("Selling Agent is visible for ACS", true, misOrgControl.SellingAgentOrgControl.Visible);
				AssertEquals("Buyer Agent is visible for ACS", true, misOrgControl.BuyerAgentOrgControl.Visible);
				AssertEquals("Shipper is NOT visible for ACS", false, misOrgControl.ShipperAddressControl.Visible);
				AssertEquals("Distributor is NOT visible for ACS", false, misOrgControl.DistributorAddressControl.Visible);
				AssertEquals("Packager is NOT visible for ACS", false, misOrgControl.PackagerAddressControl.Visible);
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				AssertEquals("Invoicer is NOT visible for ACE", false, misOrgControl.InvoicerAddressControl.Visible);
				AssertEquals("Selling Agent is NOT visible for ACE", false, misOrgControl.SellingAgentOrgControl.Visible);
				AssertEquals("Buyer Agent is NOT visible for ACE", false, misOrgControl.BuyerAgentOrgControl.Visible);
				AssertEquals("Shipper is visible for ACE", true, misOrgControl.ShipperAddressControl.Visible);
				AssertEquals("Distributor is visible for ACE", true, misOrgControl.DistributorAddressControl.Visible);
				AssertEquals("Packager is visible for ACE", true, misOrgControl.PackagerAddressControl.Visible);
			}
		}
	}
}
