using System.IO;
using System.Text;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.AccumulativeAmendment
{
	public static class AccumulativeAmendmentManager
	{
		public static void CreateNewSnapshot(CusEntryHeader entry, string messageType, Stream snapshotStream, int version = 1, bool useUnicode = false)
		{
			var snapshot = entry.Snapshots.AddNew(messageType);
			snapshot.CES_VersionNumber = (ZShort)version;
			snapshot.SetCES_SnapshotXmlSource(useUnicode ? new TextReaderSource(snapshotStream, Encoding.Unicode) : new TextReaderSource(snapshotStream));
		}

		public static void AcceptCurrentSnapshot(CusEntryHeader entry, string messageType)
		{
			var snapshot = entry.Snapshots.GetLatestSnapshotIn(messageType, EntrySnapshotStatus.Current);
			if (snapshot != null)
			{
				snapshot.CES_Status = EntrySnapshotStatus.Lodged;
			}
		}

		public static void DeleteCurrentSnapshot(CusEntryHeader entry, string messageType)
		{
			var snapshot = entry.Snapshots.GetLatestSnapshotIn(messageType, EntrySnapshotStatus.Current);
			if (snapshot != null)
			{
				snapshot.Delete();
			}
		}

		public static void DeleteLatestLodgedSnapshot(CusEntryHeader entry, string messageType)
		{
			entry.Snapshots.GetLatestSnapshotIn(messageType, EntrySnapshotStatus.Lodged)?.Delete();
		}

		public static void MarkLodgedSnapshotAsDeleted(CusEntryHeader entry, string messageType)
		{
			var snapshot = entry.Snapshots.GetLatestSnapshotIn(messageType, EntrySnapshotStatus.Lodged);
			if (snapshot != null)
			{
				snapshot.CES_Status = EntrySnapshotStatus.Deleted;
				entry.Snapshots.Load();
			}
		}
	}
}
