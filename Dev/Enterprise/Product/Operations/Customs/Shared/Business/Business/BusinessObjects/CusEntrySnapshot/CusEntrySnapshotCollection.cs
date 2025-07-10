using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.BusinessObjects.Interfaces;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusEntrySnapshotCollection<TCusEntrySnapshot> : DependentBusinessObjectCollection<TCusEntrySnapshot, CusEntryHeader>, ICusEntrySnapshotCollection<TCusEntrySnapshot>
		where TCusEntrySnapshot : CusEntrySnapshot
	{
		public CusEntrySnapshotCollection(CusEntryHeader entry) : base(entry, new ZQuery(CusEntrySnapshotSchema.CES_Status, SQLComparisonOperator.NotEqual, AccumulativeAmendment.EntrySnapshotStatus.Deleted))
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public TCusEntrySnapshot AddNew(ZString messageType)
		{
			var result = AddNew();
			result.CES_MessageType = messageType;
			result.CES_Status = AccumulativeAmendment.EntrySnapshotStatus.Current;
			result.CES_VersionNumber = (GetLatestSnapshotIn(messageType, AccumulativeAmendment.EntrySnapshotStatus.Lodged)?.CES_VersionNumber ?? ZShort.Zero) + 1;
			return result;
		}

		public TCusEntrySnapshot GetLatestSnapshotIn(ZString messageType, ZString status)
		{
			return this.Cast<TCusEntrySnapshot>().Where(x => x.CES_MessageType == messageType && (status.IsEmpty || x.CES_Status == status)).OrderByDescending(x => x.CES_SystemCreateTimeUtc).FirstOrDefault();
		}

		public TCusEntrySnapshot GetLatestSnapshotIn(ZString messageType)
		{
			return GetLatestSnapshotIn(messageType, ZString.Empty);
		}

		public TCusEntrySnapshot GetFirstSnapshotIn(ZString messageType, ZString status)
		{
			return this.Cast<TCusEntrySnapshot>().Where(x => x.CES_MessageType == messageType && x.CES_Status == status).OrderBy(x => x.CES_SystemCreateTimeUtc).FirstOrDefault();
		}

		public bool DoesSnapshotExist(ZString messageType) => this.Cast<CusEntrySnapshot>().Any(x => x.CES_MessageType == messageType && x.CES_Status == AccumulativeAmendment.EntrySnapshotStatus.Lodged);

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusEntrySnapshotSchema.CES_CH_EntryHeader;

		public IEnumerable<TCusEntrySnapshot> ElementsAsEnumerable => Elements.Cast<TCusEntrySnapshot>();

		public IEnumerator<TCusEntrySnapshot> GetEnumerator() => ElementsAsEnumerable.GetEnumerator();
	}
}
