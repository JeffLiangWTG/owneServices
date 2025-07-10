using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using VatConstants = Enterprise.Customs.NO.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(LandedCostOnlyConfiguration))]
public sealed class LandedCostOnlyConfigurationTest : TestCaseWithFactory
{
	public void TestGetIsLandedCostingOnlyFuncForEntryHeader()
	{
		var callback = landedCostOnlyConfiguration.GetIsLandedCostingOnlyFuncForEntryHeader();
		CombineAssertions(() =>
		{
			AssertEquals("importer is NOT MVA-registered and fee code is empty", expected: false, callback(entryHeader, string.Empty));
			AssertEquals("importer is NOT MVA-registered and fee code is 'DTY'", expected: false, callback(entryHeader, Constants.RateTypes.Duty));
			AssertEquals("importer is NOT MVA-registered and fee code is 'MV1'", expected: false, callback(entryHeader, VatConstants.RefCusTaxOrFee.MV1));
			AssertEquals("importer is NOT MVA-registered and fee code is 'MV2'", expected: false, callback(entryHeader, VatConstants.RefCusTaxOrFee.MV2));

			importer.AsMVARegistered();
			AssertEquals("importer is MVA-registered and fee code is empty", expected: false, callback(entryHeader, string.Empty));
			AssertEquals("importer is MVA-registered and fee code is 'DTY'", expected: false, callback(entryHeader, Constants.RateTypes.Duty));
			AssertEquals("importer is MVA-registered and fee code is 'MV1'", expected: false, callback(entryHeader, VatConstants.RefCusTaxOrFee.MV1));
			AssertEquals("importer is MVA-registered and fee code is 'MV2'", expected: false, callback(entryHeader, VatConstants.RefCusTaxOrFee.MV2));
		});
	}

	public void TestGetIsLandedCostingOnlyFuncForEntryLine()
	{
		var callback = landedCostOnlyConfiguration.GetIsLandedCostingOnlyFuncForEntryLine();
		CombineAssertions(() =>
		{
			AssertEquals("importer is NOT MVA-registered and fee code is empty", expected: false, callback(entryLine, string.Empty));
			AssertEquals("importer is NOT MVA-registered and fee code is 'DTY'", expected: false, callback(entryLine, Constants.RateTypes.Duty));
			AssertEquals("importer is NOT MVA-registered and fee code is 'MV1'", expected: false, callback(entryLine, VatConstants.RefCusTaxOrFee.MV1));
			AssertEquals("importer is NOT MVA-registered and fee code is 'MV2'", expected: false, callback(entryLine, VatConstants.RefCusTaxOrFee.MV2));

			importer.AsMVARegistered();
			AssertEquals("importer is MVA-registered and fee code is empty", expected: false, callback(entryLine, string.Empty));
			AssertEquals("importer is MVA-registered and fee code is 'DTY'", expected: false, callback(entryLine, Constants.RateTypes.Duty));
			AssertEquals("importer is MVA-registered and fee code is 'MV1'", expected: true, callback(entryLine, VatConstants.RefCusTaxOrFee.MV1));
			AssertEquals("importer is MVA-registered and fee code is 'MV2'", expected: true, callback(entryLine, VatConstants.RefCusTaxOrFee.MV2));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		landedCostOnlyConfiguration = new LandedCostOnlyConfiguration();
		declaration = Factory.New<JobDeclaration>();
		importer = Factory.NewWithValidTestData<OrgHeader>();
		declaration.JE_OH_Importer = importer.PK;
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.AllEntryLines.AddNew();
	}

	LandedCostOnlyConfiguration landedCostOnlyConfiguration;
	JobDeclaration declaration;
	OrgHeader importer;
	CusEntryHeader entryHeader;
	CusEntryLine entryLine;
}
