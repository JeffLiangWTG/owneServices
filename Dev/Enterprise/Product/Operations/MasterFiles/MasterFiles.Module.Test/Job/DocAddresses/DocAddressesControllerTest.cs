using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(DocAddressesController))]
	sealed class DocAddressesControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.DocAddresses;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			BusinessObject shipmentAsBizO = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			JobDocAddressDependentCollection addresses = (JobDocAddressDependentCollection)shipmentAsBizO["DocAddresses"];
			JobDocAddress docAddress = addresses.AddNew();
			Factory.Save();

			return docAddress;
		}
	}
}
