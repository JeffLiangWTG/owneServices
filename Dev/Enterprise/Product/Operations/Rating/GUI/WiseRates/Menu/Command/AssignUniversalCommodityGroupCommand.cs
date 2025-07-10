using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.GUI
{
	public class AssignUniversalCommodityGroupCommand : IWiseRatesCommand
	{
		public AssignUniversalCommodityGroupCommand(ZForm parentForm)
		{
			ParentForm = parentForm;
		}

		public AssignUniversalCommodityGroupCommand()
		{
		}

		readonly ZForm ParentForm;

		public bool ShowConfirmation { get; init; }

		#region IWiseRatesCommand

		bool IWiseRatesCommand.Assign(WiseEntryView wiseEntryView, object sender)
		{
			return Assign(wiseEntryView);
		}

		bool IWiseRatesCommand.IsEnabled(WiseEntryView wiseEntryView) => true;

		#endregion

		public bool Assign(WiseEntryView wiseEntryView)
		{
			var commodityGroup = wiseEntryView?.CommodityGroup ?? ZString.Empty;
			return Assign(commodityGroup);
		}

		public bool Assign(ZString commodityGroup)
		{
			if (IsUniversalCommodityGroupMapped(commodityGroup))
			{
				return true;
			}

			var questionMapping = Res.GetString("2b007c45-d845-4bd9-aed2-f361cd31666a", @"The Universal Commodity Group '{0}' has NOT been assigned to any CW1 Commodity.
Would you like to complete the assignment?", commodityGroup);
			var captionMapping = Res.GetString("ab66990d-1463-47fc-9c4f-76295aa4a36a", "Universal Commodity Group mapping");
			var answerMapping = Globals.Message.Show(questionMapping, captionMapping, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (answerMapping == DialogResult.No)
			{
				return true;
			}

			// this check is redundant (affects performance) when assignment is done correctly through EnhanceUserPermissionToRunAction
			// however, in case this method is called from somewhere else, this check should be a safe guard.
			if (!SecurityCheck())
			{
				return false;
			}

			var selectCommodityCode = SelectCommodityCodeFromModule();
			if (selectCommodityCode == null)
			{
				return false;
			}

			var factory = new BusinessObjectFactory();
			var commodityCodeForEdit = (RefCommodityCode)factory.ImportFromAnotherFactory(selectCommodityCode);
			commodityCodeForEdit.RH_UniversalCommodityGroup = commodityGroup;
			factory.Save();
			selectCommodityCode.Reload();

			if (ShowConfirmation)
			{
				Globals.Message.Show(Res.GetString(
					"08a8e334-4e40-4be9-9599-e69a1ed84626",
					"Universal commodity group '{0}' is now assigned to CW local commodity '{1}'.",
					commodityGroup, commodityCodeForEdit.RH_Code));
			}

			return true;
		}

		bool IsUniversalCommodityGroupMapped(ZString commodityGroup)
		{
			return RefCommodityCodeCollection.Any(x => x.RH_UniversalCommodityGroup.EqualsIgnoringCase(commodityGroup));
		}

		RefCommodityCodeCollection RefCommodityCodeCollection
		{
			get
			{
				if (refCommodityCodeCollection != null)
				{
					return refCommodityCodeCollection;
				}

				refCommodityCodeCollection = new RefCommodityCodeCollection(new BusinessObjectFactory());

				return refCommodityCodeCollection;
			}
		}
		RefCommodityCodeCollection refCommodityCodeCollection;

		RefCommodityCode SelectCommodityCodeFromModule()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.RefCommodityCode))
			{
				var provider = new UnmappedCommodityCodeSingleSelect();
				module.OverrideModuleDecisionProvider(provider);

				var filterBusinessObject = module.FilterBusinessObject;
				filterBusinessObject.AfterLoadModuleFilters += () =>
				{
					var moduleTextFilterBlank = filterBusinessObject.ModuleFilters.AddTextFilter("Universal Commodity Group Blank", RefCommodityCodeSchema.RH_UniversalCommodityGroup);
					moduleTextFilterBlank.MultilingualDescription = RateEntryFilterUtility.Constants.Description.UniversalCommodityGroup;

					var moduleTextFilterNonClassified = filterBusinessObject.ModuleFilters.AddTextFilter("Universal Commodity Group NCLS", RefCommodityCodeSchema.RH_UniversalCommodityGroup);
					moduleTextFilterNonClassified.MultilingualDescription = RateEntryFilterUtility.Constants.Description.UniversalCommodityGroup;
				};

				// IsForwarding = Y
				var filterDefaults = ((IFilterBusinessObjectDefaultsProvider)module.GridCollection).FilterBusinessObjectDefaults;
				filterDefaults.Add(new FilterBusinessObjectDefault("Commodity Type", "Property0", ZBool.True, true));
				// IsShipping = Y
				filterDefaults = ((IFilterBusinessObjectDefaultsProvider)module.GridCollection).FilterBusinessObjectDefaults;
				filterDefaults.Add(new FilterBusinessObjectDefault("Commodity Type", "Property1", ZBool.True, true));
				// Universal Commodity Group IsBlank
				filterDefaults = ((IFilterBusinessObjectDefaultsProvider)module.GridCollection).FilterBusinessObjectDefaults;
				filterDefaults.Add(
					new FilterBusinessObjectDefault(
						"Universal Commodity Group Blank",
						"ComparisonOperator",
						(ZString)ModuleTextFilter.ComparisonConstants.IsBlank,
						FilterOrCategory.Red));
				// Universal Commodity Group StartsWith NCLS
				filterDefaults.Add(
					new FilterBusinessObjectDefault(
						"Universal Commodity Group NCLS",
						"ComparisonOperator",
						(ZString)ModuleTextFilter.ComparisonConstants.StartsWith,
						FilterOrCategory.Red));
				filterDefaults.Add(
					new FilterBusinessObjectDefault(
						"Universal Commodity Group NCLS",
						"Property",
						(ZString)RefCommodityCode.UniversalGroups.NotClassified,
						FilterOrCategory.Red));

				module.FilterBusinessObject.SetExternalDefaults(filterDefaults);
				var popup = new EmbeddedModulePopup(module, null, true);
				popup.RequireAtLeastOneItemToBeSelected = true;
				provider.Popup = popup;

				ZFormModaliser.ShowDialogAndDispose(popup, ParentForm);
	
				return provider.SelectedCommodityCode;
			}
		}

		/// <summary>
		/// ModuleDecisionProvider for selecting unmapped Commodity Code.
		/// Adds an error to rows that already have a Universal Commodity Group.
		/// </summary>
		public class UnmappedCommodityCodeSingleSelect : SingleSelectModuleDecisionProvider, IFilterModuleExtraNotificationProvider
		{
			public RefCommodityCode SelectedCommodityCode => (RefCommodityCode)SelectedBusinessObject;

			public INotification GetExtraNotification(BusinessObject businessObject)
			{
				var commodityCode = (RefCommodityCode)businessObject;
				return !commodityCode.RH_UniversalCommodityGroup.IsEmpty
					? new Notification(
						CargoWise.EntityFramework.NotificationType.Warning,
						Res.GetString("d8c7b0db-703c-4997-a51e-d7406b89dad4", "Commodity has been mapped to a Universal Commodity Group"))
					: null;
			}
		}

		/// <summary>
		/// Security check before RefCommodityCode is selected.
		/// Shows a reason message if check fails.
		/// </summary>
		static bool SecurityCheck()
		{
			var securityCheckpoint = Env.Security.CommodityModify;
			if (securityCheckpoint.IsAllowed)
			{
				return true;
			}

			securityCheckpoint.ShowError();
			return false;
		}
	}
}
