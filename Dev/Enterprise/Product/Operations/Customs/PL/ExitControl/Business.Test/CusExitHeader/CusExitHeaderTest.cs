using CargoWise.EntityFramework;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(CusExitHeader))]
sealed class CusExitHeaderTest : EU.ExitControl.Business.Testing.CusExitHeaderAbstractTest<CusExitHeader>
{
	public void TestCusExitReports() => AssertType<CusExitReportCollection<CusExitReport>>(exitHeader.CusExitReports);

	public void TestCusExitConsignments() => AssertType<CusExitConsignmentCollection<CusExitConsignment>>(exitHeader.CusExitConsignments);

	public void TestCusExitContainers() => AssertType<CusExitContainerCollection<CusExitContainer>>(exitHeader.CusExitContainers);

	public void TestCusExitConsignmentPackages() => AssertType<CusExitConsignmentPackageCollection<CusExitConsignmentPackage>>(exitHeader.CusExitConsignmentPackages);

	public void TestIsUCC6() => AssertEquals(true, Factory.New<CusExitHeader>().IsUCC6);

	public void TestTrainingEntryPersistance()
	{
		var filterQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, "PL_ExitControl_TrainingEntry");

		var header = Factory.NewWithValidTestData<CusExitHeader>();
		header.TrainingEntry = true;

		CombineAssertions(() =>
		{
			AssertEquals("TrainingEntry", true, header.TrainingEntry);
			AssertNotNull("TrainingEntry is persisted in GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));
		});
	}

	public void TestStoringFlagPersistance()
	{
		var filterQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, "PL_ExitControl_StoringFlag");

		var header = Factory.NewWithValidTestData<CusExitHeader>();
		header.StoringFlag = true;

		CombineAssertions(() =>
		{
			AssertEquals("StoringFlag", true, header.StoringFlag);
			AssertNotNull("StoringFlag is persisted in GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));
		});
	}

	public void TestStoringFlag_GenAddOnColumnAndDefault()
	{
		var header = Factory.NewWithValidTestData<CusExitHeader>();

		CombineAssertions(() =>
		{
			AssertEquals("StoringFlag should be true by default", true, header.StoringFlag);

			header.StoringFlag = false;
			AssertEquals("StoringFlag should be false when set", false, header.StoringFlag);
		});
	}
}
