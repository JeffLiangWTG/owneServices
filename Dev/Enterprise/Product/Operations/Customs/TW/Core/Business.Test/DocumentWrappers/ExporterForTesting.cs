using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	class ExporterForTesting
	{
		public ExporterForTesting(BusinessObjectFactory factory)
		{
			Factory = factory;
			SetUpDeclaration();
		}

		public CusEntryHeader EntryHeader;
		public JobDeclaration Declaration;
		JobComInvoiceHeader invoiceHeader;
		CusEntryInstruction entryInstruction;
		BusinessObjectFactory Factory
		{
			get;
		}

		void SetUpDeclaration()
		{
			Declaration = Factory.New<JobDeclaration>();
			Declaration.JE_HouseBill = "N5203 house";
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = Customs.Business.JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			entryInstruction = Declaration.CusEntryInstruction;
			invoiceHeader = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			new LineMerger(Declaration).DoMerge();
			Declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			EntryHeader = Declaration.ActiveEntryHeaders[0];
			EntryHeader.EntryNumber = "AEB80860000003";
			EntryHeader.CH_Status = "AWO";
			var entryLine = EntryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
		}

		OrgHeader organization1;
		OrgCusCode vATCusCode;
		OrgCusCode cCPCusCode;
		OrgCusCode pASCusCode;
		OrgCusCode pIDCusCode;
		public void SetUpOrganization()
		{
			organization1 = Factory.New<OrgHeader>();
			organization1.Addresses.RemoveAndDeleteAll();
			organization1.OH_Code = "OrgN5203";
			organization1.OH_RL_NKClosestPort = "TW";
			var contact = organization1.Contacts.AddNew();
			contact.OC_ContactName = "Contact Name";
			var address1 = organization1.Addresses[0];
			address1.OA_RN_NKCountryCode = "TW";
			address1.OA_CompanyNameOverride = "OVERRIDEN COMPANY NAME";
			address1.OA_Language = Core.SharedConstants.Languages.English;
			address1.OA_Address1 = "ADDRESS 1";
			address1.OA_Address2 = "ADDRESS 2";
			address1.OA_Phone = "PHONE";
			address1.OA_Email = "EMAIL";
			var entranslatedAddress1 = address1.TranslatedAddresses.AddNew();
			entranslatedAddress1.OTA_Language = Core.SharedConstants.Languages.English;
			entranslatedAddress1.OTA_Address1 = "EN OTA ADDRESS 1";
			entranslatedAddress1.OTA_Address2 = "EN OTA ADDRESS 2";
			var zhTWtranslatedAddress1 = address1.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress1.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			zhTWtranslatedAddress1.OTA_CompanyName = "TW OVERRIDEN COMPANY NAME";
			zhTWtranslatedAddress1.OTA_Address1 = "N5203 TW OTA ADDRESS 1";
			zhTWtranslatedAddress1.OTA_Address2 = "TW OTA ADDRESS 2";
			organization1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("TPC", "TPCREGNO", "TW");
			organization1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("AEO", "123465789", "TW");
			vATCusCode = organization1.CustomsCodes.AddNew();
			vATCusCode.OK_RN_NKCodeCountry = "TW";
			vATCusCode.OK_CodeType = "VAT";
			vATCusCode.OK_CustomsRegNo = "123465789";
			cCPCusCode = address1.CustomsCodes.AddNew();
			cCPCusCode.OK_RN_NKCodeCountry = "TW";
			cCPCusCode.OK_CodeType = "CCP";
			cCPCusCode.OK_CustomsRegNo = "987654321";
			cCPCusCode.OK_OA_PremisesAddress = address1.PK;
			pASCusCode = organization1.CustomsCodes.AddNew();
			pASCusCode.OK_RN_NKCodeCountry = "TW";
			pASCusCode.OK_CodeType = "PAS";
			pASCusCode.OK_CustomsRegNo = "PASREGNO";
			pIDCusCode = organization1.CustomsCodes.AddNew();
			pIDCusCode.OK_RN_NKCodeCountry = "TW";
			pIDCusCode.OK_CodeType = "PID";
			pIDCusCode.OK_CustomsRegNo = "PIDREGNO";
			entryInstruction.CEI_BoxNumber = "660";
			Factory.Save();
			organization1.MainAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.FTZ, "BAA123", Core.Constants.CountryCodes.Taiwan);
			Declaration.JE_OA_DeclarantAddress = organization1.MainAddress.PK;
			Declaration.SupplierDocumentaryAddress.E2_OA_Address = organization1.MainAddress.PK;
		}
	}
}
