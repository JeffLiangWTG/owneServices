using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using ContainerPenaltyBO = Enterprise.Freight.Business.ContainerPenalty;
using ContainerPenaltyDataObject = Enterprise.UniversalDataBuss.DataObjects.Universal.ContainerPenalty;

namespace Enterprise.Freight.DataTransfer.Freight.Universal
{
	public class ContainerPenaltyDataObjectReader : DataObjectReader<ContainerPenaltyDataObject, ContainerPenaltyBO>
	{
		public ContainerPenaltyDataObjectReader(ContainerPenaltyDataObject containerPenaltyDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, CommonContainer parent)
			: base(containerPenaltyDataObject, logger, factory)
		{
			this.parent = parent;
		}

		readonly CommonContainer parent;

		protected override ContainerPenaltyBO GetExistingBusinessObject()
		{
			if (parent != null && dataObject.ProcessType != null)
			{
				var processTypeCode = dataObject.ProcessType.Code ?? ZString.Empty;
				if (processTypeCode == ContainerPenaltyProcessType.Export)
				{
					return parent.ExportPenalties.FirstOrDefault(p =>
						dataObject.PenaltyType != null
						&& dataObject.CreditorType != null
						&& p.CPY_PenaltyType == dataObject.PenaltyType.Code.GetValueOrDefault()
						&& p.CPY_CreditorType == dataObject.CreditorType.Code.GetValueOrDefault());
				}
				else if (processTypeCode == ContainerPenaltyProcessType.Delivery)
				{
					return parent.DeliveryPenalties.FirstOrDefault(p =>
						dataObject.PenaltyType != null
						&& dataObject.CreditorType != null
						&& p.CPY_PenaltyType == dataObject.PenaltyType.Code.GetValueOrDefault()
						&& p.CPY_CreditorType == dataObject.CreditorType.Code.GetValueOrDefault());
				}
				else if (processTypeCode == ContainerPenaltyProcessType.Pickup)
				{
					return parent.PickupPenalties.FirstOrDefault(p =>
						dataObject.PenaltyType != null
						&& dataObject.CreditorType != null
						&& p.CPY_PenaltyType == dataObject.PenaltyType.Code.GetValueOrDefault()
						&& p.CPY_CreditorType == dataObject.CreditorType.Code.GetValueOrDefault());
				}
				else
				{
					return parent.ImportPenalties.FirstOrDefault(p =>
					dataObject.PenaltyType != null
					&& dataObject.CreditorType != null
					&& p.CPY_PenaltyType == dataObject.PenaltyType.Code.GetValueOrDefault()
					&& p.CPY_CreditorType == dataObject.CreditorType.Code.GetValueOrDefault());
				}
			}

			return null;
		}

		protected override void PopulateBusinessObject(ContainerPenaltyBO targetBO)
		{
			SetValue(targetBO, JobContainerPenaltySchema.CPY_JC_Container, parent.PK);
			SetValue(targetBO, JobContainerPenaltySchema.CPY_PenaltyType, dataObject.PenaltyType);
			SetValue(targetBO, JobContainerPenaltySchema.CPY_CreditorType, dataObject.CreditorType);
			SetValue(targetBO, JobContainerPenaltySchema.CPY_RL_NKLocation, dataObject.Location);
			SetValue(targetBO, JobContainerPenaltySchema.CPY_FreeTime, dataObject.FreeTime);
			SetValue(targetBO, JobContainerPenaltySchema.CPY_Duration, dataObject.Duration);
			SetValue(targetBO, JobContainerPenaltySchema.CPY_TimeUnit,
				dataObject.TimeUnit == Enterprise.UniversalDataBuss.DataObjects.Universal.TimeUnit.Hours
					? Core.Constants.ContainerPenaltyTimeUnit.Codes.Hours
					: Core.Constants.ContainerPenaltyTimeUnit.Codes.Days);
			SetValue(targetBO, JobContainerPenaltySchema.CPY_PerUnitCost, dataObject.PerUnitCost);
			SetValue(targetBO, JobContainerPenaltySchema.CPY_TotalCost, dataObject.TotalCost);
			SetValue(targetBO, JobContainerPenaltySchema.CPY_RX_NKCurrency, dataObject.Currency);
			SetValue(targetBO, JobContainerPenaltySchema.CPY_ProcessType, dataObject.ProcessType);

			if (dataObject.Creditor != null)
			{
				var orgAddress = new MasterFiles.DataTransfer.Universal.OrganisationDataObjectReader(dataObject.Creditor, logger, factory).GetMatched();
				if (orgAddress != null)
				{
					SetValue(targetBO, JobContainerPenaltySchema.CPY_OH_Creditor, orgAddress.OA_OH);
				}
			}

			if (targetBO.CPY_RX_NKCurrency.IsEmpty && !(targetBO.Location?.Country?.Code ?? ZString.Empty).IsEmpty)
			{
				targetBO.CPY_RX_NKCurrency = ContainerPenaltyBO.GetPenaltyCurrency(factory.BOFactory, targetBO.Location.Country.Code);
			}
		}

		protected override ContainerPenaltyBO GetNewBusinessObject()
		{
			if (parent != null && dataObject.ProcessType != null)
			{
				if ((dataObject.ProcessType.Code ?? ZString.Empty) == ContainerPenaltyProcessType.Export)
				{
					return parent.ExportPenalties.AddNew();
				}
				else
				{
					return parent.ImportPenalties.AddNew();
				}
			}

			return base.GetNewBusinessObject();
		}
	}
}
