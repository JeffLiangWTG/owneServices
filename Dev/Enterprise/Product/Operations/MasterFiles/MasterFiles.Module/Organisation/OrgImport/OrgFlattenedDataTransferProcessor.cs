using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport
{
	public class OrgFlattenedDataTransferProcessor : MergeFlattenedDataTransferProcessor<OrgHeader, OrgFlattened>
	{
		readonly BusinessObjectFactory factory;
		readonly Dictionary<OrgFlattened, OrgHeader> orgsToLinkDict;

		public OrgFlattenedDataTransferProcessor(ImportCollectionInfoImplForOrgFlattened importCollectionInfo, Dictionary<OrgFlattened, OrgHeader> orgsToLinkDict)
			: base(new OrgHeaderCollection(((IImportCollectionInfo)importCollectionInfo).Collection.Factory), importCollectionInfo)
		{
			this.factory = headerCollection.Factory;
			this.orgsToLinkDict = orgsToLinkDict;

			AddUniqueChildrenMerger(new ChildMainAddressMerger(importCollectionInfo));
			AddUniqueChildrenMerger(new ChildPostalAddressMerger(importCollectionInfo));
			AddUniqueChildrenMerger(new ChildDeliveryAddressMerger(importCollectionInfo));
			AddUniqueChildrenMerger(new ChildContactMerger(importCollectionInfo));
		}

		BusinessObject[] FindRegistrationDetailFor(OrgHeader org, BusRegDetail busRegDetail)
		{
			ZQuery query = new ZQuery(OrgCusCodeSchema.OK_CodeType, busRegDetail.CodeType);
			query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, busRegDetail.CountryOfIssue);
			query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, busRegDetail.RegistrationNumber);
			return org.CustomsCodes.Find(query);
		}

		protected override string GetProgressChangedStatus(int recordsProcessed)
		{
			return Res.GetString("faddab4f-23bb-4a10-93e9-ed899fb4c3c2", "Importing organizations ({0} of {1}) ...", recordsProcessed, flattenedCollection.Count);
		}

		public override void Import()
		{
			flattenedCollection.Factory.ActivateStringInterning();
			var originalRefreshValue = flattenedCollection.Factory.RefreshEnabled;
			try
			{
				flattenedCollection.SuspendValidation();
				flattenedCollection.Factory.SuspendValidation();
				flattenedCollection.Factory.RefreshEnabled = false;
				using (flattenedCollection.SuspendListChanged())
				{
					base.Import();
				}
			}
			finally
			{
				flattenedCollection.ResumeValidation();
				flattenedCollection.Factory.ResumeValidation();
				flattenedCollection.Factory.RefreshEnabled = originalRefreshValue;
			}
		}

		IEnumerable<string> headerColumns;
		protected override IEnumerable<string> HeaderColumnsOnFlattened
		{
			get { return headerColumns ?? (headerColumns = ((ImportCollectionInfoImplForOrgFlattened)flattenedImportCollectionInfo).HeaderProperties.Select(property => property.MappingName)); }
		}

		readonly Guid defaultDebtorGroup = ObjectFactory.Get<Enterprise.Integration.Accounting.IAccounting>().ARAccountGroup;
		readonly Guid defaultCreditorGroup = ObjectFactory.Get<Enterprise.Integration.Accounting.IAccounting>().APAccountGroup;
		protected override OrgHeader CreateHeader(IBusinessObjectCollection orgCollection, OrgFlattened orgFlattened)
		{
			OrgHeader newOrg = null;
			try
			{
				orgFlattened.OA_Address1 = orgFlattened.OA_Address1.Trim();

				if (CheckProperties(orgFlattened))
				{
					newOrg = (OrgHeader)orgCollection.AddNew();
					newOrg.OH_FullName = Env.Registry.OrgAllowMixedCase ? orgFlattened.OH_FullName : orgFlattened.OH_FullName.ToUpper();

					newOrg.OH_RL_NKClosestPort = orgFlattened.OH_RL_NKClosestPort; // must be set before state and branch
					if (string.IsNullOrWhiteSpace(newOrg.OH_RL_NKClosestPort))
					{
						newOrg.OH_RL_NKClosestPort = RefUNLOCO.LookupPortCodeFromCountryCityOrAUState(factory, orgFlattened.Country, orgFlattened.City, orgFlattened.OA_State);
						orgFlattened.OH_RL_NKClosestPort = newOrg.OH_RL_NKClosestPort;
					}

					if (newOrg.OH_Code.IsEmpty && !newOrg.CanGenerateCode)
					{
						AddErrorMessageAndDeleteOrg(newOrg, orgFlattened, Res.GetString("d8b949ec-ffc1-4279-90c9-b160b34b141e", "Cannot generate the organization code based on the data imported, please check the Full Name of the provided data."));
						return null;
					}

					// need to find existing organization after UNLOCO has been set
					if (orgFlattened.OH_Code.IsEmpty)
					{
						var foundOrg = FindOrganisationFromNameAndAddress(orgFlattened, newOrg);
						if (foundOrg != null)
						{
							AddErrorMessageAndDeleteOrg(newOrg, orgFlattened, foundOrg.IsInDatabase ? AlreadyExistsInEnterpriseTableErrorMessage : AlreadyImportingOrgWithSameNameAndAddressErrorMessage);
							return null;
						}
					}
					else
					{
						var foundOrg = FindOrganisationFromCode(orgFlattened);
						if (foundOrg != null)
						{
							AddErrorMessageAndDeleteOrg(newOrg, orgFlattened, foundOrg.IsInDatabase ? AlreadyExistsInEnterpriseTableErrorMessage : AlreadyImportingOrgWithSameCodeErrorMessage);
							return null;
						}
					}

					newOrg.MainWebURL.PU_URL = orgFlattened.PU_URL;
					if (!orgFlattened.OH_Language.IsEmpty)
					{
						newOrg.OH_Language = orgFlattened.OH_Language;
					}

					ProcessBusReg(orgFlattened, newOrg);

					newOrg.CompanyData.OB_IsDebtor = orgFlattened.OB_IsDebtor;
					newOrg.CompanyData.OB_IsCreditor = orgFlattened.OB_IsCreditor;
					newOrg.OH_IsConsignee = orgFlattened.OH_IsConsignee;
					newOrg.OH_IsConsignor = orgFlattened.OH_IsConsignor;
					newOrg.OH_IsForwarder = orgFlattened.OH_IsForwarder;
					newOrg.OH_IsBroker = orgFlattened.OH_IsBroker;
					newOrg.OH_IsShippingProvider = orgFlattened.OH_IsShippingProvider;
					newOrg.OH_IsShippingLine = orgFlattened.OH_IsShippingLine;
					newOrg.OH_IsAirLine = orgFlattened.OH_IsAirLine;
					newOrg.OH_IsLocalTransport = orgFlattened.OH_IsLocalTransport;
					newOrg.OH_IsSalesLead = orgFlattened.OH_IsSalesLead;
					newOrg.OH_IsMiscFreightServices = orgFlattened.OH_IsMiscFreightServices;
					newOrg.OH_IsCompetitor = orgFlattened.OH_IsCompetitor;
					newOrg.OH_IsWarehouseClient = orgFlattened.OH_IsWarehouseClient;
					newOrg.OH_IsControllingAgent = orgFlattened.OH_IsControllingAgent;
					newOrg.OH_IsControllingCustomer = orgFlattened.OH_IsControllingCustomer;

					ProcessDebtor(defaultDebtorGroup, orgFlattened, newOrg);
					ProcessCreditor(defaultCreditorGroup, orgFlattened, newOrg);
					ProcessForwarder(orgFlattened, newOrg);

					// load corp code
					ProcessRegDetails(orgFlattened, newOrg);

					LoadCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, orgFlattened.CustomsCode, newOrg);
					LoadCustomsCode(OrgCusCode.CodeTypes.SupplierCode, orgFlattened.SupplierCode, newOrg);
					LoadCustomsCode(OrgCusCode.CodeTypes.CustomsClientID, orgFlattened.CMRSupplierCode, newOrg);
					LoadCustomsCode(OrgCusCode.CodeTypes.CarrierCode, orgFlattened.CarrierCode, newOrg);
					LoadCustomsCode(OrgCusCode.CodeTypes.ManifestProviderID, orgFlattened.ManifestProviderCode, newOrg);
					LoadCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, orgFlattened.PremiseID, newOrg);
					LoadCustomsCode(OrgCusCode.CodeTypes.LegacySystemCode, orgFlattened.OH_Code, newOrg);

					newOrg.IgnoreValidationSuspended = true;
					newOrg.Validation.ValidateOH_RSL_ShippingLine();
					if (newOrg.OH_RSL_ShippingLineInfo.GetErrors().Count() > 0)
					{
						AddErrorMessageAndDeleteOrg(newOrg, orgFlattened, newOrg.OH_RSL_ShippingLineInfo.GetErrors().GetFirstMessage());
						return null;
					}

					LoadNotes(newOrg, orgFlattened.WorkNotes, PredefinedNoteTypes.Instance.InternalWorkNotes.Description);
					LoadNotes(newOrg, orgFlattened.ForwardingNotes, PredefinedNoteTypes.Instance.HandlingInstructions.Description);
					LoadNotes(newOrg, orgFlattened.DeliveryNotes, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description);
					LoadNotes(newOrg, orgFlattened.InvoiceNotes, PredefinedNoteTypes.Instance.AccountsReceivableCreditManagementNote.Description);
					LoadNotes(newOrg, orgFlattened.APNotes, PredefinedNoteTypes.Instance.AccountsPayableAccountManagementNotes.Description);
					LoadNotes(newOrg, orgFlattened.ARNotes, PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes.Description);

					if (!(orgFlattened.CustomsAgent.IsEmpty && orgFlattened.Debtor.IsEmpty && orgFlattened.DebtorSettlementGroup.IsEmpty))
					{
						orgsToLinkDict.Add(orgFlattened, newOrg);
					}
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				AddErrorMessageAndDeleteOrg(newOrg, orgFlattened, Res.GetString("93581b11-fd11-4c50-aecf-84577582d435", "data is inconsistent with required format {0}", e.Message));
				return null;
			}

			return newOrg;
		}

		bool CheckProperties(OrgFlattened orgFlattened)
		{
			var result = CheckMandatoryProperties(orgFlattened) && CheckPropertiesLength(orgFlattened);

			return result;
		}

		bool CheckMandatoryProperties(OrgFlattened orgFlattened)
		{
			var result = false;
			var notValidProperties = new List<String>();

			if (orgFlattened.OA_Address1.IsEmpty)
			{
				notValidProperties.Add((NoResString)"Address 1");
			}

			if (orgFlattened.Country.IsEmpty)
			{
				notValidProperties.Add((NoResString)"Country");
			}

			if (notValidProperties.Count <= 0)
			{
				result = true;
			}
			else
			{
				AddErrorMessage(orgFlattened, Res.GetString("d4f272b7-64e0-4c84-94cc-32491f5c67d0", "cannot import row with empty {0}", string.Join(", ", notValidProperties)));
			}

			return result;
		}

		bool CheckPropertiesLength(OrgFlattened orgFlattened)
		{
			var result = false;

			if (orgFlattened.OA_Address1.Length < OrgAddress.Schema.OA_Address1MinimumLength)
			{
				AddErrorMessage(orgFlattened, Res.GetString("5896a8cd-04af-4002-a837-13c07a63c71c", "cannot import row with Address 1 less than {0} characters", OrgAddress.Schema.OA_Address1MinimumLength));
			}
			else
			{
				result = true;
			}

			return result;
		}

		OrgHeader FindOrganisationFromCode(OrgFlattened org)
		{
			ZQuery codeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.LegacySystemCode);
			codeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			codeFilter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, org.OH_Code);
			OrgCusCode orgLegacyCodeRecord = factory.LoadTop1<OrgCusCode>(codeFilter);
			if (orgLegacyCodeRecord != null)
			{
				return factory.Load<OrgHeader>(orgLegacyCodeRecord.OK_OH);
			}
			return null;
		}

		OrgHeader FindOrganisationFromNameAndAddress(OrgFlattened orgFlattened, OrgHeader orgToIgnore)
		{
			ZQuery orgFilter = new ZQuery(OrgHeaderSchema.OH_FullName, orgFlattened.OH_FullName);
			orgFilter.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, orgToIgnore.PK);
			if (orgFlattened.OH_RL_NKClosestPort.IsValid && orgFlattened.OH_RL_NKClosestPort != "ZZZZZ")
			{
				orgFilter.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, orgFlattened.OH_RL_NKClosestPort);
			}
			OrgHeader[] organisations = factory.Load<OrgHeader>(orgFilter);

			foreach (OrgHeader organisation in organisations) // The same organisation can exist in multiple locations within the one locode
			{
				ZQuery addressFilter = new ZQuery(OrgAddressSchema.OA_OH, organisation.PK);
				addressFilter.AddToFilter(OrgAddressSchema.OA_Address1, (string.IsNullOrEmpty(orgFlattened.OA_Address1) && organisation.IsInDatabase) ? OrgAddress.AddressNotOnFile : orgFlattened.OA_Address1.ToString());
				addressFilter.AddToFilter(OrgAddressSchema.OA_Address2, orgFlattened.OA_Address2);
				addressFilter.AddToFilter(OrgAddressSchema.OA_City, orgFlattened.OA_City);
				addressFilter.AddToFilter(OrgAddressSchema.OA_PostCode, orgFlattened.OA_PostCode);
				addressFilter.AddToFilter(OrgAddressSchema.OA_Phone, orgFlattened.OA_Phone);
				OrgAddress organisationAddress = factory.LoadTop1<OrgAddress>(addressFilter);
				if (organisationAddress != null)
				{
					return organisation;
				}
			}
			return null;
		}

		void ProcessRegDetails(OrgFlattened org, OrgHeader newOrg)
		{
			Dictionary<ZString, BusRegDetail> busRegDetails = new Dictionary<ZString, BusRegDetail>(100);

			for (int i = 1; i <= 100; ++i)
			{
				PropertyInfo regPropInfo = org.GetType().GetProperty("RegDetail" + i);
				ZString value = (ZString)regPropInfo.GetValue(org, null);

				if (!string.IsNullOrEmpty(value) && !busRegDetails.ContainsKey(value))
				{
					ZString errorMsg;
					BusRegDetail busRegDetail = new BusRegDetail(value);

					if (busRegDetail.Validate(org.OH_Code, org.OH_FullName, value, regPropInfo.Name.ToUpper(), out errorMsg))
					{
						busRegDetails.Add(value, busRegDetail);
					}
					else
					{
						throw new ArgumentException(errorMsg);
					}
				}
			}

			foreach (var pair in busRegDetails)
			{
				var busRegDetail = pair.Value;
				var cusCodes = FindRegistrationDetailFor(newOrg, busRegDetail);
				if (cusCodes.Length == 0)
				{
					cusCodes = newOrg.CustomsCodes.GetOrgCusCodesForCodeAndCountry(busRegDetail.CodeType, busRegDetail.CountryOfIssue);
					OrgCusCode cusCode = cusCodes.Length == 1 ? (OrgCusCode)cusCodes[0] : null;
					if (cusCode == null)
					{
						cusCode = newOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(busRegDetail.CodeType, busRegDetail.RegistrationNumber, busRegDetail.CountryOfIssue);
						cusCode.OK_RN_NKCodeCountry = busRegDetail.CountryOfIssue;
					}
					else
					{
						cusCode.OK_CustomsRegNo = busRegDetail.RegistrationNumber;
					}
					if (cusCode.PremisesAddressIsAllowed)
					{
						cusCode.OK_OA_PremisesAddress = newOrg.MainAddress.PK;
					}
				}
			}
		}

		protected void LoadNotes(OrgHeader enterpriseOrganisation, ZString notes, string noteType)
		{
			if (!notes.IsEmpty)
			{
				ZQuery noteFilter = new ZQuery(StmNoteSchema.ST_ParentID, enterpriseOrganisation.PK);
				noteFilter.AddToFilter(StmNoteSchema.ST_Table, "OrgHeader");
				noteFilter.AddToFilter(StmNoteSchema.ST_Description, noteType);
				StmNote orgNote = factory.LoadTop1<StmNote>(noteFilter);
				if (orgNote == null)
				{
					StmNote newOrgNote = enterpriseOrganisation.Notes.AddNew();
					newOrgNote.ST_Description = noteType;
					newOrgNote.ST_Table = "OrgHeader";
					newOrgNote.ST_NoteDataAsText = notes;
				}
			}
		}

		void ProcessBusReg(OrgFlattened org, OrgHeader newOrg)
		{
			if (!string.IsNullOrEmpty(org.BusRegNo))
			{
				string registrationCodeType = OrgCusCode.CodeTypes.GovBusinessCode;
				switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
				{
					case Constants.CountryCodes.UnitedStates:
						if (OrgDataLoad.IsValidEIN(org.BusRegNo))
						{
							registrationCodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
						}
						else if (OrgDataLoad.IsValidSSN(org.BusRegNo))
						{
							registrationCodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
						}
						else if (OrgDataLoad.IsValidCBPAssignedNumber(org.BusRegNo))
						{
							registrationCodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
						}
						else
						{
							registrationCodeType = ZString.Empty;
						}
						break;
					case Constants.CountryCodes.Australia:
						registrationCodeType = org.BusRegNo.Length == 9
																		? OrgCusCode.CodeTypes.CorporationCode
																		: OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
						break;
					case Constants.CountryCodes.SouthAfrica:
					case Constants.CountryCodes.UnitedKingdom:
					case Constants.CountryCodes.Sweden:
						registrationCodeType = OrgCusCode.CodeTypes.VATCode;
						break;
					case Constants.CountryCodes.Italy:
						registrationCodeType = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
						break;
					case Constants.CountryCodes.SriLanka:
						registrationCodeType = OrgCusCode.SriLankaCodeTypes.SVATBusinessRegistrationNumber;
						break;
					case Constants.CountryCodes.Ethiopia:
						registrationCodeType = OrgCusCode.EthiopiaCodeTypes.TaxIdentificationNumber;
						break;
				}

				LoadCustomsCode(registrationCodeType, org.BusRegNo, newOrg);
			}

			string corpCodeType = OrgCusCode.CodeTypes.CorporationCode;
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedKingdom)
			{
				corpCodeType = ZString.Empty; // Contact DJC or A.G if you have an incident related to UK corp code being empty
			}
			if (!string.IsNullOrEmpty(corpCodeType))
			{
				LoadCustomsCode(corpCodeType, org.BusRegACN, newOrg);
			}
		}

		void LoadCustomsCode(ZString registrationCodeType, ZString value, OrgHeader orgHeader)
		{
			if (!string.IsNullOrEmpty(value))
			{
				ZQuery cusCodeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, registrationCodeType);
				cusCodeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				cusCodeFilter.AddToFilter(OrgCusCodeSchema.OK_OH, orgHeader.PK);
				OrgCusCode customsCodeNumber = factory.LoadTop1<OrgCusCode>(cusCodeFilter);
				if (customsCodeNumber == null)
				{
					OrgCusCode newEnterpriseOrgCustomsCodeNumber = orgHeader.CustomsCodes.AddNew();
					newEnterpriseOrgCustomsCodeNumber.OK_CodeType = registrationCodeType;
					newEnterpriseOrgCustomsCodeNumber.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					newEnterpriseOrgCustomsCodeNumber.OK_CustomsRegNo = value.SubstringSafe(0, 35);
				}
			}
		}

		static void ProcessForwarder(OrgFlattened org, OrgHeader newOrg)
		{
			if (newOrg.OH_IsForwarder)
			{
				if (!string.IsNullOrEmpty(org.CurrencyCode))
				{
					newOrg.MiscServ.OM_RX_NKFWDefCurrency = org.CurrencyCode;
				}
			}
		}

		void ProcessCreditor(Guid defaultCreditorGroup, OrgFlattened org, OrgHeader newOrg)
		{
			if (newOrg.OH_IsCreditor)
			{
				if (!string.IsNullOrEmpty(org.CreditorGroup))
				{
					OrgCreditorGroup creditorGroup = factory.LoadFromNaturalKey<OrgCreditorGroup>(OrgCreditorGroupSchema.OG_Code, org.CreditorGroup);
					if (creditorGroup == null)
					{
						creditorGroup = factory.New<OrgCreditorGroup>();
						creditorGroup.OG_Code = org.CreditorGroup;
						creditorGroup.OG_Desc = Res.GetString("c061b9b5-5003-4f45-8fc3-a455812cb08e", "From data import: Code {0}",
																									org.CreditorGroup);
					}

					newOrg.MiscServ.OM_OG_APCreditorGroup = creditorGroup.PK;
				}
				else
				{
					newOrg.MiscServ.OM_OG_APCreditorGroup = defaultCreditorGroup;
				}

				if (OrgContainsAccAPAccountDetails())
				{
					var accountDetails = newOrg.CompanyData.AccountDetailsCollection.AddNew();
					accountDetails.A1_BankName = org.A1_BankName;
					accountDetails.A1_AccountName = org.A1_AccountName;
					accountDetails.A1_BankAccount = org.A1_BankAccount;
					accountDetails.A1_BankBsb = org.A1_BankBsb;
					accountDetails.A1_RX_NKAccountCurrency = org.A1_RX_NKAccountCurrency;
				}
				newOrg.CompanyData.SetAPTaxApplicable(org.GSTApplicable);

				var decimalCheck = org.CreditLimit.ToString();
				double num;
				bool isNum = double.TryParse(decimalCheck, out num);
				if (isNum)
				{
					if (org.CreditLimit > 0)
					{
						newOrg.MiscServ.OM_APCreditLimit = org.CreditLimit;
					}
				}
				else
				{
					throw new Exception(Res.GetString("3250ee74-ca11-4b0d-958f-acf85e0a27be", "data is inconsistent with required format {0}", org.CreditLimitInfo.Name));
				}

				if (!string.IsNullOrEmpty(org.CurrencyCode))
				{
					newOrg.CompanyData.OB_RX_NKAPDefltCurrency = org.CurrencyCode;
				}
			}

			bool OrgContainsAccAPAccountDetails() => !string.IsNullOrEmpty(org.A1_BankBsb) && !string.IsNullOrEmpty(org.A1_BankAccount);
		}

		void ProcessDebtor(Guid defaultDebtorGroup, OrgFlattened org, OrgHeader newOrg)
		{
			if (newOrg.OH_IsDebtor)
			{
				if (!string.IsNullOrEmpty(org.DebtorGroup))
				{
					OrgDebtorGroup debtorGroup = factory.LoadFromNaturalKey<OrgDebtorGroup>(OrgDebtorGroupSchema.OJ_Code, org.DebtorGroup);
					if (debtorGroup == null)
					{
						debtorGroup = factory.New<OrgDebtorGroup>();
						debtorGroup.OJ_Code = org.DebtorGroup;
						debtorGroup.OJ_Desc = Res.GetString("c061b9b5-5003-4f45-8fc3-a455812cb08e", "From data import: Code {0}", org.DebtorGroup);
					}

					newOrg.MiscServ.OM_OJ_ARDebtorGroup = debtorGroup.PK;
				}
				else
				{
					newOrg.MiscServ.OM_OJ_ARDebtorGroup = defaultDebtorGroup;
				}

				newOrg.CompanyData.SetARTaxApplicable(org.GSTApplicable);

				if (org.CreditLimit > 0)
				{
					newOrg.MiscServ.OM_ARCreditLimit = org.CreditLimit;
				}

				if (!string.IsNullOrEmpty(org.CurrencyCode))
				{
					newOrg.CompanyData.OB_RX_NKARDDefltCurrency = org.CurrencyCode;
				}

				if (!string.IsNullOrEmpty(org.A1_BankName))
				{
					newOrg.MiscServ.OM_ARPreviousChequeDrawerBank = org.A1_BankName;
					newOrg.MiscServ.OM_ARPreviousChequeDrawerBankBranch = org.A1_AccountName;
				}

				if (!string.IsNullOrEmpty(org.OM_ARCreditRating))
				{
					newOrg.MiscServ.OM_ARCreditRating = org.OM_ARCreditRating;
				}

				if (org.PY_InvoiceTerm.IsEmpty || !IsAValidInvoiceTerm(org.PY_InvoiceTerm))
				{
					org.PY_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
				}
				if (org.InvoiceTermDisbursement.IsEmpty || !IsAValidInvoiceTerm(org.InvoiceTermDisbursement))
				{
					org.InvoiceTermDisbursement = Constants.InvoiceTerms.CashOnDelivery;
				}

				OrgARTerms termsAR = newOrg.CompanyData.CreateOrLoadARTerm(OrgARTermsLookups.InvoiceTypes.All.Code);
				termsAR.PY_InvoiceTerm = org.PY_InvoiceTerm;
				termsAR.PY_InvoiceDays = org.PY_InvoiceDays;

				if (org.PY_InvoiceTerm != org.InvoiceTermDisbursement || org.PY_InvoiceDays != org.InvoiceDaysDisbursement)
				{
					OrgARTerms disbARTerms = newOrg.CompanyData.CreateOrLoadDisbursementARTerm();
					disbARTerms.PY_InvoiceTerm = org.InvoiceTermDisbursement;
					disbARTerms.PY_InvoiceDays = org.InvoiceDaysDisbursement;
				}
			}
		}

		void AddErrorMessageAndDeleteOrg(OrgHeader orgToDelete, OrgFlattened record, string reason)
		{
			AddErrorMessage(record, reason);
			if (orgToDelete != null)
			{
				orgToDelete.Exile();
			}
		}

		void AddErrorMessage(OrgFlattened record, string reason)
		{
			Log += Res.GetString("5b2b240e-e53c-44a6-a864-8c28136a9b27", "Organization [Code: {0}, Name: {1}] excluded: {2}", record.OH_Code, record.OH_FullName, reason) + "\r\n";
		}

		static bool IsAValidInvoiceTerm(string invTerm)
		{
			return (invTerm == Constants.InvoiceTerms.CashOnDelivery || invTerm == Constants.InvoiceTerms.FromInvoiceDate
				|| invTerm == Constants.InvoiceTerms.FromMonthEnd || invTerm == Constants.InvoiceTerms.FromPeriodEnd
				|| invTerm == Constants.InvoiceTerms.FromShipmentDate || invTerm == Constants.InvoiceTerms.PaymentInAdvance
				|| invTerm == Constants.InvoiceTerms.MonthsFromInvoiceCycleDate || invTerm == Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle);
		}

		#region Error Messages

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		string AlreadyExistsInEnterpriseTableErrorMessage
		{
			get { return Res.GetString("4cba4336-d217-4fdc-a74a-daeaa5de1ac6", "already exists in {0} table", "CargoWise"); }
		}

		string AlreadyImportingOrgWithSameNameAndAddressErrorMessage
		{
			get { return Res.GetString("45b351f9-be01-4cad-80b4-29c8682083c5", "already importing an organization with the same name & address. If a multi-line record was intended, make sure the records are grouped together and all organization details are the same."); }
		}

		string AlreadyImportingOrgWithSameCodeErrorMessage
		{
			get { return Res.GetString("788d7d1d-7bae-45c9-a691-fdb17c865149", "already importing an organization with the same code. If a multi-line record was intended, make sure the records are grouped together and all organization details are the same."); }
		}

		#endregion

		internal class ChildMainAddressMerger : FlattenedToUniqueChildrenMerger<OrgHeader, OrgFlattened>
		{
			public ChildMainAddressMerger(ImportCollectionInfoImplForOrgFlattened importCollectionInfo)
				: base(importCollectionInfo.GetChildImportPropertyInfos(ImportCollectionInfoImplForOrgFlattened.ChildProperty.MainAddress).Select(property => property.MappingName))
			{
			}

			#region Import

			protected override bool CreateChildRecord(OrgHeader parentOrg, OrgFlattened record)
			{
				var useMainAddress = ChildRecordsCreatedForCurrentParent == 0;
				var newAddress = useMainAddress ? parentOrg.MainAddress : parentOrg.Addresses.AddNew();
				newAddress.OA_Address1 = record.OA_Address1;
				newAddress.OA_Address2 = record.OA_Address2;
				newAddress.PrimaryOrgAddressAdditionalInfoDetail = record.OA_AdditionalAddressInformation;

				newAddress.OA_City = record.OA_City;
				newAddress.OA_State = record.OA_State;
				newAddress.OA_PostCode = record.OA_PostCode;
				newAddress.OA_Phone = record.OA_Phone;
				newAddress.OA_Fax = record.OA_Fax;
				newAddress.OA_Email = record.OA_Email;
				if (!record.OA_Language.IsEmpty)
				{ newAddress.OA_Language = record.OA_Language; }

				if (!useMainAddress)
				{
					newAddress.AddressCapability.SetCapabilityEnabled(record.Type);
					newAddress.AddressCapability.SetIsNotMainAddress(record.Type);
				}

				return true;
			}

			#endregion

			#region ChildHumanReadableNameForPlural

			protected override string ChildHumanReadableNameForPluralCore
			{
				get { return Res.GetString("54d841fd-23f9-46e3-910f-e74e1863d878", "Addresses"); }
			}

			#endregion
		}

		internal class ChildPostalAddressMerger : FlattenedToUniqueChildrenMerger<OrgHeader, OrgFlattened>
		{
			public ChildPostalAddressMerger(ImportCollectionInfoImplForOrgFlattened importCollectionInfo)
				: base(importCollectionInfo.GetChildImportPropertyInfos(ImportCollectionInfoImplForOrgFlattened.ChildProperty.PostalAddress).Select(property => property.MappingName))
			{
			}

			#region Import

			protected override bool ContainsChildRecord(ReadonlyImportValuesDictionary propertyValues)
			{
				return !string.IsNullOrEmpty((ZString)propertyValues[OrgFlattened.Schema.Postal_OA_Address1]);
			}

			protected override bool CreateChildRecord(OrgHeader parentOrg, OrgFlattened record)
			{
				var newAddress = parentOrg.Addresses.AddNew();
				newAddress.OA_Address1 = record.Postal_OA_Address1;
				newAddress.OA_Address2 = record.Postal_OA_Address2;
				newAddress.OA_City = record.Postal_OA_City;
				newAddress.OA_State = record.Postal_OA_State;
				newAddress.OA_PostCode = record.Postal_OA_PostCode;
				newAddress.OA_Language = record.Postal_OA_Language;
				newAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal);
				newAddress.AddressCapability.SetIsNotMainAddress(OrgAddressType.Postal);

				return true;
			}

			#endregion

			#region ChildHumanReadableNameForPlural

			protected override string ChildHumanReadableNameForPluralCore
			{
				get { return Res.GetString("f4fb4fad-8336-4562-ac70-e932b9efe1e8", "Postal Addresses"); }
			}

			#endregion
		}

		internal class ChildDeliveryAddressMerger : FlattenedToUniqueChildrenMerger<OrgHeader, OrgFlattened>
		{
			public ChildDeliveryAddressMerger(ImportCollectionInfoImplForOrgFlattened importCollectionInfo)
				: base(importCollectionInfo.GetChildImportPropertyInfos(ImportCollectionInfoImplForOrgFlattened.ChildProperty.DeliveryAddress).Select(property => property.MappingName))
			{
			}

			#region Import

			protected override bool ContainsChildRecord(ReadonlyImportValuesDictionary propertyValues)
			{
				return !string.IsNullOrEmpty((ZString)propertyValues[OrgFlattened.Schema.Delivery_OA_Address1]);
			}

			protected override bool CreateChildRecord(OrgHeader parentOrg, OrgFlattened record)
			{
				var newAddress = parentOrg.Addresses.AddNew();
				newAddress.OA_Address1 = record.Delivery_OA_Address1;
				newAddress.OA_Address2 = record.Delivery_OA_Address2;
				newAddress.OA_City = record.Delivery_OA_City;
				newAddress.OA_State = record.Delivery_OA_State;
				newAddress.OA_PostCode = record.Delivery_OA_PostCode;
				newAddress.OA_Language = record.Delivery_OA_Language;
				newAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
				newAddress.AddressCapability.SetIsNotMainAddress(OrgAddressType.Delivery);

				return true;
			}

			#endregion

			#region ChildHumanReadableNameForPlural

			protected override string ChildHumanReadableNameForPluralCore
			{
				get { return Res.GetString("cb1cf885-813c-4eea-9dbf-94ec465945df", "Delivery Addresses"); }
			}

			#endregion

		}

		internal class ChildContactMerger : FlattenedToUniqueChildrenMerger<OrgHeader, OrgFlattened>
		{
			public ChildContactMerger(ImportCollectionInfoImplForOrgFlattened importCollectionInfo)
				: base(importCollectionInfo.GetChildImportPropertyInfos(ImportCollectionInfoImplForOrgFlattened.ChildProperty.Contact).Select(property => property.MappingName))
			{
			}

			#region Import

			protected override bool ContainsChildRecord(ReadonlyImportValuesDictionary propertyValues)
			{
				return !string.IsNullOrEmpty((ZString)propertyValues[OrgFlattened.Schema.OC_ContactName]);
			}

			protected override bool CreateChildRecord(OrgHeader parentOrg, OrgFlattened record)
			{
				if (parentOrg.Contacts.Any(contact => ((OrgContact)contact).OC_ContactName.Equals(record.OC_ContactName)))
				{
					AddErrorLog(parentOrg, record, Res.GetString("e95d2229-55f1-4f67-b463-413891d10201", "Can not have multiple organization contacts with the same Contact Name"));
					return false;
				}

				var newContact = parentOrg.Contacts.AddNew();
				newContact.OC_ContactName = record.OC_ContactName;
				newContact.OC_Title = record.OC_Title;
				newContact.OC_Email = record.OC_Email;
				newContact.OC_NotifyMode = string.IsNullOrEmpty(newContact.OC_Email) ? Constants.ContactNotifyModes.Print : Constants.ContactNotifyModes.Email;
				newContact.OC_Phone = record.OC_Phone;
				newContact.OC_Mobile = record.OC_Mobile;
				newContact.OC_Fax = record.OC_Fax;
				newContact.OC_ContactSource = record.OC_ContactSource;
				newContact.OC_DetailsVerified = record.OC_DetailsVerified;
				newContact.OC_Salutation = record.OC_Salutation;

				return true;
			}

			void AddErrorLog(OrgHeader parent, OrgFlattened orgFlattened, string reason)
			{
				Log(Res.GetString("21e08647-510b-4bab-abc9-fb920c79b583", "Contact [Name: {0}] for parent Organization [Code: {1}, Name: {2}] excluded: {3}", orgFlattened.OC_ContactName, parent.OH_Code, parent.OH_FullName, reason));
			}

			#endregion

			#region ChildHumanReadableNameForPlural

			protected override string ChildHumanReadableNameForPluralCore
			{
				get { return Res.GetString("0556635b-c1de-4025-84f3-fbfcdd4b8bce", "Contacts"); }
			}

			#endregion
		}
	}
}
