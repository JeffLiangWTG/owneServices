using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using NUnit.Framework;
using TransportLeg = Enterprise.UniversalDataBuss.DataObjects.Universal.TransportLeg;

namespace Enterprise.Freight.Forwarding.Documents.Testing.Shared
{
	[TestedType(typeof(Vessel))]
	sealed class VesselTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCreateFromUniversalTransportLeg()
		{
			var transportLeg = new TransportLeg()
			{
				VesselName = "Ye Olde Vessel",
				VesselLloydsIMO = "69420"
			};

			var vessel = Vessel.Create(Context, transportLeg);
			AssertEquals(transportLeg.VesselName, vessel.Name);
			AssertEquals(transportLeg.VesselLloydsIMO, vessel.LloydsIMO);
			AssertNotNull(vessel.Type.Codes);
		}

		#region Implementation

		IContext Context => context ?? (context = new CommonContext(Factory.GetCachedReadOnlyFactory()));
		IContext context;

		protected override BusinessObject GetNewBusinessObject()
		{
			return Vessel.Create(new CommonContext(Factory), (Freight.Business.Transport)null);
		}

		#endregion
	}
}
