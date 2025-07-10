using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.MasterFiles.GUI;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.GUI
{
	public class CreateAirlineAndAssignIATACodeCommand : IWiseRatesCommand
	{
		readonly bool shouldCreateAirline;
		IDialogService DialogService { get; }

		public CreateAirlineAndAssignIATACodeCommand(bool shouldCreateAirline = false, IDialogService dialogService = null)
		{
			this.shouldCreateAirline = shouldCreateAirline;
			this.Factory = new BusinessObjectFactory();
			this.DialogService = dialogService;
		}

		#region IWiseRatesCommand

		public bool IsEnabled(WiseEntryView wiseEntryView)
		{
			var isEnabled = wiseEntryView != null
				&& wiseEntryView.IsAir()
				&& !string.IsNullOrWhiteSpace(GetRateEntryIATACode(wiseEntryView))
				&& wiseEntryView.TI_OH_TransportProvider.IsEmpty
				&&
				(
					(shouldCreateAirline && GetRateEntryRefAirline(wiseEntryView) == null)
					|| (!shouldCreateAirline && GetRateEntryRefAirline(wiseEntryView) != null)
				);

			return isEnabled;
		}

		public bool Assign(WiseEntryView entry, object sender)
		{
			var assignedOrganization = Assign(entry?.RefCarrier?.IATACode, null);
			return assignedOrganization != null;
		}

		public static OrgHeader AssignWithUserConfirmation(string iataCode, IDialogService dialogService)
		{
			var airline = GetAirline(iataCode);
			var command = new CreateAirlineAndAssignIATACodeCommand(airline == null, dialogService);

			var organizations = command.GetOrganizationsHavingIATACodeAssigned(iataCode, airline);
			if (organizations.Length == 0)
			{
				var msg = Res.GetString("51dfba44-9604-48ce-b1d0-30fda5b9c715", "No Organization is assigned to Airline with '{0}' Two Character Code. Would you like to assign an Organization to Airline '{0}'?", iataCode);
				var caption = Res.GetString("428f6b7b-cffb-4c22-b370-53abdc0a4308", "Carrier mapping");

				var res = Globals.Message.Show(msg, caption, MessageBoxButtons.YesNo, DialogResult.No);
				if (res != DialogResult.Yes)
				{
					return null;
				}
			}

			return command.Assign(iataCode, organizations);
		}

		/// <summary>
		/// Assign an IATA code to an organization and return it.
		/// </summary>
		/// <param name="iataCode">The IATA code for assignment</param>
		/// <param name="organizations">
		/// One of the organizations will have the IATA code assigned. If null the already assigned ones will be loaded.
		/// </param>
		public OrgHeader Assign(string iataCode, OrgHeader[] organizations)
		{
			if (string.IsNullOrWhiteSpace(iataCode))
			{
				return null;
			}

			var (airline, error) = GetAirlineOrErrorIfNotExactlyOneFound(iataCode);
			if (!string.IsNullOrEmpty(error))
			{
				return null;
			}

			// Why check shouldCreateAirline? MMS has 2 context menu items:
			// - Assign IATA Code to Carrier
			// - Create Airline and assign to Carrier for IATA Code
			if (airline == null && shouldCreateAirline)
			{
				airline = CreateOrUpdateRefAirline(null, iataCode);
				if (airline == null)
				{
					return null;
				}
			}

			// Rule: RM_EagleAddedAirlinePrefixOrAccountingCode (3 numeric code) must not be empty.
			while (airline != null && airline.RM_EagleAddedAirlinePrefixOrAccountingCode.IsEmpty)
			{
				var message = Res.GetString("02555f7d-e149-46d0-88f9-7fb99d03aa1e", "Do you wish to edit the Airline record to assign a Numeric Code? If Cancel, Autorating will not continue.", iataCode);
				var result = Globals.Message.Show(message, string.Empty, ZMessageBoxButtons.OKCancel, ZMessageBoxIcon.Warning, ZDialogResult.OK);
				if (result == ZDialogResult.Cancel)
				{
					return null;
				}

				airline = CreateOrUpdateRefAirline(airline);
			}

			if (organizations == null)
			{
				organizations = GetOrganizationsHavingIATACodeAssigned(iataCode, airline);
			}

			if (organizations.Length == 1)
			{
				return organizations[0];
			}

			if (organizations.Length == 0)
			{
				return airline != null ? SelectOrganizationAndAssignIATACode(iataCode, airline) : null;
			}

			var cachedOrgsWithKeys = organizations.ToDictionary(x => $"{x.OH_Code} - {x.OH_FullName}", x => x);

			var prompt = Res.GetString("7cbba203-859d-4c61-9768-efe774789e81",
				"Multiple Organizations have been found assigned with Carrier '{0}'. Please select which Organization to be populated as the Carrier to the job.",
				iataCode);

			var selectedName = DialogService?.SelectSingleOrganization(cachedOrgsWithKeys.Keys, prompt);
			if (selectedName != null)
			{
				return cachedOrgsWithKeys[selectedName];
			}
			else
			{
				return null;
			}
		}

		#endregion

		/// <summary>
		/// Create a new RefAirline when input airline is null. Otherwise, update it.
		/// </summary>
		/// <param name="iataCode">IATA code set to new RefAirline</param>
		RefAirline CreateOrUpdateRefAirline(RefAirline airline = null, string iataCode = null)
		{
			var result = WiseRatesGUIHelper.EnhanceUserPermissionToRunAction
			(
				permissionName: string.Empty,
				(securityCore) => securityCore.RefAirlineModify,
				needsConfirmation: false,
				confirmationCaption: string.Empty,
				() =>
				{
					if (airline == null)
					{
						airline = CreateAirLine();
						airline.RM_TwoCharacterCode = iataCode;
					}

					using (var airlineForm = new RefAirlineForm(airline))
					{
						ZFormModaliser.ShowDialogWithoutDispose(airlineForm);
						return airlineForm.LastSaveSucceeded;
					}
				}
			);

			return airline;
		}

		public static string GetRateEntryIATACode(WiseEntryView rateEntry) => rateEntry?.RefCarrier?.IATACode;

		bool AssignIATACode(OrgHeader org, string iataCode, RefAirline refAirline)
		{
			if (Validate(org) && Confirm(org, iataCode))
			{
				var orgToSave = (OrgHeader)Factory.ImportFromAnotherFactory(org);
				orgToSave.OH_IsAirLine = true;
				orgToSave.MiscServ.OM_RM_Airline = refAirline.PK;
				Factory.Save();
				Globals.Message.Show(Res.GetString("D6F0ADAC-7B0D-4B15-9F21-CDC2D1C76FCC", "IATA Code {0} is now assigned to Carrier {1}.", iataCode, org.OH_Code));

				return true;
			}
			else
			{
				return false;
			}
		}

		OrgHeader SelectOrganizationAndAssignIATACode(string iataCode, RefAirline refAirline)
		{
			OrgHeader selectedOrg = null;
			var isOrgSelectedAndAssigned = WiseRatesGUIHelper.EnhanceUserPermissionToRunAction
			(
				permissionName: string.Empty,
				(securityCore) => securityCore.OrganisationModify,
				needsConfirmation: false,
				confirmationCaption: string.Empty,
				() =>
				{
					selectedOrg = SelectOrganization();
					if (selectedOrg != null)
					{
						return AssignIATACode(selectedOrg, iataCode, refAirline);
					}
					else
					{
						return false;
					}
				}
			);

			if (isOrgSelectedAndAssigned)
			{
				return selectedOrg;
			}
			else
			{
				return null;
			}
		}

		internal OrgHeader[] GetOrganizationsHavingIATACodeAssigned(string iataCode, RefAirline refAirline)
		{
			if (refAirline == null)
			{
				return System.Array.Empty<OrgHeader>();
			}

			var airLineSubQuery = new ZDBOnlySubQuery(typeof(RefAirline), RefAirlineSchema.PK, OrgMiscServSchema.OM_RM_Airline);
			airLineSubQuery.AddToFilter(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, SQLComparisonOperator.NotEqual, ZString.Empty);
			if (iataCode.Length == 2)
			{
				airLineSubQuery.AddToFilter(RefAirlineSchema.RM_TwoCharacterCode, iataCode);
			}
			else if (iataCode.Length == 3)
			{
				airLineSubQuery.AddToFilter(RefAirlineSchema.RM_ThreeLetterCode, iataCode);
			}

			var orgMiscServSubQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH, OrgHeaderSchema.PK);
			orgMiscServSubQuery.AddSubQuery(airLineSubQuery, JoinCondition.And);

			var organizationQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			organizationQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
			organizationQuery.AddSubQuery(orgMiscServSubQuery, JoinCondition.And);

			return Factory.Load<OrgHeader>(organizationQuery);
		}

		bool Validate(OrgHeader org)
		{
			if (!org.OH_IsShippingProvider)
			{
				var msg1 = Res.GetString("CEC4CD69-7AB2-4EA7-A69C-F448415BBD89", "Organization must have type of Carrier.");
				var msg2 = ChooseAgainMsg;
				Globals.Message.ShowError(msg1 + " " + msg2);
				return false;
			}

			if (org.MiscServ.Airline != null)
			{
				var msg1 = OrgAlreadyHasAirlineMsg(org.OH_Code);
				var msg2 = ChooseAgainMsg;
				Globals.Message.ShowError(msg1 + " " + msg2);
				return false;
			}

			return true;
		}

		bool Confirm(OrgHeader org, string iataCode)
		{
			var msg = Res.GetString("C8706F7C-CD2E-4E9C-8BDF-E586589A046C", "During the operation the IATA code {0} will be assigned to the carrier {1}. Are you sure you want to proceed?", iataCode, org.OH_Code);
			return DialogResult.Yes == Globals.Message.Show(msg, "", MessageBoxButtons.YesNo, DialogResult.Yes);
		}

		static RefAirline GetRateEntryRefAirline(WiseEntryView rateEntry)
		{
			var iataCode = GetRateEntryIATACode(rateEntry);
			if (!string.IsNullOrWhiteSpace(iataCode))
			{
				return GetAirline(iataCode);
			}

			return null;
		}

		static RefAirline[] GetAirlines(string iataCode)
		{
			var refAirlineQuery = new ZDBOnlyQuery(typeof(RefAirline));

			if (iataCode.Length == 2)
			{
				refAirlineQuery.AddToFilter(RefAirlineSchema.RM_TwoCharacterCode, iataCode);
			}
			else if (iataCode.Length == 3)
			{
				refAirlineQuery.AddToFilter(RefAirlineSchema.RM_ThreeLetterCode, iataCode);
			}

			var factory = new BusinessObjectFactory();
			return factory.Load<RefAirline>(refAirlineQuery);
		}

		/// <summary>
		/// Load airlines from an IATA code and show error if required.
		/// </summary>
		static (RefAirline, string) GetAirlineOrErrorIfNotExactlyOneFound(string iataCode)
		{
			var refAirlines = GetAirlines(iataCode);
			if (refAirlines.Length == 1)
			{
				return (refAirlines[0], null);
			}

			string errorToReturn;
			if (refAirlines.Length == 0)
			{
				var message = Res.GetString("b8be84f1-0b37-408a-af48-2c28715ea0a0", "No airline profile with IATA code {0} is found. Would you like to create one to continue Autorating?", iataCode);
				var answer = Globals.Message.Show(message, string.Empty, MessageBoxButtons.OKCancel, DialogResult.Cancel);
				if (answer == DialogResult.OK)
				{
					errorToReturn = null;
				}
				else
				{
					errorToReturn = message;
				}
			}
			else
			{
				var message = errorToReturn = Res.GetString("5AFBF05A-A47A-4151-96FC-3970EEFFA902", "More than one airline profile with IATA Code {0} found.", iataCode);
				Globals.Message.Show(message);
			}

			return (null, errorToReturn);
		}

		public static RefAirline GetAirline(string iataCode)
		{
			var refAirlines = GetAirlines(iataCode);
			return refAirlines.Length == 1 ? refAirlines[0] : null;
		}

		protected virtual OrgHeader SelectOrganization()
		{
			var nonAirLineOrgsQuery = new ZQuery(OrgHeaderSchema.OH_IsAirLine, SQLComparisonOperator.Equal, false);
			var pickerCollection = new ShippingProviderCollection(Factory, nonAirLineOrgsQuery);

			var moduleId = ModuleIDs.Organisation;

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(moduleId))
			{
				var provider = new CarrierOrgWithoutIATASingleSelect(pickerCollection);
				module.OverrideModuleDecisionProvider(provider);

				var popup = new EmbeddedModulePopup(module, null, true);
				popup.RequireAtLeastOneItemToBeSelected = true;
				provider.Popup = popup;

				ZFormModaliser.ShowDialogAndDispose(popup);

				return provider.SelectedOrgHeader;
			}
		}

		protected virtual RefAirline CreateAirLine()
		{
			var airline = Factory.New<RefAirline>();
			return airline;
		}

		BusinessObjectFactory Factory { get; }

		static string ChooseAgainMsg => Res.GetString("82C07871-D62A-4D2D-AE07-E3B09CF019E1", "Please choose another one.");
		static string OrgAlreadyHasAirlineMsg(string orgCode)
			=> Res.GetString("81C61AFA-B7A5-4BEE-B352-70A20D25E673", "An airline profile has already been assigned to the carrier {0}.", orgCode);

		class CarrierOrgWithoutIATASingleSelect : SingleSelectModuleDecisionProvider, IFilterModuleExtraNotificationProvider
		{
			public CarrierOrgWithoutIATASingleSelect(IBusinessObjectCollection list)
				: base(list)
			{
			}

			public OrgHeader SelectedOrgHeader => (OrgHeader)SelectedBusinessObject;

			public INotification GetExtraNotification(BusinessObject businessObject)
			{
				var orgHeader = (OrgHeader)businessObject;
				if (orgHeader.OH_IsAirLine)
				{
					return new Notification(CargoWise.EntityFramework.NotificationType.Error, OrgAlreadyHasAirlineMsg(orgHeader.OH_Code));
				}

				return null;
			}
		}
	}
}
