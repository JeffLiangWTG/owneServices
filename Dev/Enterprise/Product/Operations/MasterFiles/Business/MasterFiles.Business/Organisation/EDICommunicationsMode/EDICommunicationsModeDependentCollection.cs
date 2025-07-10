using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class EDICommunicationsModeDependentCollection : DependentBusinessObjectCollection<EDICommunicationsMode, OrgHeader>
	{
		public EDICommunicationsModeDependentCollection(OrgHeader parentHeader)
			: base(parentHeader)
		{
		}

		public ZString ClientSpecificCommunicationTransport
		{
			get { return ClientSpecificMode == null ? ZString.Empty : ClientSpecificMode.EK_CommunicationsTransport; }
			set
			{
				if (ClientSpecificMode == null)
				{
					EDICommunicationsMode clientSpecificMode = AddNew();
					clientSpecificMode.EK_Module = EDICommunicationsMode.Modules.ClientSpecific;
				}
				ClientSpecificMode.EK_CommunicationsTransport = value;
			}
		}

		public ZString ClientSpecificDestination
		{
			get { return ClientSpecificMode == null ? ZString.Empty : ClientSpecificMode.EK_Destination; }
			set
			{
				if (ClientSpecificMode == null)
				{
					EDICommunicationsMode clientSpecificMode = AddNew();
					clientSpecificMode.EK_Module = EDICommunicationsMode.Modules.ClientSpecific;
				}
				ClientSpecificMode.EK_Destination = value;
			}
		}

		#region GetXmlCommunicationMode

		public EDICommunicationsMode GetXmlCommunicationMode(string module)
		{
			return FindAnyByModuleAndFileFormat(module, EDICommunicationsModeFileFormatList.Codes.XML);
		}

		public EDICommunicationsMode GetXmlCommunicationMode(string module, string purpose)
		{
			return FindAnyByModuleAndFileFormat(module, EDICommunicationsModeFileFormatList.Codes.XML, purpose);
		}

		#endregion

		#region FindByModule

		public EDICommunicationsMode[] FindByModule(string module)
		{
			List<EDICommunicationsMode> result = new List<EDICommunicationsMode>();
			foreach (EDICommunicationsMode mode in this)
			{
				if (mode.EK_Module.EqualsIgnoringCase(module))
				{
					result.Add(mode);
				}
			}
			return result.ToArray();
		}

		public EDICommunicationsMode[] FindByModuleAndFileFormat(string module, string fileFormat)
		{
			List<EDICommunicationsMode> result = new List<EDICommunicationsMode>();
			foreach (EDICommunicationsMode mode in this)
			{
				if (mode.EK_Module == module && (mode.EK_FileFormat == EDICommunicationsModeFileFormatList.Codes.All || mode.EK_FileFormat == fileFormat))
				{
					result.Add(mode);
				}
			}
			return result.ToArray();
		}

		public EDICommunicationsMode[] FindModes(EDICommunicationModeQuery modeQuery)
		{
			var triggerFilter = modeQuery.Parent != null
				? new Func<EDICommunicationsMode, bool>(mode => TriggerConditionEvaluator.DoesReferenceConditionMatch(modeQuery.Descriptor, modeQuery.Parent, modeQuery.EventReference, mode.EK_EventReferenceConditionType, mode.EK_EventReferenceConditionValue))
				: mode => mode.EK_EventReferenceConditionType.IsEmpty;

			var transportMode = new ZString((modeQuery.Parent as IDocumentSupportable)?.DocumentSupporter?.TransportMode ?? string.Empty);
			return this.Cast<EDICommunicationsMode>()
				.Where(mode => mode.EK_Module == modeQuery.Descriptor.Code)
				.Where(mode => mode.IsFileFormatSupported(modeQuery.FileFormat))
				.SearchWithFallback(mode => mode.EK_MessagePurpose, modeQuery.Purpose, ZString.Empty)
				.SearchWithFallback(mode => mode.EK_RecipientRole, modeQuery.RecipientRole, ZString.Empty)
				.SearchWithFallback(mode => mode.EK_TransportMode, transportMode, ZString.Empty)
				.SearchWithFallback(mode => mode.EK_EventCode, modeQuery.EventCode, ZString.Empty)
				.Where(triggerFilter) // Evaluated last for performance
				.ToArray();
		}

		EDICommunicationsMode FindAnyByModuleAndFileFormat(string module, string fileFormat, string purpose)
		{
			bool withPurpose = true;
			bool exit = false;
			while (!exit)
			{
				foreach (EDICommunicationsMode mode in this)
				{
					if (mode.EK_Module == module && mode.EK_FileFormat == fileFormat && (mode.EK_MessagePurpose == purpose || (!withPurpose && mode.EK_MessagePurpose.IsEmpty)))
					{
						return mode;
					}
				}
				if (withPurpose)
				{
					withPurpose = false;
				}
				else
				{
					exit = true;
				}
			}

			return (fileFormat == EDICommunicationsModeFileFormatList.Codes.All)
				? null :
				FindAnyByModuleAndFileFormat(module, EDICommunicationsModeFileFormatList.Codes.All, purpose);
		}

		EDICommunicationsMode FindAnyByModuleAndFileFormat(string module, string fileFormat)
		{
			foreach (EDICommunicationsMode mode in this)
			{
				if (mode.EK_Module == module && mode.EK_FileFormat == fileFormat)
				{
					return mode;
				}
			}

			return (fileFormat == EDICommunicationsModeFileFormatList.Codes.All)
				? null
				: FindAnyByModuleAndFileFormat(module, EDICommunicationsModeFileFormatList.Codes.All);
		}

		#endregion

		#region Overrides

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return EDICommunicationsModeSchema.EK_ParentID; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(JoinCondition.And, EDICommunicationsModeSchema.EK_Module, SQLComparisonOperator.NotEqual, EDICommunicationsMode.Modules.Shipnet);
			return result;
		}

		#endregion

		#region Implementation

		EDICommunicationsMode ClientSpecificMode
		{
			get
			{
				foreach (EDICommunicationsMode mode in this)
				{
					if (mode.EK_Module == EDICommunicationsMode.Modules.ClientSpecific)
					{
						return mode;
					}
				}
				return null;
			}
		}

		#endregion
	}
	public class EDICommunicationModeQuery
	{
		public EDICommunicationModeQuery(BusinessObject parent, WorkflowDescriptor descriptor, ZString fileFormat, ZString purpose, ZString recipientRole, ZString eventCode, ZString eventReference)
		{
			Parent = parent;
			Descriptor = descriptor;
			FileFormat = fileFormat;
			Purpose = purpose;
			RecipientRole = recipientRole;
			EventCode = eventCode;
			EventReference = eventReference;
		}

		public ZString FileFormat { get; set; }
		public ZString Purpose { get; set; }
		public ZString RecipientRole { get; set; }
		public ZString EventCode { get; set; }
		public ZString EventReference { get; set; }
		public BusinessObject Parent { get; set; }
		public WorkflowDescriptor Descriptor { get; set; }

		internal static EDICommunicationModeQuery FromAction(BusinessObject parent, ProcessTaskNotification action, IStmALog @event, WorkflowDescriptor descriptor, string fileFormat)
		{
			return new EDICommunicationModeQuery(
				parent: parent,
				descriptor: descriptor,
				fileFormat: fileFormat,
				purpose: action.PQ_MessagePurpose,
				recipientRole: action.PQ_Calc_TriggerParty,
				eventCode: @event?.SL_SE_NKEvent ?? ZString.Empty,
				eventReference: @event?.SL_Reference ?? ZString.Empty);
		}
	}
}
