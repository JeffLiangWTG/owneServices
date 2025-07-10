using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CertificateOfOriginCusSupportingCollection))]
	sealed class CertificateOfOriginCusSupportingCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<CertificateOfOriginCusSupporting>
	{
		protected override Customs.Business.CusSupportingInfoCollection<CertificateOfOriginCusSupporting> GetCusSupportingInfoCollection()
		{
			var invoiceLine = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new CertificateOfOriginCusSupportingCollection(invoiceLine);
		}
	}
}
