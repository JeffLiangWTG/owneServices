using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	internal class AgencyBillContainerEDocsViaUniversalXmlSupport : IEDocsViaUniversalXmlSupport
	{
		public BusinessObject LoadBusinessObjectFromCode(BusinessObjectFactory factory, ZString code)
		{
			var query = new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, code)
				.AddToFilter(JobShipmentSchema.JS_IsForwardRegistered, false)
				.AddToFilter(JobShipmentSchema.JS_IsShipping, true);
			return factory.LoadTop1<AgencyShipment>(query);
		}

		public ZString ExpectedCodeFormat { get; } = "JSUniqueConsignRef ";
		public ZString ExampleCodeFormat { get; } = "V99999999";
	}
}
