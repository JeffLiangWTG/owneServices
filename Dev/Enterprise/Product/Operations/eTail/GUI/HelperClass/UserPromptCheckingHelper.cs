using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.eTail.GUI
{
	public static class UserPromptCheckingHelper
	{
		public static class US
		{
			public static bool CheckWayBill(IEnumerable<HVLVConsignment> consignments, string customsJobName)
			{
				var result = true;
				var hasWaybillMessageErrors = false;
				foreach (var consignment in consignments)
				{
					consignment.Validation.ValidateHVC_WaybillNumberForUSCustomsJob();
					if (!hasWaybillMessageErrors)
					{
						hasWaybillMessageErrors = consignment.HVC_WaybillNumberInfo.HasMessageError(HVLVConsignmentUSCustomsValidation.USWaybillNumberInvalidErrorMessage)
							|| consignment.HVC_WaybillNumberInfo.HasMessageError(HVLVConsignmentUSCustomsValidation.USWaybillNumberWithSCACCodeInvalidErrorMessage);
					}
				}

				if (hasWaybillMessageErrors)
				{
					var userChoice = Globals.Message.Show(Res.GetString("bbbafea5-e55d-49ba-8603-764fb7f18e4a", "There are waybill(s) that exceed 12 in length, proceeding may cause message errors on {0}.\r\nWould you like to proceed?", customsJobName),
						Res.GetString("7627800d-bb1c-4744-9b1d-25755d310b0c", "Waybill Validation"),
						ZMessageBoxButtons.YesNo,
						ZDialogResult.No);
					result = userChoice == ZDialogResult.Yes;
				}

				return result;
			}

			public static bool CheckWaybill(HVLVConsignment consignment, string customsJobName)
			{
				return CheckWayBill(new[] { consignment }, customsJobName);
			}
		}

		public static bool CheckBusinessObjectHasNoChangesOrNotify(BusinessObject businessObject, string message)
		{
			var result = true;
			if (businessObject.HasChanges)
			{
				Globals.Message.Show(message);
				result = false;
			}

			return result;
		}

		public static bool CheckHasAnyActiveConsignmentOrNotify(IEnumerable<HVLVConsignment> consignments)
		{
			var result = true;
			if (!consignments.Any(x => x.HVC_IsActive))
			{
				result = false;
				Globals.Message.Show(ShipmentNeedsActiveConsignmentsMessage);
			}

			return result;
		}

		public static bool CheckHasAnyConsignmentAvailableToSendOriginalACASReport(IEnumerable<HVLVConsignment> consignments)
		{
			if (consignments.Any(c => c.HVC_ACASMessageStatus.IsEmpty))
			{
				return true;
			}

			Globals.Message.Show(Res.GetString("c0d59fd7-d8e7-4dba-90f6-a36727632131", "Consignment ACAS reports have already been sent, please wait for further response."));
			return false;
		}

		static string ShipmentNeedsActiveConsignmentsMessage => Res.GetString("b4e797de-7c1a-43e9-9332-0c0233b141c5", "Please make sure there's at least one active consignment on this shipment.");

		static bool CheckShipmentHasSameTransportTypeAsConsolOrNotify(BaseHVLVRelatedJobCommand command)
		{
			command.Shipment.Validation.ValidateJS_TransportMode();
			var warnings = command.Shipment.JS_TransportModeInfo.GetWarnings().Select(w => w.Message);
			if (warnings.Any())
			{
				var warningText = string.Join(System.Environment.NewLine, warnings);
				var warningMessage = Res.GetString("7587d406-de64-44b2-a23d-73bdfa09e7f3", "{0}\r\nWould you like to proceed?", warningText);
				var dialogResult = Globals.Message.Show(warningMessage, Res.GetString("548f1c65-8fe3-4aa7-9cea-8ce7541b2271", "Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No);
				if (dialogResult == DialogResult.No)
				{
					return false;
				}
			}

			return true;
		}

		public static bool CheckUSSecurityFilingsEnabled()
		{
			var result = false;

			if (CountryCodes.GetCustomsCountryOfJurisdiction(GlbBranch.CurrentBranch.Country.Code) == CountryCodes.UnitedStates
				|| HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.Value)
			{
				result = true;
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("2d81cc00-f1d4-4e3e-84fc-ee5f57d27e3d", @"US Security Filings are not enabled for this branch.

To enable go to Registry -> {0}", HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.Inner.Location));
			}

			return result;
		}

		public static bool CheckCreateRelatedJob(BaseHVLVRelatedJobCommand command)
		{
			return CheckBusinessObjectHasNoChangesOrNotify(command.Shipment, Res.GetString("3c18d039-e2c2-4ad1-8dd0-26bbdbc0c6fc", "Please save the form before converting to {0}.", command.RelatedJobName))
				&& CheckIsReadyToConvertToCustoms(command)
				&& CheckShipmentHasSameTransportTypeAsConsolOrNotify(command)
				&& (!command.ShouldValidateWaybill || US.CheckWayBill(command.Header.Consignments.OfType<HVLVConsignment>(), command.RelatedJobName))
				&& (!command.NeedPreScreening || CheckProceedWhenPreScreeningNotRun(command));
		}

		static bool CheckIsReadyToConvertToCustoms(BaseHVLVRelatedJobCommand command)
		{
			var dataIsReadyToConvertToCustoms = command.CheckDataIsReadyForConvertToCustomsJob(out var errorMessage);

			if (!dataIsReadyToConvertToCustoms)
			{
				Globals.Message.Show(errorMessage);
			}

			return dataIsReadyToConvertToCustoms;
		}

		static bool CheckProceedWhenPreScreeningNotRun(BaseHVLVRelatedJobCommand command)
		{
			var result = true;
			if (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.Value.IsEnabled)
			{
				var preScreenedLogs = command.Shipment.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.HVLVReadyCode
					&& log.Parameters.TryGetValue(EventReferenceParameters.Codes.Reason, out var reason)
					&& reason == EventReferenceParameterReasons.PreScreened).ToArray();

				if (preScreenedLogs.Length == 0)
				{
					var warning = Res.GetString("AB59FC21-9D39-481D-94F6-23046C280361", @"HVLV Pre-Screening has been enabled but not run on this shipment.
Proceeding with the {0} will bypass these Pre-Screening rules, would you like to proceed? ", command.RelatedJobName);

					var dialogResult = Globals.Message.Show(warning, Res.GetString("6BE55533-7CA8-4D3D-8987-35C41A2F42CF", "Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No);
					if (dialogResult == DialogResult.No)
					{
						result = false;
					}
				}
			}

			return result;
		}
	}
}
