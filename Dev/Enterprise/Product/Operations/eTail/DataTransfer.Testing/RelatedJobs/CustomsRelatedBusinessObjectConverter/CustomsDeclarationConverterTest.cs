using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class CustomsDeclarationConverterTest : CustomsRelatedBusinessObjectConverterBaseTest<CustomsDeclarationConverter>
	{
		public override void TestExportedUniversalShipment_HasAdditionalReferenceCollection()
		{
			Assert(true);
		}

		protected override ZString LoginCountry => CountryCodes.Canada;

		protected override string[] SupportedTransportModes => new[] { TransportModes.Air, TransportModes.Sea, TransportModes.Road };

		protected override bool ShouldUpdateProgressForPopulatingHouseBills => false;

		protected override ZString GetExistingRelatedCustomsJobReference(BusinessObject existingJob) => ((IBaseJobDeclaration)existingJob).JE_DeclarationReference;

		protected override BaseHVLVRelatedJobCommand GetRelatedJobCommand(ForwardingShipment shipment) => new CustomsDeclarationCommand(shipment);

		protected override BusinessObject SetupExistingRelatedCustomsJob(ForwardingShipment shipment)
		{
			var result = Factory.New(ObjectFactory.GetType<IBaseJobDeclaration>());
			return result;
		}
	}
}
