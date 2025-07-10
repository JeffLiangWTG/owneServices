using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefAirlineDefaultCommodityCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRDC_RAR_NKProductCode()
		{
			var refAirlineDefaultCommodityCode = GetNewRefAirlineDefaultCommodityCode(airline1);

			refAirlineDefaultCommodityCode.RDC_RAR_NKProductCode = string.Empty;

			AssertHasError(refAirlineDefaultCommodityCode.RDC_RAR_NKProductCodeInfo, "Please enter a value.");

			refAirlineDefaultCommodityCode.RDC_RAR_NKProductCode = "Invalid Code";
			AssertHasError(refAirlineDefaultCommodityCode.RDC_RAR_NKProductCodeInfo, "Enter a valid selection.");

			refAirlineDefaultCommodityCode.RDC_RAR_NKProductCode = "CO1";
			AssertNoErrors(refAirlineDefaultCommodityCode.RDC_RAR_NKProductCodeInfo);
		}

		public void TestCheckRDC_RAC_NKCommodityCode()
		{
			var refAirlineDefaultCommodityCode = GetNewRefAirlineDefaultCommodityCode(airline1);

			refAirlineDefaultCommodityCode.RDC_RAC_NKCommodityCode = string.Empty;
			AssertHasError(refAirlineDefaultCommodityCode.RDC_RAC_NKCommodityCodeInfo, "Please enter a value.");

			refAirlineDefaultCommodityCode.RDC_RAR_NKProductCode = "CO1";
			refAirlineDefaultCommodityCode.RDC_RAC_NKCommodityCode = "0001";
			AssertNoErrors(refAirlineDefaultCommodityCode.RDC_RAC_NKCommodityCodeInfo);

			refAirlineDefaultCommodityCode.RDC_RAR_NKProductCode = "CO1";
			refAirlineDefaultCommodityCode.RDC_RAC_NKCommodityCode = "0002";
			AssertHasError(refAirlineDefaultCommodityCode.RDC_RAC_NKCommodityCodeInfo, "The Commodity Code must be valid for the chosen Product.");
		}

		public void TestCheckUniqueness()
		{
			var refAirlineDefaultCommodityCode1 = GetNewRefAirlineDefaultCommodityCode(airline1);
			refAirlineDefaultCommodityCode1.RDC_RL_NKOrigin = "AUSYD";
			refAirlineDefaultCommodityCode1.RDC_RL_NKDestination = "AUMEL";
			refAirlineDefaultCommodityCode1.RDC_RAC_NKCommodityCode = "0001";
			refAirlineDefaultCommodityCode1.RDC_RAR_NKProductCode = "CO1";
			Factory.Save();

			var refAirlineDefaultCommodityCode2 = GetNewRefAirlineDefaultCommodityCode(airline1);
			refAirlineDefaultCommodityCode2.RDC_RL_NKOrigin = "AUSYD";
			refAirlineDefaultCommodityCode2.RDC_RL_NKDestination = "AUMEL";
			refAirlineDefaultCommodityCode2.RDC_RAC_NKCommodityCode = "0001";
			refAirlineDefaultCommodityCode2.RDC_RAR_NKProductCode = "CO1";

			refAirlineDefaultCommodityCode2.RunPreSaveValidation();

			AssertHasError(refAirlineDefaultCommodityCode2.RDC_RAR_NKProductCodeInfo, "The same Product cannot be duplicated for the same Origin/Destination.");
			AssertHasError(refAirlineDefaultCommodityCode2.RDC_RL_NKOriginInfo, "The same Product cannot be duplicated for the same Origin/Destination.");
			AssertHasError(refAirlineDefaultCommodityCode2.RDC_RL_NKDestinationInfo, "The same Product cannot be duplicated for the same Origin/Destination.");

			var refAirlineDefaultCommodityCode3 = GetNewRefAirlineDefaultCommodityCode(airline2);
			refAirlineDefaultCommodityCode3.RDC_RL_NKOrigin = "AUSYD";
			refAirlineDefaultCommodityCode3.RDC_RL_NKDestination = "AUMEL";
			refAirlineDefaultCommodityCode3.RDC_RAC_NKCommodityCode = "0001";
			refAirlineDefaultCommodityCode3.RDC_RAR_NKProductCode = "CO1";

			refAirlineDefaultCommodityCode3.RunPreSaveValidation();

			AssertNoError(refAirlineDefaultCommodityCode3.RDC_RAR_NKProductCodeInfo, "The same Product cannot be duplicated for the same Origin/Destination.");
			AssertNoError(refAirlineDefaultCommodityCode3.RDC_RL_NKOriginInfo, "The same Product cannot be duplicated for the same Origin/Destination.");
			AssertNoError(refAirlineDefaultCommodityCode3.RDC_RL_NKDestinationInfo, "The same Product cannot be duplicated for the same Origin/Destination.");
		}

		public void TestCheckRDC_RL_NKOrigin()
		{
			var refAirlineDefaultCommodityCode = GetNewRefAirlineDefaultCommodityCode(airline1);

			refAirlineDefaultCommodityCode.RDC_RL_NKOrigin = "AAAAA";

			AssertHasError(refAirlineDefaultCommodityCode.RDC_RL_NKOriginInfo, "Enter a valid selection.");
		}

		public void TestCheckRDC_RL_NKDestination()
		{
			var refAirlineDefaultCommodityCode = GetNewRefAirlineDefaultCommodityCode(airline1);

			refAirlineDefaultCommodityCode.RDC_RL_NKDestination = "AAAAA";

			AssertHasError(refAirlineDefaultCommodityCode.RDC_RL_NKDestinationInfo, "Enter a valid selection.");
		}

		RefAirlineDefaultCommodityCode GetNewRefAirlineDefaultCommodityCode(RefAirline airline)
		{
			var refAirlineDefaultCommodityCode = Factory.New<RefAirlineDefaultCommodityCode>();
			refAirlineDefaultCommodityCode.RDC_RM = airline.PK;

			return refAirlineDefaultCommodityCode;
		}

		RefAirline airline1;
		RefAirline airline2;

		protected override void SetUp()
		{
			base.SetUp();

			var refAirlineProductCode1 = Factory.New<RefAirlineProductCode>();
			refAirlineProductCode1.RAR_AirlineID = "341";
			refAirlineProductCode1.RAR_Code = "CO1";
			refAirlineProductCode1.RAR_Description = "Product Code Description 1";
			Factory.Save();

			var refAirlineCommodityCode1 = Factory.New<RefAirlineCommodityCode>();
			refAirlineCommodityCode1.RAC_AirlineID = "341";
			refAirlineCommodityCode1.RAC_Code = "0001";
			refAirlineCommodityCode1.RAC_Description = "Commodity Code Description 1";

			var refAirlineCommodityCode2 = Factory.New<RefAirlineCommodityCode>();
			refAirlineCommodityCode2.RAC_AirlineID = "341";
			refAirlineCommodityCode2.RAC_Code = "0002";
			refAirlineCommodityCode2.RAC_Description = "Commodity Code Description 2";
			Factory.Save();

			var refAirlineProductCodeCommodityCodePivot1 = Factory.New<RefAirlineProductCodeCommodityCodePivot>();
			refAirlineProductCodeCommodityCodePivot1.RPC_AirlineID = "341";
			refAirlineProductCodeCommodityCodePivot1.RPC_RAR = refAirlineProductCode1.PK;
			refAirlineProductCodeCommodityCodePivot1.RPC_RAC = refAirlineCommodityCode1.PK;
			Factory.Save();

			airline1 = Factory.New<RefAirline>();
			airline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "341";
			Factory.Save();

			airline2 = Factory.New<RefAirline>();
			airline2.RM_EagleAddedAirlinePrefixOrAccountingCode = "624";
			Factory.Save();
		}
	}
}
