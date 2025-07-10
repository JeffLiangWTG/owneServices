using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(DISAdditionalNumber))]
	sealed class DISAdditionalNumberIXmlSerializableBusinessObjectTest : XmlSerializableNonPersistentBusinessObjectTest<DISAdditionalNumber>
	{
		protected override BusinessObject GetNewBusinessObject() => new DISAdditionalNumber(Factory);
	}
}
