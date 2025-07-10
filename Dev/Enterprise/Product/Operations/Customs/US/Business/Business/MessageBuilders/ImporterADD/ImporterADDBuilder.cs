using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public interface IUSMessageBuilder
	{
		void Generate();
	}

	public class ImporterADDBuilder : IUSMessageBuilder
	{
		const string ForeignNationExceptCanada = "FN";
		public ImporterADDBuilder(OrgAddressMessageData messageData)
		{
			this.messageData = messageData;

			var portCode = ProcessingPortCodeAndFilerFinder.GetProcessingPortCodeFromRegistry(GlbBranch.CurrentBranch);
			var officeCode = USCustomsDataRegistry.Instance.BRecordOfficeCode.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			block = new ACEInputBlockControlGenerator(GlbCompany.CurrentCompany.PK.ToGuid(), portCode, officeCode);
		}

		readonly OrgAddressMessageData messageData;

		readonly BlockControlGenerator block;

		public MQEDIMessage Generate()
		{
			block.B.ApplicationIdentifier = ACEApplicationIdentifierCodeList.Codes.AddNewCBPFormCBPF5106DatatotheImporterFile;
			block.AddMessageBlock(GenerateT1());

			var ta = GenerateTA();
			if (ta != null)
			{
				block.AddMessageBlock(ta);
			}

			block.AddMessageBlock(GenerateT2());
			block.AddMessageBlocks(GenerateT3());

			var tb = GenerateTB();
			if (tb != null)
			{
				block.AddMessageBlock(tb);
			}

			var tc = GenerateTC();
			if (tc != null)
			{
				block.AddMessageBlock(tc);
			}

			var td = GenerateTD();
			if (td != null)
			{
				block.AddMessageBlock(td);
			}

			var te = GenerateTE();
			if (te != null)
			{
				block.AddMessageBlock(te);
			}

			block.AddMessageBlocks(GenerateTF());

			var tg = GenerateTG();
			if (tg != null)
			{
				block.AddMessageBlock(tg);
			}

			block.AddMessageBlocks(GenerateTH());
			block.AddMessageBlocks(GeneratePII());
			block.AddMessageBlocks(GenerateTK());
			block.AddMessageBlocks(GenerateTL());
			block.AddMessageBlocks(GenerateTM());

			var message = block.CreateMessage<MQEDIMessage>(messageData.Factory);
			messageData.wrapper.Messages.Add(message);
			message.EM_LinkedObject = messageData.wrapper.organisation;

			return message;
		}

		ADDT1 GenerateT1()
		{
			var t1 = new ADDT1();
			t1.UpdateActionCode = messageData.US_ActionCode;
			t1.ImporterNumber = messageData.US_ImporterNumber;
			t1.LineOneOfTheImporterName = messageData.US_ImporterName.Left(32);
			t1.LineOneOfTheMailingAddress = messageData.US_LineOneAddress1;
			t1.ImporterType = messageData.US_ImporterType;
			return t1;
		}

		ADDTA GenerateTA()
		{
			ADDTA result = null;

			if (!messageData.US_NameQualifier.IsEmpty || !messageData.US_AlternativeImporterName.IsEmpty)
			{
				result = new ADDTA();
				result.NameQualifier = messageData.US_NameQualifier;
				result.LineTwoOfTheImporterName = messageData.US_AlternativeImporterName;
			}

			return result;
		}

		ADDT2 GenerateT2()
		{
			var t2 = new ADDT2();
			t2.LineTwoOfTheMailingAddress = messageData.US_LineTwoAddress1;
			t2.CityMailingAddress = messageData.US_CityAddress1;
			t2.StateMailingAddress = GetStateCode(messageData.US_StateAddress1, messageData.US_RN_NKCountry1);

			switch (messageData.US_RN_NKCountry1)
			{
				case Core.Constants.CountryCodes.Canada:
					t2.ZIPMailingAddress = FormatCAZipCode(messageData.US_ZipAddress1);
					break;
				default:
					t2.ZIPMailingAddress = messageData.US_ZipAddress1;
					break;
			}
			t2.ISOCountryCodeMailingAddress = messageData.US_RN_NKCountry1;
			return t2;
		}

		ADDTB GenerateTB()
		{
			ADDTB result = null;

			if (messageData.US_OA_Address2.IsValid)
			{
				result = new ADDTB();
				result.LineOneOfTheSecondaryAddress = messageData.US_LineOneAddress2;
				result.LineTwoOfTheSecondaryAddress = messageData.US_LineTwoAddress2;
			}

			return result;
		}

		ADDTC GenerateTC()
		{
			ADDTC result = null;

			if (messageData.US_OA_Address2.IsValid)
			{
				result = new ADDTC();
				result.CitySecondaryAddress = messageData.US_CityAddress2;
				result.StateSecondaryAddress = GetStateCode(messageData.US_StateAddress2, messageData.US_RN_NKCountry2);

				switch (messageData.US_RN_NKCountry2)
				{
					case Core.Constants.CountryCodes.Canada:
						result.ZIPSecondaryAddress = FormatCAZipCode(messageData.US_ZipAddress2);
						break;
					default:
						result.ZIPSecondaryAddress = messageData.US_ZipAddress2;
						break;
				}
				result.ISOCountryCodeSecondaryAddress = messageData.US_RN_NKCountry2;
			}

			return result;
		}

		#region Importer/Consignee Create/Update

		IEnumerable<MessageBlock> GenerateT3()
		{
			if (messageData.US_ImporterName.Length > 32)
			{
				var t3Block = new AICCT3();
				t3Block.FullLegalImporterName = messageData.US_ImporterName.Left(30);
				yield return t3Block;

				var tnBlock = new AICCTN();
				tnBlock.AdditionalInformationQualifierCode = ImporterAdditionalInformationQualifierCodeList.Codes.IN1;
				tnBlock.AdditionalInformation = messageData.US_ImporterName.SubstringSafe(30);
				yield return tnBlock;
			}
		}

		AICCTD GenerateTD()
		{
			AICCTD tdBlock = null;
			tdBlock = new AICCTD();
			tdBlock.NumberOfEntriesPlanningPerYear = messageData.US_NumberOfEntries;
			tdBlock.IdentificationNoUtilizationImporterOfRecord = messageData.US_UtlIORIndicator ? "X" : string.Empty;
			tdBlock.IdentificationNoUtilizationConsignee = messageData.US_UtlConsigneeIndicator ? "X" : string.Empty;
			tdBlock.IdentificationNoUtilizationDrawbackClaimant = messageData.US_UtlDrawbackIndicator ? "X" : string.Empty;
			tdBlock.IdentificationNoUtilizationRefundsBills = messageData.US_UtlRefundsIndicator ? "X" : string.Empty;
			tdBlock.IdentificationNoUtilizationOther = messageData.US_UtlOtherIndicator ? "X" : string.Empty;
			tdBlock.UtilizationOfIdentificationNoDescription = messageData.US_UtlOtherDescription;
			tdBlock.ProgramCode1 = messageData.US_ProgramCode1;
			tdBlock.ProgramCode2 = messageData.US_ProgramCode2;
			tdBlock.ProgramCode3 = messageData.US_ProgramCode3;
			tdBlock.ProgramCode4 = messageData.US_ProgramCode4;
			tdBlock.Phone = messageData.US_ImporterPhoneNumber;
			tdBlock.Extension = messageData.US_PhoneExtension;
			tdBlock.CBPAssignedNumberRequestReasonIndicator = messageData.US_HaveSSNIndicator ? "X" : string.Empty;
			tdBlock.SSNIndicator = messageData.US_NoSSNIndicator ? "X" : string.Empty;
			tdBlock.IRSIndicator = messageData.US_NoIRSIndicator ? "X" : string.Empty;
			tdBlock.IRSOrSSNIndicator = messageData.US_NotAppliedIndicator ? "X" : string.Empty;
			tdBlock.USResidentIndicator = messageData.US_NotResidentIndicator ? "X" : string.Empty;
			return tdBlock;
		}

		AICCTE GenerateTE()
		{
			AICCTE teBlock = null;
			teBlock = new AICCTE();
			teBlock.MailingAddressType = messageData.US_AddressType1;
			teBlock.MailingAddressExplanation = messageData.IsAddressExplanation1Required ? messageData.US_AddressExplanation1 : ZString.Empty;
			teBlock.PhysicalAddressType = messageData.US_AddressType2;
			teBlock.PhysicalAddressExplanation = messageData.IsAddressExplanation2Required ? messageData.US_AddressExplanation2 : ZString.Empty;
			teBlock.BusinessDescription = messageData.US_BusinessDescription;

			return teBlock;
		}

		IEnumerable<MessageBlock> GenerateTF()
		{
			var tfBlock = new AICCTF();
			tfBlock.Email = messageData.US_ImporterEmail.Left(30);
			tfBlock.Website = messageData.US_ImporterWebsite.Left(30);
			tfBlock.Fax = messageData.US_ImporterFaxNumber;
			yield return tfBlock;

			if (messageData.US_ImporterEmail.Length > 30)
			{
				var overflowEmailBlock = new AICCTN();
				overflowEmailBlock.AdditionalInformationQualifierCode = ImporterAdditionalInformationQualifierCodeList.Codes.CE1;
				overflowEmailBlock.AdditionalInformation = messageData.US_ImporterEmail.SubstringSafe(30);
				yield return overflowEmailBlock;
			}

			if (messageData.US_ImporterWebsite.Length > 30)
			{
				var overflowWebsiteBlock = new AICCTN();
				overflowWebsiteBlock.AdditionalInformationQualifierCode = ImporterAdditionalInformationQualifierCodeList.Codes.CW1;
				overflowWebsiteBlock.AdditionalInformation = messageData.US_ImporterWebsite.SubstringSafe(30);
				yield return overflowWebsiteBlock;
			}
		}

		AICCTG GenerateTG()
		{
			AICCTG tgBlock = null;
			if (!messageData.US_NAICSCode.IsEmpty || !messageData.US_DUNS.IsEmpty || !messageData.US_FilerCode.IsEmpty || !messageData.US_YearEstablished.IsEmpty || !messageData.US_StateCode.IsEmpty || !messageData.US_CountryISOCode.IsEmpty || !messageData.US_CertificateReference.IsEmpty)
			{
				tgBlock = new AICCTG();
				tgBlock.NAICSCode = messageData.US_NAICSCode;
				tgBlock.DUNS = messageData.US_DUNS;
				tgBlock.FilerCode = messageData.US_FilerCode;
				tgBlock.YearEstablished = messageData.US_YearEstablished;
				tgBlock.State = messageData.US_StateCode;
				tgBlock.CountryISOCode = messageData.US_CountryISOCode;
				tgBlock.Reference = messageData.US_CertificateReference;
			}
			return tgBlock;
		}

		IEnumerable<MessageBlock> GenerateTH()
		{
			if (!messageData.US_BankName.IsEmpty || !messageData.US_BankRoutingNo.IsEmpty || !messageData.US_BankCity.IsEmpty || !messageData.US_BankState.IsEmpty || !messageData.US_BankCountry.IsEmpty)
			{
				var thBlock = new AICCTH();
				thBlock.PrimaryBank = messageData.US_BankName.Left(30);
				thBlock.Routing = messageData.US_BankRoutingNo;
				thBlock.City = messageData.US_BankCity.Left(30);
				thBlock.State = messageData.US_BankState;
				thBlock.CountryISOCode = messageData.US_BankCountry;
				yield return thBlock;

				if (messageData.US_BankName.Length > 30)
				{
					var nameOverflowBlock = new AICCTN();
					nameOverflowBlock.AdditionalInformationQualifierCode = ImporterAdditionalInformationQualifierCodeList.Codes.BN1;
					nameOverflowBlock.AdditionalInformation = messageData.US_BankName.SubstringSafe(30);
					yield return nameOverflowBlock;
				}

				if (messageData.US_BankCity.Length > 30)
				{
					var cityOverflowBlock = new AICCTN();
					cityOverflowBlock.AdditionalInformationQualifierCode = ImporterAdditionalInformationQualifierCodeList.Codes.BC1;
					cityOverflowBlock.AdditionalInformation = messageData.US_BankCity.SubstringSafe(30);
					yield return cityOverflowBlock;
				}
			}
		}

		IEnumerable<MessageBlock> GeneratePII()
		{
			foreach (PersonIdentityInformationData pii in messageData.PIIs)
			{
				var tiBlock = new AICCTI();
				tiBlock.LineItem = pii.US_LineNo;
				tiBlock.Name = pii.US_Name.Left(30);
				tiBlock.Title = pii.US_Title;
				tiBlock.SSN = pii.US_SSN.KeepAlphanumericCharacters().Left(9);
				yield return tiBlock;

				if (pii.US_Name.Length > 30)
				{
					var nameOverflowBlock = new AICCTN();
					nameOverflowBlock.AdditionalInformationQualifierCode = ImporterAdditionalInformationQualifierCodeList.Codes.CN1;
					nameOverflowBlock.AdditionalInformation = pii.US_Name.SubstringSafe(30);
					yield return nameOverflowBlock;
				}

				var tjBlock = new AICCTJ();
				tjBlock.LineItem = pii.US_LineNo;
				tjBlock.Passport = pii.US_PassportNo;
				tjBlock.ExpirationDate = pii.US_ExpirationDate.Date;
				tjBlock.CountryOfIssuance = pii.US_CountryOfIssuance;
				tjBlock.PassportType = pii.US_PassportType;
				tjBlock.Phone = pii.US_PhoneNumber;
				tjBlock.Extension = pii.US_Extension;
				tjBlock.Email = pii.US_Email.Left(30);
				yield return tjBlock;

				if (pii.US_Email.Length > 30)
				{
					var emailOverflowBlock = new AICCTN();
					emailOverflowBlock.AdditionalInformationQualifierCode = ImporterAdditionalInformationQualifierCodeList.Codes.CE2;
					emailOverflowBlock.AdditionalInformation = pii.US_Email.SubstringSafe(30);
					yield return emailOverflowBlock;
				}
			}
		}

		IEnumerable<MessageBlock> GenerateTK()
		{
			foreach (RelatedBusinessData relatedBusiness in messageData.RelatedBusinessItems)
			{
				var tkBlock = new AICCTK();
				tkBlock.LineItem = relatedBusiness.US_LineNo;
				tkBlock.RelatedBusiness = relatedBusiness.US_RelatedBusiness;
				tkBlock.NameOfTheEntity = relatedBusiness.US_NameOfEntity.Left(30);
				tkBlock.TINEINSSNCBPAssigned = relatedBusiness.US_Number;
				yield return tkBlock;

				if (relatedBusiness.US_NameOfEntity.Length > 30)
				{
					var nameOverflowBlock = new AICCTN();
					nameOverflowBlock.AdditionalInformationQualifierCode = ImporterAdditionalInformationQualifierCodeList.Codes.NE1;
					nameOverflowBlock.AdditionalInformation = relatedBusiness.US_NameOfEntity.SubstringSafe(30);
					yield return nameOverflowBlock;
				}
			}
		}

		IEnumerable<MessageBlock> GenerateTL()
		{
			var tlBlock = new AICCTL();
			tlBlock.ElectronicSignature = messageData.US_AcknowledgeAndSign ? "X" : string.Empty;
			tlBlock.CertifyingIndividualFullName = messageData.US_IndividualName.Left(30);
			tlBlock.Title = messageData.US_IndividualTitle;
			yield return tlBlock;

			if (messageData.US_IndividualName.Length > 30)
			{
				var overflowNameBlock = new AICCTN();
				overflowNameBlock.AdditionalInformationQualifierCode = ImporterAdditionalInformationQualifierCodeList.Codes.IN2;
				overflowNameBlock.AdditionalInformation = messageData.US_IndividualName.SubstringSafe(30);
				yield return overflowNameBlock;
			}
		}

		IEnumerable<MessageBlock> GenerateTM()
		{
			if (!messageData.US_BrokerName.IsEmpty || !messageData.US_IndividualPhone.IsEmpty || !messageData.US_BrokerPhone.IsEmpty)
			{
				var tmBlock = new AICCTM();
				tmBlock.BrokersName = messageData.US_BrokerName.Left(30);
				tmBlock.CertifyingIndividualsPhone = messageData.US_IndividualPhone;
				tmBlock.BrokersPhone = messageData.US_BrokerPhone;
				yield return tmBlock;

				if (messageData.US_BrokerName.Length > 30)
				{
					var tnBlock = new AICCTN();
					tnBlock.AdditionalInformationQualifierCode = ImporterAdditionalInformationQualifierCodeList.Codes.BN2;
					tnBlock.AdditionalInformation = messageData.US_BrokerName.SubstringSafe(30);
					yield return tnBlock;
				}
			}
		}

		#endregion

		ZString GetStateCode(ZString realState, ZString us_RN_NKCountry)
		{
			if (messageData.IsUSImporter(us_RN_NKCountry) || messageData.IsCAImporter(us_RN_NKCountry) || messageData.IsMXImporter(us_RN_NKCountry) || !realState.IsEmpty)
			{
				return realState;
			}
			else
			{
				return ForeignNationExceptCanada;
			}
		}

		string FormatCAZipCode(string zip)
		{
			return Regex.Replace(zip, @"^([A-Z][0-9][A-Z])([0-9][A-Z][0-9])$", "$1 $2");
		}

		#region IUSMessageBuilder Members

		void IUSMessageBuilder.Generate()
		{
			Generate();
		}

		#endregion
	}
}
