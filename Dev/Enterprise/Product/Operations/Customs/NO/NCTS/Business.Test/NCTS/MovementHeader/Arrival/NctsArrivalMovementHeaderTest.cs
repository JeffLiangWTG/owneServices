using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.NO.NCTS.Business.Testing;

[TestedType(typeof(NctsArrivalMovementHeader))]
sealed class NctsArrivalMovementHeaderTest : EU.NCTS.Business.Testing.NctsArrivalMovementHeaderAbstractTest
{
	public void TestHeaderType() =>
		AssertType<NctsHeader>(arrivalMovementHeader.Header);

	public void TestValidationType() =>
		AssertType<NctsArrivalMovementHeaderValidation>(arrivalMovementHeader.Validation);

	public void TestGoodsRegistrationNumber_Attributes() => CombineAssertions(() =>
		AssertEntity<NctsArrivalMovementHeader>()
			.HasProperty(x => x.GoodsRegistrationNumber)
			.WithCaption("Goods Registration Number")
			.WithShortCaption("Goods Reg. Num.")
			.WithFullDescription("Reference ID for the customs clearance of this goods. Normally the goods-number but it may be others (such as customs approval no).")
			.WithAttribute<MaxLengthAttribute>(x => x.MaxLength == 35));

	public void TestGoodsRegistrationNumberLoad()
	{
		var grnForTest = "GRN000123";
		arrivalMovementHeader.GoodsRegistrationNumber = grnForTest;
		Factory.Save();

		var grnEntryNumber = CusEntryNumber.Load(header, CusEntryNumberTypes.Norway.GoodsNumber, Core.Constants.CountryCodes.Norway);
		AssertNotNull(grnEntryNumber);
		CombineAssertions(() =>
		{
			AssertEquals("Entry Number", grnForTest, grnEntryNumber.CE_EntryNum);
			AssertEquals("Parent Table", NctsHeader.Schema.TableName, grnEntryNumber.CE_ParentTable);
			AssertEquals("Entry Type", CusEntryNumberTypes.Norway.GoodsNumber, grnEntryNumber.CE_EntryType);
			AssertEquals("Category", CusEntryNumber.Categories.CustomsPermitClearanceNumber, grnEntryNumber.CE_Category);
			Assert("System generated", grnEntryNumber.CE_EntryIsSystemGenerated);
		});
	}

	protected override void TestBizObjectField(ZPropertyInfo info)
	{
		if (info.Name != nameof(arrivalMovementHeader.DestinationCustomsOfficeCodeForDeparture)
			&& info.Name != nameof(arrivalMovementHeader.DestinationCustomsOfficeCodeForArrival))
		{
			base.TestBizObjectField(info);
		}
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		return nctsHeader.ArrivalMovementHeader;
	}

	protected override BusinessObject GetNewBusinessObject() => arrivalMovementHeader;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => arrivalMovementHeader;

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.NewWithValidTestData<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		arrivalMovementHeader = header.ArrivalMovementHeader;
	}

	NctsHeader header;
	NctsArrivalMovementHeader arrivalMovementHeader;
}
