using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeCodeCarrierIataMappingCollection))]
	sealed class AccChargeCodeCarrierIataMappingCollectionTest : ActiveBusinessObjectCollectionTestCase<AccChargeCodeCarrierIataMappingCollection>
	{
		public void TestNoAuditLogsWithAttibute()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var carrierIataMapping = GetNewCarrierIataMappingWithAllPropertiesPreset(chargeCode);
			Factory.Save();

			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, chargeCode.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				carrierIataMapping.ACI_IATAChargeCodeMap = "DB";
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				chargeCode.AccChargeCodeCarrierIataMappings.Delete(carrierIataMapping);
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		AccChargeCodeCarrierIataMapping GetNewCarrierIataMappingWithAllPropertiesPreset(AccChargeCode chargeCode)
		{
			var carrierIataMapping = chargeCode.AccChargeCodeCarrierIataMappings.AddNew();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "AUNNTRFCNSYD";
			carrierIataMapping.ACI_OH_Carrier = orgHeader.PK;
			carrierIataMapping.ACI_IATAChargeCodeMap = "AC";
			return carrierIataMapping;
		}

		#region Implementation

		protected override AccChargeCodeCarrierIataMappingCollection GetCollectionToTest()
		{
			return Factory.NewWithValidTestData<AccChargeCode>().AccChargeCodeCarrierIataMappings;
		}

		#endregion
	}
}
