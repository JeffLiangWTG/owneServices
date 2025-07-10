using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(JobDeclarationModule))]
	sealed class JobDeclarationModuleTest : Customs.Module.Testing.JobDeclarationModuleAbstractTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Taiwan;
		protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);
		protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);
		protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);
		protected override Type GetExpectedImportJobDeclarationType_ForCreateAndImportDeclarationFromAnotherDeclaration() => typeof(Business.ImportJobDeclaration);
		protected override BaseJobDeclaration CreateDeclarationForFetchHintTest(BusinessObjectFactory factory, string messageType, int i)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = Guid.NewGuid().ToString().Substring(0, 6);
			var org2Code = orgHeader.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "00612348", Core.Constants.CountryCodes.Taiwan);
			var warehouseAddress = orgHeader.Addresses.AddNew();
			warehouseAddress.Address1 = "Address1";
			warehouseAddress.Address2 = "Address2";
			org2Code.OK_OA_PremisesAddress = orgHeader.MainAddress.PK;
			Factory.Save();
			var result = (JobDeclaration)base.CreateDeclarationForFetchHintTest(factory, messageType, i);
			result.JE_CustomsOffice = "AA";
			var entryInstruction = result.CusEntryInstruction;
			entryInstruction.CEI_Style = "B1";
			entryInstruction.CEI_CustomsOffice = "AA";
			entryInstruction.CEI_OA_Warehouse2 = orgHeader.MainAddress.PK;
			foreach (Business.CusEntryHeader entryHeader in result.CustomsEntryHeaders)
			{
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				entryHeader.CusEntryNumber.CE_EntryStatus = "C1";
				entryHeader.CH_EntryReleaseDate = new DateTime(2022, 1, 1);

				var cusDispositions = entryHeader.CusDispositions;
				var dispositionRFM = cusDispositions.AddNew();
				dispositionRFM.CDI_StatusDate = new DateTime(2022, 1, 1);
				dispositionRFM.CDI_Type = "CUS";
				dispositionRFM.CDI_StatusKey = "RFM";
				dispositionRFM.CDI_Status = "A01";

				var dispositionARM = entryHeader.CusDispositions.AddNew();
				dispositionARM.CDI_StatusDate = new DateTime(2022, 1, 2);
				dispositionARM.CDI_Type = "CUS";
				dispositionARM.CDI_StatusKey = "ARM";
				dispositionARM.CDI_Status = "A02";

				var dispositionCLR = entryHeader.CusDispositions.AddNew();
				dispositionCLR.CDI_StatusDate = new DateTime(2022, 1, 3);
				dispositionCLR.CDI_Type = "CUS";
				dispositionCLR.CDI_StatusKey = "CLR";
				dispositionCLR.CDI_Status = "1";
			}
			var zhTWtranslatedAddress1 = result.Importer.MainAddress.TranslatedAddresses.FirstOrDefault(c => c.OTA_Language == Core.SharedConstants.Languages.ChineseTraditional);
			if (zhTWtranslatedAddress1 == null)
			{
				zhTWtranslatedAddress1 = result.Importer.MainAddress.TranslatedAddresses.AddNew();
				zhTWtranslatedAddress1.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			}
			zhTWtranslatedAddress1.Address1 = $"Importer地址{i}-1";
			zhTWtranslatedAddress1.Address2 = $"Importer地址{i}-2";
			zhTWtranslatedAddress1.OTA_CompanyName = $"Importer名稱{i}";

			var zhTWtranslatedAddress2 = result.Supplier.MainAddress.TranslatedAddresses.FirstOrDefault(c => c.OTA_Language == Core.SharedConstants.Languages.ChineseTraditional);
			if (zhTWtranslatedAddress2 == null)
			{
				zhTWtranslatedAddress2 = result.Supplier.MainAddress.TranslatedAddresses.AddNew();
				zhTWtranslatedAddress2.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			}
			zhTWtranslatedAddress2.Address1 = $"Supplier地址{i}-1";
			zhTWtranslatedAddress2.Address2 = $"Supplier地址{i}-2";
			zhTWtranslatedAddress2.OTA_CompanyName = $"Supplier名稱{i}";

			return result;
		}
	}
}
