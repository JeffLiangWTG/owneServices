using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(AdditionalInfo))]
sealed class AdditionalInfoTest : Customs.Business.Testing.CusSupportingInfoTest<AdditionalInfo>
{
	public void TestLookups() => CombineAssertions(() =>
	{
		var additionalInfo = GetNewBusinessObject(Factory);
		AssertType<AdditionalInfoUcc6Lookups>("ucc6", additionalInfo.Lookups);

		var additionalInfoNoUcc6 = Factory.New<AdditionalInfoNoUcc6ForTest>();
		AssertType<AdditionalInfoLookups>("not ucc6", additionalInfoNoUcc6.Lookups);
	});

	public void TestCSI_SubType() => CombineAssertions(() =>
	{
		var additionalInfo = GetNewBusinessObject(Factory);
		AssertEquals("CSI_SubType not readonly", expected: false, additionalInfo.CSI_SubTypeInfo.ReadOnly);
		AssertEquals("CSI_SubType default empty", expected: ZString.Empty, additionalInfo.CSI_SubType);
	});

	public void TestCSI_Status() => CombineAssertions(() =>
	{
		var additionalInfo = GetNewBusinessObject(Factory);
		AssertEquals("CSI_Status default empty", expected: ZString.Empty, additionalInfo.CSI_Status);

		additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		AssertEquals("when CSI_SubType is INF", expected: true, additionalInfo.CSI_StatusInfo.ReadOnly);

		additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
		AssertEquals("when CSI_SubType is TRA", expected: false, additionalInfo.CSI_StatusInfo.ReadOnly);
	});

	public void TestValidation()
	{
		var additionalInfo = GetNewBusinessObject(Factory);
		AssertType<AdditionalInfoValidation>(additionalInfo.Validation);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

	public static AdditionalInfo GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.NewWithValidTestData<CusExitHeader>();
		var consignment = header.CusExitConsignments.AddNew();
		var report = header.CusExitReports.AddNew();
		report.CER_CXC_Consignment = consignment.PK;
		report.CER_DateTime = ZDateTimeOffset.Now;
		report.CER_OfficeOfExit = "IE001";
		return (AdditionalInfo)report.AdditionalInfos.AddNew();
	}

	class AdditionalInfoNoUcc6ForTest : AdditionalInfo
	{
		public AdditionalInfoNoUcc6ForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override bool IsUCC6Core => false;
	}
}

