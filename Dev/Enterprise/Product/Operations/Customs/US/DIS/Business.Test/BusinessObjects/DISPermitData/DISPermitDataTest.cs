using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(DISPermitData))]
	sealed class DISPermitDataTest : XmlSerializableNonPersistentBusinessObjectTest<DISPermitData>
	{
		protected override BusinessObject GetNewBusinessObject() => new DISPermitData(Factory);
	}
}
