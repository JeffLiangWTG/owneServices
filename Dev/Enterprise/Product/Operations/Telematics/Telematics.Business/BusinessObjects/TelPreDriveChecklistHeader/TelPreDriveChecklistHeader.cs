using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Telematics.Business
{
	public class TelPreDriveChecklistHeader : AutoTelPreDriveChecklistHeader
	{
		public TelPreDriveChecklistHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			lazyEntries = new Lazy<TelPreDriveChecklistEntryCollection>(
				() => new TelPreDriveChecklistEntryCollection(Factory, new ZQuery(TelPreDriveChecklistEntrySchema.TPE_TPH_ChecklistHeader, PK)));
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore => Res.GetString("2EA5CA46-70B5-4F14-9AC2-AF452401AF1C", "{0} Checklist: {1} - {2}", TPH_Type, TPH_SystemCreateUser, TPH_SystemCreateTimeUtc);

		public TelPreDriveChecklistEntryCollection Entries => lazyEntries.Value;
		readonly Lazy<TelPreDriveChecklistEntryCollection> lazyEntries;

		public ZBool AllEntriesCompleted => Entries.All(entry => entry.TPE_IsAgreed == TelPreDriveChecklistEntryValueTypes.Codes.Yes);

		[ReadOnly(true)]
		public ZDateTime CompletionTimeLocal => TPH_ChecklistCreateTimeUtc.ToLocalBranchTime(Factory);

		[ReadOnly(true)]
		public override ZString TPH_SystemCreateUser
		{
			get => base.TPH_SystemCreateUser;
			set => base.TPH_SystemCreateUser = value;
		}

		[ReadOnly(true)]
		public override ZDateTime TPH_SystemCreateTimeUtc
		{
			get => base.TPH_SystemCreateTimeUtc;
			set => base.TPH_SystemCreateTimeUtc = value;
		}
	}
}
