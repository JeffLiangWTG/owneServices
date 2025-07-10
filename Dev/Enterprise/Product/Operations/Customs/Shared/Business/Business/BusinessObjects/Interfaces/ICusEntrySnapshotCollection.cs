using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business.BusinessObjects.Interfaces
{
	public interface ICusEntrySnapshotCollection<out TCusEntrySnapshot> : IBusinessObjectCollection<TCusEntrySnapshot>
		where TCusEntrySnapshot : CusEntrySnapshot
	{
		new TCusEntrySnapshot this[int index] { get; }

		TCusEntrySnapshot AddNew(ZString messageType);
		TCusEntrySnapshot GetLatestSnapshotIn(ZString messageType, ZString status);
		TCusEntrySnapshot GetLatestSnapshotIn(ZString messageType);
		TCusEntrySnapshot GetFirstSnapshotIn(ZString messageType, ZString status);
		bool DoesSnapshotExist(ZString messageType);
	}
}
