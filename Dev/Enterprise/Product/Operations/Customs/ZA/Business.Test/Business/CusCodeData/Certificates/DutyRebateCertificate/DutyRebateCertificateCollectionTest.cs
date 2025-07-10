using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(DutyRebateCertificateCollection))]
	sealed class DutyRebateCertificateCollectionTest : CusCodeDataCollectionTest<DutyRebateCertificate>
	{
		protected override CusCodeDataCollection<DutyRebateCertificate> GetCusCodeDataCollection() => Factory.New<JobDeclaration>().CustomsEntryInstructions.AddNew().DutyRebateCertificates;

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<DutyRebateCertificate>();
	}
}
