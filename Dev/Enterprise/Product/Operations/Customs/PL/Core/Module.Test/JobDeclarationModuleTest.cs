using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.PL.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.Customs.PL.Business.Constants;
using CusEntryHeader = Enterprise.Customs.PL.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.PL.Module.Testing;

[TestedType(typeof(JobDeclarationModuleForTest))]
sealed class JobDeclarationModuleTest : EU.Module.Testing.JobDeclarationModuleTest
{
	protected override string CountryCode => Core.Constants.CountryCodes.Poland;

	protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

	protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

	protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);

	public void TestGetNewSingleLineEntryManager_Export()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		using (var module = new JobDeclarationModuleForTest())
		{
			AssertType<SingleLineEntryManager>(module.GetNewSingleLineEntryManager_Exposed(declaration));
		}
	}

	public void TestGetNewSingleLineEntryManager_Import()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		using (var module = new JobDeclarationModuleForTest())
		{
			AssertType<ImportSingleLineEntryManager>(module.GetNewSingleLineEntryManager_Exposed(declaration));
		}
	}

	public void TestGetSingleLineEntryForm_Export()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;

		var manager = new SingleLineEntryManager(declaration);

		using (var module = new JobDeclarationModuleForTest())
		{
			using (var form = module.GetSingleLineEntryForm_Exposed(manager))
			{
				AssertType<SingleLineEntryForm>(form);
			}
		}
	}

	public void TestGetSingleLineEntryForm_Import()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		var manager = new ImportSingleLineEntryManager(declaration);

		using (var module = new JobDeclarationModuleForTest())
		{
			using (var form = module.GetSingleLineEntryForm_Exposed(manager))
			{
				AssertType<ImportSingleLineEntryForm>(form);
			}
		}
	}
	protected override BaseJobDeclaration CreateDeclarationForFetchHintTest(BusinessObjectFactory factory, string messageType, int i)
	{
		var declaration = (JobDeclaration)base.CreateDeclarationForFetchHintTest(factory, messageType, i);
		declaration.CustomsEntryInstructions[0].CEI_SubStyle = "R";
		return declaration;
	}

	protected override bool HasFailedFetchHint(TableHitCount tableSelect)
	{
		var tableNameIsCusEntryInstruction = tableSelect.TableName.Equals(Customs.Business.CusEntryInstruction.Schema.TableName);
		return (!tableNameIsCusEntryInstruction && base.HasFailedFetchHint(tableSelect)) || (tableNameIsCusEntryInstruction && tableSelect.Value > 42);
	}

	public void TestGetNewActionMenuItems()
	{
		using (var module = new JobDeclarationModuleForTest())
		{
			var item = module.GetNewActionMenuItems().FindByText("Create Supplementary Declaration");
			AssertNotNull(item);
		}
	}

	public void TestCreateSupplementaryDeclaration_Click()
	{
		using (var module = new JobDeclarationModuleForTest())
		{
			var errorMessageSubStyle = "Operation \"Create Supplementary Declaration\" is available only \r\n for Simplified Declaration (where Sub Style is one of B, C, E or F)";
			var errorMessageEntryStatus = "Status of the Simplified Declaration is not valid \r\n to create a Supplementary Declaration";
			var actionMenuItem = module.GetNewActionMenuItems().FindByText("Create Supplementary Declaration");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			module.CurrentBusinessObjectInGridEXP = declaration;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			CombineAssertions(() =>
			{
				actionMenuItem.PerformClick();
				AssertEquals("Error message when no Substyle", errorMessageSubStyle, UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.CustomsEntryInstructions.Add(Factory.New<CusEntryInstruction>());
				declaration.CustomsEntryInstructions.FirstOrDefault().CEI_SubStyle = SubStyleCodes.A;
				actionMenuItem.PerformClick();
				AssertEquals("Error message when Substyle not B,C,E,F", errorMessageSubStyle, UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.CustomsEntryInstructions.FirstOrDefault().CEI_SubStyle = SubStyleCodes.B;
				var cusEntryInstruction2 = Factory.New<CusEntryInstruction>();
				cusEntryInstruction2.CEI_SubStyle = SubStyleCodes.E;
				declaration.CustomsEntryInstructions.Add(cusEntryInstruction2);
				actionMenuItem.PerformClick();
				AssertEquals("Error message when no EntryStatus", errorMessageEntryStatus, UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.CustomsEntryHeaders.Add(Factory.New<CusEntryHeader>());
				declaration.CustomsEntryHeaders.FirstOrDefault().CH_EntryStatus = PLEntryStatusList.Codes.CAN;
				actionMenuItem.PerformClick();
				AssertEquals("Error message when EntryStatus not valid", errorMessageEntryStatus, UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.CustomsEntryHeaders.FirstOrDefault().CH_EntryStatus = PLEntryStatusList.Codes.ReleasedForExport;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				actionMenuItem.PerformClick();
				((IFilterModuleInternalsForTesting)module).LastController.LastShownForm.Dispose();
				AssertNull("No error message when Substyle and EntryStatus valid", UnitTestUserNotification.Instance.LastMessage.Text);

				var cusEntryInstruction3 = Factory.New<CusEntryInstruction>();
				cusEntryInstruction3.CEI_SubStyle = SubStyleCodes.F;
				declaration.CustomsEntryInstructions.Add(cusEntryInstruction3);
				actionMenuItem.PerformClick();
				AssertEquals("Error message when Substyle not B,E or C,F", errorMessageSubStyle, UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestSupplementaryDeclaration_Created()
	{
		foreach (var (subStyle, subStyleNew) in ((string, string)[])[
			(SubStyleCodes.B, SubStyleCodes.X),
			(SubStyleCodes.E, SubStyleCodes.X),
			(SubStyleCodes.C, SubStyleCodes.Y),
			(SubStyleCodes.F, SubStyleCodes.Y)])
		{
			TestCreateSupplementaryDeclaration(subStyle, subStyleNew);
		}
		return;

		void TestCreateSupplementaryDeclaration(string subStyle, string subStyleNew)
		{
			const string expectedMRN = "1234567";
			using (var module = new JobDeclarationModuleForTest())
			{
				var actionMenuItem = module.GetNewActionMenuItems().FindByText("Create Supplementary Declaration");
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				module.CurrentBusinessObjectInGridEXP = declaration;
				CombineAssertions(() =>
				{
					var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
					entryHeader.SetSimplifiedDeclarationMRN("12345678901234567890");
					var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
					cusEntryInstruction.CEI_SubStyle = subStyle;
					entryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;
					entryHeader.CH_EntryStatus = PLEntryStatusList.Codes.ReleasedForExport;
					entryHeader.MovementReferenceNumberSetter(expectedMRN);

					actionMenuItem.PerformClick();
					Factory.Save();
					var formToTest = ((IFilterModuleInternalsForTesting)module).LastController.LastShownForm;
					AssertType<JobDeclarationForm>(formToTest);
					var copiedDeclaration = (formToTest as JobDeclarationForm).Declaration;
					Assert($"The subStyle of copied declaration should be {subStyleNew}", copiedDeclaration.CustomsEntryInstructions.All(x => x.CEI_SubStyle == subStyleNew));
					var previousDocuments = copiedDeclaration.CustomsEntryInstructions.First().PreviousDocuments.Cast<PreviousDocument>();
					Assert("The code of previous documents under instruction of copied declaration should be NMRN", previousDocuments.All(x => x.CSI_Code == TemporaryStorageConstants.PreviousDocumentsCodeType.NMRN));
					Assert("The reference number of previous documents under instruction of copied declaration should be NMRN", previousDocuments.All(x => x.CSI_ReferenceNumber == expectedMRN));
					formToTest.Dispose();
				});
			}
		}
	}

	class JobDeclarationModuleForTest : JobDeclarationModule
	{
		public EU.Business.Declaration.SingleLineEntryManager GetNewSingleLineEntryManager_Exposed(
			EU.Business.Declaration.JobDeclaration declaration) =>
			GetNewSingleLineEntryManager(declaration);

		public EU.GUI.SingleLineEntry.SingleLineEntryForm GetSingleLineEntryForm_Exposed(
			EU.Business.Declaration.ISingleLineEntryManager manager) =>
			GetSingleLineEntryForm(manager);

		public new MenuItem[] GetNewActionMenuItems() => base.GetNewActionMenuItems();

		protected override BusinessObject CurrentBusinessObjectInGrid => CurrentBusinessObjectInGridEXP;
		public BusinessObject CurrentBusinessObjectInGridEXP;
	}
}
