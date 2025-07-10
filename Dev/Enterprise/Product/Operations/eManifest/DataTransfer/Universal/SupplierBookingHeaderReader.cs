using CargoWise.Types;
using Enterprise.eManifest.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eManifest.DataTransfer.Universal
{
	public class SupplierBookingHeaderReader : ShipmentDataObjectReader<SupplierBookingHeader>
	{
		public SupplierBookingHeaderReader(UniversalShipment headerDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(headerDataObject, logger, factory)
		{
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.eManifest; }
		}

		protected override IMatchingBusinessEntityFinder<SupplierBookingHeader> GetCombinedReferenceMatcher()
		{
			return null;
		}

		protected override SupplierBookingHeader GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return null;
		}

		protected override void PopulateBusinessObject(SupplierBookingHeader targetBO)
		{
			targetBO.ShouldSetSupplierReference = true;
			SetValue(targetBO, SupplierBookingHeaderSchema.DH_GrossWeightInKg, dataObject.TotalWeight);
			SetValue(targetBO, SupplierBookingHeaderSchema.DH_CubicInM3, dataObject.TotalVolume);
			SetValue(targetBO, SupplierBookingHeaderSchema.DH_PiecesManifested, dataObject.TotalNoOfPieces);

			PopulateConsignor(targetBO);
			PopulateDispatchAddress(targetBO);

			if (dataObject.SubShipmentCollection == null)
			{
				return;
			}

			int i = 0;
			foreach (var shipmentDataObject in dataObject.SubShipmentCollection)
			{
				new SupplierBookingLineReader(shipmentDataObject, logger, factory, targetBO, ++i).ReadIntoBusinessObject();
			}

			var targetPK = targetBO.PK.ToGuid();
			factory.AddPostSaveAction(() => UpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo.Execute(targetPK));
		}

		void PopulateConsignor(SupplierBookingHeader targetBO)
		{
			var consignorDataObject = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ConsignorDocumentaryAddress));
			if (consignorDataObject != null)
			{
				var consignorAddress = new OrganisationDataObjectReader(consignorDataObject, logger, factory).GetMatched(targetBO, OrganisationTypes.Consignor);
				if (consignorAddress != null)
				{
					SetValue(targetBO, SupplierBookingHeaderSchema.DH_OA_Consignor, consignorAddress.PK);
				}
			}

			if (targetBO.Consignor == null)
			{
				var errorMessage = new ZStringBuilder();
				errorMessage.Append(Res.GetString("AB2B55B1-C0B0-4726-9D3C-F05417C61FD8", "Cannot import Supplier Booking Header"));

				if (consignorDataObject == null)
				{
					errorMessage.Append(Res.GetString("34ED89FB-AA70-48B9-9505-06EEAFC1BA0F", "No Consignor Address was provided."));
				}
				else
				{
					consignorDataObject.AddAddressErrorMessage(Res.GetString("01C4DA54-6C83-41B2-8325-ADFC595F3EAA", "Consignor"), errorMessage);
				}

				throw new DataObjectReadFailureException(errorMessage.ToStringWithNewLineBetweenAppends());
			}
		}

		void PopulateDispatchAddress(SupplierBookingHeader targetBO)
		{
			var dispatchAddressDataObject = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.PickUpAddress));
			if (dispatchAddressDataObject != null)
			{
				var dispatchAddress = new OrganisationDataObjectReader(dispatchAddressDataObject, logger, factory).GetMatched(targetBO, OrganisationTypes.Consignor);
				if (dispatchAddress != null)
				{
					SetValue(targetBO, SupplierBookingHeaderSchema.DH_OA_DispatchAddress, dispatchAddress.PK);
				}
			}
		}

		protected override bool ModuleHasReferenceAndPartyIDMatchingEnabled
		{
			get { return true; }
		}
	}
}
