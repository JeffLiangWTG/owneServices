using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ProfitShareForwardingModuleConsolCollection : ForwardingModuleConsolCollection, IFilterModuleExtraNotificationProvider
	{
		public ProfitShareForwardingModuleConsolCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			AddDefaultFilters();
		}

		void AddDefaultFilters()
		{
			//Refactoring part - We can't use JobConsolFilterBusinessObject.Descriptions.ConsolNum as it would be circular reference,
			//so we need to change it later during refactoring.
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Consol #", "Property", ZString.Empty));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("End Ports (First Load / Last Disch.)", "Property1", ZString.Empty));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("ETD / Load Port", "Property1", ZString.Empty));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("ETA / Discharge Port", "Property1", ZString.Empty));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Sending Agent Type", "Property", ZString.Empty));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Receiving Agent Type", "Property", ZString.Empty));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Send / Receive Agents", "Property1", ZString.Empty));
		}

		public INotification GetExtraNotification(BusinessObject businessObject)
		{
			var errors = GetError(businessObject);
			if (errors.IsEmpty)
			{
				return null;
			}
			return new Notification(CargoWise.EntityFramework.NotificationType.Error, GetError(businessObject));
		}

		ZString GetError(BusinessObject selectedBusinessObject)
		{
			var result = ZString.Empty;
			var consol = (ForwardingConsol)selectedBusinessObject;

			if (ProfitShareHelper.IsConsolAlreadyProcessed(Factory, consol.PK, consol.InvoicingSupporter))
			{
				result = Res.GetString("6c92ea53-aaf1-4067-998c-e23926c793e9", "This consol is already processed.");
			}
			else if (!consol.IsGatewayBillingEnabled())
			{
				result = Res.GetString("284576b5-897f-4b54-be22-65447be7d647", "Only Consol with Login Company as the Sending or Receiving Agent (GTT/GTA) is valid to be selected.");
			}

			return result;
		}
	}
}
