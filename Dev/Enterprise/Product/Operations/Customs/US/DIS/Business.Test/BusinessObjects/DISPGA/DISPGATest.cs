using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(DISPGA))]
	class DISPGATest : XmlSerializableNonPersistentBusinessObjectTest<DISPGA>
	{
		protected override BusinessObject GetNewBusinessObject() => new DISPGA(Factory);
	}
}
