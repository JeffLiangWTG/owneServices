using System;
using System.Data;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class StmChangeLog : AutoStmChangeLog, IStmChangeLog
	{
		public StmChangeLog(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business Object Overrides

		public override void OnSaving()
		{
			base.OnSaving();
			SerializeFieldChanges();
			SetPostedTime();
		}

		void SetPostedTime()
		{
			if (!IsInDatabase)
			{
				SY_PostedTimeUtc = ZDateTime.UtcNow;
			}
		}

		#endregion

		#region Related Business Objects

		public BusinessObject Parent
		{
			get
			{
#if DEBUG
				if (SY_ParentTableCode == DummyBizoSchema.Constants.Prefix)
				{
					return Factory.Load<Testing.DummyWithWorkflow>(SY_ParentID);
				}
#endif
				return Factory.Load(SY_ParentTableCode, SY_ParentID);
			}
		}

		[ChildEditable(false)]
		public StmFieldChangeLogCollection FieldChanges
		{
			get
			{
				if (fieldChanges == null)
				{
					fieldChanges = new StmFieldChangeLogCollection(this);
					DeserializeFieldChanges();
					RegisterEditableChildObject(fieldChanges);
				}
				return fieldChanges;
			}
		}

		StmFieldChangeLogCollection fieldChanges;

		#endregion

		#region Serializing Blob

		void DeserializeFieldChanges()
		{
			ZString[] fieldChanges = SY_Changes.Split('\n');
			foreach (ZString fieldChangeUntrimmed in fieldChanges)
			{
				ZString fieldChange = fieldChangeUntrimmed.Trim();
				if (!fieldChange.IsEmpty)
				{
					StmFieldChangeLog newFieldChangeLog = FieldChanges.AddNew();
					try
					{
						newFieldChangeLog.Parse(fieldChange);
					}
					catch (ZTypeValueException)
					{
						newFieldChangeLog.Delete();
					}
					catch (FormatException)
					{
						newFieldChangeLog.Delete();
					}
				}
			}
		}

		void SerializeFieldChanges()
		{
			StringBuilder changes = new StringBuilder();
			foreach (StmFieldChangeLog fieldChangeLog in FieldChanges)
			{
				fieldChangeLog.Format(changes);
				changes.Append("\n");
			}
			SY_Changes = changes.ToString();
		}

		#endregion

		#region IWorkflowTriggerSource

		ZGuid IIdentified.Identifier => PK;

		ZGuid IWorkflowTriggerSource.ParentID => SY_ParentID;

		ZDateTime IWorkflowTriggerSource.EventTime => SY_PostedTimeUtc.IsValid ? SY_PostedTimeUtc.ToLocalBranchTime(Factory) : ZDateTime.Now;
		ZDateTimeOffset IWorkflowTriggerSource.EventTimeOffset => SY_PostedTimeUtc.IsValid ? new ZDateTimeOffset(SY_PostedTimeUtc.ToLocalBranchTime(Factory)) : ZDateTimeOffset.Now;

		ZString IWorkflowTriggerSource.SourceType => StmChangeLogSchema.Constants.Prefix;

		ZString IWorkflowTriggerSource.Reference => ZString.Empty;

		ZBool IWorkflowTriggerSource.IsEstimate => false;

		ZDateTime IWorkflowTriggerSource.PostedTimeUtc => SY_PostedTimeUtc;

		ZString IWorkflowTriggerSource.DepartmentCode => ZString.Empty;

		ZString IWorkflowTriggerSource.BranchCode => ZString.Empty;

		ZString IWorkflowTriggerSource.CompanyCode => ZString.Empty;

		ZString IWorkflowTriggerSource.FriendlyTableName => ZString.Empty;

		ZDateTime IWorkflowTriggerSource.EventTimeUtc => SY_PostedTimeUtc;

		ZString IWorkflowTriggerSource.Source => ZString.Empty;

		ZBool IWorkflowTriggerSource.IsCancelled => false;

		IPropagationSettings IWorkflowTriggerSource.PropagationSettings { get; } = new DefaultPropagationSettings();

		#endregion

		#region IEventUserContextSource

		ZString IEventUserContextSource.StaffCode => SY_GS_NKUser;

		ZString IEventUserContextSource.UserCode => SY_GS_NKUser;

		#endregion
	}
}
