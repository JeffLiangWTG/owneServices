using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.eTail.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.eTail.DataTransfer
{
	public class USeManifestConverter : CustomsRelatedBusinessObjectConverter
	{
		public USeManifestConverter(BaseHVLVRelatedJobCommand relatedJobCommand)
			: base(relatedJobCommand)
		{
		}

		public override RecipientRoleType? RecipientRoleType => null;

		protected override RecipientRoleType PickupOrDeliveryCartageRole => UniversalDataBuss.Integration.RecipientRoleType.DCA;

		public override DataContextType MasterBillDataContextType => DataContextType.USeManifestTrip;

		protected override bool ShouldStripNonWesternEuropeanCharacters => HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersUSeManifest.Value;

		protected override void OnConversionFactorySaving(BusinessObjectFactory factory)
		{
			var eManifest = CustomsRelatedBusinessCollection.OfType<Trip>().SingleOrDefault();

			if (eManifest != null)
			{
				eManifest.OnSaving();

				eManifest.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>[]
				{
						new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "HVL"),
						new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, Shipment.JobNumber)
				});

				Shipment.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>[]
				{
						new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, CustomsModuleCodes.Codes.UsEmanifest),
						new KeyValuePair<string, string>(EventReferenceParameters.Codes.Mode, Core.Constants.TransportModes.Road),
						new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, eManifest.BH_JobReference)
				});

				factory.SetBulkCopyOnTable(CusInBondBillSchema.Constants.TableName, batchSize: HVLVDataRegistry.Instance.HVLVDataBulkCopyBatchSize.Value, fireTriggers: true, checkConstraints: true);
				factory.SetBulkCopyOnTable(JobDocAddressSchema.Constants.TableName, batchSize: HVLVDataRegistry.Instance.HVLVDataBulkCopyBatchSize.Value, checkRowsShouldBePersistent: false, checkConstraints: true);
				factory.SetBulkCopyOnTable(CusInBondHeaderSchema.Constants.TableName, batchSize: HVLVDataRegistry.Instance.HVLVDataBulkCopyBatchSize.Value, checkRowsShouldBePersistent: false, checkConstraints: true);
				factory.SetBulkCopyOnTable(StmALogSchema.Constants.TableName, batchSize: HVLVDataRegistry.Instance.HVLVDataBulkCopyBatchSize.Value, checkRowsShouldBePersistent: false, checkConstraints: true);
			}
		}

		protected override void PopulateDataTarget(ITopLevelDataObject dataObject, DataContextType dataContextType, ZString dataContextKey)
		{
			if (dataObject is UniversalDataBuss.DataObjects.Universal.Shipment shipment
				&& shipment.SubShipmentCollection != null)
			{
				foreach (var subShipment in shipment.SubShipmentCollection)
				{
					var shipmentType = subShipment.ShipmentType.GetCodeAsUpperCase();
					if (shipmentType == Core.Constants.ShipmentTypes.HighVolumeLowValue)
					{
						subShipment.DataContext.AddDataTarget(dataContextType, dataContextKey);
					}
				}
			}
		}
	}
}
