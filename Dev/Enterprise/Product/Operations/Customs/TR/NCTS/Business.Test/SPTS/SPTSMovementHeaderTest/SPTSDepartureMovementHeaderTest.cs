using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business
{
	[TestedType(typeof(SPTSDepartureMovementHeader))]
	class SPTSDepartureMovementHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoadOrCreateDepartureMovementHeader()
		{
			var header = Factory.New<SPTSHeader>();
			var movementHeader = SPTSDepartureMovementHeader.LoadOrCreate(header, "D");

			CombineAssertions("SPTSDepartureMovementHeader detail", () =>
			{
				AssertEquals(header.PK, movementHeader.BM_BH);
				AssertEquals("D", movementHeader.BM_SubApplicationCode);
			});

			Factory.Save();

			var newFactory = NewFactory();
			var reLoadHeader = newFactory.Load<SPTSHeader>(header.PK);
			AssertEquals(movementHeader.PK, reLoadHeader.MovementHeader.PK);
		}

		public void TestValidation()
		{
			AssertType<SPTSDepartureMovementHeaderValidation>(departureMovement.Validation);
		}

		public void TestLookups()
		{
			AssertType<SPTSDepartureMovementHeaderLookups>(departureMovement.Lookups);
		}

		public void TestSetDefaultValues()
		{
			var header = Factory.New<SPTSDepartureMovementHeader>();
			AssertEquals(SPTSTransportModeList.Codes.SEA, header.BM_InlandTransportMode);
		}

		public void TestBM_InlandTransportMode()
		{
			AssertEquals("Max Length", 3, departureMovement.BM_InlandTransportModeInfo.MaxLength);
		}

		protected override void SetUp()
		{
			base.SetUp();
			sptsHeader = Factory.New<SPTSHeader>();
			departureMovement = sptsHeader.MovementHeader;
		}
		SPTSHeader sptsHeader;
		SPTSDepartureMovementHeader departureMovement;

		protected override BusinessObject GetNewBusinessObject() => departureMovement;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => departureMovement;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => departureMovement;
	}
}
