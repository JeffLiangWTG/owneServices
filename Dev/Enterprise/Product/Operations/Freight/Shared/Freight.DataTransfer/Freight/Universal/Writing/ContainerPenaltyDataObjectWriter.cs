using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using ContainerPenaltyBO = Enterprise.Freight.Business.ContainerPenalty;
using ContainerPenaltyDataObject = Enterprise.UniversalDataBuss.DataObjects.Universal.ContainerPenalty;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ContainerPenaltyDataObjectWriter : DataObjectWriter<ContainerPenaltyBO, ContainerPenaltyDataObject>
	{
		public ContainerPenaltyDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override ContainerPenaltyDataObject PopulateDataObject(ContainerPenaltyBO sourceBO)
		{
			var penalty = new ContainerPenaltyDataObject();

			penalty.PenaltyType = ListHelper.GetWithDescription<CodeDescriptionPair>(sourceBO.CPY_PenaltyType, sourceBO.Lookups.PenaltyTypeList);
			penalty.CreditorType = ListHelper.GetWithDescription<CodeDescriptionPair>(sourceBO.CPY_CreditorType, sourceBO.Lookups.CreditorTypeList);
			penalty.Location = ListHelper.GetWithName(sourceBO.CPY_RL_NKLocation, sourceBO.Lookups.Locations);
			penalty.FreeTime = sourceBO.CPY_FreeTime;
			penalty.FreeTimeAmount = ConvertDateTimeToDaysOrHours(sourceBO.CPY_FreeTime, sourceBO.CPY_TimeUnit);
			penalty.Duration = sourceBO.CPY_Duration;
			penalty.DurationAmount = ConvertDateTimeToDaysOrHours(sourceBO.CPY_Duration, sourceBO.CPY_TimeUnit);
			penalty.FirstFreeDay = sourceBO.FirstFreeDay;
			penalty.LastFreeDay = sourceBO.LastFreeDay;
			penalty.TimeUnit = sourceBO.CPY_TimeUnit == Core.Constants.ContainerPenaltyTimeUnit.Codes.Hours ? TimeUnit.Hours : TimeUnit.Days;
			penalty.PerUnitCost = sourceBO.CPY_PerUnitCost;
			penalty.TotalCost = sourceBO.CPY_TotalCost;
			penalty.PerUnitSell = IsContainerPenaltyUnderConsol(sourceBO.CPY_ProcessType) ? 0 : sourceBO.CPY_PerUnitCost;
			penalty.TotalSell = IsContainerPenaltyUnderConsol(sourceBO.CPY_ProcessType) ? 0 : sourceBO.CPY_TotalCost;
			penalty.Currency = ListHelper.GetWithDescription<Currency>(sourceBO.CPY_RX_NKCurrency, sourceBO.Lookups.Currencies);
			penalty.ProcessType = ListHelper.GetWithDescription<CodeDescriptionPair>(sourceBO.CPY_ProcessType, sourceBO.Lookups.ProcessTypeList);

			if (!sourceBO.CPY_OH_Creditor.IsEmpty)
			{
				var creditor = sourceBO.Factory.Load<OrgHeader>(sourceBO.CPY_OH_Creditor);
				if (creditor != null)
				{
					penalty.Creditor = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.Creditor)).GetDataObject(creditor.MainAddress);
				}
			}

			return penalty;
		}

		ZByte ConvertDateTimeToDaysOrHours(ZDateTime time, string timeUnit)
		{
			return timeUnit == Core.Constants.ContainerPenaltyTimeUnit.Codes.Hours
				? ContainerPenaltyBO.ConvertDateTimeToHours(time)
				: ContainerPenaltyBO.ConvertDateTimeToDays(time);
		}

		bool IsContainerPenaltyUnderConsol(ZString processType)
		{
			return processType == Core.Constants.ContainerPenaltyProcessType.Import || processType == Core.Constants.ContainerPenaltyProcessType.Export;
		}
	}
}
