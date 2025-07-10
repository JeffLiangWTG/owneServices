using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.eTail.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class HVLVConsignmentDataObjectReader : ShipmentDataObjectReader<HVLVConsignment>
	{
		public HVLVConsignmentDataObjectReader(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, HVLVConsignment existingConsignmentBO = null, ForwardingShipment shipmentBO = null)
			: base(dataObject, logger, factory)
		{
			this.existingConsignmentBO = existingConsignmentBO;
			this.shipmentBO = shipmentBO;
		}

		readonly HVLVConsignment existingConsignmentBO;
		readonly ForwardingShipment shipmentBO;

		public override DataContextType DataContextType => DataContextType.HVLVConsignment;

		protected override HVLVConsignment GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return existingConsignmentBO;
		}

		protected override IMatchingBusinessEntityFinder<HVLVConsignment> GetCombinedReferenceMatcher() => null;

		protected override void PopulateBusinessObject(HVLVConsignment consignmentBO)
		{
			using (consignmentBO.SetIsBeingImportedFromUniversalXml())
			{
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_WaybillNumber, dataObject.WayBillNumber);
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ShipperReference, dataObject.OwnerRef);
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_WeightUQ, dataObject.TotalWeightUnit?.Code);
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ManifestedWeight, dataObject.ManifestedWeight);
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ActualWeight, dataObject.TotalWeight);
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_VolumeUQ, dataObject.TotalVolumeUnit?.Code);
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ManifestedVolume, dataObject.ManifestedVolume);
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ActualVolume, dataObject.TotalVolume);
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_GoodsDescription, dataObject.GoodsDescription);
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_IsHazardous, dataObject.IsHazardous);
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_IsSignatureRequired, dataObject.IsSignatureRequired);
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_AuthorityToLeave, dataObject.IsAuthorizedToLeave);
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_IsTracked, dataObject.IsTracked);
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_CarrierAccountNumber, dataObject?.CarrierAccount?.AccountNumber);
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_PL_NKLastMileCarrierServiceLevel, dataObject.CarrierServiceLevel?.Code);
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_INCO, dataObject.ShipmentIncoTerm?.Code);
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_RS_NKServiceLevel, dataObject.ServiceLevel?.Code);
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_InsuranceValue, dataObject.InsuranceValue);
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_TransportValue, dataObject.TransportValue);

				PopulateGoodsValue(consignmentBO);

				if (dataObject.VendorIdentifier.HasValue && !dataObject.VendorIdentifier.Value.IsEmpty)
				{
					consignmentBO.HVC_VendorIdentifier = dataObject.VendorIdentifier.Value;
				}

				var addInfoCollection = dataObject.AddInfoCollection;
				if (addInfoCollection != null)
				{
					var isGSTPrePaid = addInfoCollection.GetZStringValue(Constants.AddInfoKeys.IsGSTPrePaid);
					if (isGSTPrePaid.HasValue && YesNoList.IsYes(isGSTPrePaid.Value))
					{
						SetValue(consignmentBO, HVLVConsignmentSchema.HVC_IsTaxPrePaid, ZBool.True);
					}
				}

				PopulateInstructions(consignmentBO);
				PopulateNotes(consignmentBO);
				PopulatePackingLevelFields(consignmentBO);
				PopulateOrganizations(consignmentBO);
				PopulateItems(consignmentBO);
				PopulateDeclarationReference(consignmentBO);
				PopulateReturnLocationDetails(consignmentBO);
				PopulateAdditionalReferences(consignmentBO);
			}
		}

		void PopulateGoodsValue(HVLVConsignment consignmentBO)
		{
			var goodsValue = dataObject.GoodsValue ?? 0;
			var goodsValueCurrencyCode = dataObject.GoodsValueCurrency?.Code ?? "";

			var currencyCodes = factory.Load<RefCurrency>(new ZQuery()).Select(currency => currency.RX_Code).ToList();

			if (goodsValue < 0)
			{
				throw new DataObjectReadFailureException(Res.GetString("df6663f0-fdb8-4100-b7fb-f198b9ed5bbe", "Consignment goods value must be greater than or equal to 0, current value is: {0}", goodsValue));
			}
			else if (goodsValue > 0 && goodsValueCurrencyCode.IsEmpty)
			{
				throw new DataObjectReadFailureException(Res.GetString("16eb9e92-aa04-4cb0-9c29-1d42b99a4ef2", "Consignment goods value currency must not be empty when goods value is defined"));
			}
			else if (!goodsValueCurrencyCode.IsEmpty && !currencyCodes.Contains(goodsValueCurrencyCode))
			{
				throw new DataObjectReadFailureException(Res.GetString("4ca00084-57f9-486e-aef5-38ecaed0d2b9", "Consignment goods value currency is invalid, invalid currency is:{0}", goodsValueCurrencyCode));
			}

			SetValue(consignmentBO, HVLVConsignmentSchema.HVC_GoodsValue, dataObject.GoodsValue);
			SetValue(consignmentBO, HVLVConsignmentSchema.HVC_RX_NKGoodsValueCurrency, dataObject.GoodsValueCurrency?.Code);
		}

		void PopulateInstructions(HVLVConsignment consignmentBO)
		{
			var instruction = dataObject.InstructionCollection?.FirstOrDefault();
			if (instruction != null && !instruction.ServiceInstruction.GetValueOrDefault().IsEmpty)
			{
				consignmentBO.HVC_ConsigneeInstructions = instruction.ServiceInstruction.Value;
			}
		}

		void PopulateNotes(HVLVConsignment consignmentBO)
		{
			if (dataObject.NoteCollection != null)
			{
				foreach (var note in dataObject.NoteCollection)
				{
					var reader = new NoteDataObjectReader(note, logger, factory, consignmentBO);
					consignmentBO.Notes.Add(reader.ReadIntoBusinessObject());
				}
			}
		}

		void PopulateReturnLocationDetails(HVLVConsignment consignmentBO)
		{
			PopulateAddressByAddressType(
				nameof(DocAddressType.ReturnAddress),
				(matchedAddress) => SetValue(consignmentBO, HVLVConsignmentSchema.HVC_OA_ReturnLocation, matchedAddress.PK),
				(matchedContact) => SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ReturnContact, matchedContact.OC_ContactName),
				(addressDataObject) =>
				{
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_OA_ReturnLocation, ZGuid.Empty);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ReturnAddress1, addressDataObject.Address1);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ReturnAddress2, addressDataObject.Address2);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ReturnCity, addressDataObject.City);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ReturnState, (ZString?)addressDataObject.State);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ReturnPostcode, addressDataObject.Postcode);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ReturnName, addressDataObject.CompanyName);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_RN_NKReturnCountryCode, addressDataObject.Country?.Code);
				},
				(addressDataObject) =>
				{
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ReturnContact, addressDataObject.Contact);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ReturnEmail, addressDataObject.Email);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ReturnPhone, addressDataObject.Phone);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ReturnMobile, addressDataObject.Mobile);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ReturnFax, addressDataObject.Fax);
				},
				true
			);
		}

		void PopulateDeclarationReference(HVLVConsignment consignmentBO)
		{
			if (dataObject.CustomsReferenceCollection != null && dataObject.CustomsReferenceCollection.Count != 0)
			{
				foreach (var referenceDO in dataObject.CustomsReferenceCollection.Where(r => r.Type.Code.GetValueOrDefault().Equals(nameof(DataContext.Declaration))))
				{
					if (string.IsNullOrEmpty(referenceDO.Reference))
					{
						continue;
					}

					var declaration = factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, referenceDO.Reference));
					if (declaration != null)
					{
						var referenceSubType = referenceDO.SubType.Code.GetValueOrDefault();
						if (referenceSubType.IsEmpty || referenceSubType.Equals(FreightShipmentDirection.Code.Import))
						{
							SetValue(consignmentBO, HVLVConsignmentSchema.HVC_JE_ImportDeclaration, declaration.PK);
						}
						else if (referenceSubType.Equals(FreightShipmentDirection.Code.Export))
						{
							SetValue(consignmentBO, HVLVConsignmentSchema.HVC_JE_ExportDeclaration, declaration.PK);
						}
					}
				}
			}
		}

		void PopulatePackingLevelFields(HVLVConsignment consignmentBO)
		{
			var packingLines = dataObject.PackingLineCollection;

			if (packingLines != null)
			{
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_RequiresFumigation, packingLines.Any(x => x.RequiresFumigationCertificate.GetValueOrDefault()));
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_IsPersonalEffects, packingLines.Any(x => x.IsPersonalEffects.GetValueOrDefault()));
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_IsTimber, packingLines.Any(x => x.IsTimber.GetValueOrDefault()));
				SetValue(consignmentBO, HVLVConsignmentSchema.HVC_IsPerishable, packingLines.Any(x => x.IsPerishable.GetValueOrDefault()));

				var uniqueUNDGClasses = packingLines
					.Where(x => x.UNDGCollection != null)
					.SelectMany(x => x.UNDGCollection)
					.Select(x => x.IMOClass)
					.Where(x => !x.GetValueOrDefault().IsEmpty)
					.Distinct();

				if (uniqueUNDGClasses.IsCountEqualTo(1))
				{
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_UndgClass, uniqueUNDGClasses.Single());
				}
			}
		}

		void PopulateOrganizations(HVLVConsignment consignmentBO)
		{
			PopulateAddressByAddressType(nameof(DocAddressType.ArrivalCFSAddress),
				(destinationDepot) => SetValue(consignmentBO, HVLVConsignmentSchema.HVC_OA_DestinationDepot, destinationDepot.PK));

			PopulateAddressByAddressType(AddressTypes.DeliveryLocalCartage,
				(lastMileCarrier) => SetValue(consignmentBO, HVLVConsignmentSchema.HVC_OH_LastMileCarrier, lastMileCarrier.OA_OH));

			PopulateAddressByAddressType(nameof(DocAddressType.CarrierBookingAgent),
				(lastMileCarrierAgent) => SetValue(consignmentBO, HVLVConsignmentSchema.HVC_OH_LastMileCarrierBookingAgent, lastMileCarrierAgent.OA_OH));

			PopulateConsignee(consignmentBO);
			PopulateShipper(consignmentBO);
		}

		void PopulateConsignee(HVLVConsignment consignmentBO)
		{
			PopulateAddressByAddressType(
				nameof(DocAddressType.ConsigneeDocumentaryAddress),
				(matchedAddress) => SetValue(consignmentBO, HVLVConsignmentSchema.HVC_OA_ConsigneeAddress, matchedAddress.PK),
				(matchedContact) => SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ConsigneeContact, matchedContact.OC_ContactName),
				(addressDataObject) =>
				{
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_OA_ConsigneeAddress, ZGuid.Empty);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ConsigneeName, addressDataObject.CompanyName);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ConsigneeAddress1, addressDataObject.Address1);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ConsigneeAddress2, addressDataObject.Address2);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ConsigneeCity, addressDataObject.City);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ConsigneeState, (ZString?)addressDataObject.State);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ConsigneePostcode, addressDataObject.Postcode);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_RN_NKConsigneeCountryCode, addressDataObject.Country?.Code);
				},
				(addressDataObject) =>
				{
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ConsigneeContact, addressDataObject.Contact);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ConsigneeEmail, addressDataObject.Email);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ConsigneePhone, addressDataObject.Phone);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ConsigneeMobile, addressDataObject.Mobile);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ConsigneeFax, addressDataObject.Fax);
				},
				true
			);
		}

		void PopulateShipper(HVLVConsignment consignmentBO)
		{
			PopulateAddressByAddressType(
				nameof(DocAddressType.ConsignorDocumentaryAddress),
				(matchedAddress) => SetValue(consignmentBO, HVLVConsignmentSchema.HVC_OA_ShipperAddress, matchedAddress.PK),
				(matchedContact) => SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ShipperContact, matchedContact.OC_ContactName),
				(addressDataObject) =>
				{
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_OA_ShipperAddress, ZGuid.Empty);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ShipperName, addressDataObject.CompanyName);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ShipperAddress1, addressDataObject.Address1);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ShipperAddress2, addressDataObject.Address2);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ShipperCity, addressDataObject.City);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ShipperState, (ZString?)addressDataObject.State);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ShipperPostcode, addressDataObject.Postcode);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_RN_NKShipperCountryCode, addressDataObject.Country?.Code);
				},
				(addressDataObject) =>
				{
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ShipperContact, addressDataObject.Contact);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ShipperEmail, addressDataObject.Email);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ShipperPhone, addressDataObject.Phone);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ShipperMobile, addressDataObject.Mobile);
					SetValue(consignmentBO, HVLVConsignmentSchema.HVC_ShipperFax, addressDataObject.Fax);
				},
				true
			);
		}

		void PopulateItems(HVLVConsignment consignmentBO)
		{
			if (dataObject.PackingLineCollection != null)
			{
				foreach (var packingLine in dataObject.PackingLineCollection)
				{
					new HVLVItemDataObjectReader(dataObject, consignmentBO, packingLine, logger, factory, shipmentBO).ReadIntoBusinessObject();
				}
			}
		}

		void PopulateAdditionalReferences(HVLVConsignment consignmentBO)
		{
			var referenceNumbers = consignmentBO.CustomsReferenceNumbers.ToArray<CusEntryNumber>();

			if (dataObject.AdditionalReferenceCollection != null)
			{
				var collectionContent = dataObject.AdditionalReferenceCollection.Content;

				if (collectionContent == CollectionContent.Complete)
				{
					consignmentBO.CustomsReferenceNumbers.RemoveAndDeleteAll();
				}

				foreach (var additionalReference in dataObject.AdditionalReferenceCollection)
				{
					var cusReferenceNumber = new AdditionalReferenceDataObjectReader(additionalReference, logger, factory, referenceNumbers).ReadIntoBusinessObject();
					consignmentBO.CustomsReferenceNumbers.Add(cusReferenceNumber);
				}
			}
		}

		string GetDataObjectCacheKey(string key)
		{
			return string.Format(Culture.Invariant, "{0}_{1}", nameof(HVLVConsignmentDataObjectReader), key);
		}

		void PopulateAddressByAddressType(
			string addressType,
			Action<OrgAddress> setAddress,
			Action<OrgContact> setContact = null,
			Action<OrganizationAddress> setFreeTextAddressFields = null,
			Action<OrganizationAddress> setFreeTextContactFields = null,
			bool checkOverridden = false)
		{
			var address = dataObject.OrganizationAddressCollection.FirstOrDefault(addressType);

			if (address == null)
			{
				return;
			}

			var hasMatchedAddress = false;
			var hasMatchedContact = false;

			if (setAddress != null && (!checkOverridden || address.AddressOverride.GetValueOrDefault() == false))
			{
				var addressCacheKey = GetDataObjectCacheKey(address.AddressType + address.OrganizationCode);
				var matchedAddress = factory.GetCachedValue(addressCacheKey,
					() => new OrganisationDataObjectReader(address, logger, factory).GetMatched());

				if (matchedAddress != null)
				{
					setAddress(matchedAddress);
					hasMatchedAddress = true;

					if (setContact != null
						&& address.Contact.GetValueOrDefault() is var contactName
						&& !string.IsNullOrEmpty(contactName)
						&& matchedAddress.Header != null)
					{
						var query = new ZQuery(OrgContactSchema.OC_ContactName, contactName);
						var contact = matchedAddress.Header.Contacts.Find(query).FirstOrDefault() as OrgContact;

						if (contact != null)
						{
							setContact(contact);
							hasMatchedContact = true;
						}
					}
				}
			}

			if (!hasMatchedAddress && setFreeTextAddressFields != null)
			{
				setFreeTextAddressFields(address);
			}

			if (!hasMatchedContact && setFreeTextContactFields != null)
			{
				setFreeTextContactFields(address);
			}
		}
	}
}
