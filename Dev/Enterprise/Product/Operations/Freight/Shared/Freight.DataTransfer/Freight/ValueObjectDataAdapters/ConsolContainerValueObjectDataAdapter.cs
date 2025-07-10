using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	public class ConsolContainerValueObjectDataAdapter<TBusinessObject, TValueObject> : JobContainerValueObjectDataAdapter<TBusinessObject, TValueObject>
		where TBusinessObject : CommonContainer
		where TValueObject : Xsd.Container
	{
		public ConsolContainerValueObjectDataAdapter(CommonConsol consol)
		{
			this.Consol = consol;
		}

		public readonly CommonConsol Consol;

		protected override void ExportToValueObjectCore(TBusinessObject bizObj, TValueObject constructedValueObject, IValueObjectExportContext context)
		{
			base.ExportToValueObjectCore(bizObj, constructedValueObject, context);

			CommonContainer freightContainer = bizObj;
			Xsd.Container xsdContainer = constructedValueObject;

			xsdContainer.IsArrivingAtCTOByRail = freightContainer.JC_DepartureDeliveryByRail;
			xsdContainer.IsArrivingAtCTOByRailSpecified = true;
			xsdContainer.IsEmptyContainer = freightContainer.JC_IsEmptyContainer;
			xsdContainer.IsEmptyContainerSpecified = true;
			xsdContainer.IsDamaged = freightContainer.JC_IsDamaged;
			xsdContainer.IsDamagedSpecified = true;

			xsdContainer.ExportProcess.IsArrivingAtCTOByRail = freightContainer.JC_DepartureDeliveryByRail;
			xsdContainer.ExportProcess.IsArrivingAtCTOByRailSpecified = true;
			xsdContainer.ExportProcess.DemurrageCharge = freightContainer.DepartureTruckWaitCost;
			xsdContainer.ExportProcess.DemurrageChargeSpecified = true;

			xsdContainer.ImportProcess.DemurrageCharge = freightContainer.ArrivalTruckWaitCost;
			xsdContainer.ImportProcess.DemurrageChargeSpecified = true;
			xsdContainer.ImportProcess.DetentionDays = freightContainer.ArrivalCarrierDetentionDays.ToString();
			xsdContainer.ImportProcess.DetentionDaysSpecified = true;
			xsdContainer.ImportProcess.DetentionCharge = freightContainer.ArrivalCarrierDetentionCost;
			xsdContainer.ImportProcess.DetentionChargeSpecified = true;
			xsdContainer.ImportProcess.StorageDays = freightContainer.ArrivalCTOStorageDays.ToString();
			xsdContainer.ImportProcess.StorageDaysSpecified = true;
			xsdContainer.ImportProcess.StorageCharge = freightContainer.ArrivalCTOStorageCost;
			xsdContainer.ImportProcess.StorageChargeSpecified = true;

			xsdContainer.ImportProcess.PickupByRail = freightContainer.JC_ArrivalPickupByRail;
			xsdContainer.ImportProcess.PickupByRailSpecified = true;
			xsdContainer.ImportProcess.HeldForFCLTransitStaging = freightContainer.JC_FCLHeldInTransitStaging;
			xsdContainer.ImportProcess.HeldForFCLTransitStagingSpecified = true;
		}

		protected override void ImportFromValueObjectCore(TBusinessObject bizObj, TValueObject containerValue, IValueObjectImportContext context)
		{
			base.ImportFromValueObjectCore(bizObj, containerValue, context);

			CommonContainer freightContainer = bizObj;

			freightContainer.JC_DepartureDeliveryByRail = containerValue.IsArrivingAtCTOByRail;
			freightContainer.JC_IsEmptyContainer = containerValue.IsEmptyContainer;
			freightContainer.JC_IsDamaged = containerValue.IsDamaged;

			#region Export Process

			if (containerValue.ExportProcess.IsSpecified)
			{
				if (containerValue.ExportProcess.IsArrivingAtCTOByRailSpecified)
				{
					freightContainer.JC_DepartureDeliveryByRail = containerValue.ExportProcess.IsArrivingAtCTOByRail;
				}

				if (containerValue.ExportProcess.DemurrageChargeSpecified)
				{
					freightContainer.DepartureTruckWaitCost = containerValue.ExportProcess.DemurrageCharge;
				}
			}

			#endregion

			#region Import Process

			if (containerValue.ImportProcess.IsSpecified)
			{
				if (containerValue.ImportProcess.DemurrageChargeSpecified)
				{
					freightContainer.ArrivalTruckWaitCost = containerValue.ImportProcess.DemurrageCharge;
				}

				if (containerValue.ImportProcess.StorageChargeSpecified)
				{
					freightContainer.ArrivalCTOStorageCost = containerValue.ImportProcess.StorageCharge;
				}

				context.SetPropertyInfoValue(freightContainer.ArrivalCTOStorageDaysInfo, containerValue.ImportProcess.StorageDays, containerValue.ImportProcess.StorageDaysSpecified);
				context.SetPropertyInfoValue(freightContainer.ArrivalCarrierDetentionDaysInfo, containerValue.ImportProcess.DetentionDays, containerValue.ImportProcess.DetentionDaysSpecified);

				if (containerValue.ImportProcess.DetentionChargeSpecified)
				{
					freightContainer.ArrivalCarrierDetentionCost = containerValue.ImportProcess.DetentionCharge;
				}

				if (containerValue.ImportProcess.PickupByRailSpecified)
				{
					freightContainer.JC_ArrivalPickupByRail = containerValue.ImportProcess.PickupByRail;
				}

				if (containerValue.ImportProcess.HeldForFCLTransitStagingSpecified)
				{
					freightContainer.JC_FCLHeldInTransitStaging = containerValue.ImportProcess.HeldForFCLTransitStaging;
				}
			}

			#endregion
		}

		protected override bool RegistryDefaultForImporting
		{
			get { return SystemRegistry.UpdateConsolContainersDuringAutomaticImport.Value; }
		}

		protected override TBusinessObject NewBusinessObject(TValueObject value, IValueObjectImportContext context)
		{
			return (TBusinessObject)Consol.Containers.AddNew();
		}

		protected override TBusinessObject FindBusinessObject(TValueObject containerValue, IValueObjectImportContext context)
		{
			ZQuery filter = new ZQuery(JobContainerSchema.JC_ContainerNum, SQLComparisonOperator.Equal, containerValue.ContainerNumber);
			BusinessObject[] result = Consol.Containers.Find(filter);
			return (result.Length == 1) ? (TBusinessObject)result[0] : null;
		}
	}
}
