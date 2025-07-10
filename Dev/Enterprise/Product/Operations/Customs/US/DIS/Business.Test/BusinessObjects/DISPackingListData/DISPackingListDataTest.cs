using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(DISPackingListData))]
	sealed class DISPackingListDataTest : XmlSerializableNonPersistentBusinessObjectTest<DISPackingListData>
	{
		protected override BusinessObject GetNewBusinessObject() => new DISPackingListData(Factory);
	}
}
