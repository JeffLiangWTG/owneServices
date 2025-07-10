using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class DISMessageManagerBaseTest<T> : TestCaseWithFactory where T : XmlSerializableNonPersistentBusinessObject, IDISDocumentBase
	{
		public abstract void TestSendSubmission();

		public abstract void TestSendWithdrawal();

		protected abstract DISHostWrapperBase<T> HostWrapper { get; }
		protected abstract BusinessObject JobDeclaration { get; }
		protected abstract DISMessageManagerBase GetMessageManager(IDISDocumentBase disDocument);
	}
}
