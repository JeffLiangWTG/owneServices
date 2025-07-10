using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	class ImporterForTesting
	{
		public ImporterForTesting(BusinessObjectFactory factory)
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
			Declaration.JE_HouseBill = "house";
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = Customs.Business.JobMessageTypeList.Codes.Import;
			Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			entryInstruction = Declaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			invoiceHeader = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			new LineMerger(Declaration).DoMerge();
			Declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			EntryHeader = Declaration.ActiveEntryHeaders[0];
			EntryHeader.EntryNumber = "BEB80860000003";
			EntryHeader.CH_Status = "AWO";
			var entryLine = EntryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
		}

		OrgHeader organization1;
		OrgCusCode vATCusCode;
		OrgCusCode cCPCusCode;
		OrgCusCode pASCusCode;
		OrgCusCode pIDCusCode;
		OrgCusCode cBFCusCode;
		public void SetUpOrganization()
		{
			organization1 = Factory.New<OrgHeader>();
			organization1.Addresses.RemoveAndDeleteAll();
			organization1.OH_Code = "Org1";
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
			zhTWtranslatedAddress1.OTA_CompanyName = "NX5105 TW OVERRIDEN COMPANY NAME";
			zhTWtranslatedAddress1.OTA_Address1 = "NX5105 TW OTA ADDRESS 1";
			zhTWtranslatedAddress1.OTA_Address2 = "TW OTA ADDRESS 2";
			organization1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.TPC, "TPCREGNO", "TW");
			organization1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.AEO, "987654321", "TW");
			vATCusCode = organization1.CustomsCodes.AddNew();
			vATCusCode.OK_RN_NKCodeCountry = "TW";
			vATCusCode.OK_CodeType = "VAT";
			vATCusCode.OK_CustomsRegNo = "969444490";
			cCPCusCode = address1.CustomsCodes.AddNew();
			cCPCusCode.OK_RN_NKCodeCountry = "TW";
			cCPCusCode.OK_CodeType = "CCP";
			cCPCusCode.OK_CustomsRegNo = "987654321";
			pASCusCode = organization1.CustomsCodes.AddNew();
			pASCusCode.OK_RN_NKCodeCountry = "TW";
			pASCusCode.OK_CodeType = "PAS";
			pASCusCode.OK_CustomsRegNo = "PASREGNO";
			pIDCusCode = organization1.CustomsCodes.AddNew();
			pIDCusCode.OK_RN_NKCodeCountry = "TW";
			pIDCusCode.OK_CodeType = "PID";
			pIDCusCode.OK_CustomsRegNo = "PIDREGNO";
			cBFCusCode = address1.CustomsCodes.AddNew();
			cBFCusCode.OK_RN_NKCodeCountry = "TW";
			cBFCusCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.CBF;
			cBFCusCode.OK_CustomsRegNo = "22233";
			Factory.Save();
			Declaration.JE_OH_Importer = organization1.PK;
			entryInstruction.CEI_OA_Warehouse = organization1.MainAddress.PK;
			entryInstruction.CEI_OA_Warehouse2 = organization1.MainAddress.PK;
			entryInstruction.CEI_BoxNumber = "660";
			Declaration.JE_OA_DeclarantAddress = organization1.MainAddress.PK;
		}
	}
}
