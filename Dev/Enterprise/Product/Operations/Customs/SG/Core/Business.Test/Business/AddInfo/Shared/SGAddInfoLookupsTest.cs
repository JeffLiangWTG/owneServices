using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class SGAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			AssertEquals(AddInfo, SGAddInfoLookups.Parent);
		}

		public void TestTransportTypeList()
		{
			Assert(SGAddInfoLookups.TransportTypeList is TransportModeCodeList);
		}

		public void TestUnitOfQuantityList()
		{
			Assert(SGAddInfoLookups.UnitOfQuantityList is UnitOfQuantityCodeList);
		}

		public void TestEngineCapacityList()
		{
			AssertNotNull(SGAddInfoLookups.EngineCapacityList);
			Assert(SGAddInfoLookups.EngineCapacityList.ContainsCode(EngineCapacityCodeList.Codes.KW));
		}

		public void TestESDNPs()
		{
			AssertNotNull(SGAddInfoLookups.ESNDPs);
			Assert(SGAddInfoLookups.ESNDPs.ContainsCode(MarkingCodeList.Codes.HW));
		}

		public void TestCommodityTypes()
		{
			AssertNotNull(SGAddInfoLookups.CommodityTypes);
			Assert(SGAddInfoLookups.CommodityTypes.ContainsCode(CommodityTypeList.Codes.Alcohol));
			Assert(SGAddInfoLookups.CommodityTypes.ContainsCode(CommodityTypeList.Codes.Petroleum));
			Assert(SGAddInfoLookups.CommodityTypes.ContainsCode(CommodityTypeList.Codes.Tobacco));
			Assert(SGAddInfoLookups.CommodityTypes.ContainsCode(CommodityTypeList.Codes.Vehicle));
		}

		public void TestSGPlacesList()
		{
			AssertNotNull(SGAddInfoLookups.SGCPlacesList);
		}

		#region SGAddInfoLookups
		SGAddInfoLookups SGAddInfoLookups
		{
			get
			{
				return fSGAddInfoLookups ?? (fSGAddInfoLookups = new SGAddInfoLookups(AddInfo));
			}
		}

		SGAddInfoLookups fSGAddInfoLookups;
		#endregion

		#region AddInfo
		AddInfo AddInfo
		{
			get
			{
				if (addInfo == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					addInfo = declaration.AddInfo;
				}

				return addInfo;
			}
		}

		AddInfo addInfo;
		#endregion
	}
}
