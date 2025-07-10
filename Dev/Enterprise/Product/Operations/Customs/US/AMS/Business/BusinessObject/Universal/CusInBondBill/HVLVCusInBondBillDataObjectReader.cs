using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.US.AMS.Business.Universal
{
	public class HVLVCusInBondBillDataObjectReader : ShipmentDataObjectReader<CusInBondBill>
	{
		public HVLVCusInBondBillDataObjectReader(Shipment consignmentDataObject, Shipment consolDataObject, CusInBondHeader header, ForwardingShipment shipment, ConsolDataCalculator consolDataCalculator, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(consignmentDataObject, logger, factory)
		{
			this.consignmentDataObject = consignmentDataObject;
			this.consolDataObject = consolDataObject;
			this.header = header;
			this.shipment = shipment;
			this.consolDataCalculator = consolDataCalculator;
		}

		readonly Shipment consignmentDataObject;
		readonly Shipment consolDataObject;
		readonly CusInBondHeader header;
		readonly ForwardingShipment shipment;
		readonly ConsolDataCalculator consolDataCalculator;

		public override DataContextType DataContextType => DataContextType.USAMS;

		protected override IMatchingBusinessEntityFinder<CusInBondBill> GetCombinedReferenceMatcher() => null;

		protected override CusInBondBill GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var bill = default(CusInBondBill);
			if (header.IsInDatabase && !string.IsNullOrEmpty(consignmentDataObject.WayBillNumber))
			{
				bill = header.Bills.FirstOrDefault(b => b.B0_MasterBillNumber == consignmentDataObject.WayBillNumber.Value.ToUpper());
			}
			return bill;
		}

		protected override CusInBondBill GetNewBusinessObject() => header.Bills.AddNew();

		protected override void PopulateBusinessObject(CusInBondBill targetBO)
		{
			SetValue(targetBO, CusInBondBillSchema.B0_BH, header.PK);
			header.InitializeNewBill(targetBO);

			SetValue(targetBO, CusInBondBillSchema.B0_IssuerCode, GetIssuerCode(targetBO));
			SetValue(targetBO, CusInBondBillSchema.B0_MasterBillNumber, consignmentDataObject.WayBillNumber?.ToUpper());
			SetValue(targetBO, CusInBondBillSchema.B0_BillStatus, BillOfLadingStatusIndicatorList.Codes.HouseBill);

			if (consolDataCalculator != null)
			{
				SetValue(targetBO, CusInBondBillSchema.B0_RL_NKPortOfLading, GetPortOfLading());
				SetValue(targetBO, CusInBondBillSchema.B0_PlaceOfReceipt, GetPlaceOfReceipt());
				SetValue(targetBO, CusInBondBillSchema.B0_RL_NKLastForeignPort, GetLastForeignPort());
				SetValue(targetBO, CusInBondBillSchema.B0_RL_NKForeignPortOfContract, GetContractualPort());
			}

			SetValue(targetBO, CusInBondBillSchema.B0_ManifestQty, consignmentDataObject.OuterPacks ?? ZInt.Zero);
			SetValue(targetBO, CusInBondBillSchema.B0_ManifestUQ, new PackageTypeMapping().GetPackageType(PkgUnit.Piece));

			SetValue(targetBO, CusInBondBillSchema.B0_WeightUQ, consignmentDataObject.TotalWeightUnit?.Code ?? ZString.Empty);
			SetValue(targetBO, CusInBondBillSchema.B0_VolumeUQ, consignmentDataObject.TotalVolumeUnit?.Code ?? ZString.Empty);

			if (consignmentDataObject.ManifestedWeight == null || consignmentDataObject.ManifestedWeight < ZDecimal.Zero)
			{
				SetValue(targetBO, CusInBondBillSchema.B0_Weight, ZDecimal.Zero);
			}
			else
			{
				SetValue(targetBO, CusInBondBillSchema.B0_Weight, Math.Max(consignmentDataObject.ManifestedWeight.Value, 1m));
			}

			if (consignmentDataObject.ManifestedVolume == null || consignmentDataObject.ManifestedVolume < ZDecimal.Zero)
			{
				SetValue(targetBO, CusInBondBillSchema.B0_Volume, ZDecimal.Zero);
			}
			else
			{
				SetValue(targetBO, CusInBondBillSchema.B0_Volume, Math.Max(consignmentDataObject.ManifestedVolume.Value, 1m));
			}

			PopulateBillConsigneeAddress(consignmentDataObject, targetBO);
			PopulateBillForeignShipperAddress(consignmentDataObject, targetBO);
			PopulateBillNotifyPartyAddress(targetBO);

			using (header.MovementHeader.SuspendUpdatingContainerDetail())
			{
				PopulateContainers(consolDataObject, consignmentDataObject, targetBO);
			}
		}

		void PopulateBillConsigneeAddress(Shipment consignment, CusInBondBill bill)
		{
			var consigneeAddress = consignment.OrganizationAddressCollection.FirstOrDefault(o => o.AddressType?.ToString() == nameof(DocAddressType.ConsigneeDocumentaryAddress));

			if (consigneeAddress != null)
			{
				MasterFiles.Business.JobDocAddress jobDocAddress;
				if (consigneeAddress.AddressOverride.GetValueOrDefault())
				{
					jobDocAddress = bill.Consignee;
					SetValue(jobDocAddress, JobDocAddressSchema.E2_OA_Address, ZGuid.Empty);
					SetValue(jobDocAddress, JobDocAddressSchema.E2_Contact, consigneeAddress.Contact);
					SetValue(jobDocAddress, JobDocAddressSchema.E2_CompanyName, consigneeAddress.CompanyName);
					SetValue(jobDocAddress, JobDocAddressSchema.E2_Address1, consigneeAddress.Address1);
					SetValue(jobDocAddress, JobDocAddressSchema.E2_Address2, consigneeAddress.Address2);
					SetValue(jobDocAddress, JobDocAddressSchema.E2_City, consigneeAddress.City);
					SetValue(jobDocAddress, JobDocAddressSchema.E2_Postcode, consigneeAddress.Postcode);
					SetValue(jobDocAddress, JobDocAddressSchema.E2_State, (ZString?)consigneeAddress.State);
					SetValue(jobDocAddress, JobDocAddressSchema.E2_RN_NKCountryCode, consigneeAddress.Country.GetCodeAsUpperCase());
					SetValue(jobDocAddress, JobDocAddressSchema.E2_AddressOverride, ZBool.True);
					SetValue(jobDocAddress, JobDocAddressSchema.E2_ValidationStatus, AddressValidationStatus.ManuallyVerified);
				}
				else
				{
					jobDocAddress = new OrganisationDataObjectReader(consigneeAddress, logger, factory).GetMatchedOrNew(bill, OrganisationTypes.Consignee, null, DocAddressType.ConsigneeAddress);
					if (jobDocAddress != null)
					{
						bill.DocAddresses.Add(jobDocAddress);
					}
				}
			}
		}

		void PopulateBillForeignShipperAddress(Shipment consignment, CusInBondBill bill)
		{
			var foreignShipperAddress = consignment.OrganizationAddressCollection.FirstOrDefault(o => o.AddressType?.ToString() == nameof(DocAddressType.ConsignorDocumentaryAddress));

			if (foreignShipperAddress != null)
			{
				MasterFiles.Business.JobDocAddress jobDocAddress;
				if (foreignShipperAddress.AddressOverride.GetValueOrDefault())
				{
					jobDocAddress = bill.ForeignShipper;
					SetValue(jobDocAddress, JobDocAddressSchema.E2_OA_Address, ZGuid.Empty);
					SetValue(jobDocAddress, JobDocAddressSchema.E2_Contact, foreignShipperAddress.Contact);
					SetValue(jobDocAddress, JobDocAddressSchema.E2_CompanyName, foreignShipperAddress.CompanyName);
					SetValue(jobDocAddress, JobDocAddressSchema.E2_Address1, foreignShipperAddress.Address1);
					SetValue(jobDocAddress, JobDocAddressSchema.E2_Address2, foreignShipperAddress.Address2);
					SetValue(jobDocAddress, JobDocAddressSchema.E2_City, foreignShipperAddress.City);
					SetValue(jobDocAddress, JobDocAddressSchema.E2_Postcode, foreignShipperAddress.Postcode);
					SetValue(jobDocAddress, JobDocAddressSchema.E2_State, (ZString?)foreignShipperAddress.State);
					SetValue(jobDocAddress, JobDocAddressSchema.E2_RN_NKCountryCode, foreignShipperAddress.Country.GetCodeAsUpperCase());
					SetValue(jobDocAddress, JobDocAddressSchema.E2_AddressOverride, ZBool.True);
					SetValue(jobDocAddress, JobDocAddressSchema.E2_ValidationStatus, AddressValidationStatus.ManuallyVerified);
				}
				else
				{
					jobDocAddress = new OrganisationDataObjectReader(foreignShipperAddress, logger, factory).GetMatchedOrNew(bill, OrganisationTypes.Consignor, null, DocAddressType.ForeignShipperDocumentaryAddress);
					if (jobDocAddress != null)
					{
						bill.DocAddresses.Add(jobDocAddress);
					}
				}
			}
		}

		void PopulateBillNotifyPartyAddress(CusInBondBill bill)
		{
			if (shipment.NotifyParty != null)
			{
				bill.NotifyParty1.E2_AddressOverride = false;
				bill.NotifyParty1.E2_OA_Address = shipment.NotifyPartyDocumentaryAddress.E2_OA_Address;
				bill.NotifyParty1.E2_Contact = shipment.NotifyPartyDocumentaryAddress.E2_Contact;
			}
		}

		void PopulateContainers(Shipment consolDataObject, Shipment consignmentDataObject, CusInBondBill bill)
		{
			if (bill.IsInDatabase && bill.MovementDetail != null && bill.MovementDetail.Containers.Count > 0)
			{
				bill.MovementDetail.Containers.ForEach(c => c.Commodities.DeleteAll());
			}

			if (consolDataObject.ContainerCollection != null)
			{
				foreach (var packingLineDataObject in consignmentDataObject.PackingLineCollection)
				{
					var containerDataObject = consolDataObject.ContainerCollection.FirstOrDefault(c => c.ContainerNumber == packingLineDataObject.ContainerNumber?.ToUpper()) ?? consolDataObject.ContainerCollection.FirstOrDefault();
					if (containerDataObject != null)
					{
						new HVLVCusInBondContainerDataObjectReader(containerDataObject, consignmentDataObject, packingLineDataObject, bill.MovementDetail, shipment, logger, factory).ReadIntoBusinessObject();
					}
				}
			}
		}

		ZString GetPortOfLading()
		{
			var transport = consolDataCalculator.LoadTransportForUSBoundVessel;
			return transport != null ? transport.JW_RL_NKLoadPort : ZString.Empty;
		}

		ZString GetPlaceOfReceipt()
		{
			var result = ZString.Empty;
			var transport = consolDataCalculator.FirstCarrierContractualTransport;
			if (transport != null)
			{
				var port = transport.LoadPort;
				result = port != null ? port.RL_PortName : transport.JW_RL_NKLoadPort;
			}
			return result.Left(CusInBondBill.Schema.B0_PlaceOfReceiptMaxLength).ToUpper();
		}

		ZString GetLastForeignPort()
		{
			var port = consolDataCalculator.LastForeignPortOfLoading;
			return port != null ? port.RL_Code : ZString.Empty;
		}

		ZString GetContractualPort()
		{
			var result = ZString.Empty;
			var transport = consolDataCalculator.FirstCarrierContractualTransport;
			if (transport != null)
			{
				var port = transport.LoadPort;
				result = port != null ? port.RL_Code : ZString.Empty;
			}
			return result.Left(CusInBondBill.Schema.B0_RL_NKForeignPortOfContractMaxLength);
		}

		ZString GetIssuerCode(CusInBondBill bill)
		{
			var result = ZString.Empty;
			if (bill.IsBillAlreadyOnFile)
			{
				result = bill.B0_IssuerCode;
			}
			else
			{
				var billNumber = shipment.JS_HouseBill.KeepValidBillNumberCharacters();
				var validSCACs = shipment.GetValidSCACIssuerCodes(shipment.TransportMode);
				result = billNumber.GetSCAC(validSCACs);
			}
			return result;
		}
	}
}
