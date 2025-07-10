using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	sealed class JobMainOrgsControlTest : TestCaseWithFactory
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
				var mainOrgControl = declarationControl.JobMainOrgsControl;
				AssertEquals(mainAddress.PK, mainOrgControl.ManufacturerAddressControl.Parse("MID234323"));
			}
		}

		public void TestLabelChangeForFTZType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var declarationControl = (USJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				var addressControl = declarationControl.JobMainOrgsControl.BondedWarehouseDocAddressControl;
				AssertEquals("FTZ type only", "FTZ Operator", addressControl.Text);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Back to Original Value", "Bonded Warehouse", addressControl.Text);
				declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
				AssertEquals("FTZ type only", "FTZ Operator", addressControl.Text);
			}
		}

		public void TestShowOrHideControls()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var declarationControl = (USJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				var mainOrgControl = declarationControl.JobMainOrgsControl;
				AssertEquals(false, mainOrgControl.ImporterOfRecordOrgControl.Visible);
				AssertEquals(false, mainOrgControl.ApplicantOrgControl.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals(true, mainOrgControl.ImporterOfRecordOrgControl.Visible);
				AssertEquals(false, mainOrgControl.ApplicantOrgControl.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
				AssertEquals(false, mainOrgControl.ImporterOfRecordOrgControl.Visible);
				AssertEquals(true, mainOrgControl.ApplicantOrgControl.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				var parentTab = mainOrgControl.Parent as ZTabPage;
				AssertEquals(false, parentTab.TabVisible);
			}
		}
	}
}
