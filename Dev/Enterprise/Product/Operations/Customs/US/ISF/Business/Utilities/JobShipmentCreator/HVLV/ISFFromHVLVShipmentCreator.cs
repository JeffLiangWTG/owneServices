using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Registry.Business.CustomsReferenceNumberType;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFFromHVLVShipmentCreator : ISFFromShipmentCreator, IUSImporterSecurityFilingCreator, IISFFromShipmentCreator
	{
		public ISFFromHVLVShipmentCreator(ForwardingShipment shipment)
			: this(shipment, null)
		{
		}

		public ISFFromHVLVShipmentCreator(ForwardingShipment shipment, Action<string, int> progressUpdate)
			: base(shipment.Factory)
		{
			ShipmentPK = shipment.PK;
			uniqueISFLines = new HashSet<string>();
			consignmentToHeaderMappings = new Dictionary<IHVLVISFBillInfoProvider, CusISFHeader>();

			isFirstConsignment = true;

			createdNewHeaders = new RelatedJobCollection(Factory);
			this.progressUpdate = progressUpdate;
			totalConsignmentsCount = shipment.HVLVConsignments.Count();
		}

		readonly HashSet<string> uniqueISFLines;
		readonly Action<string, int> progressUpdate;

#if DEBUG
		public
#endif
		readonly Dictionary<IHVLVISFBillInfoProvider, CusISFHeader> consignmentToHeaderMappings;

#if DEBUG
		public
#endif
		readonly RelatedJobCollection createdNewHeaders;

		readonly int totalConsignmentsCount;
		int processedConsignmentsCount;

		bool isFirstConsignment;
		IHVLVISFBillInfoProvider currentConsignment;

		protected override ZString ShipmentType => ShipmentTypeList.Codes.Informal;

		protected override void CreateFromShipment(ForwardingShipment shipmentInPassedInFactory, CusISFHeader header)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				ExtraOperationOnHeaderForTest?.Invoke(header);
			}
#endif

			foreach (IHVLVISFBillInfoProvider consignment in shipmentInPassedInFactory.HVLVConsignments)
			{
				processedConsignmentsCount++;
				currentConsignment = consignment;

				if (isFirstConsignment)
				{
					base.CreateFromShipment(shipmentInPassedInFactory, header);
					isFirstConsignment = false;
					CreateBuyingParty(header);
					CreateSellingParty(header);
					SetShipmnetPK(shipmentInPassedInFactory.PK, header);
					createdNewHeaders.Add(header);
					consignmentToHeaderMappings.Add(consignment, header);
				}
				else
				{
					var newHeader = CreateNewHeaderWithDefaultShipmentType(header.Factory);
					base.CreateFromShipment(shipmentInPassedInFactory, newHeader);
					SetShipmnetPK(shipmentInPassedInFactory.PK, newHeader);
					CreateBuyingParty(newHeader);
					CreateSellingParty(newHeader);
					createdNewHeaders.Add(newHeader);
					consignmentToHeaderMappings.Add(consignment, newHeader);
				}

				progressUpdate?.Invoke(Res.GetString("8bb41fc4-04b6-491f-bc5a-da4f595ddcee", "[{0} / {1}] Consignments processed",
					processedConsignmentsCount,
					totalConsignmentsCount),
					(int)(processedConsignmentsCount * 100F / totalConsignmentsCount));
			}
		}

#if DEBUG
		public Action<CusISFHeader> ExtraOperationOnHeaderForTest;
#endif

		protected override void OnFactorySaving(BusinessObjectFactory factory)
		{
			AddHVLVTransferredLogForShipment();
			PopulateJobReferenceAndTRFEventForISFHeadersIfNeeded(factory);
			PopulateCustomsReferenceNumberForConsignments();
		}

		void PopulateJobReferenceAndTRFEventForISFHeadersIfNeeded(BusinessObjectFactory factory)
		{
			var isfHeadersWithoutJobReferences = createdNewHeaders.Cast<CusISFHeader>().Where(isf => isf.BF_JobReference.IsEmpty);
			if (isfHeadersWithoutJobReferences.Any())
			{
				var isfTarget = new ImporterSecurityFilingNumberGeneratorTarget();

				var generator = new NumberGenerator();
				generator.Factory = factory;
				generator.Context = new NumberGeneratorContext();
				generator.BaseFountain = Env.NumberFountains.ImporterSecurityFilingReference;
				generator.FountainGetter = Env.NumberFountains.GetImporterSecurityFilingReferenceGeneratorFountain;
				generator.PrimaryTarget = isfTarget;
				generator.ValueProviders.AddRange(new StandardValueSource());

				var isfJobNumbers = new Stack(generator.GenerateNumbers(isfTarget, isfHeadersWithoutJobReferences.Count()).Reverse().ToArray());

				foreach (var isfHeader in isfHeadersWithoutJobReferences)
				{
					isfHeader.BF_JobReference = isfJobNumbers.Pop().ToString();
					isfHeader.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>[]
					{
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber, Shipment.JobNumber),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.ShipmentTypes.HighVolumeLowValue)
					});
				}
			}
		}

		void PopulateCustomsReferenceNumberForConsignments()
		{
			foreach (var consignment in consignmentToHeaderMappings.Keys)
			{
				var reference = consignment.CustomsReferenceNumbers.AddNew();
				reference.CE_EntryType = CustomsAdditionalReferenceNumbersCodes.ImportSecurityFilingReference;
				reference.CE_EntryNum = consignmentToHeaderMappings[consignment].BF_JobReference;
			}
		}

		void AddHVLVTransferredLogForShipment()
		{
			Shipment.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "ISF")
			});
		}

		CusISFHeader CreateNewHeaderWithDefaultShipmentType(BusinessObjectFactory factoryToCreateFor)
		{
			var result = factoryToCreateFor.New<CusISFHeader>();
			result.BF_ShipmentType = ShipmentType;
			return result;
		}

		protected override void ImportLinesFromShipment(ForwardingShipment shipment, CusISFHeader header)
		{
			var consignment = currentConsignment;

			var manufacturerAddressPK = ZGuid.Empty;
			foreach (var manufacturerAddress in header.ManufacturerAddresses)
			{
				if (manufacturerAddress.E2_OA_Address == consignment.HVC_OA_ShipperAddress || AddressImportedAlready(manufacturerAddress, consignment, true))
				{
					manufacturerAddressPK = manufacturerAddress.PK;
					break;
				}
			}

			foreach (var itemLine in consignment.Items.OfType<IHVLVISFItemInfoProvider>().SelectMany(item => item.Lines.OfType<IHVLVItemLine>()))
			{
				var key = string.Format("{0}-{1}-{2}", itemLine.HVS_DestinationTariff, manufacturerAddressPK, itemLine.HVS_RN_NKOriginCountryCode);

				if (!uniqueISFLines.Contains(key))
				{
					var billNumber = GenerateReferenceDataBillNumber();
					var line = header.Lines.AddNew();

					line.BL_ManufacturerDocAddressPK = manufacturerAddressPK;
					line.BL_TextProductCode = itemLine.HVS_ProductCode;
					line.BL_HarmonisedNum = header.GetHarmonisedNumAsRequired(TariffFormatterDecider.GetByCountryCode(shipment.Destination?.Country.Code ?? string.Empty).Format(itemLine.HVS_DestinationTariff));
					line.BL_RN_NKGoodsOrigin = itemLine.HVS_RN_NKOriginCountryCode;
					line.BL_BB_Bill = header.ReferenceDatas.Where(x => x.BB_BillNum == billNumber)?.FirstOrDefault()?.PK ?? ZGuid.Empty;

					uniqueISFLines.Add(key);
				}
			}
		}

		protected override void ImportContainersFromShipment(ForwardingConsol consol, ForwardingShipment shipment, CusISFHeader header)
		{
			if (consol != null)
			{
				var containers = consol.Containers.Where(x => !x.IsDeleted).OfType<ForwardingContainer>();
				foreach (var container in containers)
				{
					var equipment = header.Equipments.AddNew();
					equipment.BE_EquipCode = USContainerCodeList.Codes.CN;
					equipment.BE_ContainerNum = container.JC_ContainerNum;
					if (container.Container != null)
					{
						equipment.BE_ContainerISO = container.Container.RC_ISOType;
					}
				}
			}
		}

		string GenerateReferenceDataBillNumber()
		{
			var waybillNumber = currentConsignment.HVC_WaybillNumber;
			return CreateBillNumberWithSCAC(Shipment, waybillNumber, Shipment.TransportMode);
		}

		protected override void ImportReferenceDataFromShipment(ForwardingShipment shipmentInPassedInFactory, CusISFHeader header)
		{
			var referenceData = header.ReferenceDatas.AddNew();
			referenceData.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			referenceData.BB_BillNum = GenerateReferenceDataBillNumber();
		}

		protected override void ImportLowValueDetailsFromShipment(ForwardingShipment shipmentInPassedInFactory, CusISFHeader header)
		{
			var consignment = currentConsignment;
			var items = consignment.Items.OfType<IHVLVISFItemInfoProvider>();

			header.BF_ShipmentSubType = ShipmentSubTypeList.Codes.LowValueEntriesShipments;
			header.BF_EstimatedQuantity = items.Count();
			header.BF_EstimatedQuantityUQ = ShippingOrPackingingUnitList.PiecesCode;
			header.BF_EstimatedValue = items.SelectMany(item => item.Lines.OfType<IHVLVItemLine>()).Sum(line => line.HVS_CustomsValue);

			var weightUnit = (ZString)Constants.Weight.Kilograms;
			var weight = (ZDecimal)TotalCalculation.GetTotalWeight(
						items,
						x => x.HVI_ActualWeight != 0 ? x.HVI_ActualWeight : x.HVI_ManifestedWeight,
						x => x.Consignment.HVC_WeightUQ,
						weightUnit);
			new WeightConversionStrategy().ReScale(ref weight, ref weightUnit, HVLVConsignmentSchema.HVC_ActualWeight.Precision, HVLVConsignmentSchema.HVC_ActualWeight.Scale);

			header.BF_EstimatedWeight = (ZInt)weight;
			header.BF_EstimatedWeightUQ = weightUnit;
		}

		protected override void ImportFromConsol(ForwardingConsol consol, CusISFHeader header)
		{
			base.ImportFromConsol(consol, header);
			var consolCFSAddress = consol.GetDepartureCFSDocAddress;
			if (consolCFSAddress != null)
			{
				header.StuffingLocation.E2_AddressOverride = false;
				header.StuffingLocation.E2_OA_Address = consolCFSAddress.E2_OA_Address;
			}
		}

		protected override void ImportImporterAndConsigneeCodeTypeFromShipment(ForwardingShipment shipmentInPassedInFactory, CusISFHeader header)
		{
			header.BF_ImporterCodeType = FindCodeType(header.BF_ImporterCode);
			header.BF_ConsigneeCodeType = FindCodeType(header.BF_ConsigneeCode);
		}

		protected override void CreateManufacturers(CusISFHeader header, ForwardingShipment shipment, ForwardingConsol consol, JobDeclaration declaration)
		{
			var consignment = currentConsignment;

			var shouldAddNew = true;
			if (!consignment.HVC_OA_ShipperAddress.IsEmpty)
			{
				foreach (var manufacturerAddress in header.ManufacturerAddresses)
				{
					if (manufacturerAddress.E2_OA_Address == consignment.HVC_OA_ShipperAddress)
					{
						shouldAddNew = false;
						break;
					}
				}

				if (shouldAddNew)
				{
					var manufacturerAddress = header.ManufacturerAddresses.AddNew();
					using (manufacturerAddress.SuspendSettingHasChangesIncludingChildren())
					{
						manufacturerAddress.E2_ValidationStatus = AddressValidationStatus.ManuallyVerified;
						manufacturerAddress.E2_AddressOverride = false;
						manufacturerAddress.E2_OA_Address = consignment.HVC_OA_ShipperAddress;
					}
				}
			}
			else
			{
				foreach (var manufacturerAddress in header.ManufacturerAddresses)
				{
					if (AddressImportedAlready(manufacturerAddress, consignment, true))
					{
						shouldAddNew = false;
						break;
					}
				}

				if (shouldAddNew)
				{
					var manufacturerAddress = header.ManufacturerAddresses.AddNew();
					using (manufacturerAddress.SuspendSettingHasChangesIncludingChildren())
					{
						manufacturerAddress.E2_AddressOverride = true;
						manufacturerAddress.E2_ValidationStatus = AddressValidationStatus.ManuallyVerified;
						manufacturerAddress.E2_CompanyName = consignment.HVC_ShipperName.ToUpper();
						manufacturerAddress.E2_Address1 = consignment.HVC_ShipperAddress1.SubstringSafe(0, JobDocAddressSchema.E2_Address1.MaxLength).ToUpper();
						manufacturerAddress.E2_Address2 = consignment.HVC_ShipperAddress2.SubstringSafe(0, JobDocAddressSchema.E2_Address2.MaxLength).ToUpper();
						manufacturerAddress.E2_Contact = consignment.HVC_ShipperContact.ToUpper();
						manufacturerAddress.E2_City = consignment.HVC_ShipperCity.ToUpper();
						manufacturerAddress.E2_State = consignment.HVC_ShipperState.Substring(0, JobDocAddressSchema.E2_State.MaxLength).ToUpper();
						manufacturerAddress.E2_Postcode = consignment.HVC_ShipperPostcode.Substring(0, JobDocAddressSchema.E2_Postcode.MaxLength).ToUpper();
						manufacturerAddress.E2_RN_NKCountryCode = consignment.HVC_RN_NKShipperCountryCode;
						manufacturerAddress.E2_Phone = consignment.HVC_ShipperPhone.ToUpper();
						manufacturerAddress.E2_Mobile = consignment.HVC_ShipperMobile.ToUpper();
						manufacturerAddress.E2_Fax = consignment.HVC_ShipperFax.ToUpper();
						manufacturerAddress.E2_Email = consignment.HVC_ShipperEmail.ToUpper();
					}
				}
			}
		}

		bool AddressImportedAlready(JobDocAddress address, IHVLVISFBillInfoProvider consignment, bool isComparedToShipper)
		{
			if (isComparedToShipper)
			{
				if (
					string.Equals(address.E2_CompanyName, consignment.HVC_ShipperName, StringComparison.OrdinalIgnoreCase)
					&& string.Equals(address.E2_Address1, consignment.HVC_ShipperAddress1.SubstringSafe(0, JobDocAddressSchema.E2_Address1.MaxLength), StringComparison.OrdinalIgnoreCase)
					&& string.Equals(address.E2_Address2, consignment.HVC_ShipperAddress2.SubstringSafe(0, JobDocAddressSchema.E2_Address2.MaxLength), StringComparison.OrdinalIgnoreCase)
					&& string.Equals(address.E2_Contact, consignment.HVC_ShipperContact, StringComparison.OrdinalIgnoreCase)
					&& string.Equals(address.E2_City, consignment.HVC_ShipperCity, StringComparison.OrdinalIgnoreCase)
					&& string.Equals(address.E2_State, consignment.HVC_ShipperState.Substring(0, JobDocAddressSchema.E2_State.MaxLength), StringComparison.OrdinalIgnoreCase)
					&& string.Equals(address.E2_Postcode, consignment.HVC_ShipperPostcode.Substring(0, JobDocAddressSchema.E2_Postcode.MaxLength), StringComparison.OrdinalIgnoreCase)
					&& address.E2_RN_NKCountryCode == consignment.HVC_RN_NKShipperCountryCode
					&& address.E2_Phone == consignment.HVC_ShipperPhone
					&& address.E2_Mobile == consignment.HVC_ShipperMobile
					&& address.E2_Fax == consignment.HVC_ShipperFax
					&& string.Equals(address.E2_Email, consignment.HVC_ShipperEmail, StringComparison.OrdinalIgnoreCase)
				)
				{
					return true;
				}
			}
			else
			{
				if (
					string.Equals(address.E2_CompanyName, consignment.HVC_ConsigneeName, StringComparison.OrdinalIgnoreCase)
					&& string.Equals(address.E2_Address1, consignment.HVC_ConsigneeAddress1.SubstringSafe(0, JobDocAddressSchema.E2_Address1.MaxLength), StringComparison.OrdinalIgnoreCase)
					&& string.Equals(address.E2_Address2, consignment.HVC_ConsigneeAddress2.SubstringSafe(0, JobDocAddressSchema.E2_Address2.MaxLength), StringComparison.OrdinalIgnoreCase)
					&& string.Equals(address.E2_Contact, consignment.HVC_ConsigneeContact, StringComparison.OrdinalIgnoreCase)
					&& string.Equals(address.E2_City, consignment.HVC_ConsigneeCity, StringComparison.OrdinalIgnoreCase)
					&& string.Equals(address.E2_State, consignment.HVC_ConsigneeState.Substring(0, JobDocAddressSchema.E2_State.MaxLength), StringComparison.OrdinalIgnoreCase)
					&& string.Equals(address.E2_Postcode, consignment.HVC_ConsigneePostcode.Substring(0, JobDocAddressSchema.E2_Postcode.MaxLength), StringComparison.OrdinalIgnoreCase)
					&& address.E2_RN_NKCountryCode == consignment.HVC_RN_NKConsigneeCountryCode
					&& address.E2_Phone == consignment.HVC_ConsigneePhone
					&& address.E2_Mobile == consignment.HVC_ConsigneeMobile
					&& address.E2_Fax == consignment.HVC_ConsigneeFax
					&& string.Equals(address.E2_Email, consignment.HVC_ConsigneeEmail, StringComparison.OrdinalIgnoreCase)
				)
				{
					return true;
				}
			}

			return false;
		}

		string FindCodeType(ZString cusCode)
		{
			var result = string.Empty;
			if (OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.USACodeTypes.EmployerIdentificationNumber, cusCode) != null)
			{
				result = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			}
			else if (OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.USACodeTypes.CBPAssignedNumber, cusCode) != null)
			{
				result = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			}
			else if (OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.USACodeTypes.SocialSecurityNumber, cusCode) != null)
			{
				result = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			}

			return result;
		}

		void SetShipmnetPK(ZGuid shipmentPK, CusISFHeader header)
		{
			header.BF_JS_Shipment = shipmentPK;
		}

		void CreateBuyingParty(CusISFHeader header)
		{
			var consignment = currentConsignment;

			if (!consignment.HVC_OA_ConsigneeAddress.IsEmpty)
			{
				header.BuyingParty.E2_AddressOverride = false;
				header.BuyingParty.E2_ValidationStatus = AddressValidationStatus.ManuallyVerified;
				header.BuyingParty.E2_OA_Address = consignment.HVC_OA_ConsigneeAddress;
			}
			else
			{
				header.BuyingParty.E2_AddressOverride = true;
				header.BuyingParty.E2_ValidationStatus = AddressValidationStatus.ManuallyVerified;
				header.BuyingParty.E2_CompanyName = consignment.HVC_ConsigneeName.ToUpper();
				header.BuyingParty.E2_Address1 = consignment.HVC_ConsigneeAddress1.SubstringSafe(0, JobDocAddressSchema.E2_Address1.MaxLength).ToUpper();
				header.BuyingParty.E2_Address2 = consignment.HVC_ConsigneeAddress2.SubstringSafe(0, JobDocAddressSchema.E2_Address2.MaxLength).ToUpper();
				header.BuyingParty.E2_Contact = consignment.HVC_ConsigneeContact.ToUpper();
				header.BuyingParty.E2_City = consignment.HVC_ConsigneeCity.ToUpper();
				header.BuyingParty.E2_State = consignment.HVC_ConsigneeState.SubstringSafe(0, JobDocAddressSchema.E2_State.MaxLength).ToUpper();
				header.BuyingParty.E2_Postcode = consignment.HVC_ConsigneePostcode.Substring(0, JobDocAddressSchema.E2_Postcode.MaxLength).ToUpper();
				header.BuyingParty.E2_RN_NKCountryCode = consignment.HVC_RN_NKConsigneeCountryCode;
				header.BuyingParty.E2_Phone = consignment.HVC_ConsigneePhone.ToUpper();
				header.BuyingParty.E2_Mobile = consignment.HVC_ConsigneeMobile.ToUpper();
				header.BuyingParty.E2_Fax = consignment.HVC_ConsigneeFax.ToUpper();
				header.BuyingParty.E2_Email = consignment.HVC_ConsigneeEmail.ToUpper();
				header.BuyingParty.E2_GovRegNum = ZString.Empty;
				header.BuyingParty.E2_GovRegNumType = ZString.Empty;
			}
		}

		void CreateSellingParty(CusISFHeader header)
		{
			var consignment = currentConsignment;

			if (!consignment.HVC_OA_ShipperAddress.IsEmpty)
			{
				header.SellingParty.E2_ValidationStatus = AddressValidationStatus.ManuallyVerified;
				header.SellingParty.E2_AddressOverride = false;
				header.SellingParty.E2_OA_Address = consignment.HVC_OA_ShipperAddress;
			}
			else
			{
				header.SellingParty.E2_AddressOverride = true;
				header.SellingParty.E2_ValidationStatus = AddressValidationStatus.ManuallyVerified;
				header.SellingParty.E2_CompanyName = consignment.HVC_ShipperName.ToUpper();
				header.SellingParty.E2_Address1 = consignment.HVC_ShipperAddress1.SubstringSafe(0, JobDocAddressSchema.E2_Address1.MaxLength).ToUpper();
				header.SellingParty.E2_Address2 = consignment.HVC_ShipperAddress2.SubstringSafe(0, JobDocAddressSchema.E2_Address2.MaxLength).ToUpper();
				header.SellingParty.E2_Contact = consignment.HVC_ShipperContact.ToUpper();
				header.SellingParty.E2_City = consignment.HVC_ShipperCity.ToUpper();
				header.SellingParty.E2_State = consignment.HVC_ShipperState.Substring(0, JobDocAddressSchema.E2_State.MaxLength).ToUpper();
				header.SellingParty.E2_Postcode = consignment.HVC_ShipperPostcode.Substring(0, JobDocAddressSchema.E2_Postcode.MaxLength).ToUpper();
				header.SellingParty.E2_RN_NKCountryCode = consignment.HVC_RN_NKShipperCountryCode;
				header.SellingParty.E2_Phone = consignment.HVC_ShipperPhone.ToUpper();
				header.SellingParty.E2_Mobile = consignment.HVC_ShipperMobile.ToUpper();
				header.SellingParty.E2_Fax = consignment.HVC_ShipperFax.ToUpper();
				header.SellingParty.E2_Email = consignment.HVC_ShipperEmail.ToUpper();
			}
		}

		#region IUSImporterSecurityFilingCreator

		public RelatedJobCollection CreateHeaders()
		{
			Factory.SuspendValidation();
			Create(Factory);
			Factory.ResumeValidation();
			return createdNewHeaders;
		}

		#endregion
	}
}
