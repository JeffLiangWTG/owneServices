using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(JobDeclarationModule))]
	sealed class JobDeclarationModuleTest : Customs.Module.Testing.JobDeclarationModuleAbstractTest
	{
		public void TestHasExWarehouseMenu()
		{
			using (var declarationModule = new JobDeclarationModule())
			{
				AssertEquals(false, declarationModule.HasExWarehouseMenu);
			}
		}

		public void TestGetNewStandardMenuItems()
		{
			using (var module = new JobDeclarationModule())
			{
				module.CountryCode = "ZA";
				AssertNull("No ex-warehouse menu", module.GetNewStandardMenuItems().FindByText("New &Ex-warehouse"));
				AssertNull("No declaration menu", module.GetNewStandardMenuItems().FindByText("New &Declaration"));
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var declarationModule = new JobDeclarationModule())
			{
				AssertType<JobDeclarationCollection>(declarationModule.GetNewGridCollectionForTest());
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.SouthAfrica;

		protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

		protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

		protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);

		protected override BaseJobDeclaration CreateDeclarationForFetchHintTest(BusinessObjectFactory factory, string messageType, int i)
		{
			var result = (JobDeclaration)base.CreateDeclarationForFetchHintTest(factory, messageType, i);
			var instruction = result.CustomsEntryInstructions.AddNew();
			var caseNumber = instruction.CaseNumbers.AddNew();
			caseNumber.CY_Data = "123" + i;
			caseNumber.Document_Status = DocumentStatusCodes.Codes.PND;
			var cusEntryNumber = factory.New<CusEntryNumber>();
			cusEntryNumber.CE_ParentID = result.PK;
			cusEntryNumber.CE_ParentTable = JobDeclaration.Schema.TableName;
			cusEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
			cusEntryNumber.CE_EntryNum = "test" + i;
			var entryHeader = result.CustomsEntryHeaders.AddNew();
			var indicator = "";
			switch (i % 3)
			{
				case 1:
					indicator = "Y";
					break;
				case 2:
					indicator = "N";
					break;
			}

			entryHeader.CH_RelPrintInd = indicator;
			return result;
		}
	}
}
