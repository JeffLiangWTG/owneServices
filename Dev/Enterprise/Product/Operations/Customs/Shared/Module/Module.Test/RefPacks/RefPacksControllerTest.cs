using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Shared.Module.Testing
{
	[TestedType(typeof(RefPacksController))]
	sealed class RefPacksControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.RefPacks;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var refPacks = Factory.NewWithValidTestData<CusRefPacks>();
			refPacks.RP_Type = RPTypeList.Codes.CommercialInvoice;
			Factory.Save();
			return refPacks;
		}
	}
}
