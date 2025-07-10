using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Rating.Services
{
	public interface IDialogService
	{
		void ShowRawRate(string rawRate);
		bool MapUniversalChargeCodes(UniversalChargeCodeMapBizoCollection chargeCodes);
		OrgHeader MapCarrier(string iataCode);
		bool MapServiceLevel(OrgHeader carrier, WiseRates.Api.Model.RefServiceLevel universalServiceLevel);
		bool PromptToApplyCarrierToJob(string jobName, string message);

		/// <summary>
		/// Prompts a user with a message to confirm whether they want to update
		/// the origin in the job. 
		/// </summary>
		/// <returns>
		/// ZDialogResult.Cancel: When the application of the rate shall be canceled.
		/// ZDialogResult.Yes: When the application of the rate shall proceed and the origin in the job overwritten.
		/// ZDialogResult.No: When the application of the rate shall proceed and the origin in the job not overwritten.
		/// </returns>
		ZDialogResult PromptToApplyOriginToJob(string jobName, string message);

		/// <summary>
		/// Prompts a user with a message to confirm whether they want to update
		/// the destination in the job. 
		/// </summary>
		/// <returns>
		/// ZDialogResult.Cancel: When the application of the rate shall be canceled.
		/// ZDialogResult.Yes: When the application of the rate shall proceed and the destination in the job overwritten.
		/// ZDialogResult.No: When the application of the rate shall proceed and the destination in the job not overwritten.
		/// </returns>
		ZDialogResult PromptToApplyDestinationToJob(string jobName, string message);

		/// <summary>
		/// Prompts the user to select from either Keeping, Appending, or Replacing
		/// the current Detailed Goods Description on the job.
		/// </summary>
		/// <returns>
		/// ZDialogResult.Cancel: Keep the existing detailed goods description.
		/// ZDialogResult.Yes: Append to the existing detailed goods description.
		/// ZDialogResult.No: Replace the existing detailed goods description.
		/// </returns>
		ZDialogResult PromptToUpdateOrReplaceDetailedGoodsDescription(bool isOriginalDescriptionBlank);

		bool PromptToApplyContainerPenaltiesToJob(string jobName, string message, out bool shouldDeleteExistingPenalties);
		bool MapCommodity(string universalCommodityGroup);
		bool PromptToApplyRateWithDifferentUniversalCommodityGroups(string commodity, IEnumerable<string> jobUniversalCommodityGroups, IEnumerable<string> commodityCodesFromJob);
		bool PromptToApplyRateWithDifferentCommodityCode(string commodity, IEnumerable<string> commodityCodesFromJob);
		ZDialogResult PromptUserApplyZeroCharges(List<ZString> zeroCharges);
		ZDialogResult PromptToSendBookingInformationToCarrier(string carrierName);
		string SelectSingleClientContractNumber(IEnumerable<string> contractNumbers);
		SingleCarrierContractNumberSelectionResult SelectSingleCarrierContractNumber(IEnumerable<string> contractNumbers);
		string SelectSingleOrganization(IEnumerable<string> organizations, string prompt);
		void ShowNoFMCTariffIDCombinationFound(string commodity);
		void ShowCannotDoOperationCheckMessage(string message);
		void ShowFMCTariffIDLogs(string message);
	}

	public struct SingleCarrierContractNumberSelectionResult
	{
		public SingleCarrierContractNumberSelectionResult(string selectedNumber)
		{
			Number = selectedNumber;
			Result = ContractNumberSelectionResult.NumberSelected;
		}

		public string Number { get; set; }
		public ContractNumberSelectionResult Result { get; set; }
	}

	public enum ContractNumberSelectionResult
	{
		Default,
		Cancelled,
		NumberSelected,
		KeepJobCarrierContractNumber
	}
}
