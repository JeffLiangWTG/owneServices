using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelector.Services
{
	public class DialogService : IDialogService
	{
		public DialogService(ZForm parentForm)
		{
			this.parentForm = Argument.NotNull(parentForm, nameof(parentForm));
		}

		public void ShowRawRate(string rawRate)
		{
			WiseRatesGUIHelper.ViewJSON(rawRate);
		}

		public bool MapUniversalChargeCodes(UniversalChargeCodeMapBizoCollection chargeCodes)
		{
			using (var form = new MapChargeCodesForm(chargeCodes))
			{
				var result = ZFormModaliser.ShowDialogAndDispose(form);
				return result == DialogResult.OK;
			}
		}

		public OrgHeader MapCarrier(string iataCode)
		{
			return CreateAirlineAndAssignIATACodeCommand.AssignWithUserConfirmation(iataCode, this);
		}

		public bool MapServiceLevel(OrgHeader carrier, WiseRates.Api.Model.RefServiceLevel universalServiceLevel)
		{
			Argument.NotNull(carrier, nameof(carrier));
			Argument.NotNull(universalServiceLevel, nameof(universalServiceLevel));

			var msg = Res.GetString("ccfb9f93-2498-4bca-8769-8ee2d224e5b2", "The Universal Service Level '{0}' has NOT been assigned to Carrier '{1}'. Would you like to complete the assignment and apply rates to the job?", universalServiceLevel.Code, carrier.OH_Code);
			var caption = Res.GetString("ec15834e-7047-43cb-a7b3-843c1241e47c", "Service Level mapping");

			var res = Globals.Message.Show(msg, caption, MessageBoxButtons.YesNo, DialogResult.No);
			if (res != DialogResult.Yes)
			{
				return false;
			}

			return WiseRatesGUIHelper.EnhanceUserPermissionToRunAction
			(
				Res.GetString("0B0D1B01-305E-4DF2-BBE5-1D6778890B84", "Carrier Service Levels"),
				(securityCore) => securityCore.OrgCarrierModify,
				false,
				caption,
				() =>
				{
					var form = new OrganizationFormForCarrierServiceLevelsMapping(
						parentForm,
						carrier,
						new[] { universalServiceLevel },
						new[]
						{
							new UnmappedForeignCode(universalServiceLevel.Code, Core.Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel)
						}
					);

					return form.ShowModal();
				}
			);
		}

		public ZDialogResult PromptToUpdateOrReplaceDetailedGoodsDescription(bool isOriginalDescriptionBlank)
		{
			if (isOriginalDescriptionBlank)
			{
				// If the original Detailed Goods Description is blank then it
				// should silently replace it.
				return ZDialogResult.No;
			}
			else
			{
				var message = ResString.GetMultilingualString("4a070da1-69d5-4abe-8709-1b0f5ec78ad2", "Would you like to append the Detailed Goods Description with the selected Commodity's description (Yes) or replace it (No) or make no changes (Cancel)?");
				var caption = ResString.GetMultilingualString("3ac8bdbf-b912-400f-9ab7-ffa64d1a84d8", "Update Detailed Goods Description");

				return Globals.Message.Show(message, caption, ZMessageBoxButtons.YesNoCancel, ZMessageBoxIcon.Question, ZDialogResult.Yes);
			}
		}

		public bool PromptToApplyCarrierToJob(string jobName, string message)
		{
			var caption = ResString.GetMultilingualString(
				"5da28d3f-cdb4-43b0-a2fa-f7fea5c7a832",
				"Populate selected carrier back to {0}", jobName);

			var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.No);
			return dialogResult == DialogResult.Yes;
		}

		public ZDialogResult PromptToApplyOriginToJob(string jobName, string message)
		{
			var caption = ResString.GetMultilingualString(
				"ef994a4f-4100-44fd-bf8e-9d12088125e8",
				"Populate selected origin back to {0}", jobName);

			return Globals.Message.Show(message, caption, ZMessageBoxButtons.YesNoCancel, ZMessageBoxIcon.Question, ZDialogResult.Cancel);
		}

		public ZDialogResult PromptToApplyDestinationToJob(string jobName, string message)
		{
			var caption = ResString.GetMultilingualString(
				"8fa29e71-fcd7-4cd3-a0cb-6c5cc9737a84",
				"Populate selected destination back to {0}", jobName);

			return Globals.Message.Show(message, caption, ZMessageBoxButtons.YesNoCancel, ZMessageBoxIcon.Question, ZDialogResult.Cancel);
		}

		public bool PromptToApplyContainerPenaltiesToJob(string jobName, string message, out bool shouldDeleteExistingPenalties)
		{
			var caption = ResString.GetMultilingualString(
				"653402e8-f9f9-40d2-9bfa-d50b8db41447",
				"Populate selected container penalties back to {0}", jobName);

			var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, DialogResult.Cancel);

			shouldDeleteExistingPenalties = (dialogResult == DialogResult.Yes);
			return dialogResult != DialogResult.Cancel;
		}

		public bool MapCommodity(string universalCommodityGroup)
		{
			Argument.NotNull(universalCommodityGroup, nameof(universalCommodityGroup));

			return WiseRatesGUIHelper.EnhanceUserPermissionToRunAction
			(
				Res.GetString("c149bbd9-ea90-48d9-b10c-20e43754258c", "Universal Commodity Groups"),
				(securityCore) => securityCore.CommodityModify,
				false,
				Res.GetString("57ec2d66-7b3d-48e3-b2ab-aa169cb06b5a", "Universal Commodity Group mapping"),
				() =>
				{
					var command = new AssignUniversalCommodityGroupCommand(parentForm);
					return command.Assign(universalCommodityGroup);
				}
			);
		}

		public bool PromptToApplyRateWithDifferentUniversalCommodityGroups(string commodity, IEnumerable<string> jobUniversalCommodityGroups, IEnumerable<string> commodityCodesFromJob)
		{
			var questionApply = Res.GetString("19d40172-3878-4248-85c3-62e88a201dc6", @"The Universal Commodity Group '{0}' is different from the Universal Commodity Group '{1}' assigned to Consol > Containers > Commodity '{2}'.
Would you like to apply the rate to the job?",
				commodity,
				string.Join(", ", jobUniversalCommodityGroups),
				string.Join(", ", commodityCodesFromJob));
			var captionApply = Res.GetString("1d37a9a9-b246-4097-b495-cc872d619ada", "Applying rate");

			return Globals.Message.Show(questionApply, captionApply, ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question) == ZDialogResult.Yes;
		}

		public bool PromptToApplyRateWithDifferentCommodityCode(string commodity, IEnumerable<string> commodityCodesFromJob)
		{
			var questionApply = Res.GetString("C670390E-6561-47E9-9DEA-DB96F8C5EE3E", @"The Commodity Code '{0}' is different from the Commodity Codes '{1}' assigned to Consol > Containers or Shipment > Packline.
Would you like to apply the rate to the job?",
				commodity,
				string.Join(", ", commodityCodesFromJob));
			var captionApply = Res.GetString("1d37a9a9-b246-4097-b495-cc872d619ada", "Applying rate");

			return Globals.Message.Show(questionApply, captionApply, ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question) == ZDialogResult.Yes;
		}

		public ZDialogResult PromptUserApplyZeroCharges(List<ZString> zeroCharges)
		{
			var msg = Res.GetString("707AF6A9-ADFE-41D7-83B4-AF90EE3C80DA", "Charges: {0} are found to have Zero amount, do you want to populate those charges back to the job?", string.Join(",", zeroCharges));
			var captionApply = Res.GetString("CC9715B2-0907-435A-B60F-BF6CB847F29C", "Applying zero rates");
			return Globals.Message.Show(msg, captionApply, ZMessageBoxButtons.YesNoCancel, ZDialogResult.Yes);
		}

		public ZDialogResult PromptToSendBookingInformationToCarrier(string carrierName)
		{
			var captionApply = Res.GetString("7662ae94-10a2-46dc-aea1-c8914672b610", "Applying zero rates");
			var msg = Res.GetString("3eca9c7a-4819-40a4-92d2-e16262bacdf9", "Would you like to send {0} Spot Booking now to reserve the rate and the schedule?", string.IsNullOrEmpty(carrierName) ? (NoResString)"the" : carrierName);
			return Globals.Message.Show(msg, captionApply, ZMessageBoxButtons.YesNoCancel, ZDialogResult.No);
		}

		public string SelectSingleClientContractNumber(IEnumerable<string> contractNumbers)
		{
			using (var form = new SingleValueSelectForm(
				contractNumbers.OrderBy(x => x),
				Res.GetString("298ed86e-014c-43bf-8754-3f1981c4a2e7", "Multiple Client Contract Numbers have been found but only one can be used on this job. Please select which Client Contract Number to be populated to the Job's Client Contract Number. Please note that only rates with the same Contract Numbers or NO Contract Number to be applied."),
				Res.GetData("e3d4ce36-b1ce-4255-8d7a-0c22a7ce0e7a", "Client Contract Number")))
			{
				if (DialogResult.OK == ZFormModaliser.ShowDialogWithoutDispose(form))
				{
					return form.SelectedName;
				}
			}

			return null;
		}

		/// <summary>
		/// Shows a dialog listing all contract numbers. There are a special item added to the end of the list:
		/// <see cref="ContractNumberSelectionResult.KeepJobCarrierContractNumber"/>
		/// </summary>
		/// <returns>
		/// A composite result for either:
		/// - A selected number from the shown items
		/// - Or <see cref="ContractNumberSelectionResult.Cancelled"/> for closing the dialog without any selected item.
		/// </returns>
		public SingleCarrierContractNumberSelectionResult SelectSingleCarrierContractNumber(IEnumerable<string> contractNumbers)
		{
			var items = new List<string>(contractNumbers.OrderBy(x => x));
			var noChange = Res.GetString("7B7E48CD-A276-4863-8641-D7CA7AC3D8B0", "NOT Populating Carrier Contract Number");
			items.Add(noChange);

			var result = new SingleCarrierContractNumberSelectionResult { Result = ContractNumberSelectionResult.Cancelled };
			using (var form = new SingleValueSelectForm(
				items,
				Res.GetString("E672343E-D3A7-42BF-A1B2-41ACA43D0FA4", "Multiple Carrier Contract Numbers have been found but only one can be used on this job. Please select which Carrier Contract Number to be populated to Carrier Contract No. field."),
				Res.GetData("3619CF85-AF0F-40BF-9379-F5761ADA215D", "Carrier Contract Numbers")))
			{
				if (DialogResult.OK != ZFormModaliser.ShowDialogWithoutDispose(form))
				{
					return result;
				}

				if (form.SelectedIndex == items.Count - 1)
				{
					result.Result = ContractNumberSelectionResult.KeepJobCarrierContractNumber;
					result.Number = null;
				}
				else
				{
					result.Result = ContractNumberSelectionResult.NumberSelected;
					result.Number = form.SelectedName;
				}
			}

			return result;
		}

		public string SelectSingleOrganization(IEnumerable<string> organizations, string prompt)
		{
			using (var form = new SingleValueSelectForm(
				organizations.OrderBy(x => x),
				prompt,
				Res.GetData("659bd05f-e2c6-4a40-8da7-2a436b362672", "Organizations")))
			{
				if (DialogResult.OK == ZFormModaliser.ShowDialogWithoutDispose(form))
				{
					return form.SelectedName;
				}
			}

			return null;
		}

		public void ShowNoFMCTariffIDCombinationFound(string rateCommodity)
		{
			var msg = Res.GetString("92a670f9-2a48-4348-bb49-edc410da7f41", "There were no Commodity/FMC Tariff IDs found for the Commodity {0}.\r\nPlease check there exists Company Tariffs or Client Rates that match this job's information.", rateCommodity);
			var caption = Res.GetString("3df1ce28-60e0-4482-b84e-32e7bf104b77", "No results found");
			Globals.Message.Show(msg, caption, MessageBoxButtons.OK, DialogResult.OK);
		}

		public void ShowCannotDoOperationCheckMessage(string message)
		{
			var msg = Res.GetString("2034cfdf-012d-4036-a759-597555d0280d", "Please see the below errors:{0}{0}{1}", System.Environment.NewLine, message);
			var caption = Res.GetString("b01bf3e8-c12f-4988-8cee-43b5999b4ce5", "Could not complete the requested operation");
			Globals.Message.Show(msg, caption, MessageBoxButtons.OK, DialogResult.OK);
		}

		public void ShowFMCTariffIDLogs(string message)
		{
			var caption = Res.GetString("4adf6c5a-99d2-4dae-8f97-6644e35ac740", "Tariff ID Search Logs");
			Globals.Message.Show(message, caption, MessageBoxButtons.OK, DialogResult.OK);
		}

		readonly ZForm parentForm;

		public ZForm ParentForm => parentForm;
	}
}
