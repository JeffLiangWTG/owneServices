using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using CusAuthorisationHeader = Enterprise.Customs.NL.Business.CusAuthorisationHeader;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(NctsArrivalMovementHeader))]
sealed class NctsArrivalMovementHeaderTest : NctsArrivalMovementHeaderAbstractTest
{
	public void TestGoodsLocation()
	{
		AssertType<CusGoodsLocation>(ArrivalMovement.GoodsLocation);
	}

	protected override void TestBizObjectField(ZPropertyInfo info)
	{
		if (info.Name != "DestinationCustomsOfficeCodeForDeparture"
			&& info.Name != "DestinationCustomsOfficeCodeForArrival")
		{
			base.TestBizObjectField(info);
		}
	}

	public void TestBM_UnloadingRemarks() => CombineAssertions(() =>
	{
		ArrivalMovement.UnloadingRemarksFreeText = "Free Text";
		Factory.Save();
		AssertEquals("Free Text", "Free Text", ArrivalMovement.BM_UnloadingRemarks);
		AddUnloadingRemark(0, "NL123456789", "0", "123");
		Factory.Save();
		AssertEquals("Formatted + Free Text", "<00;NL123456789;0;123>\r\nFree Text", ArrivalMovement.BM_UnloadingRemarks);
		AddUnloadingRemark(1, "NL987654321", "1", "456");
		Factory.Save();
		AssertEquals("Formatted 2 + Free Text", "<00;NL123456789;0;123>\r\n<1;NL987654321;1;456>\r\nFree Text", ArrivalMovement.BM_UnloadingRemarks);
		ArrivalMovement.UnloadingRemarksFreeText = ZString.Empty;
		Factory.Save();
		AssertEquals("Formatted 2", "<00;NL123456789;0;123>\r\n<1;NL987654321;1;456>", ArrivalMovement.BM_UnloadingRemarks);
		ArrivalMovement.UnloadingRemarkCollection.RemoveAll();
		Factory.Save();
		AssertEquals("Empty", ZString.Empty, ArrivalMovement.BM_UnloadingRemarks);
	});

	void AddUnloadingRemark(short itemNumber, string eori, string code, string number)
	{
		var remark = ArrivalMovement.UnloadingRemarkCollection.AddNew();
		remark.ItemNumber = itemNumber;
		remark.EoriNumber = eori;
		remark.Code = code;
		remark.Number = number;
	}

	public void TestUnloadingRemarksFreeText_Attributes() => CombineAssertions(() =>
	{
		_ = AssertEntity<NctsArrivalMovementHeader>()
			.HasProperty(x => x.UnloadingRemarksFreeText)
			.WithAttribute<ReadOnlyMemberAttribute>(x => x.Member == "IsUnloadingRemarksReadOnly")
			.WithCaption("Enter the remarks regarding the unloading")
			.WithMediumCaption("Unloading remarks")
			.WithShortCaption("Remarks");

		var unloadingRemarksFreeTextInfo = ArrivalMovement.UnloadingRemarksFreeTextInfo;

		AssertEquals("Not ReadOnly", false, unloadingRemarksFreeTextInfo.ReadOnly);
		ArrivalMovement.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		AssertEquals("ReadOnly", true, unloadingRemarksFreeTextInfo.ReadOnly);
	});

	public void TestUnloadingRemarksFreeText() => CombineAssertions(() =>
	{
		ArrivalMovement.OnLoaded();
		AssertEquals("Empty", ZString.Empty, ArrivalMovement.UnloadingRemarksFreeText);
		ArrivalMovement.BM_UnloadingRemarks = "Free Text";
		ArrivalMovement.OnLoaded();
		AssertEquals("Free Text", "Free Text", ArrivalMovement.UnloadingRemarksFreeText);
		ArrivalMovement.BM_UnloadingRemarks = "<00;NL123456789;0;123>\r\nFree Text after formatted remark";
		ArrivalMovement.OnLoaded();
		AssertEquals("Free Text after formatted remark", "Free Text after formatted remark", ArrivalMovement.UnloadingRemarksFreeText);
		ArrivalMovement.BM_UnloadingRemarks = "<00;NL123456789;0;123>";
		ArrivalMovement.OnLoaded();
		AssertEquals("Empty with formatted remark", ZString.Empty, ArrivalMovement.UnloadingRemarksFreeText);
	});

	public void TestUnloadingRemarkCollection() => CombineAssertions(() =>
	{
		AssertType<NonPersistentNctsUnloadingRemarkCollection>(ArrivalMovement.UnloadingRemarkCollection);
		AssertEquals("Not ReadOnly", false, ArrivalMovement.UnloadingRemarkCollection.ReadOnly);
		ArrivalMovement.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		AssertEquals("ReadOnly", true, ArrivalMovement.UnloadingRemarkCollection.ReadOnly);
	});

	public void TestUpdateGoodsLocationAfterAuthorizationNumberUpdated()
	{
		var authorizationHeader = Factory.New<CusAuthorisationHeader>();
		authorizationHeader.CPH_Number = "1523625B02";
		authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
		var authorizationRule = authorizationHeader.CusAuthorisationRules.AddNew();
		authorizationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
		authorizationRule.GoodsLocation.CGL_Qualifier = Customs.Business.CusGoodsLocationQualifierList.Codes.PostcodeAddress;
		authorizationRule.GoodsLocation.CGL_Type = Customs.Business.CusGoodsLocationTypeList.Codes.AuthorizedPlace;
		authorizationRule.GoodsLocation.AdditionalIdentifier = "42";
		authorizationRule.GoodsLocation.Address.E2_RN_NKCountryCode = "NL";
		authorizationRule.GoodsLocation.Address.E2_Postcode = "4950 LC";
		authorizationRule.CPR_ValueFrom = "T;B;4950 LC;42;NL";

		CombineAssertions(() =>
		{
			AssertEquals("GoodsLocation is initially empty", string.Empty, ArrivalMovement.GoodsLocationDescription);
			arrivalMovement.AuthorizationCode = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
			ArrivalMovement.AuthorizationNumber = "1523625B02";
			AssertEquals("GoodsLocation is updated", "T;B;4950 LC;42;NL", ArrivalMovement.GoodsLocationDescription);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

	protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest) => new LightValidationTesterExcludingJobDocAddress(bizObjToTest);

	NctsArrivalMovementHeader ArrivalMovement => arrivalMovement ?? (arrivalMovement = GetNewBusinessObject(Factory));
	NctsArrivalMovementHeader arrivalMovement;

	NctsArrivalMovementHeader GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		return nctsHeader.ArrivalMovementHeader;
	}

	sealed class LightValidationTesterExcludingJobDocAddress : LightValidationTester
	{
		public LightValidationTesterExcludingJobDocAddress(BusinessObject bo) : base(bo)
		{
		}

		protected override bool ShouldTestProperty(ZPropertyInfo info)
		{
			var objectType = info.BizObj.GetType();
			if (objectType == typeof(CusGoodsLocation) || objectType == typeof(EU.NCTS.Business.CusGoodsLocationAddress) || IsConsignorDocumentaryAddress(info.BizObj))
			{
				return false;
			}
			else
			{
				return base.ShouldTestProperty(info);
			}

			bool IsConsignorDocumentaryAddress(object bizObj)
			{
				return bizObj is JobDocAddress { DocAddressType: DocAddressType.ConsignorDocumentaryAddress };
			}
		}
	}
}
