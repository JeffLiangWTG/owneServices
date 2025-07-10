using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChequeBookAutoAllocationValidationTest : TestCaseWithFactory
	{
		public void TestGetErrorsForChequeBook()
		{
			AccChequeBook chequeBook1 = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook1.AK_IsActive = ZBool.False;
			ZString result = ValidationObj.GetErrorsForChequeBook(chequeBook1, ZBool.False);
			AssertEquals(AccChequeBookAutoAllocationValidation.ChequeBookIsInActiveMessage, result);
			chequeBook1.AK_IsActive = ZBool.True;
			result = ValidationObj.GetErrorsForChequeBook(chequeBook1, ZBool.False);
			Assert("Result should be empty", result.IsEmpty);

			chequeBook1.AK_StartNo = 1;
			chequeBook1.AK_LastNo = 6;
			chequeBook1.AK_CurrentNo = 7;
			result = ValidationObj.GetErrorsForChequeBook(chequeBook1, ZBool.True);
			AssertEquals(AccChequeBookAutoAllocationValidation.ChequeBookIsFullMessage, result);
			chequeBook1.AK_CurrentNo = 6;

			result = ValidationObj.GetErrorsForChequeBook(chequeBook1, ZBool.True);
			AssertEquals(AccChequeBookAutoAllocationValidation.ChequeBookWithoutAPrinterMessage, result);
			chequeBook1.AK_SQ = ZGuid.Invalid;
			result = ValidationObj.GetErrorsForChequeBook(chequeBook1, ZBool.True);
			AssertEquals(AccChequeBookAutoAllocationValidation.ChequeBookWithoutAPrinterMessage, result);
			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue)));
			chequeBook1.AK_SQ = printQueue.PK;
			result = ValidationObj.GetErrorsForChequeBook(chequeBook1, ZBool.False);
			Assert("Result should be empty", result.IsEmpty);
		}

		#region Implementation

		AccChequeBookAutoAllocationValidation ValidationObj;

		protected override void SetUp()
		{
			base.SetUp();
			ValidationObj = new AccChequeBookAutoAllocationValidation();
		}

		#endregion
	}
}
