using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class ThreeLetterAirCarrierCodeHelperTest : TestCaseWithFactory
	{
		public void TestGetAirCarrierThreeLetterCode()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			var refAirline = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, "A2"));
			if (refAirline == null)
			{
				refAirline = Factory.New<RefAirline>();
				refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = "A8";
				refAirline.RM_TwoCharacterCode = "A2";
			}

			refAirline.RM_ThreeLetterCode = "A00";
			refAirline.Factory.Save();
			var carrierCodeHelper = new ThreeLetterAirCarrierCodeHelper(header);
			AssertEquals(carrierCodeHelper.GetRefAirlineThreeLetterCodeIfNecessary("A2"), refAirline.RM_ThreeLetterCode);
			refAirline.RM_TwoCharacterCode = "AB";
			refAirline.Factory.Save();
			AssertEquals(carrierCodeHelper.GetRefAirlineThreeLetterCodeIfNecessary("AB"), ZString.Empty);
		}

		public void TestShouldSendThreeLetterAirCarrierCode()
		{
			Assert(ThreeLetterAirCarrierCodeHelper.IsThreeLetterAirCarrierCodeRequired("A2"));
			Assert(!ThreeLetterAirCarrierCodeHelper.IsThreeLetterAirCarrierCodeRequired("AB"));
			Assert(!ThreeLetterAirCarrierCodeHelper.IsThreeLetterAirCarrierCodeRequired("AB3"));
			Assert(!ThreeLetterAirCarrierCodeHelper.IsThreeLetterAirCarrierCodeRequired(""));
		}

		public void TestGenAddOnColumn()
		{
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			header.BH_CarrierSCAC = "Z5";
			header.ThreeLetterAirCarrierCode = "ABC";
			Factory.Save();
			var header2 = Factory.Load<CusInBondHeader>(header.PK);
			AssertEquals(header2.ThreeLetterAirCarrierCode, "ABC");
			var addOnCarrierCodeQuery = new ZQuery(GenAddOnColumnSchema.XA_ParentID, header.PK);
			addOnCarrierCodeQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, header.TablePrefix);
			addOnCarrierCodeQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, "ThreeLetterAirCarrierCode");
			var addOn = Factory.LoadTop1<GenAddOnColumn>(addOnCarrierCodeQuery);
			AssertNotNull(addOn);
			AssertEquals(addOn.XA_Data, "ABC");
		}
	}
}
