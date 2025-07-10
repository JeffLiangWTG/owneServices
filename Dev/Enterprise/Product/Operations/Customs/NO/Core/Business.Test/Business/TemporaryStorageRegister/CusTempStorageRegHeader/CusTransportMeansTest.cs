using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusTransportMeans))]
sealed class CusTransportMeansTest : EnterpriseBusinessObjectTestCase
{
	public void TestTPM_IdentificationNumber_Attributes() => CombineAssertions(() =>
		AssertEntity<CusTransportMeans>()
			.HasProperty(x => x.TPM_IdentificationNumber)
			.WithCaption("Transport ID")
			.WithFullDescription("For SEA use ship name. For AIR use flight number. For other modes use the vehicle Id."));

	public void TestTPM_RN_NKTransportNationality_Attributes() => CombineAssertions(() =>
		AssertEntity<CusTransportMeans>()
			.HasProperty(x => x.TPM_RN_NKTransportNationality)
			.WithCaption("Nationality"));

	public void TestParentHeader()
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		transportMeans.TPM_ParentID = header.PK;

		AssertEquals(header, transportMeans.ParentHeader);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var bizObj = factory.NewWithValidTestData<CusTransportMeans>();
		bizObj.TPM_ParentTableCode = CusTempStorageRegHeaderSchema.Constants.Prefix;
		return bizObj;
	}

	protected override BusinessObject GetNewBusinessObject() => transportMeans;

	protected override void SetUp()
	{
		base.SetUp();
		transportMeans = Factory.New<CusTransportMeans>();
		transportMeans.TPM_ParentTableCode = CusTempStorageRegHeaderSchema.Constants.Prefix;
	}
	CusTransportMeans transportMeans;
}
