using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class DISHostWrapperBaseTest<T1, T2> : NonPersistentBusinessObjectTestCase where T1 : DISHostWrapperBase<T2>
			where T2 : XmlSerializableNonPersistentBusinessObject, IDISDocumentBase
	{
	}
}
