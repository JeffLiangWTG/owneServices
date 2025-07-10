using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.ExitControl.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(AlternativeEvidence))]
sealed class AlternativeEvidenceTest : Customs.Business.Testing.CusCodeDataTest<AlternativeEvidence>
{
	public void TestAdditionalInfos()
	{
		var alternativeEvidence = Factory.New<AlternativeEvidence>();
		AssertType<AdditionalInfoCollection<AdditionalInfo>>(alternativeEvidence.AdditionalInfos);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return GetNewBusinessObjectForDeleteTest(Factory);
	}

	protected override IEnumerable<AlternativeEvidence> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		=> [(AlternativeEvidence)GetNewBusinessObjectForDeleteTest(factory)];

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var header = factory.NewWithValidTestData<CusExitHeader>();
		var consignment = header.CusExitConsignments.AddNew();
		var report = header.CusExitReports.AddNew();
		report.CER_CXC_Consignment = consignment.PK;
		report.CER_DateTime = ZDateTimeOffset.Now;
		report.CER_OfficeOfExit = "DE001";
		return report.AlternativeEvidences.AddNew();
	}
}
