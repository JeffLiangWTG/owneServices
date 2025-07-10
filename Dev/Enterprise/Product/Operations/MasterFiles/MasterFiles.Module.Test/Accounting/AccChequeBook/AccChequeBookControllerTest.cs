using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccChequeBookController))]
	sealed class AccChequeBookControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AccChequeBook;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			AccChequeBook chequeBook = Factory.New<AccChequeBook>();

			AccBankAccount bank = Factory.New<AccBankAccount>();
			bank.AB_AG = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery()).PK;
			chequeBook.AK_AB = bank.PK;
			chequeBook.AK_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;
			bank.AB_Code = "ABCBANK";
			Factory.Save();
			return chequeBook;
		}
	}
}
