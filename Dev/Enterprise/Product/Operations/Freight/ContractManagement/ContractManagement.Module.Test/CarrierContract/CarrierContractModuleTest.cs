using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Module.Testing
{
	[TestedType(typeof(CarrierContractModule))]
	class CarrierContractModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CarrierContracts;
		}

		public override void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			Assert("No controller available for this module", true);
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var carrier = collection.Factory.NewWithValidTestData<OrgHeader>();

			var contract = collection.AddNew() as RatingContract;
			contract.RCT_ContractNumber = "NUMBER";
			contract.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract.RCT_StartDate = ZDate.Today;
			contract.RCT_EndDate = ZDate.Today.AddDays(30);
			contract.RCT_OH = carrier.PK;
			contract.RCT_GS_NKContractOwner = "USR";

			collection.Factory.Save();

			base.AddTestObjects(collection);
		}
	}
}
