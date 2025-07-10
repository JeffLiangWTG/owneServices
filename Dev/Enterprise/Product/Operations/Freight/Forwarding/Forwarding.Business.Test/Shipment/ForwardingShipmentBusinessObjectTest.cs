using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipment))]
	public class ForwardingShipmentBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		//NOTE: Do NOT add new tests for ForwardingShipment here, use ForwardingShipmentTest class above. Thank you.

		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.CommonShipment);
			}
		}

		#endregion

		public override void TestCorrectlyDeleteChildrenIfSupporterInterfacesIsUsed()
		{
			// TODO: This should be undo when when Enterprise.Integration.Customs.AU.ICustomsManifestLineSequence is changed to make JobConsol it's parent; there is a requirement that a CustomsManifestLineSequence record still exists after JobShipment is deleted and this record is can be linked back JobConsol
			Assert(true);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var shipment = factory.New<ForwardingShipment>();
			shipment.FillWithValidTestData();

			shipment.JS_IsForwardRegistered = false;
			shipment.JS_IsCFSRegistered = false;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			return shipment;
		}

		#endregion
	}
}
