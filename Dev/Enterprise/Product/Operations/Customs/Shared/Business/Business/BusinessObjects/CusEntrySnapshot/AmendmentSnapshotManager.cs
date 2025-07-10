using System.IO;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using EZC = Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business;

public abstract class AmendmentSnapshotManager
{
	public AmendmentSnapshotManager(CusEntryHeader entryHeader, ZString messageType)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		this.messageType = messageType;
	}

	protected readonly CusEntryHeader entryHeader;
	readonly ZString messageType;

	protected CusEntrySnapshot LastSnapshot => lastSnapshot ??= entryHeader.Snapshots.GetLatestSnapshotIn(messageType, EntrySnapshotStatus.Lodged);
	CusEntrySnapshot lastSnapshot;

	protected IXmlImportLogger Logger => logger ??= new XmlSessionTracker(new SimpleLogger());
	IXmlImportLogger logger;

	public void CreateNewAndAccept()
	{
		var snapshotXmlStream = TakeSnapshot();
		AccumulativeAmendmentManager.CreateNewSnapshot(entryHeader, messageType, snapshotXmlStream, GetVersionNumber(), true);
		AccumulativeAmendmentManager.AcceptCurrentSnapshot(entryHeader, messageType);
	}

	Stream TakeSnapshot() => TakeSnapshotCore();
	protected abstract Stream TakeSnapshotCore();

	ZInt GetVersionNumber() => GetVersionNumberCore();
	protected abstract ZInt GetVersionNumberCore();

	public bool HasLodgedSnapshot => LastSnapshot is not null;

	public void RevertToLastLodged(SnapshotRevertingStrategy strategy)
	{
		if (LastSnapshot != null)
		{
			RestoreEntryHeader(strategy);
			PreserveAuditLog();
		}

		void PreserveAuditLog()
		{
			var noteDescription = GetNoteDescription();
			var sb = new ZStringBuilder();
			sb.Append((EZC.NoResString)"User: ").AppendLine(GlbStaff.CurrentUser.GS_FullName)
				.Append((EZC.NoResString)"Time: ").AppendLine(ZDateTime.Now.ToLongTimeString())
				.AppendLine().Append(Logger.ToString());
			var declaration = entryHeader.Declaration;
			var note = declaration.GetNotes().AddNew(true, noteDescription, sb.ToString());
			note.ST_GC_RelatedCompany = declaration.JE_GC;
			note.ReadOnly = true;
		}

		string GetNoteDescription() => $"{EntryRestoreLog} - {entryHeader.CH_BGMReference}";
	}

	void RestoreEntryHeader(SnapshotRevertingStrategy strategy) => RestoreEntryHeaderCore(strategy, Logger);

	protected abstract void RestoreEntryHeaderCore(SnapshotRevertingStrategy strategy, IXmlImportLogger logger);

	public void DeleteLatestLodged()
	{
		AccumulativeAmendmentManager.DeleteLatestLodgedSnapshot(entryHeader, messageType);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Log message")]
	const string EntryRestoreLog = "Entry Restore Log";
}

public enum SnapshotRevertingStrategy
{
	Override,
	Skip
}
