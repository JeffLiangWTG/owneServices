using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ExportAWBSecurityStatusLine))]
	sealed class ExportAWBSecurityStatusLineBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var shipment = factory.New<ForwardingShipment>();
			shipment.FillWithValidTestData();

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_OverrideWaybillDefaults = true;

			var exportHeader = shipment.AWBHeader;
			exportHeader.ForceSavingByFactory = true;

			var result = factory.New<ExportAWBSecurityStatusLine>();
			result.FillWithValidTestData();
			result.EAS_EH = exportHeader.PK;

			return result;
		}
	}
}
