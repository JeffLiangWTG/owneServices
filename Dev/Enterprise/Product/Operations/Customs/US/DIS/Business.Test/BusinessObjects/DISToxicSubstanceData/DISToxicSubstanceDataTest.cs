using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(DISToxicSubstanceData))]
	sealed class DISToxicSubstanceDataTest : XmlSerializableNonPersistentBusinessObjectTest<DISToxicSubstanceData>
	{
		protected override BusinessObject GetNewBusinessObject() => new DISToxicSubstanceData(Factory);
	}
}
