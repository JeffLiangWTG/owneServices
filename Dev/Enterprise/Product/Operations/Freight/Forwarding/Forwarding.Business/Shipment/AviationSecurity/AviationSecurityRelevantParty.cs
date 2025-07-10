using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class AviationSecurityRelevantParty
	{
		#region Construction

		AviationSecurityRelevantParty(ForwardingShipment shipment, string organisationCode, OrgHeader orgHeader, OrgAddress orgAddress, JobDocAddress jobDocAddress, string countryCode)
		{
			this.countryCode = Constants.CountryCodes.IsInEuropeanUnionAviationSecurityScheme(countryCode)
				? Constants.CountryCodes.EuropeanUnion
				: countryCode;
			this.organisationCode = organisationCode;
			this.relevantOrgHeader = orgHeader;
			this.relevantOrgAddress = orgAddress;
			this.relevantJobDocAddress = jobDocAddress;
			this.aviationSecurity = new AviationSecuritySupport(shipment, this.countryCode);
			this.supplyChainSecurityConfiguration = aviationSecurity.SupplyChainSecurityConfiguration;
		}

		readonly string countryCode;
		readonly string organisationCode;
		readonly OrgHeader relevantOrgHeader;
		readonly OrgAddress relevantOrgAddress;
		readonly JobDocAddress relevantJobDocAddress;
		readonly SupplyChainSecurityConfiguration supplyChainSecurityConfiguration;
		readonly AviationSecuritySupport aviationSecurity;

		public OrgHeader RelevantOrgHeader => relevantOrgHeader;

		public string OrganisationCode => organisationCode;

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static AviationSecurityRelevantParty New(ForwardingShipment shipment, string organisationCode, string countryCode = null)
		{
			Argument.NotNull(shipment, "shipment");

			if (countryCode == null)
			{
				countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			}

			JobDocAddress relevantJobDocAddress = null;
			OrgAddress relevantOrgAddress = null;
			OrgHeader relevantOrgHeader = null;

			switch (organisationCode)
			{
				case SupplyChainSecurityOrganisationTypes.Consignor:
					relevantJobDocAddress = shipment.ConsignorDocumentaryAddress;
					relevantOrgAddress = relevantJobDocAddress.Address;
					relevantOrgHeader = relevantJobDocAddress.Organisation;
					break;

				case SupplyChainSecurityOrganisationTypes.LocalClient:
					if (shipment.ShipmentJobHeader != null && shipment.ShipmentJobHeader.LocalCharges != null)
					{
						relevantOrgAddress = shipment.ShipmentJobHeader.LocalChargesAddr;
						relevantOrgHeader = shipment.ShipmentJobHeader.LocalCharges;
					}
					break;

				case SupplyChainSecurityOrganisationTypes.ShipmentPickupTransportCompany:
					relevantOrgAddress = shipment.DocsAndCartage.PickupCartageCoAddr;
					relevantOrgHeader = shipment.DocsAndCartage.PickupCartageCo;
					break;

				case SupplyChainSecurityOrganisationTypes.ShipmentPackingCFS:
					if (shipment.ExportReceivingDepot != null)
					{
						relevantOrgAddress = shipment.ExportReceivingDepot;
						relevantOrgHeader = shipment.ExportReceivingDepot.Header;
					}
					break;

				case SupplyChainSecurityOrganisationTypes.CoLoadMasterShipmentSendingForwarder:
					if (shipment.CoLoadMasterShipment != null)
					{
						relevantJobDocAddress = shipment.CoLoadMasterShipment.ConsignorDocumentaryAddress;
						relevantOrgAddress = relevantJobDocAddress.Address;
						relevantOrgHeader = relevantJobDocAddress.Organisation;
					}
					break;

				case SupplyChainSecurityOrganisationTypes.ConsolSendingAgent:
					if (shipment.DepartureConsol != null)
					{
						relevantOrgAddress = shipment.DepartureConsol.SendingForwarderAddress;
						relevantOrgHeader = shipment.DepartureConsol.SendingForwarder;
					}
					break;

				case SupplyChainSecurityOrganisationTypes.ConsolAirline:
					if (shipment.DepartureConsol != null)
					{
						relevantOrgAddress = shipment.DepartureConsol.ShippingLineAddress;
						relevantOrgHeader = shipment.DepartureConsol.ShippingLine;
					}
					break;

				case SupplyChainSecurityOrganisationTypes.ConsolTransportCompany:
					if (shipment.DepartureConsol != null)
					{
						relevantOrgAddress = shipment.DepartureConsol.DeparturePackCFSTransportAddress;
						relevantOrgHeader = shipment.DepartureConsol.DeparturePackCFSTransport;
					}
					break;

				case SupplyChainSecurityOrganisationTypes.ConsolCFS:
					if (shipment.DepartureConsol != null && shipment.DepartureConsol.PackDepotAddress != null)
					{
						relevantOrgAddress = shipment.DepartureConsol.PackDepotAddress;
						relevantOrgHeader = shipment.DepartureConsol.PackDepotAddress.Header;
					}
					break;

				case SupplyChainSecurityOrganisationTypes.ConsolCoLoadWithOrganization:
					if (shipment.DepartureConsol != null && shipment.DepartureConsol.IsCoLoad)
					{
						relevantOrgAddress = shipment.DepartureConsol.CreditorAddress;
						relevantOrgHeader = shipment.DepartureConsol.Creditor;
					}
					break;

				case SupplyChainSecurityOrganisationTypes.ConsolForwarderYouHaveBorrowedMAWBStockFrom:
					if (shipment.DepartureConsol != null && shipment.DepartureConsol.JK_IsNeutralMaster)
					{
						JobMawb mawb = shipment.DepartureConsol.GetAllocatedMAWB(shipment.Factory);
						if (mawb != null && mawb.From != null)
						{
							relevantOrgAddress = mawb.From;
							relevantOrgHeader = mawb.From.Header;
						}
					}
					break;

				case SupplyChainSecurityOrganisationTypes.ShipmentPickupFrom:
					relevantJobDocAddress = shipment.ConsignorPickupAddress;
					relevantOrgAddress = relevantJobDocAddress.Address;
					relevantOrgHeader = relevantJobDocAddress.Organisation;
					break;

				default:
					throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Organization type {0} is not handled", organisationCode));
			}

			if ((shipment.AviationSecurity.SupplyChainSecurityConfiguration.IsAddressLevelScheme && relevantOrgAddress != null)
				|| (!shipment.AviationSecurity.SupplyChainSecurityConfiguration.IsAddressLevelScheme && relevantOrgHeader != null))
			{
				return new AviationSecurityRelevantParty(shipment, organisationCode, relevantOrgHeader, relevantOrgAddress, relevantJobDocAddress, countryCode);
			}

			return null;
		}

		#endregion

		public ZString HumanReadableName
		{
			get
			{
				var humanReadableName = new ZStringBuilder(SupplyChainSecurityOrganisationTypes.GetDescription(organisationCode));
				if (relevantOrgHeader != null && !relevantOrgHeader.IsMiscellaneous)
				{
					humanReadableName.Append(" (" + relevantOrgHeader.OH_Code + ")");
				}

				return humanReadableName.ToString();
			}
		}

		public bool IsAviationSecurityApproved
		{
			get
			{
				if (relevantJobDocAddress != null && !relevantJobDocAddress.HasRealOrganisation)
				{
					return false;
				}
				else if (supplyChainSecurityConfiguration.IsAddressLevelScheme)
				{
					if (relevantOrgAddress == null)
					{
						return true;
					}
					else if (KnownShipperRecord == null)
					{
						return false;
					}
				}

				return KnownShipperRecord != null
					&& supplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(KnownShipperRecord.OV_EXApprovedOrMajorExporter)
					&& aviationSecurity.GetErrorForApprovalInvalidForShipment(KnownShipperRecord).IsEmpty
					&& ExpiryDateIsValid;
			}
		}

		public bool IsApprovedToShipOnPassengerFlights
		{
			get
			{
				return KnownShipperRecord == null
					|| (supplyChainSecurityConfiguration.ApprovalCodeIsApprovedToShipOnPassengerFlights(KnownShipperRecord.OV_EXApprovedOrMajorExporter)
						&& ExpiryDateIsValid);
			}
		}

		internal OrgCountryData KnownShipperRecord
		{
			get
			{
				if (knownShipperRecord == null)
				{
					if (supplyChainSecurityConfiguration.IsAddressLevelScheme)
					{
						var relevantOrgAddressKnownShipperDetails = GetAddressKnownShipperDetails(relevantOrgAddress, countryCode);
						if (relevantOrgAddressKnownShipperDetails.Length == 1)
						{
							knownShipperRecord = relevantOrgAddressKnownShipperDetails[0];
						}
						else if (relevantOrgHeader != null)
						{
							knownShipperRecord = relevantOrgHeader.Addresses
								.Cast<OrgAddress>()
								.Where(x => x.PK != relevantOrgAddress.PK)
								.Select(x => GetAddressKnownShipperDetails(x, countryCode))
								.FirstOrDefault(x => x.Length == 1 && supplyChainSecurityConfiguration.ApprovalCodeIsOrgLevelApproval(x[0].OV_EXApprovedOrMajorExporter))
								?.FirstOrDefault();
						}
					}
					else if (relevantOrgHeader != null)
					{
						knownShipperRecord = relevantOrgHeader.GetCountryData(countryCode);
					}
				}

				return knownShipperRecord;
			}
		}
		OrgCountryData knownShipperRecord;

		OrgCountryData[] GetAddressKnownShipperDetails(OrgAddress address, ZString country)
		{
			if (address == null)
			{
				return Array.Empty<OrgCountryData>();
			}

			var query = new ZQuery(OrgCountryDataSchema.OV_OA_ApprovedLocation, address.PK);
			query.AddToFilter(OrgCountryDataSchema.OV_RN_NKClientCountryRelation, country);
			return address.Factory.Load<OrgCountryData>(query);
		}

		bool ExpiryDateIsValid
		{
			get
			{
				if (!supplyChainSecurityConfiguration.IsEnabled)
				{
					return true;
				}

				var shipmentDateForAviationSecurity = aviationSecurity.ShipmentDateForAviationSecurity;
				if (shipmentDateForAviationSecurity.IsEmpty)
				{
					return true;
				}

				return shipmentDateForAviationSecurity.IsValid
					&& ((!supplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(knownShipperRecord.OV_EXApprovedOrMajorExporter) && knownShipperRecord.OV_EXApprovalExpiryDate.IsEmpty)
						|| (KnownShipperRecord.OV_EXApprovalExpiryDate.IsValid && KnownShipperRecord.OV_EXApprovalExpiryDate >= shipmentDateForAviationSecurity.Date));
			}
		}

		public bool ExpiryDateWillLapseBeforeShipmentDateForAviationSecurity
		{
			get
			{
				return KnownShipperRecord != null
					&& supplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(KnownShipperRecord.OV_EXApprovedOrMajorExporter)
						&& aviationSecurity.GetErrorForApprovalInvalidForShipment(KnownShipperRecord).IsEmpty
						&& !ExpiryDateIsValid
						&& KnownShipperRecord.OV_EXApprovalExpiryDate >= ZDate.Today
						&& KnownShipperRecord.OV_EXApprovalExpiryDate < aviationSecurity.ShipmentDateForAviationSecurity.Date;
			}
		}
	}
}
