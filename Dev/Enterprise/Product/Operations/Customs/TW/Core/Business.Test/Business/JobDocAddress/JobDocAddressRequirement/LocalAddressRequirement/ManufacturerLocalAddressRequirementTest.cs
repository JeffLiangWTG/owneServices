using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ManufacturerLocalAddressRequirement))]
	sealed class ManufacturerLocalAddressRequirementTest : TestCaseWithFactory
	{
		public void TestCheckE2_CompanyName()
		{
			var message = "You have not entered a Manufacturer Local Company Name.";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			var cMHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var invoicelineLinkCMHeader = invoiceLine.InvoiceLineLinkControllingMsgHeaders[0];
			invoicelineLinkCMHeader.IsLinkedCMHeader = true;
			var manufacturerDocAddress = invoiceLine.ManufacturerDocAddress;
			manufacturerDocAddress.E2_AddressOverride = true;
			var localAddress = invoiceLine.ManufacturerDocAddress.LocalAddress;
			var targetInfo = localAddress.E2_CompanyNameInfo;
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var validation = localAddress.Validation;
			validation.ValidateE2_CompanyName();
			AssertHasMessageError(targetInfo, message);

			invoicelineLinkCMHeader.IsLinkedCMHeader = false;
			validation.ValidateE2_CompanyName();
			AssertNoMessageError("No linked CMHeader", targetInfo, message);

			invoicelineLinkCMHeader.IsLinkedCMHeader = true;
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			validation.ValidateE2_CompanyName();
			AssertNoMessageError("CMHeader message type is not NX101", targetInfo, message);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			manufacturerDocAddress.E2_CompanyName = "Company Name";
			validation.ValidateE2_CompanyName();
			AssertNoMessageError("Company Name has value", targetInfo, message);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			validation.ValidateE2_CompanyName();
			AssertHasMessageError("Certificate Type is 15", targetInfo, message);

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
				AssertNoMessageError("ID Same LocalProcessAddress", targetInfo, message);

				manufacturerDocAddress.IDCode = "99999999";
				validation.ValidateE2_CompanyName();
				AssertHasMessageError("ID different LocalProcessAddress", targetInfo, message);

				manufacturerDocAddress.IDCodeType = Constants.OrgCusCodeType.CustomCode;
				manufacturerDocAddress.IDCode = "123456789";
				validation.ValidateE2_CompanyName();
				AssertHasMessageError("ID Type different LocalProcessAddress", targetInfo, message);

				manufacturerDocAddress.FRICodeType = codeTypes;
				manufacturerDocAddress.FRICode = "123456789";
				validation.ValidateE2_CompanyName();
				AssertNoMessageError("FRI Same LocalProcessAddress", targetInfo, message);

				manufacturerDocAddress.FRICode = "99999999";
				validation.ValidateE2_CompanyName();
				AssertHasMessageError("FRI different LocalProcessAddress", targetInfo, message);

				manufacturerDocAddress.FRICodeType = Constants.OrgCusCodeType.CustomCode;
				manufacturerDocAddress.FRICode = "123456789";
				validation.ValidateE2_CompanyName();
				AssertHasMessageError("FRI Type different LocalProcessAddress", targetInfo, message);
			}

			localAddress.E2_CompanyName = "公司名稱";
			AssertNoMessageError("Local Company Name has value", targetInfo, message);
		}

		public void TestCheckE2_CompanyNameMaxLength()
		{
			var warningMessage = "Only the first 70 characters will be sent to the customs.";
			var targetInfo = manufacturerLocalAddress.E2_CompanyNameInfo;
			manufacturerLocalAddress.E2_CompanyName = new ZString('A', 90);
			AssertHasWarning(targetInfo, warningMessage);

			manufacturerLocalAddress.E2_CompanyName = new ZString('A', 69);
			AssertNoWarning(targetInfo, warningMessage);
		}

		public void TestCheckE2_Address1()
		{
			var message = "You have not entered a Manufacturer Local Address.";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			var cMHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var invoicelineLinkCMHeader = invoiceLine.InvoiceLineLinkControllingMsgHeaders[0];
			invoicelineLinkCMHeader.IsLinkedCMHeader = true;
			var manufacturerDocAddress = invoiceLine.ManufacturerDocAddress;
			manufacturerDocAddress.E2_AddressOverride = true;
			var localAddress = invoiceLine.ManufacturerDocAddress.LocalAddress;
			var targetInfo = localAddress.E2_Address1Info;
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var validation = localAddress.Validation;
			validation.ValidateE2_Address1();
			AssertHasMessageError(targetInfo, message);

			invoicelineLinkCMHeader.IsLinkedCMHeader = false;
			validation.ValidateE2_Address1();
			AssertNoMessageError("No linked CMHeader", targetInfo, message);

			invoicelineLinkCMHeader.IsLinkedCMHeader = true;
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			validation.ValidateE2_Address1();
			AssertNoMessageError("CMHeader message type is not NX101", targetInfo, message);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			manufacturerDocAddress.E2_Address1 = "Company Name";
			validation.ValidateE2_Address1();
			AssertNoMessageError("Company Name has value", targetInfo, message);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			validation.ValidateE2_Address1();
			AssertHasMessageError("Certificate Type is 15", targetInfo, message);

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

			localAddress.E2_Address1 = "地址名稱";
			AssertNoMessageError("Local Company Name has value", targetInfo, message);
		}

		public void TestCheckE2_Address1Maxlength()
		{
			var warningMessage = "Chinese Address Only the first 100 characters will be sent to the customs.";
			var targetInfo = manufacturerLocalAddress.E2_Address1Info;
			manufacturerLocalAddress.Address2 = new string('A', 50);
			manufacturerLocalAddress.AdditionalAddressInformation = new string('A', 50);
			manufacturerLocalAddress.Address1 = new string('A', 50);
			AssertHasWarning(targetInfo, warningMessage);

			manufacturerLocalAddress.Address2 = ZString.Empty;
			manufacturerLocalAddress.Address1 = new string('A', 49);
			AssertNoWarning(targetInfo, warningMessage);
		}

		public void TestCheckE2_City()
		{
			manufacturerLocalAddress.E2_AddressOverride = true;
			manufacturerLocalAddress.E2_RN_NKCountryCode = "AU";
			manufacturerLocalAddress.Validation.ValidateE2_City();
			AssertNoErrors(manufacturerLocalAddress.E2_CityInfo);
		}

		public void TestCheckE2_Postcode()
		{
			manufacturerLocalAddress.E2_AddressOverride = true;
			manufacturerLocalAddress.Validation.ValidateE2_Postcode();
			AssertNoErrors(manufacturerLocalAddress.E2_PostcodeInfo);
		}

		public void TestCheckE2_State()
		{
			var targetInfo = manufacturerLocalAddress.E2_StateInfo;
			var warningMessage = "This state code needs to be followed by valid country code.";
			manufacturerLocalAddress.E2_AddressOverride = true;
			manufacturerLocalAddress.E2_RN_NKCountryCode = "78";
			manufacturerLocalAddress.E2_State = "EE";
			AssertHasWarning(targetInfo, warningMessage);
			manufacturerLocalAddress.E2_RN_NKCountryCode = "DE";
			manufacturerLocalAddress.Validation.ValidateE2_State();
			AssertNoWarning(targetInfo, warningMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			var manufacturerDocAddress = invoiceLine.ManufacturerDocAddress;
			manufacturerDocAddress.E2_AddressOverride = true;
			manufacturerLocalAddress = manufacturerDocAddress.LocalAddress;
		}

		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
		JobDocAddress manufacturerLocalAddress;
	}
}
