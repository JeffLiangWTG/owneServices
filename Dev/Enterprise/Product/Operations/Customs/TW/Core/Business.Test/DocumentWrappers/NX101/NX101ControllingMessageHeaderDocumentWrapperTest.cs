using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.TW.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(NX101ControllingMessageHeaderDocumentWrapper))]
	sealed class NX101ControllingMessageHeaderDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestCertificateType()
		{
			header.TW1_CertificateType = CertificateTypeList.Codes.Code8;
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(documentWrapper.CertificateType, NUnit.Framework.Is.EqualTo(CertificateTypeList.Codes.Code8).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestExporter()
		{
			var org = new TestTWCreator(Factory).CreateOrganization();
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.PassportID, "PAS001", Core.Constants.CountryCodes.Taiwan);
			var supplierDocumentaryAddress = header.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.OrganisationPK = org.PK;

			supplierDocumentaryAddress.E2_AddressOverride = true;
			var supplierTranslatedDocumentaryAddress = supplierDocumentaryAddress.LocalAddress;
			supplierTranslatedDocumentaryAddress.E2_AddressType = "STA";
			supplierTranslatedDocumentaryAddress.E2_ParentTableCode = "TW1";
			supplierTranslatedDocumentaryAddress.CompanyName = "快樂股份有限公司";

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(documentWrapper.ExporterInfos, NUnit.Framework.Is.EqualTo("PAS001\r\nHAPPY CO., LTD.\r\n快樂股份有限公司\r\n1500 HAPPY RD ORANGE DISTRICT APPLE CITY TAIPEI 12345 TAIWAN\r\n90093台灣臺北巿臺北加工出口區園東街6號\r\nTEL:13925568211\r\nFAX:13925579322\r\nEMAIL:123@456.com").Using(CustomComparers.TypeComparison), "ExporterInfos");
				NUnit.Framework.Assert.That(documentWrapper.ExporterNameAndAddress, NUnit.Framework.Is.EqualTo("HAPPY CO., LTD.\r\n1500 HAPPY RD ORANGE DISTRICT APPLE CITY TAIPEI 12345 TAIWAN\r\n快樂股份有限公司\r\n90093台灣臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison), "ExporterNameAndAddress");
				NUnit.Framework.Assert.That(documentWrapper.ExporterTypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison), "ExporterTypeCode");
				NUnit.Framework.Assert.That(documentWrapper.ExporterID, NUnit.Framework.Is.EqualTo("PAS001").Using(CustomComparers.TypeComparison), "ExporterID");
				NUnit.Framework.Assert.That(documentWrapper.ExporterName, NUnit.Framework.Is.EqualTo("HAPPY CO., LTD.").Using(CustomComparers.TypeComparison), "ExporterName");
				NUnit.Framework.Assert.That(documentWrapper.ExporterChineseName, NUnit.Framework.Is.EqualTo("快樂股份有限公司").Using(CustomComparers.TypeComparison), "ExporterChineseName");
				NUnit.Framework.Assert.That(documentWrapper.ExporterAddressLine, NUnit.Framework.Is.EqualTo("1500 HAPPY RD ORANGE DISTRICT APPLE CITY TAIPEI 12345 TAIWAN").Using(CustomComparers.TypeComparison), "ExporterAddressLine");
				NUnit.Framework.Assert.That(documentWrapper.ExporterAddressChineseLineLine, NUnit.Framework.Is.EqualTo("90093台灣臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison), "ExporterAddressChineseLineLine");
				NUnit.Framework.Assert.That(documentWrapper.ExporterTEL, NUnit.Framework.Is.EqualTo("13925568211").Using(CustomComparers.TypeComparison), "ExporterTEL");
				NUnit.Framework.Assert.That(documentWrapper.ExporterFAX, NUnit.Framework.Is.EqualTo("13925579322").Using(CustomComparers.TypeComparison), "ExporterFAX");
				NUnit.Framework.Assert.That(documentWrapper.ExporterEMAIL, NUnit.Framework.Is.EqualTo("123@456.com").Using(CustomComparers.TypeComparison), "ExporterEMAIL");
			});

			header.TW1_CertificateType = CertificateTypeList.Codes.Code9;
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(documentWrapper.ExporterInfos, NUnit.Framework.Is.EqualTo("HAPPY CO., LTD.\r\n快樂股份有限公司\r\n1500 HAPPY RD ORANGE DISTRICT APPLE CITY TAIPEI 12345 TAIWAN\r\n90093台灣臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison), "ExporterInfos");

			header.TW1_CertificateType = CertificateTypeList.Codes.Code11;
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(documentWrapper.ExporterInfos, NUnit.Framework.Is.EqualTo("HAPPY CO., LTD.\r\n快樂股份有限公司\r\n1500 HAPPY RD ORANGE DISTRICT APPLE CITY TAIPEI 12345 TAIWAN\r\n90093台灣臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison), "ExporterInfos");

			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(documentWrapper.ExporterInfos, NUnit.Framework.Is.EqualTo("HAPPY CO., LTD.\r\n快樂股份有限公司\r\n1500 HAPPY RD ORANGE DISTRICT APPLE CITY TAIPEI 12345 TAIWAN\r\n臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison), "ExporterInfos");

			header.TW1_CertificateType = CertificateTypeList.Codes.Code18;
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(documentWrapper.ExporterInfos, NUnit.Framework.Is.EqualTo("HAPPY CO., LTD.\r\n快樂股份有限公司\r\n1500 HAPPY RD ORANGE DISTRICT APPLE CITY TAIPEI 12345 TAIWAN\r\n90093台灣臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison), "ExporterInfos for Code18");

			header.TW1_CertificateType = CertificateTypeList.Codes.Code19;
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(documentWrapper.ExporterInfos, NUnit.Framework.Is.EqualTo("HAPPY CO., LTD.\r\n快樂股份有限公司\r\n1500 HAPPY RD ORANGE DISTRICT APPLE CITY TAIPEI 12345 TAIWAN\r\n90093台灣臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison), "ExporterInfos for Code19");
		}

		[ExpectNoExceptions]
		public void TestImporter()
		{
			var org = new TestTWCreator(Factory).CreateOrganization();
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.PassportID, "PAS001", Core.Constants.CountryCodes.Taiwan);
			var importerDocumentaryAddress = header.ImporterDocumentaryAddress;
			importerDocumentaryAddress.OrganisationPK = org.PK;

			importerDocumentaryAddress.E2_AddressOverride = true;
			var importerTranslatedDocumentaryAddress = importerDocumentaryAddress.LocalAddress;
			importerTranslatedDocumentaryAddress.E2_AddressType = "STA";
			importerTranslatedDocumentaryAddress.E2_ParentTableCode = "TW1";
			importerTranslatedDocumentaryAddress.CompanyName = "快樂股份有限公司";

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(documentWrapper.ImporterInfos, NUnit.Framework.Is.EqualTo("90093台灣TPE臺北巿臺北加工出口區園東街6號\r\n快樂股份有限公司\r\nHAPPY CO., LTD.\r\n1500 HAPPY RD ORANGE DISTRICT APPLE CITY TPE 12345 TAIWAN\r\nTEL:13925568211\r\nFAX:13925579322\r\nEMAIL:123@456.com").Using(CustomComparers.TypeComparison), "ImporterInfos");
				NUnit.Framework.Assert.That(documentWrapper.ImporterTypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison), "ImporterTypeCode");
				NUnit.Framework.Assert.That(documentWrapper.ImporterID, NUnit.Framework.Is.EqualTo("PAS001").Using(CustomComparers.TypeComparison), "ImporterID");
				NUnit.Framework.Assert.That(documentWrapper.ImporterName, NUnit.Framework.Is.EqualTo("HAPPY CO., LTD.").Using(CustomComparers.TypeComparison), "ImporterName");
				NUnit.Framework.Assert.That(documentWrapper.ImporterChineseName, NUnit.Framework.Is.EqualTo("快樂股份有限公司").Using(CustomComparers.TypeComparison), "ImporterChineseName");
				NUnit.Framework.Assert.That(documentWrapper.ImporterAddressLine, NUnit.Framework.Is.EqualTo("1500 HAPPY RD ORANGE DISTRICT APPLE CITY TPE 12345 TAIWAN").Using(CustomComparers.TypeComparison), "ImporterAddressLine");
				NUnit.Framework.Assert.That(documentWrapper.ImporterAddressChineseLineLine, NUnit.Framework.Is.EqualTo("90093台灣TPE臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison), "ImporterAddressChineseLineLine");
				NUnit.Framework.Assert.That(documentWrapper.ImporterTEL, NUnit.Framework.Is.EqualTo("13925568211").Using(CustomComparers.TypeComparison), "ImporterTEL");
				NUnit.Framework.Assert.That(documentWrapper.ImporterFAX, NUnit.Framework.Is.EqualTo("13925579322").Using(CustomComparers.TypeComparison), "ImporterFAX");
				NUnit.Framework.Assert.That(documentWrapper.ImporterEMAIL, NUnit.Framework.Is.EqualTo("123@456.com").Using(CustomComparers.TypeComparison), "ImporterEMAIL");
			});

			header.TW1_CertificateType = CertificateTypeList.Codes.Code9;
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(documentWrapper.ImporterInfos, NUnit.Framework.Is.EqualTo("HAPPY CO., LTD.\r\n1500 HAPPY RD ORANGE DISTRICT APPLE CITY TPE 12345 TAIWAN").Using(CustomComparers.TypeComparison), "ImporterInfos");

			header.TW1_CertificateType = CertificateTypeList.Codes.Code11;
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(documentWrapper.ImporterInfos, NUnit.Framework.Is.EqualTo("HAPPY CO., LTD.\r\n1500 HAPPY RD ORANGE DISTRICT APPLE CITY TPE 12345 TAIWAN").Using(CustomComparers.TypeComparison), "ImporterInfos for Code11");

			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(documentWrapper.ImporterInfos, NUnit.Framework.Is.EqualTo("HAPPY CO., LTD.\r\n快樂股份有限公司\r\n1500 HAPPY RD ORANGE DISTRICT APPLE CITY TPE 12345 TAIWAN\r\nTPE臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison), "ImporterInfos for Code15");

			header.TW1_CertificateType = CertificateTypeList.Codes.Code18;
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(documentWrapper.ImporterInfos, NUnit.Framework.Is.EqualTo("HAPPY CO., LTD.\r\n快樂股份有限公司\r\n1500 HAPPY RD ORANGE DISTRICT APPLE CITY TPE 12345 TAIWAN\r\n90093台灣TPE臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison), "ImporterInfos for Code18");

			header.TW1_CertificateType = CertificateTypeList.Codes.Code19;
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(documentWrapper.ImporterInfos, NUnit.Framework.Is.EqualTo("HAPPY CO., LTD.\r\n快樂股份有限公司\r\n1500 HAPPY RD ORANGE DISTRICT APPLE CITY TPE 12345 TAIWAN\r\n90093台灣TPE臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison), "ImporterInfos for Code19");
		}

		[ExpectNoExceptions]
		public void TestManufacturerInfos()
		{
			header.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			var org = new TestTWCreator(Factory).CreateOrganization();
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.PassportID, "PAS001", Core.Constants.CountryCodes.Taiwan);
			var localProcessorAddress = header.LocalProcessorAddress;
			localProcessorAddress.OrganisationPK = org.PK;

			localProcessorAddress.E2_AddressOverride = true;
			var localProcessorTranslatedDocumentaryAddress = localProcessorAddress.LocalAddress;
			localProcessorTranslatedDocumentaryAddress.E2_AddressType = "STA";
			localProcessorTranslatedDocumentaryAddress.E2_ParentTableCode = "TW1";
			localProcessorTranslatedDocumentaryAddress.CompanyName = "快樂股份有限公司";

			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			CombineAssertions("Certificate Type equals 01", () =>
			{
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerInfos, NUnit.Framework.Is.EqualTo("HAPPY CO., LTD.\r\n快樂股份有限公司\r\n1500 HAPPY RD ORANGE DISTRICT APPLE CITY 12345 TAIWAN\r\n90093臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison), "ManufacturerInfos");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerTEL, NUnit.Framework.Is.EqualTo("13925568211").Using(CustomComparers.TypeComparison), "ManufacturerTEL");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerFAX, NUnit.Framework.Is.EqualTo("13925579322").Using(CustomComparers.TypeComparison), "ManufacturerFAX");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerEMAIL, NUnit.Framework.Is.EqualTo("123@456.com").Using(CustomComparers.TypeComparison), "ManufacturerEmail");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerTypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison), "ManufacturerTypeCode");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerID, NUnit.Framework.Is.EqualTo("PAS001").Using(CustomComparers.TypeComparison), "ManufacturerID");
			});

			header.TW1_CertificateType = CertificateTypeList.Codes.Code10;
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			CombineAssertions("Certificate Type equals 10", () =>
			{
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerInfos, NUnit.Framework.Is.EqualTo("HAPPY CO., LTD.\r\n快樂股份有限公司\r\n1500 HAPPY RD ORANGE DISTRICT APPLE CITY 12345 TAIWAN\r\n90093臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison), "ManufacturerInfos");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerTEL, NUnit.Framework.Is.EqualTo("13925568211").Using(CustomComparers.TypeComparison), "ManufacturerTEL");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerFAX, NUnit.Framework.Is.EqualTo("13925579322").Using(CustomComparers.TypeComparison), "ManufacturerFAX");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerEMAIL, NUnit.Framework.Is.EqualTo("123@456.com").Using(CustomComparers.TypeComparison), "ManufacturerEmail");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerTypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison), "ManufacturerTypeCode");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerID, NUnit.Framework.Is.EqualTo("PAS001").Using(CustomComparers.TypeComparison), "ManufacturerID");
			});

			header.TW1_CertificateType = CertificateTypeList.Codes.Code7;
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			CombineAssertions("Certificate Type equals 07", () =>
			{
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerInfos, NUnit.Framework.Is.EqualTo("Available upon request of competent authority (主管機關要求時提供)").Using(CustomComparers.TypeComparison), "ManufacturerInfos");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerTEL, NUnit.Framework.Is.EqualTo(ZString.Empty), "ManufacturerTEL");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerFAX, NUnit.Framework.Is.EqualTo(ZString.Empty), "ManufacturerFAX");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerEMAIL, NUnit.Framework.Is.EqualTo("123@456.com").Using(CustomComparers.TypeComparison), "ManufacturerEmail");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerTypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison), "ManufacturerTypeCode");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerID, NUnit.Framework.Is.EqualTo("PAS001").Using(CustomComparers.TypeComparison), "ManufacturerID");
			});

			header.TW1_CertificateType = CertificateTypeList.Codes.Code16;
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			CombineAssertions("Certificate Type equals 16", () =>
			{
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerInfos, NUnit.Framework.Is.EqualTo("快樂股份有限公司\r\n90093臺北巿臺北加工出口區園東街6號\r\nTEL:13925568211\r\nFAX:13925579322\r\nEMAIL:123@456.com").Using(CustomComparers.TypeComparison), "ManufacturerInfos");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerTEL, NUnit.Framework.Is.EqualTo("13925568211").Using(CustomComparers.TypeComparison), "ManufacturerTEL");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerFAX, NUnit.Framework.Is.EqualTo("13925579322").Using(CustomComparers.TypeComparison), "ManufacturerFAX");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerEMAIL, NUnit.Framework.Is.EqualTo("123@456.com").Using(CustomComparers.TypeComparison), "ManufacturerEmail");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerTypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison), "ManufacturerTypeCode");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerID, NUnit.Framework.Is.EqualTo("PAS001").Using(CustomComparers.TypeComparison), "ManufacturerID");
			});

			header.TW1_CertificateType = CertificateTypeList.Codes.Code11;
			CombineAssertions("Certificate Type equals 11", () =>
			{
				header.TW1_ManufacturerPrintingCode = CPT_123_ManufacturerPrintingCodeList.Codes._1;
				documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerInfos, NUnit.Framework.Is.EqualTo("Available upon request of competent authority (主管機關要求時提供)").Using(CustomComparers.TypeComparison), "ManufacturerInfos for ManufacturerPrintingCode is 1");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerTEL, NUnit.Framework.Is.EqualTo(ZString.Empty), "ManufacturerInfos for ManufacturerPrintingCode is 1 [Tel]");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerFAX, NUnit.Framework.Is.EqualTo(ZString.Empty), "ManufacturerInfos for ManufacturerPrintingCode is 1 [Fax]");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerEMAIL, NUnit.Framework.Is.EqualTo(ZString.Empty), "ManufacturerInfos for ManufacturerPrintingCode is 1 [Email]");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerID, NUnit.Framework.Is.EqualTo(ZString.Empty), "ManufacturerInfos for ManufacturerPrintingCode is 1 [ID]");

				header.TW1_ManufacturerPrintingCode = CPT_123_ManufacturerPrintingCodeList.Codes._2;
				documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerInfos, NUnit.Framework.Is.EqualTo("Same (相同)").Using(CustomComparers.TypeComparison), "ManufacturerInfos for ManufacturerPrintingCode is 2");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerTEL, NUnit.Framework.Is.EqualTo(ZString.Empty), "ManufacturerInfos for ManufacturerPrintingCode is 2 [Tel]");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerFAX, NUnit.Framework.Is.EqualTo(ZString.Empty), "ManufacturerInfos for ManufacturerPrintingCode is 2 [Fax]");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerEMAIL, NUnit.Framework.Is.EqualTo(ZString.Empty), "ManufacturerInfos for ManufacturerPrintingCode is 2 [Email]");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerID, NUnit.Framework.Is.EqualTo(ZString.Empty), "ManufacturerInfos for ManufacturerPrintingCode is 2 [ID]");

				header.TW1_ManufacturerPrintingCode = CPT_123_ManufacturerPrintingCodeList.Codes._3;
				documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerInfos, NUnit.Framework.Is.EqualTo("快樂股份有限公司\r\nHAPPY CO., LTD.\r\n1500 HAPPY RD ORANGE DISTRICT APPLE CITY 12345 TAIWAN\r\n90093臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison), "ManufacturerInfos for ManufacturerPrintingCode is 3");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerTEL, NUnit.Framework.Is.EqualTo("13925568211").Using(CustomComparers.TypeComparison), "ManufacturerInfos for ManufacturerPrintingCode is 3 [Tel]");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerFAX, NUnit.Framework.Is.EqualTo("13925579322").Using(CustomComparers.TypeComparison), "ManufacturerInfos for ManufacturerPrintingCode is 3 [Fax]");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerEMAIL, NUnit.Framework.Is.EqualTo("123@456.com").Using(CustomComparers.TypeComparison), "ManufacturerInfos for ManufacturerPrintingCode is 3 [Email]");
				NUnit.Framework.Assert.That(documentWrapper.ManufacturerID, NUnit.Framework.Is.EqualTo("PAS001").Using(CustomComparers.TypeComparison), "ManufacturerInfos for ManufacturerPrintingCode is 3 [ID]");
			});
		}

		[ExpectNoExceptions]
		public void TestPortofLoading()
		{
			var uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_Code = "TWXXX";
			uNLOCO.RL_PortName = "TAIWANG";

			declaration.JE_RL_NKOrigin = "TWXXX";
			declaration.JE_ExportDate = new ZDateTime(2023, 5, 5);
			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			header.TW1_IsEstimatedLoadingDate = true;

			header.TW1_PortOfLoadingName = "Origin Port";
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(documentWrapper.PortofLoading, NUnit.Framework.Is.EqualTo("Origin Port").Using(CustomComparers.TypeComparison));

			header.TW1_PortOfLoadingName = "Z99 Port";
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(documentWrapper.PortofLoading, NUnit.Framework.Is.EqualTo("Z99 Port").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPortofDischarge()
		{
			header.TW1_RL_NKPortOfUnloading = "Y";
			header.TW1_PortOfUnloadingName = "台灣";
			NUnit.Framework.Assert.That(documentWrapper.PortofDischarge, NUnit.Framework.Is.EqualTo("台灣").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCountryofDestination()
		{
			var uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_Code = "XX123";
			uNLOCO.RL_PortName = "TAIWANG";
			uNLOCO.RL_RN_NKCountryCode = "TW";

			NUnit.Framework.Assert.That(documentWrapper.CountryofDestination, NUnit.Framework.Is.EqualTo(ZString.Empty), "CountryofDestination");
			declaration.JE_RL_NKFinalDestination = "XX123";
			NUnit.Framework.Assert.That(documentWrapper.CountryofDestination, NUnit.Framework.Is.EqualTo("Taiwan").Using(CustomComparers.TypeComparison), "CountryofDestination");
			declaration.JE_RL_NKFinalDestination = "XX234";
			NUnit.Framework.Assert.That(documentWrapper.CountryofDestination, NUnit.Framework.Is.EqualTo(ZString.Empty), "CountryofDestination");
			declaration.JE_RL_NKFinalDestination = "ADXXX";
			NUnit.Framework.Assert.That(documentWrapper.CountryofDestination, NUnit.Framework.Is.EqualTo("Andorra").Using(CustomComparers.TypeComparison), "CountryofDestination");
		}

		[ExpectNoExceptions]
		public void TestObservations()
		{
			header.TW1_Observations = "Ob";
			NUnit.Framework.Assert.That(documentWrapper.Observations, NUnit.Framework.Is.EqualTo("Ob").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestQuantityAndUnitSummary()
		{
			header.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			var invoiceLines = declaration.Invoices.AddNew().InvoiceLines;
			var invoiceLine1 = (JobComInvoiceLine)invoiceLines.AddNew();
			invoiceLine1.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			invoiceLine1.JI_PermitQty = 15m;
			invoiceLine1.JI_CustomPermitUQ = "TNE";

			var invoiceLine2 = (JobComInvoiceLine)invoiceLines.AddNew();
			invoiceLine2.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			invoiceLine2.JI_PermitQty = 16m;
			invoiceLine2.JI_CustomPermitUQ = "KG";

			var invoiceLine3 = (JobComInvoiceLine)invoiceLines.AddNew();
			invoiceLine3.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			invoiceLine3.JI_PermitQty = 1000m;
			invoiceLine3.JI_CustomPermitUQ = "TNE";

			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(documentWrapper.QuantityAndUnitSummary, NUnit.Framework.Is.EqualTo("____________\r\n1,015 TNE\r\n16 KG\r\nVVVVVVVVVV").Using(CustomComparers.TypeComparison), "no decimal");

			invoiceLine1.JI_PermitQty = 15.5m;
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(documentWrapper.QuantityAndUnitSummary, NUnit.Framework.Is.EqualTo("____________\r\n1,015.5 TNE\r\n16.0 KG\r\nVVVVVVVVVV").Using(CustomComparers.TypeComparison), "1 decimal place");
		}

		[ExpectNoExceptions]
		public void TestQuantityAndUnitSummaryWhenNoElementsException()
		{
			header.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			var invoiceLines = declaration.Invoices.AddNew().InvoiceLines;
			var invoiceLine = (JobComInvoiceLine)invoiceLines.AddNew();
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(documentWrapper.QuantityAndUnitSummary, NUnit.Framework.Is.EqualTo("____________\r\n\r\nVVVVVVVVVV").Using(CustomComparers.TypeComparison), "no exception");
		}

		[ExpectNoExceptions]
		public void TestTradersRemarks()
		{
			declaration.JE_TotalNoOfPacks = 1;
			declaration.JE_TotalNoOfPacksPackType = Core.Constants.PkgUnit.Piece;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(documentWrapper.TradersRemarks, NUnit.Framework.Is.EqualTo("SAY TOTAL ONE (1) PCE ONLY").Using(CustomComparers.TypeComparison));
				declaration.JE_TotalNoOfPacks = 9999;
				declaration.JE_TotalNoOfPacksPackType = Core.Constants.PkgUnit.Box;
				documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
				NUnit.Framework.Assert.That(documentWrapper.TradersRemarks, NUnit.Framework.Is.EqualTo("SAY TOTAL NINE THOUSAND, NINE HUNDRED AND NINETY NINE (9999) BOXES ONLY").Using(CustomComparers.TypeComparison));

				header.TW1_Remarks = "header remark";
				documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
				NUnit.Framework.Assert.That(documentWrapper.TradersRemarks, NUnit.Framework.Is.EqualTo("header remark\r\nSAY TOTAL NINE THOUSAND, NINE HUNDRED AND NINETY NINE (9999) BOXES ONLY").Using(CustomComparers.TypeComparison));

				header.TW1_Remarks = ZString.Replicate('A', 1024);
				var a256 = ZString.Replicate('A', 256);
				documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
				NUnit.Framework.Assert.That(documentWrapper.TradersRemarks, NUnit.Framework.Is.EqualTo($"{a256}\r\n{a256}\r\n{a256}\r\n{a256}").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestGoodsInformation()
		{
			declaration.JE_TotalNoOfPacks = 1;
			declaration.JE_TotalNoOfPacksPackType = Core.Constants.PkgUnit.Piece;
			NUnit.Framework.Assert.That(documentWrapper.GoodsInformation, NUnit.Framework.Is.EqualTo($"{documentWrapper.TradersRemarks}.\r\n☆☆☆").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCountrysofOrigin()
		{
			var invoiceLines = declaration.Invoices.AddNew().InvoiceLines;
			var invoiceLine1 = (JobComInvoiceLine)invoiceLines.AddNew();
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Austria;
			invoiceLine1.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;

			var invoiceLine2 = (JobComInvoiceLine)invoiceLines.AddNew();
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Belgium;
			invoiceLine2.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;

			var invoiceLine3 = (JobComInvoiceLine)invoiceLines.AddNew();
			invoiceLine3.JI_CountryOfOrigin = Core.Constants.CountryCodes.Belgium;
			invoiceLine3.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;

			header.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			NUnit.Framework.Assert.That(documentWrapper.CountrysofOrigin, NUnit.Framework.Is.EqualTo(ZString.Empty), "Only output when CertificateType is '08', '16', '17'");

			var certificateTypes = new[] { CertificateTypeList.Codes.Code8, CertificateTypeList.Codes.Code16, CertificateTypeList.Codes.Code17 };
			foreach (var certificateType in certificateTypes)
			{
				header.TW1_CertificateType = certificateType;
				NUnit.Framework.Assert.That(documentWrapper.CountrysofOrigin, NUnit.Framework.Is.EqualTo("Austria/Belgium").Using(CustomComparers.TypeComparison));
			}
		}

		[ExpectNoExceptions]
		public void TestDocumentIDs()
		{
			var certificateOfOrigins = header.CertificateOfOrigins;
			certificateOfOrigins.AddNew().CSI_ReferenceNumber = "B0001";
			certificateOfOrigins.AddNew().CSI_ReferenceNumber = "B0002";

			var previousDocumentNumbers = header.PreviousDocumentNumbers;
			previousDocumentNumbers.AddNew().CSI_ReferenceNumber = "C0001";
			previousDocumentNumbers.AddNew().CSI_ReferenceNumber = "C0002";
			var documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(documentWrapper.AdditionalDocumentIDs, NUnit.Framework.Is.EqualTo("B0001/B0002").Using(CustomComparers.TypeComparison), "AdditionalDocumentIDs");
				NUnit.Framework.Assert.That(documentWrapper.PreviousDocumentIDs, NUnit.Framework.Is.EqualTo("C0001/C0002").Using(CustomComparers.TypeComparison), "PreviousDocumentIDs");
				NUnit.Framework.Assert.That(documentWrapper.DocumentIDs, NUnit.Framework.Is.EqualTo("B0001/B0002/C0001/C0002").Using(CustomComparers.TypeComparison), "DocumentIDs");
			});
		}

		[ExpectNoExceptions]
		public void TestCPT_127_GoodsReleaseReasonCodeList()
		{
			header.TW1_BeforeClearanceApplicationReason = CPT_127_GoodsReleaseReasonCodeList.Codes._01;
			NUnit.Framework.Assert.That(documentWrapper.GoodsReleaseReason, NUnit.Framework.Is.EqualTo($"{CPT_127_GoodsReleaseReasonCodeList.Codes._01}{CPT_127_GoodsReleaseReasonCodeList.Descriptions._01}").Using(CustomComparers.TypeComparison));

			header.TW1_BeforeClearanceApplicationReason = CPT_127_GoodsReleaseReasonCodeList.Codes._02;
			NUnit.Framework.Assert.That(documentWrapper.GoodsReleaseReason, NUnit.Framework.Is.EqualTo($"{CPT_127_GoodsReleaseReasonCodeList.Codes._02}{CPT_127_GoodsReleaseReasonCodeList.Descriptions._02}").Using(CustomComparers.TypeComparison));

			header.TW1_BeforeClearanceApplicationReason = CPT_127_GoodsReleaseReasonCodeList.Codes._03;
			NUnit.Framework.Assert.That(documentWrapper.GoodsReleaseReason, NUnit.Framework.Is.EqualTo($"{CPT_127_GoodsReleaseReasonCodeList.Codes._03}{CPT_127_GoodsReleaseReasonCodeList.Descriptions._03}").Using(CustomComparers.TypeComparison));

			header.TW1_BeforeClearanceApplicationReason = CPT_127_GoodsReleaseReasonCodeList.Codes._04;
			NUnit.Framework.Assert.That(documentWrapper.GoodsReleaseReason, NUnit.Framework.Is.EqualTo($"{CPT_127_GoodsReleaseReasonCodeList.Codes._04}{CPT_127_GoodsReleaseReasonCodeList.Descriptions._04}").Using(CustomComparers.TypeComparison));

			header.TW1_BeforeClearanceApplicationReason = CPT_127_GoodsReleaseReasonCodeList.Codes._05;
			NUnit.Framework.Assert.That(documentWrapper.GoodsReleaseReason, NUnit.Framework.Is.EqualTo($"{CPT_127_GoodsReleaseReasonCodeList.Codes._05}{CPT_127_GoodsReleaseReasonCodeList.Descriptions._05}").Using(CustomComparers.TypeComparison));

			header.TW1_BeforeClearanceApplicationReason = CPT_127_GoodsReleaseReasonCodeList.Codes._06;
			NUnit.Framework.Assert.That(documentWrapper.GoodsReleaseReason, NUnit.Framework.Is.EqualTo($"{CPT_127_GoodsReleaseReasonCodeList.Codes._06}{CPT_127_GoodsReleaseReasonCodeList.Descriptions._06}").Using(CustomComparers.TypeComparison));

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "CABF0945600030";
			entryHeader.CusEntryNumber.CE_EntryStatus = "C1";
			NUnit.Framework.Assert.That(documentWrapper.GoodsReleaseReason, NUnit.Framework.Is.EqualTo("N/A").Using(CustomComparers.TypeComparison), "When GoodsReleaseCode is N");
		}

		[ExpectNoExceptions]
		public void TestTriangularTrade()
		{
			header.TW1_IsTriangularTrade = true;
			NUnit.Framework.Assert.That(documentWrapper.TriangularTrade, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison));

			header.TW1_IsTriangularTrade = false;
			NUnit.Framework.Assert.That(documentWrapper.TriangularTrade, NUnit.Framework.Is.EqualTo("N").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestContainerNumber()
		{
			NUnit.Framework.Assert.That(documentWrapper.ContainerNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			var cusContainers = declaration.CusContainers;
			cusContainers.AddNew().CO_ContainerNumber = "CRXU1234569";
			NUnit.Framework.Assert.That(documentWrapper.ContainerNumber, NUnit.Framework.Is.EqualTo("CRXU1234569").Using(CustomComparers.TypeComparison));

			cusContainers.AddNew().CO_ContainerNumber = "DRXU1234569";
			NUnit.Framework.Assert.That(documentWrapper.ContainerNumber, NUnit.Framework.Is.EqualTo("CRXU1234569/DRXU1234569").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestNotes()
		{
			header.TW_Notes = "TEST NOTES";
			NUnit.Framework.Assert.That(documentWrapper.Notes, NUnit.Framework.Is.EqualTo("TEST NOTES").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSetMarksNumbers()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			invoice.TW_MarksAndNumbers = "1\r\n2\r\n3\r\n4\r\n5\r\n6\r\n7\r\n8\r\n9\r\n10\r\n11\r\n12\r\n13\r\n14\r\n15\r\n16\r\n17\r\n18";
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.AssignCMHeaderToInvoices(header);
			invoiceLine.JI_Group = "Grouping";
			invoiceLine.JI_Description = "english";
			invoiceLine.JI_NDescription = "中文";
			invoiceLine.JI_Model = "Model";
			invoiceLine.JI_BrandName = "Brand";
			invoiceLine.JI_Tariff = "98990000006";
			invoiceLine.JI_TariffPrintLength = TariffPrintLengthList.Codes.THREE;
			invoiceLine.JI_PermitQty = 15m;
			invoiceLine.JI_CustomPermitUQ = "TNE";
			invoiceLine.JI_InnerPackDescription = "Pack Description";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			var sectionBodydocumentWrapper = documentWrapper.InvoiceLines.Cast<NX101ControllingMessageHeaderSectionBodyDocumentWrapper>().FirstOrDefault();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(documentWrapper.MarksNumbers, NUnit.Framework.Is.EqualTo("1\r\n2\r\n3\r\n4\r\n5\r\n6\r\n7\r\n8\r\n9\r\n10\r\n11\r\n12\r\n13\r\n14\r\n15\r\n16\r\n17\r\n18").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sectionBodydocumentWrapper.AlignGroupingMarksNumbers, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison), "InvoiceLines.AlignGroupingMarksNumbers");
				NUnit.Framework.Assert.That(sectionBodydocumentWrapper.AlignTariffformatMarksNumbers, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison), "InvoiceLines.AlignTariffformatMarksNumbers");
				NUnit.Framework.Assert.That(sectionBodydocumentWrapper.AlignGoodsDescriptionMarksNumbers, NUnit.Framework.Is.EqualTo("3\r\n4").Using(CustomComparers.TypeComparison), "InvoiceLines.AlignGoodsDescriptionMarksNumbers");
				NUnit.Framework.Assert.That(sectionBodydocumentWrapper.AlignBrandMarksNumbers, NUnit.Framework.Is.EqualTo("5").Using(CustomComparers.TypeComparison), "InvoiceLines.AlignBrandMarksNumbers");
				NUnit.Framework.Assert.That(sectionBodydocumentWrapper.AlignModelMarksNumbers, NUnit.Framework.Is.EqualTo("6").Using(CustomComparers.TypeComparison), "InvoiceLines.AlignModelMarksNumbers");
				NUnit.Framework.Assert.That(sectionBodydocumentWrapper.AlignSpecificationMarksNumbers, NUnit.Framework.Is.EqualTo("7").Using(CustomComparers.TypeComparison), "InvoiceLines.AlignSpecificationMarksNumbers");
				NUnit.Framework.Assert.That(sectionBodydocumentWrapper.AlignShippingMarksMarksNumbers, NUnit.Framework.Is.EqualTo(ZString.Empty), "InvoiceLines.AlignShippingMarksMarksNumbers");
				NUnit.Framework.Assert.That(documentWrapper.AlignQuantityAndUnitSummaryMarksNumbers, NUnit.Framework.Is.EqualTo("8\r\n9\r\n10").Using(CustomComparers.TypeComparison), "AlignQuantityAndUnitSummaryMarksNumbers");
				NUnit.Framework.Assert.That(documentWrapper.AlignTradersRemarksMarksNumbers, NUnit.Framework.Is.EqualTo("11").Using(CustomComparers.TypeComparison), "AlignTradersRemarksMarksNumbers");
				NUnit.Framework.Assert.That(documentWrapper.LastMarksNumbers, NUnit.Framework.Is.EqualTo("12\r\n13\r\n14\r\n15\r\n16\r\n17\r\n18").Using(CustomComparers.TypeComparison), "LastMarksNumbers");
			});

			invoiceLine.JI_Group = ZString.Empty;
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			sectionBodydocumentWrapper = documentWrapper.InvoiceLines.Cast<NX101ControllingMessageHeaderSectionBodyDocumentWrapper>().FirstOrDefault();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sectionBodydocumentWrapper.AlignGroupingMarksNumbers, NUnit.Framework.Is.EqualTo(ZString.Empty), "InvoiceLines.AlignGroupingMarksNumbers");
				NUnit.Framework.Assert.That(sectionBodydocumentWrapper.AlignTariffformatMarksNumbers, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison), "InvoiceLines.AlignTariffformatMarksNumbers");
				NUnit.Framework.Assert.That(sectionBodydocumentWrapper.AlignGoodsDescriptionMarksNumbers, NUnit.Framework.Is.EqualTo("2\r\n3").Using(CustomComparers.TypeComparison), "InvoiceLines.AlignGoodsDescriptionMarksNumbers");
				NUnit.Framework.Assert.That(sectionBodydocumentWrapper.AlignBrandMarksNumbers, NUnit.Framework.Is.EqualTo("4").Using(CustomComparers.TypeComparison), "InvoiceLines.AlignBrandMarksNumbers");
				NUnit.Framework.Assert.That(sectionBodydocumentWrapper.AlignModelMarksNumbers, NUnit.Framework.Is.EqualTo("5").Using(CustomComparers.TypeComparison), "InvoiceLines.AlignModelMarksNumbers");
				NUnit.Framework.Assert.That(sectionBodydocumentWrapper.AlignSpecificationMarksNumbers, NUnit.Framework.Is.EqualTo("6").Using(CustomComparers.TypeComparison), "InvoiceLines.AlignSpecificationMarksNumbers");
				NUnit.Framework.Assert.That(sectionBodydocumentWrapper.AlignShippingMarksMarksNumbers, NUnit.Framework.Is.EqualTo(ZString.Empty), "InvoiceLines.AlignShippingMarksMarksNumbers");
				NUnit.Framework.Assert.That(documentWrapper.AlignQuantityAndUnitSummaryMarksNumbers, NUnit.Framework.Is.EqualTo("7\r\n8\r\n9").Using(CustomComparers.TypeComparison), "AlignQuantityAndUnitSummaryMarksNumbers");
				NUnit.Framework.Assert.That(documentWrapper.AlignTradersRemarksMarksNumbers, NUnit.Framework.Is.EqualTo("10").Using(CustomComparers.TypeComparison), "AlignTradersRemarksMarksNumbers");
				NUnit.Framework.Assert.That(documentWrapper.LastMarksNumbers, NUnit.Framework.Is.EqualTo("11\r\n12\r\n13\r\n14\r\n15\r\n16\r\n17\r\n18").Using(CustomComparers.TypeComparison), "LastMarksNumbers");
			});
		}

		[ExpectNoExceptions]
		public void TestMarksNumbersLengthOfALine()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			invoice.TW_MarksAndNumbers = "BOSS AA BB CC DD EE FF GG HH II JJ";
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.AssignCMHeaderToInvoices(header);
			invoiceLine.JI_Group = "Grouping";
			invoiceLine.JI_Tariff = "98990000006";
			invoiceLine.JI_TariffPrintLength = TariffPrintLengthList.Codes.THREE;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			var sectionBodydocumentWrapper = documentWrapper.InvoiceLines.Cast<NX101ControllingMessageHeaderSectionBodyDocumentWrapper>().FirstOrDefault();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sectionBodydocumentWrapper.AlignGroupingMarksNumbers, NUnit.Framework.Is.EqualTo("BOSS AA BB CC DD EE FF").Using(CustomComparers.TypeComparison), "InvoiceLines.AlignGroupingMarksNumbers");
				NUnit.Framework.Assert.That(sectionBodydocumentWrapper.AlignTariffformatMarksNumbers, NUnit.Framework.Is.EqualTo("GG HH II JJ").Using(CustomComparers.TypeComparison), "InvoiceLines.AlignTariffformatMarksNumbers");
			});
		}

		[ExpectNoExceptions]
		public void TestContactOfficeDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage(Core.SharedConstants.Languages.English, "English");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanCertificateOfOriginIssuingUnit, "CertificateOfOriginIssuingUnit");
			var processingUnit1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanCertificateOfOriginIssuingUnit, "XXXX0123", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(processingUnit1.PK, RefCusCodeListAttributeTypes.Codes.Type, CertificateTypeList.Codes.Code1);
			helper.CreateCusCodeListLanguage(processingUnit1, Core.SharedConstants.Languages.English, "XXXXXX EN");
			var processingUnit2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanCertificateOfOriginIssuingUnit, "XXXX4567", "ZZZZZZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(processingUnit2.PK, RefCusCodeListAttributeTypes.Codes.Type, CertificateTypeList.Codes.Code8);
			helper.CreateCusCodeListLanguage(processingUnit2, Core.SharedConstants.Languages.English, "ZZZZZZ EN");
			Factory.Save();

			header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			header.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			header.TW1_ProcessingUnit = "XXXX0123";
			NUnit.Framework.Assert.That(documentWrapper.ContactOfficeDescription, NUnit.Framework.Is.EqualTo("XXXXXX EN").Using(CustomComparers.TypeComparison));
			header.TW1_CertificateType = CertificateTypeList.Codes.Code8;
			header.TW1_ProcessingUnit = "XXXX4567";
			NUnit.Framework.Assert.That(header.ProcessingUnitDescription, NUnit.Framework.Is.EqualTo("ZZZZZZ EN").Using(CustomComparers.TypeComparison));
			header.TW1_ProcessingUnit = "4567";
			NUnit.Framework.Assert.That(header.ProcessingUnitDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestContactOfficeChineseDescription()
		{
			GlbStaff.CurrentUser[ZArchitecture.Schema.GlbStaffSchema.GS_WorkingLanguage] = Core.SharedConstants.Languages.English;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage(Core.SharedConstants.Languages.English, "English");
			helper.CreateNewOrGetExistingCusCodeType(Codes.TaiwanCertificateOfOriginIssuingUnit, "Certificate of Origin Issuing Unit", Core.Constants.CountryCodes.Taiwan);
			var processingUnit1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TaiwanCertificateOfOriginIssuingUnit, "BA", "台灣花卉輸出業同業公會", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(processingUnit1.PK, RefCusCodeListAttributeTypes.Codes.Type, CertificateTypeList.Codes.Code15);
			helper.CreateCusCodeListLanguage(processingUnit1, Core.SharedConstants.Languages.English, "TAIWAN FLORICULTURE EXPORTS ASSOCIATION");
			Factory.Save();

			header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			header.TW1_ProcessingUnit = "BA";
			NUnit.Framework.Assert.That(documentWrapper.ContactOfficeChineseDescription, NUnit.Framework.Is.EqualTo("台灣花卉輸出業同業公會").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDepartureId()
		{
			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			header.TW1_RL_NKPortOfLoading = "Y";
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(documentWrapper.DepartureId, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPortofLoadingChineseDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Codes.ECFALoadingPort, "ECFA Loading Port", Core.Constants.CountryCodes.Taiwan);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.ECFALoadingPort, "TWHUN", "花蓮", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			header.TW1_RL_NKPortOfLoading = "TWHUN";
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(documentWrapper.PortofLoadingChineseDescription, NUnit.Framework.Is.EqualTo("花蓮").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPortofDischargeChineseDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Codes.ECFAUnloadingPort, "ECFA Unloading Port", Core.Constants.CountryCodes.Taiwan);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.ECFAUnloadingPort, "CNWSN", "重慶巫山", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			header.TW1_RL_NKPortOfUnloading = "CNWSN";
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(documentWrapper.PortofDischargeChineseDescription, NUnit.Framework.Is.EqualTo("重慶巫山").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDepartureDate()
		{
			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			declaration.JE_RL_NKOrigin = "Y";
			declaration.JE_ExportDate = new ZDateTime(2023, 12, 1);
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(documentWrapper.DepartureDate, NUnit.Framework.Is.EqualTo(declaration.JE_ExportDate));
		}

		[ExpectNoExceptions]
		public void TestEstimatedLoadingCode()
		{
			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			header.TW1_IsEstimatedLoadingDate = true;
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(documentWrapper.EstimatedLoadingCode, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison));
			header.TW1_IsEstimatedLoadingDate = false;
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(documentWrapper.EstimatedLoadingCode, NUnit.Framework.Is.EqualTo("N").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestManufacturerPrintingCode()
		{
			header.TW1_ManufacturerPrintingCode = CPT_123_15_ManufacturerPrintingCodeList.Codes._1;
			NUnit.Framework.Assert.That(documentWrapper.ManufacturerPrintingCode, NUnit.Framework.Is.EqualTo(CPT_123_15_ManufacturerPrintingCodeList.Codes._1).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestVessel()
		{
			declaration.JE_VesselName = "AB123";
			declaration.JE_VoyageFlightNo = "CD456";
			NUnit.Framework.Assert.That(documentWrapper.Vessel, NUnit.Framework.Is.EqualTo(" CD456").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestECFANote()
		{
			header.TW1_ECFAPrintedRemarks = "ECFA Printed Remarks";
			NUnit.Framework.Assert.That(documentWrapper.ECFANote, NUnit.Framework.Is.EqualTo("ECFA Printed Remarks").Using(CustomComparers.TypeComparison));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			return new NX101ControllingMessageHeaderDocumentWrapper(header);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			documentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader header;
		NX101ControllingMessageHeaderDocumentWrapper documentWrapper;
	}
}
