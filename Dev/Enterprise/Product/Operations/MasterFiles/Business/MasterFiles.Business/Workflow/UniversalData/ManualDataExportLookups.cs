using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.UniversalData
{
	public class ManualDataExportLookups : ZLookups
	{
		public ManualDataExportLookups(ManualDataExport manualDataExport)
			: base(manualDataExport)
		{
			this.manualDataExport = Argument.NotNull(manualDataExport, "ManualDataExport manualDataExport");
		}

		readonly ManualDataExport manualDataExport;

		#region RecipientTypeList

		public CodeDescriptionPairList RecipientTypeList
		{
			get { return Factory.GetCachedValue("RecipientTypeList", GetRecipientTypeList); }
		}

		CodeDescriptionPairList GetRecipientTypeList()
		{
			var workflowDescriptor = manualDataExport.WorkflowDescriptor;
			return WorkflowListHelper.GetRecipientTypeList(workflowDescriptor, manualDataExport.ActionType);
		}

		#endregion

		#region RecipientServices

		public CodeDescriptionPairList RecipientServices
		{
			get { return manualDataExport.IsRecipientServiceAvailable ? WorkflowListHelper.GetCachedTriggerPartyServiceList(Factory, manualDataExport.WorkflowDescriptor, manualDataExport.RecipientType) : new CodeDescriptionPairList(); }
		}

		#endregion

		public CodeDescriptionPairList AlternateRecipientPartyTypeList
		{
			get { return MessageRecipientPartyTypeList.GetAlternateMessageRecipientPartyTypeList(Factory); }
		}

		public OrgHeaderCollection Recipients
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public CodeDescriptionPairList EventCodeList
		{
			get
			{
				var result = TriggerConditionsViewModelLookups.GetDefaultEventTypes(Factory);
				EventTypeListProvider.InsertInSortOrderByDescription(result, true);

				// Customizable events should be at the end of the list
				var customizables = Factory.GetCachedValue("StmCustomizableEventCodeDescriptionPairList", () => new StmCustomizableEventCodeDescriptionPairList(Factory));

				result.AddRange(customizables);

				return result;
			}
		}

		public CodeDescriptionPairList PurposeCodeList
		{
			get { return WorkflowListHelper.GetCachedPurposeCodeList(Factory); }
		}
	}
}
