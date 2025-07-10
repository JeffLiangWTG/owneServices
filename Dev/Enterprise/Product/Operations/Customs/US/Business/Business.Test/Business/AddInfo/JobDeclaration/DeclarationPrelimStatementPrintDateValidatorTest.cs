using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DeclarationPrelimStatementPrintDateValidatorTest : TestCaseWithFactory
	{
		[TestDate(2009, 1, 5)]
		public void TestDifferentToCalculatedDate()
		{
			var statementData = new DefaultStatementPrintDate();
			statementData.DoDefaultPrelimStatementPrintDate = true;
			statementData.NumberOfDays = 2;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, statementData);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_EntryDate = new ZDateTime(2009, 01, 17);
			string warningText = @"The Preliminary Statement Print Date differs from the calculated default value, (21-Jan-09). The Preliminary Statement Print Date default value is calculated based on the following precedence of date values, Release Date, Presentation Date or Estimated Entry Date, (if entered, in precedence), or the later of Estimated Date of Arrival (ETA) and Current Date plus the number of working days, (2 working days), based upon a System Registry setting. 
To alter the registry, (if required), please adjust registry setting: Maintain > System > Registry > Customs > United States of America > Import > ABI > Statement > Default Statement Print Date.";
			AssertNoWarningContaining(declaration.US_PreliminaryStatementPrintDateInfo, warningText);
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 1, 22);
			AssertHasWarning(declaration.US_PreliminaryStatementPrintDateInfo, warningText);
			var reconDeclaration = new ReconDeclaration(declaration);
			reconDeclaration.US_EstimatedEntryDate = new ZDateTime(2009, 1, 7);
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertEquals("PreCondition:Preliminary Statement PrintDate is defaulted", new ZDateTime(2009, 1, 7), reconDeclaration.US_PreliminaryStatementPrintDate);
			reconDeclaration.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 1, 9);
			AssertNoWarnings(reconDeclaration.US_PreliminaryStatementPrintDateInfo);
		}

		[TestDate(2009, 01, 15)]
		public void TestWarningWhenDiffersFromValueEntered()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			AssertEquals("Default registry value - Preliminary Statement Print Date should not default", ZDateTime.Empty, declaration.US_PreliminaryStatementPrintDate);
			var statementData = new DefaultStatementPrintDate();
			statementData.DoDefaultPrelimStatementPrintDate = true;
			statementData.NumberOfDays = 8;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, statementData);
			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertEquals("Default registry set - Preliminary Statement Print Date should still not default as required fields not entered", ZDateTime.Empty, declaration.US_PreliminaryStatementPrintDate);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 01, 30);
			declaration.US_EntryDate = new ZDateTime(2009, 01, 17);
			AssertEquals("Preliminary Statement Print Date should override value entered (30/01/09)", new ZDateTime(2009, 01, 29), declaration.US_PreliminaryStatementPrintDate);
			var iOR = Factory.NewWithValidTestData<OrgHeader>();
			var addInfo = new OrgImpAddInfo((ZPropertyInfoString)iOR.CountryData.OV_ImportCustomsDefaultAddInfoInfo);
			addInfo.ZO_SPDNumberOfDays = 5;
			Factory.Save();
			statementData.DoDefaultPrelimStatementPrintDate = true;
			statementData.NumberOfDays = 8;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, statementData);
			declaration.IOROrgPK = iOR.PK;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporterWithSuffixes;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 1, 22);
			AssertHasWarning(declaration.US_PreliminaryStatementPrintDateInfo, "The Preliminary Statement Print Date differs from the calculated default value, (26-Jan-09). The Preliminary Statement Print Date default value is calculated based on the following precedence of date values, Release Date, Presentation Date or Estimated Entry Date, (if entered, in precedence), or the later of Estimated Date of Arrival (ETA) and Current Date plus the number of working days, (5 working days, for this specific Importer of Record), based upon their Organization setting. \r\nTo alter this setting, (if required), adjust the 'Statement Print Date working days to be added' field on the Importer of Record details in Organization > Details > Config > US Defaults tab.\r\n\r\nTo alter the underlying system registry if necessary, please adjust the registry setting: Maintain > System > Registry > Customs > United States of America > Import > ABI > Statement > Default Statement Print Date?");
		}
	}
}
