using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.TW.Business.Testing.TWJobDocAddressTest;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ManufacturerAddressRequirement))]
	sealed class ManufacturerAddressRequirementTest : TestCaseWithFactory
	{
		public void TestCheckE2_CompanyName()
		{
			var message = "You have not entered a Manufacturer Company Name.";
			var messageForeign = "You have not entered a Foreign Manufacturer Company Name.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoicelineLinkCMHeader = invoiceLine.InvoiceLineLinkControllingMsgHeaders[0];
			invoicelineLinkCMHeader.IsLinkedCMHeader = true;
			var manufacturerDocAddress = invoiceLine.ManufacturerDocAddress;
			var targetInfoOverrided = manufacturerDocAddress.E2_CompanyNameInfo;
			var targetInfoNotOverrided = manufacturerDocAddress.OrganisationPKInfo;
			manufacturerDocAddress.E2_AddressOverride = true;
			var validation = manufacturerDocAddress.Validation;

			CombineAssertions(() =>
			{
				foreach (var messageType in new ControllingMessageTypeList().GetAllCodes())
				{
					cMHeader.TW1_ControllingMessageType = messageType;
					if (messageType == ControllingMessageTypeList.Codes.NX101)
					{
						declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
						validation.ValidateE2_CompanyName();
						AssertNoMessageError("Not validate for NX101 when import", targetInfoOverrided, message);

						declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
						validation.ValidateE2_CompanyName();
						AssertHasMessageError("Both Company Name and Local Company Name are empty", targetInfoOverrided, message);

						var manufactureIDTypes = new ZString[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID, OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber };
						var localProcessorAddress = cMHeader.LocalProcessorAddress;
						localProcessorAddress.E2_AddressOverride = true;
						foreach (var codeTypes in manufactureIDTypes)
						{
							localProcessorAddress.IDCodeType = codeTypes;
							localProcessorAddress.IDCode = "123456789";
							manufacturerDocAddress.IDCodeType = codeTypes;
							manufacturerDocAddress.IDCode = "123456789";
							validation.ValidateE2_CompanyName();
							AssertNoMessageError("ID Same LocalProcessAddress", targetInfoOverrided, message);

							manufacturerDocAddress.IDCode = "99999999";
							validation.ValidateE2_CompanyName();
							AssertHasMessageError("ID different LocalProcessAddress", targetInfoOverrided, message);

							manufacturerDocAddress.IDCodeType = Constants.OrgCusCodeType.CustomCode;
							manufacturerDocAddress.IDCode = "123456789";
							validation.ValidateE2_CompanyName();
							AssertHasMessageError("ID Type different LocalProcessAddress", targetInfoOverrided, message);

							manufacturerDocAddress.FRICodeType = codeTypes;
							manufacturerDocAddress.FRICode = "123456789";
							validation.ValidateE2_CompanyName();
							AssertNoMessageError("FRI Same LocalProcessAddress", targetInfoOverrided, message);

							manufacturerDocAddress.FRICode = "99999999";
							validation.ValidateE2_CompanyName();
							AssertHasMessageError("FRI different LocalProcessAddress", targetInfoOverrided, message);

							manufacturerDocAddress.FRICodeType = Constants.OrgCusCodeType.CustomCode;
							manufacturerDocAddress.FRICode = "123456789";
							validation.ValidateE2_CompanyName();
							AssertHasMessageError("FRI Type different LocalProcessAddress", targetInfoOverrided, message);
						}

						manufacturerDocAddress.E2_CompanyName = "Company Name";
						validation.ValidateE2_CompanyName();
						AssertNoMessageError("Company Name has value, Local Company Name is empty", targetInfoOverrided, message);

						manufacturerDocAddress.E2_CompanyName = ZString.Empty;
						manufacturerDocAddress.LocalAddress.E2_CompanyName = "公司名稱";
						validation.ValidateE2_CompanyName();
						AssertNoMessageError("Company Name is empty, Local Company Name has value", targetInfoOverrided, message);
					}
					else if (messageType == ControllingMessageTypeList.Codes.NX301 ||
						messageType == ControllingMessageTypeList.Codes.NX301_AX ||
						messageType == ControllingMessageTypeList.Codes.NX301_DN ||
						messageType == ControllingMessageTypeList.Codes.NX601 ||
						messageType == ControllingMessageTypeList.Codes.NX603)
					{
						manufacturerDocAddress.E2_AddressOverride = false;
						manufacturerDocAddress.OrganisationPK = ZGuid.Empty;
						declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
						validation.ValidateOrganisationPK();
						AssertNoMessageError($"{messageType}: Not validate when export", targetInfoNotOverrided, message);

						declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
						validation.ValidateOrganisationPK();
						AssertHasMessageError($"{messageType}: Organisation is empty and not overrided", targetInfoNotOverrided, messageForeign);

						manufacturerDocAddress.E2_AddressOverride = true;
						validation.ValidateE2_CompanyName();
						AssertHasMessageError($"{messageType}: Company Name is empty and overrided", targetInfoOverrided, messageForeign);

						manufacturerDocAddress.E2_CompanyName = "Company Name";
						manufacturerDocAddress.LocalAddress.E2_CompanyName = "公司名稱";
						validation.ValidateE2_CompanyName();
						AssertNoMessageError($"{messageType}: Company Name is 'Company Name'", targetInfoOverrided, messageForeign);
					}
					else
					{
						manufacturerDocAddress.E2_CompanyName = ZString.Empty;
						validation.ValidateE2_CompanyName();
						AssertNoMessageError($"{messageType}: not validate Company Name", targetInfoOverrided, message);
						AssertNoMessageError($"{messageType}: not validate Company Name", targetInfoOverrided, messageForeign);
					}
				}
			});
		}

		public void TestCheckE2_Address1()
		{
			var message = "You have not entered a Manufacturer Address.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoicelineLinkCMHeader = invoiceLine.InvoiceLineLinkControllingMsgHeaders[0];
			invoicelineLinkCMHeader.IsLinkedCMHeader = true;
			var manufacturerDocAddress = invoiceLine.ManufacturerDocAddress;
			var targetInfo = manufacturerDocAddress.E2_Address1Info;
			manufacturerDocAddress.E2_AddressOverride = true;

			var validation = manufacturerDocAddress.Validation;
			manufacturerDocAddress.E2_CompanyName = ZString.Empty;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			validation.ValidateE2_Address1();
			AssertNoMessageError("Not validate when Import", targetInfo, message);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			validation.ValidateE2_Address1();
			AssertHasMessageError("Both Address and Local Address are empty", targetInfo, message);

			var manufactureIDTypes = new ZString[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID, OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber };
			var localProcessorAddress = cMHeader.LocalProcessorAddress;
			localProcessorAddress.E2_AddressOverride = true;
			foreach (var codeTypes in manufactureIDTypes)
			{
				localProcessorAddress.IDCodeType = codeTypes;
				localProcessorAddress.IDCode = "123456789";
				manufacturerDocAddress.IDCodeType = codeTypes;
				manufacturerDocAddress.IDCode = "123456789";
				validation.ValidateE2_Address1();
				AssertNoMessageError("ID Same LocalProcessAddress", targetInfo, message);

				manufacturerDocAddress.IDCode = "99999999";
				validation.ValidateE2_Address1();
				AssertHasMessageError("ID different LocalProcessAddress", targetInfo, message);

				manufacturerDocAddress.IDCodeType = Constants.OrgCusCodeType.CustomCode;
				manufacturerDocAddress.IDCode = "123456789";
				validation.ValidateE2_Address1();
				AssertHasMessageError("ID Type different LocalProcessAddress", targetInfo, message);

				manufacturerDocAddress.FRICodeType = codeTypes;
				manufacturerDocAddress.FRICode = "123456789";
				validation.ValidateE2_Address1();
				AssertNoMessageError("FRI Same LocalProcessAddress", targetInfo, message);

				manufacturerDocAddress.FRICode = "99999999";
				validation.ValidateE2_Address1();
				AssertHasMessageError("FRI different LocalProcessAddress", targetInfo, message);

				manufacturerDocAddress.FRICodeType = Constants.OrgCusCodeType.CustomCode;
				manufacturerDocAddress.FRICode = "123456789";
				validation.ValidateE2_Address1();
				AssertHasMessageError("FRI Type different LocalProcessAddress", targetInfo, message);
			}

			manufacturerDocAddress.E2_Address1 = "Company Address";
			validation.ValidateE2_Address1();
			AssertNoMessageError("Address has value, Local Address is empty", targetInfo, message);

			manufacturerDocAddress.E2_Address1 = ZString.Empty;
			manufacturerDocAddress.LocalAddress.E2_Address1 = "公司地址";
			validation.ValidateE2_Address1();
			AssertNoMessageError("Address is empty, Local Address has value", targetInfo, message);

			manufacturerDocAddress.LocalAddress.E2_Address1 = ZString.Empty;
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			validation.ValidateE2_Address1();
			AssertNoMessageError("ControllingMessageType is not NX101, both Address and Local Address are empty", targetInfo, message);
		}

		public void TestCheckManufacturerContact()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoicelineLinkCMHeader = invoiceLine.InvoiceLineLinkControllingMsgHeaders[0];
			invoicelineLinkCMHeader.IsLinkedCMHeader = true;
			var manufacturerDocAddress = invoiceLine.ManufacturerDocAddress;
			manufacturerDocAddress.E2_AddressOverride = true;
			var notification = "You have not entered a Manufacturer Contact Information.";
			var validation = manufacturerDocAddress.Validation;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertContactHasMessageError("Not validate when Import", false);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertContactHasMessageError("Email, Phone, Fax are all empty.", true);

			var manufactureIDTypes = new ZString[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID, OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber };
			var localProcessorAddress = cMHeader.LocalProcessorAddress;
			localProcessorAddress.E2_AddressOverride = true;
			foreach (var codeTypes in manufactureIDTypes)
			{
				localProcessorAddress.IDCodeType = codeTypes;
				localProcessorAddress.IDCode = "123456789";
				manufacturerDocAddress.IDCodeType = codeTypes;
				manufacturerDocAddress.IDCode = "123456789";
				AssertContactHasMessageError("ID Same LocalProcessAddress", false);

				manufacturerDocAddress.IDCode = "99999999";
				AssertContactHasMessageError("ID different LocalProcessAddress", true);

				manufacturerDocAddress.IDCodeType = Constants.OrgCusCodeType.CustomCode;
				manufacturerDocAddress.IDCode = "123456789";
				AssertContactHasMessageError("ID Type different LocalProcessAddress", true);

				manufacturerDocAddress.FRICodeType = codeTypes;
				manufacturerDocAddress.FRICode = "123456789";
				AssertContactHasMessageError("FRI Same LocalProcessAddress", false);

				manufacturerDocAddress.FRICode = "99999999";
				AssertContactHasMessageError("FRI different LocalProcessAddress", true);

				manufacturerDocAddress.FRICodeType = Constants.OrgCusCodeType.CustomCode;
				manufacturerDocAddress.FRICode = "123456789";
				AssertContactHasMessageError("FRI Type different LocalProcessAddress", true);
			}

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			AssertContactHasMessageError("ControllingMessageType is not NX101, Email, Phone, Fax are all empty.", false);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			AssertContactHasMessageError("CertificateTyp is not 15, Email, Phone, Fax are all empty.", false);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			manufacturerDocAddress.E2_Email = "user@wtg.com";
			AssertContactHasMessageError("Email has value.", false);

			manufacturerDocAddress.E2_Email = ZString.Empty;
			manufacturerDocAddress.E2_Phone = "+88621234567";
			AssertContactHasMessageError("Phone has value.", false);

			manufacturerDocAddress.E2_Phone = ZString.Empty;
			manufacturerDocAddress.E2_Fax = "+88621234567";
			AssertContactHasMessageError("Fax has value.", false);

			void AssertContactHasMessageError(string message, bool expected)
			{
				validation.ValidateE2_Email();
				validation.ValidateE2_Phone();
				validation.ValidateE2_Fax();

				CombineAssertions(message, () =>
				{
					if (expected)
					{
						AssertHasMessageError(manufacturerDocAddress.E2_EmailInfo, notification);
						AssertHasMessageError(manufacturerDocAddress.E2_PhoneInfo, notification);
						AssertHasMessageError(manufacturerDocAddress.E2_FaxInfo, notification);
					}
					else
					{
						AssertNoMessageError(manufacturerDocAddress.E2_EmailInfo, notification);
						AssertNoMessageError(manufacturerDocAddress.E2_PhoneInfo, notification);
						AssertNoMessageError(manufacturerDocAddress.E2_FaxInfo, notification);
					}
				});
			}
		}

		public void TestCheckManufacturerID()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoicelineLinkCMHeader = invoiceLine.InvoiceLineLinkControllingMsgHeaders[0];
			invoicelineLinkCMHeader.IsLinkedCMHeader = true;
			var manufacturerDocAddress = invoiceLine.ManufacturerDocAddress;
			manufacturerDocAddress.E2_AddressOverride = true;
			var notification = "You have not entered a Manufacturer ID: A valid TW-VAT or TW-PAS or TW-PID or TW-FRI number is required for Manufacturer.";
			var validation = manufacturerDocAddress.Validation;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertIDHasMessageError("Not validate when Import", false);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertIDHasMessageError("All IDs are empty.", true);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			AssertIDHasMessageError("ControllingMessageType is not NX101, all IDs are empty.", false);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			manufacturerDocAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;
			manufacturerDocAddress.IDCode = "12345670";
			AssertIDHasMessageError("VAT has value.", false);

			manufacturerDocAddress.IDCodeType = OrgCusCode.CodeTypes.PassportID;
			manufacturerDocAddress.IDCode = "12345670";
			AssertIDHasMessageError("PAS has value.", false);

			manufacturerDocAddress.IDCodeType = OrgCusCode.TaiwanCodeTypes.PID;
			manufacturerDocAddress.IDCode = "12345670";
			AssertIDHasMessageError("PID has value.", false);

			manufacturerDocAddress.IDCode = ZString.Empty;
			manufacturerDocAddress.FRICodeType = Constants.OrgCusCodeType.CustomCode;
			manufacturerDocAddress.FRICode = "12345670";
			AssertIDHasMessageError("ID is empty, FRICodeType is ZZZ.", false);

			manufacturerDocAddress.FRICodeType = OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber;
			manufacturerDocAddress.FRICode = "12345670";
			AssertIDHasMessageError("ID is empty, FRI has value.", false);

			void AssertIDHasMessageError(string message, bool expected)
			{
				validation.ValidateIDCode();
				validation.ValidateFRICode();

				CombineAssertions(message, () =>
				{
					if (expected)
					{
						AssertHasMessageError(manufacturerDocAddress.IDCodeInfo, notification);
						AssertHasMessageError(manufacturerDocAddress.FRICodeInfo, notification);
					}
					else
					{
						AssertNoMessageError(manufacturerDocAddress.IDCodeInfo, notification);
						AssertNoMessageError(manufacturerDocAddress.FRICodeInfo, notification);
					}
				});
			}
		}

		public void TestCheckOrganisationPK()
		{
			var idMessage = "You have not entered a Manufacturer ID: A valid TW-VAT or TW-PAS or TW-PID or TW-FRI number is required for Manufacturer. To create a valid TW-VAT or TW-PAS or TW-PID or TW-FRI, visit Organization > Details > Config > Registration.";
			var nameMessage = "You have not entered a Manufacturer Company Name.";
			var localNameMessage = "You have not entered a Manufacturer Local Company Name.";
			var addressMessage = "You have not entered a Manufacturer Address.";
			var localAddressMessage = "You have not entered a Manufacturer Local Address.";
			var orgHeader = Factory.New<OrgHeader>();
			var mainAddress = orgHeader.MainAddress;
			var taiwan = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Taiwan);
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoicelineLinkCMHeader = invoiceLine.InvoiceLineLinkControllingMsgHeaders[0];
			invoicelineLinkCMHeader.IsLinkedCMHeader = true;
			var manufacturerDocAddress = invoiceLine.ManufacturerDocAddress;

			var validation = manufacturerDocAddress.Validation;
			validation.ValidateOrganisationPK();
			CombineAssertions("CertificateType is empty and organisation is null", () =>
			{
				AssertNoMessageError(manufacturerDocAddress.OrganisationPKInfo, idMessage);
				AssertNoMessageError(manufacturerDocAddress.OrganisationPKInfo, nameMessage);
				AssertNoMessageError(manufacturerDocAddress.OrganisationPKInfo, localNameMessage);
				AssertNoMessageError(manufacturerDocAddress.OrganisationPKInfo, addressMessage);
				AssertNoMessageError(manufacturerDocAddress.OrganisationPKInfo, localAddressMessage);
			});

			manufacturerDocAddress.OrganisationPK = orgHeader.PK;
			foreach (var manufactureIDType in manufacturerDocAddress.ManufactureIDTypes)
			{
				cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
				orgHeader.SetCustomsCode(manufactureIDType, taiwan, "12345670");
				validation.ValidateOrganisationPK();
				AssertNoMessageError($"CertificateType is empty and organisation has {manufactureIDType}", manufacturerDocAddress.OrganisationPKInfo, idMessage);

				orgHeader.CustomsCodes.RemoveAndDeleteAll();
				validation.ValidateOrganisationPK();
				AssertHasMessageError("CertificateType is empty and organisation has no ID", manufacturerDocAddress.OrganisationPKInfo, idMessage);

				cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
				validation.ValidateOrganisationPK();
				AssertNoMessageError($"ControllingMessageType is not NX101 and certificateType is empty and organisation has {manufactureIDType}", manufacturerDocAddress.OrganisationPKInfo, idMessage);
			}

			foreach (var certificateType in ConfidentialCertificateTypes)
			{
				cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
				manufacturerDocAddress.OrganisationPK = ZGuid.Empty;
				cMHeader.TW1_CertificateType = certificateType;
				validation.ValidateOrganisationPK();
				CombineAssertions($"CertificateType is {certificateType} and organisation is null", () =>
				{
					AssertHasMessageError(manufacturerDocAddress.OrganisationPKInfo, idMessage);
					AssertHasMessageError(manufacturerDocAddress.OrganisationPKInfo, nameMessage);
					AssertHasMessageError(manufacturerDocAddress.OrganisationPKInfo, localNameMessage);
					AssertHasMessageError(manufacturerDocAddress.OrganisationPKInfo, addressMessage);
					AssertHasMessageError(manufacturerDocAddress.OrganisationPKInfo, localAddressMessage);
				});

				cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
				validation.ValidateOrganisationPK();
				CombineAssertions($"ControllingMessageType is not NX101 and certificateType is {certificateType} and organisation is null", () =>
				{
					AssertNoMessageError(manufacturerDocAddress.OrganisationPKInfo, idMessage);
					AssertNoMessageError(manufacturerDocAddress.OrganisationPKInfo, nameMessage);
					AssertNoMessageError(manufacturerDocAddress.OrganisationPKInfo, localNameMessage);
					AssertNoMessageError(manufacturerDocAddress.OrganisationPKInfo, addressMessage);
					AssertNoMessageError(manufacturerDocAddress.OrganisationPKInfo, localAddressMessage);
				});

				manufacturerDocAddress.OrganisationPK = orgHeader.PK;
				foreach (var manufactureIDType in manufacturerDocAddress.ManufactureIDTypes)
				{
					cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
					orgHeader.SetCustomsCode(manufactureIDType, taiwan, "12345670");
					validation.ValidateOrganisationPK();
					AssertNoMessageError($"CertificateType is {certificateType} and organisation has {manufactureIDType}", manufacturerDocAddress.OrganisationPKInfo, idMessage);

					orgHeader.CustomsCodes.RemoveAndDeleteAll();
					validation.ValidateOrganisationPK();
					AssertHasMessageError($"CertificateType is {certificateType} and organisation has no ID", manufacturerDocAddress.OrganisationPKInfo, idMessage);

					cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
					validation.ValidateOrganisationPK();
					AssertNoMessageError($"ControllingMessageType is not NX101 and certificateType is {certificateType} and organisation has {manufactureIDType}", manufacturerDocAddress.OrganisationPKInfo, idMessage);
				}
			}
		}

		ImmutableHashSet<string> ConfidentialCertificateTypes => confidentialCertificateTypes ??= new[]
		{
			CertificateTypeList.Codes.Code9,
			CertificateTypeList.Codes.Code11,
			CertificateTypeList.Codes.Code13,
			CertificateTypeList.Codes.Code14,
			CertificateTypeList.Codes.Code15,
			CertificateTypeList.Codes.Code18,
			CertificateTypeList.Codes.Code19,
		}.ToImmutableHashSet();

		ImmutableHashSet<string> confidentialCertificateTypes;

		public void TestCheckE2_RN_NKCountryCode()
		{
			var targetInfo = manufacturerAddress.E2_RN_NKCountryCodeInfo;
			manufacturerAddress.E2_AddressOverride = true;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var messageError = "The country of Foreign Manufacturer must not be Taiwan.";

			manufacturerAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertNoMessageErrorContaining("US is a foreign country. Should have no error when IMP.", targetInfo, messageError);

			manufacturerAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			AssertHasMessageErrorContaining("TW is not a foreign country. Should have error when IMP.", targetInfo, messageError);

			messageError = "The country of Manufacturer must be Taiwan.";
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			manufacturerAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertHasMessageErrorContaining("US is not Taiwan. Should have error when EXP.", targetInfo, messageError);

			manufacturerAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			AssertNoMessageErrorContaining("TW is Taiwan, Should have no error when EXP.", targetInfo, messageError);
		}

		public void TestCheckE2_OA_Address()
		{
			var targetInfo = manufacturerAddress.E2_OA_AddressInfo;
			var validation = manufacturerAddress.Validation;
			var orgHeader = Factory.New<OrgHeader>();
			var address = orgHeader.MainAddress;
			address.OA_Address1 = "Address1";

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			manufacturerAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			var messageError = "The country of Foreign Manufacturer must not be Taiwan.";

			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining("TW is not a foreign country. Should have error when IMP.", targetInfo, messageError);

			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining("US is a foreign country. Should have no error when IMP.", targetInfo, messageError);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining("TW is not a foreign country, Should have no error when EXP.", targetInfo, messageError);

			messageError = "The country of Manufacturer must be Taiwan.";
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining("US is not Taiwan. Should have error when EXP.", targetInfo, messageError);

			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMessageHeader = entryInstruction.ControllingMessageHeaders.AddNew();

			manufacturerAddress.E2_OA_Address = ZGuid.Empty;
			AssertNoMessageErrorContaining("ManufacturerAddressInfo is not required.", targetInfo, MandatoryValidation.YouHaveNotEntered);

			manufacturerAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertNoMessageErrorContaining("ManufacturerAddressInfo is entered.", targetInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;

			controllingMessageHeader.TW1_ControllingMessageType = "NX301";
			manufacturerAddress.E2_OA_Address = ZGuid.Empty;
			AssertHasMessageErrorContaining("ManufacturerAddressInfo is required for NX301.", targetInfo, MandatoryValidation.YouHaveNotEntered);

			manufacturerAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertNoMessageErrorContaining("ManufacturerAddressInfo is entered for NX301.", targetInfo, MandatoryValidation.YouHaveNotEntered);

			controllingMessageHeader.TW1_ControllingMessageType = "NX301_DN";
			manufacturerAddress.E2_OA_Address = ZGuid.Empty;
			AssertHasMessageErrorContaining("ManufacturerAddressInfo is required for NX301_DN.", targetInfo, MandatoryValidation.YouHaveNotEntered);

			manufacturerAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertNoMessageErrorContaining("ManufacturerAddressInfo is entered for NX301_DN.", targetInfo, MandatoryValidation.YouHaveNotEntered);

			controllingMessageHeader.TW1_ControllingMessageType = "NX601";
			manufacturerAddress.E2_OA_Address = ZGuid.Empty;
			AssertHasMessageErrorContaining("ManufacturerAddressInfo is required for NX601.", targetInfo, MandatoryValidation.YouHaveNotEntered);

			manufacturerAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertNoMessageErrorContaining("ManufacturerAddressInfo is entered for NX601.", targetInfo, MandatoryValidation.YouHaveNotEntered);

			controllingMessageHeader.TW1_ControllingMessageType = "NX603";
			manufacturerAddress.E2_OA_Address = ZGuid.Empty;
			AssertHasMessageErrorContaining("ManufacturerAddressInfo is required for NX603.", targetInfo, MandatoryValidation.YouHaveNotEntered);

			manufacturerAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertNoMessageErrorContaining("ManufacturerAddressInfo is entered for NX603.", targetInfo, MandatoryValidation.YouHaveNotEntered);

			controllingMessageHeader.TW1_ControllingMessageType = "NX401";
			manufacturerAddress.E2_OA_Address = ZGuid.Empty;
			AssertNoMessageErrorContaining("ManufacturerAddressInfo is not required for NX401.", targetInfo, MandatoryValidation.YouHaveNotEntered);

			manufacturerAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertNoMessageErrorContaining("ManufacturerAddressInfo is required for NX301.", targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckE2_OA_Address_IsForCMHeaderMessageTypeNX101()
		{
			var nameMessage = "You have not entered a Manufacturer Company Name.";
			var localNameMessage = "You have not entered a Manufacturer Local Company Name.";
			var addressMessage = "You have not entered a Manufacturer Address.";
			var localAddressMessage = "You have not entered a Manufacturer Local Address.";
			var contactInformationMessage = "You have not entered a Manufacturer Contact Information.";
			var orgHeader = Factory.New<OrgHeader>();
			var mainAddress = orgHeader.MainAddress;
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoicelineLinkCMHeader = invoiceLine.InvoiceLineLinkControllingMsgHeaders[0];
			invoicelineLinkCMHeader.IsLinkedCMHeader = true;
			var manufacturerDocAddress = invoiceLine.ManufacturerDocAddress;
			manufacturerDocAddress.OrganisationPK = orgHeader.PK;

			var validation = manufacturerDocAddress.Validation;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			validation.ValidateE2_OA_Address();
			CombineAssertions("Not validate when Import", () =>
			{
				AssertNoMessageError(manufacturerDocAddress.E2_OA_AddressInfo, nameMessage);
				AssertNoMessageError(manufacturerDocAddress.E2_OA_AddressInfo, localNameMessage);
				AssertNoMessageError(manufacturerDocAddress.E2_OA_AddressInfo, addressMessage);
				AssertNoMessageError(manufacturerDocAddress.E2_OA_AddressInfo, localAddressMessage);
			});

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			validation.ValidateE2_OA_Address();
			CombineAssertions("Company Name, Address are empty, no ZH-TW tranlated address", () =>
			{
				AssertHasMessageError(manufacturerDocAddress.E2_OA_AddressInfo, nameMessage);
				AssertHasMessageError(manufacturerDocAddress.E2_OA_AddressInfo, localNameMessage);
				AssertHasMessageError(manufacturerDocAddress.E2_OA_AddressInfo, addressMessage);
				AssertHasMessageError(manufacturerDocAddress.E2_OA_AddressInfo, localAddressMessage);
			});

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			validation.ValidateE2_OA_Address();
			CombineAssertions("Not NX101, Company Name, Address are empty, no ZH-TW tranlated address", () =>
			{
				AssertNoMessageError(manufacturerDocAddress.E2_OA_AddressInfo, nameMessage);
				AssertNoMessageError(manufacturerDocAddress.E2_OA_AddressInfo, localNameMessage);
				AssertNoMessageError(manufacturerDocAddress.E2_OA_AddressInfo, addressMessage);
				AssertNoMessageError(manufacturerDocAddress.E2_OA_AddressInfo, localAddressMessage);
			});

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var translatedAddress = mainAddress.TranslatedAddresses.AddNew();
			translatedAddress.Language = Enterprise.Core.Constants.Languages.ChineseTraditional;
			translatedAddress.CompanyName = "公司名稱";
			translatedAddress.Address1 = "公司地址";
			validation.ValidateE2_OA_Address();
			CombineAssertions("Company Name, Address are empty, has ZH-TW tranlated address", () =>
			{
				AssertNoMessageError(manufacturerDocAddress.E2_OA_AddressInfo, nameMessage);
				AssertNoMessageError(manufacturerDocAddress.E2_OA_AddressInfo, localNameMessage);
				AssertNoMessageError(manufacturerDocAddress.E2_OA_AddressInfo, addressMessage);
				AssertNoMessageError(manufacturerDocAddress.E2_OA_AddressInfo, localAddressMessage);
			});

			translatedAddress.CompanyName = ZString.Empty;
			translatedAddress.Address1 = ZString.Empty;
			validation.ValidateE2_OA_Address();
			CombineAssertions("Company Name, Local Company Name, Address, Local Address are all empty", () =>
			{
				AssertHasMessageError(manufacturerDocAddress.E2_OA_AddressInfo, nameMessage);
				AssertHasMessageError(manufacturerDocAddress.E2_OA_AddressInfo, localNameMessage);
				AssertHasMessageError(manufacturerDocAddress.E2_OA_AddressInfo, addressMessage);
				AssertHasMessageError(manufacturerDocAddress.E2_OA_AddressInfo, localAddressMessage);
			});

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			validation.ValidateE2_OA_Address();
			CombineAssertions("Not NX101, Company Name, Local Company Name, Address, Local Address are all empty", () =>
			{
				AssertNoMessageError(manufacturerDocAddress.E2_OA_AddressInfo, nameMessage);
				AssertNoMessageError(manufacturerDocAddress.E2_OA_AddressInfo, localNameMessage);
				AssertNoMessageError(manufacturerDocAddress.E2_OA_AddressInfo, addressMessage);
				AssertNoMessageError(manufacturerDocAddress.E2_OA_AddressInfo, localAddressMessage);
			});

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			mainAddress.CompanyName = "Company Name";
			mainAddress.Address1 = "Company Address";
			validation.ValidateE2_OA_Address();
			CombineAssertions("Company Name and Address have values, Local Company Name and Local Address are empty", () =>
			{
				AssertNoMessageError(manufacturerDocAddress.E2_OA_AddressInfo, nameMessage);
				AssertNoMessageError(manufacturerDocAddress.E2_OA_AddressInfo, localNameMessage);
				AssertNoMessageError(manufacturerDocAddress.E2_OA_AddressInfo, addressMessage);
				AssertNoMessageError(manufacturerDocAddress.E2_OA_AddressInfo, localAddressMessage);
			});

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			validation.ValidateE2_OA_Address();
			CombineAssertions("CertificateType is 15, Company Name and Address have values, Local Company Name and Local Address are empty", () =>
			{
				AssertHasMessageError(manufacturerDocAddress.E2_OA_AddressInfo, localNameMessage);
				AssertHasMessageError(manufacturerDocAddress.E2_OA_AddressInfo, localAddressMessage);
			});
			AssertHasMessageError("CertificateType is 15, contact information is empty", manufacturerDocAddress.E2_OA_AddressInfo, contactInformationMessage);

			mainAddress.OA_Email = "user@wtg.com";
			validation.ValidateE2_OA_Address();
			AssertNoMessageError("CertificateType 15, Email has value", manufacturerDocAddress.E2_OA_AddressInfo, contactInformationMessage);

			mainAddress.OA_Email = ZString.Empty;
			mainAddress.OA_Phone = "+88621234567";
			validation.ValidateE2_OA_Address();
			AssertNoMessageError("CertificateType 15, Phone has value", manufacturerDocAddress.E2_OA_AddressInfo, contactInformationMessage);

			mainAddress.OA_Phone = ZString.Empty;
			mainAddress.OA_Mobile = "+88621234567";
			validation.ValidateE2_OA_Address();
			AssertNoMessageError("CertificateType 15, Mobile has value", manufacturerDocAddress.E2_OA_AddressInfo, contactInformationMessage);

			mainAddress.OA_Mobile = ZString.Empty;
			mainAddress.OA_Fax = "+88621234567";
			validation.ValidateE2_OA_Address();
			AssertNoMessageError("CertificateType 15, Fax has value", manufacturerDocAddress.E2_OA_AddressInfo, contactInformationMessage);

			mainAddress.OA_Fax = ZString.Empty;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			validation.ValidateE2_OA_Address();
			CombineAssertions("CertificateType is not 15, Company Name and Address have values, Local Company Name and Local Address are empty", () =>
			{
				AssertNoMessageError(manufacturerDocAddress.E2_OA_AddressInfo, localNameMessage);
				AssertNoMessageError(manufacturerDocAddress.E2_OA_AddressInfo, localAddressMessage);
			});
			AssertNoMessageError("CertificateType not 15, contact information is empty", manufacturerDocAddress.E2_OA_AddressInfo, contactInformationMessage);
		}

		public void TestCheckE2_OA_AddressCompanyNameLength()
		{
			var targetInfo = manufacturerAddress.E2_OA_AddressInfo;
			var chineseWarning = "Chinese Company Name Only the first 70 characters will be sent to the customs.";
			var manufacturer = Factory.New<OrgHeader>();
			var mainAddress = manufacturer.MainAddress;
			var transAddress = mainAddress.TranslatedAddresses.AddNew();
			transAddress.Language = "ZH-TW";
			transAddress.CompanyName = new ZString('A', 71);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			manufacturerAddress.E2_OA_Address = mainAddress.PK;
			AssertNoWarning(targetInfo, chineseWarning);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			manufacturerAddress.Validation.ValidateE2_OA_Address();
			AssertHasWarning(targetInfo, chineseWarning);

			transAddress.CompanyName = new ZString('A', 70);
			manufacturerAddress.Validation.ValidateE2_OA_Address();
			AssertNoWarning(targetInfo, chineseWarning);
		}

		public void TestCheckE2_OA_AddressAddressLength()
		{
			var warning = "Foreign Address Only the first 120 characters will be sent to the customs.";
			var manufacturer = Factory.New<OrgHeader>();
			var manufacturerAddress = manufacturer.MainAddress;
			manufacturerAddress.Language = "EN";
			manufacturerAddress.CompanyName = "CompanyName";
			manufacturerAddress.City = "Taipei City";
			manufacturerAddress.Postcode = "239";
			manufacturerAddress.OA_RN_NKCountryCode = "TW";
			manufacturerAddress.Address1 = new ZString('A', 50);
			manufacturerAddress.Address2 = new ZString('B', 46);
			manufacturerAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			var targetInfo = this.manufacturerAddress.E2_OA_AddressInfo;
			this.manufacturerAddress.E2_OA_Address = manufacturer.MainAddress.PK;
			AssertNoWarning(targetInfo, warning);

			manufacturerAddress.Address2 = new ZString('B', 47);
			this.manufacturerAddress.Validation.ValidateE2_OA_Address();
			AssertHasWarning(targetInfo, warning);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			this.manufacturerAddress.Validation.ValidateE2_OA_Address();
			AssertNoWarning(targetInfo, warning);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			manufacturerAddress = invoiceLine.ManufacturerDocAddress;
		}

		#endregion

		JobDeclarationForJobDocAddressTest declaration;
		JobComInvoiceLine invoiceLine;
		TWJobDocAddress manufacturerAddress;
	}
}
