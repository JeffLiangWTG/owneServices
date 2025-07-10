using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using static Enterprise.Core.Constants;
using static Enterprise.Warehouse.Transit.Business.TransitConstants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public static class WhsTransitKnownConsignorHelper
	{
		public static bool IsTransportCompanyKnown(WhsItemReceiveTransportationUnit vehicle)
		{
			var transportOrgAddress = GetAddress(vehicle, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress);
			if (transportOrgAddress == null || transportOrgAddress.OA_RN_NKCountryCode == ZString.Empty)
			{
				return false;
			}

			var driverSecurityActivated = WarehouseDataRegistry.Instance.DriverSecurityCertificationCheckingActivated.Value;
			if (driverSecurityActivated && !IsDriverAccredited(vehicle, transportOrgAddress))
			{
				return false;
			}

			var countryOrEUCode = GetCountryOrEUCode(transportOrgAddress);
			var orgCountryDataList = transportOrgAddress.Header
				.Addresses.Cast<OrgAddress>()
				.SelectMany(a => a.KnownShipperDetails)
				.Where(c => c.OV_RN_NKClientCountryRelation == countryOrEUCode);

			var certifiedHaulierActiveInEU = ZDateTime.Now < new ZDateTime(2027, 01, 01);
			var now = TransitWarehouseHelper.GetNowInCurrentWarehouse(vehicle.Warehouse);
			var dateTimeOffsetNow = new ZDateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, 0, now.Offset).ToZDateTime();

			var known = orgCountryDataList.Any(data =>
				((data.OV_EXApprovedOrMajorExporter == TransportCertType.CertifiedHaulier && certifiedHaulierActiveInEU) ||
					data.OV_EXApprovedOrMajorExporter == TransportCertType.ApprovedHaulier ||
					(data.OV_EXApprovedOrMajorExporter == TransportCertType.RegulatedAgentACENotice && data.OV_OA_ApprovedLocation == transportOrgAddress.PK))
				&& (data.OV_EXApprovalExpiryDate == ZDateTime.Empty || data.OV_EXApprovalExpiryDate >= dateTimeOffsetNow));
			return known;
		}

		static OrgAddress GetAddress(IDocAddresses addressProvider, string addressType)
		{
			var address = addressProvider.DocAddresses?.Where(a => a.E2_AddressType == addressType).FirstOrDefault();
			return address?.Address;
		}

		static string GetCountryOrEUCode(OrgAddress transportOrgAddress)
		{
			var currentCompanyCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString();
			var countryOrEUCode = currentCompanyCountryCode == CountryCodes.UnitedKingdom || !CountryCodes.IsInEuropeanUnionAviationSecurityScheme(currentCompanyCountryCode) ? currentCompanyCountryCode : CountryCodes.EuropeanUnion;
			if (countryOrEUCode == CountryCodes.EuropeanUnion && transportOrgAddress.OA_RN_NKCountryCode == CountryCodes.UnitedKingdom)
			{
				countryOrEUCode = CountryCodes.UnitedKingdom;
			}
			return countryOrEUCode;
		}

		static bool IsDriverAccredited(WhsItemReceiveTransportationUnit vehicle, OrgAddress transportOrgAddress)
		{
			var emptyResult = new HashSet<string>();
			var driverName = vehicle.WRH_SignedBy;
			if (string.IsNullOrWhiteSpace(driverName))
			{
				return false;
			}

			if (transportOrgAddress == null)
			{
				return false;
			}

			var contact = transportOrgAddress.Header.Contacts.Where(c => c.OC_ContactName == driverName).SingleOrDefault();
			if ((contact == null) || (contact.Certificates.Count == 0))
			{
				return false;
			}

			var now = TransitWarehouseHelper.GetNowInCurrentWarehouse(vehicle.Warehouse);
			var dateTimeOffsetNow = new ZDateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, 0, now.Offset).ToZDateTime();
			var validCerts = contact.Certificates
				.Where(cert => cert.XZ_ExpiryOrDueDate >= dateTimeOffsetNow)
				.Select(cert => cert.XZ_Type)
				.ToHashSet();
			
			return validCerts.Contains("BKG") && validCerts.Contains("DTA");
		}
	}
}
