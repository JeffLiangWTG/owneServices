using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccChequeBookModule))]
	sealed class AccChequeBookModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AccChequeBook;
		}

		protected override string CountryCode
		{
			get { return "AU"; }
		}

		public void TestCheckpoints()
		{
			using (AccChequeBookModule module = new AccChequeBookModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.ChequeBooks, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (AccChequeBookModuleForTest module = new AccChequeBookModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is AccChequeBookFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (AccChequeBookModuleForTest module = new AccChequeBookModuleForTest())
			{
				IBusinessObjectCollection chequeBooksCollection = module.NewGridCollection;
				Assert("Invalid type", chequeBooksCollection is AccChequeBookCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (AccChequeBookModuleForTest module = new AccChequeBookModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is AccChequeBookFilterBusinessObject);
			}
		}

		#endregion
	}
}
