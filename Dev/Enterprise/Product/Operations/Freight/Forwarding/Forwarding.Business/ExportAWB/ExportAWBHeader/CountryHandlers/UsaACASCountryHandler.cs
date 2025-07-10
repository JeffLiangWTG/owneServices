using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Freight.Integration.AWB;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class UsaACASCountryHandler : IACASCountryHandler
	{
		readonly ExportAWBHeader awbHeader;
		const string ShipperAccountHolder = "S";
		const string ConsigneeAccountHolder = "C";
		const string ThirdPartyAccountHolder = "3";
		const string PPD = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
		const string COL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
		const string BTH = ShipmentExportAWBHeader.Constants.PrepaidCollect3CharCodes.Both;
		const string ImmediateTransaction = "I";
		const string OccasionalShipper = "O";
		const string RegularDailyShipper = "B";
		const string HighVolumeShipper = "R";
		readonly string DefaultPaymentType = ObjectFactory.Get<IAccounting>().DefaultPaymentType;

		public UsaACASCountryHandler(ExportAWBHeader exportAWBHeader)
		{
			awbHeader = exportAWBHeader;
		}

		public bool ShouldApplyACAS()
		{
			return awbHeader.ShouldApplyACAS();
		}

		public bool IsVerifiedKnownConsignor()
		{
			if (awbHeader is ConsolExportAWBHeader consolExport)
			{
				var consol = consolExport.Consol;
				return consol != null && consol.Shipments.OfType<ForwardingShipment>().All(shipment => consolExport.SupplyChainSecurityConfiguration.IsVerifiedKnownConsignor(shipment));
			}

			if (awbHeader is ShipmentExportAWBHeader shipmentExport)
			{
				var shipment = shipmentExport.Shipment;
				return shipment != null && shipmentExport.SupplyChainSecurityConfiguration.IsVerifiedKnownConsignor(shipment);
			}

			return false;
		}

		#region ACAS Customer Account Holder and Customer Account Name
		public bool GetCustomerAccountHolderAndName(out string accountHolder, out string accountName)
		{
			accountHolder = string.Empty;
			accountName = string.Empty;

			if (!ShouldApplyACAS())
			{
				return false;
			}

			if (awbHeader is ConsolExportAWBHeader consolHeader)
			{
				return GetFWBCustomerAccountHolderAndName(consolHeader, out accountHolder, out accountName);
			}
			else if (awbHeader is ShipmentExportAWBHeader shipmentHeader)
			{
				return GetFHLCustomerAccountHolderAndName(shipmentHeader, out accountHolder, out accountName);
			}

			return false;
		}

		bool ValidateAccountHolderAndNameForFWB(ConsolExportAWBHeader consolHeader)
		{
			/*
				For direct consol
				When all of below fields are empty, the account holder/name will be blank(invalid)
					Shipment > Addresses > Controlling Customer
					consol > AWB > TW/VAL
					Shipment > Additional Detail > View/Edit AWB > WT/VAL
					Shipment > Basic Registration > Incoterm > Freight Collect
			 */
			if (consolHeader?.Consol == null)
			{
				return true;
			}

			if (consolHeader.IsDirectMAWB)
			{
				var shipment = GetFHLShipmentFromConsol(consolHeader.Consol);
				var emptyConsolAwbTWVAL = string.IsNullOrEmpty(consolHeader.EH_WeightPrepaidCollect);
				var emptyControllCustomer = string.IsNullOrEmpty(shipment?.ControllingCustomer?.OH_FullName);
				var emptyShipmentAwbTWVAL = string.IsNullOrEmpty(shipment?.AWBHeader?.EH_WeightPrepaidCollect);
				var emptyShipmentIncoterm = string.IsNullOrEmpty(shipment?.JS_INCO);

				return !(emptyControllCustomer && emptyConsolAwbTWVAL && emptyShipmentAwbTWVAL && emptyShipmentIncoterm);
			}

			// For non-direct consol, it should be always valid, as Consol > AWB > Shipper Name cannot be empty for sending FWB
			return true;
		}

		public bool ValidateAccountHolderAndNameForFHL(ShipmentExportAWBHeader shipmentHeader)
		{
			// Either shipmentHeader or shipment is null then don't check FHL validation, returns true
			var shipment = shipmentHeader?.Shipment;
			if (shipment == null)
			{
				return true;
			}

			var emptyControllCustomer = string.IsNullOrEmpty(shipment.ControllingCustomer?.OH_FullName);
			var emptyShipmentAwbTWVAL = string.IsNullOrEmpty(shipmentHeader.EH_WeightPrepaidCollect);
			var emptyShipmentIncoterm = string.IsNullOrEmpty(shipment.JS_INCO);

			return !(emptyControllCustomer && emptyShipmentAwbTWVAL && emptyShipmentIncoterm);
		}

		bool GetFWBCustomerAccountHolderAndName(ConsolExportAWBHeader consolHeader, out string accountHolder, out string accountName)
		{
			accountHolder = string.Empty;
			accountName = string.Empty;
			var consol = consolHeader.Consol;
			if (consol == null)
			{
				return false;
			}

			if (consolHeader.IsDirectMAWB)
			{
				/*
				For Direct Consol (DRT):
					if Shipment > Addresses has a Controlling Customer record 
						Account holder - Third-Party Account code '3' is used
						Account name - Controlling Customer's Organization > Details > Details > Full Name is used

					if Consol > AWB > WT/VAL is P (Prepaid):
						Account holder - Shipper Account code 'S' is used
						Account name - Consol > AWB > Shipper Name is used

					if Consol > AWB > WT/VAL is C (Collect):
						Account holder - Consignee Account code 'C' is used
						Account name - Consol >AWB > Consignee Name is used

					if Consol > AWB > WT/VAL is blank then fallback to Shipment > Additional Detail > View/Edit AWB > WT/VAL and follow the same logic to determine 'S' or 'C' code based on P (Freight Prepaid) and C (Freight Collect) value.

					if Shipment > Additional Detail > View/Edit AWB > WT/VAL is B (Both) then see Shipment > Basic Registration > Incoterm > Freight Collect or Freight Prepaid to determine the 'S' or 'C' code (S for Prepaid and C for Collect).
				 */
				var shipment = GetFHLShipmentFromConsol(consol);
				if (shipment?.ControllingCustomer != null)
				{
					accountHolder = ThirdPartyAccountHolder;
					accountName = shipment.ControllingCustomer.OH_FullName;
				}
				else if (consolHeader.EH_WeightPPD)
				{
					accountHolder = ShipperAccountHolder;
					accountName = consolHeader.EH_ShipperName;
				}
				else if (consolHeader.EH_WeightCOL)
				{
					accountHolder = ConsigneeAccountHolder;
					accountName = consolHeader.EH_ConsigneeName;
				}
				else if (string.IsNullOrEmpty(consolHeader.EH_WeightPrepaidCollect))
				{
					if (shipment != null)
					{
						GetAccountHolderAndNameFromShipment(shipment, out accountHolder, out accountName);
					}
				}
			}
			else
			{
				// For Non-Direct Consol
				// Account holder will always be a Third-Party Account code '3'
				// Account name will always be Consol > AWB > Shipper Name
				accountHolder = ThirdPartyAccountHolder;
				accountName = consolHeader.EH_ShipperName;
			}

			return !string.IsNullOrWhiteSpace(accountHolder) && !string.IsNullOrWhiteSpace(accountName);
		}

		void GetAccountHolderAndNameFromShipment(ForwardingShipment shipment, out string accountHolder, out string accountName)
		{
			accountHolder = string.Empty;
			accountName = string.Empty;
			if (shipment?.AWBHeader == null)
			{
				return;
			}

			/*
			if Shipment > Addresses has a Controlling Customer record
				Third-Party Account code '3' is used
				Controlling Customer's Organization > Details > Details > Full Name is used
			if Shipment > Additional Detail > View/Edit AWB > WT/VAL is P (Prepaid)
				Shipper Account code 'S' is used
				Shipper Name is used
			if Shipment > Additional Detail > View/Edit AWB > WT/VAL is C (Collect)
				Consignee Account code 'C' is used
				Consignee Name is used
			If Shipment > Additional Detail > View/Edit AWB > WT/VAL is B (Both)
				then see Shipment > Basic Registration > Incoterm > Freight Collect or Freight Prepaid
				to determine the 'S' or 'C' code (S for Prepaid and C for Collect).
			 */
			if (shipment.ControllingCustomer != null)
			{
				accountHolder = ThirdPartyAccountHolder;
				accountName = shipment.ControllingCustomer.OH_FullName;
				return;
			}

			switch (shipment.AWBHeader.EH_WeightVPPDCOL)
			{
				case PPD:
					accountHolder = ShipperAccountHolder;
					accountName = shipment.AWBHeader.EH_ShipperName;
					break;

				case COL:
					accountHolder = ConsigneeAccountHolder;
					accountName = shipment.AWBHeader.EH_ConsigneeName;
					break;

				case BTH:
					if (shipment.AWBHeader.WeightVPPDCOL == PPD)
					{
						accountHolder = ShipperAccountHolder;
						accountName = shipment.AWBHeader.EH_ShipperName;
					}
					else if (shipment.AWBHeader.WeightVPPDCOL == COL)
					{
						accountHolder = ConsigneeAccountHolder;
						accountName = shipment.AWBHeader.EH_ConsigneeName;
					}
					break;
			}
		}

		bool GetFHLCustomerAccountHolderAndName(ShipmentExportAWBHeader shipmentHeader, out string accountHolder, out string accountName)
		{
			accountHolder = string.Empty;
			accountName = string.Empty;
			var shipment = shipmentHeader.Shipment;
			if (shipment == null)
			{
				return false;
			}

			GetAccountHolderAndNameFromShipment(shipment, out accountHolder, out accountName);

			return !string.IsNullOrWhiteSpace(accountHolder) && !string.IsNullOrWhiteSpace(accountName);
		}
		#endregion

		#region ACAS Customer Account Number and Customer Account Issuer
		public bool GetCustomerAccountIssuerAndNumber(out string accountIssuer, out string accountNumber)
		{
			accountIssuer = string.Empty;
			accountNumber = string.Empty;

			if (!ShouldApplyACAS())
			{
				return false;
			}

			if (awbHeader is ConsolExportAWBHeader consolHeader)
			{
				return GetFWBCustomerAccountIssuerAndNumber(consolHeader, ref accountIssuer, ref accountNumber);
			}
			else if (awbHeader is ShipmentExportAWBHeader shipmentHeader)
			{
				return GetFHLCustomerAccountIssuerAndNumber(shipmentHeader, ref accountIssuer, ref accountNumber);
			}

			return false;
		}

		bool GetFWBCustomerAccountIssuerAndNumber(ConsolExportAWBHeader consolHeader, ref string accountIssuer, ref string accountNumber)
		{
			accountIssuer = GetFWBCustomerAccountIssuer(consolHeader).ToAlphaNumericOnly();
			if (string.IsNullOrEmpty(accountIssuer))
			{
				return false;
			}

			accountNumber = GetFWBCustomerAccountNumber(consolHeader).ToAlphaNumericOnly();
			if (string.IsNullOrEmpty(accountNumber))
			{
				return false;
			}

			return true;
		}

		bool GetFHLCustomerAccountIssuerAndNumber(ShipmentExportAWBHeader shipmentHeader, ref string accountIssuer, ref string accountNumber)
		{
			accountIssuer = GetFHLCustomerAccountIssuer(shipmentHeader).ToAlphaNumericOnly();
			if (string.IsNullOrEmpty(accountIssuer))
			{
				return false;
			}

			accountNumber = GetFHLCustomerAccountNumber(shipmentHeader).ToAlphaNumericOnly();
			if (string.IsNullOrEmpty(accountNumber))
			{
				return false;
			}

			return true;
		}

		string GetFWBCustomerAccountIssuer(ConsolExportAWBHeader consolHeader)
		{
			if (consolHeader.IsDirectMAWB)
			{
				var regNumber = GetCompanyProxyOrganisationRegistrationNumber();
				if (!string.IsNullOrEmpty(regNumber))
				{
					return regNumber;
				}

				// Fall-back: Consol > AWB > Agents IATA Code value is used
				return consolHeader.EH_AgentIATACodeFormatted;
			}
			else
			{
				// Non-Direct Consol: Consol > Carrier > Organisation > Carrier > Master Bill Prefix
				return consolHeader.Consol?.ShippingLine?.MiscServ?.Airline?.RM_EagleAddedAirlinePrefixOrAccountingCode;
			}
		}

		string GetFWBCustomerAccountNumber(ConsolExportAWBHeader consolHeader)
		{
			var consol = consolHeader.Consol;
			var shipment = GetFHLShipmentFromConsol(consol);

			if (consolHeader.IsDirectMAWB)
			{
				// Direct Consol (DRT):
				// if Shipment > Addresses has a Controlling Customer record then Shipment > Addresses > Controlling Customer > Organisation Code field is used.
				var ctrlCustomerOrgCode = shipment?.ControllingCustomer?.OH_Code;
				if (!string.IsNullOrEmpty(ctrlCustomerOrgCode))
				{
					return ctrlCustomerOrgCode;
				}

				/*
					Fall-backs:
						If Consol > AWB > WT/VAL value is prepaid (P), Consol > AWB > Shipper’s Name and Address > Account Number value is used.
						If Consol > AWB > WT/VAL value is collect (C), Consol > AWB > Consignee’s Name and Address > Account Number value used.
						If Consol > AWB > WT/VAL is blank, then Shipment > Additional Detail > View/Edit AWB > WT/VAL is checked for P (Freight Prepaid) or C (Freight Collect) to determine if Consol > AWB > Shipper’s Name and Address > Account Number or Consol > AWB > Consignee’s Name and Address > Account Number value is used (based on prepaid (P) or collect (C) consol payment/incoterm logic outlined above).
						If Shipment > Additional Detail > View/Edit AWB > WT/VAL is B (Both) then Shipment > Basic Registration > Incoterm is checked for P (Freight Prepaid) or C (Freight Collect) to determine which Account Number value is used. 
				 */
				var shouldUseShipperInfo = consolHeader.EH_WeightPPD
					|| string.Equals(shipment?.AWBHeader?.EH_WeightVPPDCOL, PPD)
					|| string.Equals(shipment?.AWBHeader?.WeightVPPDCOL, PPD);

				if (shouldUseShipperInfo && !string.IsNullOrEmpty(consolHeader.EH_ShipperAccount))
				{
					return consolHeader.EH_ShipperAccount;
				}

				var shouldUseConsigneeInfo = consolHeader.EH_WeightCOL
					|| string.Equals(shipment?.AWBHeader?.EH_WeightVPPDCOL, COL)
					|| string.Equals(shipment?.AWBHeader?.WeightVPPDCOL, COL);

				if (shouldUseConsigneeInfo && !string.IsNullOrEmpty(consolHeader.EH_ConsigneeAccount))
				{
					return consolHeader.EH_ConsigneeAccount;
				}
			}
			else
			{
				// For a Non-Direct Consol, the Customer Account Number value for the FWB is taken from the
				// air consol Carrier > Configuration > Airline Account Number > Branch Specific Airline Account Number field
				// based on the Carrier Organisation and the logged in branch
				var carrierOrg = consol.ShippingLine;
				var accountNumberOfCarrierOfCurrentBranch = carrierOrg?.OrgAirlineBranchAccounts.FirstOrDefault(x => x.OAA_GB_Branch == GlbBranch.CurrentBranch.PK)?.OAA_APAirlineAccountNumber;
				if (!string.IsNullOrEmpty(accountNumberOfCarrierOfCurrentBranch))
				{
					return accountNumberOfCarrierOfCurrentBranch;
				}

				// Fall-back: If there is no Account Number against the logged in Branch, then: Carrier > Configuration > Airline Account Number is used.
				var accountNumberOfCarrier = carrierOrg?.CompanyData?.OB_APAirlineAccountNumber;
				if (!string.IsNullOrEmpty(accountNumberOfCarrier))
				{
					return accountNumberOfCarrier;
				}
			}

			return string.Empty;
		}

		string GetFHLCustomerAccountIssuer(ShipmentExportAWBHeader shipmentHeader)
		{
			var regNumber = GetCompanyProxyOrganisationRegistrationNumber();
			if (!string.IsNullOrEmpty(regNumber))
			{
				return regNumber;
			}

			// Fall-back: Shipment > Additional Detail > View / Edit AWB > Agents IATA Code value is used.
			return shipmentHeader?.EH_AgentIATACodeFormatted ?? string.Empty;
		}

		string GetFHLCustomerAccountNumber(ShipmentExportAWBHeader shipmentHeader)
		{
			if (shipmentHeader == null)
			{
				return string.Empty;
			}

			// If Shipment > Addresses has a Controlling Customer record then Shipment > Addresses > Controlling Customer > Organisation Code field is used.
			var ctrlCustomerOrgCode = shipmentHeader.Shipment?.ControllingCustomer?.OH_Code;
			if (!string.IsNullOrEmpty(ctrlCustomerOrgCode))
			{
				return ctrlCustomerOrgCode;
			}

			// Fall-back:
			//	If Shipment > Additional Detail > View / Edit AWB > WT / VAL is prepaid(P), Shipment > Additional Detail > View / Edit AWB > Shipper’s Name and Address > Account Number value is used
			//	If Shipment > Additional Detail > View / Edit AWB > WT / VAL is collect(C), the Shipment > Additional Detail > View / Edit AWB > Consignee’s Name and Address > Account Number value is used.
			//	If Shipment > Additional Detail > View / Edit AWB > WT / VAL is B(Both) then see Shipment > Basic Registration > Incoterm > Freight Collect or Freight Prepaid to determine which Account Number value is used.
			var shouldUseShipperInfo = shipmentHeader.EH_WeightVPPDCOL == PPD
				|| shipmentHeader.EH_WeightVPPDCOL == BTH && shipmentHeader.WeightVPPDCOL == PPD;

			if (shouldUseShipperInfo && !string.IsNullOrEmpty(shipmentHeader.EH_ShipperAccount))
			{
				return shipmentHeader.EH_ShipperAccount;
			}

			var shouldUseConsigneeInfo = shipmentHeader.EH_WeightVPPDCOL == COL
					|| shipmentHeader.EH_WeightVPPDCOL == BTH && shipmentHeader.WeightVPPDCOL == COL;

			if (shouldUseConsigneeInfo && !string.IsNullOrEmpty(shipmentHeader.EH_ConsigneeAccount))
			{
				return shipmentHeader.EH_ConsigneeAccount;
			}

			return string.Empty;
		}

		string GetCompanyProxyOrganisationRegistrationNumber()
		{
			// If Company Organisation Proxy: Organisation > Details > Config > Registration Numbers / Codes
			// has a 'Country / Region of Issue' record of US with 'Type' of CCA(Standard Carrier Alpha Code -Air)
			// then the value in the 'Registration Number / Code' field is used.
			var currentOrgProxy = GlbCompany.CurrentCompany?.OrgProxy;
			var usaCCACode = currentOrgProxy?.CustomsCodes?.Cast<OrgCusCode>()?.FirstOrDefault(
				x => x.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.UnitedStates && x.OK_CodeType == OrgCusCode.USACodeTypes.StandardCarrierAlphaCodeAir);
			var regNumber = usaCCACode?.SecuredCustomsRegNo;
			return regNumber;
		}
		#endregion

		#region ACAS Customer Account Establisment Date and Billing Type

		public bool GetCustomerAccountEstablishmentDate(out string establishmentDate)
		{
			establishmentDate = string.Empty;

			if (!ShouldApplyACAS())
			{
				return false;
			}

			if (awbHeader is ConsolExportAWBHeader consolHeader)
			{
				establishmentDate = GetFWBCustomerAccountEstablishmentDate(consolHeader);
				return !string.IsNullOrEmpty(establishmentDate);
			}
			else if (awbHeader is ShipmentExportAWBHeader shipmentHeader)
			{
				establishmentDate = GetFHLCustomerAccountEstablishmentDate(shipmentHeader);
				return !string.IsNullOrEmpty(establishmentDate);
			}

			return false;
		}

		string GetFWBCustomerAccountEstablishmentDate(ConsolExportAWBHeader consolHeader)
		{
			var consol = consolHeader.Consol;
			var shipment = GetFHLShipmentFromConsol(consol);

			if (consolHeader.IsDirectMAWB)
			{
				// Direct Consol (DRT):
				// if Shipment > Addresses has a Controlling Customer record then Shipment > Addresses > Controlling Customer > Creation Date is used.
				var ctrlCustomerCreationDate = GetFormattedDate(shipment?.ControllingCustomer);
				if (!string.IsNullOrEmpty(ctrlCustomerCreationDate))
				{
					return ctrlCustomerCreationDate;
				}

				/*
					Fall-backs:
						If Consol > AWB > WT/VAL value is prepaid (P), Consol > AWB > Creation Date of the Shipper Organisation is used.
						If Consol > AWB > WT/VAL value is collect (C), Consol > AWB > Creation Date of the Consignee Organisation is used.
						If Consol > AWB > WT/VAL is blank, then Shipment > Additional Detail > View/Edit AWB > WT/VAL is checked for P (Freight Prepaid) or C (Freight Collect) to determine the status.
				 */
				var shouldUseShipperInfo = consolHeader.EH_WeightPPD
					|| string.Equals(shipment?.AWBHeader?.EH_WeightVPPDCOL, PPD)
					|| string.Equals(shipment?.AWBHeader?.WeightVPPDCOL, PPD);

				if (shouldUseShipperInfo && !string.IsNullOrEmpty(GetFormattedDate(shipment?.Consignor)))
				{
					return GetFormattedDate(shipment?.Consignor);
				}

				var shouldUseConsigneeInfo = consolHeader.EH_WeightCOL
					|| string.Equals(shipment?.AWBHeader?.EH_WeightVPPDCOL, COL)
					|| string.Equals(shipment?.AWBHeader?.WeightVPPDCOL, COL);

				if (shouldUseConsigneeInfo && !string.IsNullOrEmpty(GetFormattedDate(shipment?.Consignee)))
				{
					return GetFormattedDate(shipment?.Consignee);
				}
			}
			else
			{
				// For a Non-Direct Consol, Creation date of the 'Sending Agent' organization record in CW.
				if (!string.IsNullOrEmpty(GetFormattedDate(consol.SendingForwarder)))
				{
					return GetFormattedDate(consol.SendingForwarder);
				}
			}

			return string.Empty;
		}

		string GetFHLCustomerAccountEstablishmentDate(ShipmentExportAWBHeader shipmentHeader)
		{
			if (shipmentHeader == null)
			{
				return string.Empty;
			}

			var shipment = shipmentHeader.Shipment;

			var ctrlCustomerCreationDate = GetFormattedDate(shipment?.ControllingCustomer);
			if (!string.IsNullOrEmpty(ctrlCustomerCreationDate))
			{
				return ctrlCustomerCreationDate;
			}

			/*
				Fall-backs:
					If Consol > AWB > WT/VAL value is prepaid (P), Consol > AWB > Creation Date of the Shipper Organisation is used.
					If Consol > AWB > WT/VAL value is collect (C), Consol > AWB > Creation Date of the Consignee Organisation is used.
					If Consol > AWB > WT/VAL is blank, then Shipment > Additional Detail > View/Edit AWB > WT/VAL is checked for P (Freight Prepaid) or C (Freight Collect) to determine the status.
			 */
			var shouldUseShipperInfo = shipmentHeader.EH_WeightVPPDCOL == PPD
				|| shipmentHeader.EH_WeightVPPDCOL == BTH && shipmentHeader.WeightVPPDCOL == PPD;

			if (shouldUseShipperInfo && !string.IsNullOrEmpty(GetFormattedDate(shipment?.Consignor)))
			{
				return GetFormattedDate(shipment?.Consignor);
			}
			else
			{
				var shouldUseConsigneeInfo = shipmentHeader.EH_WeightVPPDCOL == COL
						|| shipmentHeader.EH_WeightVPPDCOL == BTH && shipmentHeader.WeightVPPDCOL == COL;

				if (shouldUseConsigneeInfo && !string.IsNullOrEmpty(GetFormattedDate(shipment?.Consignee)))
				{
					return GetFormattedDate(shipment?.Consignee);
				}
			}

			return string.Empty;
		}

		public bool GetCustomerAccountBillingType(out string billingType)
		{
			billingType = string.Empty;

			if (!ShouldApplyACAS())
			{
				return false;
			}

			if (awbHeader is ConsolExportAWBHeader consolHeader)
			{
				billingType = GetFWBCustomerAccountBillingType(consolHeader);
				return !string.IsNullOrEmpty(billingType);
			}
			else if (awbHeader is ShipmentExportAWBHeader shipmentHeader)
			{
				billingType = GetFHLCustomerAccountBillingType(shipmentHeader);
				return !string.IsNullOrEmpty(billingType);
			}

			return false;
		}

		string GetFWBCustomerAccountBillingType(ConsolExportAWBHeader consolHeader)
		{
			var consol = consolHeader.Consol;
			var shipment = GetFHLShipmentFromConsol(consol);

			string billingCode = null;

			if (consolHeader.IsDirectMAWB)
			{
				// Direct Consol (DRT):
				// if Shipment > Addresses has a Controlling Customer record then Organisation > A/P > Configuration > Credit Details > Agreed Payment Method
				if (shipment?.ControllingCustomer != null)
				{
					if (shipment?.ControllingCustomer?.OH_IsCreditor ?? false)
					{
						billingCode = GetAgreedPaymentMethod(shipment?.ControllingCustomer);
					}
				}
				else
				{
					/*
					Fall-backs:
						If Consol > AWB > WT/VAL value is prepaid (P),Consol > AWB > Agreed Payment Method of the Shipper Organisation.
						If Consol > AWB > WT/VAL value is collect (C), Consol > AWB > Agreed Payment Method of the Consignee Organisation.
						If Consol > AWB > WT/VAL is blank, then Shipment > Additional Detail > View/Edit AWB > WT/VAL is checked for P (Freight Prepaid) or C (Freight Collect) to determine the status.
					*/
					var shouldUseShipperInfo = consolHeader.EH_WeightPPD
						|| string.Equals(shipment?.AWBHeader?.EH_WeightVPPDCOL, PPD)
						|| string.Equals(shipment?.AWBHeader?.WeightVPPDCOL, PPD);

					var shouldUseConsigneeInfo = consolHeader.EH_WeightCOL
						|| string.Equals(shipment?.AWBHeader?.EH_WeightVPPDCOL, COL)
						|| string.Equals(shipment?.AWBHeader?.WeightVPPDCOL, COL);

					if (shouldUseShipperInfo)
					{
						if (shipment?.Consignor?.OH_IsCreditor ?? false)
						{
							billingCode = GetAgreedPaymentMethod(shipment?.Consignor);
						}
					}
					else if (shouldUseConsigneeInfo)
					{
						if (shipment?.Consignee?.OH_IsCreditor ?? false)
						{
							billingCode = GetAgreedPaymentMethod(shipment?.Consignee);
						}
					}
				}
			}
			else
			{
				// For a Non-Direct Consol, Billing Type of the 'Sending Agent' organization record in CW.
				if (consol.SendingForwarder?.OH_IsCreditor ?? false)
				{
					billingCode = GetAgreedPaymentMethod(consol.SendingForwarder);
				}
			}

			if (billingCode != null)
			{
				if (billingCode.Length == 0)
				{
					billingCode = DefaultPaymentType;
					return CWToACASBillingTypeDictionary.Values.Contains(billingCode) ? billingCode : "EFT";
				}

				return CWToACASBillingTypeDictionary.TryGetValue(billingCode, out string value) ? value : "EFT";
			}
			else
			{
				return string.Empty;
			}
		}

		string GetFHLCustomerAccountBillingType(ShipmentExportAWBHeader shipmentHeader)
		{
			if (shipmentHeader == null)
			{
				return string.Empty;
			}

			var shipment = shipmentHeader.Shipment;
			string billingCode = null;
			if (shipment?.ControllingCustomer != null)
			{
				if (shipment?.ControllingCustomer?.OH_IsCreditor ?? false)
				{
					billingCode = GetAgreedPaymentMethod(shipment?.ControllingCustomer);
				}
			}
			else
			{
				/*
				Fall-backs:
					If Consol > AWB > WT/VAL value is prepaid (P),Consol > AWB > Agreed Payment Method of the Shipper Organisation.
					If Consol > AWB > WT/VAL value is collect (C), Consol > AWB > Agreed Payment Method of the Consignee Organisation.
					If Consol > AWB > WT/VAL is blank, then Shipment > Additional Detail > View/Edit AWB > WT/VAL is checked for P (Freight Prepaid) or C (Freight Collect) to determine the status.
				*/
				var shouldUseShipperInfo = shipmentHeader.EH_WeightVPPDCOL == PPD
					|| shipmentHeader.EH_WeightVPPDCOL == BTH && shipmentHeader.WeightVPPDCOL == PPD;

				if (shouldUseShipperInfo)
				{
					if (shipment?.Consignor?.OH_IsCreditor ?? false)
					{
						billingCode = GetAgreedPaymentMethod(shipment?.Consignor);
					}
				}
				else
				{
					var shouldUseConsigneeInfo = shipmentHeader.EH_WeightVPPDCOL == COL
							|| shipmentHeader.EH_WeightVPPDCOL == BTH && shipmentHeader.WeightVPPDCOL == COL;

					if (shouldUseConsigneeInfo)
					{
						if (shipment?.Consignee?.OH_IsCreditor ?? false)
						{
							billingCode = GetAgreedPaymentMethod(shipment?.Consignee);
						}
					}
				}
			}

			if (billingCode != null)
			{
				if (billingCode.Length == 0)
				{
					billingCode = DefaultPaymentType;
					return CWToACASBillingTypeDictionary.Values.Contains(billingCode) ? billingCode : "EFT";
				}

				return CWToACASBillingTypeDictionary.TryGetValue(billingCode, out string value) ? value : "EFT";
			}
			else
			{
				return string.Empty;
			}
		}

		internal static Dictionary<string, string> CWToACASBillingTypeDictionary = new Dictionary<string, string>
		{
			{ "TRF", "EFT" },
			{ "CCD", "CC" },
			{ "CBC", "CSH" },
			{ "CHK", "CHQ" },
		};

		#endregion

		#region FWB/FHL Warnings
		public bool HasWarningForFWB(out List<string> fwbWarnings)
		{
			fwbWarnings = new List<string>();

			if (!ShouldApplyACAS())
			{
				return false;
			}

			if (!ValidateAccountHolderAndNameForFWB(awbHeader as ConsolExportAWBHeader))
			{
				fwbWarnings.Add(Res.GetString("2cd2d437-480e-4285-af86-b245fc974743", "The Customer Account Holder and Customer Account Name could not be determined for ACAS reporting requirements and will not be sent to the airline."));
			}

			if (!GetCustomerAccountIssuerAndNumber(out var _, out var _))
			{
				fwbWarnings.Add(Res.GetString("0a32080a-e53f-47e4-b156-57f7b625d07f", "The Customer Account Number and/or Customer Account Issuer could not be determined for ACAS reporting requirements and will not be sent to the airline."));
			}

			if (!GetCustomerAccountEstablishmentDate(out var _))
			{
				fwbWarnings.Add(Res.GetString("38782A9F-276D-4388-93FA-F8CB55AB2533", "The Customer Account Establishment Date could not be determined for ACAS reporting requirements and will not be sent to the airline."));
			}

			if (!GetCustomerAccountBillingType(out var _))
			{
				fwbWarnings.Add(Res.GetString("D2139DA7-7C46-497F-9212-C2A37E52FB6A", "The Customer Account Billing Type could not be determined for ACAS reporting requirements and will not be sent to the airline."));
			}

			if (string.IsNullOrEmpty(GetCustomerAccountShippingFrequency()))
			{
				fwbWarnings.Add(Res.GetString("3da144fd-8701-41e6-a6c5-a3241cbe62dc", "The Customer Account Shipping Frequency/Volume could not be determined for ACAS reporting requirements and will not be sent to the airline."));
			}

			var fwbBioData = GetBiographicData();
			if (fwbBioData.isNaturalPersonOrg && string.IsNullOrEmpty(fwbBioData.idType))
			{
				fwbWarnings.Add(Res.GetString("6826e60c-8400-48b4-8798-a743277f02f8", "The Biographic Data (e.g. passport number or drivers license of a Natural Person/Individual Organization type) could not be determined for ACAS reporting requirements and will not be sent to the airline."));
			}

			return fwbWarnings.Count > 0;
		}

		public bool HasWarningsForFHL(out List<string> fhlWarnings)
		{
			fhlWarnings = new List<string>();
			var consol = awbHeader.Consol;
			if (consol == null || !ShouldApplyACAS())
			{
				return false;
			}

			var accountHolderAndNameWarning = new List<string>();
			var accountIssuerAndNumberWarning = new List<string>();
			var accountEstablishmentDateWarning = new List<string>();
			var accountBillingTypeWarning = new List<string>();
			var shippingFrequencyWarning = new List<string>();
			var bioDataWarning = new List<string>();

			foreach (var shipment in consol.Shipments.Cast<ForwardingShipment>().Where(x => x.IsFHLShipment()))
			{
				var shipmentAwbHeader = shipment.AWBHeader as ShipmentExportAWBHeader;
				if (!ValidateAccountHolderAndNameForFHL(shipmentAwbHeader))
				{
					accountHolderAndNameWarning.Add(shipment.JS_HouseBill);
				}

				var accountName = string.Empty;
				var accountIssuer = string.Empty;
				if (!GetFHLCustomerAccountIssuerAndNumber(shipmentAwbHeader, ref accountIssuer, ref accountName))
				{
					accountIssuerAndNumberWarning.Add(shipment.JS_HouseBill);
				}

				if (string.IsNullOrEmpty(GetFHLCustomerAccountEstablishmentDate(shipmentAwbHeader)))
				{
					accountEstablishmentDateWarning.Add(shipment.JS_HouseBill);
				}

				if (string.IsNullOrEmpty(GetFHLCustomerAccountBillingType(shipmentAwbHeader)))
				{
					accountBillingTypeWarning.Add(shipment.JS_HouseBill);
				}

				if (string.IsNullOrEmpty(GetFHLCustomerAccountShippingFrequency(shipmentAwbHeader)))
				{
					shippingFrequencyWarning.Add(shipment.JS_HouseBill);
				}

				var fhlBioData = GetFHLBiographicData(shipmentAwbHeader);
				if (fhlBioData.isNaturalPersonOrg && string.IsNullOrEmpty(fhlBioData.idType))
				{
					bioDataWarning.Add(shipment.JS_HouseBill);
				}
			}

			if (accountHolderAndNameWarning.Count > 0)
			{
				accountHolderAndNameWarning.Sort();
				fhlWarnings.Add(string.Format(
					Res.GetString("b22ab0e2-bfa2-4bca-a414-b8bc92fe3e4d", "{0} - The Customer Account Holder and Customer Account Name could not be determined for ACAS reporting requirements and will not be sent to the airline."),
					string.Join(", ", accountHolderAndNameWarning)));
			}

			if (accountIssuerAndNumberWarning.Count > 0)
			{
				accountIssuerAndNumberWarning.Sort();
				fhlWarnings.Add(string.Format(
					Res.GetString("0322fdb3-6086-4d09-886c-6ab49ead75e6", "{0} - The Customer Account Number and/or Customer Account Issuer could not be determined for ACAS reporting requirements and will not be sent to the airline."),
					string.Join(", ", accountIssuerAndNumberWarning)));
			}

			if (accountEstablishmentDateWarning.Count > 0)
			{
				accountEstablishmentDateWarning.Sort();
				fhlWarnings.Add(string.Format(
					Res.GetString("88F2AF8C-2134-43E9-B25A-B3BF52256B91", "{0} - The Customer Account Establishment Date could not be determined for ACAS reporting requirements and will not be sent to the airline."),
					string.Join(", ", accountEstablishmentDateWarning)));
			}

			if (accountBillingTypeWarning.Count > 0)
			{
				accountBillingTypeWarning.Sort();
				fhlWarnings.Add(string.Format(
					Res.GetString("BF353D1E-541F-4ECF-BF69-B8BBCA9256CF", "{0} - The Customer Account Billing Type could not be determined for ACAS reporting requirements and will not be sent to the airline."),
					string.Join(", ", accountBillingTypeWarning)));
			}

			if (shippingFrequencyWarning.Count > 0)
			{
				shippingFrequencyWarning.Sort();
				fhlWarnings.Add(string.Format(
					Res.GetString("7f37cc90-c450-477d-bc9c-ebc413e4f33a", "{0} - The Customer Account Shipping Frequency/Volume could not be determined for ACAS reporting requirements and will not be sent to the airline."),
					string.Join(", ", shippingFrequencyWarning)));
			}

			if (bioDataWarning.Count > 0)
			{
				bioDataWarning.Sort();
				fhlWarnings.Add(string.Format(
					Res.GetString("58729a8d-099e-4f8c-88ce-1c16846f8406", "{0} - The Biographic Data (e.g. passport number or drivers license of a Natural Person/Individual Organization type) could not be determined for ACAS reporting requirements and will not be sent to the airline."),
					string.Join(", ", bioDataWarning)));
			}

			return fhlWarnings.Count > 0;
		}
		#endregion

		ForwardingShipment GetFHLShipmentFromConsol(ForwardingConsol consol)
			=> consol?.Shipments?.OfType<ForwardingShipment>().FirstOrDefault(shipment => shipment.IsFHLShipment());

		ForwardingConsol GetFWBConsolFromShipment(ForwardingShipment shipment)
			=> shipment?.Consols?.OfType<ForwardingConsol>().FirstOrDefault(consol => consol.IsAWBHeaderAccessible);

		string GetFormattedDate(OrgHeader organisation)
		{
			if (!organisation?.OH_SystemCreateTimeUtc.IsEmpty ?? false)
			{
				return organisation.OH_SystemCreateTimeUtc.ToString("ddMMMyy", System.Globalization.CultureInfo.InvariantCulture);
			}

			return null;
		}

		string GetAgreedPaymentMethod(OrgHeader organisation)
		{
			return organisation?.CompanyData?.OB_APCreditAgreedPaymentMethod ?? string.Empty;
		}

		#region ACAS Customer Account Shipping Frequency / Volume
		public string GetCustomerAccountShippingFrequency()
		{
			if (!ShouldApplyACAS())
			{
				return string.Empty;
			}

			if (awbHeader is ConsolExportAWBHeader consolHeader)
			{
				return GetFWBCustomerAccountShippingFrequency(consolHeader);
			}
			else if (awbHeader is ShipmentExportAWBHeader shipmentHeader)
			{
				return GetFHLCustomerAccountShippingFrequency(shipmentHeader);
			}

			return string.Empty;
		}

		string GetFWBCustomerAccountShippingFrequency(ConsolExportAWBHeader consolHeader)
		{
			var consol = consolHeader.Consol;
			if (consol == null)
			{
				return string.Empty;
			}

			var (orgRole, orgHeader, orgAddress) = ChooseOrganization(consol);
			return GetCustomerAccountShippingFrequencyFromOrg(orgRole, orgHeader, orgAddress);
		}

		string GetFHLCustomerAccountShippingFrequency(ShipmentExportAWBHeader shipmentHeader)
		{
			var shipment = shipmentHeader?.Shipment;
			if (shipment == null)
			{
				return string.Empty;
			}

			// For FHL message, the consol header PPD should not take effect
			var (orgRole, orgHeader, orgAddress) = ChooseOrganization(shipment, shouldCheckConsolHeaderPPD: false);
			return GetCustomerAccountShippingFrequencyFromOrg(orgRole, orgHeader, orgAddress);
		}

		(OrganizationRole orgRole, OrgHeader orgHeader, OrgAddress orgAddress) ChooseOrganization(ForwardingConsol consol)
		{
			if (!consol.IsDirect)
			{
				// Consol sending agent
				return (OrganizationRole.ConsolSendingAgent, consol.SendingForwarder, consol.SendingForwarderAddress);
			}
			else
			{
				return ChooseOrganization(GetFHLShipmentFromConsol(consol), shouldCheckConsolHeaderPPD: true);
			}
		}

		(OrganizationRole orgRole, OrgHeader orgHeader, OrgAddress orgAddress) ChooseOrganization(ForwardingShipment shipment, bool shouldCheckConsolHeaderPPD)
		{
			if (shipment == null || !shipment.IsFHLShipment())
			{
				return (OrganizationRole.NotApplicable, null, null);
			}

			// shipment controlling customer
			if (shipment.ControllingCustomer != null)
			{
				return (OrganizationRole.ControllingCustomer, shipment.ControllingCustomer, null);
			}

			// Shipper
			var shouldChooseShipper = shouldCheckConsolHeaderPPD && GetFWBConsolFromShipment(shipment)?.AWBHeader?.EH_WeightPPD == true
				|| string.Equals(shipment.AWBHeader?.EH_WeightVPPDCOL, PPD)
				|| string.Equals(shipment.AWBHeader?.EH_WeightVPPDCOL, BTH) && string.Equals(shipment.AWBHeader?.WeightVPPDCOL, PPD);

			if (shouldChooseShipper)
			{
				return (OrganizationRole.Shipper, shipment.Consignor, null);
			}

			// Consignee
			var shouldChooseConsignee = shouldCheckConsolHeaderPPD && GetFWBConsolFromShipment(shipment)?.AWBHeader?.EH_WeightCOL == true
				|| string.Equals(shipment.AWBHeader?.EH_WeightVPPDCOL, COL)
				|| string.Equals(shipment.AWBHeader?.EH_WeightVPPDCOL, BTH) && string.Equals(shipment.AWBHeader?.WeightVPPDCOL, COL);

			if (shouldChooseConsignee)
			{
				return (OrganizationRole.Consignee, shipment.Consignee, null);
			}

			return (OrganizationRole.NotApplicable, null, null);
		}

		string GetCustomerAccountShippingFrequencyFromOrg(OrganizationRole orgRole, OrgHeader orgHeader, OrgAddress orgAddress)
		{
			if (orgHeader == null || orgHeader.PK == ZGuid.Empty || orgRole == OrganizationRole.NotApplicable)
			{
				return string.Empty;
			}

			// if Organization > Details > Details > Organization Type > Temporary Acct is ticked
			if (orgHeader.OH_IsTempAccount)
			{
				return ImmediateTransaction;
			}

			var shippingCount = CalculateShippingCount(orgRole, orgHeader, orgAddress);
			if (shippingCount == -1)
			{
				return string.Empty;
			}

			// more than 60x shipments created in the last 30x days
			if (shippingCount > 60)
			{
				return HighVolumeShipper;
			}

			// between 30 - 60 shipments created in the last 30x days
			if (shippingCount >= 30)
			{
				return RegularDailyShipper;
			}

			// less than 30x shipments created in the last 30x days
			return OccasionalShipper;
		}

		readonly Dictionary<string, int> orgShippingCountDict = new Dictionary<string, int>();

		int CalculateShippingCount(OrganizationRole orgRole, OrgHeader orgHeader, OrgAddress orgAddress)
		{
			var key = Enum.GetName(typeof(OrganizationRole), orgRole) + (orgRole == OrganizationRole.ConsolSendingAgent ? orgAddress.PK : orgHeader.PK);
			var inUnitTest = false;
#if DEBUG
			inUnitTest = true;
#endif

			if (inUnitTest || !orgShippingCountDict.ContainsKey(key))
			{
				ZDateTime from = ZDateTime.UtcNow.AddDays(-30);
				ZDateTime to = ZDateTime.UtcNow;
				switch (orgRole)
				{
					case OrganizationRole.ConsolSendingAgent:
						if (orgAddress == null || orgAddress == ZGuid.Empty)
						{
							orgShippingCountDict[key] = -1;
						}
						orgShippingCountDict[key] = ExecuteSqlCount(SqlForConsolSendingAgent(orgAddress.PK, from, to));
						break;

					case OrganizationRole.ControllingCustomer:
						orgShippingCountDict[key] = ExecuteSqlCount(SqlForControllingCustomer(orgHeader.PK, from, to));
						break;

					case OrganizationRole.Shipper:
						orgShippingCountDict[key] = ExecuteSqlCount(SqlForShipper(orgHeader.PK, from, to));
						break;

					case OrganizationRole.Consignee:
						orgShippingCountDict[key] = ExecuteSqlCount(SqlForConsignee(orgHeader.PK, from, to));
						break;

					default:
						orgShippingCountDict[key] = -1;
						break;
				}
			}

			return orgShippingCountDict[key];
		}

		string SqlForConsolSendingAgent(ZGuid addressPk, ZDateTime from, ZDateTime to) =>
@$"
SELECT COUNT(DISTINCT JK_PK)
FROM JobConsol
INNER JOIN JobConsolTransport ON JW_ParentGUID = JK_PK
WHERE
	JK_OA_SendingForwarderAddress = '{addressPk}'
AND
	JK_TransportMode = 'AIR'
AND
	JW_ETD BETWEEN '{from}' AND '{to}'
AND
	JK_IsForwarding = 1 
AND
	JK_IsCancelled = 0
";

		string SqlForControllingCustomer(ZGuid orgPk, ZDateTime from, ZDateTime to) =>
@$"
SELECT COUNT(DISTINCT JS_PK)
FROM JobShipment
WHERE
	JS_E_DEP BETWEEN '{from}' AND '{to}'
AND
	JS_PK IN 
	(
		SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_AddressType = 'SCP' 
		AND
		E2_OA_Address IN 
		(
			SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH = '{orgPk}'
		)
	)
AND
	JS_TransportMode = 'AIR' 
AND
	JS_IsForwardRegistered = 1 
AND
	JS_IsCancelled = 0
";

		string SqlForShipper(ZGuid orgPk, ZDateTime from, ZDateTime to) =>
@$"
SELECT COUNT(DISTINCT JS_PK)
FROM JobShipment
WHERE
	JS_E_DEP BETWEEN '{from}' AND '{to}'
AND
	JS_PK IN 
	(
		SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_AddressType = 'CRD' 
		AND
		E2_OA_Address IN 
		(
			SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH = '{orgPk}'
		)
	)
AND
	JS_TransportMode = 'AIR' 
AND
	JS_IsForwardRegistered = 1 
AND
	JS_IsCancelled = 0";

		string SqlForConsignee(ZGuid orgPk, ZDateTime from, ZDateTime to) =>
@$"
SELECT COUNT(DISTINCT JS_PK)
FROM JobShipment
WHERE
	JS_E_DEP BETWEEN '{from}' AND '{to}'
AND
	JS_PK IN 
	(
		SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_AddressType = 'CED' 
		AND
		E2_OA_Address IN 
		(
			SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH = '{orgPk}'
		)
	)
AND
	JS_TransportMode = 'AIR' 
AND
	JS_IsForwardRegistered = 1 
AND
	JS_IsCancelled = 0";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Cannot use factory here due to performance consideration")]
		int ExecuteSqlCount(string sql)
		{
			using (var command = Db.Connection.Command(sql))
			{
				var count = command.ExecuteScalar();
				return count == DBNull.Value ? 0 : Convert.ToInt32(count);
			}
		}

		enum OrganizationRole
		{
			ConsolSendingAgent,
			ControllingCustomer,
			Shipper,
			Consignee,
			NotApplicable
		}
		#endregion

		#region IP Addresses
		public string GetCustomerAccountCreationIPAddress()
		{
			return GetAWBCreationIPAddress();
		}

		string ipAddressforACAS = string.Empty;

		public string GetAWBCreationIPAddress()
		{
#if DEBUG
			// Expected FWB/FHL message is hard coded in unit test so this is required
			ipAddressforACAS = "10.2.10.107";
#endif
			if (string.IsNullOrEmpty(ipAddressforACAS))
			{
				ipAddressforACAS = GetClientOrLocalHostIPAddress();
			}

			return ipAddressforACAS;
		}

		string GetClientOrLocalHostIPAddress()
		{
			var ipAddress = string.Empty;

			// Get client IP address via CargoWise Remote Desktop Services
			try
			{
				var productKey = ObjectFactory.Get<IProductRegistration>().Key;
				ipAddress = ObjectFactory.Get<IWiseCloudSecurityClient>().GetClientIPAddress(
					productKey.EnterpriseCode + productKey.ServerCode, GlbStaff.CurrentUser.GS_LoginName);
			}
			catch
			{
				// IWiseCloudSecurityClient.GetClientIPAddress will throw for a non-remote desktop case or failed login
			}

			if (!string.IsNullOrEmpty(ipAddress))
			{
				return ipAddress;
			}

			// Fall back to local host IP if failed to get RDP address
			try
			{
				foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
				{
					if (ip.AddressFamily == AddressFamily.InterNetwork)
					{
						return ip.ToString();
					}
				}
			}
			catch
			{
				// Network related code may throw unexpected exception while running in customer's environment
			}

			return string.Empty;
		}
		#endregion

		#region ACAS Biographic Data
		public (bool isNaturalPersonOrg, string idType, string idIssuer, string idNumber) GetBiographicData()
		{
			if (!ShouldApplyACAS())
			{
				return default;
			}

			if (awbHeader is ConsolExportAWBHeader consolHeader)
			{
				return GetFWBBiographicData(consolHeader);
			}

			if (awbHeader is ShipmentExportAWBHeader shipmentHeader)
			{
				return GetFHLBiographicData(shipmentHeader);
			}

			return default;
		}

		(bool isNaturalPersonOrg, string idType, string idIssuer, string idNumber) GetFWBBiographicData(ConsolExportAWBHeader consolHeader)
		{
			// For a Non-Direct Consol, the Biographic Data is not applicable for FWB
			if (consolHeader.Consol == null || !consolHeader.Consol.IsDirect)
			{
				return default;
			}

			var (orgRole, orgHeader, _) = ChooseOrganization(consolHeader.Consol);
			if (orgRole != OrganizationRole.NotApplicable && orgHeader != null)
			{
				return GetOrganizationBiographicData(orgHeader);
			}

			return default;
		}

		(bool isNaturalPersonOrg, string idType, string idIssuer, string idNumber) GetFHLBiographicData(ShipmentExportAWBHeader shipmentHeader)
		{
			if (shipmentHeader.Shipment == null || !shipmentHeader.Shipment.IsFHLShipment())
			{
				return default;
			}

			var (orgRole, orgHeader, _) = ChooseOrganization(shipmentHeader.Shipment, shouldCheckConsolHeaderPPD: false);
			if (orgRole != OrganizationRole.NotApplicable && orgHeader != null)
			{
				return GetOrganizationBiographicData(orgHeader);
			}

			return default;
		}

		(bool isNaturalPersonOrg, string idType, string idIssuer, string idNumber) GetOrganizationBiographicData(OrgHeader orgHeader)
		{
			// Biographic data is only applicable to Organisation records flagged as “Natural Person/Individual”
			if (orgHeader.OH_Category != OrgConstants.Category.NaturalPersonIndividual)
			{
				return default;
			}

			var passportNumber = orgHeader.CustomsCodes?.OfType<OrgCusCode>()
				.Where(x => x.OK_CodeType == OrgCusCode.CodeTypes.PassportID)
				.OrderBy(x => x.OK_SystemCreateTimeUtc)
				.FirstOrDefault();

			if (passportNumber != null && !string.IsNullOrEmpty(passportNumber.OK_RN_NKCodeCountry) && !string.IsNullOrEmpty(passportNumber.OK_CustomsRegNo))
			{
				return
				(
					isNaturalPersonOrg: true,
					idType: "PPT",
					idIssuer: passportNumber.OK_RN_NKCodeCountry,
					idNumber: passportNumber.OK_CustomsRegNo
				);
			}

			var driverLicense = orgHeader.CustomsCodes?.OfType<OrgCusCode>()
				.Where(x => x.OK_CodeType == OrgCusCode.CodeTypes.DriverLicenceID)
				.OrderBy(x => x.OK_SystemCreateTimeUtc)
				.FirstOrDefault();

			if (driverLicense != null && !string.IsNullOrEmpty(driverLicense.OK_RN_NKCodeCountry) && !string.IsNullOrEmpty(driverLicense.OK_CustomsRegNo))
			{
				return
				(
					isNaturalPersonOrg: true,
					idType: "DL",
					idIssuer: driverLicense.OK_RN_NKCodeCountry,
					idNumber: driverLicense.OK_CustomsRegNo
				);
			}

			return
			(
				isNaturalPersonOrg: true,
				idType: null,
				idIssuer: null,
				idNumber: null
			);
		}
		#endregion
	}
}
