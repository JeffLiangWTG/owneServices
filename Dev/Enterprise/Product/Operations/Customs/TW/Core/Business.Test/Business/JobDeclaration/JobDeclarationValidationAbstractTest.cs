using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestsSubclassesOf(typeof(JobDeclarationValidation))]
	abstract class JobDeclarationValidationAbstractTest<TJobDeclarationValidation> : Customs.Business.Testing.BaseJobDeclarationValidationTest<JobDeclaration>
		where TJobDeclarationValidation : JobDeclarationValidation
	{
		public void TestCheckJE_MasterBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var declarationValidation = declaration.Validation;

			CombineAssertions("Nil for Empty Master Bill", () =>
			{
				var warningMessage = "System will automatically declare 'NIL' when 'Master Bill' is empty.";
				declaration.JE_MasterBill = "010-9999999";
				declaration.JE_MessageType = "EXP";

				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D1;
				declaration.JE_MasterBill = ZString.Empty;
				AssertHasWarning("EXP-D1", declaration.JE_MasterBillInfo, warningMessage);
				declaration.JE_MasterBill = "78512345678";
				AssertNoWarning("EXP-D1", declaration.JE_MasterBillInfo, warningMessage);

				declaration.JE_MasterBill = ZString.Empty;
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;
				AssertHasWarning("EXP-B1", declaration.JE_MasterBillInfo, warningMessage);
				declaration.JE_MasterBill = "78512345678";
				AssertNoWarning("EXP-B1", declaration.JE_MasterBillInfo, warningMessage);

				declaration.JE_MasterBill = ZString.Empty;
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B2;
				AssertHasWarning("EXP-B2", declaration.JE_MasterBillInfo, warningMessage);
				declaration.JE_MasterBill = "78512345678";
				AssertNoWarning("EXP-B2", declaration.JE_MasterBillInfo, warningMessage);

				declaration.JE_MasterBill = ZString.Empty;
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F5;
				AssertHasWarning("EXP-F5", declaration.JE_MasterBillInfo, warningMessage);
				declaration.JE_MasterBill = "78512345678";
				AssertNoWarning("EXP-F5", declaration.JE_MasterBillInfo, warningMessage);

				declaration.JE_TransportMode = "AIR";
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D1;
				declaration.JE_MasterBill = ZString.Empty;
				AssertNoWarning("EXP-AIR-D1", declaration.JE_MasterBillInfo, warningMessage);

				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;
				declarationValidation.ValidateJE_MasterBill();
				AssertNoWarning("EXP-AIR-B1", declaration.JE_MasterBillInfo, warningMessage);

				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B2;
				declarationValidation.ValidateJE_MasterBill();
				AssertNoWarning("EXP-AIR-B2", declaration.JE_MasterBillInfo, warningMessage);

				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F5;
				declarationValidation.ValidateJE_MasterBill();
				AssertNoWarning("EXP-AIR-F5", declaration.JE_MasterBillInfo, warningMessage);

				warningMessage = "System will automatically declare 'NIL' when 'Ocean Bill' is empty.";
				declaration.JE_MessageType = "IMP";
				declaration.JE_TransportMode = "SEA";
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G2;
				declarationValidation.ValidateJE_MasterBill();
				AssertHasWarning("IMP-SEA-G2", declaration.JE_MasterBillInfo, warningMessage);
				declaration.JE_MasterBill = "78512345678";
				AssertNoWarning("IMP-SEA-G2", declaration.JE_MasterBillInfo, warningMessage);

				declaration.JE_MasterBill = ZString.Empty;
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D2;
				AssertHasWarning("IMP-SEA-D2", declaration.JE_MasterBillInfo, warningMessage);
				declaration.JE_MasterBill = "78512345678";
				AssertNoWarning("IMP-SEA-D2", declaration.JE_MasterBillInfo, warningMessage);

				declaration.JE_MasterBill = ZString.Empty;
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D7;
				AssertHasWarning("IMP-SEA-D7", declaration.JE_MasterBillInfo, warningMessage);
				declaration.JE_MasterBill = "78512345678";
				AssertNoWarning("IMP-SEA-D7", declaration.JE_MasterBillInfo, warningMessage);

				declaration.JE_MasterBill = ZString.Empty;
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F3;
				AssertHasWarning("IMP-SEA-F3", declaration.JE_MasterBillInfo, warningMessage);
				declaration.JE_MasterBill = "78512345678";
				AssertNoWarning("IMP-SEA-F3", declaration.JE_MasterBillInfo, warningMessage);

				warningMessage = "System will automatically declare 'NIL' when 'Master Bill' is empty.";
				declaration.JE_TransportMode = "AIR";
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G2;
				declaration.JE_MasterBill = ZString.Empty;
				AssertNoWarning("IMP-AIR-G2", declaration.JE_MasterBillInfo, warningMessage);

				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D2;
				declarationValidation.ValidateJE_MasterBill();
				AssertNoWarning("IMP-AIR-D2", declaration.JE_MasterBillInfo, warningMessage);

				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D7;
				declarationValidation.ValidateJE_MasterBill();
				AssertNoWarning("IMP-AIR-D7", declaration.JE_MasterBillInfo, warningMessage);

				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F3;
				declarationValidation.ValidateJE_MasterBill();
				AssertNoWarning("IMP-AIR-F3", declaration.JE_MasterBillInfo, warningMessage);
			});

			CombineAssertions("Master Bill not entered", () =>
			{
				var messageError = "You have not entered";
				declaration.JE_MessageType = "EXP";
				declaration.JE_TransportMode = "SEA";
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.G3;
				declaration.JE_MasterBill = ZString.Empty;
				AssertHasMessageErrorContaining("EXP-SEA-G3", declaration.JE_MasterBillInfo, messageError);
				declaration.JE_MasterBill = "785123456789";
				AssertNoMessageErrorContaining("EXP-SEA-G3", declaration.JE_MasterBillInfo, messageError);

				declaration.JE_MasterBill = ZString.Empty;
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.G5;
				AssertHasMessageErrorContaining("EXP-SEA-G5", declaration.JE_MasterBillInfo, messageError);
				declaration.JE_MasterBill = "78512345678";
				AssertNoMessageErrorContaining("EXP-SEA-G5", declaration.JE_MasterBillInfo, messageError);

				declaration.JE_MessageType = "IMP";
				declaration.JE_MasterBill = ZString.Empty;
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G1;
				AssertHasMessageErrorContaining("EXP-SEA-G1", declaration.JE_MasterBillInfo, messageError);
				declaration.JE_MasterBill = "78512345678";
				AssertNoMessageErrorContaining("EXP-SEA-G1", declaration.JE_MasterBillInfo, messageError);

				declaration.JE_MasterBill = ZString.Empty;
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G7;
				AssertHasMessageErrorContaining("EXP-SEA-G7", declaration.JE_MasterBillInfo, messageError);
				declaration.JE_MasterBill = "78512345678";
				AssertNoMessageErrorContaining("EXP-SEA-G7", declaration.JE_MasterBillInfo, messageError);

				declaration.JE_MessageType = "EXP";
				declaration.JE_TransportMode = "AIR";
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.G3;
				declaration.JE_MasterBill = ZString.Empty;
				AssertNoMessageErrorContaining("EXP-AIR-G3", declaration.JE_MasterBillInfo, messageError);

				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.G5;
				declarationValidation.ValidateJE_MasterBill();
				AssertNoMessageErrorContaining("EXP-AIR-G5", declaration.JE_MasterBillInfo, messageError);

				declaration.JE_MessageType = "IMP";
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G1;
				declarationValidation.ValidateJE_MasterBill();
				AssertNoMessageErrorContaining("IMP-AIR-G1", declaration.JE_MasterBillInfo, messageError);

				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G7;
				declarationValidation.ValidateJE_MasterBill();
				AssertNoMessageErrorContaining("IMP-AIR-G7", declaration.JE_MasterBillInfo, messageError);
			});

			CombineAssertions(() =>
			{
				var messageError = "The MAWB should contain 11 digits.";
				declaration.JE_MessageType = "EXP";
				declaration.JE_TransportMode = "AIR";
				declaration.JE_MasterBill = ZString.Empty;
				AssertNoMessageError(declaration.JE_MasterBillInfo, messageError);
				declaration.JE_MasterBill = "7851234567";
				AssertHasMessageError(declaration.JE_MasterBillInfo, messageError);
				declaration.JE_MasterBill = "78512345671";
				AssertNoMessageError(declaration.JE_MasterBillInfo, messageError);
			});
		}

		public void TestCheckJE_VoyageFlightNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "SEA";
			declaration.JE_VoyageFlightNo = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_VoyageFlightNo = "TEST";
			AssertNoMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = "AIR";
			declaration.JE_VoyageFlightNo = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageType = "EXP";
			declaration.JE_TransportMode = "SEA";
			declaration.JE_VoyageFlightNo = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_VoyageFlightNo = "TEST";
			AssertNoMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = "AIR";
			declaration.JE_VoyageFlightNo = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = "IMP";
			declaration.JE_VoyageFlightNo = "CI 0008";
			var formatErrorMessage = "System will automatically declare two-letter airline code and the four-digit flight number, separated by a space character. For example, CI 0008, where the two-letter airline code is CI and the four-digit flight number is 0008.";
			AssertNoWarningContaining(declaration.JE_VoyageFlightNoInfo, formatErrorMessage);

			declaration.JE_VoyageFlightNo = "'ABC 0008";
			AssertHasWarningContaining(declaration.JE_VoyageFlightNoInfo, formatErrorMessage);

			declaration.JE_VoyageFlightNo = "NIL";
			AssertNoWarningContaining(declaration.JE_VoyageFlightNoInfo, formatErrorMessage);

			declaration.JE_VoyageFlightNo = "5X 1234";
			AssertNoWarningContaining(declaration.JE_VoyageFlightNoInfo, formatErrorMessage);

			declaration.JE_MessageType = "EXP";
			declaration.JE_VoyageFlightNo = "CI0008";
			AssertHasWarningContaining(declaration.JE_VoyageFlightNoInfo, formatErrorMessage);

			declaration.JE_VoyageFlightNo = "CI 0008";
			AssertNoWarningContaining(declaration.JE_VoyageFlightNoInfo, formatErrorMessage);

			declaration.JE_VoyageFlightNo = "NIL";
			AssertNoWarningContaining(declaration.JE_VoyageFlightNoInfo, formatErrorMessage);
		}

		public void TestCheckTW1_CertificateType_JE_VoyageFlightNo()
		{
			AssertCheckTW1_CertificateType_JE_VoyageFlightNo(true, CertificateTypeList.Codes.Code15, ControllingMessageTypeList.Codes.NX101, Core.Constants.TransportModes.Air, ZString.Empty);
			AssertCheckTW1_CertificateType_JE_VoyageFlightNo(false, CertificateTypeList.Codes.Code15, ControllingMessageTypeList.Codes.NX101, Core.Constants.TransportModes.Sea, ZString.Empty);
			AssertCheckTW1_CertificateType_JE_VoyageFlightNo(false, CertificateTypeList.Codes.Code1, ControllingMessageTypeList.Codes.NX101, Core.Constants.TransportModes.Air, ZString.Empty);
			AssertCheckTW1_CertificateType_JE_VoyageFlightNo(false, CertificateTypeList.Codes.Code15, ControllingMessageTypeList.Codes.X101, Core.Constants.TransportModes.Air, ZString.Empty);
			AssertCheckTW1_CertificateType_JE_VoyageFlightNo(false, CertificateTypeList.Codes.Code15, ControllingMessageTypeList.Codes.NX101, Core.Constants.TransportModes.Air, "TEST");
		}

		void AssertCheckTW1_CertificateType_JE_VoyageFlightNo(bool expectShowError, ZString certificateType, ZString messageType, ZString transportMode, ZString voyageFlightNo)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = transportMode;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = messageType;
			declaration.JE_VoyageFlightNo = voyageFlightNo;
			messageHeader.TW1_CertificateType = certificateType;
			AssertEquals("no error when not link to CMHeader", false, declaration.JE_VoyageFlightNoInfo.HasMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Flight/Folio")));

			var invoiceLine = (JobComInvoiceLine)invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			messageHeader.TW1_ControllingAgency = "XX";
			var link = invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.ControllingAgency == "XX");
			link.IsLinkedCMHeader = true;
			messageHeader.TW1_CertificateType = certificateType;
			AssertEquals("Flight/Folio", expectShowError, declaration.JE_VoyageFlightNoInfo.HasMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Flight/Folio")));
		}

		public void TestDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(declaration.Validation.Declaration, declaration);
		}

		public void TestCheckJE_VesselNameMandatory()
		{
			var targetInfo = declaration.JE_VesselNameInfo;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_VesselName = "Vessel";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCallSignOfJE_VesselNameMandatory()
		{
			var vessel1 = CreateVessel("CCCC", "1234567");
			var vessel2 = CreateVessel("DDDD", "1234567", "9MBE4");
			Factory.Save();
			var targetInfo = declaration.JE_VesselNameInfo;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertCheckCallSignOfJE_VesselNameMandatory();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertCheckCallSignOfJE_VesselNameMandatory();

			void AssertCheckCallSignOfJE_VesselNameMandatory()
			{
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_VesselName = vessel1.RV_Code;
				AssertHasMessageError(targetInfo, ValidationConstants.Declaration.CallSignMandatory);
				declaration.JE_VesselName = vessel2.RV_Code;
				AssertNoErrors(targetInfo);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				declaration.JE_VesselName = vessel1.RV_Code;
				AssertNoErrors(targetInfo);
				declaration.JE_VesselName = vessel2.RV_Code;
				AssertNoErrors(targetInfo);
			}
		}

		protected RefVessel CreateVessel(string vessel, string imo = null, string callSign = null)
		{
			var refVessel = RefVessel.LookupVesselByCode(vessel, Factory);
			if (refVessel == null)
			{
				refVessel = Factory.NewWithValidTestData<RefVessel>();
				refVessel.RV_Code = vessel;
			}

			if (imo != null)
			{
				refVessel.RV_LloydsNumber = imo;
			}

			if (callSign != null)
			{
				refVessel.RV_RadioCallSign = callSign;
			}

			return refVessel;
		}

		public void TestCheckJE_CustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var listType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			var countryCode = Core.Constants.CountryCodes.Taiwan;
			helper.CreateNewOrGetExistingCusCodeType(listType, "CustomsOffice");
			var codeList = helper.CreateCusCodeList(countryCode, listType, "CC", "BABA THE BUILDER", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(codeList.PK, TransportTypeList.Codes.Sea);
			var codeList2 = helper.CreateCusCodeList(countryCode, listType, "DD", "DD Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(codeList2.PK, TransportTypeList.Codes.Air);
			Factory.Save();
			declaration.JE_CustomsOffice = "BJ";
			AssertHasMessageError(declaration.JE_CustomsOfficeInfo, "The code you have selected is not in the list.");
			declaration.JE_CustomsOffice = "CC";
			AssertNoNotifications(declaration.JE_CustomsOfficeInfo);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.Validation.ValidateJE_CustomsOffice();
			AssertHasMessageError(declaration.JE_CustomsOfficeInfo, ValidationConstants.Declaration.CustomsOfficeNotForTransportMode);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.Validation.ValidateJE_CustomsOffice();
			AssertNoNotifications(declaration.JE_CustomsOfficeInfo);
			declaration.JE_CustomsOffice = "DD";
			AssertHasMessageError(declaration.JE_CustomsOfficeInfo, ValidationConstants.Declaration.CustomsOfficeNotForTransportMode);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			declaration.JE_CustomsOffice = "CC";
			AssertNoNotifications(declaration.JE_CustomsOfficeInfo);
			declaration.JE_CustomsOffice = "DD";
			AssertNoNotifications(declaration.JE_CustomsOfficeInfo);
		}

		public void TestCheckJE_LocationOfGoods()
		{
			var customsOffice = Factory.New<ZZRefCusCodeListCombined>();
			customsOffice.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Taiwan;
			customsOffice.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			customsOffice.ZZD_Code = "BT";
			customsOffice.ZZD_StartDate = ZDateTime.Today;
			customsOffice.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			var facility1 = Factory.New<ZZRefCusCodeListCombined>();
			facility1.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Taiwan;
			facility1.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
			facility1.ZZD_Code = "ANP0060N";
			facility1.ZZD_StartDate = ZDateTime.Today;
			facility1.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			facility1.ZZD_IsAir = true;
			facility1.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "BT");
			var facility2 = Factory.New<ZZRefCusCodeListCombined>();
			facility2.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Taiwan;
			facility2.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
			facility2.ZZD_Code = "BNP0060N";
			facility2.ZZD_StartDate = ZDateTime.Today;
			facility2.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			facility2.ZZD_IsSea = true;
			facility2.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "AT");
			Factory.Save();
			declaration.JE_CustomsOffice = "BT";
			declaration.JE_LocationOfGoods = "ANP0060N";
			AssertEquals(false, declaration.JE_LocationOfGoodsInfo.HasMessageErrors());
			AssertNoWarnings(declaration.JE_LocationOfGoodsInfo);
			declaration.JE_LocationOfGoods = "BNP0060N";
			AssertEquals(false, declaration.JE_LocationOfGoodsInfo.HasMessageErrors());
			AssertHasWarning(declaration.JE_LocationOfGoodsInfo, ValidationConstants.Declaration.LocationOfGoodsDoesNotBelongToCustomsOffice("BNP0060N", "BT"));
		}

		public void TestCheckJE_DefermentAccountNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.MiscellaneousCustoms;
			declaration.JE_DefermentAccountNumber = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_DefermentAccountNumberInfo, ValidationConstants.Declaration.CassNoOrAccountNoMandatory);
			declaration.JE_PaymentMethod = IMPPaymentMethod.Codes._1;
			declaration.JE_DefermentAccountNumber = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_DefermentAccountNumberInfo, ValidationConstants.Declaration.CassNoOrAccountNoMandatory);
			declaration.JE_PaymentMethod = IMPPaymentMethod.Codes._2;
			declaration.JE_DefermentAccountNumber = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_DefermentAccountNumberInfo, ValidationConstants.Declaration.CassNoOrAccountNoMandatory);
			declaration.JE_DefermentAccountNumber = "A";
			AssertNoMessageErrorContaining(declaration.JE_DefermentAccountNumberInfo, ValidationConstants.Declaration.CassNoOrAccountNoMandatory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = ZString.Empty;
			declaration.JE_DefermentAccountNumber = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_DefermentAccountNumberInfo, ValidationConstants.Declaration.CassNoOrAccountNoMandatory);
			declaration.JE_PaymentMethod = IMPPaymentMethod.Codes._1;
			declaration.JE_DefermentAccountNumber = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_DefermentAccountNumberInfo, ValidationConstants.Declaration.CassNoOrAccountNoMandatory);
			declaration.JE_PaymentMethod = IMPPaymentMethod.Codes._2;
			declaration.JE_DefermentAccountNumber = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_DefermentAccountNumberInfo, ValidationConstants.Declaration.CassNoOrAccountNoMandatory);
			declaration.JE_DefermentAccountNumber = "A";
			AssertNoMessageErrorContaining(declaration.JE_DefermentAccountNumberInfo, ValidationConstants.Declaration.CassNoOrAccountNoMandatory);
		}

		public virtual void TestCheckJE_PaymentMethod()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_PaymentMethod = EXPPaymentMethod.Codes._1;
			AssertNoMessageError(declaration.JE_PaymentMethodInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_PaymentMethod = "X";
			AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = IMPPaymentMethod.Codes._1;
			AssertNoMessageError(declaration.JE_PaymentMethodInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_PaymentMethod = "X";
			AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestJE_GS_NKCusAgent()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "DEVELOPER COMPANY";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_OH_OrgProxy = org.PK;
			Factory.Save();
			GlbStaff.CurrentUser.GS_GB_HomeBranch = branch.PK;
			var newBroker = Factory.NewWithValidTestData<GlbStaff>();
			newBroker.GS_GB_HomeBranch = GlbStaff.CurrentUser.HomeBranch.PK;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GS_NKCusAgent = newBroker.GS_Code;
			var staffInfo = declaration.JE_GS_NKCusAgentInfo;
			declaration.Validation.ValidateJE_GS_NKCusAgent();
			AssertHasMessageError(staffInfo, ValidationConstants.CusInBondHeader.BrokerStaffNotHaveValidCertificateNumber);
			var brkCertificate = newBroker.Certificates.AddNew();
			brkCertificate.XZ_Type = Enterprise.Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			brkCertificate.XZ_RN_NKCountryOfIssuance = "TW";
			brkCertificate.XZ_RefNumber = "1234";
			brkCertificate.XZ_ExpiryOrDueDate = System.DateTime.Today.AddYears(1);
			Factory.Save();
			declaration.Validation.ValidateJE_GS_NKCusAgent();
			AssertNoMessageError(staffInfo, ValidationConstants.CusInBondHeader.BrokerStaffNotHaveValidCertificateNumber);
			declaration.JE_GS_NKCusAgent = ZString.Empty;
			AssertHasMessageErrorContaining(staffInfo, ValidationConstants.Declaration.BrokerStaffShouldNotBeEmpty);
			declaration.JE_GS_NKCusAgent = newBroker.GS_Code;
			var errorNotification = "The Taiwan broker certificate must be exactly 5 characters long with only numbers and capital English letters.";
			brkCertificate.XZ_RefNumber = "一二三";
			declaration.Validation.ValidateJE_GS_NKCusAgent();
			AssertEquals("一二三", declaration.CusAgentCertificateNumber);
			AssertHasMessageErrorContaining(staffInfo, errorNotification);
			brkCertificate.XZ_RefNumber = "ABC123456";
			AssertEquals("ABC123456", declaration.CusAgentCertificateNumber);
			declaration.Validation.ValidateJE_GS_NKCusAgent();
			AssertHasMessageErrorContaining(staffInfo, errorNotification);
			brkCertificate.XZ_RefNumber = "abc12";
			AssertEquals("abc12", declaration.CusAgentCertificateNumber);
			declaration.Validation.ValidateJE_GS_NKCusAgent();
			AssertNoMessageErrorContaining(staffInfo, errorNotification);
			brkCertificate.XZ_RefNumber = "ABC12";
			AssertEquals("ABC12", declaration.CusAgentCertificateNumber);
			declaration.Validation.ValidateJE_GS_NKCusAgent();
			AssertNoMessageErrorContaining(staffInfo, errorNotification);

			var staffWrapper = TWGlbStaffWrapper.Get(newBroker);
			var credentials = staffWrapper.TWPasswordCollection;
			var password = credentials.AddNew();
			password.GP_PasswordType = PasswordTypesList.Codes.TVA;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_MailBoxID = "CBK0124-0";
			password.GP_PasswordStatus = PasswordStatusList.Codes.PasswordOK;
			password.GP_GS = newBroker.PK;
			password.GP_UserID = "2";
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			declaration.JE_CustomsProfile = "CBK0124-0";

			var errorNotification2 = "The selected Broker Staff does not have a valid Credential.";
			declaration.Validation.ValidateJE_GS_NKCusAgent();
			AssertNoMessageErrorContaining(staffInfo, errorNotification2);

			password.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			declaration.Validation.ValidateJE_GS_NKCusAgent();
			AssertHasMessageErrorContaining(staffInfo, errorNotification2);

			credentials.RemoveAndDeleteAll();
			declaration.Validation.ValidateJE_GS_NKCusAgent();
			AssertHasMessageErrorContaining(staffInfo, errorNotification2);
		}

		[TestDate(2019, 12, 1)]
		public void TestCheckJE_CustomsProfile()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "DEVELOPER COMPANY";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_OH_OrgProxy = org.PK;
			Factory.Save();
			GlbStaff.CurrentUser.GS_GB_HomeBranch = branch.PK;
			var broker1 = Factory.NewWithValidTestData<GlbStaff>();
			broker1.GS_FullName = "111";
			broker1.GS_GB_HomeBranch = GlbStaff.CurrentUser.HomeBranch.PK;
			var brkCertificate = broker1.Certificates.AddNew();
			brkCertificate.XZ_Type = Enterprise.Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			brkCertificate.XZ_RN_NKCountryOfIssuance = "TW";
			brkCertificate.XZ_RefNumber = "1111";
			var broker2 = Factory.NewWithValidTestData<GlbStaff>();
			broker2.GS_FullName = "222";
			broker2.GS_GB_HomeBranch = GlbStaff.CurrentUser.HomeBranch.PK;
			var brkCertificate2 = broker2.Certificates.AddNew();
			brkCertificate2.XZ_Type = Enterprise.Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			brkCertificate2.XZ_RN_NKCountryOfIssuance = "TW";
			brkCertificate2.XZ_RefNumber = "2222";
			Factory.Save();
			var extPswUva = Factory.New<GlbExternalPassword>();
			extPswUva.GP_GC = GlbCompany.CurrentCompany.PK;
			extPswUva.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPswUva.GP_GS = broker1.PK;
			extPswUva.GP_MailBoxID = "BBB-2";
			extPswUva.GP_UserID = "001";
			extPswUva.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			extPswUva.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var targetInfo = declaration.JE_CustomsProfileInfo;
			declaration.JE_GS_NKCusAgent = broker1.GS_Code;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_GS_NKCusAgent = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_GS_NKCusAgent = broker2.GS_Code;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_CustomsProfile = "AAA";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForTesting);
			AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForProduction);
			TWCustomsDataRegistry.Instance.TWIsTestMode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			declaration.JE_GS_NKCusAgent = broker1.GS_Code;
			declaration.JE_CustomsProfile = "BBB-2";
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			extPswUva.GP_UserID = PasswordTypesList.Codes.TVA;
			extPswUva.GP_UserID = "001TEST";
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			declaration.Validation.ValidateJE_CustomsProfile();
			AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForTesting);
			AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForProduction);
			extPswUva.GP_UserID = PasswordTypesList.Codes.UVC;
			extPswUva.GP_UserID = "001TEST";
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			declaration.Validation.ValidateJE_CustomsProfile();
			AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForTesting);
			AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForProduction);
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			declaration.Validation.ValidateJE_CustomsProfile();
			AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForTesting);
			AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForProduction);
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			extPswUva.GP_UserID = PasswordTypesList.Codes.TVA;
			extPswUva.GP_UserID = "001";
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			declaration.Validation.ValidateJE_CustomsProfile();
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForTesting);
			AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForProduction);
			extPswUva.GP_UserID = PasswordTypesList.Codes.UVC;
			extPswUva.GP_UserID = "001";
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			declaration.Validation.ValidateJE_CustomsProfile();
			AssertHasMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForTesting);
			AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForProduction);
			TWCustomsDataRegistry.Instance.TWIsTestMode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			extPswUva.GP_UserID = PasswordTypesList.Codes.TVA;
			extPswUva.GP_UserID = "001TEST";
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			declaration.Validation.ValidateJE_CustomsProfile();
			AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForTesting);
			AssertHasMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForProduction);
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			declaration.Validation.ValidateJE_CustomsProfile();
			AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForTesting);
			AssertHasMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForProduction);
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			extPswUva.GP_UserID = PasswordTypesList.Codes.UVC;
			extPswUva.GP_UserID = "001TEST";
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			declaration.Validation.ValidateJE_CustomsProfile();
			AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForTesting);
			AssertHasMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForProduction);
			extPswUva.GP_UserID = PasswordTypesList.Codes.TVA;
			extPswUva.GP_UserID = "001";
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			declaration.Validation.ValidateJE_CustomsProfile();
			AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForTesting);
			AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForProduction);
			extPswUva.GP_UserID = PasswordTypesList.Codes.UVC;
			extPswUva.GP_UserID = "001";
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			declaration.Validation.ValidateJE_CustomsProfile();
			AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForTesting);
			AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForProduction);
			var warningCertificateWillExpire = "The selected mail box certificate will expire on";
			declaration.JE_CustomsProfile = ZString.Empty;
			AssertNoWarningContaining(declaration.JE_CustomsProfileInfo, warningCertificateWillExpire);
			extPswUva.GP_ExpiryDate = new ZDate(2019, 12, 31);
			declaration.JE_CustomsProfile = "BBB-2";
			AssertHasWarningContaining(declaration.JE_CustomsProfileInfo, warningCertificateWillExpire);
			extPswUva.GP_ExpiryDate = new ZDate(2020, 1, 1);
			declaration.Validation.ValidateJE_CustomsProfile();
			AssertNoWarningContaining(declaration.JE_CustomsProfileInfo, warningCertificateWillExpire);
			extPswUva.GP_ExpiryDate = new ZDate(2019, 12, 30);
			declaration.Validation.ValidateJE_CustomsProfile();
			AssertHasWarningContaining(declaration.JE_CustomsProfileInfo, warningCertificateWillExpire);
			var messageErrorCertificateExpired = ValidationConstants.Declaration.CertificateExpired;
			extPswUva.GP_ExpiryDate = new ZDate(2019, 12, 31);
			declaration.Validation.ValidateJE_CustomsProfile();
			AssertNoMessageErrorContaining(declaration.JE_CustomsProfileInfo, messageErrorCertificateExpired);
			extPswUva.GP_ExpiryDate = new ZDate(2019, 11, 30);
			declaration.Validation.ValidateJE_CustomsProfile();
			AssertHasMessageErrorContaining(declaration.JE_CustomsProfileInfo, messageErrorCertificateExpired);
		}

		public void TestCheckJE_ApplicationCode_Mandatory()
		{
			CombineAssertions(() =>
			{
				var customsInterface = new LocalCountryCustomsInterface
				{
					RecipientID = "RecipientID",
					SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced
				};
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_ApplicationCode = ZString.Empty;
					AssertHasError("Empty", declaration.JE_ApplicationCodeInfo, ValidationConstants.Declaration.ApplicationCodeShouldNotBeEmpty);
					declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
					AssertNoError("Valid", declaration.JE_ApplicationCodeInfo, ValidationConstants.Declaration.ApplicationCodeShouldNotBeEmpty);
				}
			});
		}

		public void TestCheckJE_ApplicationCode_ListValidation()
		{
			CombineAssertions(() =>
			{
				var customsInterface = new LocalCountryCustomsInterface
				{
					RecipientID = "RecipientID",
					SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced
				};
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_ApplicationCode = "123";
					AssertHasMessageError("Invalid", declaration.JE_ApplicationCodeInfo, ListValidation.InvalidCodeMessageError);
					declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
					AssertNoMessageError("Valid", declaration.JE_ApplicationCodeInfo, ListValidation.InvalidCodeMessageError);
				}
			});
		}

		public void TestCheckJE_ApplicationCode_LocalCountryCustomsInterface_Interfaced()
		{
			var customsInterface = new LocalCountryCustomsInterface
			{
				RecipientID = "RecipientID",
				SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced
			};
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.Validation.ValidateJE_ApplicationCode();
				AssertEquals(false, declaration.JE_ApplicationCodeInfo.HasNotifications());
			}
		}

		public void TestCheckJE_ApplicationCode_LocalCountryCustomsInterface_Builtin()
		{
			var customsInterface = new LocalCountryCustomsInterface
			{
				SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted
			};
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.Validation.ValidateJE_ApplicationCode();
				AssertEquals(false, declaration.JE_ApplicationCodeInfo.HasNotifications());
			}
		}

		public void TestCheckJE_ApplicationCode_NotConfigureLocalCountryCustomsInterface_Empty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = ZString.Empty;
			AssertEquals(false, declaration.JE_ApplicationCodeInfo.HasNotifications());
		}

		public void TestCheckJE_ApplicationCode_NotConfigureLocalCountryCustomsInterface_Invalid()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "123";
			AssertEquals(false, declaration.JE_ApplicationCodeInfo.HasNotifications());
		}

		public void TestCheckJE_ContainerMode()
		{
			var info = this.declaration.JE_ContainerModeInfo;
			this.declaration.JE_TransportMode = this.declaration.TransportModeAirCodeForTesting;
			this.declaration.JE_ContainerMode = ZString.Empty;
			AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
			this.declaration.JE_ContainerMode = "XX";
			AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);
			this.declaration.JE_ContainerMode = this.declaration.Lookups.CargoIdTypeList[0].Code;
			AssertNoMessageErrors(info);

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			info = declaration.JE_ContainerModeInfo;
			declaration.JE_ContainerMode = ContainerModeList.Codes.Loose;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F3;
			AssertHasMessageErrorContaining(info, ValidationConstants.Declaration.ContainerModeMustbeOther(entryInstruction.CEI_Style));
			declaration.JE_ContainerMode = ContainerModeList.Codes.Other;
			AssertNoMessageErrorContaining(info, ValidationConstants.Declaration.ContainerModeMustbeOther(entryInstruction.CEI_Style));

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F4;
			declaration.JE_ContainerMode = ContainerModeList.Codes.Loose;
			AssertHasMessageErrorContaining(info, ValidationConstants.Declaration.ContainerModeMustbeOther(entryInstruction.CEI_Style));
			declaration.JE_ContainerMode = ContainerModeList.Codes.Other;
			AssertNoMessageErrorContaining(info, ValidationConstants.Declaration.ContainerModeMustbeOther(entryInstruction.CEI_Style));
		}

		public void TestCheckJE_RL_NKPortOfLoading()
		{
			var targetInfo = declaration.JE_RL_NKPortOfLoadingInfo;
			declaration.JE_RL_NKPortOfLoading = ZString.Empty;
			AssertNoMessageError(targetInfo, ExpectedMessageErrorDestinationPortCodeInvalid);
			declaration.JE_RL_NKPortOfLoading = "TWABC";
			AssertHasMessageError(targetInfo, ExpectedMessageErrorDestinationPortCodeInvalid);
			declaration.JE_RL_NKPortOfLoading = "TWTPE";
			AssertNoMessageError(targetInfo, ExpectedMessageErrorDestinationPortCodeInvalid);
		}

		public virtual void TestCheckJE_RL_NKFinalDestination()
		{
			var declaration = (this.declaration as JobDeclaration);
			var targetInfo = declaration.JE_RL_NKFinalDestinationInfo;
			declaration.JE_RL_NKFinalDestination = "WZ99";
			AssertHasMessageError(targetInfo, ValidationConstants.Declaration.Z99UNLOCOCodeMustBe5Characters);
			declaration.JE_RL_NKFinalDestination = "TWZ99";
			AssertNoMessageError(targetInfo, ValidationConstants.Declaration.Z99UNLOCOCodeMustBe5Characters);
			declaration.JE_RL_NKFinalDestination = "CNZ99";
			AssertNoMessageError(targetInfo, ValidationConstants.Declaration.Z99UNLOCOCodeMustBe5Characters);
			declaration.JE_RL_NKFinalDestination = "3XZ99";
			AssertHasMessageError(targetInfo, ValidationConstants.Declaration.Z99UNLOCOCodeMustHasValidCountryCode);
			var testCountry = Factory.New<RefCountry>();
			testCountry.RN_Code = "3X";
			testCountry.RN_Desc = "Country";
			declaration.JE_RL_NKFinalDestination = "3XZ99";
			AssertNoMessageError(targetInfo, ValidationConstants.Declaration.Z99UNLOCOCodeMustHasValidCountryCode);
		}

		public virtual void TestCheckJE_RL_NKOrigin()
		{
			var declaration = this.declaration as JobDeclaration;
			var targetInfo = declaration.JE_RL_NKOriginInfo;

			CombineAssertions("CheckZ99PortValidation", () =>
			{
				declaration.JE_RL_NKOrigin = "WZ99";
				AssertHasMessageError(targetInfo, ValidationConstants.Declaration.Z99UNLOCOCodeMustBe5Characters);
				declaration.JE_RL_NKOrigin = "TWZ99";
				AssertNoMessageError(targetInfo, ValidationConstants.Declaration.Z99UNLOCOCodeMustBe5Characters);
				declaration.JE_RL_NKOrigin = "CNZ99";
				AssertNoMessageError(targetInfo, ValidationConstants.Declaration.Z99UNLOCOCodeMustBe5Characters);
				declaration.JE_RL_NKOrigin = "3XZ99";
				AssertHasMessageError(targetInfo, ValidationConstants.Declaration.Z99UNLOCOCodeMustHasValidCountryCode);
				var testCountry = Factory.New<RefCountry>();
				testCountry.RN_Code = "3X";
				testCountry.RN_Desc = "Country";
				declaration.JE_RL_NKOrigin = "3XZ99";
				AssertNoMessageError(targetInfo, ValidationConstants.Declaration.Z99UNLOCOCodeMustHasValidCountryCode);
			});
		}

		public void TestCheckJE_RL_NKFinalDestination_ListValidation()
		{
			var declaration = this.declaration as JobDeclaration;
			var info = declaration.JE_RL_NKFinalDestinationInfo;
			CombineAssertions(() =>
			{
				declaration.CusEntryInstruction.CEI_Style = "";
				declaration.JE_RL_NKFinalDestination = ZString.Empty;
				AssertEquals("JE_RL_NKFinalDestination empty", false, HasMessageError());
				declaration.JE_RL_NKFinalDestination = "ZDSD@";
				AssertEquals("Invalid JE_RL_NKFinalDestination", true, HasMessageError());
				declaration.JE_RL_NKFinalDestination = "USLAX";
				AssertEquals("Valid JE_RL_NKFinalDestination", false, HasMessageError());
			}

			);
			bool HasMessageError() => info.HasMessageError(ExpectedMessageErrorDestinationPortCodeInvalid);
		}

		const string ExpectedMessageErrorDestinationPortCodeInvalid = "This port code is invalid. Please check against the transport mode and shipment type.";

		public void TestShouldCheckPortCodeInTW()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.CusEntryInstruction.CEI_Style = "";
			AssertPortCode(declaration.JE_RL_NKOriginInfo);
			AssertPortCode(declaration.JE_RL_NKFinalDestinationInfo);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.CusEntryInstruction.CEI_Style = "";
			AssertPortCode(declaration.JE_RL_NKOriginInfo);
			AssertPortCode(declaration.JE_RL_NKFinalDestinationInfo);
		}

		void AssertPortCode(ZPropertyInfo portCodeInfo)
		{
			portCodeInfo.Value = new ZString("Z99");
			AssertNoMessageErrorContaining(portCodeInfo, JobDeclarationValidation.MessageErrorPortCodeInvalid);
			portCodeInfo.Value = new ZString("XXXXX");
			AssertHasMessageErrorContaining(portCodeInfo, JobDeclarationValidation.MessageErrorPortCodeInvalid);
		}

		public void TestCheckTW1_CertificateType_JE_RL_NKOrigin()
		{
			AssertCheckTW1_CertificateType_JE_RL_NKOrigin(true, CertificateTypeList.Codes.Code1, ControllingMessageTypeList.Codes.NX101, ZString.Empty);
			AssertCheckTW1_CertificateType_JE_RL_NKOrigin(true, CertificateTypeList.Codes.Code7, ControllingMessageTypeList.Codes.NX101, ZString.Empty);
			AssertCheckTW1_CertificateType_JE_RL_NKOrigin(true, CertificateTypeList.Codes.Code8, ControllingMessageTypeList.Codes.NX101, ZString.Empty);
			AssertCheckTW1_CertificateType_JE_RL_NKOrigin(true, CertificateTypeList.Codes.Code10, ControllingMessageTypeList.Codes.NX101, ZString.Empty);
			AssertCheckTW1_CertificateType_JE_RL_NKOrigin(true, CertificateTypeList.Codes.Code15, ControllingMessageTypeList.Codes.NX101, ZString.Empty);
			AssertCheckTW1_CertificateType_JE_RL_NKOrigin(true, CertificateTypeList.Codes.Code16, ControllingMessageTypeList.Codes.NX101, ZString.Empty);
			AssertCheckTW1_CertificateType_JE_RL_NKOrigin(true, CertificateTypeList.Codes.Code17, ControllingMessageTypeList.Codes.NX101, ZString.Empty);

			AssertCheckTW1_CertificateType_JE_RL_NKOrigin(false, CertificateTypeList.Codes.Code2, ControllingMessageTypeList.Codes.NX101, ZString.Empty);
			AssertCheckTW1_CertificateType_JE_RL_NKOrigin(false, CertificateTypeList.Codes.Code1, ControllingMessageTypeList.Codes.X101, ZString.Empty);
			AssertCheckTW1_CertificateType_JE_RL_NKOrigin(false, CertificateTypeList.Codes.Code1, ControllingMessageTypeList.Codes.NX101, "AUSYD");
		}

		void AssertCheckTW1_CertificateType_JE_RL_NKOrigin(bool expectShowError, ZString certificateType, ZString messageType, ZString origin)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKOrigin = origin;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = messageType;
			messageHeader.TW1_CertificateType = certificateType;
			messageHeader.Validation.ValidateTW1_CertificateType();
			AssertEquals("no error when not link to CMHeader", false, declaration.JE_RL_NKOriginInfo.HasMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Port Of Origin")));

			var invoiceLine = (JobComInvoiceLine)invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			messageHeader.TW1_ControllingAgency = "XX";
			var link = invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.ControllingAgency == "XX");
			link.IsLinkedCMHeader = true;
			messageHeader.TW1_CertificateType = certificateType;
			AssertEquals("Port of Origin", expectShowError, declaration.JE_RL_NKOriginInfo.HasMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Port Of Origin")));
		}

		public void TestCheckJE_MergeBy()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testInstruction1 = testDeclaration.CustomsEntryInstructions.AddNew();
			var mergeByInfo = testDeclaration.JE_MergeByInfo;
			testDeclaration.JE_MergeBy = ZString.Empty;
			AssertHasMessageErrorContaining(mergeByInfo, "You have not entered a Merge By");
			testDeclaration.JE_MergeBy = "6";
			AssertNoMessageErrorContaining(mergeByInfo, "You have not entered a Merge By");
			AssertHasMessageErrorContaining(mergeByInfo, "you have selected is not in the list");
			testDeclaration.JE_MergeBy = MergeByCodeList.Codes.CondensedDeclaration;
			AssertNoMessageErrorContaining(mergeByInfo, "you have selected is not in the list");
			AssertNoMessageErrorContaining(mergeByInfo, "You have not entered a Merge By");
			var condensedDeclarationTypes = new List<ZString> { Constants.DeclarationTypes.Import.G1, Constants.DeclarationTypes.Import.G2, Constants.DeclarationTypes.Export.G5 };
			testDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertMergeByByDeclarationType(testInstruction1, mergeByInfo, condensedDeclarationTypes);
			testDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertMergeByByDeclarationType(testInstruction1, mergeByInfo, condensedDeclarationTypes);
			testInstruction1.CEI_ExamMode = ExamModeList.Codes.FactoryInspection;
			testDeclaration.Validation.ValidateJE_MergeBy();
			AssertHasMessageErrorContaining(mergeByInfo, "A declaration can only be condensed when its Examination Mode is 8");
			testInstruction1.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
			testDeclaration.Validation.ValidateJE_MergeBy();
			AssertNoMessageErrorContaining(mergeByInfo, "A declaration can only be condensed when its Examination Mode is 8");
			var invoiceHeader = testDeclaration.Invoices.AddNew();
			for (int i = 0; i < 55; i++)
			{
				var invoiceLine = testDeclaration.InvoiceLines.AddNew();
				invoiceLine.JI_JZ = invoiceHeader.PK;
				invoiceLine.JI_CEI = testInstruction1.PK;
				testDeclaration.Validation.ValidateJE_MergeBy();
				if ((i + 1) < 50)
				{
					AssertHasMessageErrorContaining(mergeByInfo, "A condensed declaration must have more than 50 invoice lines");
				}
				else
				{
					AssertNoMessageErrorContaining(mergeByInfo, "A condensed declaration must have more than 50 invoice lines");
				}
			}

			testDeclaration.InvoiceLines[0].JI_Gears = 1;
			testDeclaration.Validation.ValidateJE_MergeBy();
			AssertHasMessageErrorContaining(mergeByInfo, "A declaration with car information reporting requirements cannot be condensed");
			testDeclaration.InvoiceLines[0].JI_Gears = ZShort.Zero;
			testDeclaration.Validation.ValidateJE_MergeBy();
			AssertNoMessageErrorContaining(mergeByInfo, "A declaration with car information reporting requirements cannot be condensed");
			testInstruction1.CEI_DutyRefund = true;
			testDeclaration.Validation.ValidateJE_MergeBy();
			AssertHasMessageErrorContaining(mergeByInfo, "A condensed declaration cannot apply for Duty Refund");
			testInstruction1.CEI_DutyRefund = false;
			testDeclaration.Validation.ValidateJE_MergeBy();
			AssertNoMessageErrorContaining(mergeByInfo, "A condensed declaration cannot apply for Duty Refund");
			AssertNoMessageErrorContaining(mergeByInfo, "A declaration with controlling reporting requirements cannot be condensed");
			var controllingMessageHeader = testInstruction1.ControllingMessageHeaders.AddNew();
			testDeclaration.Validation.ValidateJE_MergeBy();
			AssertNoMessageErrorContaining(mergeByInfo, "A declaration with controlling reporting requirements cannot be condensed");
			controllingMessageHeader.PermitNumber = "1";
			testDeclaration.Validation.ValidateJE_MergeBy();
			AssertHasMessageErrorContaining(mergeByInfo, "A declaration with controlling reporting requirements cannot be condensed");
		}

		void AssertMergeByByDeclarationType(CusEntryInstruction testInstruction1, ZPropertyInfo mergeByInfo, List<ZString> condensedDeclarationTypes)
		{
			foreach (CodeDescriptionPair codeDescription in testInstruction1.Lookups.StyleList)
			{
				testInstruction1.CEI_Style = codeDescription.Code;
				testInstruction1.JobDeclaration.Validation.ValidateJE_MergeBy();
				if (!condensedDeclarationTypes.Contains(codeDescription.Code))
				{
					AssertHasMessageErrorContaining(mergeByInfo, "A declaration with a bonded declaration type cannot be condensed");
				}
				else
				{
					AssertNoMessageErrorContaining(mergeByInfo, "A declaration with a bonded declaration type cannot be condensed");
				}
			}
		}

		[TestDate(2020, 3, 30)]
		public virtual void TestCheckJE_OH_Supplier()
		{
			var targetInfo = declaration.JE_OH_SupplierInfo;
			var errorMessage = ValidationConstants.Declaration.TheOrganizationShouldHaveEnglishCompanyName;
			var supplier = Factory.New<OrgHeader>();
			supplier.Addresses.RemoveAndDeleteAll();
			supplier.OH_Code = "supplier";
			var mainAddress = supplier.Addresses[0];
			mainAddress.OA_Language = Core.SharedConstants.Languages.English;
			mainAddress.CompanyName = "Company Name";
			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_OH_Supplier = supplier.PK;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_OH_Supplier = supplier.PK;
			mainAddress.OA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertHasMessageErrorContaining(targetInfo, errorMessage);
			var entranslatedAddress = mainAddress.TranslatedAddresses.AddNew();
			entranslatedAddress.OTA_Language = Core.SharedConstants.Languages.English;
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertHasMessageErrorContaining(targetInfo, errorMessage);
			entranslatedAddress.OTA_CompanyName = "Translate CompanyName";
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertNoMessageErrorContaining(targetInfo, errorMessage);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			using (declaration.SuspendValidationTesting())
			{
				var poa1 = supplier.RequiredDocuments.AddNew("POA");
				poa1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				poa1.EQ_DateReceived = new ZDateTimeOffset(2020, 1, 1);
				poa1.EQ_ValidToDate = new ZDateTime(2020, 3, 2);
				declaration.JE_MessageType = "IMP";
				declaration.Validation.ValidateJE_OH_Supplier();
				AssertNoWarnings(targetInfo);
				AssertNoMessageErrors(targetInfo);
				declaration.JE_MessageType = "EXP";
				declaration.Validation.ValidateJE_OH_Supplier();
				AssertHasMessageError(targetInfo, "The Power of Attorney Document on the organization (eDocs > Document Tracking) expired on 02-Mar-20.");
				var poa2 = supplier.RequiredDocuments.AddNew("POA");
				poa2.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				poa2.EQ_DateReceived = new ZDateTimeOffset(2020, 1, 1);
				poa2.EQ_ValidToDate = new ZDateTime(2020, 3, 3);
				declaration.Validation.ValidateJE_OH_Supplier();
				AssertHasMessageError(targetInfo, "The Power of Attorney Document on the organization (eDocs > Document Tracking) expired on 03-Mar-20.");
				var poc = supplier.RequiredDocuments.AddNew("POC");
				poc.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				poc.EQ_DateReceived = new ZDateTimeOffset(2020, 1, 1);
				poc.EQ_ValidToDate = new ZDateTime(2020, 3, 4);
				declaration.Validation.ValidateJE_OH_Supplier();
				AssertHasMessageError(targetInfo, "The Power of Attorney Document on the organization (eDocs > Document Tracking) expired on 04-Mar-20.");
				var poc2 = supplier.RequiredDocuments.AddNew("POC");
				poc2.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				poc2.EQ_DateReceived = new ZDateTimeOffset(2020, 1, 1);
				poc2.EQ_ValidToDate = new ZDateTime(2020, 4, 1);
				declaration.Validation.ValidateJE_OH_Supplier();
				AssertHasWarning(targetInfo, "The Power of Attorney Document on the organization (eDocs > Document Tracking) will expire on 01-Apr-20.");
			}
		}

		[TestDate(2020, 3, 30)]
		public virtual void TestCheckJE_OH_SupplierPowerOfAttorneyDocument()
		{
			var declaration = this.declaration as JobDeclaration;
			var targetInfo = declaration.JE_OH_SupplierInfo;
			var supplier = TestTWCreator.CreateOrganizationForPowerOfAttorneyDocument();
			declaration.JE_MessageType = "EXP";
			declaration.JE_OH_Supplier = supplier.PK;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "BA";
			var requiredDocument = supplier.RequiredDocuments[0];
			var attribute = requiredDocument.Attributes[0];
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			using (declaration.SuspendValidationTesting())
			{
				var validation = declaration.Validation;
				requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Carrier;
				validation.ValidateJE_OH_Supplier();
				AssertHasMessageError(targetInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for TW and/or this direction.");
				requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
				validation.ValidateJE_OH_Supplier();
				AssertNoMessageError(targetInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for TW and/or this direction.");
				requiredDocument.EQ_DocNumber = "";
				validation.ValidateJE_OH_Supplier();
				AssertHasMessageError(targetInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for TW and/or this direction.");
				requiredDocument.EQ_DocNumber = "1234";
				validation.ValidateJE_OH_Supplier();
				AssertNoMessageError(targetInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for TW and/or this direction.");
				requiredDocument.EQ_RN_NKRelatedCountry = "CN";
				validation.ValidateJE_OH_Supplier();
				AssertHasMessageError(targetInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for TW and/or this direction.");
				requiredDocument.EQ_RN_NKRelatedCountry = "TW";
				validation.ValidateJE_OH_Supplier();
				AssertNoMessageError(targetInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for TW and/or this direction.");
				attribute.D0_AttribValue = "C";
				validation.ValidateJE_OH_Supplier();
				AssertHasMessageError(targetInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for TW and/or this direction.");
				attribute.D0_AttribValue = "B";
				validation.ValidateJE_OH_Supplier();
				AssertNoMessageError(targetInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for TW and/or this direction.");
				requiredDocument.EQ_ValidToDate = new ZDateTime(2020, 3, 2);
				validation.ValidateJE_OH_Supplier();
				AssertHasMessageErrorContaining(targetInfo, "The Power of Attorney Document on the organization (eDocs > Document Tracking) expired on");
				requiredDocument.EQ_ValidToDate = new ZDateTime(2020, 5, 1);
				validation.ValidateJE_OH_Supplier();
				AssertNoMessageErrorContaining(targetInfo, "The Power of Attorney Document on the organization (eDocs > Document Tracking) expired on");
				requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyForwarding;
				validation.ValidateJE_OH_Supplier();
				AssertHasMessageError(targetInfo, "There is no Power of Attorney against this organization. Please place your cursor on the organization and press F3 to edit it. The document can be added to the eDocs tab.");
				requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
				validation.ValidateJE_OH_Supplier();
				AssertNoMessageError(targetInfo, "There is no Power of Attorney against this organization. Please place your cursor on the organization and press F3 to edit it. The document can be added to the eDocs tab.");
				requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
				validation.ValidateJE_OH_Supplier();
				AssertNoMessageError(targetInfo, "There is no Power of Attorney against this organization. Please place your cursor on the organization and press F3 to edit it. The document can be added to the eDocs tab.");
				requiredDocument.EQ_DocCategory = Core.Constants.ReferenceTypes.Accounting;
				validation.ValidateJE_OH_Supplier();
				AssertHasMessageError(targetInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for TW and/or this direction.");
				requiredDocument.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
				validation.ValidateJE_OH_Supplier();
				AssertNoMessageError(targetInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for TW and/or this direction.");
			}
		}

		[TestDate(2020, 3, 30)]
		public virtual void TestCheckJE_OH_Importer()
		{
			var targetInfo = declaration.JE_OH_ImporterInfo;
			var errorMessage = ValidationConstants.Declaration.TheOrganizationShouldHaveEnglishCompanyName;
			var importer = Factory.New<OrgHeader>();
			importer.Addresses.RemoveAndDeleteAll();
			importer.OH_Code = "importer";
			var mainAddress = importer.Addresses[0];
			mainAddress.OA_Language = Core.SharedConstants.Languages.English;
			mainAddress.CompanyName = "Company Name";
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_OH_Importer = importer.PK;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_OH_Importer = importer.PK;
			mainAddress.OA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageErrorContaining(targetInfo, errorMessage);
			var entranslatedAddress = mainAddress.TranslatedAddresses.AddNew();
			entranslatedAddress.OTA_Language = Core.SharedConstants.Languages.English;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageErrorContaining(targetInfo, errorMessage);
			entranslatedAddress.OTA_CompanyName = "Translate CompanyName";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageErrorContaining(targetInfo, errorMessage);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			using (declaration.SuspendValidationTesting())
			{
				var poa1 = importer.RequiredDocuments.AddNew("POA");
				poa1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				poa1.EQ_DateReceived = new ZDateTimeOffset(2020, 1, 1);
				poa1.EQ_ValidToDate = new ZDateTime(2020, 3, 2);
				declaration.JE_MessageType = "EXP";
				declaration.Validation.ValidateJE_OH_Importer();
				AssertNoWarnings(targetInfo);
				AssertNoMessageErrors(targetInfo);
				declaration.JE_MessageType = "IMP";
				declaration.Validation.ValidateJE_OH_Importer();
				AssertHasMessageError(targetInfo, "The Power of Attorney Document on the organization (eDocs > Document Tracking) expired on 02-Mar-20.");
				var poa2 = importer.RequiredDocuments.AddNew("POA");
				poa2.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				poa2.EQ_DateReceived = new ZDateTimeOffset(2020, 1, 1);
				poa2.EQ_ValidToDate = new ZDateTime(2020, 3, 3);
				declaration.Validation.ValidateJE_OH_Importer();
				AssertHasMessageError(targetInfo, "The Power of Attorney Document on the organization (eDocs > Document Tracking) expired on 03-Mar-20.");
				var poc = importer.RequiredDocuments.AddNew("POC");
				poc.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				poc.EQ_DateReceived = new ZDateTimeOffset(2020, 1, 1);
				poc.EQ_ValidToDate = new ZDateTime(2020, 3, 4);
				declaration.Validation.ValidateJE_OH_Importer();
				AssertHasMessageError(targetInfo, "The Power of Attorney Document on the organization (eDocs > Document Tracking) expired on 04-Mar-20.");
				var poc2 = importer.RequiredDocuments.AddNew("POC");
				poc2.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				poc2.EQ_DateReceived = new ZDateTimeOffset(2020, 1, 1);
				poc2.EQ_ValidToDate = new ZDateTime(2020, 4, 1);
				declaration.Validation.ValidateJE_OH_Importer();
				AssertHasWarning(targetInfo, "The Power of Attorney Document on the organization (eDocs > Document Tracking) will expire on 01-Apr-20.");
			}
		}

		[TestDate(2020, 3, 30)]
		public virtual void TestCheckJE_OH_ImporterPowerOfAttorneyDocument()
		{
			var declaration = this.declaration as JobDeclaration;
			var targetInfo = declaration.JE_OH_ImporterInfo;
			var importer = TestTWCreator.CreateOrganizationForPowerOfAttorneyDocument();
			declaration.JE_MessageType = "IMP";
			declaration.JE_OH_Importer = importer.PK;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "BA";
			var requiredDocument = importer.RequiredDocuments[0];
			var attribute = requiredDocument.Attributes[0];
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			using (declaration.SuspendValidationTesting())
			{
				var validation = declaration.Validation;
				requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Carrier;
				validation.ValidateJE_OH_Importer();
				AssertHasMessageError(targetInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for TW and/or this direction.");
				requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
				validation.ValidateJE_OH_Importer();
				AssertNoMessageError(targetInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for TW and/or this direction.");
				requiredDocument.EQ_DocNumber = "";
				validation.ValidateJE_OH_Importer();
				AssertHasMessageError(targetInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for TW and/or this direction.");
				requiredDocument.EQ_DocNumber = "1234";
				validation.ValidateJE_OH_Importer();
				AssertNoMessageError(targetInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for TW and/or this direction.");
				requiredDocument.EQ_RN_NKRelatedCountry = "CN";
				validation.ValidateJE_OH_Importer();
				AssertHasMessageError(targetInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for TW and/or this direction.");
				requiredDocument.EQ_RN_NKRelatedCountry = "TW";
				validation.ValidateJE_OH_Importer();
				AssertNoMessageError(targetInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for TW and/or this direction.");
				attribute.D0_AttribValue = "C";
				validation.ValidateJE_OH_Importer();
				AssertHasMessageError(targetInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for TW and/or this direction.");
				attribute.D0_AttribValue = "B";
				validation.ValidateJE_OH_Importer();
				AssertNoMessageError(targetInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for TW and/or this direction.");
				requiredDocument.EQ_ValidToDate = new ZDateTime(2020, 3, 2);
				validation.ValidateJE_OH_Importer();
				AssertHasMessageErrorContaining(targetInfo, "The Power of Attorney Document on the organization (eDocs > Document Tracking) expired on");
				requiredDocument.EQ_ValidToDate = new ZDateTime(2020, 5, 1);
				validation.ValidateJE_OH_Importer();
				AssertNoMessageErrorContaining(targetInfo, "The Power of Attorney Document on the organization (eDocs > Document Tracking) expired on");
				requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyForwarding;
				validation.ValidateJE_OH_Importer();
				AssertHasMessageError(targetInfo, "There is no Power of Attorney against this organization. Please place your cursor on the organization and press F3 to edit it. The document can be added to the eDocs tab.");
				requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
				validation.ValidateJE_OH_Importer();
				AssertNoMessageError(targetInfo, "There is no Power of Attorney against this organization. Please place your cursor on the organization and press F3 to edit it. The document can be added to the eDocs tab.");
				requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
				validation.ValidateJE_OH_Importer();
				AssertNoMessageError(targetInfo, "There is no Power of Attorney against this organization. Please place your cursor on the organization and press F3 to edit it. The document can be added to the eDocs tab.");
				requiredDocument.EQ_DocCategory = Core.Constants.ReferenceTypes.Accounting;
				validation.ValidateJE_OH_Importer();
				AssertHasMessageError(targetInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for TW and/or this direction.");
				requiredDocument.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
				validation.ValidateJE_OH_Importer();
				AssertNoMessageError(targetInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for TW and/or this direction.");
			}
		}

		public override void TestPackagesActualPackageCount_Validation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_TotalNoOfPacks = 100;
			var info = declaration.PackagesActualPackageCountInfo;
			var messageError = JobDeclarationValidation.TotalPackageCountDoesNotMatchErrorMessage;
			Assert("No message errors", !info.HasMessageError(messageError));
			var packGroup = declaration.PackingGroups.AddNew();
			var package = packGroup.Packages.AddNew();
			Assert("No message errors", !info.HasMessageError(messageError));
			package.CW_PackQty = 100;
			Assert("No message errors", !info.HasMessageError(messageError));
			declaration.JE_TotalNoOfPacks = 10;
			Assert("No message errors", !info.HasMessageError(messageError));
			package.CW_PackQty = 10;
			Assert("No message errors", !info.HasMessageError(messageError));
			declaration.Packages.RemoveAndDeleteAll();
			Assert("No message errors", !info.HasMessageError(messageError));
		}

		public void TestCheckJE_TotalNoOfPacks()
		{
			var targetInfo = declaration.JE_TotalNoOfPacksInfo;
			declaration.JE_TotalNoOfPacksPackType = ZString.Empty;
			declaration.JE_TotalNoOfPacks = ZInt.Zero;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeZero);
			declaration.JE_TotalNoOfPacks = 1;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeZero);
			declaration.JE_TotalNoOfPacks = 100000000;
			AssertHasError(targetInfo, "The number 100000000 is too large, the maximum value allowed for Total Packages No. is 99,999,999.");
			declaration.JE_TotalNoOfPacks = 99999999;
			AssertNoErrors(targetInfo);
		}

		public new void TestJE_TotalNoOfPacksActualHeaderNoOfPacks()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TotalNoOfPacksPackType = "CTN";
			declaration.JE_TotalNoOfPacks = 10;
			var invoice = declaration.Invoices.AddNew();
			declaration.Validation.ValidateJE_TotalNoOfPacks();
			AssertNoWarningContaining(declaration.JE_TotalNoOfPacksInfo, "The sum of all invoice header package numbers");
			AssertNoWarningContaining(declaration.JE_TotalNoOfPacksInfo, "does not balance with the declaration total package number");
			invoice.JZ_NoOfPacks = 5;
			declaration.Validation.ValidateJE_TotalNoOfPacks();
			AssertHasWarningContaining(declaration.JE_TotalNoOfPacksInfo, "The sum of all invoice header package numbers");
			AssertHasWarningContaining(declaration.JE_TotalNoOfPacksInfo, "does not balance with the declaration total package number");
			Assert(declaration.JE_TotalNoOfPacksInfo.HasWarning("The sum of all invoice header package numbers {5 CTN} does not balance with the declaration total package number {10 CTN}."));
		}

		public new void TestJE_TotalNoOfPacks_OM_IMBalanceInvoicePackage()
		{
			Assert("Work Item WI00282059 modify the CheckJE_TotalNoOfPacks", true);
		}

		public void TestCheckJE_TotalNoOfPacksPackType()
		{
			var targetInfo = declaration.JE_TotalNoOfPacksPackTypeInfo;
			declaration.JE_TotalNoOfPacks = ZInt.Zero;
			declaration.JE_TotalNoOfPacksPackType = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TotalNoOfPacksPackType = "PLT";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TotalNoOfPacks = 1;
			declaration.JE_TotalNoOfPacksPackType = ZString.Empty;
			Assert("Total Packages Type should not have the warning message: 'Package type is required when package is greater than 0.'", !declaration.JE_TotalNoOfPacksPackTypeInfo.HasWarning("Package type is required when package is greater than 0."));
		}

		public void TestCheckJE_TotalWeight()
		{
			var testDecl = this.declaration as JobDeclaration;
			var targetInfo = testDecl.JE_TotalWeightInfo;
			AssertNoNotifications(targetInfo);
			testDecl.JE_TotalWeight = 0;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			testDecl.JE_TotalWeight = 1;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			testDecl.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			testDecl.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = testDecl.CusEntryInstruction;

			var invoice = testDecl.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
			invoice.JZ_IncoTerm = "FOB";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Tariff = "8419.20.00.00-5";
			invoiceLine.JI_Procedure = "50";
			invoiceLine.JI_InvoiceQuantity = 1m;
			invoiceLine.JI_InvoiceUQ = "PCE";
			invoiceLine.JI_EnteredUnitPrice = 7738.2m;
			invoiceLine.JI_NetWeight = 1.125m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			testDecl.JE_TotalWeight = 1.000m;

			testDecl.ResumeApportionment();
			testDecl.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			testDecl.Validation.ValidateJE_TotalWeight();
			AssertHasMessageError(targetInfo, ValidationConstants.CusEntryHeader.NetWeightNotBeGreaterThanGrossWeight);
			testDecl.JE_TotalWeight = 1.125m;
			AssertNoMessageError(targetInfo, ValidationConstants.CusEntryHeader.NetWeightNotBeGreaterThanGrossWeight);
		}

		public override void TestJE_ShipmentIncoTerm()
		{
			Assert("Precondition: No error", !declaration.JE_ShipmentIncoTermInfo.HasErrors());
			declaration.JE_ShipmentIncoTerm = "XYZ";
			AssertHasMessageErrors(declaration.JE_ShipmentIncoTermInfo);
			declaration.JE_ShipmentIncoTerm = declaration.Lookups.IncoTermList[0].Code;
			Assert("No error after selecting valid incoterm", !declaration.JE_ShipmentIncoTermInfo.HasErrors());
		}

		TestTWCreator TestTWCreator
		{
			get
			{
				return testTWCreator ?? (testTWCreator = new TestTWCreator(Factory));
			}
		}

		TestTWCreator testTWCreator;
		public void TestCheckDeclarationNumberDisplay()
		{
			var customsOfficeAR = CreateZZRefCusCodeListCombinedForTestEntryNumberWhenChanged("AR");
			var customsOfficeSE = CreateZZRefCusCodeListCombinedForTestEntryNumberWhenChanged("BE");
			new TestTWCreator(Factory).CreateRegistryItemCusBrokerageBoxNumber();
			var orgHeaderCBF = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeaderEPZ = Factory.NewWithValidTestData<OrgHeader>();
			var orgCBFCode = orgHeaderCBF.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "00612348", Core.Constants.CountryCodes.Taiwan);
			var orgEPZCode = orgHeaderEPZ.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, "00987654", Core.Constants.CountryCodes.Taiwan);
			var cbfAddress = orgHeaderCBF.Addresses.AddNew();
			cbfAddress.Address1 = "Address1";
			cbfAddress.Address2 = "Address2";
			orgCBFCode.OK_OA_PremisesAddress = orgHeaderCBF.MainAddress.PK;
			orgEPZCode.OK_OA_PremisesAddress = orgHeaderEPZ.MainAddress.PK;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.JE_MessageType = "EXP";
			declaration.JE_CustomsProfile = "123-3";
			declaration.JE_CustomsOffice = "AR";
			declaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeaderCBF.MainAddress.PK;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeaderCBF.MainAddress.PK;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = "F2";
			entryInstruction.CEI_CustomsOffice = "BE";
			entryInstruction.CEI_BoxNumber = "100";
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 08, 05);
			CombineAssertions(() =>
			{
				declaration.EntryNumber = EntryNumberGenerator.New(declaration).GenerateEntryNumber();
				AssertEquals("BEF20910000001", declaration.EntryNumber);
				var errorMessage = ValidationConstants.AllocateNumber.KeyComponentValuesChanged;
				entryInstruction.CEI_CustomsOffice = "AR";
				declaration.RunPreSaveValidation();
				AssertHasMessageErrorContaining(declaration.DeclarationNumberDisplayInfo, errorMessage);
				entryInstruction.CEI_CustomsOffice = "BE";
				declaration.RunPreSaveValidation();
				AssertNoMessageErrorContaining(declaration.DeclarationNumberDisplayInfo, errorMessage);
				entryInstruction.CEI_Style = "F3";
				declaration.RunPreSaveValidation();
				AssertHasMessageErrorContaining(declaration.DeclarationNumberDisplayInfo, errorMessage);
				entryInstruction.CEI_Style = "F2";
				declaration.RunPreSaveValidation();
				AssertNoMessageErrorContaining(declaration.DeclarationNumberDisplayInfo, errorMessage);
				entryInstruction.CEI_DateForDuty = new ZDateTime(2021, 08, 05);
				declaration.RunPreSaveValidation();
				AssertHasMessageErrorContaining(declaration.DeclarationNumberDisplayInfo, errorMessage);
				entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 08, 05);
				declaration.RunPreSaveValidation();
				AssertNoMessageErrorContaining(declaration.DeclarationNumberDisplayInfo, errorMessage);
				entryInstruction.CEI_BoxNumber = "456";
				declaration.RunPreSaveValidation();
				AssertHasMessageErrorContaining(declaration.DeclarationNumberDisplayInfo, errorMessage);
				entryInstruction.CEI_BoxNumber = "100";
				declaration.RunPreSaveValidation();
				AssertNoMessageErrorContaining(declaration.DeclarationNumberDisplayInfo, errorMessage);
				entryInstruction.CEI_Style = "B1";
				declaration.EntryNumber = EntryNumberGenerator.New(declaration).GenerateEntryNumber();
				AssertEquals("BEB10923480001", declaration.EntryNumber);
				declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeaderEPZ.MainAddress.PK;
				declaration.RunPreSaveValidation();
				AssertHasMessageErrorContaining(declaration.DeclarationNumberDisplayInfo, errorMessage);
				declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeaderCBF.MainAddress.PK;
				declaration.RunPreSaveValidation();
				AssertNoMessageErrorContaining(declaration.DeclarationNumberDisplayInfo, errorMessage);
				entryInstruction.CEI_Style = "B2";
				declaration.EntryNumber = EntryNumberGenerator.New(declaration).GenerateEntryNumber();
				AssertEquals("BEB20923480002", declaration.EntryNumber);
				declaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeaderEPZ.MainAddress.PK;
				declaration.RunPreSaveValidation();
				AssertHasMessageErrorContaining(declaration.DeclarationNumberDisplayInfo, errorMessage);
				declaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeaderCBF.MainAddress.PK;
				declaration.RunPreSaveValidation();
				AssertNoMessageErrorContaining(declaration.DeclarationNumberDisplayInfo, errorMessage);
				entryInstruction.CEI_Style = "G1";
				declaration.EntryNumber = EntryNumberGenerator.New(declaration).GenerateEntryNumber();
				AssertEquals("BEAR0910000001", declaration.EntryNumber);
				declaration.JE_CustomsOffice = "BE";
				declaration.RunPreSaveValidation();
				AssertHasMessageErrorContaining(declaration.DeclarationNumberDisplayInfo, errorMessage);
				declaration.JE_CustomsOffice = "AR";
				declaration.RunPreSaveValidation();
				AssertNoMessageErrorContaining(declaration.DeclarationNumberDisplayInfo, errorMessage);
			}

			);
		}

		public void TestCheckDeclarationNumberDisplayWhenEntryNumberIsEmpty()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = "AWO";
			var message = "You have not allocate an Entry number.";
			var warningMessage = "You have not allocated an Entry Number. The system will automatically allocate entry number when sending the message.";
			using (TWCustomsDataRegistry.Instance.ValidateEntryNumber.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				declaration.Validation.ValidateDeclarationNumberDisplay();
				AssertHasMessageError(declaration.DeclarationNumberDisplayInfo, message);
				AssertNoWarning(declaration.DeclarationNumberDisplayInfo, warningMessage);

				entry.EntryNumber = "CABB0999900001";
				declaration.Validation.ValidateDeclarationNumberDisplay();
				AssertNoMessageError(declaration.DeclarationNumberDisplayInfo, message);
				AssertNoWarning(declaration.DeclarationNumberDisplayInfo, warningMessage);
			}

			using (TWCustomsDataRegistry.Instance.ValidateEntryNumber.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				entry.EntryNumber = ZString.Empty;
				declaration.Validation.ValidateDeclarationNumberDisplay();
				AssertNoMessageError(declaration.DeclarationNumberDisplayInfo, message);
				AssertHasWarning(declaration.DeclarationNumberDisplayInfo, warningMessage);
			}
		}

		public void TestCheckJE_OH_ExporterCompanyNameLength()
		{
			var foreignWarning = "Foreign Company Name Only the first 80 characters will be sent to the customs.";
			var chineseWarning = "Chinese Company Name Only the first 70 characters will be sent to the customs.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			var targetInfo = declaration.JE_OH_ExporterInfo;
			var org = Factory.New<OrgHeader>();
			var mainAddress = org.MainAddress;
			mainAddress.Language = "EN";
			mainAddress.CompanyName = new ZString('A', 80);
			var transAddress = mainAddress.TranslatedAddresses.AddNew();
			transAddress.Language = "ZH-TW";
			transAddress.CompanyName = new ZString('A', 70);
			declaration.JE_OH_Exporter = org.PK;
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.Validation.ValidateJE_OH_Exporter();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);
			transAddress.CompanyName = new ZString('A', 71);
			declaration.Validation.ValidateJE_OH_Exporter();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.Validation.ValidateJE_OH_Exporter();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
			mainAddress.CompanyName = new ZString('A', 81);
			declaration.Validation.ValidateJE_OH_Exporter();
			AssertHasWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.Validation.ValidateJE_OH_Exporter();
			AssertHasWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
		}

		public void TestCheckJE_OH_ExporterAddressLength()
		{
			var foreignWarning = "Foreign Address Only the first 120 characters will be sent to the customs.";
			var chineseWarning = "Chinese Address Only the first 100 characters will be sent to the customs.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			var targetInfo = declaration.JE_OH_ExporterInfo;
			var org = Factory.New<OrgHeader>();
			var mainAddress = org.MainAddress;
			mainAddress.Language = "EN";
			mainAddress.CompanyName = "CompanyName";
			mainAddress.City = "Taipei City";
			mainAddress.Postcode = "239";
			mainAddress.OA_RN_NKCountryCode = "TW";
			mainAddress.Address1 = new ZString('A', 50);
			mainAddress.Address2 = new ZString('B', 46);
			var transAddress = mainAddress.TranslatedAddresses.AddNew();
			transAddress.Language = "ZH-TW";
			transAddress.CompanyName = "CompanyName";
			transAddress.City = "Taipei";
			transAddress.Postcode = "239";
			transAddress.Address1 = new ZString('C', 49);
			transAddress.Address2 = new ZString('D', 42);
			declaration.JE_OH_Exporter = org.PK;
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.Validation.ValidateJE_OH_Exporter();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);
			transAddress.Address2 = new ZString('D', 43);
			declaration.Validation.ValidateJE_OH_Exporter();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.Validation.ValidateJE_OH_Exporter();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
			mainAddress.Address2 = new ZString('B', 47);
			declaration.Validation.ValidateJE_OH_Exporter();
			AssertHasWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.Validation.ValidateJE_OH_Exporter();
			AssertHasWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
		}

		public void TestCheckJE_OH_ConsigneeCompanyNameLength()
		{
			var foreignWarning = "Foreign Company Name Only the first 80 characters will be sent to the customs.";
			var chineseWarning = "Chinese Company Name Only the first 70 characters will be sent to the customs.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			var targetInfo = declaration.JE_OH_ConsigneeInfo;
			var org = Factory.New<OrgHeader>();
			var mainAddress = org.MainAddress;
			mainAddress.Language = "EN";
			mainAddress.CompanyName = new ZString('A', 80);
			var transAddress = mainAddress.TranslatedAddresses.AddNew();
			transAddress.Language = "ZH-TW";
			transAddress.CompanyName = new ZString('A', 70);
			declaration.JE_OH_Consignee = org.PK;
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.Validation.ValidateJE_OH_Consignee();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);
			transAddress.CompanyName = new ZString('A', 71);
			declaration.Validation.ValidateJE_OH_Consignee();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.Validation.ValidateJE_OH_Consignee();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
			mainAddress.CompanyName = new ZString('A', 81);
			declaration.Validation.ValidateJE_OH_Consignee();
			AssertHasWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.Validation.ValidateJE_OH_Consignee();
			AssertHasWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
		}

		public void TestCheckJE_OH_ConsigneeAddressLength()
		{
			var foreignWarning = "Foreign Address Only the first 120 characters will be sent to the customs.";
			var chineseWarning = "Chinese Address Only the first 100 characters will be sent to the customs.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			var targetInfo = declaration.JE_OH_ConsigneeInfo;
			var org = Factory.New<OrgHeader>();
			var mainAddress = org.MainAddress;
			mainAddress.Language = "EN";
			mainAddress.CompanyName = "CompanyName";
			mainAddress.City = "Taipei City";
			mainAddress.Postcode = "239";
			mainAddress.OA_RN_NKCountryCode = "TW";
			mainAddress.Address1 = new ZString('A', 50);
			mainAddress.Address2 = new ZString('B', 46);
			var transAddress = mainAddress.TranslatedAddresses.AddNew();
			transAddress.Language = "ZH-TW";
			transAddress.CompanyName = "CompanyName";
			transAddress.City = "Taipei";
			transAddress.Postcode = "239";
			transAddress.Address1 = new ZString('C', 49);
			transAddress.Address2 = new ZString('D', 42);
			declaration.JE_OH_Consignee = org.PK;
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.Validation.ValidateJE_OH_Consignee();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);
			transAddress.Address2 = new ZString('D', 43);
			declaration.Validation.ValidateJE_OH_Consignee();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.Validation.ValidateJE_OH_Consignee();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
			mainAddress.Address2 = new ZString('B', 47);
			declaration.Validation.ValidateJE_OH_Consignee();
			AssertHasWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.Validation.ValidateJE_OH_Consignee();
			AssertHasWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
		}

		public void TestCheckJE_OH_NotifyPartyCompanyNameLength()
		{
			var foreignWarning = "Foreign Company Name Only the first 80 characters will be sent to the customs.";
			var chineseWarning = "Chinese Company Name Only the first 70 characters will be sent to the customs.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			var targetInfo = declaration.JE_OH_NotifyPartyInfo;
			var org = Factory.New<OrgHeader>();
			var mainAddress = org.MainAddress;
			mainAddress.Language = "EN";
			mainAddress.CompanyName = new ZString('A', 80);
			var transAddress = mainAddress.TranslatedAddresses.AddNew();
			transAddress.Language = "ZH-TW";
			transAddress.CompanyName = new ZString('A', 70);
			declaration.JE_OH_NotifyParty = org.PK;
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.Validation.ValidateJE_OH_NotifyParty();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);
			transAddress.CompanyName = new ZString('A', 71);
			declaration.Validation.ValidateJE_OH_NotifyParty();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.Validation.ValidateJE_OH_NotifyParty();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
			mainAddress.CompanyName = new ZString('A', 81);
			declaration.Validation.ValidateJE_OH_NotifyParty();
			AssertHasWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.Validation.ValidateJE_OH_NotifyParty();
			AssertHasWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
		}

		public void TestCheckJE_OH_NotifyPartyAddressLength()
		{
			var foreignWarning = "Foreign Address Only the first 120 characters will be sent to the customs.";
			var chineseWarning = "Chinese Address Only the first 100 characters will be sent to the customs.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			var targetInfo = declaration.JE_OH_NotifyPartyInfo;
			var org = Factory.New<OrgHeader>();
			var mainAddress = org.MainAddress;
			mainAddress.Language = "EN";
			mainAddress.CompanyName = "CompanyName";
			mainAddress.City = "Taipei City";
			mainAddress.Postcode = "239";
			mainAddress.OA_RN_NKCountryCode = "TW";
			mainAddress.Address1 = new ZString('A', 50);
			mainAddress.Address2 = new ZString('B', 46);
			var transAddress = mainAddress.TranslatedAddresses.AddNew();
			transAddress.Language = "ZH-TW";
			transAddress.CompanyName = "CompanyName";
			transAddress.City = "Taipei";
			transAddress.Postcode = "239";
			transAddress.Address1 = new ZString('C', 49);
			transAddress.Address2 = new ZString('D', 42);
			declaration.JE_OH_NotifyParty = org.PK;
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.Validation.ValidateJE_OH_NotifyParty();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);
			transAddress.Address2 = new ZString('D', 43);
			declaration.Validation.ValidateJE_OH_NotifyParty();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.Validation.ValidateJE_OH_NotifyParty();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
			mainAddress.Address2 = new ZString('B', 47);
			declaration.Validation.ValidateJE_OH_NotifyParty();
			AssertHasWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.Validation.ValidateJE_OH_NotifyParty();
			AssertHasWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
		}

		public void TestCheckJE_OA_DeclarantAddressCompanyNameLength()
		{
			var foreignWarning = "Foreign Company Name Only the first 80 characters will be sent to the customs.";
			var chineseWarning = "Chinese Company Name Only the first 70 characters will be sent to the customs.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			var targetInfo = declaration.JE_OA_DeclarantAddressInfo;
			var org = Factory.New<OrgHeader>();
			var mainAddress = org.MainAddress;
			mainAddress.Language = "EN";
			mainAddress.CompanyName = new ZString('A', 80);
			var transAddress = mainAddress.TranslatedAddresses.AddNew();
			transAddress.Language = "ZH-TW";
			transAddress.CompanyName = new ZString('A', 70);
			declaration.JE_OA_DeclarantAddress = mainAddress.PK;
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);
			transAddress.CompanyName = new ZString('A', 71);
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
			mainAddress.CompanyName = new ZString('A', 81);
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertHasWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);
		}

		public void TestCheckJE_OA_DeclarantAddressAddressLength()
		{
			var foreignWarning = "Foreign Address Only the first 120 characters will be sent to the customs.";
			var chineseWarning = "Chinese Address Only the first 100 characters will be sent to the customs.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			var targetInfo = declaration.JE_OA_DeclarantAddressInfo;
			var org = Factory.New<OrgHeader>();
			var mainAddress = org.MainAddress;
			mainAddress.Language = "EN";
			mainAddress.CompanyName = "CompanyName";
			mainAddress.City = "Taipei City";
			mainAddress.Postcode = "239";
			mainAddress.OA_RN_NKCountryCode = "TW";
			mainAddress.Address1 = new ZString('A', 50);
			mainAddress.Address2 = new ZString('B', 46);
			var transAddress = mainAddress.TranslatedAddresses.AddNew();
			transAddress.Language = "ZH-TW";
			transAddress.CompanyName = "CompanyName";
			transAddress.City = "Taipei";
			transAddress.Postcode = "239";
			transAddress.Address1 = new ZString('C', 49);
			transAddress.Address2 = new ZString('D', 42);
			declaration.JE_OA_DeclarantAddress = mainAddress.PK;
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);
			transAddress.Address2 = new ZString('D', 43);
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
			mainAddress.Address2 = new ZString('B', 47);
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertHasWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);
		}

		public void TestCheckJE_OA_DeclarantAddressMandatory()
		{
			Enterprise.Customs.Business.Testing.ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_OA_DeclarantAddressInfo);
		}

		public void TestCheckJE_OA_DeclarantAddressID()
		{
			var messageError = "You have not entered a Declarant ID: A valid TW-VAT or TW-PAS or TW- PID number is required for Declarant.";
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var testMainAddress1 = testOrg1.MainAddress;
			testMainAddress1.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;

			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			var testMainAddress2 = testOrg2.MainAddress;
			testMainAddress2.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testMainAddress2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "11111111", Core.Constants.CountryCodes.Taiwan);

			var testOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			var testMainAddress3 = testOrg3.MainAddress;
			testMainAddress3.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testMainAddress3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "22222222", Core.Constants.CountryCodes.Taiwan);

			var testOrg4 = Factory.NewWithValidTestData<OrgHeader>();
			var testMainAddress4 = testOrg4.MainAddress;
			testMainAddress4.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testMainAddress4.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "33333333", Core.Constants.CountryCodes.Taiwan);

			var declaration = Factory.New<JobDeclaration>();
			var targetInfo = declaration.JE_OA_DeclarantAddressInfo;
			declaration.JE_OA_DeclarantAddress = testMainAddress1.PK;
			AssertHasMessageError("CustomsCodes do not contain VAT or PAS or PID", targetInfo, messageError);

			declaration.JE_OA_DeclarantAddress = testMainAddress2.PK;
			AssertNoMessageError("CustomsCodes contains VAT", targetInfo, messageError);

			declaration.JE_OA_DeclarantAddress = testMainAddress3.PK;
			AssertNoMessageError("CustomsCodes contains PAS", targetInfo, messageError);

			declaration.JE_OA_DeclarantAddress = testMainAddress4.PK;
			AssertNoMessageError("CustomsCodes contains PID", targetInfo, messageError);

			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertNoMessageError("Declarant is empty", targetInfo, messageError);
		}

		public void TestNoWarningOnDuplicatedWhenTransportModeIsSeaAndMasterBillIsNIL()
		{
			var dec1 = JobDeclaration.New(Factory);
			dec1.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec1.JE_TransportMode = TransportTypeList.Codes.Sea;
			dec1.JE_MasterBill = "08111111110";

			var dec2 = BaseJobDeclaration.New(Factory);
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.JE_TransportMode = TransportTypeList.Codes.Air;
			dec2.JE_MasterBill = "08111111121";

			var dec3 = BaseJobDeclaration.New(Factory);
			dec3.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec3.JE_TransportMode = TransportTypeList.Codes.Sea;
			dec3.JE_MasterBill = Constants.NIL;

			var dec4 = BaseJobDeclaration.New(Factory);
			dec4.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec4.JE_TransportMode = TransportTypeList.Codes.Air;
			dec4.JE_MasterBill = Constants.NIL;
			Factory.Save();

			dec1.JE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-4);
			dec2.JE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-3);
			dec3.JE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-2);
			dec4.JE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);

			Factory.Save();

			var dec5 = BaseJobDeclaration.New(Factory);
			dec5.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec5.JE_TransportMode = TransportTypeList.Codes.Sea;
			dec5.JE_MasterBill = Constants.NIL;
			AssertNoWarnings(dec5.JE_MasterBillInfo);

			var dec6 = JobDeclaration.New(Factory);
			dec6.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec6.JE_TransportMode = TransportTypeList.Codes.Sea;
			dec6.JE_MasterBill = "08111111110";
			AssertHasWarning("Has warning for duplicate master", dec6.JE_MasterBillInfo, BillValidator.AlreadyContainsMasterBill(dec1.JE_DeclarationReference, dec1.Company.GC_Name, dec1.Branch.GB_BranchName));

			var dec7 = BaseJobDeclaration.New(Factory);
			dec7.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec7.JE_TransportMode = TransportTypeList.Codes.Air;
			dec7.JE_MasterBill = Constants.NIL;
			AssertHasWarning("Has warning for duplicate master", dec7.JE_MasterBillInfo, BillValidator.AlreadyContainsMasterBill(dec4.JE_DeclarationReference, dec4.Company.GC_Name, dec4.Branch.GB_BranchName));

			var dec8 = BaseJobDeclaration.New(Factory);
			dec8.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec8.JE_TransportMode = TransportTypeList.Codes.Sea;
			dec8.JE_MasterBill = Constants.NIL;
			AssertNoWarnings(dec5.JE_MasterBillInfo);

			var dec9 = BaseJobDeclaration.New(Factory);
			dec9.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec9.JE_TransportMode = TransportTypeList.Codes.Air;
			dec9.JE_MasterBill = "08111111121";
			AssertHasWarning("Has warning for duplicate master", dec9.JE_MasterBillInfo, BillValidator.AlreadyContainsMasterBill(dec2.JE_DeclarationReference, dec2.Company.GC_Name, dec2.Branch.GB_BranchName));
		}

		public void TestCheckTW1_CertificateType_JE_ExportDate()
		{
			AssertCheckTW1_CertificateType_JE_ExportDate(true, CertificateTypeList.Codes.Code15, ControllingMessageTypeList.Codes.NX101, ZDateTime.Empty);
			AssertCheckTW1_CertificateType_JE_ExportDate(false, CertificateTypeList.Codes.Code1, ControllingMessageTypeList.Codes.NX101, ZDateTime.Empty);
			AssertCheckTW1_CertificateType_JE_ExportDate(false, CertificateTypeList.Codes.Code15, ControllingMessageTypeList.Codes.X101, ZDateTime.Empty);
			AssertCheckTW1_CertificateType_JE_ExportDate(false, CertificateTypeList.Codes.Code15, ControllingMessageTypeList.Codes.NX101, ZDateTime.BrettsBirthday);
		}
		public void TestCheckJE_DeclDocType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertCheckJE_DeclDocType(declaration, new ExportDeclDocTypeList().GetAllCodes());
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertCheckJE_DeclDocType(declaration, new ImportDeclDocTypeList().GetAllCodes());
		}

		void AssertCheckJE_DeclDocType(JobDeclaration declaration, string[] codes)
		{
			declaration.JE_DeclDocType = "";
			AssertNoMessageErrors(declaration.JE_DeclDocTypeInfo);
			declaration.JE_DeclDocType = "8";
			AssertHasMessageError(declaration.JE_DeclDocTypeInfo, ListValidation.InvalidCodeMessageError);
			foreach (var code in codes)
			{
				declaration.JE_DeclDocType = code;
				AssertNoMessageError(declaration.JE_DeclDocTypeInfo, ListValidation.InvalidCodeMessageError);
				AssertNoMessageErrors(declaration.JE_DeclDocTypeInfo);
			}
		}

		public void TestCheckJE_SLD()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			testDeclaration.JE_SLD = "XXX";
			AssertHasMessageError(testDeclaration.JE_SLDInfo, ValidationConstants.Declaration.LengthForSONoOrManifest);
			testDeclaration.JE_SLD = "XXXX";
			AssertEquals(false, testDeclaration.JE_SLDInfo.HasMessageErrors());
		}

		public void TestCheckJE_OtherBankAccount()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var bankAccountOnlyAllowsAlphanumericCharacters = ValidationConstants.Declaration.BankAccountOnlyAllowsAlphanumericCharacters;
			testDeclaration.JE_OtherBankAccount = "123-456";
			AssertHasMessageError(testDeclaration.JE_OtherBankAccountInfo, bankAccountOnlyAllowsAlphanumericCharacters);
			testDeclaration.JE_OtherBankAccount = "123456ABCDE";
			AssertNoMessageError(testDeclaration.JE_OtherBankAccountInfo, bankAccountOnlyAllowsAlphanumericCharacters);
		}

		public void TestCheckJE_Z99FinalDestination()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_RL_NKFinalDestination = "CNZ99";
			testDeclaration.JE_Z99FinalDestination = ZString.Empty;
			AssertHasWarningContaining(testDeclaration.JE_Z99FinalDestinationInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_Z99FinalDestination = "XXXX";
			AssertNoWarningContaining(testDeclaration.JE_Z99FinalDestinationInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_RL_NKFinalDestination = "CNSHA";
			testDeclaration.JE_Z99FinalDestination = ZString.Empty;
			AssertNoWarningContaining(testDeclaration.JE_Z99FinalDestinationInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_Z99PortOfOrigin()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_RL_NKOrigin = "CNZ99";
			testDeclaration.JE_Z99PortOfOrigin = ZString.Empty;
			AssertHasWarningContaining(testDeclaration.JE_Z99PortOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_Z99PortOfOrigin = "XXXX";
			AssertNoWarningContaining(testDeclaration.JE_Z99PortOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_RL_NKOrigin = "CNSHA";
			testDeclaration.JE_Z99PortOfOrigin = ZString.Empty;
			AssertNoWarningContaining(testDeclaration.JE_Z99PortOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestLoadBeforeValationAll()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supplierAddress = declaration.DocAddresses.AddNew();
			supplierAddress.E2_AddressType = AutoDocAddressTypes.Codes.SupplierDocumentaryAddress;
			supplierAddress.E2_AddressOverride = true;
			var supplierLocalAddress = declaration.DocAddresses.AddNew();
			supplierLocalAddress.E2_AddressType = AutoDocAddressTypes.Codes.SupplierTranslatedDocumentaryAddress;
			var supplierPickupDeliveryAddress = declaration.DocAddresses.AddNew();
			supplierPickupDeliveryAddress.E2_AddressType = AutoDocAddressTypes.Codes.SupplierPickupDeliveryAddress;

			var importerAddress = declaration.DocAddresses.AddNew();
			importerAddress.E2_AddressType = AutoDocAddressTypes.Codes.ImporterDocumentaryAddress;
			importerAddress.E2_AddressOverride = true;
			var importerLocalAddress = declaration.DocAddresses.AddNew();
			importerLocalAddress.E2_AddressType = AutoDocAddressTypes.Codes.ImporterTranslatedDocumentaryAddress;
			var importerPickupDeliveryAddress = declaration.DocAddresses.AddNew();
			importerPickupDeliveryAddress.E2_AddressType = AutoDocAddressTypes.Codes.ImporterPickupDeliveryAddress;

			CombineAssertions("Pre req", () =>
			{
				AssertNull("supplierAddress", supplierAddress.OverrideRequirement);
				AssertNull("supplierLocalAddress", supplierLocalAddress.OverrideRequirement);
				AssertNull("supplierLocalAddress", supplierPickupDeliveryAddress.OverrideRequirement);
				AssertNull("importerAddress", importerAddress.OverrideRequirement);
				AssertNull("importerLocalAddress", importerLocalAddress.OverrideRequirement);
				AssertNull("importerPickupDeliveryAddress", importerPickupDeliveryAddress.OverrideRequirement);
			});

			declaration.RunPreSaveValidation();

			CombineAssertions(() =>
			{
				AssertType<SupplierAddressRequirement>("Requirement of SupplierDocumentaryAddress", supplierAddress.OverrideRequirement);
				AssertEquals("SupplierDocumentaryAddress", supplierAddress, declaration.SupplierDocumentaryAddress);

				AssertType<SupplierLocalAddressRequirement>("Requirement of SupplierDocumentaryAddress.LocalAddress", supplierLocalAddress.OverrideRequirement);
				AssertEquals("SupplierDocumentaryAddress.LocalAddress", supplierLocalAddress, declaration.SupplierDocumentaryAddress.LocalAddress);

				AssertType<SupplierPicDlvAddressRequirement>("Requirement of SupplierPickupAddress", supplierPickupDeliveryAddress.OverrideRequirement);
				AssertEquals("SupplierPickupAddress", supplierPickupDeliveryAddress, declaration.SupplierPickupAddress);

				AssertType<ImporterAddressRequirement>("Requirement of ImporterDocumentaryAddress", importerAddress.OverrideRequirement);
				AssertEquals("ImporterDocumentaryAddress", importerAddress, declaration.ImporterDocumentaryAddress);

				AssertType<ImporterLocalAddressRequirement>("Requirement of ImporterDocumentaryAddress.LocalAddress", importerLocalAddress.OverrideRequirement);
				AssertEquals("ImporterDocumentaryAddress.LocalAddress", importerLocalAddress, declaration.ImporterDocumentaryAddress.LocalAddress);

				AssertType<ImporterPicDlvAddressRequirement>("Requirement of ImporterDeliveryAddress", importerPickupDeliveryAddress.OverrideRequirement);
				AssertEquals("ImporterDeliveryAddress", importerPickupDeliveryAddress, declaration.ImporterDeliveryAddress);
			});
		}

		void AssertCheckTW1_CertificateType_JE_ExportDate(bool expectShowError, ZString certificateType, ZString messageType, ZDateTime exportDate)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ExportDate = exportDate;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = messageType;
			messageHeader.TW1_CertificateType = certificateType;
			messageHeader.Validation.ValidateTW1_CertificateType();
			AssertEquals("no error when not link to CMHeader", false, declaration.JE_ExportDateInfo.HasMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Date of Export")));

			var invoiceLine = (JobComInvoiceLine)invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			messageHeader.TW1_ControllingAgency = "XX";
			var link = invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.ControllingAgency == "XX");
			link.IsLinkedCMHeader = true;
			messageHeader.TW1_CertificateType = certificateType;
			messageHeader.Validation.ValidateTW1_CertificateType();
			AssertEquals("Date of Export", expectShowError, declaration.JE_ExportDateInfo.HasMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Date of Export")));
		}

		ZZRefCusCodeListCombined CreateZZRefCusCodeListCombinedForTestEntryNumberWhenChanged(ZString code)
		{
			var customsOffice = Factory.New<ZZRefCusCodeListCombined>();
			customsOffice.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Taiwan;
			customsOffice.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			customsOffice.ZZD_Code = code;
			customsOffice.ZZD_StartDate = ZDateTime.Today;
			customsOffice.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			return customsOffice;
		}
	}
}
