using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgDataLoad : DataLoad
	{
		public OrgDataLoad()
		{
			OrganisationLinks = new ArrayList();
		}

		public void ImportOrganisationData(string dataLocation, bool generateNewCodes)
		{
			GenerateNewOrgCode = generateNewCodes;
			ImportData(dataLocation, (NoResString)"Organisation");
		}

		#region DataFields OrganisationData

		public class CsvOrg
		{
			public CsvOrg()
			{
				MainAddress = new AddressData();
				PostalAddress = new AddressData();
				DeliveryAddress = new AddressData();
				OptionalBusRegDetails = new Dictionary<ZString, BusRegDetail>();
			}

			public class AddressData
			{
				public ZString Address1;
				public ZString Address2;
				public ZString Address3;
				public ZString State;
				public ZString Postcode;
				public ZString Language;
			}

			public ZString OrgCode;
			public ZString OrgName;
			public readonly AddressData MainAddress;
			public readonly AddressData PostalAddress;
			public readonly AddressData DeliveryAddress;
			public ZString Phone;
			public ZString Fax;
			public ZString Email;
			public ZString Web;
			public ZString ContactTitle;
			public ZString ContactName;
			public ZString ContactEmail;
			public ZString ContactPhone;
			public ZString ContactMobile;
			public ZString ContactFax;
			public ZString ContactNotifyMode;
			public ZString ContactSourceType;
			public ZDateTime ContactDateDetailsVerified;
			public ZString ContactSalutation;
			public ZString UNLocode;
			public ZString LocodeCountry;
			public ZString LocodeCity;
			public ZString BusRegNo;
			public Dictionary<ZString, BusRegDetail> OptionalBusRegDetails;
			public ZString BusRegACN;
			public ZString CustomsCode;
			public ZString SupplierCode;
			public ZString CMRSupplierCode;
			public ZString CarrierCode;
			public ZString ManifestProviderCode;
			public bool IsDebtor;
			public bool IsCreditor;
			public bool IsConsignee;
			public bool IsConsignor;
			public bool IsControllingAgent;
			public bool IsControllingCustomer;
			public bool IsCarrier;
			public bool IsShippingLine;
			public bool IsAirLine;
			public bool IsLocalTransport;
			public bool IsForwarder;
			public bool IsBroker;
			public bool IsServices;
			public bool IsSalesLead;
			public bool IsCompetitor;
			public bool IsNewsletter;
			public bool IsWarehouse;
			public decimal CreditLimit;
			public ZString CreditRating;
			public ZString CurrencyCode;
			public ZString BankName;
			public ZString BankAccount;
			public ZString BankAccountNo;
			public ZString BankBSB;
			public bool GSTApplicaple;
			public ZString InvTermsStd;
			public ZByte InvDaysStd;
			public ZString InvTermsDisb;
			public ZByte InvDaysDisb;
			public ZString LegacyCode;
			public ZString DrsAccGroup;
			public ZString CrsAccGroup;
			public ZString PremiseID;

			public ZString WorkNotes;
			public ZString APNotes;
			public ZString ARNotes;
			public ZString ForwardingNotes;
			public ZString DeliveryNotes;
			public ZString InvoiceNotes;

			public ZString BankCurrency;
			public ZString Language;
		}

		public class CsvOrgDataError
		{
			public string ErrorMsg;
		}

		internal CsvOrgDataError DataError;

		#region ExcelOrgLinks

		readonly internal ArrayList OrganisationLinks;
		bool OrgLinksExist;

		public class OrgLinks
		{
			public ZString OrgLegacyCode;
			public ZString CustomsAgent;
			public ZString Debtor;
			public ZString ARSettlementGroup;
		}

		#endregion

		bool GenerateNewOrgCode;
		ZGuid DefaultCreditorGroup;
		ZGuid DefaultDebtorGroup;

		#endregion

		#region ImportFrom .csv file

		protected override void ProcessDataForThisLine(OCsvLine line)
		{
			CsvOrg orgData = ExtractOrganisationData(line);
			Guid transPK = Guid.Empty;

			if (orgData != null)
			{
				ValidateInvoiceTerms(orgData);
				transPK = ProcessExtractedData(orgData);
			}
			else
			{
				RunCounters.RecsExcluded++;
				DisplayLogMessage(Res.GetString("92e6044d-3254-4449-843b-8936667c7144", "Row {0} excluded: data is inconsistent with required format {1}", RunCounters.CurrentRow.ToString(), DataError.ErrorMsg));
			}

			UpdateAndDisplayIfRequired(transPK, OrgHeaderSchema.Constants.TableName);
		}

		protected virtual Guid ProcessExtractedData(CsvOrg orgData)
		{
			Guid transPK = Guid.Empty;
			OrgHeader enterpriseOrg = FindOrganisationIfItExists(orgData);
			if (enterpriseOrg == null)
			{
				try
				{
					OrgHeader newEnterpriseOrg = Factory.New<OrgHeader>();
					LoadImportedExcelValues(newEnterpriseOrg, orgData);
					RunCounters.RecsToUpdate++;
					RunCounters.RecsCreated++;
					transPK = newEnterpriseOrg.PK.ToGuid();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					RunCounters.RecsExcluded++;
					DisplayFormattedLogMessage(orgData.OrgCode, ex.Message);
				}
			}
			else
			{
				RunCounters.RecsExcluded++;
				DisplayLogMessage(Res.GetString("845fa376-d18c-4eb3-8944-e15872281f67", "Organization excluded: {0} / {1} - already exists in {2} table", orgData.OrgCode, orgData.OrgName, BrandingFactory.Instance.ProductName));
				Factory.ClearQueryCache();
			}

			return transPK;
		}

		protected override void RunSubsequentDataParsingIfRequired()
		{
			if (OrgLinksExist)
			{
				RunCounters.CurrentRow = 0;
				RunCounters.RecordsToImportPlusHeader = OrganisationLinks.Count;
				UpdateExcelOrgLinks();
			}
		}

		#region ExcelOrgLinks

		internal void UpdateExcelOrgLinks()
		{
			DisplayLogMessage(Res.GetString("f1aa8e32-6409-4ad0-aca8-cc7c1cb5bdfe", "Updating organization links:"));

			while (RunCounters.CurrentRow < OrganisationLinks.Count)
			{
				OrgLinks orgLinkDetails = (OrgLinks)OrganisationLinks[RunCounters.CurrentRow];
				ZGuid orgPK = GetOrgPKFromLegacyCode(orgLinkDetails.OrgLegacyCode, OrgCusCode.CodeTypes.LegacySystemCode);
				OrgHeader enterpriseOrg = (OrgHeader)Factory.Load(typeof(OrgHeader), orgPK);
				if (enterpriseOrg == null)
				{
					DisplayLogMessage(Res.GetString("2600ddeb-61a2-4e2b-b447-7f843fae5a06", "{0} / Organization not found on linking parse", orgLinkDetails.OrgLegacyCode));
				}
				else
				{
					if (!orgLinkDetails.CustomsAgent.IsEmpty)
					{
						UpdateAgentLinks(enterpriseOrg, FindLinkedOrganisation(orgLinkDetails.CustomsAgent, Res.GetString("80356457-61d6-4eac-8a50-fbd9fc427814", "Agent")));
					}

					if (!orgLinkDetails.Debtor.IsEmpty)
					{
						UpdateDebtorLinks(enterpriseOrg, FindLinkedOrganisation(orgLinkDetails.Debtor, Res.GetString("0d81aba0-1e0d-4e15-8a9e-5b051ea3acc9", "Debtor")));
					}

					if (!orgLinkDetails.ARSettlementGroup.IsEmpty)
					{
						UpdateSettlementGroupLinks(enterpriseOrg, FindLinkedOrganisation(orgLinkDetails.ARSettlementGroup, Res.GetString("2d14f471-2898-4039-a98b-cb3d16fbe6c2", "Settlement Group")));
					}
				}

				RunCounters.CurrentRow++;
				RunCounters.RecsToUpdate++;
				RunCounters.RecsCreated++;
				UpdateAndDisplayIfRequired(Guid.Empty, "");
			}

			DisplayLogMessage(Res.GetString("88b8c564-0964-4ad0-b76c-73bb271f8276", "Updating organization links completed:"));
			DisplayLinkedDataTotals();
		}

		#endregion

		#endregion

		#region LoadValues

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		protected virtual void LoadImportedExcelValues(OrgHeader enterpriseOrganisation, CsvOrg extractedData)
		{
			ZString orgname = extractedData.OrgName;
			enterpriseOrganisation.OH_FullName = Env.Registry.OrgAllowMixedCase ? orgname : orgname.ToUpper();
			enterpriseOrganisation.OH_RL_NKClosestPort = extractedData.UNLocode;
			enterpriseOrganisation.MainAddress.OA_Address1 = extractedData.MainAddress.Address1;
			enterpriseOrganisation.MainAddress.OA_Address2 = extractedData.MainAddress.Address2;
			enterpriseOrganisation.MainAddress.OA_City = extractedData.MainAddress.Address3;
			enterpriseOrganisation.MainAddress.OA_PostCode = extractedData.MainAddress.Postcode;
			enterpriseOrganisation.MainAddress.OA_State = extractedData.MainAddress.State;
			enterpriseOrganisation.MainAddress.OA_Phone = extractedData.Phone;
			enterpriseOrganisation.MainAddress.OA_Fax = extractedData.Fax;
			enterpriseOrganisation.MainAddress.OA_Email = extractedData.Email;
			if (!extractedData.MainAddress.Language.IsEmpty)
			{
				enterpriseOrganisation.MainAddress.OA_Language = extractedData.MainAddress.Language;
			}

			enterpriseOrganisation.MainWebURL.PU_URL = extractedData.Web;
			enterpriseOrganisation.CompanyData.OB_IsDebtor = extractedData.IsDebtor;

			if (!extractedData.Language.IsEmpty)
			{
				enterpriseOrganisation.OH_Language = extractedData.Language;
			}

			if (extractedData.IsDebtor)
			{
				if (!extractedData.DrsAccGroup.IsEmpty)
				{
					enterpriseOrganisation.MiscServ.OM_OJ_ARDebtorGroup = GetDebtorGroupPKFromCode(extractedData.DrsAccGroup);
				}
				else
				{
					enterpriseOrganisation.MiscServ.OM_OJ_ARDebtorGroup = DefaultDebtorGroup;
				}

				enterpriseOrganisation.CompanyData.SetARTaxApplicable(extractedData.GSTApplicaple);
				if (extractedData.CreditLimit > 0)
				{
					enterpriseOrganisation.MiscServ.OM_ARCreditLimit = extractedData.CreditLimit;
				}

				if (!extractedData.CurrencyCode.IsEmpty)
				{
					enterpriseOrganisation.CompanyData.OB_RX_NKARDDefltCurrency = extractedData.CurrencyCode;
				}

				if (!extractedData.BankName.IsEmpty)
				{
					enterpriseOrganisation.MiscServ.OM_ARPreviousChequeDrawerBank = extractedData.BankName;
					enterpriseOrganisation.MiscServ.OM_ARPreviousChequeDrawerBankBranch = extractedData.BankAccount;
				}

				if (!extractedData.CreditRating.IsEmpty)
				{
					enterpriseOrganisation.MiscServ.OM_ARCreditRating = extractedData.CreditRating;
				}

				enterpriseOrganisation.CompanyData.ARTerms[0].PY_InvoiceTerm = extractedData.InvTermsStd;
				enterpriseOrganisation.CompanyData.ARTerms[0].PY_InvoiceDays = extractedData.InvDaysStd;
				if (extractedData.InvTermsStd != extractedData.InvTermsDisb || extractedData.InvDaysStd != extractedData.InvDaysDisb)
				{
					OrgARTerms disbARTerms = enterpriseOrganisation.CompanyData.CreateOrLoadDisbursementARTerm();
					disbARTerms.PY_InvoiceTerm = extractedData.InvTermsDisb;
					disbARTerms.PY_InvoiceDays = extractedData.InvDaysDisb;
				}
			}

			enterpriseOrganisation.CompanyData.OB_IsCreditor = extractedData.IsCreditor;
			if (extractedData.IsCreditor)
			{
				if (!extractedData.CrsAccGroup.IsEmpty)
				{
					enterpriseOrganisation.MiscServ.OM_OG_APCreditorGroup = GetCreditorGroupPKFromCode(extractedData.CrsAccGroup);
				}
				else
				{
					enterpriseOrganisation.MiscServ.OM_OG_APCreditorGroup = DefaultCreditorGroup;
				}

				if (!extractedData.BankName.IsEmpty || !extractedData.BankAccount.IsEmpty || !extractedData.BankAccountNo.IsEmpty || !extractedData.BankBSB.IsEmpty)
				{
					AccAPAccountDetails accountDetails = enterpriseOrganisation.CompanyData.AccountDetailsCollection.AddNew();
					accountDetails.A1_BankName = extractedData.BankName;
					accountDetails.A1_AccountName = extractedData.BankAccount;
					accountDetails.A1_BankAccount = extractedData.BankAccountNo;
					accountDetails.A1_BankBsb = extractedData.BankBSB;
					accountDetails.A1_RX_NKAccountCurrency = extractedData.BankCurrency;
				}

				enterpriseOrganisation.CompanyData.SetAPTaxApplicable(extractedData.GSTApplicaple);
				if (extractedData.CreditLimit > 0)
				{
					enterpriseOrganisation.MiscServ.OM_APCreditLimit = extractedData.CreditLimit;
				}

				if (!extractedData.CurrencyCode.IsEmpty)
				{
					enterpriseOrganisation.CompanyData.OB_RX_NKAPDefltCurrency = extractedData.CurrencyCode;
				}
			}

			enterpriseOrganisation.CompanyData.OB_GB_ControllingBranch = ZGuid.Empty;
			enterpriseOrganisation.OH_Code = GetCompanyCode(enterpriseOrganisation, extractedData.OrgCode);
			enterpriseOrganisation.OH_IsConsignee = extractedData.IsConsignee;
			enterpriseOrganisation.OH_IsConsignor = extractedData.IsConsignor;
			enterpriseOrganisation.OH_IsControllingAgent = extractedData.IsControllingAgent;
			enterpriseOrganisation.OH_IsControllingCustomer = extractedData.IsControllingCustomer;
			enterpriseOrganisation.OH_IsShippingProvider = extractedData.IsCarrier;
			enterpriseOrganisation.OH_IsShippingLine = extractedData.IsShippingLine;
			enterpriseOrganisation.OH_IsAirLine = extractedData.IsAirLine;
			enterpriseOrganisation.OH_IsLocalTransport = extractedData.IsLocalTransport;
			enterpriseOrganisation.OH_IsForwarder = extractedData.IsForwarder;
			if (extractedData.IsForwarder)
			{
				if (!extractedData.CurrencyCode.IsEmpty)
				{
					enterpriseOrganisation.MiscServ.OM_RX_NKFWDefCurrency = extractedData.CurrencyCode;
				}
			}

			enterpriseOrganisation.OH_IsBroker = extractedData.IsBroker;
			enterpriseOrganisation.OH_IsMiscFreightServices = extractedData.IsServices;
			enterpriseOrganisation.OH_IsCompetitor = extractedData.IsCompetitor;
			enterpriseOrganisation.OH_IsSalesLead = extractedData.IsSalesLead;
			enterpriseOrganisation.OH_IsWarehouseClient = extractedData.IsWarehouse;

			LoadAddress(enterpriseOrganisation, "PST", extractedData.PostalAddress);
			LoadAddress(enterpriseOrganisation, "DLV", extractedData.DeliveryAddress);

			LoadBusinessRegNo(enterpriseOrganisation, extractedData.BusRegNo);
			LoadOptionalBusinessRegNos(enterpriseOrganisation, extractedData.OptionalBusRegDetails);

			LoadCorpCode(enterpriseOrganisation, extractedData.BusRegACN);
			LoadCustomsCode(enterpriseOrganisation, OrgCusCode.CodeTypes.CustomsClientCode, extractedData.CustomsCode);
			LoadCustomsCode(enterpriseOrganisation, OrgCusCode.CodeTypes.SupplierCode, extractedData.SupplierCode);
			LoadCustomsCode(enterpriseOrganisation, OrgCusCode.CodeTypes.CustomsClientID, extractedData.CMRSupplierCode);
			LoadCustomsCode(enterpriseOrganisation, OrgCusCode.CodeTypes.CarrierCode, extractedData.CarrierCode);
			LoadCustomsCode(enterpriseOrganisation, OrgCusCode.CodeTypes.ManifestProviderID, extractedData.ManifestProviderCode);
			LoadCustomsCode(enterpriseOrganisation, OrgCusCode.CodeTypes.ControlledPremisesID, extractedData.PremiseID);
			LoadCustomsCode(enterpriseOrganisation, OrgCusCode.CodeTypes.LegacySystemCode, extractedData.LegacyCode);

			if (!extractedData.ContactName.Trim().IsEmpty)
			{
				LoadExcelContact(enterpriseOrganisation, extractedData);
			}

			LoadNotes(enterpriseOrganisation, extractedData.WorkNotes, (NoResString)"Internal Work Notes");
			LoadNotes(enterpriseOrganisation, extractedData.ForwardingNotes, "Goods Handling Instructions");
			LoadNotes(enterpriseOrganisation, extractedData.DeliveryNotes, "Import Delivery Instructions");
			LoadNotes(enterpriseOrganisation, extractedData.InvoiceNotes, "A/R Credit Management Note");
			LoadNotes(enterpriseOrganisation, extractedData.APNotes, "A/P Account Management Notes");
			LoadNotes(enterpriseOrganisation, extractedData.ARNotes, "A/R Account Management Notes");
		}

		void LoadOptionalBusinessRegNos(OrgHeader org, Dictionary<ZString, BusRegDetail> optionalBusRegDetails)
		{
			foreach (var pair in optionalBusRegDetails)
			{
				var busRegDetail = pair.Value;
				var cusCodes = FindRegistrationDetailFor(org, busRegDetail);
				if (cusCodes.Length == 0)
				{
					cusCodes = org.CustomsCodes.GetOrgCusCodesForCodeAndCountry(busRegDetail.CodeType, busRegDetail.CountryOfIssue);
					OrgCusCode cusCode = cusCodes.Length == 1 ? (OrgCusCode)cusCodes[0] : null;
					if (cusCode == null)
					{
						cusCode = org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(busRegDetail.CodeType, busRegDetail.RegistrationNumber, busRegDetail.CountryOfIssue);
						cusCode.OK_RN_NKCodeCountry = busRegDetail.CountryOfIssue;
					}
					else
					{
						cusCode.OK_CustomsRegNo = busRegDetail.RegistrationNumber;
					}
					if (cusCode.PremisesAddressIsAllowed)
					{
						cusCode.OK_OA_PremisesAddress = org.MainAddress.PK;
					}
				}
			}
		}

		BusinessObject[] FindRegistrationDetailFor(OrgHeader org, BusRegDetail busRegDetail)
		{
			ZQuery query = new ZQuery(OrgCusCodeSchema.OK_CodeType, busRegDetail.CodeType);
			query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, busRegDetail.CountryOfIssue);
			query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, busRegDetail.RegistrationNumber);
			return org.CustomsCodes.Find(query);
		}

		void LoadCorpCode(OrgHeader enterpriseOrganisation, ZString code)
		{
			string corpCodeType = DetermineCorpCodeType(code);
			if (!string.IsNullOrEmpty(corpCodeType))
			{
				LoadCustomsCode(enterpriseOrganisation, corpCodeType, code);
			}
		}

		ZString DetermineCorpCodeType(ZString code)
		{
			string corpCodeType = OrgCusCode.CodeTypes.CorporationCode;
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedKingdom)
			{
				corpCodeType = ZString.Empty; // Contact DJC or A.G if you have an incident related to UK corp code being empty
			}
			return corpCodeType;
		}

		protected void LoadExcelContact(OrgHeader enterpriseOrganisation, CsvOrg extractedData)
		{
			ZQuery contactFilter = new ZQuery(OrgContactSchema.OC_ContactName, extractedData.ContactName);
			contactFilter.AddToFilter(OrgContactSchema.OC_OH, enterpriseOrganisation.PK);
			OrgContact organisationContact = (OrgContact)Factory.LoadTop1(typeof(OrgContact), contactFilter);
			if (organisationContact == null)
			{
				OrgContact newEnterpriseOrganisationContact = enterpriseOrganisation.Contacts.AddNew();
				newEnterpriseOrganisationContact.OC_ContactName = extractedData.ContactName;
				newEnterpriseOrganisationContact.OC_NotifyMode = extractedData.ContactNotifyMode;
				newEnterpriseOrganisationContact.OC_Title = extractedData.ContactTitle;
				newEnterpriseOrganisationContact.OC_Email = extractedData.ContactEmail;
				newEnterpriseOrganisationContact.OC_Phone = extractedData.ContactPhone;
				newEnterpriseOrganisationContact.OC_Fax = extractedData.ContactFax;
				newEnterpriseOrganisationContact.OC_Mobile = extractedData.ContactMobile;
				newEnterpriseOrganisationContact.OC_ContactSource = extractedData.ContactSourceType;
				newEnterpriseOrganisationContact.OC_DetailsVerified = extractedData.ContactDateDetailsVerified;
				newEnterpriseOrganisationContact.OC_Salutation = extractedData.ContactSalutation;
			}
		}

		protected void LoadAddress(OrgHeader enterpriseOrganisation, string addressTypeCode, CsvOrg.AddressData address)
		{
			if (!address.Address1.IsEmpty)
			{
				ZString uniqueKey = addressTypeCode + ": " + address.Address1;
				uniqueKey = uniqueKey.SubstringSafe(0, 25);
				ZQuery addressFilter = new ZQuery(OrgAddressSchema.OA_Code, uniqueKey);
				addressFilter.AddToFilter(OrgAddressSchema.OA_OH, enterpriseOrganisation.PK);
				OrgAddress thisAddress = (OrgAddress)Factory.LoadTop1(typeof(OrgAddress), addressFilter);
				if (thisAddress == null)
				{
					OrgAddress newEnterpriseOrgAddress = enterpriseOrganisation.Addresses.AddNew();
					newEnterpriseOrgAddress.OA_Address1 = address.Address1;
					newEnterpriseOrgAddress.OA_Address2 = address.Address2;
					newEnterpriseOrgAddress.OA_City = address.Address3;
					newEnterpriseOrgAddress.OA_State = address.State;
					newEnterpriseOrgAddress.OA_PostCode = address.Postcode;
					newEnterpriseOrgAddress.AddressCapability.SetCapabilityEnabled(addressTypeCode);
					newEnterpriseOrgAddress.AddressCapability.SetIsNotMainAddress(addressTypeCode);
					newEnterpriseOrgAddress.OA_Code = uniqueKey;

					if (!address.Language.IsEmpty)
					{
						newEnterpriseOrgAddress.OA_Language = address.Language;
					}
				}
			}
		}

		protected void LoadCustomsCode(OrgHeader enterpriseOrganisation, string codeType, ZString codeValue)
		{
			if (!codeValue.IsEmpty)
			{
				ZQuery cusCodeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, codeType);
				cusCodeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				cusCodeFilter.AddToFilter(OrgCusCodeSchema.OK_OH, enterpriseOrganisation.PK);
				OrgCusCode customsCodeNumber = (OrgCusCode)Factory.LoadTop1(typeof(OrgCusCode), cusCodeFilter);
				if (customsCodeNumber == null)
				{
					OrgCusCode newEnterpriseOrgCustomsCodeNumber = enterpriseOrganisation.CustomsCodes.AddNew();
					newEnterpriseOrgCustomsCodeNumber.OK_CodeType = codeType;
					newEnterpriseOrgCustomsCodeNumber.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					newEnterpriseOrgCustomsCodeNumber.OK_CustomsRegNo = codeValue.SubstringSafe(0, 35);
				}
			}
		}

		protected void LoadBusinessRegNo(OrgHeader enterpriseOrganisation, ZString busRego)
		{
			if (!busRego.IsEmpty)
			{
				string codeType = DetermineRegstrationCodeType(busRego);
				LoadCustomsCode(enterpriseOrganisation, codeType, busRego);
			}
		}

		protected string DetermineRegstrationCodeType(ZString regoValue)
		{
			string registrationCodeType = OrgCusCode.CodeTypes.GovBusinessCode;
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates)
			{
				registrationCodeType = DetermineAppropriateUSIdentifier(regoValue);
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia)
			{
				if (regoValue.Length == 9)
				{
					registrationCodeType = OrgCusCode.CodeTypes.CorporationCode;
				}
				else
				{
					registrationCodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
				}
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.SouthAfrica
					|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedKingdom
					|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Sweden
					|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Azerbaijan
					|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Kenya
					|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Mauritius
					|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Mongolia
					|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Botswana
					|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Kazakhstan
					|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Zimbabwe
					|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Lebanon
					|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Nepal
					|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Iran)
			{
				registrationCodeType = OrgCusCode.CodeTypes.VATCode;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Singapore)
			{
				registrationCodeType = OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Italy)
			{
				registrationCodeType = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.SriLanka)
			{
				registrationCodeType = OrgCusCode.SriLankaCodeTypes.SVATBusinessRegistrationNumber;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Ethiopia)
			{
				registrationCodeType = OrgCusCode.EthiopiaCodeTypes.TaxIdentificationNumber;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Uganda)
			{
				registrationCodeType = UgandaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Zambia)
			{
				registrationCodeType = OrgCusCode.ZambiaCodeTypes.TaxIdentificationNumber;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Nigeria)
			{
				registrationCodeType = OrgCusCode.NigeriaCodeTypes.TaxIdentificationNumber;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Guatemala)
			{
				registrationCodeType = OrgCusCode.GuatemalaCodeTypes.NumeroDeIdentificacionTributaria;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Venezuela)
			{
				registrationCodeType = OrgCusCode.VenezuelaCodeTypes.RegistroUnicoDeInformacionFiscal;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Latvia)
			{
				registrationCodeType = OrgCusCode.LatviaCodeTypes.PVN;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Lithuania)
			{
				registrationCodeType = OrgCusCode.LithuaniaCodeTypes.PVM;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Slovenia)
			{
				registrationCodeType = OrgCusCode.SloveniaCodeTypes.DDV;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Ecuador)
			{
				registrationCodeType = OrgCusCode.EcuadorCodeTypes.RUC;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.ElSalvador)
			{
				registrationCodeType = ElSalvadorOrgCusCodeInfo.OrgCusCodes.NRC;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.PuertoRico)
			{
				registrationCodeType = OrgCusCode.PuertoRicoCodeTypes.NRC;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Paraguay)
			{
				registrationCodeType = OrgCusCode.ParaguayCodeTypes.RUC;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Uruguay)
			{
				registrationCodeType = UruguayOrgCusCodeInfo.OrgCusCodes.RUT;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Bolivia)
			{
				registrationCodeType = OrgCusCode.BoliviaCodeTypes.NIT;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.FrenchPolynesia)
			{
				registrationCodeType = OrgCusCode.FrenchPolynesiaCodeTypes.TAH;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Honduras)
			{
				registrationCodeType = OrgCusCode.HondurasCodeTypes.RTN;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Nicaragua)
			{
				registrationCodeType = OrgCusCode.NicaraguaCodeTypes.RUC;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Portugal)
			{
				registrationCodeType = OrgCusCode.CodeTypes.IVA;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Slovakia)
			{
				registrationCodeType = OrgCusCode.SlovakiaCodeTypes.DPH;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Tanzania)
			{
				registrationCodeType = OrgCusCode.TanzaniaCodeTypes.VRN;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Mali
				|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.EquatorialGuinea)
			{
				registrationCodeType = OrgCusCode.MaliCodeTypes.NIF;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Senegal)
			{
				registrationCodeType = OrgCusCode.SenegalCodeTypes.NIN;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.CoteDivoire)
			{
				registrationCodeType = OrgCusCode.CoteDivoireCodeTypes.NCC;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Cameroon)
			{
				registrationCodeType = OrgCusCode.CameroonCodeTypes.NIU;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Mozambique)
			{
				registrationCodeType = OrgCusCode.MozambiqueCodeTypes.NUI;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.DominicanRepublic)
			{
				registrationCodeType = DominicanRepublicOrgCusCodeInfo.OrgCusCodes.RNC;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Yemen)
			{
				registrationCodeType = OrgCusCode.CodeTypes.GSTCode;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Somalia)
			{
				registrationCodeType = OrgCusCode.CodeTypes.CorporationCode;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Panama)
			{
				registrationCodeType = PanamaOrgCusCodeInfo.OrgCusCodes.RUC;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Algeria)
			{
				registrationCodeType = AlgeriaOrgCusCodeInfo.OrgCusCodes.NIF;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Niger)
			{
				registrationCodeType = OrgCusCode.NigerCodeTypes.NIF;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Malawi)
			{
				registrationCodeType = OrgCusCode.MalawiCodeTypes.TIN;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Kiribati)
			{
				registrationCodeType = OrgCusCode.KiribatiCodeTypes.TaxIdentificationNumber;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.BurkinaFaso)
			{
				registrationCodeType = BurkinaFasoOrgCusCodeInfo.OrgCusCodes.IFU;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.NewCaledonia)
			{
				registrationCodeType = OrgCusCode.NewCaledoniaCodeTypes.TGC;
			}

			return registrationCodeType;
		}

		#region US Organisation Identifier

		ZString DetermineAppropriateUSIdentifier(ZString identifierCode)
		{
			ZString uSid = ZString.Empty;

			if (IsValidEIN(identifierCode))
			{
				uSid = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			}
			else if (IsValidSSN(identifierCode))
			{
				uSid = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			}
			else if (IsValidCBPAssignedNumber(identifierCode))
			{
				uSid = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			}

			return uSid;
		}

		public static bool IsValidEIN(ZString eINNumber)
		{
			return Regex.IsMatch(eINNumber, @"^[0-9]{2}-[0-9]{7}[A-Z0-9]{2}$", RegexOptions.IgnoreCase);
		}

		public static bool IsValidSSN(ZString number)
		{
			return Regex.IsMatch(number, @"^[0-9]{3}-[0-9]{2}-[0-9]{4}$", RegexOptions.IgnoreCase);
		}

		public static bool IsValidCBPAssignedNumber(ZString cBPAssignedNumber)
		{
			return Regex.IsMatch(cBPAssignedNumber, @"^[0-9]{2}[A-Z0-9]{4}-[0-9]{5}$", RegexOptions.IgnoreCase);
		}

		#endregion

		protected void LoadNotes(OrgHeader enterpriseOrganisation, ZString notes, string noteType)
		{
			if (!notes.IsEmpty)
			{
				ZQuery noteFilter = new ZQuery(StmNoteSchema.ST_ParentID, enterpriseOrganisation.PK);
				noteFilter.AddToFilter(StmNoteSchema.ST_Table, "OrgHeader");
				noteFilter.AddToFilter(StmNoteSchema.ST_Description, noteType);
				StmNote orgNote = (StmNote)Factory.LoadTop1(typeof(StmNote), noteFilter);
				if (orgNote == null)
				{
					StmNote newOrgNote = enterpriseOrganisation.Notes.AddNew();
					newOrgNote.ST_Description = noteType;
					newOrgNote.ST_Table = "OrgHeader";
					newOrgNote.ST_NoteDataAsText = notes;
				}
			}
		}

		#region Add Organisation Links

		protected void UpdateAgentLinks(OrgHeader enterpriseOrganisation, OrgHeader enterpriseAgent)
		{
			if (enterpriseAgent != null)
			{
				enterpriseOrganisation.SetRelatedParty(enterpriseAgent, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty);
				enterpriseOrganisation.SetRelatedParty(enterpriseAgent, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);
				enterpriseOrganisation.SetRelatedParty(enterpriseAgent, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty);
				enterpriseOrganisation.SetRelatedParty(enterpriseAgent, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty);
			}
		}

		protected void UpdateDebtorLinks(OrgHeader enterpriseOrganisation, OrgHeader enterpriseDebtor)
		{
			if (enterpriseDebtor != null)
			{
				enterpriseOrganisation.SetRelatedParty(enterpriseDebtor, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty);
				enterpriseOrganisation.SetRelatedParty(enterpriseDebtor, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty);
				enterpriseOrganisation.SetRelatedParty(enterpriseDebtor, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);
				enterpriseOrganisation.SetRelatedParty(enterpriseDebtor, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);
			}
		}

		protected void UpdateSettlementGroupLinks(OrgHeader enterpriseOrganisation, OrgHeader enterpriseSettlementGroup)
		{
			if (enterpriseSettlementGroup != null)
			{
				enterpriseOrganisation.ARSettlementGroupPK = enterpriseSettlementGroup.PK;
			}
		}

		#endregion

		#endregion

		#region ExtractData

		internal CsvOrg ExtractOrganisationData(OCsvLine line)
		{
			try
			{
				return ExtractCSVOrgData(line);
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{ throw; }
				return null;
			}
		}

		protected virtual CsvOrg GetNewCsvOrg()
		{
			return new CsvOrg();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Data Importer Field Ref")]
		protected virtual CsvOrg ExtractCSVOrgData(OCsvLine line)
		{
			int totalFieldValuesCount = line.FieldValues.Length;
			CsvOrg orgData = GetNewCsvOrg();
			DataError = new CsvOrgDataError();
			orgData.OrgCode = orgData.LegacyCode = TrimmedZString(line.FieldValues[0]);
			orgData.OrgName = TrimmedZString(line.FieldValues[1], OrgHeaderSchema.OH_FullName.MaxLength);

			orgData.MainAddress.Address1 = TrimmedZString(line.FieldValues[2], OrgAddressSchema.OA_Address1.MaxLength);
			if (orgData.MainAddress.Address1.IsEmpty)
			{
				DataError.ErrorMsg = Res.GetString("1ca69979-1985-493e-8230-999f618aed50", "- cannot import an Organization with blank address data");
				return null;
			}
			else if (orgData.MainAddress.Address1.Length < OrgAddress.Schema.OA_Address1MinimumLength)
			{
				DataError.ErrorMsg = Res.GetString("9bc079f2-2860-401f-af40-15eee66861a1", "- cannot import an Organization with address less than {0} characters", OrgAddress.Schema.OA_Address1MinimumLength);
				return null;
			}

			if (totalFieldValuesCount > 3)
			{
				orgData.MainAddress.Address2 = TrimmedZString(line.FieldValues[3], OrgAddressSchema.OA_Address2.MaxLength);
			}

			if (totalFieldValuesCount > 4)
			{
				orgData.MainAddress.Address3 = TrimmedZString(line.FieldValues[4], OrgAddressSchema.OA_City.MaxLength);
			}

			if (totalFieldValuesCount > 5)
			{
				orgData.MainAddress.State = TrimmedZString(line.FieldValues[5], OrgAddressSchema.OA_State.MaxLength);
			}

			if (totalFieldValuesCount > 6)
			{
				orgData.MainAddress.Postcode = TrimmedZString(line.FieldValues[6], OrgAddressSchema.OA_PostCode.MaxLength);
			}

			if (totalFieldValuesCount > 7)
			{
				orgData.UNLocode = TrimmedZString(line.FieldValues[7], OrgHeaderSchema.OH_RL_NKClosestPort.MaxLength);
			}

			if (totalFieldValuesCount > 8)
			{
				orgData.LocodeCountry = TrimmedZString(line.FieldValues[8], 35);
			}

			if (totalFieldValuesCount > 9)
			{
				orgData.LocodeCity = TrimmedZString(line.FieldValues[9], 35);
			}

			if (orgData.UNLocode.IsEmpty)
			{
				orgData.UNLocode = RefUNLOCO.LookupPortCodeFromCountryCityOrAUState(Factory, orgData.LocodeCountry, orgData.LocodeCity, orgData.MainAddress.State);
			}

			if (totalFieldValuesCount > 10)
			{
				orgData.Phone = TrimmedZString(line.FieldValues[10], OrgAddressSchema.OA_Phone.MaxLength);
			}

			if (totalFieldValuesCount > 11)
			{
				orgData.Fax = TrimmedZString(line.FieldValues[11], OrgAddressSchema.OA_Fax.MaxLength);
			}

			if (totalFieldValuesCount > 12)
			{
				orgData.Email = TrimmedZString(line.FieldValues[12], OrgAddressSchema.OA_Email.MaxLength);
			}

			if (totalFieldValuesCount > 13)
			{
				orgData.Web = TrimmedZString(line.FieldValues[13], OrgWebURLSchema.PU_URL.MaxLength);
			}

			if (totalFieldValuesCount > 14)
			{
				orgData.BusRegNo = TrimmedZString(line.FieldValues[14]).Trim().Replace(" ", "");
			}

			if (totalFieldValuesCount > 15)
			{
				orgData.BusRegACN = TrimmedZString(line.FieldValues[15]).Trim().Replace(" ", "");
			}

			if (totalFieldValuesCount > 16)
			{
				orgData.IsDebtor = IsYesChar(line.FieldValues[16]);
			}

			if (totalFieldValuesCount > 17)
			{
				orgData.IsCreditor = IsYesChar(line.FieldValues[17]);
			}

			if (totalFieldValuesCount > 18)
			{
				orgData.IsConsignee = IsYesChar(line.FieldValues[18]);
			}

			if (totalFieldValuesCount > 19)
			{
				orgData.IsConsignor = IsYesChar(line.FieldValues[19]);
			}

			if (totalFieldValuesCount > 20)
			{
				orgData.IsForwarder = IsYesChar(line.FieldValues[20]);
			}

			if (totalFieldValuesCount > 21)
			{
				orgData.IsBroker = IsYesChar(line.FieldValues[21]);
			}

			if (totalFieldValuesCount > 22)
			{
				orgData.IsCarrier = IsYesChar(line.FieldValues[22]);
			}

			if (totalFieldValuesCount > 23)
			{
				orgData.IsShippingLine = IsYesChar(line.FieldValues[23]);
			}

			if (totalFieldValuesCount > 24)
			{
				orgData.IsAirLine = IsYesChar(line.FieldValues[24]);
			}

			if (totalFieldValuesCount > 25)
			{
				orgData.IsLocalTransport = IsYesChar(line.FieldValues[25]);
			}

			if (totalFieldValuesCount > 26)
			{
				orgData.IsSalesLead = IsYesChar(line.FieldValues[26]);
			}

			if (totalFieldValuesCount > 27)
			{
				orgData.IsServices = IsYesChar(line.FieldValues[27]);
			}

			if (totalFieldValuesCount > 28)
			{
				orgData.IsCompetitor = IsYesChar(line.FieldValues[28]);
			}

			if (totalFieldValuesCount > 29)
			{
				orgData.ContactName = TrimmedZString(line.FieldValues[29], OrgContactSchema.OC_ContactName.MaxLength);
			}

			if (totalFieldValuesCount > 30)
			{
				orgData.ContactTitle = TrimmedZString(line.FieldValues[30], OrgContactSchema.OC_Title.MaxLength);
			}

			if (totalFieldValuesCount > 31)
			{
				orgData.ContactEmail = TrimmedZString(line.FieldValues[31], OrgContactSchema.OC_Email.MaxLength);
			}

			if (orgData.ContactEmail.IsEmpty)
			{
				orgData.ContactNotifyMode = "PRN";
			}
			else
			{
				orgData.ContactNotifyMode = "EML";
			}

			if (totalFieldValuesCount > 32)
			{
				orgData.ContactPhone = TrimmedZString(line.FieldValues[32], OrgContactSchema.OC_Phone.MaxLength);
			}

			if (totalFieldValuesCount > 33)
			{
				orgData.ContactMobile = TrimmedZString(line.FieldValues[33], OrgContactSchema.OC_Mobile.MaxLength);
			}

			if (totalFieldValuesCount > 34)
			{
				orgData.ContactFax = TrimmedZString(line.FieldValues[34], OrgContactSchema.OC_Fax.MaxLength);
			}

			ZString debtor = "";
			if (totalFieldValuesCount > 35)
			{
				debtor = TrimmedZString(line.FieldValues[35]);
			}

			if (totalFieldValuesCount > 36)
			{
				orgData.DrsAccGroup = TrimmedZString(line.FieldValues[36], 3);
			}

			ZString aRSettlementGroup = "";
			if (totalFieldValuesCount > 37)
			{
				aRSettlementGroup = TrimmedZString(line.FieldValues[37]);
			}

			if (totalFieldValuesCount > 38)
			{
				orgData.CurrencyCode = TrimmedZString(line.FieldValues[38], 3);
			}

			if (totalFieldValuesCount > 39)
			{
				if (!string.IsNullOrEmpty(line.FieldValues[39].Trim()) && line.FieldValues[39].Trim() != "0")
				{
					DataError.ErrorMsg = "- Field No. 40, CreditLimit";
					string cLValue = TrimmedZString(line.FieldValues[39], 10);
					orgData.CreditLimit = Convert.ToDecimal(cLValue);
				}
			}

			if (totalFieldValuesCount > 40)
			{
				orgData.CreditRating = TrimmedZString(line.FieldValues[40], 3);
			}

			if (totalFieldValuesCount > 41)
			{
				orgData.GSTApplicaple = IsYesChar(line.FieldValues[41]);
			}

			if (totalFieldValuesCount > 42)
			{
				orgData.InvTermsStd = TrimmedZString(line.FieldValues[42]);
			}

			if (totalFieldValuesCount > 43)
			{
				if (!string.IsNullOrEmpty(line.FieldValues[43].Trim()) && line.FieldValues[43].Trim() != "0")
				{
					DataError.ErrorMsg = "- Field No. 44, Inv_Days_Standard";
					string invDaysStdValue = TrimmedZString(line.FieldValues[43], 3);
					orgData.InvDaysStd = Convert.ToByte(invDaysStdValue);
				}
			}

			if (totalFieldValuesCount > 44)
			{
				orgData.InvTermsDisb = TrimmedZString(line.FieldValues[44]);
			}

			if (totalFieldValuesCount > 45)
			{
				if (!string.IsNullOrEmpty(line.FieldValues[45].Trim()) && line.FieldValues[45].Trim() != "0")
				{
					DataError.ErrorMsg = "- Field No. 46, Inv_Days_Disbursement";
					string invDaysDisbValue = TrimmedZString(line.FieldValues[45], 3);
					orgData.InvDaysDisb = Convert.ToByte(invDaysDisbValue);
				}
			}

			if (totalFieldValuesCount > 46)
			{
				orgData.CrsAccGroup = TrimmedZString(line.FieldValues[46], 3);
			}

			ZString customsAgent = "";
			if (totalFieldValuesCount > 47)
			{
				customsAgent = TrimmedZString(line.FieldValues[47]).Trim();
			}

			if (totalFieldValuesCount > 48)
			{
				orgData.PostalAddress.Address1 = TrimmedZString(line.FieldValues[48], OrgAddressSchema.OA_Address1.MaxLength);
			}

			if (totalFieldValuesCount > 49)
			{
				orgData.PostalAddress.Address2 = TrimmedZString(line.FieldValues[49], OrgAddressSchema.OA_Address2.MaxLength);
			}

			if (totalFieldValuesCount > 50)
			{
				orgData.PostalAddress.Address3 = TrimmedZString(line.FieldValues[50], OrgAddressSchema.OA_City.MaxLength);
			}

			if (totalFieldValuesCount > 51)
			{
				orgData.PostalAddress.State = TrimmedZString(line.FieldValues[51], OrgAddressSchema.OA_State.MaxLength);
			}

			if (totalFieldValuesCount > 52)
			{
				orgData.PostalAddress.Postcode = TrimmedZString(line.FieldValues[52], OrgAddressSchema.OA_PostCode.MaxLength);
			}

			if (totalFieldValuesCount > 53)
			{
				orgData.DeliveryAddress.Address1 = TrimmedZString(line.FieldValues[53], OrgAddressSchema.OA_Address1.MaxLength);
			}

			if (totalFieldValuesCount > 54)
			{
				orgData.DeliveryAddress.Address2 = TrimmedZString(line.FieldValues[54], OrgAddressSchema.OA_Address2.MaxLength);
			}

			if (totalFieldValuesCount > 55)
			{
				orgData.DeliveryAddress.Address3 = TrimmedZString(line.FieldValues[55], OrgAddressSchema.OA_City.MaxLength);
			}

			if (totalFieldValuesCount > 56)
			{
				orgData.DeliveryAddress.State = TrimmedZString(line.FieldValues[56], OrgAddressSchema.OA_State.MaxLength);
			}

			if (totalFieldValuesCount > 57)
			{
				orgData.DeliveryAddress.Postcode = TrimmedZString(line.FieldValues[57], OrgAddressSchema.OA_PostCode.MaxLength);
			}

			if (totalFieldValuesCount > 58)
			{
				orgData.BankName = TrimmedZString(line.FieldValues[58], 25);
			}

			if (totalFieldValuesCount > 59)
			{
				orgData.BankAccount = TrimmedZString(line.FieldValues[59], 25);
			}

			if (totalFieldValuesCount > 60)
			{
				orgData.BankAccountNo = TrimmedZString(line.FieldValues[60], 35);
			}

			if (totalFieldValuesCount > 61)
			{
				orgData.BankBSB = TrimmedZString(line.FieldValues[61], 15);
			}

			if (totalFieldValuesCount > 62)
			{
				orgData.CustomsCode = TrimmedZString(line.FieldValues[62], 10);
			}

			if (totalFieldValuesCount > 63)
			{
				orgData.SupplierCode = TrimmedZString(line.FieldValues[63], 35);
			}

			if (totalFieldValuesCount > 64)
			{
				orgData.CMRSupplierCode = TrimmedZString(line.FieldValues[64], 35);
			}

			if (totalFieldValuesCount > 65)
			{
				orgData.PremiseID = TrimmedZString(line.FieldValues[65], 35);
			}

			if (totalFieldValuesCount > 66)
			{
				orgData.CarrierCode = TrimmedZString(line.FieldValues[66], 35);
			}

			if (totalFieldValuesCount > 67)
			{
				orgData.ManifestProviderCode = TrimmedZString(line.FieldValues[67], 35);
			}

			if (totalFieldValuesCount > 68)
			{
				orgData.WorkNotes = TrimmedZString(line.FieldValues[68]);
			}

			if (totalFieldValuesCount > 69)
			{
				orgData.ForwardingNotes = TrimmedZString(line.FieldValues[69]);
			}

			if (totalFieldValuesCount > 70)
			{
				orgData.DeliveryNotes = TrimmedZString(line.FieldValues[70]);
			}

			if (totalFieldValuesCount > 71)
			{
				orgData.ARNotes = TrimmedZString(line.FieldValues[71]);
			}

			if (totalFieldValuesCount > 72)
			{
				orgData.InvoiceNotes = TrimmedZString(line.FieldValues[72]);
			}

			if (totalFieldValuesCount > 73)
			{
				orgData.APNotes = TrimmedZString(line.FieldValues[73]);
			}

			if (totalFieldValuesCount > 74)
			{
				orgData.ContactSourceType = TrimmedZString(line.FieldValues[74], 20);
			}

			if (totalFieldValuesCount > 75)
			{
				orgData.ContactDateDetailsVerified = ParsedZDateTime(line.FieldValues[75]);
			}

			if (totalFieldValuesCount > 76)
			{
				orgData.ContactSalutation = TrimmedZString(line.FieldValues[76], 50);
			}

			if (totalFieldValuesCount > 77)
			{
				orgData.Language = TrimmedZString(line.FieldValues[77], 7);
			}

			if (totalFieldValuesCount > 78)
			{
				orgData.MainAddress.Language = TrimmedZString(line.FieldValues[78], 7);
			}

			if (totalFieldValuesCount > 79)
			{
				orgData.PostalAddress.Language = TrimmedZString(line.FieldValues[79], 7);
			}

			if (totalFieldValuesCount > 80)
			{
				orgData.DeliveryAddress.Language = TrimmedZString(line.FieldValues[80], 7);
			}

			int totalSupportedFieldsCount = ValidMandatoryFileHeaderColumns.Length + ValidOptionalFileHeaderColumns.Length;
			for (int i = ValidMandatoryFileHeaderColumns.Length; i < totalSupportedFieldsCount && i < totalFieldValuesCount; i++)
			{
				string headerColumn = GetHeadColumn(i);
				ExtraOptionalData(orgData, DataError, headerColumn, line.FieldValues[i]);
			}

			if (!(customsAgent.IsEmpty && debtor.IsEmpty && aRSettlementGroup.IsEmpty))
			{
				OrgLinksExist = true;
				OrgLinks links = new OrgLinks();
				links.OrgLegacyCode = orgData.OrgCode;
				links.CustomsAgent = customsAgent;
				links.Debtor = debtor;
				links.ARSettlementGroup = aRSettlementGroup;
				OrganisationLinks.Add(links);
			}

			return orgData;
		}

		void ExtraOptionalData(CsvOrg orgData, CsvOrgDataError dataError, string headerColumn, string value)
		{
			if (headerColumn.StartsWith("REGDETAIL"))
			{
				if (!string.IsNullOrEmpty(value) && !orgData.OptionalBusRegDetails.ContainsKey(value))
				{
					ZString errorMsg;
					var busRegDetail = new BusRegDetail(value);
					if (busRegDetail.Validate(orgData.OrgCode, orgData.OrgName, value, headerColumn, out errorMsg))
					{
						orgData.OptionalBusRegDetails.Add(value, busRegDetail);
					}
					else
					{
						dataError.ErrorMsg = errorMsg;
						throw new ArgumentException(errorMsg);
					}
				}
			}
			else
			{
				switch (headerColumn)
				{
					case "BANKCURRENCY":
						orgData.BankCurrency = TrimmedZString(value);
						break;
					case "WAREHOUSE":
						orgData.IsWarehouse = IsYesChar(value);
						break;
					case "CONTROLLINGAGENT":
						orgData.IsControllingAgent = IsYesChar(value);
						break;
					case "CONTROLLINGCUSTOMER":
						orgData.IsControllingCustomer = IsYesChar(value);
						break;
				}
			}
		}

		string GetHeadColumn(int index)
		{
			string result = "";
			if (index > 0 && index < headerColumns.Length)
			{
				result = headerColumns[index];
			}
			return result;
		}

		bool IsYesChar(string value)
		{
			return value.Trim() == "Y";
		}

		ZString TrimmedZString(string value)
		{
			return new ZString(value).Trim();
		}

		ZString TrimmedZString(string value, int maxLength)
		{
			return TrimmedZString(value).SubstringSafe(0, maxLength);
		}

		ZDateTime ParsedZDateTime(string value)
		{
			ZDateTime result = ZDateTime.Empty;
			ZDateTime.TryParseExact(value, out result, "yyyyMMdd");
			return result;
		}

		#endregion

		#region Utilities

		#region SetupBeforeImport

		protected override void SetupBeforeImport()
		{
			base.SetupBeforeImport();

			DefaultDebtorGroup = GetDefaultDebtorGroupPK();
			DefaultCreditorGroup = GetDefaultCreditorGroupPK();
		}

		#endregion

		#region Notifications

		internal void DisplayLinkedDataTotals()
		{
			DisplayLogMessage("\r\n" + Res.GetString("6846e930-8226-4533-b9a9-90a207db096f", "T O T A L : Organizations updated with linked records = {0}", RunCounters.RecsCreated) + "\r\n");
		}

		protected internal void DisplayFormattedLogMessage(ZString organisation, string detailedExceptionMessage)
		{
			var ouputRowNo = Res.GetString("f7f1b6e1-74dd-4696-9ce3-821835184898", "Line {0}:", RunCounters.CurrentRow.ToString());
			var logMessage = Res.GetString("d86afe01-ed63-46a4-a1e2-3c8ed824b47b", "{0} Organization: {1}  {2}", ouputRowNo, organisation, detailedExceptionMessage);

			DisplayLogMessage(logMessage);
		}

		#endregion

		#region Org Handling

		protected OrgHeader FindOrganisationIfItExists(CsvOrg extractedData)
		{
			OrgHeader result = null;
			if (extractedData.OrgCode.IsEmpty)
			{
				result = FindOrganisationFromNameAndAddress(extractedData);
			}
			else
			{
				result = GetOrgFromLegacyCode(extractedData.OrgCode, OrgCusCode.CodeTypes.LegacySystemCode);
			}

			return result;
		}

		OrgHeader FindOrganisationFromNameAndAddress(CsvOrg extractedData)
		{
			OrgHeader result = null;

			ZQuery orgFilter = new ZQuery(OrgHeaderSchema.OH_FullName, extractedData.OrgName);
			if (extractedData.UNLocode.IsValid && extractedData.UNLocode != "ZZZZZ")
			{
				orgFilter.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, extractedData.UNLocode);
			}
			OrgHeader[] organisations = (OrgHeader[])Factory.Load(typeof(OrgHeader), orgFilter);

			foreach (OrgHeader organisation in organisations) // The same organisation can exist in multiple locations within the one locode
			{
				ZQuery addressFilter = new ZQuery(OrgAddressSchema.OA_OH, organisation.PK);
				addressFilter.AddToFilter(OrgAddressSchema.OA_Address1, extractedData.MainAddress.Address1);
				addressFilter.AddToFilter(OrgAddressSchema.OA_Address2, extractedData.MainAddress.Address2);
				addressFilter.AddToFilter(OrgAddressSchema.OA_City, extractedData.MainAddress.Address3);
				addressFilter.AddToFilter(OrgAddressSchema.OA_PostCode, extractedData.MainAddress.Postcode);
				addressFilter.AddToFilter(OrgAddressSchema.OA_Phone, extractedData.Phone);
				OrgAddress organisationAddress = (OrgAddress)Factory.LoadTop1(typeof(OrgAddress), addressFilter);
				if (organisationAddress != null)
				{
					result = organisation;
					break;
				}
			}

			return result;
		}

		OrgHeader GetOrgFromLegacyCode(ZString orgLegacyCode, ZString codeType)
		{
			OrgHeader org = null;
			ZQuery codeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, codeType);
			codeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			codeFilter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, orgLegacyCode);
			OrgCusCode orgLegacyCodeRecord = (OrgCusCode)Factory.LoadTop1(typeof(OrgCusCode), codeFilter);
			if (orgLegacyCodeRecord != null)
			{
				org = (OrgHeader)Factory.Load(typeof(OrgHeader), orgLegacyCodeRecord.OK_OH);
			}

			return org;
		}

		ZGuid GetOrgPKFromLegacyCode(ZString orgLegacyCode, ZString codeType)
		{
			ZGuid orgPK = ZGuid.Empty;
			ZQuery codeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, codeType);
			codeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			codeFilter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, orgLegacyCode);
			OrgCusCode orgLegacyCodeRecord = (OrgCusCode)Factory.LoadTop1(typeof(OrgCusCode), codeFilter);
			if (orgLegacyCodeRecord != null)
			{
				orgPK = orgLegacyCodeRecord.OK_OH;
			}

			return orgPK;
		}

		ZString GetCompanyCode(OrgHeader organisation, ZString orgCode)
		{
			ZString companyCode;

			if (GenerateNewOrgCode || orgCode.IsEmpty)
			{
				companyCode = organisation.OH_Code;
			}
			else
			{
				organisation.OH_Code = "";
				companyCode = orgCode.SubstringSafe(0, 12).ToUpper();
				companyCode = ValidateOrgCodeIsUnique(companyCode);
			}

			return companyCode;
		}

		string ValidateOrgCodeIsUnique(string orgCode)
		{
			bool uniqueCode = false;
			int uniquenessAdjustment = 0;
			string orgCodeForChecking = orgCode;
			while (!uniqueCode)
			{
				OrgHeader orgFound = OrgHeader.LoadFromCode(Factory, orgCodeForChecking);
				if (orgFound == null)
				{
					uniqueCode = true;
				}
				else
				{
					uniquenessAdjustment += 1;
					if (orgCodeForChecking.Length > 9)
					{
						orgCodeForChecking = orgCodeForChecking.Substring(0, 9);
					}

					orgCodeForChecking = orgCodeForChecking + uniquenessAdjustment.ToString();
				}
			}

			return orgCodeForChecking;
		}

		OrgHeader FindLinkedOrganisation(ZString orgLegacyValue, string orgType)
		{
			OrgHeader linkedOrganisation = null;
			try
			{
				ZGuid orgLinkPK = GetOrgPKFromLegacyCode(orgLegacyValue, OrgCusCode.CodeTypes.LegacySystemCode);
				linkedOrganisation = (OrgHeader)Factory.Load(typeof(OrgHeader), orgLinkPK);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				DisplayLogMessage(Res.GetString("f0e7b4fb-7494-415e-ab3b-772742a4a3fb", "{0} / {1} Organization not found on linking parse", orgLegacyValue, orgType));
			}

			return linkedOrganisation;
		}

		#endregion

		#region DB Queries

		ZGuid GetDefaultDebtorGroupPK()
		{
			return ObjectFactory.Get<IAccounting>().ARAccountGroup;
		}

		ZGuid GetDebtorGroupPKFromCode(ZString debtorGroupCode)
		{
			OrgDebtorGroup debtorGroup = (OrgDebtorGroup)Factory.LoadFromNaturalKey(typeof(OrgDebtorGroup), OrgDebtorGroupSchema.OJ_Code, debtorGroupCode)
				?? CreateDebtorGroup(debtorGroupCode);

			return debtorGroup.PK;
		}

		OrgDebtorGroup CreateDebtorGroup(ZString debtorGroupCode)
		{
			OrgDebtorGroup debtorGroup = Factory.New<OrgDebtorGroup>();
			debtorGroup.OJ_Code = debtorGroupCode;
			debtorGroup.OJ_Desc = Res.GetString("93581b11-fd11-4c50-aecf-84577582d430", "From data import: Code {0}", debtorGroupCode);

			return debtorGroup;
		}

		ZGuid GetCreditorGroupPKFromCode(ZString creditorGroupCode)
		{
			OrgCreditorGroup creditorGroup = (OrgCreditorGroup)Factory.LoadFromNaturalKey(typeof(OrgCreditorGroup), OrgCreditorGroupSchema.OG_Code, creditorGroupCode)
				?? CreateCreditorGroup(creditorGroupCode);

			return creditorGroup.PK;
		}

		OrgCreditorGroup CreateCreditorGroup(ZString creditorGroupCode)
		{
			OrgCreditorGroup creditorGroup = Factory.New<OrgCreditorGroup>();
			creditorGroup.OG_Code = creditorGroupCode;
			creditorGroup.OG_Desc = Res.GetString("93581b11-fd11-4c50-aecf-84577582d430", "From data import: Code {0}", creditorGroupCode);

			return creditorGroup;
		}

		ZGuid GetDefaultCreditorGroupPK()
		{
			return ObjectFactory.Get<IAccounting>().APAccountGroup;
		}

		#endregion

		#endregion

		#region CSV File Header

		protected string[] ValidMandatoryFileHeaderColumns
		{
			get { return validMandatoryFileHeaderColumns ?? (validMandatoryFileHeaderColumns = GetNewValidMandatoryFileHeaderColumns()); }
		}
		string[] validMandatoryFileHeaderColumns;

		protected virtual string[] GetNewValidMandatoryFileHeaderColumns()
		{
			return new string[] {
						"CODE",
						"NAME",
						"ADDRESS1",
						"ADDRESS2",
						"CITY",
						"STATE",
						"POSTCODE",
						"UNLOCO",
						"COUNTRY",
						"PORTCITY",
						"PHONE",
						"FAX",
						"EMAIL",
						"WEB",
						"REGNO",
						"CORPCODE",
						"DEBTOR",
						"CREDITOR",
						"CONSIGNEE",
						"CONSIGNOR",
						"FORWARDER",
						"BROKER",
						"CARRIER",
						"SHIPLINE",
						"AIRLINE",
						"LOCALTRANSPORT",
						"SALESLEAD",
						"SERVICES",
						"COMPETITOR",
						"CONTACT",
						"TITLE",
						"EMAIL",
						"PHONE",
						"MOBILE",
						"FAX",
						"DEBTORCODE",
						"DEBTORGROUP",
						"DEBTORSETTLEGROUP",
						"CURRENCY",
						"CREDITLIMIT",
						"CREDITRATING",
						"GST",
						"INV_TERMS_STANDARD",
						"INV_DAYS_STANDARD",
						"INV_TERMS_DISBURSEMENT",
						"INV_DAYS_DISBURSEMENT",
						"CREDITORGROUP",
						"CUSTOMSAGENT",
						"POSTADDRESS1",
						"POSTADDRESS2",
						"POSTCITY",
						"POSTSTATE",
						"POSTPOSTCODE",
						"DELIVERADDRESS1",
						"DELIVERADDRESS2",
						"DELIVERCITY",
						"DELIVERSTATE",
						"DELIVERPOSTCODE",
						"BANK",
						"ACCOUNTNAME",
						"ACCOUNTNO",
						"BSB",
						"CCD",
						"CSC",
						"SCC",
						"CCP",
						"CCC",
						"CMP",
						"WORKNOTES",
						"HANDLINGNOTES",
						"DELIVERYNOTES",
						"ARNOTES",
						"ARCREDITNOTES",
						"APNOTES",
						"CONTACTSOURCETYPE",
						"CONTACTDATEDETAILSVERIFIED",
						"CONTACTSALUTATION",
						"LANGUAGE",
						"MAINADDRESSLANGUAGE",
						"POSTALADDRESSLANGUAGE",
						"DELIVERYADDRESSLANGUAGE"
					};
		}

		protected string[] ValidOptionalFileHeaderColumns
		{
			get { return validOptionalFileHeaderColumns ?? (validOptionalFileHeaderColumns = GetValidOptionalFileHeaderColumns()); }
		}
		string[] validOptionalFileHeaderColumns;

		string[] GetValidOptionalFileHeaderColumns()
		{
			List<string> result = new List<string>();
			result.Add("BANKCURRENCY");
			for (int i = 1; i < 100; i++)
			{
				result.Add("REGDETAIL" + i.ToString());
			}
			result.Add("WAREHOUSE");
			result.Add("CONTROLLINGAGENT");
			result.Add("CONTROLLINGCUSTOMER");

			return result.ToArray();
		}

		#endregion

		#region Validation

		protected override bool IsFileHeaderValid(OCsvLine line)
		{
			bool result = false;
			if (AreMandatoryFileHeaderColumnsValid(line) && AreOptionalFileHeaderColumnsValid(line))
			{
				result = true;
				StoreHeaderColumn(line.FieldValues);
			}
			return result;
		}

		void StoreHeaderColumn(string[] columns)
		{
			List<string> headers = new List<string>();
			foreach (string column in columns)
			{
				headers.Add(column.ToUpper());
			}
			headerColumns = headers.ToArray();
		}
		string[] headerColumns;

		bool AreOptionalFileHeaderColumnsValid(OCsvLine line)
		{
			int numberOfMandatoryColumns = ValidMandatoryFileHeaderColumns.Length;
			int numberOfOptionalColumns = ValidOptionalFileHeaderColumns.Length;
			int totalFields = line.FieldValues.Length;
			bool result = totalFields >= numberOfMandatoryColumns && totalFields <= (numberOfMandatoryColumns + numberOfOptionalColumns);
			if (result)
			{
				List<string> optionalFileHeaderColumns = new List<string>(ValidOptionalFileHeaderColumns);
				List<string> invalidColumns = new List<string>();
				List<string> matchedColumns = new List<string>();
				for (int i = numberOfMandatoryColumns; i < totalFields; i++)
				{
					string headerColumn = line.FieldValues[i].ToUpper();
					if (optionalFileHeaderColumns.Contains(headerColumn))
					{
						optionalFileHeaderColumns.Remove(headerColumn);
						matchedColumns.Add(headerColumn);
					}
					else
					{
						invalidColumns.Add(headerColumn);
					}
				}
				if (invalidColumns.Count > 0)
				{
					result = false;
					ZStringBuilder notSupportedColumsMessage = new ZStringBuilder();
					ZStringBuilder duplicatedColumns = new ZStringBuilder();
					foreach (string columns in invalidColumns)
					{
						if (matchedColumns.Contains(columns))
						{
							duplicatedColumns.Append(columns);
						}
						else
						{
							notSupportedColumsMessage.Append(columns);
						}
					}

					if (!duplicatedColumns.IsEmpty || !notSupportedColumsMessage.IsEmpty)
					{
						DisplayLogMessage(Res.GetString("579c0a69-8f2f-4100-8e33-f8989ff823af", "Unexpected columns were found in the File Header after column 80."));
						if (!duplicatedColumns.IsEmpty)
						{
							DisplayLogMessage(Res.GetString("2396da77-938f-466e-a180-ce4f4670cb01", "Duplicated Columns:"));
							DisplayLogMessage(duplicatedColumns.ToStringWithDelimiterBetweenAppends(", "));
						}
						if (!notSupportedColumsMessage.IsEmpty)
						{
							DisplayLogMessage(Res.GetString("7ffc658d-93b4-43af-9807-954a5917729b", "Not Supported Columns:"));
							DisplayLogMessage(notSupportedColumsMessage.ToStringWithDelimiterBetweenAppends(", "));
						}
					}
				}
			}
			return result;
		}

		bool AreMandatoryFileHeaderColumnsValid(OCsvLine line)
		{
			int numberOfMandatoryColumns = ValidMandatoryFileHeaderColumns.Length;
			bool result = line.FieldValues.Length >= numberOfMandatoryColumns;
			if (result)
			{
				for (int i = 0; i < numberOfMandatoryColumns; i++)
				{
					if (!ValidMandatoryFileHeaderColumns[i].Equals(line.FieldValues[i], StringComparison.OrdinalIgnoreCase))
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}

		public override string CSVTemplateHeading
		{
			get { return String.Join(",", ValidMandatoryFileHeaderColumns); }
		}

		void ValidateInvoiceTerms(CsvOrg orgData)
		{
			if (orgData.InvTermsStd.IsEmpty || !IsAValidInvoiceTerm(orgData.InvTermsStd))
			{
				orgData.InvTermsStd = "COD";
			}

			if (orgData.InvTermsDisb.IsEmpty || !IsAValidInvoiceTerm(orgData.InvTermsDisb))
			{
				orgData.InvTermsDisb = "COD";
			}
		}

		bool IsAValidInvoiceTerm(string invTerm)
		{
			return (invTerm == "COD" || invTerm == "INV" || invTerm == "MTH" || invTerm == "PER" || invTerm == "SHP");
		}

		#endregion
	}
}
