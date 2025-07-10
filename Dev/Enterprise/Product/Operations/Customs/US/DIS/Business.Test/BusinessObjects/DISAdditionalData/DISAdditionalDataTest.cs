using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(DISAdditionalData))]
	sealed class DISAdditionalDataTest : XmlSerializableNonPersistentBusinessObjectTest<DISAdditionalData>
	{
		protected override BusinessObject GetNewBusinessObject() => new DISAdditionalData(Factory);
	}
}
