using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business
{
	public class DisbursementBillingHelper : IDisbursementBillingHelper
	{
		public DisbursementBillingHelper()
		{
		}

		public bool IsDisbursementBillingEnabled
		{
			get
			{
				var isEnabled = (ObjectFactory.Get<IAccounting>().Registry?.EnableElectronicProcessingChargeFunctionality as BooleanRegistryItem)?.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty) ?? false;
				var isIncludedInElectronicProcessingChargeConfiguration = ObjectFactory.Get<IAccounting>().IsIncludedInElectronicProcessingChargeConfiguration(ZDateTime.Today, JobInvoicingConsumerTypes.Shipment.Code);
				return isEnabled && isIncludedInElectronicProcessingChargeConfiguration;
			}
		}
	}
}
