using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public static class SettingOrderDeterminer
	{
		#region Declaration

		public static IEnumerable<ZString> GetSettingOrder(JobDeclaration declaration)
		{
			// Main Declaration Fields
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OverrideFreightDefaults);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_GB);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OA_SupplierAddress);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OA_ImporterAddress);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_Supplier);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_Importer);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_ControllingAgent);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_ControllingCustomer);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_MessageType);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_MessageSubType);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_TransportMode);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_ContainerMode);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_ApplicationCode);

			IEnumerable<ZString> messageTypeSpecificSettingOrder;
			if (declaration.IsExport)
			{
				messageTypeSpecificSettingOrder = GetExportSettingOrder(declaration);
			}
			else if (declaration.IsDrawback)
			{
				messageTypeSpecificSettingOrder = GetDrawbackSettingOrder(declaration);
			}
			else if (declaration.IsFTZAdmission)
			{
				messageTypeSpecificSettingOrder = GetFTZSettingOrder(declaration);
			}
			else
			{
				messageTypeSpecificSettingOrder = GetImportSettingOrder(declaration);
			}

			foreach (var matchingKey in messageTypeSpecificSettingOrder)
			{
				yield return matchingKey;
			}
		}

		static IEnumerable<ZString> GetShipmentDetailsSettingOrder(JobDeclaration declaration)
		{
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_HouseBill);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_RL_NKOrigin);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_DateAtOrigin);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_RL_NKFinalDestination);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_DateAtFinalDestination);
			if (!declaration.IsExport)
			{
				yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_DestinationState);
			}
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_UC_NKCountryOfExport);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_DateOfExport);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_GoodsDescription);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OwnerRef);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_TotalWeight);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_TotalWeightUnit);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_TotalVolume);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_TotalVolumeUnit);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_TotalNoOfPieces);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_TotalNoOfPacks);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_TotalNoOfPacksPackType);
		}

		static IEnumerable<ZString> GetPickupDetailsSettingOrder(JobDeclaration declaration)
		{
			yield return OrganizationDataObjectValueSetter.GetKey(declaration.PK, nameof(DocAddressType.SupplierPickupDeliveryAddress));
			var docsAndCartage = declaration.DocsAndCartage;
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_OA_PickupCartageCoAddr);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_FCLAvailable);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_FCLStorageCommences);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_FCLPickupEquipmentNeeded);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_LCLDatesOverrideConsol);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_LCLAvailable);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_LCLStorageCommences);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_EstimatedPickup);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_PickupRequiredBy);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_PickupCartageAdvised);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_PickupCartageCompleted);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_PickupLabourCharge);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_PickupLabourTime);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_PickupTruckWaitCharge);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_PickupTruckWaitTime);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_LCLAirStorageCharge);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_LCLAirStorageDaysOrHours);
		}

		static IEnumerable<ZString> GetDeliveryDetailsSettingOrder(JobDeclaration declaration)
		{
			yield return OrganizationDataObjectValueSetter.GetKey(declaration.PK, nameof(DocAddressType.ImporterPickupDeliveryAddress));
			var docsAndCartage = declaration.DocsAndCartage;
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_OA_DeliveryCartageCoAddr);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_FCLAvailable);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_FCLStorageCommences);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_FCLDeliveryEquipmentNeeded);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_LCLDatesOverrideConsol);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_LCLAvailable);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_LCLStorageCommences);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_EstimatedDelivery);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_DeliveryRequiredBy);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_DeliveryCartageAdvised);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_DeliveryCartageCompleted);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_DeliveryLabourCharge);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_DeliveryLabourTime);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_DeliveryTruckWaitCharge);
			yield return ColumnValueSetter.GetKey(docsAndCartage.PK, JobDocsAndCartageSchema.JP_DeliveryTruckWaitTime);
		}

		static IEnumerable<ZString> GetFTZTransportDetailsSettingOrder(JobDeclaration declaration)
		{
			foreach (var matchingKey in GetCommonTransportDetailsSettingOrder(declaration))
			{
				yield return matchingKey;
			}
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_SchDEntry);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_EntryDate);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_IsHMFApplicable);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_EstimatedEntryDate);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_ITDate);
		}

		static IEnumerable<ZString> GetImportTransportDetailsSettingOrder(JobDeclaration declaration)
		{
			foreach (var matchingKey in GetCommonTransportDetailsSettingOrder(declaration))
			{
				yield return matchingKey;
			}
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_SchDEntry);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_EntryDate);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_IsHMFApplicable);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_DateOfFirstArrival);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_ITDate);
		}

		static IEnumerable<ZString> GetExportTransportDetailsSettingOrder(JobDeclaration declaration)
		{
			foreach (var matchingKey in GetCommonTransportDetailsSettingOrder(declaration))
			{
				yield return matchingKey;
			}
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_SchDExport);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_DateOfExport);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_RL_NKPortOfExport);
		}

		static IEnumerable<ZString> GetCommonTransportDetailsSettingOrder(JobDeclaration declaration)
		{
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_MasterBill);
			if (declaration.IsSea)
			{
				yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_VesselName);
			}
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_VoyageFlightNo);
			if (declaration.IsAir)
			{
				yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_Folio);
			}
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_UI_NKCarrierSCAC);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_SchDLoading);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_ExportDate);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_RL_NKPortOfLoading);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_SchDArrival);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_DateOfArrival);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_RL_NKPortOfArrival);
		}

		static IEnumerable<ZString> GetFTZSettingOrder(JobDeclaration declaration)
		{
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_F_AdmissionType);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_EnableSPN);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_F_DirectDelivery);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_F_IncludePTT);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_RS_NKServiceLevel);

			foreach (var matchingKey in GetFTZTransportDetailsSettingOrder(declaration))
			{
				yield return matchingKey;
			}

			foreach (var matchingKey in GetShipmentDetailsSettingOrder(declaration))
			{
				yield return matchingKey;
			}

			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_US_NKLocationOfGoods);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_F_DeliveryCode);

			// Organisation
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_ShippingLine);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_Forwarder);
			yield return OrganizationDataObjectValueSetter.GetKey(declaration.PK, nameof(DocAddressType.CustomsContainerTerminalOperatorAddress));
			yield return OrganizationDataObjectValueSetter.GetKey(declaration.PK, nameof(DocAddressType.CustomsDepotAddress));
			yield return OrganizationDataObjectValueSetter.GetKey(declaration.PK, nameof(DocAddressType.CustomsContainerYardAddress));
			yield return OrganizationDataObjectValueSetter.GetKey(declaration.PK, nameof(DocAddressType.CustomsWarehouseAddress));
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_ExternalBroker);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OA_ConsigneeAddress);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OA_DeclarantAddress);

			foreach (var matchingKey in GetDeliveryDetailsSettingOrder(declaration))
			{
				yield return matchingKey;
			}

			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_GS_NKCusAgent);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_MergeBy);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_BRDRefNo);

			foreach (var matchingKey in GetPriorNoticeSettingOrder(declaration))
			{
				yield return matchingKey;
			}

			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_F_RoutingDetails);
		}

		static IEnumerable<ZString> GetExportSettingOrder(JobDeclaration declaration)
		{
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_TransactionsRelated);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_HazardousCargo);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_RoutedTransaction);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_CommodityFilingOption);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_MergeBy);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_TariffType);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_SoldEnRouteIndicator);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_RS_NKServiceLevel);

			foreach (var matchingKey in GetExportTransportDetailsSettingOrder(declaration))
			{
				yield return matchingKey;
			}

			foreach (var matchingKey in GetShipmentDetailsSettingOrder(declaration))
			{
				yield return matchingKey;
			}

			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_TransportReference);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_RN_NKCountryOfDestination);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_ForeignTradeZone);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_StateOfOrigin);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_InbondType);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_ImportEntryNo);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_LicenseType);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_LicenseNo);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_ExportCode);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_ECCN);

			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_Consignee);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_ShippingLine);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_Forwarder);
			yield return OrganizationDataObjectValueSetter.GetKey(declaration.PK, nameof(DocAddressType.CustomsContainerTerminalOperatorAddress));
			yield return OrganizationDataObjectValueSetter.GetKey(declaration.PK, nameof(DocAddressType.CustomsDepotAddress));
			yield return OrganizationDataObjectValueSetter.GetKey(declaration.PK, nameof(DocAddressType.CustomsContainerYardAddress));

			foreach (var matchingKey in GetPickupDetailsSettingOrder(declaration))
			{
				yield return matchingKey;
			}

			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_DDTCITARExemptionNo);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_DDTCRegistrationNo);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_DDTCMilitaryEquipmentIndicator);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_DDTCPartyCertificationIndicator);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_DDTCUSMLCategoryCode);
		}

		static IEnumerable<ZString> GetImportSettingOrder(JobDeclaration declaration)
		{
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_EntryType);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_EntryMode);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_LiveEntryIndicator);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_EnableENS);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_IsInvoiceByRequest);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_EnableCRL);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_CargoReleaseType);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_EnableAII);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_CertifyCargoRelease);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_EnableSPN);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_RS_NKServiceLevel);

			foreach (var matchingKey in GetImportTransportDetailsSettingOrder(declaration))
			{
				yield return matchingKey;
			}

			foreach (var matchingKey in GetShipmentDetailsSettingOrder(declaration))
			{
				yield return matchingKey;
			}

			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_US_NKLocationOfGoods);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_US_NKCentralizedExamSite);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_EntryFilerCode);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_7501Purchased);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_ManEntry);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_7501Agent);

			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_ShippingLine);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_Forwarder);
			yield return OrganizationDataObjectValueSetter.GetKey(declaration.PK, nameof(DocAddressType.CustomsContainerTerminalOperatorAddress));
			yield return OrganizationDataObjectValueSetter.GetKey(declaration.PK, nameof(DocAddressType.CustomsDepotAddress));
			yield return OrganizationDataObjectValueSetter.GetKey(declaration.PK, nameof(DocAddressType.CustomsContainerYardAddress));
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_ExternalBroker);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OA_ConsigneeAddress);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OA_DeclarantAddress);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_NotifyParty);

			foreach (var matchingKey in GetDeliveryDetailsSettingOrder(declaration))
			{
				yield return matchingKey;
			}

			yield return OrganizationDataObjectValueSetter.GetKey(declaration.PK, nameof(DocAddressType.CBPBroker));
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_Exporter);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OA_ManufacturerAddress);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OA_SellerAddress);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_SellingAgent);
			yield return PropertyValueSetter.GetKey(declaration.PK, JobDeclaration.Schema.JE_OA_InvoicerAddress);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_Buyer);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_BuyingAgent);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OA_ShipToPartyAddress);

			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_GS_NKCusAgent);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_MergeBy);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_BRDRefNo);
			// Entry Data
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_MissingDocument1);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_MissingDocument2);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_TeamNo);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_OGALineReleaseIndicator);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_EntryDateElectionCode);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_PresentationDate);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_ConsolidatedInformalIndicator);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_GeneralOrderNo);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_TaxDeferIndicator);
			// Remote Filing Data
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_PreparerDistrictPort);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_SchDExam);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_DES);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_PreparerOfficeCode);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_OtherReconIndicator);
			// Reconciliation Data
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_NAFTAReconIndicator);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_FixRecon);
			// Bond Data
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_BondType);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_BondAmount);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_BondCalcCode);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_BondProducerAccNo);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_SuretyCode);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_ADDCVDSuretyCode);
			// Payment Data
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_PaymentType);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_PaymentMethod);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_PeriodicStatementMM);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_PreliminaryStatementPrintDate);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_FixPSD);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_ClientBranchDesignation);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_CheckNo);

			foreach (var matchingKey in GetPriorNoticeSettingOrder(declaration))
			{
				yield return matchingKey;
			}
		}

		static IEnumerable<ZString> GetPriorNoticeSettingOrder(JobDeclaration declaration)
		{
			// Prior Notice
			yield return OrganizationDataObjectValueSetter.GetKey(declaration.PK, nameof(DocAddressType.FDASubmitter));
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_FDAContactName);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_FDAContactPhoneNo);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_FDAContactEmail);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_FDAADTA);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_FDAAPC);
			// Carrier/Privately Owned Vehicle
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_FDACANType);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_FDACCN);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_FDACAN);
		}

		static IEnumerable<ZString> GetDrawbackSettingOrder(JobDeclaration declaration)
		{
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_EntryType);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_DRWRejectedMerchandiseReason);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_GoodsDescription);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_EstimatedEntryDate);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_EarliestExportDate);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_DRWDatePeriodFrom);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_DRWDatePeriodTo);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_DRWFilingMethod);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_DRWTotalPRDC);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_NotifyParty);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_DRWSection);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_ExporterSummaryInd);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_NAFTAClaimInd);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_AcceleratedClaimInd);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_PreInspectionInd);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_PetroleumClaimInd);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_WaiverNoticeInd);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_NAFTADrawbackCountry);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_PreparerDistrictPort);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_ClaimPort);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_TeamNo);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_DRWIntendedPortOfExport);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_DRWTENo);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_GS_NKCusAgent);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OwnerRef);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_BondType);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_SuretyCode);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_EntryFilerCode);
			yield return ColumnValueSetter.GetKey(declaration.PK, USAddInfoSchema.US_DRWPurpose);
		}

		#endregion

		#region Invoice Header

		#region Standalone

		public static IEnumerable<ZString> GetSettingOrderForStandalone(JobComInvoiceHeader invoice)
		{
			return invoice.IsExport ? GetExportSettingOrderForStandalone(invoice) : GetImportSettingOrderForStandalone(invoice);
		}

		static IEnumerable<ZString> GetImportSettingOrderForStandalone(JobComInvoiceHeader invoice)
		{
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OH_Supplier);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OH_Buyer);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_StandAloneInvoiceDirection);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceNumber);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceDate);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_GB);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceAmount);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_RX_NKInvoice_Currency);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceCurrExRateType);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceCurrExRate);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_IncoTerm);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_Weight);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_WeightUQ);

			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_InvoiceType);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_PaymentTerms);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_PaymentTermsDesc);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_DES);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_ValueForDiscount);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_ValueForForeignTax);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_TermsOfDeliveryLocationQualifier);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_TermsOfDeliveryLocationIndicator);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_TermsOfDeliveryLocation);

			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OA_SupplierAddress);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OA_ManufacturerAddress);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OA_ExporterAddress);
			yield return PropertyValueSetter.GetKey(invoice.PK, JobComInvoiceHeader.Schema.JZ_OA_InvoicerDocAddress);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OA_SellerAddress);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OH_SellingAgent);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OH_Buyer);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OA_BuyerAddress);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OH_BuyerAgent);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OA_ConsigneeAddress);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OA_SoldToPartyAddress);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OA_ShipToPartyAddress);
		}

		static IEnumerable<ZString> GetExportSettingOrderForStandalone(JobComInvoiceHeader invoice)
		{
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OH_Supplier);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OH_Buyer);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_StandAloneInvoiceDirection);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_TariffType);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceNumber);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceDate);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_GB);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceAmount);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_RX_NKInvoice_Currency);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceCurrExRateType);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceCurrExRate);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_IncoTerm);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_Weight);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_WeightUQ);
		}

		#endregion

		#region Normal

		public static IEnumerable<ZString> GetSettingOrderForNormal(JobComInvoiceHeader invoice)
		{
			return invoice.IsExport ? GetExportSettingOrderForNormal(invoice) : GetImportSettingOrderForNormal(invoice);
		}

		static IEnumerable<ZString> GetImportSettingOrderForNormal(JobComInvoiceHeader invoice)
		{
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OA_SupplierAddress);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OA_ManufacturerAddress);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OA_ExporterAddress);
			yield return PropertyValueSetter.GetKey(invoice.PK, JobComInvoiceHeader.Schema.JZ_OA_InvoicerDocAddress);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OA_SellerAddress);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OH_SellingAgent);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OH_Buyer);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OA_BuyerAddress);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OH_BuyerAgent);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OA_ConsigneeAddress);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OA_SoldToPartyAddress);

			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceNumber);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceAmount);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_RX_NKInvoice_Currency);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceCurrExRateType);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceCurrExRate);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_IncoTerm);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceDate);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_Weight);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_WeightUQ);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_UC_NKCountryOfExport);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_DateOfExport);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_UC_NKCountryOfOrigin);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_TransactionsRelated);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_FirstSale);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_NoOfPacks);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_IsLineGrouping);
			var declaration = invoice.JobDeclaration;
			if (declaration.IsFTZAdmission)
			{
				yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_ZoneStatus);
			}

			// FDA Data
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_FDAContactName);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_FDAContactPhoneNo);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_FDAContactEmail);
			yield return PropertyValueSetter.GetKey(invoice.PK, JobComInvoiceHeader.Schema.JZ_OA_FDAShipperAddress);

			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceCurrLandedCostExRate);
		}

		static IEnumerable<ZString> GetExportSettingOrderForNormal(JobComInvoiceHeader invoice)
		{
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OH_Supplier);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OA_SupplierAddress);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OH_Buyer);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OA_BuyerAddress);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OH_Consignee);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OA_IntermediateConsigneeAddress);

			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceNumber);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceAmount);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_RX_NKInvoice_Currency);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceCurrExRate);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_IncoTerm);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceDate);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_Weight);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_WeightUQ);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_NoOfPacks);
			// AES Data
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_TransactionsRelated);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_HazardousCargo);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_RoutedTransaction);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_InbondType);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_AESOriginIndicator);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_StateOfOrigin);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_ForeignTradeZone);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_ImportEntryNo);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_DateOfExport);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_TariffType);
			// License Data
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_LicenseType);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_LicenseNo);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_ECCN);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_ExportCode);
			// DDTC Data
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_DDTCITARExemptionNo);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_DDTCMilitaryEquipmentIndicator);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_DDTCPartyCertificationIndicator);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_DDTCRegistrationNo);
			yield return ColumnValueSetter.GetKey(invoice.PK, USAddInfoSchema.US_DDTCUSMLCategoryCode);
		}

		#endregion

		#endregion

		#region Invoice Line

		public static IEnumerable<ZString> GetSettingOrder(JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine.IsExport)
			{
				return GetExportSettingOrder(invoiceLine);
			}
			else if (invoiceLine.IsDrawback)
			{
				return GetDrawbackSettingOrder(invoiceLine);
			}
			else
			{
				return GetImportSettingOrder(invoiceLine);
			}
		}

		static IEnumerable<ZString> GetImportSettingOrder(JobComInvoiceLine invoiceLine)
		{
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_JI_ParentProduct);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_FlavorContentCreditInd);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_PartNo);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_CC);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_SupTariff);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_OA_ManufacturerAddress);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_UC_NKCountryOfExport);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_UC_NKCountryOfOrigin);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_Tariff);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_SPI);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_SecondarySPI);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DestinationState);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_OA_ShipToPartyAddress);

			var declaration = invoiceLine.Declaration;
			var isFTZAdmission = declaration.IsFTZAdmission;
			if (isFTZAdmission)
			{
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_TextileCategoryNo);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_ZoneStatus);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_F_PNDisclaimer);
			}
			else
			{
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_TaxApply);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_TaxCode);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_TaxRateT);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_TaxRateS);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_TransactionsRelated);
			}

			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_OA_Seller);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_Description);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_InvoiceQuantity);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_InvoiceUQ);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_LinePrice);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_OA_ConsigneeAddress);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_RH_NKCommodity_Code);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_ManifestQty);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_Weight);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_WeightUQ);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_NetWeight);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_NetWeightUQ);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_Volume);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_VolumeUQ);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_CustomsQuantity);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_CustomsSecondQuantity);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_CustomsThirdQuantity);

			if (!isFTZAdmission)
			{
				// License/Permit Data
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_PIRPRulingType);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_AgricultureLicNo);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_CAExportCertificate);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_PIRPRulingNo);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_WoolLicenceNo);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_CBTPACertificateNo);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_CottonFeeExempt);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_CottonCertificateNo);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_MiscPermitNo);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_SWPMIndicator);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_IsNAFTANet);

				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_LumberExportPrice);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_LumberExportCharges);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_LumberImporterDeclaration);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_VisaNo);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_VisaQty);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_VisaUQ);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_TextileCategoryNo);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DateOfExportFromCountryOfOrigin);

				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_ADD_NA);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_ADDCaseNo);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_ADDDepositValue);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_IsBondedADD);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_ADDDepositRateIndicator);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_ADDuty);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_ADDDecID);

				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_CVD_NA);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_CVDCaseNo);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_CVDDepositValue);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_IsBondedCVD);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_CVDDepositRateIndicator);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_CVDuty);

				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_OverrideDuty);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_Duty);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_OverrideSupDuty);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_SupDuty);

				yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_HazMatCode);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_HazMatCodeQualifier);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_HazMatDesc);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_HazMatClassDesc);
			}
		}

		static IEnumerable<ZString> GetExportSettingOrder(JobComInvoiceLine invoiceLine)
		{
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_JI_ParentProduct);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_PartNo);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_CC);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_TariffType);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_Tariff);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_InvoiceQuantity);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_InvoiceUQ);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_CustomsQuantity);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_CustomsSecondQuantity);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_LinePrice);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_ExportCode);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_AESOriginIndicator);

			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_CountryOfOrigin);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_Weight);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_WeightUQ);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_NetWeight);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_NetWeightUQ);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_Volume);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_VolumeUQ);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_RH_NKCommodity_Code);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_Description);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_MarksAndNumbers);
			// License Data
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_LicenseType);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_LicenseNo);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_ECCN);
			// DDTC Data
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DDTCITARExemptionNo);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DDTCMilitaryEquipmentIndicator);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DDTCPartyCertificationIndicator);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DDTCRegistrationNo);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DDTCUSMLCategoryCode);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DDTCQuantity);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DDTCUnit);

			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_IsUsedVehicle);
			if (invoiceLine.US_IsUsedVehicle)
			{
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_VehicleIDType);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_VehicleID);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_VehicleTitleNo);
				yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_VehicleTitleState);
			}
		}

		static IEnumerable<ZString> GetDrawbackSettingOrder(JobComInvoiceLine invoiceLine)
		{
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_PartNo);

			#region Import Section

			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWIsForImportSection);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWImpActInd);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_ImportEntryNo);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWImportEntryLine);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_Tariff);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_Description);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWClaimBasis);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWDateRcvFrom);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWDateUsedFrom);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWImpManufRuleNo);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWCDInd);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWImpTrkID);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWAccMethod);

			#endregion

			#region Export Section

			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWIsForExportSection);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWExportAction);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWExportDate);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWExportDest);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_TariffType);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_ExportTariff);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWExpBOLInd);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWExpBOLCarrier);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWExportID);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWExpNoticeInd);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWExpWavInd);

			#endregion

			#region Manufacturer Section

			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWIsForManufacturerSection);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWMafActInd);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWQuantityUsed);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWUQUsed);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWDateOfManufacture);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWDescrUsed);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWDescrManufactured);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWFactoryLocation);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWManufRuleNo);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWMafTrkID);

			#endregion

			#region Claimed Amounts

			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWClaimAmountOverriden_New);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWWeightedRatio);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWMPFWeightedRatio);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWLineDutyRateDesc);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWCalcDutyWithAdValoremRate);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWAdValoremRate);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWImportQuantity);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWImportUQ);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWAllowQty);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWValuePerUQ);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWSubstituted);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWExportQuantity);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWExportUQ);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWQuantityUsed);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWUQUsed);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWDeclaredVFD);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWLineDuty);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWCalcDuty);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWAdjClaimDuty);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWDeclaredTax);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWCalcTax);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWAdjClaimTax);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWDeclaredMPF);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWCalcMPF);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWAdjClaimMPF);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWDeclaredHMF);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWQuarterlyHMF);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWCalcHMF);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, USAddInfoSchema.US_DRWAdjClaimHMF);

			#endregion
		}

		#endregion

		#region AIILine

		public static IEnumerable<ZString> GetSettingOrder(AIILine aiiLine)
		{
			yield return ColumnValueSetter.GetKey(aiiLine.PK, USAIILineAddInfoSchema.US_InvQty);
			yield return ColumnValueSetter.GetKey(aiiLine.PK, USAIILineAddInfoSchema.US_InvAmount);
			yield return ColumnValueSetter.GetKey(aiiLine.PK, USAIILineAddInfoSchema.US_UnitBasis);
			yield return ColumnValueSetter.GetKey(aiiLine.PK, USAIILineAddInfoSchema.US_PercActvIngr);
			yield return ColumnValueSetter.GetKey(aiiLine.PK, USAIILineAddInfoSchema.US_CustomsQty);
			yield return ColumnValueSetter.GetKey(aiiLine.PK, USAIILineAddInfoSchema.US_SecondQty);
			yield return ColumnValueSetter.GetKey(aiiLine.PK, USAIILineAddInfoSchema.US_ThirdQty);
			yield return ColumnValueSetter.GetKey(aiiLine.PK, USAIILineAddInfoSchema.US_InvQtyDisp);
			yield return ColumnValueSetter.GetKey(aiiLine.PK, USAIILineAddInfoSchema.US_InvUQDisp);
			yield return ColumnValueSetter.GetKey(aiiLine.PK, USAIILineAddInfoSchema.US_QtyDiffRsnCode);
			yield return ColumnValueSetter.GetKey(aiiLine.PK, USAIILineAddInfoSchema.US_QtyDiffRsn);
		}

		#endregion

		public static IEnumerable<ZString> GetSettingOrder(FDA fda)
		{
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_FDAShipperAddress);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_FDAManufacturerAddress);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_OA_FDAFEI);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_UC_NKFDAProduction);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_FDACommercialDesc);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_FDAProductCode);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_FDACargoStorageCode);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_InvCurrFDAValue);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_TradeBrandName);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_FDAQty1);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_FDAMeasure1);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_FDAQty2);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_FDAMeasure2);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_FDAQty3);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_FDAMeasure3);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_FDAQty4);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_FDAMeasure4);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_FDAQty5);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_FDAMeasure5);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_FDAQty6);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_DimUQ);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_TradeBrandName);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_ContainerDim1);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_ContainerDim2);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_ContainerDim3);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_OFT);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_TradeBrandName);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_CSH);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_PFR);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_PFT);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_FME);
			yield return ColumnValueSetter.GetKey(fda.PK, USFDAAddInfoSchema.US_SFR);
		}

		public static IEnumerable<ZString> GetSettingOrder(ACEFDA fda)
		{
			yield return ColumnValueSetter.GetKey(fda.PK, USACEFDAAddInfoSchema.US_ManufacturerAddress);
			yield return ColumnValueSetter.GetKey(fda.PK, USACEFDAAddInfoSchema.US_ProgramCode);
			yield return ColumnValueSetter.GetKey(fda.PK, USACEFDAAddInfoSchema.US_PFR);
		}

		public static IEnumerable<ZString> GetSettingOrder(NMFSHarvestingDetail nmfs)
		{
			yield return ColumnValueSetter.GetKey(nmfs.PK, USNMFSHarvestingDetailAddInfoSchema.US_GearType);
			yield return ColumnValueSetter.GetKey(nmfs.PK, USNMFSHarvestingDetailAddInfoSchema.US_HarvestedCountry);
			yield return ColumnValueSetter.GetKey(nmfs.PK, USNMFSHarvestingDetailAddInfoSchema.US_ContainsYellowfinTuna);
			yield return ColumnValueSetter.GetKey(nmfs.PK, USNMFSHarvestingDetailAddInfoSchema.US_OceanAreaOfCatch);
			yield return ColumnValueSetter.GetKey(nmfs.PK, USNMFSHarvestingDetailAddInfoSchema.US_VesselCountry);
			yield return ColumnValueSetter.GetKey(nmfs.PK, USNMFSHarvestingDetailAddInfoSchema.US_GearStartDate);
			yield return ColumnValueSetter.GetKey(nmfs.PK, USNMFSHarvestingDetailAddInfoSchema.US_ContactPartyType);
			yield return ColumnValueSetter.GetKey(nmfs.PK, USNMFSHarvestingDetailAddInfoSchema.US_GearDescription);
			yield return ColumnValueSetter.GetKey(nmfs.PK, USNMFSHarvestingDetailAddInfoSchema.US_GeographicLocation);
			yield return ColumnValueSetter.GetKey(nmfs.PK, USNMFSHarvestingDetailAddInfoSchema.US_NoSmallVessels);
			yield return ColumnValueSetter.GetKey(nmfs.PK, USNMFSHarvestingDetailAddInfoSchema.US_OA_ContactParty);
		}
	}
}
