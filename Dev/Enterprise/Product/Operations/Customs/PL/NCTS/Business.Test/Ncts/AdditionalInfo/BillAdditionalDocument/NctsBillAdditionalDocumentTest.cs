using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(NctsBillAdditionalDocument))]
sealed class NctsBillAdditionalDocumentTest : CusSupportingInfoTest<NctsBillAdditionalDocument>
{
	public void TestCSI_ReferenceNumber_MaxLength_Arrival()
	{
		CombineAssertions(() =>
		{
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			AssertEquals(70, additionalDocument.CSI_ReferenceNumberInfo.MaxLength);

			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			AssertEquals(70, additionalDocument.CSI_ReferenceNumberInfo.MaxLength);
		});
	}

	public void TestCSI_ReferenceNumber_MaxLength_Departure()
	{
		CombineAssertions(() =>
		{
			departureAdditionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			AssertEquals(70, departureAdditionalDocument.CSI_ReferenceNumberInfo.MaxLength);

			departureAdditionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			AssertEquals(70, departureAdditionalDocument.CSI_ReferenceNumberInfo.MaxLength);
		});
	}

	public void TestLookups() => CombineAssertions(() =>
	{
		AssertType<NctsBillAdditionalDocumentLookups>("Arrival", additionalDocument.Lookups);
		AssertType<NctsBillAdditionalDocumentLookups>("Departure", departureAdditionalDocument.Lookups);
	});

	public void TestValidation() => CombineAssertions(() =>
	{
		AssertType<NctsBillAdditionalDocumentPhase5Validation>(additionalDocument.Validation);
	});

	protected override IEnumerable<NctsBillAdditionalDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var bill = nctsHeader.Bills.AddNew();
		yield return bill.AdditionalDocuments.AddNew();
	}

	protected override BusinessObject GetNewBusinessObject() => additionalDocument;

	protected override void SetUp()
	{
		base.SetUp();
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var bill = nctsHeader.Bills.AddNew();
		additionalDocument = bill.AdditionalDocuments.AddNew();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;

		var nctsHeader2 = Factory.New<NctsHeader>();
		nctsHeader2.SetMovementType(NctsMovementType.Codes.Departure);
		var bill2 = nctsHeader2.Bills.AddNew();
		departureAdditionalDocument = bill2.AdditionalDocuments.AddNew();
		nctsHeader2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader2.BH_HeaderType = NctsMovementType.Codes.Departure;
	}
	NctsBillAdditionalDocument additionalDocument, departureAdditionalDocument;
}
