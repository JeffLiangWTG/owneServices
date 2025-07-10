using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(NctsAdditionalInfo))]
sealed class NctsAdditionalInfoTest : EU.NCTS.Business.Testing.NctsAdditionalInfoTest<NctsAdditionalInfo>
{
	public void TestCSI_ReferenceNumber_MaxLength_Arrival()
	{
		CombineAssertions(() =>
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			var bill = nctsHeader.Bills.AddNew();
			var goodsItem = bill.ArrivalGoodsItems.AddNew();
			var additionalInfo = goodsItem.AdditionalInfos.AddNew();
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			AssertEquals(70, additionalInfo.CSI_ReferenceNumberInfo.MaxLength);

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			AssertEquals(70, additionalInfo.CSI_ReferenceNumberInfo.MaxLength);
		});
	}

	public void TestCSI_ReferenceNumber_MaxLength_Departure()
	{
		CombineAssertions(() =>
		{
			var (_, _, _, additionalInfo) = CreateDepartureData(Factory, phase: CusInBondApplicationCodeList.Codes.NCTS5);
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			AssertEquals(70, additionalInfo.CSI_ReferenceNumberInfo.MaxLength);

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			AssertEquals(70, additionalInfo.CSI_ReferenceNumberInfo.MaxLength);
		});
	}

	public void TestLookups_Phase5()
	{
		var (_, additionalInfoHeader, _, additionalInfo) = CreateDepartureData(Factory, phase: CusInBondApplicationCodeList.Codes.NCTS5);
		CombineAssertions(() =>
		{
			AssertType<NctsAdditionalInfoPhase5Lookups>("Header", additionalInfoHeader.Lookups);
			AssertType<NctsAdditionalInfoPhase5Lookups>("Item", additionalInfo.Lookups);
		});
	}

	public void TestValidation_Phase5()
	{
		var (_, additionalInfoHeader, _, additionalInfo) = CreateDepartureData(Factory, phase: CusInBondApplicationCodeList.Codes.NCTS5);
		CombineAssertions(() =>
		{
			AssertType<NctsAdditionalInfoPhase5Validation>("Header", additionalInfoHeader.Validation);
			AssertType<NctsAdditionalInfoPhase5Validation>("Item", additionalInfo.Validation);
		});
	}

	public void TestCSI_Description()
	{
		CombineAssertions(() =>
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = nctsHeader.Bills.AddNew();
			var additionalInfoHeader = nctsHeader.AdditionalDocuments.AddNew();

			additionalInfoHeader.CSI_SubType = AdditionalInfoKindList.Codes.INF;
			AssertEquals("Is not readonly", false, additionalInfoHeader.CSI_DescriptionInfo.ReadOnly);
			additionalInfoHeader.CSI_SubType = AdditionalInfoKindList.Codes.REF;
			AssertEquals("Is readonly", true, additionalInfoHeader.CSI_DescriptionInfo.ReadOnly);

			using (PLCustomsDataRegistry.Instance.PCSEmailChannel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "PL701071897800000"))
			{
				additionalInfoHeader.CSI_SubType = AdditionalInfoKindList.Codes.INF;
				additionalInfoHeader.CSI_Code = Constants.AdditionalInfoCodes._POW01;
				AssertEquals("Is  readonly when PCSEmailChannel is pre-configured and Code is  equal to POW01", true, additionalInfoHeader.CSI_DescriptionInfo.ReadOnly);

				additionalInfoHeader.CSI_Code = Constants.AdditionalInfoCodes._PCS01;
				AssertEquals("Is readonly when PCSEmailChannel is pre-configured and Code equals to PCS01", true, additionalInfoHeader.CSI_DescriptionInfo.ReadOnly);
			}
		});
	}

	static (NctsHeader nctsHeader, NctsAdditionalInfo additionalInfoHeader, NctsDepartureCargoDesc goodsItem, NctsAdditionalInfo additionalInfo) CreateDepartureData(BusinessObjectFactory factory, string phase = CusInBondApplicationCodeList.Codes.NCTS4)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = phase;
		var additionalInfoHeader = nctsHeader.AdditionalDocuments.AddNew();
		var bill = nctsHeader.Bills.AddNew();
		var goodsItem = bill.GoodsItems.AddNew();
		var additionalInfo = goodsItem.AdditionalInfos.AddNew();
		return (nctsHeader, additionalInfoHeader, goodsItem, (NctsAdditionalInfo)additionalInfo);
	}
}
