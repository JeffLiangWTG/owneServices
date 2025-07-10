using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(CusExitReport))]
sealed class CusExitReportTest : EnterpriseBusinessObjectTestCase
{
	public void TestHeader()
	{
		var (report, _, header) = GetNewBusinessObject(Factory);
		AssertEquals(header.PK, report.Header.PK);
	}

	public void TestAdditionalInfos()
	{
		var (report, _, _) = GetNewBusinessObject(Factory);
		AssertType<AdditionalInfoCollection<AdditionalInfo>>(report.AdditionalInfos);
	}

	public void TestCusSupportingInfoTypes()
	{
		var (report, _, _) = GetNewBusinessObject(Factory);
		var cusSupportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)report).GetCusSupportingInfoTypes();
		AssertEquals(typeof(AdditionalInfo), cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.AdditionalInfo]);
	}

	public void TestCusExitReportItems()
	{
		var (report, _, _) = GetNewBusinessObject(Factory);
		AssertType<CusExitReportItemCollection<CusExitReportItem>>(report.CusExitReportItems);
	}

	public void TestCusExitReportItemPackages()
	{
		var (report, _, _) = GetNewBusinessObject(Factory);
		AssertType<CusExitReportItemCollection<CusExitReportItem>>(report.CusExitReportItemPackages);
	}

	public void TestICusExitReportCorrectlySetup() => CombineAssertions(() =>
	{
		var (report, _, _) = GetNewBusinessObject(Factory);
		Factory.Save();
		var newFactory = new BusinessObjectFactory();
		var iReport = newFactory.Load<Integration.Customs.PLExitControl.ICusExitReport>(report.PK);

		AssertType<CusExitReport>("report", iReport);
		AssertType<CusExitHeader>("header", (iReport as CusExitReport).Header);
		AssertType<CusExitConsignment>("Consignment", iReport.Consignment);
	});

	public void TestCER_Location_Caption()
	{
		(var report, _, _) = GetNewBusinessObject(Factory);
		this.AssertDataBoundResourceStringsWithMultipleResourceKey(report.CER_LocationInfo, (string[])null, "Arrival Notification Place", mediumCaption: "Arr. Notif. Place");
	}

	public void TestAlternativeEvidences()
	{
		var (report, _, _) = GetNewBusinessObject(Factory);
		AssertType<AlternativeEvidenceCollection<AlternativeEvidence>>(report.AlternativeEvidences);
	}

	public void TestSetAdditionalInfoReadOnly() => CombineAssertions(() =>
	{
		var (report, _, header) = GetNewBusinessObject(Factory);

		var additionalInfo1 = report.AdditionalInfos.AddNew();
		additionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		var additionalInfo2 = report.AdditionalInfos.AddNew();
		additionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;

		report.CER_Calc_Discrepancies = false;
		AssertEquals("when CER_Calc_Discrepancies is false, additionalInfo1 is not readonly", false, additionalInfo1.ReadOnly);
		AssertEquals("when CER_Calc_Discrepancies is false, additionalInfo2 is readonly", true, additionalInfo2.ReadOnly);

		report.CER_Calc_Discrepancies = true;
		AssertEquals("when CER_Calc_Discrepancies is true, additionalInfo1 is not readonly", false, additionalInfo1.ReadOnly);
		AssertEquals("when CER_Calc_Discrepancies is true, additionalInfo2 is not readonly", false, additionalInfo2.ReadOnly);
	});

	public void TestCER_TransportID() => CombineAssertions(() =>
	{
		var (report, _, _) = GetNewBusinessObject(Factory);

		foreach (var transportType in (string[])["50", "53", "70", "79"])
		{
			AssertProperty(transportType, isReadOnly: false);
		}

		foreach (var transportType in (string[])["10", "23", "30", "63", "87"])
		{
			AssertProperty(transportType, isReadOnly: true);
		}
		return;

		void AssertProperty(string transportType, bool isReadOnly)
		{
			report.CER_TransportID = "12";
			report.CER_TransportType = transportType;
			report.Validation.ValidateCER_TransportID();
			if (!isReadOnly)
			{
				AssertEquals($"Transport ID: {report.CER_TransportID}, Transport type: {transportType}, CER_TransportIDInfo is ReadOnly", true, report.CER_TransportIDInfo.ReadOnly);
				AssertEquals($"Transport ID: {report.CER_TransportID}, Transport type: {transportType}, CER_TransportID value cleared", string.Empty, report.CER_TransportID);
			}
			else
			{
				AssertEquals($"Transport ID: {report.CER_TransportID}, Transport type: {transportType}, CER_TransportIDInfo is not ReadOnly", false, report.CER_TransportIDInfo.ReadOnly);
				AssertNotEquals($"Transport ID: {report.CER_TransportID}, Transport type: {transportType}, CER_TransportID value not cleared", string.Empty, report.CER_TransportID);
			}
		}
	});

	public void TestCER_RN_NKTransportNationality() => CombineAssertions(() =>
	{
		var (report, _, _) = GetNewBusinessObject(Factory);

		foreach (var transportType in (string[])["20", "25", "50", "53", "70", "79"])
		{
			AssertProperty(transportType, isReadOnly: false);
		}

		foreach (var transportType in (string[])["10", "30", "63", "87"])
		{
			AssertProperty(transportType, isReadOnly: true);
		}
		return;

		void AssertProperty(string transportType, bool isReadOnly)
		{
			report.CER_RN_NKTransportNationality = "BR";
			report.CER_TransportType = transportType;
			report.Validation.ValidateCER_RN_NKTransportNationality();
			if (!isReadOnly)
			{
				AssertEquals($"Transport Nationality: {report.CER_RN_NKTransportNationality}, Transport type: {transportType}, CER_RN_NKTransportNationalityInfo is ReadOnly", true, report.CER_RN_NKTransportNationalityInfo.ReadOnly);
				AssertEquals($"Transport Nationality: {report.CER_RN_NKTransportNationality}, Transport type: {transportType}, CER_RN_NKTransportNationality value cleared", string.Empty, report.CER_RN_NKTransportNationality);
			}
			else
			{
				AssertEquals($"Transport Nationality: {report.CER_RN_NKTransportNationality}, Transport type: {transportType}, CER_RN_NKTransportNationalityInfo is not ReadOnly", false, report.CER_RN_NKTransportNationalityInfo.ReadOnly);
				AssertNotEquals($"Transport Nationality: {report.CER_RN_NKTransportNationality}, Transport type: {transportType}, CER_RN_NKTransportNationality value not cleared", string.Empty, report.CER_RN_NKTransportNationality);
			}
		}
	});

	public void TestValidationType() => AssertType<CusExitReportValidation>(Factory.New<CusExitReport>().Validation);

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory).report;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).report;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory).report;

	public static (CusExitReport report, CusExitConsignment consignment, CusExitHeader header) GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.New<CusExitHeader>();
		var consignment = header.CusExitConsignments.AddNew();
		var report = header.CusExitReports.AddNew();
		report.CER_CXC_Consignment = consignment.PK;
		report.CER_DateTime = ZDateTimeOffset.Now;
		report.CER_OfficeOfExit = "PL001";
		return (report, consignment, header);
	}

	public void TestGetNewLookups()
	{
		var (exitReport, _, _) = GetNewBusinessObject(Factory);
		AssertType<CusExitReportUcc6Lookups>(exitReport.Lookups);
	}
}
