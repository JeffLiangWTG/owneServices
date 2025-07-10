using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ZoneSelection))]
	sealed class ZoneSelectionTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ZoneSelection(new DummyZoneSelectionParent(Factory), "TransportZonePK", "InternationalZonePK");
		}

		public void TestZones()
		{
			var carrier = Factory.New<OrgHeader>();
			var zoneOwner = Factory.New<OrgHeader>();
			var transportProvider = (BusinessObject)Factory.New<IRateTransportProvider>();
			transportProvider[RateTransportProviderSchema.TP_RN_NKCountry] = "NZ";
			transportProvider[RateTransportProviderSchema.TP_OH_RelatedParty] = zoneOwner.PK;

			var transportZone = (BusinessObject)Factory.New<IRateTransportZone>();
			transportZone[RateTransportZonesSchema.TZ_TP] = transportProvider.PK;

			var internationalZone = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "USEC"));
			internationalZone.FZ_OH_RelatedParty = carrier.PK;

			var parent = new DummyZoneSelectionParent(Factory);
			var zoneSelection = new ZoneSelection(parent, "TransportZonePK", "InternationalZonePK");

			zoneSelection.IsDomestic = true;
			zoneSelection.ZonePK = transportZone.PK;

			AssertEquals(zoneOwner.PK, zoneSelection.TransportZoneOwnerPK);
			AssertEquals("NZ", zoneSelection.TransportZoneCountry);
			AssertEquals(transportZone.PK, parent.TransportZonePK);
			AssertEquals(ZGuid.Empty, parent.InternationalZonePK);

			zoneSelection.IsDomestic = false;
			zoneSelection.ZonePK = internationalZone.PK;

			AssertEquals(carrier.PK, zoneSelection.InternationalZoneCarrierPK);
			AssertEquals(ZGuid.Empty, parent.TransportZonePK);
			AssertEquals(internationalZone.PK, parent.InternationalZonePK);
		}

		class DummyZoneSelectionParent : NonPersistentBusinessObject
		{
			public DummyZoneSelectionParent(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ZGuid TransportZonePK { get; set; }
			public ZGuid InternationalZonePK { get; set; }
		}
	}
}
