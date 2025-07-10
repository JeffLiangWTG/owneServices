using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(DISCertificateData))]
	sealed class DISCertificateDataTest : XmlSerializableNonPersistentBusinessObjectTest<DISCertificateData>
	{
		protected override BusinessObject GetNewBusinessObject() => new DISCertificateData(Factory);
	}
}
