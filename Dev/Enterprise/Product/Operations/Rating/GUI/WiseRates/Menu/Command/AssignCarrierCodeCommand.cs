using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
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
	public class AssignCarrierCodeCommand : IWiseRatesCommand
	{
		#region IWiseRatesCommand

		bool IWiseRatesCommand.Assign(WiseEntryView wiseEntryView, object sender)
		{
			var (assigned, _) = Assign(wiseEntryView);
			return assigned;
		}

		public (bool, OrgHeader) Assign(WiseEntryView wiseEntryView)
		{
			var carrier = wiseEntryView?.RefCarrier;
			return carrier == null
				? (false, null)
				: Assign(carrier.SCACCode, carrier.C1Code);
		}

		public (bool, OrgHeader) Assign(string scac, string c1c)
		{
			OrgHeader carrierOrg = null;
			var carrierModified = WiseRatesGUIHelper.EnhanceUserPermissionToRunAction
			(
				permissionName: string.Empty,
				(securityCore) => securityCore.OrganisationModify,
				needsConfirmation: false,
				confirmationCaption: string.Empty,
				() =>
				{
					var factory = new BusinessObjectFactory();

					var (shippingLine, assigningCode) = GetShippingLine(factory, scac, c1c);
					if (shippingLine == null)
					{
						return false;
					}

					carrierOrg = SelectCarrierOrg(factory);
					if (carrierOrg == null)
					{
						return false;
					}

					return AssignShippingLine(factory, carrierOrg, assigningCode, shippingLine.PK);
				}
			);

			return (carrierModified, carrierOrg);
		}

		bool IWiseRatesCommand.IsEnabled(WiseEntryView wiseEntryView)
		{
			return !string.IsNullOrWhiteSpace(GetSCACOrC1C(wiseEntryView))
				&& wiseEntryView.TI_OH_TransportProvider.IsEmpty;
		}

		#endregion

		protected virtual OrgHeader SelectCarrierOrg(BusinessObjectFactory factory)
		{
			var pickerCollection = new ShippingProviderCollection(factory);

			var filterDefaults = ((IFilterBusinessObjectDefaultsProvider)pickerCollection).FilterBusinessObjectDefaults;
			filterDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", (ZString)OrgConstants.FilterControl.SecondaryOrgType.ShippingLine));
			filterDefaults.Add(new FilterBusinessObjectDefault("Shipping Line", nameof(ModuleTextFilter.ComparisonOperator), new ZString(ModuleTextFilter.ComparisonConstants.IsBlank)));

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Organisation))
			{
				var provider = new CarrierOrgWithoutShippingLineSingleSelect(pickerCollection);
				module.OverrideModuleDecisionProvider(provider);

				var popup = new EmbeddedModulePopup(module, null, true);
				popup.RequireAtLeastOneItemToBeSelected = true;
				provider.Popup = popup;

				ZFormModaliser.ShowDialogAndDispose(popup);

				return provider.SelectedOrg;
			}
		}

		static (RefShippingLine, string) GetShippingLine(BusinessObjectFactory factory, string scac, string c1c)
		{
			var assigningCode = string.Empty;
			RefShippingLine matchedShippingLine = null;

			if (!string.IsNullOrWhiteSpace(scac))
			{
				assigningCode = scac;
				matchedShippingLine = factory.LoadFromNaturalKey<RefShippingLine>(RefShippingLineSchema.RSL_StandardCarrierAlphaCode, scac);
			}

			if (matchedShippingLine == null && !string.IsNullOrWhiteSpace(c1c))
			{
				assigningCode = c1c;
				matchedShippingLine = factory.LoadFromNaturalKey<RefShippingLine>(RefShippingLineSchema.RSL_CargoWiseOneCode, c1c);
			}

			if (matchedShippingLine != null)
			{
				return (matchedShippingLine, assigningCode);
			}

			Globals.Message.ShowWarning(Res.GetString(
				"c5ec5fef-d703-4dfd-9d8b-5ea2828f79e6",
				"A Shipping Line with SCAC/C1C '{0}' could not be found. Please raise eRequest for support.",
				new[] { scac, c1c }.Where(x => !string.IsNullOrWhiteSpace(x)).ToStringWithDelimiterBetweenStrings("/")));

			return (null, null);
		}

		static bool AssignShippingLine(BusinessObjectFactory factory, OrgHeader org, string assigningCode, ZGuid matchedShippingLine)
		{
			var msg = Res.GetString("91dfdf4f-e55d-4508-aed8-abff043217f1", "During the operation SCAC/C1C {0} will be assigned to the carrier {1}. Do you want to proceed?", assigningCode, org.OH_Code);
			if (DialogResult.Yes != Globals.Message.Show(msg, "", MessageBoxButtons.YesNo, DialogResult.Yes))
			{
				return false;
			}

			var orgForEdit = (OrgHeader)factory.ImportFromAnotherFactory(org);
			orgForEdit.OH_RSL_ShippingLine = matchedShippingLine;
			factory.Save();

			Globals.Message.Show(Res.GetString("9dc84423-97fd-4e17-9207-41d73f721baa", "SCAC/C1C {0} is now assigned to Carrier {1}.", assigningCode, org.OH_Code));
			return true;
		}

		public static string GetSCACOrC1C(WiseEntryView rateEntry) => rateEntry?.RefCarrier?.SCACCode ?? rateEntry?.RefCarrier?.C1Code;

		public class CarrierOrgWithoutShippingLineSingleSelect : SingleSelectModuleDecisionProvider, IFilterModuleExtraNotificationProvider
		{
			public CarrierOrgWithoutShippingLineSingleSelect(IBusinessObjectCollection list)
				: base(list)
			{
			}

			public OrgHeader SelectedOrg => (OrgHeader)SelectedBusinessObject;

			public INotification GetExtraNotification(BusinessObject businessObject)
			{
				var org = (OrgHeader)businessObject;
				var errorMessage = GetValidationErrorMessage(org);
				return string.IsNullOrEmpty(errorMessage)
					? null
					: new Notification(CargoWise.ComponentModel.NotificationType.Error, errorMessage);
			}

			static string GetValidationErrorMessage(OrgHeader org)
			{
				Argument.NotNull(org, nameof(org));

				if (!org.OH_IsShippingProvider)
				{
					return Res.GetString("435C32A2-8228-402B-849D-F222B07C898C", "Organization must have type of Carrier.");
				}

				return org.ShippingLine == null
					? string.Empty
					: Res.GetString("e991ad2a-f8aa-42d4-9d1d-8f0f2e9c26f2", "The carrier {0} already has a shipping line.", org.OH_Code);
			}
		}
	}
}
