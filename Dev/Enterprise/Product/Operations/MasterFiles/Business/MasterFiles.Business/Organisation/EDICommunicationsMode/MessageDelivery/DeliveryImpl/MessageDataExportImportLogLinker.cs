using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	public class MessageDataExportImportLogLinker : MessageDataLogLinker
	{
		public MessageDataExportImportLogLinker(Event dataExportImportEvent, BusinessObjectFactory factory, ZString triggerPurpose)
			: base(dataExportImportEvent, factory, triggerPurpose)
		{
			if (dataExportImportEvent != Events.DataExport && dataExportImportEvent != Events.DataImport)
			{
				throw new ArgumentException("Only Events.DataExport or Events.DataImport can be accepted as argument.");
			}

			if (factory == null)
			{
				throw new ArgumentException("Argument 'factory' can not be null.");
			}

			if (!triggerPurpose.IsEmpty)
			{
				var triggerPurposeDesc = TriggerPurposeList.GetDescriptionFromCode(triggerPurpose);

				var builder = new ZStringBuilder((NoResString)"Purpose: ");
				builder.Append(triggerPurpose);

				if (!string.IsNullOrEmpty(triggerPurposeDesc))
				{
					builder.Append(" - ");
					builder.Append(triggerPurposeDesc);
				}

				EventReference = builder.ToString();
			}
		}

		public MessageDataExportImportLogLinker(Event dataExportImportEvent, BusinessObjectFactory factory) : this(dataExportImportEvent, factory, ZString.Empty)
		{
		}

		internal CodeDescriptionPairList TriggerPurposeList
		{
			get { return Factory.GetCachedValue("ProcessTaskTriggerPurposeList", ProcessTaskNotificationLookups.GetNewProcessTaskTriggerPurposeList); }
		}
	}
}
