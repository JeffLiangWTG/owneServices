using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusTWControllingMessageHeader))]
	sealed class CusTWControllingMessageHeaderTest : ZArchitecture.Business.Testing.EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var messageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			NUnit.Framework.Assert.That(messageHeader.Declaration, NUnit.Framework.Is.SameAs(declaration));
		}

		[ExpectNoExceptions]
		public void TestEntryNumberForSendingObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			entry.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_CustomsOffice = "BB";
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_BoxNumber = "123";
			entry.EntryNumber = "AA  0912300002";

			var messageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			AssertEquals("No entry number", MessageConstants.EntryNumberPlaceHolder, messageHeader.EntryNumberForSendingObject);

			entry.CH_Status = "AWO";
			AssertEquals("AA  0912300002", messageHeader.EntryNumberForSendingObject);
		}

		[ExpectNoExceptions]
		public void TestIsImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			NUnit.Framework.Assert.That(header.IsImport, NUnit.Framework.Is.EqualTo(false), "IsImport is false");

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			NUnit.Framework.Assert.That(header.IsImport, NUnit.Framework.Is.EqualTo(true), "IsImport is true");
		}

		[ExpectNoExceptions]
		public void TestIsExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			NUnit.Framework.Assert.That(header.IsExport, NUnit.Framework.Is.EqualTo(true), "IsExport is true");

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			NUnit.Framework.Assert.That(header.IsExport, NUnit.Framework.Is.EqualTo(false), "IsExport is false");
		}

		[ExpectNoExceptions]
		public void TestDefaultPortsIfNeeded()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_RL_NKOrigin = "USLAX";
			declaration.JE_RL_NKFinalDestination = "TWKEL";
			var entryInstruction = declaration.CusEntryInstruction;
			var messageHeader1 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader1.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			messageHeader1.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(messageHeader1.TW1_RL_NKPortOfLoading, NUnit.Framework.Is.EqualTo("USLAX").Using(CustomComparers.TypeComparison), "TW1_RL_NKPortOfLoading");
				NUnit.Framework.Assert.That(messageHeader1.TW1_RL_NKPortOfUnloading, NUnit.Framework.Is.EqualTo("TWKEL").Using(CustomComparers.TypeComparison), "TW1_RL_NKPortOfUnloading");
			});

			var messageHeader2 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader2.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX201_01;
			messageHeader2.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(messageHeader2.TW1_RL_NKPortOfLoading.ToString(), NUnit.Framework.Is.Empty, "TW1_RL_NKPortOfLoading should be empty");
				NUnit.Framework.Assert.That(messageHeader2.TW1_RL_NKPortOfUnloading.ToString(), NUnit.Framework.Is.Empty, "TW1_RL_NKPortOfUnloading should be empty");
			});

			var messageHeader3 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader3.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			messageHeader3.TW1_CertificateType = CertificateTypeList.Codes.Code4;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(messageHeader3.TW1_RL_NKPortOfLoading.ToString(), NUnit.Framework.Is.Empty, "TW1_RL_NKPortOfLoading should be empty");
				NUnit.Framework.Assert.That(messageHeader3.TW1_RL_NKPortOfUnloading.ToString(), NUnit.Framework.Is.Empty, "TW1_RL_NKPortOfUnloading should be empty");
			});
		}

		[ExpectNoExceptions]
		public void TestDefaultPortOfLoadingNameIfNeeded()
		{
			AssertDefaultPortNameIfNeeded(x => x.JE_RL_NKOriginInfo, x => x.JE_Z99PortOfOriginInfo, x => x.TW1_PortOfLoadingName);
		}

		[ExpectNoExceptions]
		public void TestDefaultPortOfUnloadingNameIfNeeded()
		{
			AssertDefaultPortNameIfNeeded(x => x.JE_RL_NKFinalDestinationInfo, x => x.JE_Z99FinalDestinationInfo, x => x.TW1_PortOfUnloadingName);
		}

		void AssertDefaultPortNameIfNeeded(Func<JobDeclaration, ZPropertyInfo> portCodeSelector, Func<JobDeclaration, ZPropertyInfo> portNameSelector, Func<CusTWControllingMessageHeader, string> propertySelector)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			portCodeSelector.Invoke(declaration).Value = new ZString("TWZ99");
			portNameSelector.Invoke(declaration).Value = ZString.Replicate('Z', 71);
			var entryInstruction = declaration.CusEntryInstruction;
			var messageHeader1 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader1.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			messageHeader1.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			NUnit.Framework.Assert.That(propertySelector.Invoke(messageHeader1), NUnit.Framework.Is.EqualTo(ZString.Replicate('Z', 70)).Using(CustomComparers.TypeComparison));

			var messageHeader2 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader2.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			messageHeader2.TW1_CertificateType = CertificateTypeList.Codes.Code4;
			NUnit.Framework.Assert.That(propertySelector.Invoke(messageHeader2), NUnit.Framework.Is.Empty);

			var messageHeader3 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader3.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX201_01;
			messageHeader3.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			NUnit.Framework.Assert.That(propertySelector.Invoke(messageHeader3), NUnit.Framework.Is.Empty);

			portCodeSelector.Invoke(declaration).Value = new ZString("TWKEL");
			var messageHeader4 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader4.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			messageHeader4.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			NUnit.Framework.Assert.That(propertySelector.Invoke(messageHeader4), NUnit.Framework.Is.EqualTo("Keelung (Chilung)").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIsPortRequiredCertificateTypes()
		{
			var isRequiredCodes = new string[] { CertificateTypeList.Codes.Code1, CertificateTypeList.Codes.Code7, CertificateTypeList.Codes.Code8, CertificateTypeList.Codes.Code10, CertificateTypeList.Codes.Code15, CertificateTypeList.Codes.Code16, CertificateTypeList.Codes.Code17 };
			foreach (var code in new CertificateTypeList().GetAllCodes())
			{
				controllingMessageHeader.TW1_CertificateType = code;
				NUnit.Framework.Assert.That(controllingMessageHeader.IsPortRequiredCertificateTypes, NUnit.Framework.Is.EqualTo(isRequiredCodes.Contains(code)).Using(CustomComparers.TypeComparison), code);
			}
		}

		[ExpectNoExceptions]
		public void TestIsPortNameRequired()
		{
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var allCodes = new CertificateTypeList().GetAllCodes();
			var isRequiredCodes = new string[] { CertificateTypeList.Codes.Code1, CertificateTypeList.Codes.Code7, CertificateTypeList.Codes.Code8, CertificateTypeList.Codes.Code10, CertificateTypeList.Codes.Code15, CertificateTypeList.Codes.Code16, CertificateTypeList.Codes.Code17 };
			foreach (var code in allCodes)
			{
				controllingMessageHeader.TW1_CertificateType = code;
				NUnit.Framework.Assert.That(controllingMessageHeader.IsPortNameRequired, NUnit.Framework.Is.EqualTo(isRequiredCodes.Contains(code)).Using(CustomComparers.TypeComparison), code);
			}

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code5;
			NUnit.Framework.Assert.That(controllingMessageHeader.IsPortNameRequired, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "NX601");
		}

		[ExpectNoExceptions]
		public void TestTW1_PortOfLoadingName()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType( Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECFALoadingPort, "ECFA Loading Port", Core.Constants.CountryCodes.Taiwan);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECFALoadingPort, "TWTPE", "台北", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			controllingMessageHeader.TW1_RL_NKPortOfLoading = "TWTPE";
			controllingMessageHeader.TW1_PortOfLoadingName = "override val";
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_PortOfLoadingName, NUnit.Framework.Is.EqualTo("override val").Using(CustomComparers.TypeComparison));

			controllingMessageHeader.TW1_PortOfLoadingName = ZString.Empty;
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_PortOfLoadingName, NUnit.Framework.Is.EqualTo("Taipei").Using(CustomComparers.TypeComparison));

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_PortOfLoadingName, NUnit.Framework.Is.EqualTo("台北").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTW1_PortOfUnloadingName()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECFAUnloadingPort, "ECFA UnLoading Port", Core.Constants.CountryCodes.Taiwan);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECFAUnloadingPort, "TWTPE", "台北", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			controllingMessageHeader.TW1_RL_NKPortOfUnloading = "TWTPE";
			controllingMessageHeader.TW1_PortOfUnloadingName = "override val";
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_PortOfUnloadingName, NUnit.Framework.Is.EqualTo("override val").Using(CustomComparers.TypeComparison));

			controllingMessageHeader.TW1_PortOfUnloadingName = ZString.Empty;
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_PortOfUnloadingName, NUnit.Framework.Is.EqualTo("Taipei").Using(CustomComparers.TypeComparison));

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_PortOfUnloadingName, NUnit.Framework.Is.EqualTo("台北").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestReOrderTW1SequenceNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var messageHeader1 = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var messageHeader2 = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var messageHeader3 = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();

			CombineAssertions("PreCondition", () =>
			{
				NUnit.Framework.Assert.That(messageHeader1.TW1_Sequence, NUnit.Framework.Is.EqualTo((ZShort)1));
				NUnit.Framework.Assert.That(messageHeader2.TW1_Sequence, NUnit.Framework.Is.EqualTo((ZShort)2));
				NUnit.Framework.Assert.That(messageHeader3.TW1_Sequence, NUnit.Framework.Is.EqualTo((ZShort)3));
			});

			messageHeader3.TW1_Sequence = 2;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(messageHeader1.TW1_Sequence, NUnit.Framework.Is.EqualTo((ZShort)1));
				NUnit.Framework.Assert.That(messageHeader2.TW1_Sequence, NUnit.Framework.Is.EqualTo((ZShort)3));
				NUnit.Framework.Assert.That(messageHeader3.TW1_Sequence, NUnit.Framework.Is.EqualTo((ZShort)2));
			});

			messageHeader3.Delete();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(messageHeader1.TW1_Sequence, NUnit.Framework.Is.EqualTo((ZShort)1));
				NUnit.Framework.Assert.That(messageHeader2.TW1_Sequence, NUnit.Framework.Is.EqualTo((ZShort)2));
			});
		}

		[ExpectNoExceptions]
		public void TestAssignHeaderToInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line3 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
			messageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code9;
			messageHeader.AssignHeaderToInvoiceLines();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(line1.IsForCMHeaderMessageTypeNX301, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "line1 - NX301");
				NUnit.Framework.Assert.That(line2.IsForCMHeaderMessageTypeNX301, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "line2 - NX301");
				NUnit.Framework.Assert.That(line3.IsForCMHeaderMessageTypeNX301, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "line3 - NX301");
				NUnit.Framework.Assert.That(line1.IsForCMHeaderMessageTypeNX601, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "line1 - NX601");
				NUnit.Framework.Assert.That(line2.IsForCMHeaderMessageTypeNX601, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "line2 - NX601");
				NUnit.Framework.Assert.That(line3.IsForCMHeaderMessageTypeNX601, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "line3 - NX601");
			});
		}

		[ExpectNoExceptions]
		public void TestAssigningHeaderToInvoiceLinesWhenLineHasLinkedToOtherSameTypeControllingMessageHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line3 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var messageHeader1 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader1.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			messageHeader1.TW1_CertificateType = CertificateTypeList.Codes.Code9;
			var messageHeader2 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader2.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			messageHeader2.TW1_CertificateType = CertificateTypeList.Codes.Code5;
			var messageHeader3 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader3.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			messageHeader3.TW1_CertificateType = CertificateTypeList.Codes.Code6;
			var messageHeader4 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader4.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			messageHeader4.TW1_CertificateType = CertificateTypeList.Codes.Code6;
			var messageHeader5 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader5.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			messageHeader5.TW1_CertificateType = CertificateTypeList.Codes.Code8;

			var errorMessage = "Only assign Licensing Header to the Invoice Lines which does not link to NX101 message type.";
			line1.AssignCMHeaderToInvoices(messageHeader1);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(messageHeader1.AssignHeaderToInvoiceLines().ToString(), NUnit.Framework.Is.Null.Or.Empty, "messageHeader1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(messageHeader2.AssignHeaderToInvoiceLines().ToString(), NUnit.Framework.Is.Null.Or.Empty, "messageHeader2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(messageHeader3.AssignHeaderToInvoiceLines(), NUnit.Framework.Is.EqualTo(errorMessage).Using(CustomComparers.TypeComparison), "messageHeader3");
				NUnit.Framework.Assert.That(messageHeader4.AssignHeaderToInvoiceLines().ToString(), NUnit.Framework.Is.Null.Or.Empty, "messageHeader4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(messageHeader5.AssignHeaderToInvoiceLines(), NUnit.Framework.Is.EqualTo(errorMessage).Using(CustomComparers.TypeComparison), "messageHeader5");
			});

			line3.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.ControllingMessageHeaderPK == messageHeader1.PK).IsLinkedCMHeader = false;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(messageHeader3.AssignHeaderToInvoiceLines(), NUnit.Framework.Is.EqualTo(errorMessage).Using(CustomComparers.TypeComparison), "messageHeader3");
				NUnit.Framework.Assert.That(line3.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.ControllingMessageHeaderPK == messageHeader3.PK).IsLinkedCMHeader, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestVATNumber()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = "DUMMY COMP";
			org.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Taiwan;
			org.MainAddress.OA_Address1 = "Address 1";
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "96944490", Core.Constants.CountryCodes.Taiwan);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = org.MainAddress.PK;
			var messageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			NUnit.Framework.Assert.That(messageHeader.VATNumber, NUnit.Framework.Is.EqualTo("96944490").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestRefreshBindingLocalProcessorAddressOnMessageTypeChanged()
		{
			var messageHeader = Factory.New<CusTWControllingMessageHeader>();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var childOnElementFired = false;
			((IBusiness)messageHeader.LocalProcessorAddress).ListChanged += (object sender, ListChangedEventArgs e) => childOnElementFired = true;
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			NUnit.Framework.Assert.That(childOnElementFired, NUnit.Framework.Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestIsEmpty()
		{
			var messageHeader = Factory.New<CusTWControllingMessageHeader>();
			NUnit.Framework.Assert.That(messageHeader.IsEmpty, NUnit.Framework.Is.True);
			AssertMessageHeaderPropertyInfoIsEmpty(messageHeader, messageHeader.TW1_AppointmentDateInfo, ZDate.Today, ZDate.Empty);
			AssertMessageHeaderPropertyInfoIsEmpty(messageHeader, messageHeader.TW1_AppointmentPeriodInfo, new ZString("A"), ZString.Empty);
			AssertMessageHeaderPropertyInfoIsEmpty(messageHeader, messageHeader.TW1_BusinessTypeInfo, new ZString("A"), ZString.Empty);
			AssertMessageHeaderPropertyInfoIsEmpty(messageHeader, messageHeader.TW1_ControllingAgencyInfo, new ZString("A"), ZString.Empty);
			AssertMessageHeaderPropertyInfoIsEmpty(messageHeader, messageHeader.TW1_FunctionalReferenceIdInfo, new ZString("A"), ZString.Empty);
			AssertMessageHeaderPropertyInfoIsEmpty(messageHeader, messageHeader.TW1_PaymentMethodInfo, new ZString("A"), ZString.Empty);
			AssertMessageHeaderPropertyInfoIsEmpty(messageHeader, messageHeader.PermitNumberInfo, new ZString("A"), ZString.Empty);
			AssertMessageHeaderPropertyInfoIsEmpty(messageHeader, messageHeader.TW1_ProcessingUnitInfo, new ZString("A"), ZString.Empty);
			AssertMessageHeaderPropertyInfoIsEmpty(messageHeader, messageHeader.TW1_RequestDescriptionInfo, new ZString("A"), ZString.Empty);
		}

		[ExpectNoExceptions]
		void AssertMessageHeaderPropertyInfoIsEmpty(CusTWControllingMessageHeader messageHeader, ZPropertyInfo propertyinfo, IZType value, IZType emptyValue)
		{
			propertyinfo.Value = value;
			NUnit.Framework.Assert.That(!messageHeader.IsEmpty, NUnit.Framework.Is.True);
			propertyinfo.Value = emptyValue;
			NUnit.Framework.Assert.That(messageHeader.IsEmpty, NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestTW1_ControllingAgencyWhenMessageTypeNX603()
		{
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_BusinessType, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			controllingMessageHeader.TW1_ControllingMessageType = "NX603";
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_BusinessType, NUnit.Framework.Is.EqualTo("01").Using(CustomComparers.TypeComparison));
			controllingMessageHeader.TW1_ControllingAgency = "IF";
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_BusinessType, NUnit.Framework.Is.EqualTo("01").Using(CustomComparers.TypeComparison));
			controllingMessageHeader.TW1_ControllingAgency = "DH";
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_BusinessType, NUnit.Framework.Is.EqualTo("01").Using(CustomComparers.TypeComparison));
			controllingMessageHeader.TW1_ControllingAgency = "CD";
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_BusinessType, NUnit.Framework.Is.EqualTo("01").Using(CustomComparers.TypeComparison));
			controllingMessageHeader.TW1_ControllingAgency = "";
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_BusinessType, NUnit.Framework.Is.EqualTo("01").Using(CustomComparers.TypeComparison));
		}

		#region readonly tests

		[ExpectNoExceptions]
		public void TestTW1_CertificateType_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_CertificateTypeInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_BusinessType_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_BusinessTypeInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_ProcessingUnit_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_ProcessingUnitInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_ManufacturerPrintingCode_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_ManufacturerPrintingCodeInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_PrintingCode_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_PrintingCodeInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_OriginalQuantity_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_OriginalQuantityInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_CopyQuantity_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_CopyQuantityInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_BeforeClearanceApplicationReason_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_BeforeClearanceApplicationReasonInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_IsTriangularTrade_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_IsTriangularTradeInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_IsEstimatedLoadingDate_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			AssertReadOnly(header, header.TW1_IsEstimatedLoadingDateInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_PaymentMethod_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_PaymentMethodInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_Purpose_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_PurposeInfo);
		}

		[ExpectNoExceptions]
		public void TestBulkPaymentID_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.BulkPaymentIDInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_ElectronicReceipt_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_ElectronicReceiptInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_AppointmentDate_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_AppointmentDateInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_AppointmentPeriod_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_AppointmentPeriodInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_ProofOfPaper_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_ProofOfPaperInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_InspectionRegistrationNumber_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_InspectionRegistrationNumberInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_PreWineInspectionStatus_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_PreWineInspectionStatusInfo);
		}

		[ExpectNoExceptions]
		public void TestPermitNumber_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.PermitNumberInfo);
		}

		[ExpectNoExceptions]
		public void TestPermitNoExpirationDate_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.PermitNoExpirationDateInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_PrePermitNumber_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_PrePermitNumberInfo);
		}

		[ExpectNoExceptions]
		public void TestBulkApplicationID_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.BulkApplicationIDInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_PortOfBulkCommodity_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_PortOfBulkCommodityInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_SampleReturnAddress_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_SampleReturnAddressInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_ApplyForSampleReturn_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_ApplyForSampleReturnInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_RequestDescription_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_RequestDescriptionInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_ReturnPreviousCOO_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_ReturnPreviousCOOInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_IsSpecialApplication_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_IsSpecialApplicationInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_SpecialApplicationId_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_SpecialApplicationIdInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_EUSteelProductPhase_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_EUSteelProductPhaseInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_EUSteelProductNo_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_EUSteelProductNoInfo);
		}

		[ExpectNoExceptions]
		public void TestProcessingNumber_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.ProcessingNumberInfo);
		}

		[ExpectNoExceptions]
		public void TestCustomsMessageIdentifier_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.CustomsMessageIdentifierInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_Observations_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_ObservationsInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_Remarks_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_RemarksInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_RL_NKPortOfLoading_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_RL_NKPortOfLoadingInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_PortOfLoadingName_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_PortOfLoadingNameInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_RL_NKPortOfUnloading_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_RL_NKPortOfUnloadingInfo);
		}

		[ExpectNoExceptions]
		public void TestTW1_PortOfUnloadingName_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			AssertReadOnly(header, header.TW1_PortOfUnloadingNameInfo);
		}

		[ExpectNoExceptions]
		void AssertReadOnly(CusTWControllingMessageHeader header, ZPropertyInfo propertyInfo)
		{
			var readonlyTypes = propertiesReadOnly[propertyInfo.Name];
			foreach (var controllingMessageType in new ControllingMessageTypeList().GetAllCodes())
			{
				header.TW1_ControllingMessageType = controllingMessageType;
				NUnit.Framework.Assert.That(propertyInfo.ReadOnly, NUnit.Framework.Is.EqualTo(readonlyTypes.Contains(controllingMessageType)), $"Readonly for Controlling Message Type {controllingMessageType}");
			}
		}

		static readonly Dictionary<ZString, ImmutableHashSet<string>> propertiesReadOnly = new Dictionary<ZString, ImmutableHashSet<string>>()
		{
			{
				CusTWControllingMessageHeader.Schema.TW1_CertificateType, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_BusinessType, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX101,
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX601
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_ProcessingUnit, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX301_DN
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_ManufacturerPrintingCode, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_PrintingCode, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_OriginalQuantity, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_CopyQuantity, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_BeforeClearanceApplicationReason, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_IsTriangularTrade, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_IsEstimatedLoadingDate, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_PaymentMethod, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX101,
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_Purpose, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX101,
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.BulkPaymentID, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX101,
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_ElectronicReceipt, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX101,
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_AppointmentDate, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX101,
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_AppointmentPeriod, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX101,
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_ProofOfPaper, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX101,
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_DN
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_InspectionRegistrationNumber, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX101,
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_PreWineInspectionStatus, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX101,
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.PermitNumber, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX101,
					ControllingMessageTypeList.Codes.X101
					)
			},
			{
				CusTWControllingMessageHeader.Schema.PermitNoExpirationDate, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX101,
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_PrePermitNumber, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.BulkApplicationID, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX101,
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_PortOfBulkCommodity, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX101,
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_SampleReturnAddress, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX101,
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_ApplyForSampleReturn, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX101,
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_SamplingReductionReason, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX101,
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_RequestDescription, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX101,
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_ReturnPreviousCOO, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_IsSpecialApplication, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_SpecialApplicationId, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_EUSteelProductPhase, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_EUSteelProductNo, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.ProcessingNumber, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX101,
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.CustomsMessageIdentifier, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX101,
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_Observations, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				CusTWControllingMessageHeader.Schema.TW1_Remarks, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				nameof(CusTWControllingMessageHeader.EthanolPermitNumbers), ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX101,
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				nameof(CusTWControllingMessageHeader.LocalProcessorAddress), ImmutableHashSet.Create(

					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				nameof(CusTWControllingMessageHeader.ProductLabelRanges), ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX101,
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				nameof(CusTWControllingMessageHeader.TW1_RL_NKPortOfLoading), ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				nameof(CusTWControllingMessageHeader.TW1_PortOfLoadingName), ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				nameof(CusTWControllingMessageHeader.TW1_RL_NKPortOfUnloading), ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				nameof(CusTWControllingMessageHeader.TW1_PortOfUnloadingName), ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.X101,
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX201_07,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
		};

		public void TestTW1_RequestDescription()
		{
			var messageHeader = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			CombineAssertions(() =>
			{
				AssertExceptionThrown<MaxLengthExceededException>(() => messageHeader.TW1_RequestDescription = "1".PadLeft(257, '1'));
				AssertNoExceptionThrown(() => messageHeader.TW1_RequestDescription = "1".PadLeft(256, '1'));
				ExceptionReporterTestListener.Instance.Clear();
				messageHeader.TW1_RequestDescription = "哈哈";
				NUnit.Framework.Assert.That(!messageHeader.TW1_RequestDescriptionInfo.GetErrors().Contains("only accepts Western European languages characters"), NUnit.Framework.Is.True);
			});
		}

		#endregion

		[ExpectNoExceptions]
		public void TestEntryInstruction()
		{
			var entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			NUnit.Framework.Assert.That(messageHeader.EntryInstruction, NUnit.Framework.Is.EqualTo(entryInstruction));
			messageHeader = Factory.New<CusTWControllingMessageHeader>();
			NUnit.Framework.Assert.That(messageHeader.EntryInstruction, NUnit.Framework.Is.EqualTo(default(CusEntryInstruction)));
		}

		[ExpectNoExceptions]
		public void TestProcessingUnit()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit, "TWReceivingUnit");
			var facility = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit, "XXXX0123", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(facility.PK, RefCusCodeListAttributeTypes.Codes.ControlAgency, "AT");

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanCertificateOfOriginIssuingUnit, "CertificateOfOriginIssuingUnit");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanCertificateOfOriginIssuingUnit, "OOOO9876", "OOOOOO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var facility2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit, "FTKHH", "高雄", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(facility2.PK, RefCusCodeListAttributeTypes.Codes.ControlAgency, ControllingAgencyList.Codes.FT);
			Factory.Save();

			var entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ProcessingUnit = "XXXX0123";
			var processingUnit1 = (ICodeDescription)messageHeader.ProcessingUnit;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(processingUnit1.Code, NUnit.Framework.Is.EqualTo("XXXX0123"));
				NUnit.Framework.Assert.That(processingUnit1.Description, NUnit.Framework.Is.EqualTo("XXXXXX"));
				NUnit.Framework.Assert.That(messageHeader.ProcessingUnit.Attributes.HasAttribute(RefCusCodeListAttributeTypes.Codes.ControlAgency, "AT"), NUnit.Framework.Is.EqualTo(true));
			});

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			messageHeader.TW1_ProcessingUnit = "OOOO9876";
			var processingUnit2 = (ICodeDescription)TWRefCusCodeListLoader.GetProcessingUnit(Factory, true, "OOOO9876", ZDateTime.Today);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(processingUnit2.Code, NUnit.Framework.Is.EqualTo("OOOO9876"));
				NUnit.Framework.Assert.That(processingUnit2.Description, NUnit.Framework.Is.EqualTo("OOOOOO"));
			});

			CombineAssertions("ProcessingUnit List", () =>
			{
				controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX201_01;
				var processingUnitList = controllingMessageHeader.Lookups.ProcessingUnitList;
				NUnit.Framework.Assert.That(processingUnitList.CodesAsString, NUnit.Framework.Is.EqualTo("FTKHH"), "NX201_01");

				controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX201_07;
				processingUnitList = controllingMessageHeader.Lookups.ProcessingUnitList;
				NUnit.Framework.Assert.That(processingUnitList.CodesAsString, NUnit.Framework.Is.EqualTo("FTKHH"), "NX201_07");
			});
		}

		[ExpectNoExceptions]
		public void TestTW1_SampleReturnAddress()
		{
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_SampleReturnAddressInfo.MaxLength, NUnit.Framework.Is.EqualTo(100));
		}

		[ExpectNoExceptions]
		public void TestTW1_SamplingReductionReason()
		{
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_SamplingReductionReasonInfo.MaxLength, NUnit.Framework.Is.EqualTo(100));
		}

		[ExpectNoExceptions]
		public void TestTW1_InspectionRegistrationNumber()
		{
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_InspectionRegistrationNumberInfo.MaxLength, NUnit.Framework.Is.EqualTo(6));
		}

		[ExpectNoExceptions]
		public void TestTW1_PreWineInspectionStatus()
		{
			var info = controllingMessageHeader.TW1_PreWineInspectionStatusInfo;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(info.MaxLength, NUnit.Framework.Is.EqualTo(1));
				NUnit.Framework.Assert.That(controllingMessageHeader.GetType(), CustomConstraints.HasCustomAttribute<ListAttribute>(nameof(controllingMessageHeader.TW1_PreWineInspectionStatus), false, attrib => attrib.ListDataSourceMember == "Lookups.PreWineInspectionStatusList"));
			});
		}

		[ExpectNoExceptions]
		public void TestTW1_Purpose()
		{
			var info = controllingMessageHeader.TW1_PurposeInfo;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(info.MaxLength, NUnit.Framework.Is.EqualTo(2));
				NUnit.Framework.Assert.That(controllingMessageHeader.GetType(), CustomConstraints.HasCustomAttribute<ListAttribute>(nameof(controllingMessageHeader.TW1_Purpose), false, attrib => attrib.ListDataSourceMember == "Lookups.PurposeList"));
				controllingMessageHeader.TW1_ControllingAgency = "DN";
				NUnit.Framework.Assert.That(info.Value, NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestTW1_IsEstimatedLoadingDate()
		{
			var info = controllingMessageHeader.TW1_IsEstimatedLoadingDateInfo;
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			NUnit.Framework.Assert.That(info.ReadOnly, NUnit.Framework.Is.EqualTo(false), "ReadOnly");
			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code14;
			NUnit.Framework.Assert.That(info.ReadOnly, NUnit.Framework.Is.EqualTo(true), "ReadOnly");
		}

		[ExpectNoExceptions]
		public void TestTW1_OriginalQuantity()
		{
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_OriginalQuantityInfo.MaxLength, NUnit.Framework.Is.EqualTo(2), "MaxLength");
		}

		[ExpectNoExceptions]
		public void TestTW1_CopyQuantity()
		{
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_CopyQuantityInfo.MaxLength, NUnit.Framework.Is.EqualTo(2), "MaxLength");
		}

		[ExpectNoExceptions]
		public void TestLicensingStatusCaption()
		{
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(controllingMessageHeader.TW1_PrePermitNumberInfo, "Previous Permit Number", "Pre. Permit No.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_IsEstimatedLoadingDateInfo, "Estimated Date of Loading", "Est. Date Of Lading", "ETL", "Indicates whether it is the estimated date of Loading. Only enable when Certificate Type is '15'.");
			BusinessObjectCaptionTestHelper.CombineAssertCaptionsWithFullDescription(controllingMessageHeader.TW1_IsSpecialApplicationInfo, "Special Application", "Special App", "Special App", "Indicates that this certificate of origin message is a Special Application.");
			BusinessObjectCaptionTestHelper.CombineAssertCaptionsWithFullDescription(controllingMessageHeader.TW1_SpecialApplicationIdInfo, "Special Application ID", "Special Application ID", "Special App ID", "Indicates the Special Application approval receipt ID.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_OriginalQuantityInfo, "Original Copy", "Orig Copy", "OC", "Indicates the number of original copies of the application.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_CopyQuantityInfo, "Copy Quantity", "Copy QTY", "CQ", "Indicates the number of copies of the application.");
			BusinessObjectCaptionTestHelper.CombineAssertCaptionsWithFullDescription(controllingMessageHeader.TW1_EUSteelProductNoInfo, "EU Steel Product No.", "EU Steel No.", "EU ST No.", "Indicates the EU steel quota declared category.");
			BusinessObjectCaptionTestHelper.CombineAssertCaptionsWithFullDescription(controllingMessageHeader.TW1_EUSteelProductPhaseInfo, "EU Steel Phase", "EU ST Phase", "EU ST Phase", "Indicates the EU steel quota phase.");
			BusinessObjectCaptionTestHelper.CombineAssertCaptionsWithFullDescription(controllingMessageHeader.TW1_ManufacturerPrintingCodeInfo, "Manufacturer Printing Option", "Manufacturer Printing OPT", "Mfr. Printing OPT", "Indicates the manufacturer's data printing option.");
			BusinessObjectCaptionTestHelper.CombineAssertCaptionsWithFullDescription(controllingMessageHeader.TW1_ReturnPreviousCOOInfo, "Return Previous COO", "Return Pre. COO", "RET Pre. COO", "Indicates the option to return the original Certificate of Origin.");
			BusinessObjectCaptionTestHelper.CombineAssertCaptionsWithFullDescription(controllingMessageHeader.TW1_PrintingCodeInfo, "Printing Option", "Printing OPT", "Printing OPT", "Indicates the print option for the Certificate of Origin.");
			BusinessObjectCaptionTestHelper.CombineAssertCaptionsWithFullDescription(controllingMessageHeader.TW1_IsTriangularTradeInfo, "Triangular Trade", "Triangular Trade", "Triangular Trade", "Indicates that this certificate of origin is a triangular trade.");
			BusinessObjectCaptionTestHelper.CombineAssertCaptionsWithFullDescription(controllingMessageHeader.TW1_RemarksInfo, "Remarks", "Remarks", "Remarks", "Indicates other transaction remarks for the certificate of origin printing.");
			BusinessObjectCaptionTestHelper.CombineAssertCaptionsWithFullDescription(controllingMessageHeader.TW1_BeforeClearanceApplicationReasonInfo, "Reason to apply C/O before clearance", "Before Clearance C/O Application Reason", "Before Clearance Reason", "Indicates the reasons why Certificate of Origin is applied before clearance.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_ECFAPrintedRemarksInfo, "ECFA Printed Remarks", "ECFA Remarks", "ECFA Remarks", "Indicates ECFA print remarks. When the certificate type is '15', the ECFA transaction remarks can be filled in this column and printed in the remarks column on the ECFA certificate of origin.");
			BusinessObjectCaptionTestHelper.CombineAssertCaptionsWithFullDescription(controllingMessageHeader.TW1_ObservationsInfo, "Observations", "Observations", "Obs.", "Indicates observation records. When the certificate type is '09', '11', '13', '14', the applicant can describe the remarks related to the goods.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW_NotesInfo, "Notes", "Indicates other additional details for certificates of origin.");
			BusinessObjectCaptionTestHelper.AssertCaptions(controllingMessageHeader.TW1_SequenceInfo, "Sequence Number", "Sequence No", "Seq. #");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_FunctionalReferenceIdInfo, "Reference", "Ref.", "The identification number of the licensing message. It is a unique number and automatically generated by the system.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_ControllingMessageTypeInfo, "Controlling Message Type", "Msg. Type", "The type of the licensing message.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_ControllingAgencyInfo, "Controlling Agency", "The identification code of the controlling agency.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_CertificateTypeInfo, "Certificate Type", "The type of the certificate.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_ElectronicReceiptInfo, "Apply for Electronic Receipt", "Indicates whether to apply for electronic receipt.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_ProofOfPaperInfo, "Apply for Proof of Paper", "Indicates that the importer (tax obligor, inspection obligor or importer) or inspection/application agent applies for hard copies of the certificate.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_ApplyForSampleReturnInfo, "Apply for Sample Return", "Sample Return", "Indicates that the importer (tax obligor, inspection obligor or importer) or inspection/application agent applies for the return of samples of inspection residue.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_AppointmentDateInfo, "Appointment Date", "The appointment date of the quarantine or inspection.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_AppointmentPeriodInfo, "Appointment Period", "The appointment period of the quarantine or inspection.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_BusinessTypeInfo, "Business Type", "The type of the application business included in the licensing service platform.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_InspectionRegistrationNumberInfo, "Inspection Reg. Number", "Inspection Reg. No.", "The inspection registration number of the inspection application must be obtained in advance according to the regulations of the Bureau of Standards, Metrology and Inspection.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_PaymentMethodInfo, "Payment Method", "The payment method of the licensing fee.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.PermitNumberInfo, "Permit No", "The permit number pre-allocated by the controlling agency.");
			BusinessObjectCaptionTestHelper.AssertCaptions(controllingMessageHeader.PermitNoExpirationDateInfo, "Permit No Expiration Date");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_PrePermitNumberInfo, "Previous Permit Number", "Pre. Permit No.", "The previous permit number that was issued before. The previous permit number of the case that has been inspected and approved by the National Treasury Administration.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_PreWineInspectionStatusInfo, "Previous Wine Inspection Status", "Pre. Wine Ins. Status", "The previous inspection status of this alcohol.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_ProcessingUnitInfo, "Processing Unit", "The processing or issuing unit of the licensing authority.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_PurposeInfo, "Purpose Code", "Purpose", "The purpose of imported goods.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_RequestDescriptionInfo, "Request", "The remarks and marks of special request.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_SampleReturnAddressInfo, "Sample Return Address", "The Chinese address of the sample being returned.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(controllingMessageHeader.TW1_SamplingReductionReasonInfo, "Sampling Reduction Reason", "The reason to apply for reduction of samples.");
			BusinessObjectCaptionTestHelper.AssertCaptions(controllingMessageHeader.BulkApplicationIDInfo, "Bulk Application ID");
			BusinessObjectCaptionTestHelper.AssertCaptions(controllingMessageHeader.TW1_PortOfBulkCommodityInfo, "Port of bulk commodity");
			BusinessObjectCaptionTestHelper.AssertCaptions(controllingMessageHeader.BulkPaymentIDInfo, "Bulk Payment ID");
			BusinessObjectCaptionTestHelper.CombineAssertCaptionsWithFullDescription(controllingMessageHeader.TW1_EntryStatusInfo, "Licensing Status", "Status", "Status", "The status of the licensing message.");
			BusinessObjectCaptionTestHelper.CombineAssertCaptionsWithFullDescription(controllingMessageHeader.LicensingStatusDescriptionInfo, "Licensing Status Description", "Description", "Description", "The status Description of the licensing message.");
		}

		[ExpectNoExceptions]
		public void TestLicensingStatusDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			var messageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			NUnit.Framework.Assert.That(messageHeader.LicensingStatusDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
			messageHeader.TW1_EntryStatus = "Y";
			NUnit.Framework.Assert.That(messageHeader.LicensingStatusDescription, NUnit.Framework.Is.EqualTo("合格").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLicensingMessageStatusDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			var messageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			NUnit.Framework.Assert.That(messageHeader.LicensingMessageStatusDescription, NUnit.Framework.Is.EqualTo("Not Sent").Using(CustomComparers.TypeComparison));
			messageHeader.TW1_MessageStatus = "AWR";
			NUnit.Framework.Assert.That(messageHeader.LicensingMessageStatusDescription, NUnit.Framework.Is.EqualTo("Awaiting Response").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDefaultValues()
		{
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_MessageStatus, NUnit.Framework.Is.EqualTo(TWMessageStatusCodeList.Codes.NotSent).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestECFAPrintedRemarks()
		{
			var factory = new BusinessObjectFactory();
			var messageHeader = factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			messageHeader.TW1_ECFAPrintedRemarks = "Test ECFA Printed Remarks";
			factory.Save();

			var noteQuery = new ZQuery(StmNoteSchema.ST_ParentID, SQLComparisonOperator.Equal, messageHeader.PK);
			noteQuery.AddToFilter(StmNoteSchema.ST_Description, SQLComparisonOperator.Equal, "ECFA Printed Remarks");
			noteQuery.AddToFilter(StmNoteSchema.ST_NoteType, SQLComparisonOperator.Equal, "DOC");
			noteQuery.AddToFilter(StmNoteSchema.ST_Table, SQLComparisonOperator.Equal, "CusTWControllingMessageHeader");
			var note = factory.LoadTop1<StmNote>(noteQuery);
			NUnit.Framework.Assert.That(note.ST_NoteText, NUnit.Framework.Is.EqualTo("Test ECFA Printed Remarks").Using(CustomComparers.TypeComparison));

			note.ST_NoteText = "Modify ECFA Printed Remarks";
			factory.Save();

			var loadInvoiceLine = factory.LoadTop1<CusTWControllingMessageHeader>(new ZQuery(CusTWControllingMessageHeaderSchema.PK, SQLComparisonOperator.Equal, messageHeader.PK));
			NUnit.Framework.Assert.That(loadInvoiceLine.TW1_ECFAPrintedRemarks, NUnit.Framework.Is.EqualTo("Modify ECFA Printed Remarks").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(messageHeader.TW1_ECFAPrintedRemarksInfo.MaxLength, NUnit.Framework.Is.EqualTo(256), "MaxLength");
		}

		[ExpectNoExceptions]
		public void TestDefaultTW1_OriginalQuantity()
		{
			controllingMessageHeader.TW1_OriginalQuantity = 2;
			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_OriginalQuantity, NUnit.Framework.Is.EqualTo((ZByte)1));

			controllingMessageHeader.TW1_OriginalQuantity = 3;
			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code7;
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_OriginalQuantity, NUnit.Framework.Is.EqualTo((ZByte)3));
		}

		[ExpectNoExceptions]
		public void TestIsCertificate15()
		{
			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			NUnit.Framework.Assert.That(controllingMessageHeader.IsCertificate15, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code7;
			NUnit.Framework.Assert.That(controllingMessageHeader.IsCertificate15, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTW1_ControllingMessageType()
		{
			var info = controllingMessageHeader.TW1_ControllingMessageTypeInfo;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(info.MaxLength, NUnit.Framework.Is.EqualTo(8));
				NUnit.Framework.Assert.That(controllingMessageHeader.GetType(), CustomConstraints.HasCustomAttribute<ListAttribute>(nameof(controllingMessageHeader.TW1_ControllingMessageType), false, attrib => attrib.ListDataSourceMember == "Lookups.ControllingMessageTypeList"));
			});
		}

		[ExpectNoExceptions]
		void AssertPropertyValueIsDefaultWhenReadOnly(ZPropertyInfo propertyInfo)
		{
			NUnit.Framework.Assert.That(propertyInfo.ReadOnly, NUnit.Framework.Is.EqualTo(propertyInfo.Value.IsDefault), $"{propertyInfo.Name} value should be a default when property is ReadOnly.");
		}

		[ExpectNoExceptions]
		void AssertBusinessObjectCollectionIsEmptyWhenReadOnly(BusinessObjectCollection businessObjects)
		{
			NUnit.Framework.Assert.That(businessObjects.ReadOnly, NUnit.Framework.Is.EqualTo(!businessObjects.Any()), $"{businessObjects.GetType().Name} should be empty when ReadOnly.");
		}

		void SetPropertyValues()
		{
			var header = controllingMessageHeader;
			header.TW1_ApplyForSampleReturn = true;
			header.TW1_AppointmentDate = ZDate.Today;
			header.TW1_AppointmentPeriod = "X";
			header.TW1_BusinessType = "X";
			header.TW1_CertificateType = "X";
			header.TW1_ElectronicReceipt = true;
			header.TW1_FunctionalReferenceId = "X";
			header.TW1_InspectionRegistrationNumber = "X";
			header.TW1_PaymentMethod = "X";
			header.PermitNumber = "X";
			header.TW1_PrePermitNumber = "X";
			header.TW1_PreWineInspectionStatus = "X";
			header.TW1_ProcessingUnit = "X";
			header.TW1_ProofOfPaper = true;
			header.TW1_RequestDescription = "X";
			header.TW1_Purpose = "X";
			header.TW1_SampleReturnAddress = "X";
			header.TW1_SamplingReductionReason = "X";
			header.TW1_IsSpecialApplication = true;
			header.TW1_SpecialApplicationId = "X";
			header.TW1_CopyQuantity = 1;
			header.TW1_OriginalQuantity = 1;
			header.TW1_EUSteelProductNo = "X";
			header.TW1_EUSteelProductPhase = "X";
			header.TW1_ManufacturerPrintingCode = "X";
			header.TW1_Observations = "X";
			header.TW1_IsTriangularTrade = true;
			header.TW1_Remarks = "X";
			header.TW1_IsEstimatedLoadingDate = true;
			header.EthanolPermitNumbers.AddNew();
			header.ProductLabelRanges.AddNew();
		}

		[ExpectNoExceptions]
		public void TestResetPropertiesOnMessageTypeChanged()
		{
			CombineAssertions(() =>
			{
				var header = controllingMessageHeader;
				var messageTypes = controllingMessageHeader.Lookups.ControllingMessageTypeList.GetAllCodes();
				foreach (var messageType in messageTypes)
				{
					SetPropertyValues();
					header.TW1_ControllingMessageType = messageType;
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_ApplyForSampleReturnInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_AppointmentDateInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_AppointmentPeriodInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_CertificateTypeInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_ElectronicReceiptInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_FunctionalReferenceIdInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_InspectionRegistrationNumberInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_PaymentMethodInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.PermitNumberInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_PrePermitNumberInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_PreWineInspectionStatusInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_ProcessingUnitInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_ProofOfPaperInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_RequestDescriptionInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_PurposeInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_SampleReturnAddressInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_SamplingReductionReasonInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_IsSpecialApplicationInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_SpecialApplicationIdInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_CopyQuantityInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_OriginalQuantityInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_EUSteelProductNoInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_EUSteelProductPhaseInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_ManufacturerPrintingCodeInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_ObservationsInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_IsTriangularTradeInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_RemarksInfo);
					AssertPropertyValueIsDefaultWhenReadOnly(header.TW1_IsEstimatedLoadingDateInfo);
					AssertResetTW1_ControllingAgency(header);
					AssertResetTW1_BusinessType(header);
					AssertBusinessObjectCollectionIsEmptyWhenReadOnly(header.EthanolPermitNumbers);
					AssertBusinessObjectCollectionIsEmptyWhenReadOnly(header.ProductLabelRanges);
				}
			});
		}

		[ExpectNoExceptions]
		void AssertResetTW1_BusinessType(CusTWControllingMessageHeader header)
		{
			var messageType = header.TW1_ControllingMessageType;
			string expected;
			switch (messageType)
			{
				case ControllingMessageTypeList.Codes.NX301_DN:
					expected = "A";
					break;
				case ControllingMessageTypeList.Codes.NX401:
					expected = "30";
					break;
				case ControllingMessageTypeList.Codes.NX603:
					expected = "01";
					break;
				case ControllingMessageTypeList.Codes.NX201_01:
				case ControllingMessageTypeList.Codes.NX201_07:
					expected = "0";
					break;
				case ControllingMessageTypeList.Codes.NX101:
				case ControllingMessageTypeList.Codes.X101:
				case ControllingMessageTypeList.Codes.NX301:
				case ControllingMessageTypeList.Codes.NX301_AX:
				case ControllingMessageTypeList.Codes.NX601:
					expected = "";
					break;
				default:
					throw new Exception($"Unhandled Message Type: {messageType}. Include the unhandled message type in this unit test to resolve this error.");
			}
			NUnit.Framework.Assert.That(header.TW1_BusinessType, NUnit.Framework.Is.EqualTo(expected).Using(CustomComparers.TypeComparison), $"{messageType} BusinessType: default value is not expected");
		}

		[ExpectNoExceptions]
		void AssertResetTW1_ControllingAgency(CusTWControllingMessageHeader header)
		{
			var messageType = header.TW1_ControllingMessageType;
			string expected;
			switch (messageType)
			{
				case ControllingMessageTypeList.Codes.NX101:
				case ControllingMessageTypeList.Codes.X101:
				case ControllingMessageTypeList.Codes.NX201_01:
				case ControllingMessageTypeList.Codes.NX201_07:
					expected = ControllingAgencyList.Codes.FT;
					break;
				case ControllingMessageTypeList.Codes.NX301:
					expected = ControllingAgencyList.Codes.CI;
					break;
				case ControllingMessageTypeList.Codes.NX301_AX:
					expected = ControllingAgencyList.Codes.AX;
					break;
				case ControllingMessageTypeList.Codes.NX301_DN:
					expected = ControllingAgencyList.Codes.DN;
					break;
				case ControllingMessageTypeList.Codes.NX401:
					expected = ControllingAgencyList.Codes.VP;
					break;
				case ControllingMessageTypeList.Codes.NX601:
					expected = ControllingAgencyList.Codes.IF;
					break;
				case ControllingMessageTypeList.Codes.NX603:
					expected = ControllingAgencyList.Codes.CD;
					break;
				default:
					throw new Exception($"Unhandled Message Type: {messageType}. Include the unhandled message type in this unit test to resolve this error.");
			}
			NUnit.Framework.Assert.That(header.TW1_ControllingAgency, NUnit.Framework.Is.EqualTo(expected).Using(CustomComparers.TypeComparison), messageType.ToString());
		}

		[ExpectNoExceptions]
		public void TestSetTW1_ControllingMessageTypeToAndFromNX603()
		{
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_BusinessType, NUnit.Framework.Is.EqualTo(ZString.Empty));
			controllingMessageHeader.TW1_ControllingMessageType = "NX603";
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_BusinessType, NUnit.Framework.Is.EqualTo("01").Using(CustomComparers.TypeComparison));
			controllingMessageHeader.TW1_ControllingMessageType = "NX601";
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_BusinessType, NUnit.Framework.Is.EqualTo(ZString.Empty));
			controllingMessageHeader.TW1_ControllingMessageType = "NX603";
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_BusinessType, NUnit.Framework.Is.EqualTo("01").Using(CustomComparers.TypeComparison));
			controllingMessageHeader.TW1_ControllingMessageType = ZString.Empty;
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_BusinessType, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestControllingMessageTypeDescription()
		{
			controllingMessageHeader.TW1_ControllingAgency = "FT";
			controllingMessageHeader.TW1_ControllingMessageType = ZString.Empty;
			NUnit.Framework.Assert.That(controllingMessageHeader.ControllingMessageTypeDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
			controllingMessageHeader.TW1_ControllingMessageType = "XXX";
			NUnit.Framework.Assert.That(controllingMessageHeader.ControllingMessageTypeDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
			controllingMessageHeader.TW1_ControllingMessageType = "X101";
			NUnit.Framework.Assert.That(controllingMessageHeader.ControllingMessageTypeDescription, NUnit.Framework.Is.EqualTo("產地證明申辦訊息").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestEthanolPermitNumbers()
		{
			CombineAssertions(() =>
			{
				var ethanolPermitNumber1 = controllingMessageHeader.EthanolPermitNumbers.AddNew();
				ethanolPermitNumber1.CSI_ReferenceNumber = "XXX";
				var ethanolPermitNumber2 = controllingMessageHeader.EthanolPermitNumbers.AddNew();
				ethanolPermitNumber2.CSI_ReferenceNumber = "OOO";
				NUnit.Framework.Assert.That(controllingMessageHeader.EthanolPermitNumbers.Count, NUnit.Framework.Is.EqualTo(2));
			});
		}

		[ExpectNoExceptions]
		public void TestPreviousDocumentNumbers()
		{
			var messageHeader = Factory.New<CusTWControllingMessageHeader>();
			NUnit.Framework.Assert.That(messageHeader.PreviousDocumentNumbers, NUnit.Framework.Is.TypeOf<PreviousDocumentNumberCusSupportingCollection>());
		}

		[ExpectNoExceptions]
		public void TestCertificateOfOrigins()
		{
			var messageHeader = Factory.New<CusTWControllingMessageHeader>();
			NUnit.Framework.Assert.That(messageHeader.CertificateOfOrigins, NUnit.Framework.Is.TypeOf<CMCertificateOfOriginCusSupportingCollection>());
		}

		[ExpectNoExceptions]
		public void TestLocalProcessorAddress()
		{
			CombineAssertions(() =>
			{
				controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
				var processorAddress = controllingMessageHeader.LocalProcessorAddress;
				NUnit.Framework.Assert.That(controllingMessageHeader.DocAddresses, NUnit.Framework.Is.TypeOf<TWJobDocAddressDependentCollection>());
				NUnit.Framework.Assert.That(processorAddress, NUnit.Framework.Is.TypeOf<TWJobDocAddress>());
				processorAddress.OrganisationPK = Organization.PK;
				NUnit.Framework.Assert.That(processorAddress.E2_CompanyName, NUnit.Framework.Is.EqualTo("Test Org").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(processorAddress.E2_AddressType, NUnit.Framework.Is.EqualTo("LPA").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(processorAddress.E2_ParentTableCode, NUnit.Framework.Is.EqualTo("TW1").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestDeleteIfEthanolPermitNumbersRowIsEmpty()
		{
			var testControllingMessageHeader = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			var ethanolPermitNumber1 = testControllingMessageHeader.EthanolPermitNumbers.AddNew();
			ethanolPermitNumber1.CSI_ReferenceNumber = "t1";
			var ethanolPermitNumber2 = testControllingMessageHeader.EthanolPermitNumbers.AddNew();
			ethanolPermitNumber2.CSI_ReferenceNumber = "t2";
			Factory.Save();
			var query = new ZDBOnlyQuery(typeof(CusSupportingInfo));
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, testControllingMessageHeader.PK);
			query.AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.EthanolPermitNumber);
			var cusSupportingInfo = Factory.Load(typeof(CusSupportingInfo), query);
			NUnit.Framework.Assert.That(cusSupportingInfo.Length, NUnit.Framework.Is.EqualTo(2));
			ethanolPermitNumber1.CSI_ReferenceNumber = ZString.Empty;
			ethanolPermitNumber2.CSI_ReferenceNumber = ZString.Empty;
			Factory.Save();
			cusSupportingInfo = Factory.Load(typeof(CusSupportingInfo), query);
			NUnit.Framework.Assert.That(cusSupportingInfo.Length, NUnit.Framework.Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public void TestDeleteIfPreviousDocumentNumbersRowIsEmpty()
		{
			var testControllingMessageHeader = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			var previousDocumentNumbers1 = testControllingMessageHeader.PreviousDocumentNumbers.AddNew();
			previousDocumentNumbers1.CSI_ReferenceNumber = "p1";
			var previousDocumentNumbers2 = testControllingMessageHeader.PreviousDocumentNumbers.AddNew();
			previousDocumentNumbers2.CSI_ReferenceNumber = "p2";
			Factory.Save();
			var query = new ZDBOnlyQuery(typeof(CusSupportingInfo));
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, testControllingMessageHeader.PK);
			query.AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.PreviousDocumentNumber);
			var cusSupportingInfo = Factory.Load(typeof(CusSupportingInfo), query);
			NUnit.Framework.Assert.That(cusSupportingInfo.Length, NUnit.Framework.Is.EqualTo(2));
			previousDocumentNumbers1.CSI_ReferenceNumber = ZString.Empty;
			previousDocumentNumbers2.CSI_ReferenceNumber = ZString.Empty;
			Factory.Save();
			cusSupportingInfo = Factory.Load(typeof(CusSupportingInfo), query);
			NUnit.Framework.Assert.That(cusSupportingInfo.Length, NUnit.Framework.Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public void TestDeleteIfCertificateOfOriginsRowIsEmpty()
		{
			var testControllingMessageHeader = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			var certificateOfOrigins1 = testControllingMessageHeader.CertificateOfOrigins.AddNew();
			certificateOfOrigins1.CSI_ReferenceNumber = "c1";
			var certificateOfOrigins2 = testControllingMessageHeader.CertificateOfOrigins.AddNew();
			certificateOfOrigins2.CSI_ReferenceNumber = "c2";
			Factory.Save();
			var query = new ZDBOnlyQuery(typeof(CusSupportingInfo));
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, testControllingMessageHeader.PK);
			query.AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.CmCertificateOfOriginNumber);
			var cusSupportingInfo = Factory.Load(typeof(CusSupportingInfo), query);
			NUnit.Framework.Assert.That(cusSupportingInfo.Length, NUnit.Framework.Is.EqualTo(2));
			certificateOfOrigins1.CSI_ReferenceNumber = ZString.Empty;
			certificateOfOrigins2.CSI_ReferenceNumber = ZString.Empty;
			Factory.Save();
			cusSupportingInfo = Factory.Load(typeof(CusSupportingInfo), query);
			NUnit.Framework.Assert.That(cusSupportingInfo.Length, NUnit.Framework.Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public void TestDelete()
		{
			var testControllingMessageHeader = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			var productLabelRange = testControllingMessageHeader.ProductLabelRanges.AddNew();
			var localProcessorAddress = testControllingMessageHeader.LocalProcessorAddress;
			var ethanolPermitNumber = testControllingMessageHeader.EthanolPermitNumbers.AddNew();
			ethanolPermitNumber.CSI_ReferenceNumber = "e1";
			var previousDocumentNumber = testControllingMessageHeader.PreviousDocumentNumbers.AddNew();
			previousDocumentNumber.CSI_ReferenceNumber = "p1";
			var certificateOfOrigin = testControllingMessageHeader.CertificateOfOrigins.AddNew();
			certificateOfOrigin.CSI_ReferenceNumber = "c1";
			testControllingMessageHeader.PermitNumber = "111";
			testControllingMessageHeader.ProcessingNumber = "111";
			testControllingMessageHeader.CustomsMessageIdentifier = "111";
			var permitEntryNumber = CusEntryNumber.Load(testControllingMessageHeader, CusEntryNumberTypes.Taiwan.Permit, Core.Constants.CountryCodes.Taiwan);
			var processingNumberEntryNumber = CusEntryNumber.Load(testControllingMessageHeader, CusEntryNumberTypes.Taiwan.ProcessingNumber, Core.Constants.CountryCodes.Taiwan);
			var customsMessageIdentifierEntryNumber = CusEntryNumber.Load(testControllingMessageHeader, CusEntryNumberTypes.Taiwan.CustomsMessageIdentifier, Core.Constants.CountryCodes.Taiwan);
			testControllingMessageHeader.Delete();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(productLabelRange.IsDeleted, NUnit.Framework.Is.EqualTo(true), "productLabelRange should be Deleted");
				NUnit.Framework.Assert.That(localProcessorAddress.IsDeleted, NUnit.Framework.Is.EqualTo(true), "localProcessorAddress should be Deleted");
				NUnit.Framework.Assert.That(ethanolPermitNumber.IsDeleted, NUnit.Framework.Is.EqualTo(true), "ethanolPermitNumber should be Deleted");
				NUnit.Framework.Assert.That(previousDocumentNumber.IsDeleted, NUnit.Framework.Is.EqualTo(true), "previousDocumentNumber should be Deleted");
				NUnit.Framework.Assert.That(certificateOfOrigin.IsDeleted, NUnit.Framework.Is.EqualTo(true), "certificateOfOrigin should be Deleted");
				NUnit.Framework.Assert.That(permitEntryNumber.IsDeleted, NUnit.Framework.Is.EqualTo(true), "permitEntryNumber should be Deleted");
				NUnit.Framework.Assert.That(processingNumberEntryNumber.IsDeleted, NUnit.Framework.Is.EqualTo(true), "processingNumberEntryNumber should be Deleted");
				NUnit.Framework.Assert.That(customsMessageIdentifierEntryNumber.IsDeleted, NUnit.Framework.Is.EqualTo(true), "customsMessageIdentifierEntryNumber should be Deleted");
			});
		}

		OrgHeader Organization
		{
			get
			{
				if (organization == null)
				{
					organization = Factory.New<OrgHeader>();
					organization.FillWithValidTestData();
					organization.OH_FullName = "Test Org";
					var address1 = organization.Addresses.AddNew();
					address1.OA_Address1 = "Test 1";
					var address2 = organization.Addresses.AddNew();
					address2.OA_Address1 = "Test 2";
				}

				return organization;
			}
		}

		OrgHeader organization;
		[ExpectNoExceptions]
		public void TestMessages()
		{
			var messageHeader = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			messageHeader.Messages.AddNew();
			NUnit.Framework.Assert.That(messageHeader.Messages.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(messageHeader.Messages, NUnit.Framework.Is.TypeOf<TWMessageCollection>());
		}

		[ExpectNoExceptions]
		public void TestTW1_CertificateType()
		{
			CombineAssertions(() =>
			{
				var messageHeader = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
				messageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code0;
				messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
				NUnit.Framework.Assert.That(messageHeader.TW1_CertificateType, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestDefaultInvoicelineCustomPermitUQWhenSetCertificateType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var messageHeader = instruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			messageHeader.AssignHeaderToInvoiceLines();
			invoiceLine1.JI_PermitUQ = "EAC";
			invoiceLine2.JI_PermitUQ = "DRU";
			messageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			CombineAssertions("When message type is NX101 and TW_CustomPermitUQ is empty, set certificate type to 01, default TW_CustomPermitUQ from JI_PermitUQ", () =>
			{
				NUnit.Framework.Assert.That(invoiceLine1.JI_CustomPermitUQ, NUnit.Framework.Is.EqualTo("EAC").Using(CustomComparers.TypeComparison), "line1");
				NUnit.Framework.Assert.That(invoiceLine2.JI_CustomPermitUQ, NUnit.Framework.Is.EqualTo("DRU").Using(CustomComparers.TypeComparison), "line2");
			});

			invoiceLine1.JI_PermitUQ = "ACR";
			invoiceLine2.JI_PermitUQ = "BAG";
			messageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code7;
			CombineAssertions("When message type is NX101 and TW_CustomPermitUQ has value, set certificate type to 07, do not default TW_CustomPermitUQ", () =>
			{
				NUnit.Framework.Assert.That(invoiceLine1.JI_CustomPermitUQ, NUnit.Framework.Is.EqualTo("EAC").Using(CustomComparers.TypeComparison), "line1");
				NUnit.Framework.Assert.That(invoiceLine2.JI_CustomPermitUQ, NUnit.Framework.Is.EqualTo("DRU").Using(CustomComparers.TypeComparison), "line2");
			});

			messageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			CombineAssertions("When message type is NX101 and TW_CustomPermitUQ has value, set certificate type to 15, empty TW_CustomPermitUQ", () =>
			{
				NUnit.Framework.Assert.That(invoiceLine1.JI_CustomPermitUQ, NUnit.Framework.Is.EqualTo(ZString.Empty), "line1");
				NUnit.Framework.Assert.That(invoiceLine2.JI_CustomPermitUQ, NUnit.Framework.Is.EqualTo(ZString.Empty), "line2");
			});

			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
			messageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			CombineAssertions("When message type is NX301 and TW_CustomPermitUQ is empty, set certificate type to 01, do not default TW_CustomPermitUQ", () =>
			{
				NUnit.Framework.Assert.That(invoiceLine1.JI_CustomPermitUQ, NUnit.Framework.Is.EqualTo(ZString.Empty), "line1, NX301, Code1");
				NUnit.Framework.Assert.That(invoiceLine2.JI_CustomPermitUQ, NUnit.Framework.Is.EqualTo(ZString.Empty), "line2, NX301, Code1");
			});
		}

		[ExpectNoExceptions]
		public void TestISMessageType()
		{
			bool expectedIsNX101, expectedIsX101, expectedIsNX201_01, expectedIsNX201_07, expectedIsNX301, expectedIsNX301_AX, expectedIsNX301_DN, expectedIsNX401, expectedIsNX601, expectedIsNX603;
			var messageHeader = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();

			CombineAssertions(() =>
			{
				foreach (var messageType in messageHeader.Lookups.ControllingMessageTypeList.GetAllCodes())
				{
					messageHeader.TW1_ControllingMessageType = messageType;
					expectedIsNX101 = expectedIsX101 = expectedIsNX201_01 = expectedIsNX201_07 = expectedIsNX301 = expectedIsNX301_AX = expectedIsNX301_DN = expectedIsNX401 = expectedIsNX601 = expectedIsNX603 = false;
					switch (messageType)
					{
						case ControllingMessageTypeList.Codes.NX101:
							expectedIsNX101 = true;
							break;
						case ControllingMessageTypeList.Codes.X101:
							expectedIsX101 = true;
							break;
						case ControllingMessageTypeList.Codes.NX201_01:
							expectedIsNX201_01 = true;
							break;
						case ControllingMessageTypeList.Codes.NX201_07:
							expectedIsNX201_07 = true;
							break;
						case ControllingMessageTypeList.Codes.NX301:
							expectedIsNX301 = true;
							break;
						case ControllingMessageTypeList.Codes.NX301_AX:
							expectedIsNX301_AX = true;
							break;
						case ControllingMessageTypeList.Codes.NX301_DN:
							expectedIsNX301_DN = true;
							break;
						case ControllingMessageTypeList.Codes.NX401:
							expectedIsNX401 = true;
							break;
						case ControllingMessageTypeList.Codes.NX601:
							expectedIsNX601 = true;
							break;
						case ControllingMessageTypeList.Codes.NX603:
							expectedIsNX603 = true;
							break;
					}
					NUnit.Framework.Assert.That(messageHeader.IsNX101, NUnit.Framework.Is.EqualTo(expectedIsNX101).Using(CustomComparers.TypeComparison), $"{messageType}: IsNX101");
					NUnit.Framework.Assert.That(messageHeader.IsX101, NUnit.Framework.Is.EqualTo(expectedIsX101).Using(CustomComparers.TypeComparison), $"{messageType}: IsX101");
					NUnit.Framework.Assert.That(messageHeader.IsNX201_01, NUnit.Framework.Is.EqualTo(expectedIsNX201_01).Using(CustomComparers.TypeComparison), $"{messageType}: IsNX201_01");
					NUnit.Framework.Assert.That(messageHeader.IsNX201_07, NUnit.Framework.Is.EqualTo(expectedIsNX201_07).Using(CustomComparers.TypeComparison), $"{messageType}: IsNX201_07");
					NUnit.Framework.Assert.That(messageHeader.IsNX301, NUnit.Framework.Is.EqualTo(expectedIsNX301).Using(CustomComparers.TypeComparison), $"{messageType}: IsNX301");
					NUnit.Framework.Assert.That(messageHeader.IsNX301_AX, NUnit.Framework.Is.EqualTo(expectedIsNX301_AX).Using(CustomComparers.TypeComparison), $"{messageType}: IsNX301_AX");
					NUnit.Framework.Assert.That(messageHeader.IsNX301_DN, NUnit.Framework.Is.EqualTo(expectedIsNX301_DN).Using(CustomComparers.TypeComparison), $"{messageType}: IsNX301_DN");
					NUnit.Framework.Assert.That(messageHeader.IsNX401, NUnit.Framework.Is.EqualTo(expectedIsNX401).Using(CustomComparers.TypeComparison), $"{messageType}: IsNX401");
					NUnit.Framework.Assert.That(messageHeader.IsNX601, NUnit.Framework.Is.EqualTo(expectedIsNX601).Using(CustomComparers.TypeComparison), $"{messageType}: IsNX601");
					NUnit.Framework.Assert.That(messageHeader.IsNX603, NUnit.Framework.Is.EqualTo(expectedIsNX603).Using(CustomComparers.TypeComparison), $"{messageType}: IsNX603");
				}
			});
		}

		[ExpectNoExceptions]
		public void TestIsNX101ContainZZZCertificateTypes()
		{
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			CombineAssertions("NX101", () =>
			{
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code0, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code1, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code2, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code4, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code5, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code6, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code7, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code8, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code9, true, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code10, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code11, true, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code12, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code13, true, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code14, true, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code15, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code16, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code17, false, true);
			});

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			CombineAssertions("Not NX101", () =>
			{
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code0, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code1, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code2, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code4, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code5, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code6, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code7, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code8, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code9, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code10, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code11, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code12, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code13, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code14, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code15, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code16, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code17, false, false);
			});

			void AssertIsNX101ContainZZZCertificateTypes(string certificateType, bool expectedIsNX101ContainZZZCertificateTypes, bool expectedIsNX101NotContainZZZCertificateTypes)
			{
				controllingMessageHeader.TW1_CertificateType = certificateType;
				NUnit.Framework.Assert.That(controllingMessageHeader.IsNX101ContainZZZCertificateTypes, NUnit.Framework.Is.EqualTo(expectedIsNX101ContainZZZCertificateTypes).Using(CustomComparers.TypeComparison), string.Format("IsNX101ContainZZZCertificateTypes When Certificate Type is {0}", certificateType));
				NUnit.Framework.Assert.That(controllingMessageHeader.IsNX101NotContainZZZCertificateTypes, NUnit.Framework.Is.EqualTo(expectedIsNX101NotContainZZZCertificateTypes).Using(CustomComparers.TypeComparison), string.Format("IsNX101NotContainZZZCertificateTypes When Certificate Type is {0}", certificateType));
			}
		}

		[ExpectNoExceptions]
		public void TestClearValuesWhenMessageIsX101()
		{
			var messageHeader = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			messageHeader.TW1_PaymentMethod = "X";
			messageHeader.PermitNumber = "G";
			NUnit.Framework.Assert.That(messageHeader.TW1_PaymentMethod, NUnit.Framework.Is.EqualTo("X").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(messageHeader.PermitNumber, NUnit.Framework.Is.EqualTo("G").Using(CustomComparers.TypeComparison));
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.X101;
			NUnit.Framework.Assert.That(messageHeader.TW1_PaymentMethod.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(messageHeader.PermitNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestControllingAgencyDescription()
		{
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.X101;
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_ControllingAgency, NUnit.Framework.Is.EqualTo("FT").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(controllingMessageHeader.ControllingAgencyDescription, NUnit.Framework.Is.EqualTo("FT DES.").Using(CustomComparers.TypeComparison));
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_ControllingAgency, NUnit.Framework.Is.EqualTo("VP").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(controllingMessageHeader.ControllingAgencyDescription, NUnit.Framework.Is.EqualTo("VP DES.").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCertificateTypeDescription()
		{
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.X101;
			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code0;
			NUnit.Framework.Assert.That(controllingMessageHeader.CertificateTypeDescription, NUnit.Framework.Is.EqualTo("其他原產地証明(進口國制定格式)").Using(CustomComparers.TypeComparison));
			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			NUnit.Framework.Assert.That(controllingMessageHeader.CertificateTypeDescription, NUnit.Framework.Is.EqualTo("一般原產地證明").Using(CustomComparers.TypeComparison));
			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code2;
			NUnit.Framework.Assert.That(controllingMessageHeader.CertificateTypeDescription, NUnit.Framework.Is.EqualTo("優惠關稅原產地證明").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBusinessTypeDescription()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			messageHeader.TW1_BusinessType = CPT_111_BusinessTypeList.Codes.Inspection;
			NUnit.Framework.Assert.That(messageHeader.BusinessTypeDescription, NUnit.Framework.Is.EqualTo("查驗申辦").Using(CustomComparers.TypeComparison));
			messageHeader.TW1_BusinessType = CPT_111_BusinessTypeList.Codes.Recheck;
			NUnit.Framework.Assert.That(messageHeader.BusinessTypeDescription, NUnit.Framework.Is.EqualTo("複驗申辦").Using(CustomComparers.TypeComparison));
			messageHeader.TW1_BusinessType = CPT_111_BusinessTypeList.Codes.ExemptionFromInspection;
			NUnit.Framework.Assert.That(messageHeader.BusinessTypeDescription, NUnit.Framework.Is.EqualTo("免驗申辦").Using(CustomComparers.TypeComparison));
			declaration.JE_MessageType = "EXP";
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			messageHeader.TW1_BusinessType = CPT_111_BusinessTypeList.Codes.QuarantineOfExportAnimal;
			NUnit.Framework.Assert.That(messageHeader.BusinessTypeDescription, NUnit.Framework.Is.EqualTo("動物輸出檢疫申請").Using(CustomComparers.TypeComparison));
			messageHeader.TW1_BusinessType = CPT_111_BusinessTypeList.Codes.QuarantineOfExportPlant;
			NUnit.Framework.Assert.That(messageHeader.BusinessTypeDescription, NUnit.Framework.Is.EqualTo("植物輸出檢疫申請").Using(CustomComparers.TypeComparison));
			declaration.JE_MessageType = "IMP";
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			messageHeader.TW1_BusinessType = CPT_111_BusinessTypeList.Codes.QuarantineOfImportAnimal;
			NUnit.Framework.Assert.That(messageHeader.BusinessTypeDescription, NUnit.Framework.Is.EqualTo("動物輸入檢疫申請").Using(CustomComparers.TypeComparison));
			messageHeader.TW1_BusinessType = CPT_111_BusinessTypeList.Codes.QuarantineOfImportPlant;
			NUnit.Framework.Assert.That(messageHeader.BusinessTypeDescription, NUnit.Framework.Is.EqualTo("植物輸入檢疫申請").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestProcessingUnitDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit, "TWReceivingUnit");
			var processingUnit1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit, "XXXX0123", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(processingUnit1.PK, RefCusCodeListAttributeTypes.Codes.ControlAgency, ControllingAgencyList.Codes._20);
			var processingUnit2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit, "XXXX4567", "ZZZZZZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(processingUnit2.PK, RefCusCodeListAttributeTypes.Codes.ControlAgency, ControllingAgencyList.Codes.VP);
			Factory.Save();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
			controllingMessageHeader.TW1_ControllingAgency = ControllingAgencyList.Codes._20;
			controllingMessageHeader.TW1_ProcessingUnit = "XXXX0123";
			NUnit.Framework.Assert.That(controllingMessageHeader.ProcessingUnitDescription, NUnit.Framework.Is.EqualTo("XXXXXX").Using(CustomComparers.TypeComparison));
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			controllingMessageHeader.TW1_ControllingAgency = ControllingAgencyList.Codes.VP;
			controllingMessageHeader.TW1_ProcessingUnit = "XXXX4567";
			NUnit.Framework.Assert.That(controllingMessageHeader.ProcessingUnitDescription, NUnit.Framework.Is.EqualTo("ZZZZZZ").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestControllingMessageHeaderLinkInvoiceLines()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line3 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			var testCollection = messageHeader.ControllingMessageHeaderLinkInvoiceLines;
			NUnit.Framework.Assert.That(testCollection.Count, NUnit.Framework.Is.EqualTo(3));

			var linePks = testCollection.Cast<ControllingMessageHeaderLinkInvoiceLine>().Select(x => x.InvoicelinePK);
			NUnit.Framework.Assert.That(linePks, NUnit.Framework.Has.Some.EqualTo(line1.PK));
			NUnit.Framework.Assert.That(linePks, NUnit.Framework.Has.Some.EqualTo(line2.PK));
			NUnit.Framework.Assert.That(linePks, NUnit.Framework.Has.Some.EqualTo(line3.PK));
		}

		[ExpectNoExceptions]
		public void TestIsControllingMessageHeaderLinkInvoiceLinesLoaded()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line3 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			NUnit.Framework.Assert.That(!messageHeader.IsControllingMessageHeaderLinkInvoiceLinesLoaded, NUnit.Framework.Is.True);

			var testCollection = messageHeader.ControllingMessageHeaderLinkInvoiceLines;
			NUnit.Framework.Assert.That(messageHeader.IsControllingMessageHeaderLinkInvoiceLinesLoaded, NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestLookupsType()
		{
			NUnit.Framework.Assert.That(controllingMessageHeader.Lookups, NUnit.Framework.Is.TypeOf<CusTWControllingMessageHeaderLookups>());
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			NUnit.Framework.Assert.That(controllingMessageHeader.Lookups, NUnit.Framework.Is.TypeOf<CusTWControllingMessageHeaderNX101Lookups>());
		}

		[ExpectNoExceptions]
		public void TestClearEthanolPermitNumbersOnReadOnlyChangedIfReadOnly()
		{
			CombineAssertions(() =>
			{
				var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
				_ = header.EthanolPermitNumbers.AddNew();
				NUnit.Framework.Assert.That(header.EthanolPermitNumbers.Count, NUnit.Framework.Is.EqualTo(1));
				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
				NUnit.Framework.Assert.That(header.EthanolPermitNumbers.Count, NUnit.Framework.Is.EqualTo(0));
			});
		}

		[ExpectNoExceptions]
		public void TestITWMessageInfoProvider()
		{
			controllingMessageHeader.TW1_FunctionalReferenceId = "96944490002301110001";
			ITWMessageInfoProvider provider = controllingMessageHeader;
			CombineAssertions("ITWMessageInfoProvider members", () =>
			{
				NUnit.Framework.Assert.That(provider.EntryNumber, NUnit.Framework.Is.EqualTo("96944490002301110001").Using(CustomComparers.TypeComparison), "Entry Number");
				NUnit.Framework.Assert.That(provider.EntryNumberType, NUnit.Framework.Is.EqualTo(CusEntryNumberTypes.Taiwan.LicensingMessage).Using(CustomComparers.TypeComparison), "Entry Number Type");
				NUnit.Framework.Assert.That(provider.StaffCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Staff Code - should be [null] or [empty]");
				NUnit.Framework.Assert.That(provider.CompanyID, NUnit.Framework.Is.EqualTo(GlbCompany.CurrentCompany.GC_Code), "Company ID");
				NUnit.Framework.Assert.That(provider.PasswordType, NUnit.Framework.Is.EqualTo(PasswordTypesList.Codes.NXM).Using(CustomComparers.TypeComparison), "Password Type");
			});
		}

		[ExpectNoExceptions]
		public void TestSupplierDocumentaryAddress()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "ORG";
			supplier.OH_FullName = "DUMMY COMP";
			supplier.MainAddress.OA_Address1 = "Address 1";

			var supplierDocAddr = controllingMessageHeader.SupplierDocumentaryAddress;
			NUnit.Framework.Assert.That(supplierDocAddr.DocAddressType, NUnit.Framework.Is.EqualTo(DocAddressType.SupplierDocumentaryAddress), "DocAddressType should be SupplierDocumentaryAddress");

			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_OH_Supplier.IsEmpty, NUnit.Framework.Is.True);
			supplierDocAddr.OrganisationPK = supplier.PK;
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_OH_Supplier, NUnit.Framework.Is.EqualTo(supplier.PK), "TW1_OH_Supplier should be set.");
		}

		[ExpectNoExceptions]
		public void TestApplicantDocumentaryAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = "DUMMY COMP";
			org.MainAddress.OA_Address1 = "Address 1";

			var applicantDocAddr = controllingMessageHeader.ApplicantDocumentaryAddress;
			NUnit.Framework.Assert.That(applicantDocAddr.DocAddressType, NUnit.Framework.Is.EqualTo(DocAddressType.Applicant), "DocAddressType should be Applicant");

			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_OH_Applicant.IsEmpty, NUnit.Framework.Is.True);
			applicantDocAddr.OrganisationPK = org.PK;
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_OH_Applicant, NUnit.Framework.Is.EqualTo(org.PK), "TW1_OH_Applicant should be set.");
		}

		[ExpectNoExceptions]
		public void TestImporterDocumentaryAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = "DUMMY COMP";
			org.MainAddress.OA_Address1 = "Address 1";

			var importerDocAddr = controllingMessageHeader.ImporterDocumentaryAddress;
			NUnit.Framework.Assert.That(importerDocAddr.DocAddressType, NUnit.Framework.Is.EqualTo(DocAddressType.ImporterDocumentaryAddress), "DocAddressType should be ImporterDocumentaryAddress");

			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_OH_Importer.IsEmpty, NUnit.Framework.Is.True);
			importerDocAddr.OrganisationPK = org.PK;
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_OH_Importer, NUnit.Framework.Is.EqualTo(org.PK), "TW1_OH_Importer should be set.");
		}

		[ExpectNoExceptions]
		public void TestApplicantDocumentaryAddressDefault()
		{
			var supplierOrg = Factory.New<OrgHeader>();
			supplierOrg.OH_Code = "ORS";
			supplierOrg.OH_FullName = "DUMMY COMP1";
			supplierOrg.MainAddress.OA_Address1 = "Address 1";

			var importerOrg = Factory.New<OrgHeader>();
			importerOrg.OH_Code = "ORI";
			importerOrg.OH_FullName = "DUMMY COMP2";
			importerOrg.MainAddress.OA_Address1 = "Address 1";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.SupplierDocumentaryAddress.OrganisationPK = supplierOrg.PK;
			declaration.ImporterDocumentaryAddress.OrganisationPK = importerOrg.PK;
			var entryInstruction = declaration.CusEntryInstruction;
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			NUnit.Framework.Assert.That(messageHeader.ApplicantDocumentaryAddress.Organisation.PK, NUnit.Framework.Is.EqualTo(supplierOrg.PK));

			declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
			declaration.SupplierDocumentaryAddress.IDCode = "123";
			messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(messageHeader.ApplicantDocumentaryAddress.E2_AddressOverride, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(messageHeader.ApplicantDocumentaryAddress.E2_CompanyName, NUnit.Framework.Is.EqualTo(declaration.SupplierDocumentaryAddress.E2_CompanyName));
				NUnit.Framework.Assert.That(messageHeader.ApplicantDocumentaryAddress.IDCode, NUnit.Framework.Is.EqualTo(declaration.SupplierDocumentaryAddress.IDCode));
			});

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			NUnit.Framework.Assert.That(messageHeader.ApplicantDocumentaryAddress.Organisation.PK, NUnit.Framework.Is.EqualTo(importerOrg.PK));
		}

		[ExpectNoExceptions]
		public void TestSupplierDocumentaryAddressDefault()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = "DUMMY COMP";
			org.MainAddress.OA_Address1 = "Address 1";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.SupplierDocumentaryAddress.OrganisationPK = org.PK;
			var entryInstruction = declaration.CusEntryInstruction;
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			NUnit.Framework.Assert.That(messageHeader.SupplierDocumentaryAddress.Organisation.PK, NUnit.Framework.Is.EqualTo(org.PK));

			declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
			declaration.SupplierDocumentaryAddress.IDCode = "123";
			messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			NUnit.Framework.Assert.That(messageHeader.SupplierDocumentaryAddress.E2_AddressOverride, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(messageHeader.SupplierDocumentaryAddress.E2_CompanyName, NUnit.Framework.Is.EqualTo(declaration.SupplierDocumentaryAddress.E2_CompanyName));
			NUnit.Framework.Assert.That(messageHeader.ApplicantDocumentaryAddress.IDCode, NUnit.Framework.Is.EqualTo(declaration.SupplierDocumentaryAddress.IDCode));
		}

		[ExpectNoExceptions]
		public void TestImporterDocumentaryAddressDefault()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = "DUMMY COMP";
			org.MainAddress.OA_Address1 = "Address 1";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.ImporterDocumentaryAddress.OrganisationPK = org.PK;
			var entryInstruction = declaration.CusEntryInstruction;
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			NUnit.Framework.Assert.That(messageHeader.ImporterDocumentaryAddress.Organisation.PK, NUnit.Framework.Is.EqualTo(org.PK));

			declaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
			declaration.ImporterDocumentaryAddress.IDCode = "123";
			messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			NUnit.Framework.Assert.That(messageHeader.ImporterDocumentaryAddress.E2_AddressOverride, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(messageHeader.ImporterDocumentaryAddress.E2_CompanyName, NUnit.Framework.Is.EqualTo(declaration.ImporterDocumentaryAddress.E2_CompanyName));
			NUnit.Framework.Assert.That(messageHeader.ImporterDocumentaryAddress.IDCode, NUnit.Framework.Is.EqualTo(declaration.ImporterDocumentaryAddress.IDCode));
		}

		[ExpectNoExceptions]
		public void TestTW_Notes()
		{
			NUnit.Framework.Assert.That(controllingMessageHeader.TW_NotesInfo.MaxLength, NUnit.Framework.Is.EqualTo(512), "MaxLength");
			NUnit.Framework.Assert.That(controllingMessageHeader.TW_Notes, NUnit.Framework.Is.EqualTo(ZString.Empty));
			controllingMessageHeader.TW_Notes = "TEST NOTES";

			var noteQuery = new ZQuery(StmNoteSchema.ST_ParentID, controllingMessageHeader.PK);
			noteQuery.AddToFilter(StmNoteSchema.ST_Description, SQLComparisonOperator.Equal, "NX101_Notes");
			noteQuery.AddToFilter(StmNoteSchema.ST_NoteType, SQLComparisonOperator.Equal, "DOC");
			noteQuery.AddToFilter(StmNoteSchema.ST_Table, SQLComparisonOperator.Equal, "CusTWControllingMessageHeader");
			var note = Factory.LoadTop1<StmNote>(noteQuery);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(note.ST_NoteText, NUnit.Framework.Is.EqualTo("TEST NOTES").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(controllingMessageHeader.TW_Notes, NUnit.Framework.Is.EqualTo("TEST NOTES").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestTW1_FunctionalReferenceId()
		{
			NUnit.Framework.Assert.That(!controllingMessageHeader.TW1_FunctionalReferenceIdInfo.ReadOnly, NUnit.Framework.Is.True, "Should be editable");
		}

		[ExpectNoExceptions]
		public void TestPermitNumber()
		{
			controllingMessageHeader.PermitNumber = "123";
			var entryNum = CusEntryNumber.Load(controllingMessageHeader, CusEntryNumberTypes.Taiwan.Permit, Core.Constants.CountryCodes.Taiwan);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryNum.CE_EntryNum, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(controllingMessageHeader.PermitNumberInfo.MaxLength, NUnit.Framework.Is.EqualTo(14));
			});
		}

		[ExpectNoExceptions]
		public void TestPermitNoExpirationDate()
		{
			controllingMessageHeader.PermitNoExpirationDate = ZDateTime.BrettsBirthday;
			var entryNum = CusEntryNumber.Load(controllingMessageHeader, CusEntryNumberTypes.Taiwan.Permit, Core.Constants.CountryCodes.Taiwan);
			NUnit.Framework.Assert.That(entryNum.CE_ExpiryDate, NUnit.Framework.Is.EqualTo(ZDateTime.BrettsBirthday));
		}

		[ExpectNoExceptions]
		public void TestPermitNumber_Delete()
		{
			controllingMessageHeader.PermitNumber = "123";
			var entryNum = CusEntryNumber.Load(controllingMessageHeader, CusEntryNumberTypes.Taiwan.Permit, Core.Constants.CountryCodes.Taiwan);
			controllingMessageHeader.Delete();
			NUnit.Framework.Assert.That(entryNum.IsDeleted, NUnit.Framework.Is.True, "Entry Number should marked deleted");
		}

		[ExpectNoExceptions]
		public void TestProcessingNumber()
		{
			controllingMessageHeader.ProcessingNumber = "1";
			Factory.Save();
			var entryNumber = LoadEntryNumber("PRS");
			NUnit.Framework.Assert.That(entryNumber.CE_EntryNum, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));

			controllingMessageHeader.ProcessingNumber = ZString.Empty;
			entryNumber = LoadEntryNumber("PRS");
			NUnit.Framework.Assert.That(entryNumber, NUnit.Framework.Is.EqualTo(default(CusEntryNumber)), "entry Number be null when Processing Number clearan - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestCustomsMessageIdentifier()
		{
			controllingMessageHeader.CustomsMessageIdentifier = "2";
			Factory.Save();
			var entryNumber = LoadEntryNumber("CMI");
			NUnit.Framework.Assert.That(entryNumber.CE_EntryNum, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison));

			controllingMessageHeader.CustomsMessageIdentifier = ZString.Empty;
			entryNumber = LoadEntryNumber("CMI");
			NUnit.Framework.Assert.That(entryNumber, NUnit.Framework.Is.EqualTo(default(CusEntryNumber)), "entry Number be null when Customs Message Identifier clearan - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestApplicantDocumentaryAddressRequirementType()
		{
			var applicantDocumentaryAddress = controllingMessageHeader.ApplicantDocumentaryAddress;
			applicantDocumentaryAddress.E2_AddressOverride = true;
			NUnit.Framework.Assert.That(applicantDocumentaryAddress.Requirement, NUnit.Framework.Is.TypeOf<ControllingMessageHeaderApplicantAddressRequirement>());
			NUnit.Framework.Assert.That(applicantDocumentaryAddress.LocalAddress.Requirement, NUnit.Framework.Is.TypeOf<ApplicantLocalAddressRequirement>());
		}

		[ExpectNoExceptions]
		public void TestSupplierDocumentaryAddressRequirementType()
		{
			var supplierDocumentaryAddress = controllingMessageHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			NUnit.Framework.Assert.That(supplierDocumentaryAddress.Requirement, NUnit.Framework.Is.TypeOf<ControllingMessageHeaderSupplierAddressRequirement>());
			NUnit.Framework.Assert.That(supplierDocumentaryAddress.LocalAddress.Requirement, NUnit.Framework.Is.TypeOf<SupplierLocalAddressRequirement>());
		}

		[ExpectNoExceptions]
		public void TestImporterDocumentaryAddressRequirementType()
		{
			var importerDocumentaryAddress = controllingMessageHeader.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = true;
			NUnit.Framework.Assert.That(importerDocumentaryAddress.Requirement, NUnit.Framework.Is.TypeOf<ControllingMessageHeaderImporterAddressRequirement>());
			NUnit.Framework.Assert.That(importerDocumentaryAddress.LocalAddress.Requirement, NUnit.Framework.Is.TypeOf<ControllingMessageHeaderImporterLocalAddressRequirement>());
		}

		CusEntryNumber LoadEntryNumber(ZString entryType)
		{
			var entryNumQuery = new ZDBOnlyQuery(typeof(CusEntryNumber));
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_ParentID, controllingMessageHeader.PK);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_Category, "CUS");
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Taiwan);
			var entryNum = Factory.LoadTop1<CusEntryNumber>(entryNumQuery);
			return entryNum;
		}

		protected override void SetUp()
		{
			base.SetUp();
			new TestTWCreator(Factory).CreateRefCusCodeForControllingMessageType();
			controllingMessageHeader = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
		}

		CusTWControllingMessageHeader controllingMessageHeader;
	}
}
