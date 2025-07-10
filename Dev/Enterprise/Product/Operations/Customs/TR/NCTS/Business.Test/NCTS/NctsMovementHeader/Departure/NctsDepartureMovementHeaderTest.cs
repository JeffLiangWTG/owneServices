using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDepartureMovementHeader))]
	sealed class NctsDepartureMovementHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidation_Phase4()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertType<NctsDepartureMovementHeaderPhase4Validation>(departureMovement.Validation);
		}

		public void TestValidation_Phase5()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType<NctsDepartureMovementHeaderPhase5Validation>(departureMovement.Validation);
		}

		public void TestLookups()
		{
			AssertType<NctsDepartureMovementHeaderPhase4Lookups>(departureMovement.Lookups);
		}

		public void TestBM_LocationOfGoodsCode()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(NctsDepartureMovementHeader), nameof(NctsDepartureMovementHeader.BM_LocationOfGoodsCode), false, attr => attr.ListDataSourceMember == (nameof(departureMovement.Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.GoodsShippingLocationList)));
		}

		public void TestIsGIkEnabled()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(NctsDepartureMovementHeader), nameof(NctsDepartureMovementHeader.IsGIKEnabled), false, attribute => attribute.Caption == "Shipment with the scope of GİK 117");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(NctsDepartureMovementHeader), nameof(NctsDepartureMovementHeader.IsGIKEnabled), false, attribute => attribute.ShortCaption == "GİK 117 ?");
			departureMovement.IsGIKEnabled = ZBool.False;
			AssertEquals("IsGIKEnabled Must Be Empty", "", departureMovement.BM_LocationQualifier);

			departureMovement.IsGIKEnabled = ZBool.True;
			AssertEquals("IsGIKEnabled Must Be Y", "Y", departureMovement.BM_LocationQualifier);
		}

		public void TestBM_LocationOfGoodsCode_Caption()
		{
			AssertEquals("Goods Shipping Location", DataBoundResourceStrings.GetDataForProperty(departureMovement.BM_LocationOfGoodsCodeInfo).Caption);
			AssertEquals("Goods Ship.Loc.", DataBoundResourceStrings.GetDataForProperty(departureMovement.BM_LocationOfGoodsCodeInfo).ShortCaption);
		}

		public void TestBM_LocationQualifier_Caption()
		{
			AssertEquals("Shipment with the scope of GİK 117", DataBoundResourceStrings.GetDataForProperty(departureMovement.IsGIKEnabledInfo).Caption);
			AssertEquals("GİK 117 ?", DataBoundResourceStrings.GetDataForProperty(departureMovement.IsGIKEnabledInfo).ShortCaption);
		}

		public void TestBM_CustomsStatus()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(NctsDepartureMovementHeader), nameof(NctsDepartureMovementHeader.BM_CustomsStatus), false, attr => attr.ListDataSourceMember == (nameof(departureMovement.Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.CustomsStatusList)));
		}

		public void TestBM_PlaceOfLoading_Caption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(NctsDepartureMovementHeader), nameof(NctsDepartureMovementHeader.BM_PlaceOfLoading), false, attribute => attribute.Caption == "[S17] Place of Loading Code");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(NctsDepartureMovementHeader), nameof(NctsDepartureMovementHeader.BM_PlaceOfLoading), false, attribute => attribute.ShortCaption == "Place of Loading Code");
		}

		public void TestBM_GrossWeightUQ_List() => AssertHasCustomAttribute<ListAttribute>(typeof(NctsDepartureMovementHeader), nameof(NctsDepartureMovementHeader.BM_GrossWeightUQ), false, attr => attr.ListDataSourceMember == (nameof(NctsDepartureMovementHeader.Lookups) + "." + nameof(NctsDepartureMovementHeader.Lookups.WeightUnitList)));

		public void TestBM_InBondEntryType()
		{
			departureMovement.BM_InBondEntryType = "T2SM";
			AssertEquals("T2SM", departureMovement.BM_InBondEntryType);
		}

		public void TestMoveToFTZ()
		{
			departureMovement.MoveToFTZ = ZBool.False;
			AssertEquals("MoveToFTZ Must Be N", "N", departureMovement.BM_MoveToFTZ);

			departureMovement.MoveToFTZ = ZBool.True;
			AssertEquals("MoveToFTZ Must Be Y", "Y", departureMovement.BM_MoveToFTZ);
		}

		public void TestMoveToFTZCondition()
		{
			AssertEquals("MoveToFTZ must be false at first", ZBool.False, departureMovement.MoveToFTZ);

			departureMovement.BM_MoveToFTZ = "Y";
			AssertEquals("MoveToFTZ must be true this time", ZBool.True, departureMovement.MoveToFTZ);

			departureMovement.BM_MoveToFTZ = "N";
			AssertEquals("MoveToFTZ must be again false", ZBool.False, departureMovement.MoveToFTZ);
			AssertEquals("BM_CustomsOfficeAtBorder must be empty anymore", ZString.Empty, departureMovement.BM_CustomsOfficeAtBorder);
		}

		public void TestMoveToFTZCaption()
		{
			AssertEquals("GKİ 117", DataBoundResourceStrings.GetDataForProperty(departureMovement.MoveToFTZInfo).Caption);
		}

		public void TestBM_CustomsOfficeAtBorder_Caption()
		{
			AssertEquals("Goods Ship to Code", DataBoundResourceStrings.GetDataForProperty(departureMovement.BM_CustomsOfficeAtBorderInfo).Caption);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("TR", departureMovement.BM_InBondEntryType);
			AssertEquals("MoveToFTZ", ZBool.False, departureMovement.MoveToFTZ);
			AssertEquals("IsGIKEnabled", ZBool.False, departureMovement.IsGIKEnabled);
		}

		public void TestGuarantees()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			AssertType<NctsGuaranteeCollection<NctsGuarantee>>(nctsHeader.MovementHeader.Guarantees);
		}

		public void TestBM_InBondEntryTypeValueChange()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureMovement = nctsHeader.MovementHeader;
			departureMovement.BM_InBondEntryType = "AA";
			nctsHeader.BH_FTZMove = false;
			departureMovement.BM_InBondEntryType = NctsDepartureMovementHeaderHelper.DeclarationTypeListCode;
			AssertEquals(true, nctsHeader.BH_FTZMove);
		}

		public void TestMovementDetailTypeCore()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertNull(departureMovement.MovementDetailType);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertNotNull(departureMovement.MovementDetailType);
		}

		public void TestOnSaving()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			AssertEquals("Pre-condition", false, nctsHeader.BH_FTZMove);
			Factory.Save();
			AssertEquals("BM_InBondEntryType default value is TR than ,BH_FTZMove should be true", true, nctsHeader.BH_FTZMove);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var actualTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)nctsHeader.MovementHeader).GetCusSupportingInfoTypes();
			CombineAssertions(() =>
			{
				AssertEquals("SupportingDocument", typeof(EU.NCTS.Business.NctsSupportingDocument), actualTypes[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
				AssertEquals("WarehouseToOpen", typeof(NctsWarehouseToOpen), actualTypes[CusSupportingInfoTypeList.Codes.PRE]);
			});
		}

		public void TestGetFetchStrategies()
		{
			var expectedTypes = new[]
			{
				typeof(CusSupportingInfoTypeSupporterFetchStrategy)
			};
			var actualTypes = ((IAdditionalBusinessObjectFetchStrategyProvider)departureMovement).GetFetchStrategies().Select(c => c.GetType());
			AssertContainsExactElementsInAnyOrder(expectedTypes, actualTypes);
		}

		public void TestWarehouseToOpenList()
		{
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			CombineAssertions(() =>
			{
				departureMovement.WarehouseToOpenList.AddNew();
				var result = departureMovement.WarehouseToOpenList;
				AssertEquals("Parent", departureMovement.PK, result[0].Parent.PK);
				AssertType<NctsWarehouseToOpenCollection>("Type", result);
			});
		}

		public void TestTankerStatusGenAddOnColumn()
		{
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureMovement.TankerStatus = "1";
			Factory.Save();

			var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, departureMovement.PK);
			var genAddOnColumn = Factory.LoadTop1<GenAddOnColumn>(query);

			CombineAssertions(() =>
			{
				AssertNotNull(genAddOnColumn);
				AssertEquals("1", departureMovement.TankerStatus);
				AssertEquals("1", genAddOnColumn.XA_Data);
				AssertEquals("TankerStatus", genAddOnColumn.XA_Name);
			});

			departureMovement.TankerStatus = string.Empty;
			Factory.Save();

			genAddOnColumn = Factory.LoadTop1<GenAddOnColumn>(query);
			AssertNull("GenAddOnColumn is deleted when value is set to empty", genAddOnColumn);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => departureMovement;

		protected override BusinessObject GetNewBusinessObject() => departureMovement;

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureMovement = nctsHeader.MovementHeader;
		}
		NctsHeader nctsHeader;
		NctsDepartureMovementHeader departureMovement;
	}
}
