using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccBankAccountController))]
	sealed class AccBankAccountControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AccBankAccount;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			AccBankAccount bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_AG = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery()).PK;
			bankAccount.AB_Code = "ABCBANK";
			Factory.Save();
			return bankAccount;
		}
	}
}
