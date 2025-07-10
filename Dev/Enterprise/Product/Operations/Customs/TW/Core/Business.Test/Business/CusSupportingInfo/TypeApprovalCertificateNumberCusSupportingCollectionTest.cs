using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TypeApprovalCertificateNumberCusSupportingCollection))]
	sealed class TypeApprovalCertificateNumberCusSupportingCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<TypeApprovalCertificateNumberCusSupporting>
	{
		protected override CusSupportingInfoCollection<TypeApprovalCertificateNumberCusSupporting> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new TypeApprovalCertificateNumberCusSupportingCollection(jobComInvoice);
		}
	}
}
