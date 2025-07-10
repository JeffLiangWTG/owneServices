using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business.Testing
{
	public class ElectronicInvoiceStatusCalculatorTest : TestCaseWithFactory
	{
		public void TestElectronicInvoiceStatus()
		{
			AssertEquals(ZString.Empty, Calculator.ElectronicInvoiceStatus);

			JobComInvoiceHeader invoice1 = Declaration.Invoices.AddNew();
			JobComInvoiceHeader invoice2 = Declaration.Invoices.AddNew();
			invoice1.JZ_MessageStatus = MessageStatus1ForTest;
			invoice2.JZ_MessageStatus = MessageStatus1ForTest;
			AssertEquals(MessageStatus1ForTest, Calculator.ElectronicInvoiceStatus);

			invoice2.JZ_MessageStatus = MessageStatus2ForTest;
			AssertEquals(MessageStatusListEI.Codes.Multiple, Calculator.ElectronicInvoiceStatus);

			invoice1.JZ_MessageStatus = MessageStatus2ForTest;
			AssertEquals(MessageStatus2ForTest, Calculator.ElectronicInvoiceStatus);
		}
		const string MessageStatus1ForTest = MessageStatusListEI.Codes.AwaitingElectronicInvoiceOriginal;
		const string MessageStatus2ForTest = MessageStatusListEI.Codes.ErrorElectronicInvoiceOriginal;

		#region implementation

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		ElectronicInvoiceStatusCalculator Calculator
		{
			get
			{
				if (electronicInvoiceStatusCalculator == null)
				{
					electronicInvoiceStatusCalculator = new ElectronicInvoiceStatusCalculator(Declaration);
				}
				return electronicInvoiceStatusCalculator;
			}
		}
		ElectronicInvoiceStatusCalculator electronicInvoiceStatusCalculator;

		#endregion
	}
}
