using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbPortDeliveryTime.Loader))]
	sealed class GlbPortDeliveryTimeLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new GlbPortDeliveryTime.Loader(Factory, null, null, "", false);
		}

		public void TestLoaderReturnsCorrectDelayWithMatchingData()
		{
			GlbPortDeliveryTime deliveryTimeLoaded = new GlbPortDeliveryTime.Loader(Factory, Discharge, Destination, Core.Constants.TransportModes.Sea, ZBool.True).Load();
			AssertNotNull("TestDelivery shouldn't be null", deliveryTimeLoaded);
			AssertEquals("Loaded object Should be the same as test one", TestDeliveryTime, deliveryTimeLoaded);
			AssertEquals("There should be 3 days of delay", TestDeliveryTime.G1_DaysDelayFromArrivalToDeliver, deliveryTimeLoaded.G1_DaysDelayFromArrivalToDeliver);
		}

		public void TestLoaderReturnsACorrectDelayForFCLWhenOnlySEAIsDefined()
		{
			GlbPortDeliveryTime deliveryTimeLoaded = new GlbPortDeliveryTime.Loader(Factory, Discharge, Destination, Core.Constants.ContainerModes.FCL, ZBool.True).Load();
			AssertNotNull("TestDelivery shouldn't be null", deliveryTimeLoaded);
			AssertEquals("Loaded object Should be the same as test one", TestDeliveryTime, deliveryTimeLoaded);
			AssertEquals("There should be 3 days of delay", TestDeliveryTime.G1_DaysDelayFromArrivalToDeliver, deliveryTimeLoaded.G1_DaysDelayFromArrivalToDeliver);
		}

		public void TestLoaderReturnsACorrectDelayForLCLWhenOnlySEAIsDefined()
		{
			GlbPortDeliveryTime deliveryTimeLoaded = new GlbPortDeliveryTime.Loader(Factory, Discharge, Destination, Core.Constants.ContainerModes.LCL, ZBool.True).Load();
			AssertNotNull("TestDelivery shouldn't be null", deliveryTimeLoaded);
			AssertEquals("Loaded object Should be the same as test one", TestDeliveryTime, deliveryTimeLoaded);
			AssertEquals("There should be 3 days of delay", TestDeliveryTime.G1_DaysDelayFromArrivalToDeliver, deliveryTimeLoaded.G1_DaysDelayFromArrivalToDeliver);
		}

		public void TestLoaderReturnsNullWithNonMatchingData()
		{
			RefUNLOCO newUNLOCO = Factory.New<RefUNLOCO>();
			Discharge.RL_Code = "ABCDE";

			GlbPortDeliveryTime deliveryTimeLoaded = new GlbPortDeliveryTime.Loader(Factory, newUNLOCO, newUNLOCO, Core.Constants.TransportModes.Air, ZBool.True).Load();
			AssertNull("TestDelivery should be null - no default delivery time", deliveryTimeLoaded);
		}

		public void TestLoaderWithClientOverride()
		{
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client2 = Factory.NewWithValidTestData<OrgHeader>();

			RefUNLOCO syd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			RefUNLOCO bne = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");

			GlbPortDeliveryTime delivery4 = Factory.New<GlbPortDeliveryTime>();
			delivery4.G1_FreightMode = Core.Constants.ContainerModes.LCL;
			delivery4.G1_RL_NKDischargePort = "AUSYD";
			delivery4.G1_RL_NKDestinationPort = "AUBNE";
			delivery4.G1_DaysDelayFromArrivalToDeliver = 8;
			delivery4.G1_JobMode = "FWD";

			GlbPortDeliveryTime delivery2 = Factory.New<GlbPortDeliveryTime>();
			delivery2.G1_FreightMode = Core.Constants.TransportModes.Sea;
			delivery2.G1_RL_NKDischargePort = "AUSYD";
			delivery2.G1_RL_NKDestinationPort = "AUBNE";
			delivery2.G1_DaysDelayFromArrivalToDeliver = 7;
			delivery2.G1_JobMode = "FWD";

			GlbPortDeliveryTime deliveryTimeLoaded = new GlbPortDeliveryTime.Loader(Factory, syd, bne, Core.Constants.ContainerModes.LCL, ZBool.True).Load();
			AssertEquals("Delivery with container mode should have been loaded.", delivery4.PK, deliveryTimeLoaded.PK);

			GlbPortDeliveryTime delivery1 = Factory.New<GlbPortDeliveryTime>();
			delivery1.G1_FreightMode = Core.Constants.TransportModes.Sea;
			delivery1.G1_RL_NKDischargePort = "AUSYD";
			delivery1.G1_RL_NKDestinationPort = "AUBNE";
			delivery1.G1_DaysDelayFromArrivalToDeliver = 4;
			delivery1.G1_OH_ClientOverride = client.PK;
			delivery1.G1_JobMode = "FWD";

			deliveryTimeLoaded = new GlbPortDeliveryTime.Loader(Factory, syd, bne, client, Core.Constants.ContainerModes.LCL, ZBool.True).Load();
			AssertEquals("Delivery with matching client override should have been loaded.", delivery1.PK, deliveryTimeLoaded.PK);

			GlbPortDeliveryTime delivery3 = Factory.New<GlbPortDeliveryTime>();
			delivery3.G1_FreightMode = Core.Constants.TransportModes.Sea;
			delivery3.G1_RL_NKDischargePort = "AUSYD";
			delivery3.G1_RL_NKDestinationPort = "AUBNE";
			delivery3.G1_DaysDelayFromArrivalToDeliver = 6;
			delivery3.G1_OH_ClientOverride = client2.PK;
			delivery3.G1_JobMode = "FWD";

			deliveryTimeLoaded = new GlbPortDeliveryTime.Loader(Factory, syd, bne, client2, Core.Constants.TransportModes.Sea, ZBool.True).Load();
			AssertEquals("Delivery with matching client override should have been loaded.", delivery3.PK, deliveryTimeLoaded.PK);

			deliveryTimeLoaded = new GlbPortDeliveryTime.Loader(Factory, syd, bne, Core.Constants.TransportModes.Sea, ZBool.True).Load();
			AssertEquals("Delivery with no client override should have been loaded.", delivery2.PK, deliveryTimeLoaded.PK);
		}

		public void TestLoaderWithJobMode()
		{
			RefUNLOCO syd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			RefUNLOCO bne = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");

			GlbPortDeliveryTime delivery01 = Factory.New<GlbPortDeliveryTime>();
			delivery01.G1_FreightMode = Core.Constants.ContainerModes.LCL;
			delivery01.G1_RL_NKDischargePort = "AUSYD";
			delivery01.G1_RL_NKDestinationPort = "AUBNE";
			delivery01.G1_DaysDelayFromArrivalToDeliver = 1;
			delivery01.G1_JobMode = "ALL";

			GlbPortDeliveryTime delivery05 = Factory.New<GlbPortDeliveryTime>();
			delivery05.G1_FreightMode = Core.Constants.ContainerModes.LCL;
			delivery05.G1_RL_NKDischargePort = "AUSYD";
			delivery05.G1_RL_NKDestinationPort = "AUBNE";
			delivery05.G1_DaysDelayFromArrivalToDeliver = 5;
			delivery05.G1_JobMode = "FWD";

			GlbPortDeliveryTime deliveryTimeLoaded = new GlbPortDeliveryTime.Loader(Factory, syd, bne, Core.Constants.ContainerModes.LCL, ZBool.True).Load();
			AssertEquals("Delivery with job mode should have been loaded.", delivery05.PK, deliveryTimeLoaded.PK);

			GlbPortDeliveryTime delivery02 = Factory.New<GlbPortDeliveryTime>();
			delivery02.G1_FreightMode = Core.Constants.TransportModes.Sea;
			delivery02.G1_RL_NKDischargePort = "AUSYD";
			delivery02.G1_RL_NKDestinationPort = "AUBNE";
			delivery02.G1_DaysDelayFromArrivalToDeliver = 2;
			delivery02.G1_JobMode = "FWD";

			GlbPortDeliveryTime delivery03 = Factory.New<GlbPortDeliveryTime>();
			delivery03.G1_FreightMode = Core.Constants.TransportModes.Sea;
			delivery03.G1_RL_NKDischargePort = "AUSYD";
			delivery03.G1_RL_NKDestinationPort = "AUBNE";
			delivery03.G1_DaysDelayFromArrivalToDeliver = 3;
			delivery03.G1_JobMode = "CUS";

			deliveryTimeLoaded = new GlbPortDeliveryTime.Loader(Factory, syd, bne, Core.Constants.TransportModes.Sea, ZBool.True).Load();
			AssertEquals("Delivery with job mode should have been loaded.", delivery02.PK, deliveryTimeLoaded.PK);

			GlbPortDeliveryTime delivery04 = Factory.New<GlbPortDeliveryTime>();
			delivery04.G1_FreightMode = Core.Constants.TransportModes.Air;
			delivery04.G1_RL_NKDischargePort = "AUSYD";
			delivery04.G1_RL_NKDestinationPort = "AUBNE";
			delivery04.G1_DaysDelayFromArrivalToDeliver = 4;

			deliveryTimeLoaded = new GlbPortDeliveryTime.Loader(Factory, syd, bne, Core.Constants.TransportModes.Air, ZBool.False).Load();
			AssertEquals("Delivery with job mode should have been loaded.", delivery04.PK, deliveryTimeLoaded.PK);
		}

		public void TestWithNoClientMatch()
		{
			RefUNLOCO syd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			RefUNLOCO bne = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");

			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client2 = Factory.NewWithValidTestData<OrgHeader>();

			GlbPortDeliveryTime delivery01 = Factory.New<GlbPortDeliveryTime>();
			delivery01.G1_FreightMode = Core.Constants.TransportModes.Air;
			delivery01.G1_RL_NKDischargePort = "AUSYD";
			delivery01.G1_RL_NKDestinationPort = "AUBNE";
			delivery01.G1_DaysDelayFromArrivalToDeliver = 1;
			delivery01.G1_JobMode = "ALL";
			delivery01.G1_OH_ClientOverride = client.PK;

			GlbPortDeliveryTime delivery02 = Factory.New<GlbPortDeliveryTime>();
			delivery02.G1_FreightMode = Core.Constants.TransportModes.Sea;
			delivery02.G1_RL_NKDischargePort = "AUSYD";
			delivery02.G1_RL_NKDestinationPort = "AUBNE";
			delivery02.G1_DaysDelayFromArrivalToDeliver = 5;
			delivery02.G1_JobMode = "ALL";
			delivery02.G1_OH_ClientOverride = client2.PK;

			GlbPortDeliveryTime deliveryTimeLoaded = new GlbPortDeliveryTime.Loader(Factory, syd, bne, client, Core.Constants.TransportModes.Sea, ZBool.True).Load();
			AssertNull("Delivery should be null", deliveryTimeLoaded);

			GlbPortDeliveryTime delivery03 = Factory.New<GlbPortDeliveryTime>();
			delivery03.G1_FreightMode = Core.Constants.TransportModes.Sea;
			delivery03.G1_RL_NKDischargePort = "AUSYD";
			delivery03.G1_RL_NKDestinationPort = "AUBNE";
			delivery03.G1_DaysDelayFromArrivalToDeliver = 56;
			delivery03.G1_JobMode = "ALL";

			deliveryTimeLoaded = new GlbPortDeliveryTime.Loader(Factory, syd, bne, client, Core.Constants.TransportModes.Sea, ZBool.True).Load();
			AssertEquals("Delivery03 should have been loaded.", delivery03.PK, deliveryTimeLoaded.PK);
		}

		public void TestLoaderForInvalidParameters()
		{
			RefUNLOCO nullUNLOCO = null;

			GlbPortDeliveryTime deliveryTimeLoaded = new GlbPortDeliveryTime.Loader(Factory, nullUNLOCO, Destination, Core.Constants.ContainerModes.LCL, ZBool.True).Load();
			AssertNull("TestDelivery should be null for null Port Of Discharge", deliveryTimeLoaded);

			deliveryTimeLoaded = new GlbPortDeliveryTime.Loader(Factory, Discharge, nullUNLOCO, Core.Constants.TransportModes.Air, ZBool.True).Load();
			AssertNull("TestDelivery should be null for null Port Of Destination", deliveryTimeLoaded);

			deliveryTimeLoaded = new GlbPortDeliveryTime.Loader(Factory, Discharge, Destination, "", ZBool.True).Load();
			AssertNull("TestDelivery should be null for empty FreightMode", deliveryTimeLoaded);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestDeliveryTime = Factory.New<GlbPortDeliveryTime>();
			TestDeliveryTime.G1_FreightMode = Core.Constants.TransportModes.Sea;
			TestDeliveryTime.G1_RL_NKDischargePort = "USAAA";
			TestDeliveryTime.G1_RL_NKDestinationPort = "USNYC";
			TestDeliveryTime.G1_DaysDelayFromArrivalToDeliver = 3;

			GlbPortDeliveryTime deliveryTime2 = Factory.New<GlbPortDeliveryTime>();
			deliveryTime2.G1_FreightMode = Core.Constants.TransportModes.Rail;
			deliveryTime2.G1_RL_NKDischargePort = "AUBNE";
			deliveryTime2.G1_RL_NKDestinationPort = "AUSYD";
			deliveryTime2.G1_DaysDelayFromArrivalToDeliver = 2;
		}

		GlbPortDeliveryTime TestDeliveryTime;

		RefUNLOCO fDischarge;
		RefUNLOCO Discharge
		{
			get
			{
				if (fDischarge == null)
				{
					fDischarge = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, DischargePortCode));
				}
				AssertNotNull("There should be an UNLOCO with the code:" + DischargePortCode + " in the db.", fDischarge);
				return fDischarge;
			}
		}

		RefUNLOCO fDestination;
		RefUNLOCO Destination
		{
			get
			{
				if (fDestination == null)
				{
					fDestination = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, DestinationPortCode));
				}
				AssertNotNull("There should be a UNLOCO with the code: " + DestinationPortCode + " in the db.", fDestination);
				return fDestination;
			}
		}

		const string DischargePortCode = "USAAA";
		const string DestinationPortCode = "USNYC";

		#endregion
	}
}
