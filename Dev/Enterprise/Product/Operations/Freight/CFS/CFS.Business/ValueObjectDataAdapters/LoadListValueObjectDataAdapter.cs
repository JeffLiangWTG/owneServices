using CargoWise.EntityFramework;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.CFS.Business
{
	public class LoadListValueObjectDataAdapter : ConsolValueObjectDataAdapter<CFSLoadListConsol, CFSShipment, Xsd.Consol>
	{
		protected override void SetForwarders(CFSLoadListConsol consol, Xsd.Consol value, IValueObjectImportContext context)
		{
			Xsd.Organisation org = consol.IsPackLoadList
				? value.ConsolDetail.SendingAgent
				: value.ConsolDetail.ReceivingAgent;

			consol.JK_OH_Forwarder = context.FindOrCreateTempOrganisationPK(org, consol, OrganisationTypes.Forwarder);
		}

		#region Import

		protected override bool ShouldTryToMatchAndConvertQuickBookingBeforeImportingShipment
		{
			get { return false; }
		}

		protected override ShipmentValueObjectDataAdapter<CFSShipment> GetNewShipmentValueObjectDataAdapter(CFSLoadListConsol existingConsol)
		{
			return new CFSShipmentValueObjectDataAdapter<CFSShipment>(existingConsol, TriggeredByEvents);
		}

		protected override void ImportReferenceNumbers(CommonConsol consol, Xsd.Consol value, IValueObjectImportContext context)
		{
		}

		#endregion
	}

	class CFSShipmentValueObjectDataAdapter<TBusinessObject> : ShipmentValueObjectDataAdapter<TBusinessObject> where TBusinessObject : CFSShipment
	{
		public CFSShipmentValueObjectDataAdapter(CFSLoadListConsol existingConsol, EventsWithSourceType triggeredByEvents)
			: base(existingConsol, triggeredByEvents)
		{
		}

		protected override ZQuery GetFindQuery()
		{
			return new ZQuery(JobShipmentSchema.JS_IsCFSRegistered, true);
		}

		protected override void ImportFromValueObjectCore(TBusinessObject shipment, Xsd.Shipment value, IValueObjectImportContext context)
		{
			ImportUniqueConsignRef(shipment, value, context);
			ImportHouseBillIdentifier(shipment, value, context);
			ImportCoLoadMaster(shipment, value, context);
			ImportDocAddresses(shipment, value, context);

			var errorContext = Res.GetString("edb3e09d-e835-4b93-8895-47f30a895b4c", "House Bill '{0}'", shipment.JS_HouseBill);
			StmALogValueObjectDataAdapter.New(shipment, errorContext, TriggeredByEvents).FromXmlCollectionValueObject(value.Events, context);

			if ((value.ShipmentDetailsSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value) && value.ShipmentDetails != null)
			{
				ImportModeAndMovementInfo(shipment, value, context, errorContext);

				shipment.JS_E_ARVInfo.ClearValue();
				shipment.JS_E_DEPInfo.ClearValue();

				ImportCargoInfo(shipment, value, context, errorContext);

				context.SetPropertyInfoValue(shipment.JS_InterimReceiptInfo, value.ShipmentDetails.InterimReceipt, value.ShipmentDetails.InterimReceiptSpecified);

				ImportPackages(shipment, value, context);

				XsdCustomEntryNumbersObjectHelper.ImportFromXsdCustomsEntryNumberCollection(value.ShipmentDetails.CustomsEntryNumbers, shipment.PK, JobShipmentSchema.Constants.TableName, () => shipment.CusEntryNumbers, context);
				XsdPlannedLegObjectHelper.ImportPlannedLegs(shipment.Transports, value.ShipmentDetails.TransportPlan, context, string.Empty);

				if (value.ShipmentDetails.Pickup.DateOfReceipt.IsValid)
				{
					shipment.JS_A_RCV = value.ShipmentDetails.Pickup.DateOfReceipt.ToSmallDateTime();
				}

				if (value.ShipmentDetails.Pickup.CFS.Location.IsValid)
				{
					shipment.JS_WarehouseLocation = value.ShipmentDetails.Pickup.CFS.Location;
				}

				ImportJobInfo(shipment, value, context);
			}

			ImportOrganisationDetails(shipment, value, context);
			ImportNotesFromShipment(shipment, value, context);
			ImporteDocs(shipment, value.Documents, context);
			ImportBilling(shipment, value, context);
			DocDataValueObjectDataAdapter.ImportData(shipment, value.DocData, context);

			AddImportEvent(shipment, value);
		}

		void ImportDocAddresses(TBusinessObject shipment, Xsd.Shipment value, IValueObjectImportContext context)
		{
			if (value.ShipmentDetails.DocAddresses.IsSpecified)
			{
				var helper = new DocAddressValueObjectHelper(string.Empty);
				helper.ImportFromValueObjectCollection(value.ShipmentDetails.DocAddresses.DocAddress, shipment.DocAddresses, context);
			}

			if (value.ShipmentDetails.DocAddresses.DocAddress.GetAddressByType(Xsd.DocAddressAddressType.CRD) == null)
			{
				if (value.ShipmentDetails.Consignor.IsSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
				{
					shipment.ConsignorPK = context.FindOrCreateTempOrganisationPK(value.ShipmentDetails.Consignor, shipment, OrganisationTypes.Consignor);
				}
			}

			if (value.ShipmentDetails.DocAddresses.DocAddress.GetAddressByType(Xsd.DocAddressAddressType.CED) == null)
			{
				if (value.ShipmentDetails.Consignee.IsSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
				{
					shipment.ConsigneePK = context.FindOrCreateTempOrganisationPK(value.ShipmentDetails.Consignee, shipment, OrganisationTypes.Consignee);
				}
			}

			if (value.ShipmentDetails.DocAddresses.DocAddress.GetAddressByType(Xsd.DocAddressAddressType.CRG) == null && value.ShipmentDetails.Pickup.Address.IsSpecified)
			{
				if (value.ShipmentDetails.Pickup.Address.IsSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
				{
					SetDocAddressValuesFromPickupOrDeliveryAddress(value.ShipmentDetails.Pickup.Address, shipment.ConsignorPickupAddress, Res.GetString("39c9a082-ca1d-429c-a978-c486016ad8e0", "Pickup"), context);
				}
			}

			if (value.ShipmentDetails.DocAddresses.DocAddress.GetAddressByType(Xsd.DocAddressAddressType.CEG) == null && value.ShipmentDetails.Deliver.Address.IsSpecified)
			{
				if (value.ShipmentDetails.Deliver.Address.IsSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
				{
					SetDocAddressValuesFromPickupOrDeliveryAddress(value.ShipmentDetails.Deliver.Address, shipment.ConsigneeDeliveryAddress, Res.GetString("674694ed-967d-43ef-b21f-7ccaea47a495", "Delivery"), context);
				}
			}
		}

		void ImportOrganisationDetails(TBusinessObject shipment, Xsd.Shipment value, IValueObjectImportContext context)
		{
			if (value.ShipmentDetails != null)
			{
				if (value.ShipmentDetails.Deliver.CartageCompany.IsSpecified)
				{
					shipment.DocsAndCartage.DeliveryCartageCoPK = context.FindOrCreateTempOrganisationPK(value.ShipmentDetails.Deliver.CartageCompany, shipment, OrganisationTypes.Carrier);
				}
				else
				{
					DefaultImportCartage(shipment, value);
				}

				if (value.ShipmentDetails.Pickup.CartageCompany.IsSpecified)
				{
					shipment.DocsAndCartage.PickupCartageCoPK = context.FindOrCreateTempOrganisationPK(value.ShipmentDetails.Pickup.CartageCompany, shipment, OrganisationTypes.Carrier);
				}
				else
				{
					DefaultExportCartage(shipment, value);
				}
			}
		}

		DocDataValueObjectDataAdapter DocDataValueObjectDataAdapter
		{
			get { return docDataValueObjectDataAdapter ?? (docDataValueObjectDataAdapter = new DocDataValueObjectDataAdapter()); }
		}
		DocDataValueObjectDataAdapter docDataValueObjectDataAdapter;
	}
}
