using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using CusTempStorageDecCollection = Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageDecCollection<Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageDec, Enterprise.Customs.PL.Business.CusTempStorage.CusTempStorageJobHeader>;

namespace Enterprise.Customs.PL.Business.CusTempStorage.Testing;

[TestedType(typeof(CusTempStorageJobHeader))]
class CusTempStorageJobHeaderTest : EnterpriseBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject() => CusTempStorageJobHeader.New(Factory);

	public void TestLookups() => AssertType<CusTempStorageJobHeaderLookups>(((CusTempStorageJobHeader)GetNewBusinessObject()).Lookups);

	public void TestCusTempStorageDecs() => AssertType<CusTempStorageDecCollection>(((CusTempStorageJobHeader)GetNewBusinessObject()).CusTempStorageDecs);

	public void TestCusTempStorageDec() => AssertType<CusTempStorageDec>(((CusTempStorageJobHeader)GetNewBusinessObject()).CusTempStorageDec);

	public void TestNew() => AssertNotNull(CusTempStorageJobHeader.New(Factory));

	public void TestOnSaving()
	{
		var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
		Factory.Save();

		AssertEquals("SJH_JobReference", false, header.SJH_JobReference.IsEmpty);
	}

	public void TestDefaultValues()
	{
		var header = CusTempStorageJobHeader.New(Factory);

		AssertEquals("IST", header.SJH_AppCode);
	}

	public void TestCaptions()
	{
		var header = CusTempStorageJobHeader.New(Factory);

		CombineAssertions(() =>
		{
			AssertEquals("SJH_TransportMeansCode caption", "Transport Means", DataBoundResourceStrings.GetDataForProperty(header.SJH_TransportMeansCodeInfo).Caption);
			AssertEquals("SJH_TransportMeansCode mediumCaption", "Transp. Means", DataBoundResourceStrings.GetDataForProperty(header.SJH_TransportMeansCodeInfo).MediumCaption);
			AssertEquals("SJH_TransportMeansCode shortCaption", "Transp.", DataBoundResourceStrings.GetDataForProperty(header.SJH_TransportMeansCodeInfo).ShortCaption);

			AssertEquals("SJH_TransportMeansCode caption", "Departure Date", DataBoundResourceStrings.GetDataForProperty(header.SJH_DepartureDateInfo).Caption);
			AssertEquals("SJH_TransportMeansCode mediumCaption", "Departure", DataBoundResourceStrings.GetDataForProperty(header.SJH_DepartureDateInfo).MediumCaption);
			AssertEquals("SJH_TransportMeansCode shortCaption", "Dep.", DataBoundResourceStrings.GetDataForProperty(header.SJH_DepartureDateInfo).ShortCaption);

			AssertEquals("SJH_TempStorageEndDateUtc caption", "Temporary Storage End Date", DataBoundResourceStrings.GetDataForProperty(header.SJH_TempStorageEndDateUtcInfo).Caption);
			AssertEquals("SJH_TempStorageEndDateUtc mediumCaption", "End Date", DataBoundResourceStrings.GetDataForProperty(header.SJH_TempStorageEndDateUtcInfo).MediumCaption);
			AssertEquals("SJH_TempStorageEndDateUtc shortCaption", "End", DataBoundResourceStrings.GetDataForProperty(header.SJH_TempStorageEndDateUtcInfo).ShortCaption);
		});
	}
}
