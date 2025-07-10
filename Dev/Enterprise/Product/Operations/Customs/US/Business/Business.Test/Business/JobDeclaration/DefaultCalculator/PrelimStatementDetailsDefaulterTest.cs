using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	class PrelimStatementDetailsDefaulterTest : TestCaseWithFactory
	{
		[TestDate(2008, 07, 20)]
		public void TestDefaultPreliminaryStatementDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			AssertEquals("Default registry value - Preliminary Statement Print Date should not default", ZDateTime.Empty, declaration.US_PreliminaryStatementPrintDate);

			var statementData = new DefaultStatementPrintDate();
			statementData.DoDefaultPrelimStatementPrintDate = true;
			statementData.NumberOfDays = 8;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, statementData);

			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertEquals("Default registry set - Preliminary Statement Print Date should still not default as required fields not entered", ZDateTime.Empty, declaration.US_PreliminaryStatementPrintDate);

			declaration.US_PaymentType = "";
			declaration.US_EntryDate = new ZDateTime(2008, 06, 18);
			AssertEquals("Preliminary Statement Print Date should NOT default when Entry Type is Blank", ZDateTime.Empty, declaration.US_PreliminaryStatementPrintDate);

			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			declaration.US_EntryDate = new ZDateTime(2008, 06, 18);
			AssertEquals("Preliminary Statement Print Date should NOT default when Entry Type is 1", ZDateTime.Empty, declaration.US_PreliminaryStatementPrintDate);

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_EntryDate = new ZDateTime(2008, 06, 19);
			AssertEquals("Preliminary Statement Print Date should default to 30/07/2008 - fallback to current date, plus 8 working days, as this is later than ETA", new ZDateTime(2008, 07, 30), declaration.US_PreliminaryStatementPrintDate);

			declaration.US_EntryDate = new ZDateTime(2008, 08, 15);
			AssertEquals("Preliminary Statement Print Date should default to 27/08/2008 - fallback to Date of Arrival, plus 8 working days, as ETA is later than current date", new ZDateTime(2008, 08, 27), declaration.US_PreliminaryStatementPrintDate);

			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 09, 15);
			AssertEquals("Preliminary Statement Print Date should default to 25/09/2008 - fallback to Estimated Entry Date plus 8 working days, by precendence order", new ZDateTime(2008, 09, 25), declaration.US_PreliminaryStatementPrintDate);

			declaration.US_PresentationDate = new ZDateTime(2008, 12, 01);
			AssertEquals("Preliminary Statement Print Date should default to 11/12/2008 - fallback to Presentation Date plus 8 working days, by precendence order", new ZDateTime(2008, 12, 11), declaration.US_PreliminaryStatementPrintDate);

			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertEquals("No Payment Date for Pay Type 1", ZDateTime.Empty, declaration.US_PreliminaryStatementPrintDate);
		}

		[TestDate(2009, 01, 15)]
		public void TestDefaultPreliminaryStatementDateWhenDiffersFromValueEntered()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			AssertEquals("Default registry value - Preliminary Statement Print Date should not default", ZDateTime.Empty, declaration.US_PreliminaryStatementPrintDate);

			var statementData = new DefaultStatementPrintDate();
			statementData.DoDefaultPrelimStatementPrintDate = true;
			statementData.NumberOfDays = 8;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, statementData);

			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertEquals("Default registry set - Preliminary Statement Print Date should still not default as required fields not entered", ZDateTime.Empty, declaration.US_PreliminaryStatementPrintDate);

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 01, 30);
			declaration.US_EntryDate = new ZDateTime(2009, 01, 17);
			AssertEquals("Preliminary Statement Print Date should override value entered (30/01/09)", new ZDateTime(2009, 01, 29), declaration.US_PreliminaryStatementPrintDate);
		}

		[TestDate(2009, 01, 15)]
		public void TestDoesNotDefaultPreliminaryStatementDateWhenFixed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			AssertEquals("Default registry value - Preliminary Statement Print Date should not default", ZDateTime.Empty, declaration.US_PreliminaryStatementPrintDate);

			var statementData = new DefaultStatementPrintDate();
			statementData.DoDefaultPrelimStatementPrintDate = true;
			statementData.NumberOfDays = 8;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, statementData);

			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertEquals("Default registry set - Preliminary Statement Print Date should still not default as required fields not entered", ZDateTime.Empty, declaration.US_PreliminaryStatementPrintDate);

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 01, 30);
			declaration.US_FixPSD = true;
			declaration.US_EntryDate = new ZDateTime(2009, 01, 17);
			AssertEquals("Preliminary Statement Print Date should not override value entered (30/01/09)", new ZDateTime(2009, 01, 30), declaration.US_PreliminaryStatementPrintDate);
		}
	}
}
