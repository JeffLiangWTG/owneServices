using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;

namespace Enterprise.Customs.Business
{
	public class CreateBrokerageOnShipmentProcessor :
		Integration.Customs.Shared.ICreateBrokerageOnShipmentProcessor,
		IProcessor
	{
		public CreateBrokerageOnShipmentProcessor(ForwardingShipment shipment)
		{
			Argument.NotNull(shipment, "shipment");
			this.shipment = shipment;
		}

		readonly ForwardingShipment shipment;

		public void Process(INotifications notifications, CancellationToken token)
		{
			if (shipment.GetDeclaration() == null)
			{
				var mutex = DeclarationBeingCreatedForShipmentMutexCreator.Create(shipment.PK);
				shipment.Factory.Saved += Factory_Saved;

				try
				{
					var createDeclarationHelper = GetCreateDeclarationHelper();
					createDeclarationHelper.Notify += (ForwardingShipment shipment, CreateDeclarationHelper.NotifyType notifyType) => notifications.AddInformation(createDeclarationHelper.GetDescriptionForNotifyType(notifyType));
					createDeclarationHelper.ConfirmCreateNewDeclaration = false;
					createDeclarationHelper.CreateDeclaration(shipment, mutex, () => (BaseJobDeclaration)shipment.GetDeclaration());
				}
				catch (Exception)
				{
					UnlockMutex();
					shipment.Factory.Saved -= Factory_Saved;
					throw;
				}

				void UnlockMutex()
				{
					if (mutex != null && mutex.HasLock)
					{
						mutex.Unlock();
					}
				}

				void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
				{
					UnlockMutex();
					factory.Saved -= Factory_Saved;
				}
			}
			else
			{
				notifications.AddInformation(Res.GetString("EB124F52-EDDA-4082-A6AE-6E3F1B9F546C", "Shipment already has a Declaration."));
			}
		}

		internal CreateDeclarationHelper GetCreateDeclarationHelper()
		{
			var helperProvider = new CreateDeclarationHelperProvider();
			return (CreateDeclarationHelper)helperProvider.NewCreateDeclarationHelper(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}
	}
}
