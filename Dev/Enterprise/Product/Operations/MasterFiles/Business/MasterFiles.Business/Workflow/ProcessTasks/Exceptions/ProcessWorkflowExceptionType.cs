using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessWorkflowExceptionType : AutoProcessWorkflowExceptionType, IAuditParent
	{
		#region System Exception Types

		public const string ExceptionScheduledActionMissedCheckedDaily = "EXC";
		public const string ExceptionDamageLostGoods = "EXD";
		public const string ExceptionFutureEvent = "EXE";
		public const string ExceptionWorkflowTimeExpired = "EXF";
		public const string ExceptionCustomsHeldGoods = "EXH";
		public const string ExceptionFlightVoyageMissed = "EXM";
		public const string ExceptionContainerStorage = "EXS";
		public const string ExceptionContainerDetention = "EXT";
		public const string ExceptionDataConversionIssue = "EXV";
		public const string ExceptionMiscellaneous = "EXZ";

		#endregion

		public ProcessWorkflowExceptionType(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ReadOnlyMember(nameof(WET_IsSystem))]
		public override ZString WET_Code { get => base.WET_Code; set => base.WET_Code = value; }

		[ResourceStringData("ProcessWorkflowExceptionType|WET_Description", Caption = "Description")]
		[TranslatableDataField(Schema.TableName, Schema.WET_Description, @"Database\Odyssey\Data\Public\ExceptionTypes\ExceptionTypes.xml", MaxLength = Schema.WET_DescriptionMaxLength, Type = typeof(ProcessWorkflowExceptionType), SecurityCheckpoint = "EventsWorkflowExceptionTypesEdit", Asmid = ResString.AssemblyId)]
		[ReadOnlyMember(nameof(WET_IsSystem))]
		public override ZString WET_Description { get => base.WET_Description; set => base.WET_Description = value; }

		[ResourceStringData("ProcessWorkflowExceptionType|WET_DescriptionMultilingual", Caption = "Description")]
		public MultilingualString WET_DescriptionMultilingual => GetMultilingual(WET_DescriptionInfo);

		protected override ZString HumanReadableNameCore => Res.GetString("10221371-444F-4F72-BD8A-10822A3ADEF5", "Exception Type");

		[ReadOnlyMember(nameof(WET_IsSystem))]
		public override ZBool WET_IsActive { get => base.WET_IsActive; set => base.WET_IsActive = value; }

		[ReadOnlyMember(nameof(WET_IsSystem))]
		public override ZString WET_JobType { get => base.WET_JobType; set => base.WET_JobType = value; }

		public ProcessWorkflowExceptionType[] Types
		{
			get
			{
				if (types == null)
				{
					types = Factory.Load<ProcessWorkflowExceptionType>(new ZQuery());
				}

				return types;
			}
		}
		ProcessWorkflowExceptionType[] types;

		[ChildEditable]
		public ProcessWorkflowExceptionCauseDependentCollection Causes
		{
			get
			{
				if (causes == null)
				{
					causes = !PK.IsValid ?
						new ProcessWorkflowExceptionCauseDependentCollection(this) :
						new ProcessWorkflowExceptionCauseDependentCollection(this, new ZQuery(ProcessWorkflowExceptionCauseSchema.WEC_WET_Type, PK));

					if (PK.IsValid)
					{
						causes.Load();
					}

					RegisterEditableChildObject(causes);
				}

				return causes;
			}
		}
		ProcessWorkflowExceptionCauseDependentCollection causes;

		[ChildEditable]
		public ProcessWorkflowExceptionResolutionDependentCollection Resolutions
		{
			get
			{
				if (resolutions == null)
				{
					resolutions = !PK.IsValid ?
						new ProcessWorkflowExceptionResolutionDependentCollection(this) :
						new ProcessWorkflowExceptionResolutionDependentCollection(this, new ZQuery(ProcessWorkflowExceptionResolutionSchema.WER_WET_Type, PK));

					if (PK.IsValid)
					{
						resolutions.Load();
					}

					RegisterEditableChildObject(resolutions);
				}

				return resolutions;
			}
		}
		ProcessWorkflowExceptionResolutionDependentCollection resolutions;

		IEnumerable<AuditChildInfo> IAuditParent.RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(ProcessWorkflowExceptionCauseSchema.WEC_WET_Type, ProcessWorkflowExceptionCauseSchema.WEC_Code);
				yield return new AuditChildInfo(ProcessWorkflowExceptionResolutionSchema.WER_WET_Type, ProcessWorkflowExceptionResolutionSchema.WER_Code);
			}
		}
	}
}
