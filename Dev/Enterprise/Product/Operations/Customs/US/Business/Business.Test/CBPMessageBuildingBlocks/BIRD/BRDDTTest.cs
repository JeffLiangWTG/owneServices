using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.BIRD.Testing
{
	sealed class BRDDTTest : TestCaseWithFactory
	{
		public void TestProcessDates()
		{
			BRDDT dt = new BRDDT();
			dt.Date1Qualifier = BIRDDateQualifierList.Codes.ArrivalAtFirstPortUnlading;
			dt.Date1 = new ZDate(2009, 1, 1);

			dt.Date2Qualifier = BIRDDateQualifierList.Codes.ArrivalAtPortOfEntry;
			dt.Date2 = new ZDate(2009, 1, 2);

			dt.Date3Qualifier = BIRDDateQualifierList.Codes.Liquidation;
			dt.Date3 = new ZDate(2009, 1, 9);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;

			((IBIRDStatusRecord)dt).Update(declaration, new NotificationCollection());
			AssertEquals("Date at First Port updated", new ZDateTime(2009, 1, 1), declaration.JE_DateOfArrival);
			AssertEquals("Date at Entry Port updated", new ZDateTime(2009, 1, 2), declaration.US_EntryDate);

			dt = new BRDDT();
			dt.Date1Qualifier = BIRDDateQualifierList.Codes.DutyDueDate;
			dt.Date1 = new ZDate(2009, 2, 1);

			dt.Date2Qualifier = BIRDDateQualifierList.Codes.DutyPaid;
			dt.Date2 = new ZDate(2009, 2, 2);

			dt.Date3Qualifier = BIRDDateQualifierList.Codes.Statement;
			dt.Date3 = new ZDate(2009, 2, 9);

			((IBIRDStatusRecord)dt).Update(declaration, new NotificationCollection());
			AssertEquals("Duty due date updated", new ZDateTime(2009, 2, 1), declaration.US_PaymentDueDate);
			AssertEquals("payment date updated", new ZDateTime(2009, 2, 2), declaration.US_PaymentDate);
			AssertEquals("statement date updated", new ZDateTime(2009, 2, 9), declaration.US_PreliminaryStatementPrintDate);
		}
	}
}
