using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.Testing;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using JobDeclaration = Enterprise.Customs.NZ.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.NZ.GUI.Declaration.ECIWriteOff.Manifesting.Testing
{
	[TestedType(typeof(EditManifestForm))]
	public class EditManifestFormTest : ZFormBasherTest
	{
		#region TestModuleIDHasntBeenTrashedByTheDesignerAgain
		public void TestModuleIDHasntBeenTrashedByTheDesignerAgain()
		{
			using (var form = new EditManifestForm(EntryHeader))
			{
				var grid = form.FindSingle<ZModuleButtonGrid>("DeclarationModuleButtonGrid");
				AssertEquals("Form.DeclarationModuleButtonGrid.ModuleID", Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.NZ.CUSCAR, grid.ModuleID);
			}
		}

		#endregion
		#region TestSubmitManifestButtonClick
		public void TestSubmitManifestButtonClick()
		{
			TestHelper.SetupMessagingEnvironment();
			using (var form = new EditManifestForm(EntryHeader))
			{
				EntryHeader.Declarations.SuspendValidation();
				AssertEquals(LowValueManifestStatusList.Codes.ManifestInError, EntryHeader.CH_EntryStatus);
				form.Show();
				form.FindSingle<ZButton>("SubmitManifestButton").PerformClick();
				AssertEquals(LowValueManifestStatusList.Codes.SentToCustoms, EntryHeader.CH_EntryStatus);
			}
		}

		public void TestSubmitManifestButtonClick_ManifestDeclarations()
		{
			TestHelper.SetupMessagingEnvironment();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			EntryHeader.CH_JE = declaration.PK;
			using (var form = new EditManifestForm(EntryHeader))
			{
				form.Show();
				AssertNoExceptionThrown(() => form.FindSingle<ZButton>("SubmitManifestButton").PerformClick());
			}
		}

		#endregion
		#region TestCancelManifestButtonClick
		public void TestCancelManifestButtonClick()
		{
			TestHelper.SetupMessagingEnvironment();
			using (var form = new EditManifestForm(EntryHeader))
			{
				EntryHeader.Declarations.SuspendValidation();
				AssertEquals(LowValueManifestStatusList.Codes.ManifestInError, EntryHeader.CH_EntryStatus);
				form.Show();
				form.FindSingle<ZButton>("CancelManifestButton").PerformClick();
				AssertEquals(LowValueManifestStatusList.Codes.SentToCustoms, EntryHeader.CH_EntryStatus);
			}
		}

		#endregion
		#region TestFormCaption
		public void TestFormCaption()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			using (EditManifestForm form = new EditManifestForm(entryHeader))
			{
				AssertEquals("ECI Write-Off Manifest", form.FormCaption);
			}

			entryHeader.CH_BGMReference = "M00001001";
			using (EditManifestForm form = new EditManifestForm(entryHeader))
			{
				AssertEquals("ECI Write-Off Manifest - M00001001", form.FormCaption);
			}
		}

		#endregion
		#region EntryHeader
		protected CusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					var refHelper = new UniversalReferenceTestDataHelper(Factory);
					refHelper.CreateNewOrGetExistingCusCodeType("NZFAV", "NZFlightsAndVessels");
					refHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.NewZealand, "NewZealand");
					refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.NewZealand, "NZFAV", "QF117", "QF117", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
					fEntryHeader = Factory.New<CusEntryHeader>();
					TestManifestCreator manifestCreator = new TestManifestCreator(fEntryHeader, "081-22222222", "QF117", "USSFO", "NZWLG", new ZDateTime(2005, 12, 2), new ZDateTime(2005, 12, 3), "01010101");
					JobDeclaration declaration1 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier1, manifestCreator.Importer1, "HOUSEBILL1", "RATS HEADS", 15.2m, 4, 45.54m);
					JobDeclaration declaration2 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier2, manifestCreator.Importer2, "HOUSEBILL2", "RATS TEETH", 13.4m, 2, 27.72m);
					fEntryHeader.EntryNumber = "12435687";
					fEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.ManifestInError;
					declaration1.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
					declaration2.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
					Factory.Save();
				}

				return fEntryHeader;
			}
		}

		CusEntryHeader fEntryHeader;
		#endregion
		#region GetFormToBashCore
		protected override Form GetFormToBashCore()
		{
			return new EditManifestForm(EntryHeader);
		}
		#endregion
	}
}
