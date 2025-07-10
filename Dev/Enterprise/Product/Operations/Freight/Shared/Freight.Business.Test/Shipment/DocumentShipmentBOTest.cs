using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(DocumentShipment))]
	sealed class DocumentShipmentBOTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "08112345678";

			Transport transport = consol.Transports[0];
			transport.JW_IsLinked = true;
			transport.JW_RL_NKLoadPort = "AUSYD";

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 7;

			return new DocumentShipment(shipment, Constants.DataContext.FreightLabels);
		}

		#endregion
	}
}
