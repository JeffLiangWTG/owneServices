using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin
{
	[TestedType(typeof(AgreementInfo))]
	sealed class AgreementInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AgreementInfo()
			{
				VersionNo = 1,
				HasBeenAcknowledged = false
			};
		}
	}
}
