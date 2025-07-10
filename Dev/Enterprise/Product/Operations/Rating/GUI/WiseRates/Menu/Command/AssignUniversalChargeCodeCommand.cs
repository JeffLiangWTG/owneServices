using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.GUI
{
	public class AssignUniversalChargeCodeCommand : IWiseRatesCommand
	{
		public AssignUniversalChargeCodeCommand(bool isGlobalChargeCode = default)
		{
			this.isGlobalChargeCode = isGlobalChargeCode;
		}

		readonly bool isGlobalChargeCode;

		#region IWiseRatesCommand

		bool IWiseRatesCommand.Assign(WiseEntryView wiseEntryView, object sender)
		{
			var menuItem = (MenuItem)sender;
			var universalCode = (string)menuItem.Tag;
			return Assign(universalCode);
		}

		public bool Assign(string universalChargeCode)
		{
			if (!SecurityCheck(isGlobalChargeCode))
			{
				return false;
			}

			var selected = SelectChargeCode(isGlobalChargeCode);
			return selected is not null && AssignUniversalCode(selected, universalChargeCode);
		}

		bool IWiseRatesCommand.IsEnabled(WiseEntryView wiseEntryView)
		{
			var unmappedCodes = AssignUniversalChargeCodeCommand.UnmappedUniversalChargeCodes(wiseEntryView).ToList();
			return unmappedCodes.Count > 0;
		}

		#endregion

		public bool AssignUniversalCode(AccChargeCode chargeCode, string universalCode)
		{
			if (!Validate(chargeCode) ||
				!SecurityCheck(chargeCode) ||
				!Confirm(chargeCode, universalCode))
			{
				return false;
			}

			var factory = new BusinessObjectFactory();
			var chargeCodeForEdit = (AccChargeCode)factory.ImportFromAnotherFactory(chargeCode);
			var mapping = chargeCodeForEdit.UniversalChargeCodeMappingsCollection.AddNew();
			mapping.AUP_Code = universalCode;
			factory.Save();
			chargeCode.Reload();

			Globals.Message.Show(GetUniversalNowAssignedToLocalMsg(chargeCode, universalCode));

			return true;
		}

		AccChargeCode SelectChargeCode(bool isGlobal)
		{
			var moduleId = isGlobal ? ModuleIDs.AccGlobalChargeCode : ModuleIDs.AccChargeCode;

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(moduleId))
			{
				var provider = new UnmappedChargeCodeSingleSelect();
				module.OverrideModuleDecisionProvider(provider);

				var filterDefaults = ((IFilterBusinessObjectDefaultsProvider)module.GridCollection).FilterBusinessObjectDefaults;
				filterDefaults.Add(new FilterBusinessObjectDefault("Universal Charge Code", "ComparisonOperator", new ZString(ModuleTextFilter.ComparisonConstants.IsBlank)));
				module.FilterBusinessObject.SetExternalDefaults(filterDefaults);

				var popup = new EmbeddedModulePopup(module, null, true);
				popup.RequireAtLeastOneItemToBeSelected = true;
				provider.Popup = popup;

				ZFormModaliser.ShowDialogAndDispose(popup);

				return provider.SelectedChargeCode;
			}
		}

		/// <summary>
		/// ModuleDecisionProvider for selecting unmapped Charge Codes.
		/// Adds an error to rows that already have a Universal Charge Code.
		/// </summary>
		public class UnmappedChargeCodeSingleSelect : SingleSelectModuleDecisionProvider
		{
			public AccChargeCode SelectedChargeCode => (AccChargeCode)SelectedBusinessObject;
		}

		static string GetUniversalNowAssignedToLocalMsg(AccChargeCode chargeCode, string universalCode)
			=> Res.GetString("CF546B68-5621-4E1F-A225-D6CF85C531BC", "Universal charge code '{0}' is now assigned to Charge code '{1}'.", universalCode, chargeCode.AC_Code);

		static string GetChargeCodeErrorMsg(AccChargeCode chargeCode)
		{
			return Res.GetString("387f7eb7-599d-4a99-86d8-b5dbf708e650", @"{0}
The invalid charge code '{1}' can be found in {2}.
Please edit the charge code to correct the error shown above.
To highlight errors to be corrected choose ""File"" then ""Validate All"" after editing the charge code(s).", chargeCode.GetErrors().ToUniqueMessageListString(),
chargeCode.AC_Code,
"Maintain > Account >" + (chargeCode.IsGlobal ? (NoResString)" Global" : string.Empty) + " Charge Codes");
		}

		bool Validate(AccChargeCode chargeCode)
		{
			chargeCode.RunPreSaveValidation();

			if (chargeCode.HasErrors)
			{
				Globals.Message.ShowWarning(GetChargeCodeErrorMsg(chargeCode));
				return false;
			}

			return true;
		}

		bool SecurityCheck(AccChargeCode chargeCode)
		{
			var controllerId = chargeCode.IsGlobal ? ControllerIDs.AccGlobalChargeCode : ControllerIDs.AccChargeCode;
			var controller = ZControllerFactory.Create(controllerId);
			var security = controller.GetCheckPointForEdit(chargeCode);
			if (!security.IsAllowed)
			{
				security.ShowError();
				return false;
			}

			return true;
		}

		bool Confirm(AccChargeCode chargeCode, string universalChargeCode)
		{
			var isGlobal = chargeCode.IsGlobal;

			var msg = isGlobal
				? Res.GetString("914DE3F7-780B-4C6D-A8AC-13F2C5999C6B", "During the operation the Universal Charge Code {0} will be assigned to Global Charge Code {1}. Are you sure you want to proceed?", universalChargeCode, chargeCode.AC_Code)
				: Res.GetString("F931F201-C4F8-4FE1-A4E4-1E6BA2FB65DA", "During the operation the Universal Charge Code {0} will be assigned to Charge Code {1}. Are you sure you want to proceed?", universalChargeCode, chargeCode.AC_Code);

			if (DialogResult.Yes != Globals.Message.Show(msg, "", MessageBoxButtons.YesNo, DialogResult.Yes))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Security check before AccChargeCode is selected.
		/// Shows a reason message if check fails.
		/// </summary>
		static bool SecurityCheck(bool isGlobal)
		{
			if (isGlobal)
			{
				var securityCheckpoint = Env.Security.GlobalChargeCodesModify;
				if (!securityCheckpoint.IsAllowed)
				{
					securityCheckpoint.ShowError();
					return false;
				}
			}
			else
			{
				var securityCheckpoint1 = Env.Security.ChargeCodesModify;
				var securityCheckpoint2 = Env.Security.ChargeCodesLTGModify;
				if (!securityCheckpoint1.IsAllowed && !securityCheckpoint2.IsAllowed)
				{
					Env.Security.ShowError(new Security.SecurityCheckpoint[] { securityCheckpoint1, securityCheckpoint2 });
					return false;
				}
			}

			return true;
		}

		public static IEnumerable<string> UnmappedUniversalChargeCodes(WiseEntryView rate)
		{
			if (rate == null)
			{
				return Enumerable.Empty<string>();
			}

			return rate.ChildWiseRateLineViews
				.Cast<WiseLineView>()
				.SelectMany(x => ((INeedCodeMappings)x).UnmappedCodes)
				.Where(x => x.Relationship == Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.ChargeCodes)
				.Select(x => x.ForeignCode)
				.Distinct()
				.OrderBy(x => x);
		}
	}
}
