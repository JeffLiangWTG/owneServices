using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	class DeclarationPrelimStatementDetailsDefaulterTest : TestCaseWithFactory
	{
		[TestDate(2008, 07, 20)]
		public void TestDefaultingOfPeriodicStatementMonth()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertEquals("Default registry value - Periodic Statement MM should not default", "", declaration.US_PeriodicStatementMM);

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			AssertEquals("Default registry value - Periodic Statement MM should not default", "", declaration.US_PeriodicStatementMM);

			var statementData = new DefaultStatementPrintDate();
			statementData.DoDefaultPrelimStatementPrintDate = true;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, statementData);

			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertEquals("Default registry set - Periodic Statement MM should still not default for this payment type", "", declaration.US_PeriodicStatementMM);

			declaration.US_FixPSD = true;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
			AssertEquals("Default registry set - Periodic Statement MM should still not default when Fix PSD is ticked", "", declaration.US_PeriodicStatementMM);

			declaration.US_FixPSD = false;
			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
			AssertEquals("Periodic Statement MM should default to 08 - fallback to current date plus 1 month", "08", declaration.US_PeriodicStatementMM);

			declaration.US_EntryDate = new ZDateTime(2008, 06, 19);
			AssertEquals("Periodic Statement MM should still default to 08 - fallback to current date, plus 1 month as this is later than ETA, ", "08", declaration.US_PeriodicStatementMM);

			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, statementData);

			declaration.US_EntryDate = new ZDateTime(2008, 08, 15);
			AssertEquals("Periodic Statement MM should default to 09 - fallback to Date of Arrival, plus 1 month, as ETA is later than current date", "09", declaration.US_PeriodicStatementMM);

			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 09, 15);
			AssertEquals("Periodic Statement MM should default to 10 - fallback to Estimated Entry Date plus 1 month, by precendence order", "10", declaration.US_PeriodicStatementMM);

			declaration.US_PresentationDate = new ZDateTime(2008, 12, 15);
			AssertEquals("Periodic Statement MM should default to 01 - fallback to Presentation Date plus 1 month, by precendence order", "01", declaration.US_PeriodicStatementMM);

			declaration.US_PeriodicStatementMM = "02";
			declaration.US_EntryDate = new ZDateTime(2008, 12, 13);
			AssertEquals("Periodic Statement MM should be overriden", "01", declaration.US_PeriodicStatementMM);

			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertEquals("Periodic Statement MM is not required", "", declaration.US_PeriodicStatementMM);

			declaration.US_PeriodicStatementMM = "02";
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporterWithSuffixes;
			AssertEquals("Periodic Statement MM is not required", "", declaration.US_PeriodicStatementMM);

			declaration.US_PeriodicStatementMM = "02";
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			AssertEquals("Periodic Statement MM is not required", "", declaration.US_PeriodicStatementMM);

			declaration.US_PeriodicStatementMM = "02";
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertEquals("Periodic Statement MM is not required", "", declaration.US_PeriodicStatementMM);
		}
	}
}
