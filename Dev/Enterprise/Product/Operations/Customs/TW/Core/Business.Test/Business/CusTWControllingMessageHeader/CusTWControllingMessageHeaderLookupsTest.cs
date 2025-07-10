using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusTWControllingMessageHeaderLookups))]
	sealed class CusTWControllingMessageHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestControllingMessageTypeList()
		{
			declartion.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			NUnit.Framework.Assert.That(lookups.ControllingMessageTypeList.CodesAsString, NUnit.Framework.Is.EqualTo("NX101, X101, NX201_01, NX401"));

			declartion.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			NUnit.Framework.Assert.That(lookups.ControllingMessageTypeList.CodesAsString, NUnit.Framework.Is.EqualTo("NX201_01, NX201_07, NX301, NX301_AX, NX301_DN, NX401, NX601, NX603"));
		}

		[ExpectNoExceptions]
		public void TestPortList()
		{
			NUnit.Framework.Assert.That(lookups.PortList, Is.TypeOf<RefUNLOCOCollection>());
		}

		[ExpectNoExceptions]
		public void TestBusinessTypeList()
		{
			var header = cusTWControllingMessageHeader;
			var messageTypes = header.Lookups.ControllingMessageTypeList.GetAllCodes();
			CombineAssertions(() =>
			{
				foreach (var messageType in messageTypes)
				{
					cusTWControllingMessageHeader.TW1_ControllingMessageType = messageType;
					var businessTypeList = lookups.BusinessTypeList;
					switch (header.TW1_ControllingMessageType)
					{
						case ControllingMessageTypeList.Codes.NX201_01:
							declartion.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
							businessTypeList = lookups.BusinessTypeList;
							NUnit.Framework.Assert.That(businessTypeList.Count, NUnit.Framework.Is.EqualTo(3), $"NX201_01 [Import]: The count of BusinessTypeList is 3");
							NUnit.Framework.Assert.That(businessTypeList.CodesAsString, NUnit.Framework.Is.EqualTo("0, 2, 3"), $"NX201_01 [Import]: CodesAsString");
							declartion.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
							businessTypeList = lookups.BusinessTypeList;
							NUnit.Framework.Assert.That(businessTypeList.Count, NUnit.Framework.Is.EqualTo(2), $"NX201_01 [Export]: The count of BusinessTypeList is 2");
							NUnit.Framework.Assert.That(businessTypeList.CodesAsString, NUnit.Framework.Is.EqualTo("0, 1"), $"NX201_01 [Export]: CodesAsString");
							break;
						case ControllingMessageTypeList.Codes.NX201_07:
							businessTypeList = lookups.BusinessTypeList;
							NUnit.Framework.Assert.That(businessTypeList.Count, NUnit.Framework.Is.EqualTo(3), $"NX201_07: The count of BusinessTypeList is 3");
							NUnit.Framework.Assert.That(businessTypeList.CodesAsString, NUnit.Framework.Is.EqualTo("0, 2, 3"), $"NX201_07: CodesAsString");
							break;
						case ControllingMessageTypeList.Codes.NX301_DN:
							NUnit.Framework.Assert.That(businessTypeList.Count, NUnit.Framework.Is.EqualTo(3), "NX301_DN: The count of BusinessTypeList is 3 when Message Type");
							NUnit.Framework.Assert.That(businessTypeList.CodesAsString, NUnit.Framework.Is.EqualTo("A, B, C"), "NX301_DN: CodesAsString");
							NUnit.Framework.Assert.That(businessTypeList.ElementsAsString, NUnit.Framework.Is.EqualTo(@"A - 查驗申辦
B - 複驗申辦
C - 免驗申辦"), "NX301_DN: ElementsAsString");
							break;
						case ControllingMessageTypeList.Codes.NX401:
							declartion.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
							businessTypeList = lookups.BusinessTypeList;
							NUnit.Framework.Assert.That(businessTypeList.Count, NUnit.Framework.Is.EqualTo(2), "NX401 [Import]: The count of BusinessTypeList is 2");
							NUnit.Framework.Assert.That(businessTypeList.CodesAsString, NUnit.Framework.Is.EqualTo("40, 60"), "NX401 [Import]: CodesAsString");
							NUnit.Framework.Assert.That(businessTypeList.ElementsAsString, NUnit.Framework.Is.EqualTo(@"40 - 動物輸入檢疫申請
60 - 植物輸入檢疫申請"), "NX401 [Import]: ElementsAsString");
							declartion.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
							businessTypeList = lookups.BusinessTypeList;
							NUnit.Framework.Assert.That(businessTypeList.Count, NUnit.Framework.Is.EqualTo(2), "NX401 [Export]: The count of BusinessTypeList is 2");
							NUnit.Framework.Assert.That(businessTypeList.CodesAsString, NUnit.Framework.Is.EqualTo("30, 50"), "NX401 [Export]: CodesAsString");
							NUnit.Framework.Assert.That(businessTypeList.ElementsAsString, NUnit.Framework.Is.EqualTo(@"30 - 動物輸出檢疫申請
50 - 植物輸出檢疫申請"), "NX401 [Export]: ElementsAsString");
							declartion.JE_MessageType = ZString.Empty;
							businessTypeList = lookups.BusinessTypeList;
							NUnit.Framework.Assert.That(businessTypeList.Count, NUnit.Framework.Is.EqualTo(0), "NX401: The count of BusinessTypeList is 0 JE_MessageType is other");
							break;
						case ControllingMessageTypeList.Codes.NX603:
							NUnit.Framework.Assert.That(businessTypeList.Count, NUnit.Framework.Is.EqualTo(1), "NX603: The count of BusinessTypeList is 1");
							NUnit.Framework.Assert.That(businessTypeList.CodesAsString, NUnit.Framework.Is.EqualTo("01"), "NX603: CodesAsString");
							NUnit.Framework.Assert.That(businessTypeList.ElementsAsString, NUnit.Framework.Is.EqualTo("01 - 醫療器材"), "NX603: ElementsAsString");
							break;
						default:
							NUnit.Framework.Assert.That(businessTypeList.Count, NUnit.Framework.Is.EqualTo(0), $"{header.TW1_ControllingMessageType}: The count of BusinessTypeList should be 0");
							break;
					}
				}
			});
		}

		[ExpectNoExceptions]
		public void TestProcessingUnitList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Codes.TWReceivingUnit, "Processing Unit");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.ControlAgency, "The Control Agency to which the Processing Unit belongs", Codes.TWReceivingUnit, "TW");

			var codeList1 = helper.CreateCusCodeList("TW", Codes.TWReceivingUnit, "10", "總局", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList("TW", Codes.TWReceivingUnit, "20", "基隆分局", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeList3 = helper.CreateCusCodeList("TW", Codes.TWReceivingUnit, "H14", "機放一課", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			codeList1.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "VP");
			codeList3.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "VV");
			Factory.Save();
			var processingUnitList = lookups.ProcessingUnitList;
			NUnit.Framework.Assert.That(processingUnitList.CodesAsString, NUnit.Framework.Is.EqualTo("20"));

			cusTWControllingMessageHeader.TW1_ControllingAgency = "VP";
			processingUnitList = lookups.ProcessingUnitList;
			NUnit.Framework.Assert.That(processingUnitList.CodesAsString, NUnit.Framework.Is.EqualTo("10, 20"));

			cusTWControllingMessageHeader.TW1_ControllingAgency = "VV";
			processingUnitList = lookups.ProcessingUnitList;
			NUnit.Framework.Assert.That(processingUnitList.CodesAsString, NUnit.Framework.Is.EqualTo("20, H14"));
		}

		[ExpectNoExceptions]
		public void TestCPT_116_PaymentMethodList()
		{
			foreach (var controllingAgency in new string[] { "20", "CI", "2Q", "DN", "VP", "CD", "IF", "DH", "AA" })
			{
				cusTWControllingMessageHeader.TW1_ControllingAgency = controllingAgency;
				var paymentMethodList = lookups.CPT_116_PaymentMethodList;
				if (controllingAgency == "20" || controllingAgency == "CI" || controllingAgency == "2Q")
				{
					NUnit.Framework.Assert.That(paymentMethodList.Count, NUnit.Framework.Is.EqualTo(8), "The count of PaymentMethodList is 8 when Controlling Agency is 20, CI, or 2Q");
					NUnit.Framework.Assert.That(paymentMethodList.CodesAsString, NUnit.Framework.Is.EqualTo("1, 2, 3, 4, 5, 6, 7, 8"));
					NUnit.Framework.Assert.That(paymentMethodList.ElementsAsString, NUnit.Framework.Is.EqualTo(@"1 - 現金
2 - 銀行本票或即期支票
3 - 郵局匯票
4 - 使用擔保額度銀行帳戶扣款
5 - 匯款
6 - e政府繳費平台
7 - 轉帳
8 - 虛擬帳號繳款"));
				}
				else if (controllingAgency == "DN")
				{
					NUnit.Framework.Assert.That(paymentMethodList.Count, NUnit.Framework.Is.EqualTo(2), "The count of PaymentMethodList is 2 when Controlling Agency is DN");
					NUnit.Framework.Assert.That(paymentMethodList.CodesAsString, NUnit.Framework.Is.EqualTo("1, 2"));
					NUnit.Framework.Assert.That(paymentMethodList.ElementsAsString, NUnit.Framework.Is.EqualTo(@"1 - 銀行繳款
2 - 自動扣款"));
				}
				else if (controllingAgency == "VP")
				{
					NUnit.Framework.Assert.That(paymentMethodList.Count, NUnit.Framework.Is.EqualTo(4), "The count of PaymentMethodList is 4 when Controlling Agency is VP");
					NUnit.Framework.Assert.That(paymentMethodList.CodesAsString, NUnit.Framework.Is.EqualTo("1, 2, 3, 4"));
					NUnit.Framework.Assert.That(paymentMethodList.ElementsAsString, NUnit.Framework.Is.EqualTo(@"1 - 臨櫃
2 - 銀行轉帳
3 - 虛擬帳號
4 - 指定帳戶扣款"));
				}
				else if (controllingAgency == "CD" || controllingAgency == "IF" || controllingAgency == "DH")
				{
					NUnit.Framework.Assert.That(paymentMethodList.Count, NUnit.Framework.Is.EqualTo(7), "The count of PaymentMethodList is 7 when Controlling Agency is CD , IF or DH");
					NUnit.Framework.Assert.That(paymentMethodList.CodesAsString, NUnit.Framework.Is.EqualTo("1, 2, 21, 3, 4, 5, 6"));
					NUnit.Framework.Assert.That(paymentMethodList.ElementsAsString, NUnit.Framework.Is.EqualTo(@"1 - 現金
2 - 本地支票
21 - 外埠支票
3 - 匯票
4 - 信用擔保額度
5 - 銀行轉帳
6 - 多元繳費"));
				}
				else if (controllingAgency == "AX")
				{
					NUnit.Framework.Assert.That(paymentMethodList.Count, NUnit.Framework.Is.EqualTo(3), "The count of PaymentMethodList is 3 when Controlling Agency is AX");
					NUnit.Framework.Assert.That(paymentMethodList.CodesAsString, NUnit.Framework.Is.EqualTo("1, 2, 3"));
					NUnit.Framework.Assert.That(paymentMethodList.ElementsAsString, NUnit.Framework.Is.EqualTo(@"1 - 銀行繳款
2 - 自動扣款
3 - 支票"));
				}
				else
				{
					NUnit.Framework.Assert.That(paymentMethodList.Count, NUnit.Framework.Is.EqualTo(0), "The count of PaymentMethodList is 0 when Controlling Agency is not 20, CI, 2Q, DN, VP, CD, IF or DH");
				}
			}
		}

		[ExpectNoExceptions]
		public void TestAppointmentPeriodList()
		{
			var messageTypes = lookups.ControllingMessageTypeList.GetAllCodes();
			foreach (var messageType in messageTypes)
			{
				cusTWControllingMessageHeader.TW1_ControllingMessageType = messageType;
				var list = lookups.AppointmentPeriodList;
				if (messageType == ControllingMessageTypeList.Codes.NX401
					|| messageType == ControllingMessageTypeList.Codes.NX601
					|| messageType == ControllingMessageTypeList.Codes.NX603
					|| messageType == ControllingMessageTypeList.Codes.NX301_AX)
				{
					NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(4));
					NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("A, P, O, M"));
					NUnit.Framework.Assert.That(list.ElementsAsString, NUnit.Framework.Is.EqualTo(@"A - 0800~1200
P - 1201~1800
O - 1801~2400
M - Other"));
				}
				else if (messageType == ControllingMessageTypeList.Codes.NX301 || messageType == ControllingMessageTypeList.Codes.NX301_DN)
				{
					NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(2));
					NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("A, P"));
					NUnit.Framework.Assert.That(list.ElementsAsString, NUnit.Framework.Is.EqualTo(@"A - 0800~1200
P - 1201~1800"));
				}
				else
				{
					NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(0));
				}
			}
		}

		[ExpectNoExceptions]
		public void TestPreWineInspectionStatusList()
		{
			var list = lookups.PreWineInspectionStatusList;
			NUnit.Framework.Assert.That(list, NUnit.Framework.Is.TypeOf<PreviousImportedWineInspectionStatusList>());
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(3));
			NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("1, 2, 3"));
		}

		[ExpectNoExceptions]
		public void TestPurposeList()
		{
			AssertPurposeListWhenBusinessTypeChanged<CodeDescriptionPairList>(ZString.Empty);
			AssertPurposeListWhenBusinessTypeChanged<CPT_117_A_B_PurposeCodeList>("A", new string[] { "1", "2", "3" });
			AssertPurposeListWhenBusinessTypeChanged<CPT_117_A_B_PurposeCodeList>("B", new string[] { "1", "2", "3" });
			AssertPurposeListWhenBusinessTypeChanged<CPT_117_C_PurposeCodeList>("C", new string[] { "51", "52", "53", "54", "99" });
		}

		[ExpectNoExceptions]
		void AssertPurposeListWhenBusinessTypeChanged<T>(ZString businessType, params string[] codes)
			where T : CodeDescriptionPairList
		{
			cusTWControllingMessageHeader.TW1_BusinessType = businessType;
			var list = lookups.PurposeList;
			NUnit.Framework.Assert.That(list, NUnit.Framework.Is.TypeOf<T>());
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(codes.Length));
			NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo(new ZStringBuilder(codes).ToStringWithDelimiterBetweenAppends(", ")));
		}

		[ExpectNoExceptions]
		public void TestControllingAgencyList()
		{
			AssertControllingAgencyListWhenControllingMessageTypeChanged("X101", "FT");
			AssertControllingAgencyListWhenControllingMessageTypeChanged("NX301", new string[] { "20", "2Q", "CI" });
			AssertControllingAgencyListWhenControllingMessageTypeChanged("NX301_DN", "DN");
			AssertControllingAgencyListWhenControllingMessageTypeChanged("NX401", "VP");
			AssertControllingAgencyListWhenControllingMessageTypeChanged("NX601", new string[] { "CD", "DH", "IF" });
			AssertControllingAgencyListWhenControllingMessageTypeChanged("NX603", new string[] { "CD", "DH", "IF" });
		}

		[ExpectNoExceptions]
		void AssertControllingAgencyListWhenControllingMessageTypeChanged<T>(ZString controllingMessageType, params string[] codes)
			where T : CodeDescriptionPairList
		{
			cusTWControllingMessageHeader.TW1_ControllingMessageType = controllingMessageType;
			var list = lookups.ControllingAgencyList;
			NUnit.Framework.Assert.That(list, NUnit.Framework.Is.TypeOf<T>());
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(codes.Length));
			NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo(new ZStringBuilder(codes).ToStringWithDelimiterBetweenAppends(", ")), controllingMessageType.ToString());
		}

		[ExpectNoExceptions]
		void AssertControllingAgencyListWhenControllingMessageTypeChanged(ZString controllingMessageType, params string[] codes)
		{
			AssertControllingAgencyListWhenControllingMessageTypeChanged<CodeDescriptionPairList>(controllingMessageType, codes);
		}

		[ExpectNoExceptions]
		public void TestLocalProcessorOrganisations()
		{
			NUnit.Framework.Assert.That(lookups.LocalProcessorOrganisations.GetType(), NUnit.Framework.Is.EqualTo(typeof(OrgHeaderCollection)));
		}

		[ExpectNoExceptions]
		public void TestCertificateTypeList()
		{
			var certificateTypeList = lookups.CertificateTypeList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(certificateTypeList, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<CertificateTypeList>()));
				NUnit.Framework.Assert.That(certificateTypeList.CodesAsString, NUnit.Framework.Is.EqualTo("00, 01, 02, 04, 05, 06, 07, 08, 09, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19"));
			});
		}

		[ExpectNoExceptions]
		public void TestEUSteelDeclarationCodeList()
		{
			var euSteelDeclarationCodeList = lookups.EUSteelDeclarationCodeList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(euSteelDeclarationCodeList, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<EUSteelDeclarationCodeList>()));
				NUnit.Framework.Assert.That(euSteelDeclarationCodeList.CodesAsString, NUnit.Framework.Is.EqualTo("01, 02"));
			});
		}

		[ExpectNoExceptions]
		public void TestEUSteelPhaseCodeList()
		{
			var euSteelPhaseCodeList = lookups.EUSteelPhaseCodeList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(euSteelPhaseCodeList, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<EUSteelPhaseCodeList>()));
				NUnit.Framework.Assert.That(euSteelPhaseCodeList.CodesAsString, NUnit.Framework.Is.EqualTo("1, 2, 3, 4, 5, 6"));
			});
		}

		[ExpectNoExceptions]
		public void TestManufacturerPrintingCodeList()
		{
			cusTWControllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code9;
			var manufacturerPrintingCodeList = lookups.ManufacturerPrintingCodeList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(manufacturerPrintingCodeList, NUnit.Framework.Is.TypeOf<CPT_123_ManufacturerPrintingCodeList>());
				NUnit.Framework.Assert.That(manufacturerPrintingCodeList.CodesAsString, NUnit.Framework.Is.EqualTo("1, 2, 3"));
			});

			cusTWControllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code11;
			manufacturerPrintingCodeList = lookups.ManufacturerPrintingCodeList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(manufacturerPrintingCodeList, NUnit.Framework.Is.TypeOf<CPT_123_ManufacturerPrintingCodeList>());
				NUnit.Framework.Assert.That(manufacturerPrintingCodeList.CodesAsString, NUnit.Framework.Is.EqualTo("1, 2, 3"));
			});

			cusTWControllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code13;
			manufacturerPrintingCodeList = lookups.ManufacturerPrintingCodeList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(manufacturerPrintingCodeList, NUnit.Framework.Is.TypeOf<CPT_123_ManufacturerPrintingCodeList>());
				NUnit.Framework.Assert.That(manufacturerPrintingCodeList.CodesAsString, NUnit.Framework.Is.EqualTo("1, 2, 3"));
			});

			cusTWControllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code14;
			manufacturerPrintingCodeList = lookups.ManufacturerPrintingCodeList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(manufacturerPrintingCodeList, NUnit.Framework.Is.TypeOf<CPT_123_ManufacturerPrintingCodeList>());
				NUnit.Framework.Assert.That(manufacturerPrintingCodeList.CodesAsString, NUnit.Framework.Is.EqualTo("1, 2, 3"));
			});

			cusTWControllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			manufacturerPrintingCodeList = lookups.ManufacturerPrintingCodeList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(manufacturerPrintingCodeList, NUnit.Framework.Is.TypeOf<CPT_123_15_ManufacturerPrintingCodeList>());
				NUnit.Framework.Assert.That(manufacturerPrintingCodeList.CodesAsString, NUnit.Framework.Is.EqualTo("1, 2, 3"));
			});

			cusTWControllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code5;
			manufacturerPrintingCodeList = lookups.ManufacturerPrintingCodeList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(manufacturerPrintingCodeList, NUnit.Framework.Is.TypeOf<CodeDescriptionPairList>());
				NUnit.Framework.Assert.That(manufacturerPrintingCodeList.Count, NUnit.Framework.Is.EqualTo(0));
			});
		}

		[ExpectNoExceptions]
		public void TestCPT_123_PrintingCodeList()
		{
			var printingCodeList = lookups.CPT_123_PrintingCodeList;
			var list = Factory.GetCachedValue<CPT_123_PrintingCodeList>();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(printingCodeList, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<CPT_123_PrintingCodeList>()));
				NUnit.Framework.Assert.That(printingCodeList.CodesAsString, NUnit.Framework.Is.EqualTo("01, 02, 03, 04"));
			});
		}

		[ExpectNoExceptions]
		public void TestCPT_127_GoodsReleaseReasonCodeList()
		{
			var goodsReleaseReasonCodeList = lookups.CPT_127_GoodsReleaseReasonCodeList;
			var list = Factory.GetCachedValue<CPT_127_GoodsReleaseReasonCodeList>();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsReleaseReasonCodeList, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<CPT_127_GoodsReleaseReasonCodeList>()));
				NUnit.Framework.Assert.That(goodsReleaseReasonCodeList.CodesAsString, NUnit.Framework.Is.EqualTo("01, 02, 03, 04, 05, 06"));
			});
		}

		[ExpectNoExceptions]
		public void TestPortOfBulkCommodityList()
		{
			var portOfBulkCommodityList = lookups.PortOfBulkCommodityList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(portOfBulkCommodityList, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<PortOfBulkCommodityList>()));
				NUnit.Framework.Assert.That(portOfBulkCommodityList.CodesAsString, NUnit.Framework.Is.EqualTo("0, 1, 2, 9"));
			});
		}

		[ExpectNoExceptions]
		public void TestLicensingStatusList()
		{
			cusTWControllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			NUnit.Framework.Assert.That(lookups.LicensingStatusList, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<NX102ResultCodeList>()));

			cusTWControllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX201_01;
			NUnit.Framework.Assert.That(lookups.LicensingStatusList, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<CPT_120_202_Result>()));

			cusTWControllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX201_07;
			NUnit.Framework.Assert.That(lookups.LicensingStatusList, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<CPT_120_202_Result>()));

			cusTWControllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
			NUnit.Framework.Assert.That(lookups.LicensingStatusList, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<NX302NX602AuditResultCodeList>()));

			cusTWControllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_AX;
			NUnit.Framework.Assert.That(lookups.LicensingStatusList, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<NX302NX602AuditResultCodeList>()));

			cusTWControllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			NUnit.Framework.Assert.That(lookups.LicensingStatusList, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<NX302_DNAuditResultCodeList>()));

			cusTWControllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			NUnit.Framework.Assert.That(lookups.LicensingStatusList, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<CPT_104_QuarantineResultCodeList>()));

			cusTWControllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			NUnit.Framework.Assert.That(lookups.LicensingStatusList, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<NX302NX602AuditResultCodeList>()));

			cusTWControllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX603;
			NUnit.Framework.Assert.That(lookups.LicensingStatusList, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<NX302NX602AuditResultCodeList>()));
		}

		[ExpectNoExceptions]
		public void TestLicensingMessageStatusList()
		{
			var licensingMessageStatusList = lookups.LicensingMessageStatusList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(licensingMessageStatusList, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<TWMessageStatusCodeList>()));
				NUnit.Framework.Assert.That(licensingMessageStatusList.CodesAsString, NUnit.Framework.Is.EqualTo("NOT, UNK, AWR, ERR, SNT, ACK"));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			new TestTWCreator(Factory).CreateRefCusCodeForControllingMessageType();
			declartion = Factory.NewWithValidTestData<JobDeclaration>();
			cusTWControllingMessageHeader = declartion.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			lookups = cusTWControllingMessageHeader.Lookups;
		}

		JobDeclaration declartion;
		CusTWControllingMessageHeaderLookups lookups;
		CusTWControllingMessageHeader cusTWControllingMessageHeader;
	}
}
