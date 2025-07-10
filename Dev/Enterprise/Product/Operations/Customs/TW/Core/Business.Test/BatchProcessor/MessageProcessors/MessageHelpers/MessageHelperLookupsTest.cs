using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(MessageHelperLookups))]
	sealed class MessageHelperLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestGetGovernmentAgencyResponseCodeList()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(lookups.GetGovernmentAgencyResponseCodeList(ControllingAgencyList.Codes.FT).Count, NUnit.Framework.Is.EqualTo(67));
				NUnit.Framework.Assert.That(lookups.GetGovernmentAgencyResponseCodeList(ControllingAgencyList.Codes.CI).Count, NUnit.Framework.Is.EqualTo(97));
				NUnit.Framework.Assert.That(lookups.GetGovernmentAgencyResponseCodeList(ControllingAgencyList.Codes._20), NUnit.Framework.Is.EqualTo(lookups.GetGovernmentAgencyResponseCodeList(ControllingAgencyList.Codes.CI)));
				NUnit.Framework.Assert.That(lookups.GetGovernmentAgencyResponseCodeList(ControllingAgencyList.Codes._2Q), NUnit.Framework.Is.EqualTo(lookups.GetGovernmentAgencyResponseCodeList(ControllingAgencyList.Codes.CI)));
				NUnit.Framework.Assert.That(lookups.GetGovernmentAgencyResponseCodeList(ControllingAgencyList.Codes.DN).Count, NUnit.Framework.Is.EqualTo(105));
				NUnit.Framework.Assert.That(lookups.GetGovernmentAgencyResponseCodeList(ControllingAgencyList.Codes.CD).Count, NUnit.Framework.Is.EqualTo(77));
				NUnit.Framework.Assert.That(lookups.GetGovernmentAgencyResponseCodeList(ControllingAgencyList.Codes.IF), NUnit.Framework.Is.EqualTo(lookups.GetGovernmentAgencyResponseCodeList(ControllingAgencyList.Codes.CD)));
				NUnit.Framework.Assert.That(lookups.GetGovernmentAgencyResponseCodeList(ControllingAgencyList.Codes.DH), NUnit.Framework.Is.EqualTo(lookups.GetGovernmentAgencyResponseCodeList(ControllingAgencyList.Codes.CD)));
				NUnit.Framework.Assert.That(lookups.GetGovernmentAgencyResponseCodeList(ControllingAgencyList.Codes.VP).Count, NUnit.Framework.Is.EqualTo(44));
				NUnit.Framework.Assert.That(lookups.GetGovernmentAgencyResponseCodeList(ControllingAgencyList.Codes.AX).Count, NUnit.Framework.Is.EqualTo(73));
				NUnit.Framework.Assert.That(lookups.GetGovernmentAgencyResponseCodeList(ControllingAgencyList.Codes.AG), NUnit.Framework.Is.EqualTo(lookups.GetGovernmentAgencyResponseCodeList(ControllingAgencyList.Codes.AX)));
			});
		}

		[ExpectNoExceptions]
		public void TestLookups()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanCertificateOfOriginType, "Taiwan Certificate Of Origin Type");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanCertificateOfOriginType, "C1", "C1 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanCertificateOfOriginType, "C2", "C2 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(lookups.NX302_DNAuditResultCodeList.Count, NUnit.Framework.Is.EqualTo(3), "NX302_DNAuditResultCodeList");
				NUnit.Framework.Assert.That(lookups.CPT_118_302_DN_CodeofErrorConditionOverrideCodeList.Count, NUnit.Framework.Is.EqualTo(3), "CPT_118_302_DN_CodeofErrorConditionOverrideCodeList");
				NUnit.Framework.Assert.That(lookups.NX302_DNBusinessTypeList.Count, NUnit.Framework.Is.EqualTo(3), "NX302_DNBusinessTypeList");
				NUnit.Framework.Assert.That(lookups.TransportCodeList.Count, NUnit.Framework.Is.EqualTo(16), "TransportCodeList");
				NUnit.Framework.Assert.That(lookups.CPT_120_402_ResultCodeList.Count, NUnit.Framework.Is.EqualTo(53), "CPT_120_402_ResultCodeList");
				NUnit.Framework.Assert.That(lookups.CPT_104_QuarantineResultCodeList.Count, NUnit.Framework.Is.EqualTo(11), "CPT_104_QuarantineResultCodeList");
				NUnit.Framework.Assert.That(lookups.CPT_025_ExtraConditionCodeList.Count, NUnit.Framework.Is.EqualTo(21), "CPT_025_ExtraConditionCodeList");
				NUnit.Framework.Assert.That(lookups.ModeofCustomsClearanceCodeList.Count, NUnit.Framework.Is.EqualTo(4), "ModeofCustomsClearanceCodeList");
				NUnit.Framework.Assert.That(lookups.ProcessCodeList.Count, NUnit.Framework.Is.EqualTo(22), "ProcessCodeList");
				NUnit.Framework.Assert.That(lookups.CPT_118_302_AX_CodeofErrorConditionOverrideCodeList.Count, NUnit.Framework.Is.EqualTo(7), "CPT_118_302_AX_CodeofErrorConditionOverrideCodeList");
				NUnit.Framework.Assert.That(lookups.TypeofReleaseNoteCodeList.Count, NUnit.Framework.Is.EqualTo(3), "TypeofReleaseNoteCodeList");
				NUnit.Framework.Assert.That(lookups.UnabletoHandleContainerCodeList.Count, NUnit.Framework.Is.EqualTo(6), "UnabletoHandleContainerCodeList");
				NUnit.Framework.Assert.That(lookups.ResponseCodeList.Count, NUnit.Framework.Is.EqualTo(2), "ResponseCodeList");
				NUnit.Framework.Assert.That(lookups.CPT_118_302_CodeofErrorConditionOverrideCodeList.Count, NUnit.Framework.Is.EqualTo(42), "CPT_118_302_CodeofErrorConditionOverrideCodeList");
				NUnit.Framework.Assert.That(lookups.FunctionCodeList.Count, NUnit.Framework.Is.EqualTo(2), "FunctionCodeList");
				NUnit.Framework.Assert.That(lookups.TypeOfApplicationCode_FromGlobalCodes.Count, NUnit.Framework.Is.EqualTo(2), "TypeOfApplicationCode_FromGlobalCodes");
				NUnit.Framework.Assert.That(lookups.CPT_118_NX102_ErrorConditionOverrideCodeList.Count, NUnit.Framework.Is.EqualTo(3), "CPT_118_NX102_ErrorConditionOverrideCodeList");
				NUnit.Framework.Assert.That(lookups.CPT_118_602_CodeOfErrorConditionOverrideCodeList.Count, NUnit.Framework.Is.EqualTo(6), "CPT_118_602_CodeOfErrorConditionOverrideCodeList");
				NUnit.Framework.Assert.That(lookups.CPT_121_CodeOfSpecialConditionCodeList.Count, NUnit.Framework.Is.EqualTo(200), "CPT_121_CodeOfSpecialConditionCodeList");
				NUnit.Framework.Assert.That(lookups.CPT_120_202_ResultCodeList.Count, NUnit.Framework.Is.EqualTo(73), "CPT_120_202_ResultCodeList");
				NUnit.Framework.Assert.That(lookups.NX202TypeOfApplicationCodeList.Count, NUnit.Framework.Is.EqualTo(4), "NX202TypeOfApplicationCodeList");
			});
		}

		[ExpectNoExceptions]
		public void TestNX302AuditResultCoded()
		{
			var list = lookups.NX302AuditResultCoded;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("Y, N"), "Values");
				NUnit.Framework.Assert.That(list.GetDescriptionFromCode("N"), NUnit.Framework.Is.EqualTo("不合格/不同意"), "N Descrption");
				NUnit.Framework.Assert.That(list.GetDescriptionFromCode("Y"), NUnit.Framework.Is.EqualTo("合格/同意"), "Y Descrption");
			});
		}

		[ExpectNoExceptions]
		public void TestRejectionReasonList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.RejectionReason, "RejectionReason");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.RejectionReason, "RR01", "AAA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			NUnit.Framework.Assert.That(lookups.RejectionReasonList.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(lookups.RejectionReasonList.GetDescriptionFromCode("RR01"), NUnit.Framework.Is.EqualTo("AAA"));
		}

		[ExpectNoExceptions]
		public void TestNX903_CodeOfErrorOrIrregularityNoticeCodeList()
		{
			var list = lookups.NX903_CodeOfErrorOrIrregularityNoticeCodeList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("1, 2, 3, 4, 5, 6, 7, 8, 9, A, B, C"), "Values");
				NUnit.Framework.Assert.That(list, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<NX903_CodeOfErrorOrIrregularityNoticeCodeList>()), "Cached");
			});
		}

		[ExpectNoExceptions]
		public void TestRequiredFormalitiesList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.RequiredFormalities, "RequiredFormalities");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.RequiredFormalities, "RF01", "BBB", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			NUnit.Framework.Assert.That(lookups.RequiredFormalitiesList.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(lookups.RequiredFormalitiesList.GetDescriptionFromCode("RF01"), NUnit.Framework.Is.EqualTo("BBB"));
		}

		[ExpectNoExceptions]
		public void TestICIRefCusCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("ICI", "ICI");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "ICI", "ICI1", "CCC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			NUnit.Framework.Assert.That(lookups.ICIRefCusCodeList.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(lookups.ICIRefCusCodeList.GetDescriptionFromCode("ICI1"), NUnit.Framework.Is.EqualTo("CCC"));
		}

		[ExpectNoExceptions]
		public void TestEntryTypeList()
		{
			var list = lookups.EntryTypeList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("T1, T2, T4, T5, T6"), "Values");
				NUnit.Framework.Assert.That(list, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<EntryTypeList>()), "Cached");
			});
		}

		[ExpectNoExceptions]
		public void TestInBondTransportModeList()
		{
			var list = lookups.InBondTransportModeList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("01, 04"), "Values");
				NUnit.Framework.Assert.That(list, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<InBondTransportModeCodes>()), "Cached");
			});
		}

		[ExpectNoExceptions]
		public void TestTranshipmentExaminationNoteList()
		{
			var list = lookups.TranshipmentExaminationNoteList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("E, N, P, Y"), "Values");
				NUnit.Framework.Assert.That(list, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<TranshipmentExaminationNoteList>()), "Cached");
			});
		}

		[ExpectNoExceptions]
		public void TestTransportContractTypeList()
		{
			var list = lookups.TransportContractTypeList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("703, 704, 714, 741"), "Values");
				NUnit.Framework.Assert.That(list, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<TransportContractTypeList>()), "Cached");
			});
		}

		[ExpectNoExceptions]
		public void TestN5108ResponseCodeList()
		{
			new TestTWCreator(Factory).CreateCustomsManifestStatus();
			NUnit.Framework.Assert.That(lookups.N5108ResponseCodeList.CodesAsString, NUnit.Framework.Is.EqualTo("AK, AP, EX, RE"), "Values");
		}

		[ExpectNoExceptions]
		public void TestTWCARefCusCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWControllingAgency, "TW Controlling Agency");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWControllingAgency, "A00", "CCC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(lookups.TWCARefCusCodeList.Count, NUnit.Framework.Is.EqualTo(1));
				NUnit.Framework.Assert.That(lookups.TWCARefCusCodeList.GetDescriptionFromCode("A00"), NUnit.Framework.Is.EqualTo("CCC"));
			});
		}

		[ExpectNoExceptions]
		public void TestCPT_111_201_ApplicationType()
		{
			var list = lookups.CPT_111_201_ApplicationType;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("A, B, C, D, M, Q, X"), "Values");
				NUnit.Framework.Assert.That(list, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<CPT_111_201_ApplicationType>()), "Cached");
			});
		}

		[ExpectNoExceptions]
		public void TestNX902_NotificationCodeList()
		{
			var list = lookups.CPT_119_902_NotificationCodeList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("2A, 2B, 2C, 2D, 2E, 1, 2, 3, V1, V2, V3, F1, F2, F3"), "Values");
				NUnit.Framework.Assert.That(list, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<CPT_119_902_NotificationCodeList>()), "Cached");
			});
		}

		[ExpectNoExceptions]
		public void TestAppointmentPeriodList()
		{
			var list = lookups.AppointmentPeriodList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("A, P, O, M"), "Values");
				NUnit.Framework.Assert.That(list, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<CPT_113_AppointmentPeriodList>()), "Cached");
			});
		}

		[ExpectNoExceptions]
		public void TestGetTypeOfApplicationCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanCertificateOfOriginType, "Taiwan Certificate Of Origin Type");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanCertificateOfOriginType, "C1", "C1 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanCertificateOfOriginType, "C2", "C2 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				var list = lookups.GetTypeOfApplicationCodeList(ControllingMessageTypeList.Codes.NX301_DN);
				NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("A, B, C, 30, 40, 50, 60, 01"), "NX301_DN Values");

				list = lookups.GetTypeOfApplicationCodeList(ControllingMessageTypeList.Codes.NX401);
				NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("A, B, C, 30, 40, 50, 60, 01"), "NX401 Values");

				list = lookups.GetTypeOfApplicationCodeList(ControllingMessageTypeList.Codes.NX603);
				NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("A, B, C, 30, 40, 50, 60, 01"), "NX603 Values");

				list = lookups.GetTypeOfApplicationCodeList(ControllingMessageTypeList.Codes.NX101);
				NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("C1, C2"), "NX101 Values");

				list = lookups.GetTypeOfApplicationCodeList(ControllingMessageTypeList.Codes.NX201_01);
				NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("0, 1, 2, 3"), "NX201_01 Values");

				list = lookups.GetTypeOfApplicationCodeList(ControllingMessageTypeList.Codes.NX201_07);
				NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("0, 1, 2, 3"), "NX201_07 Values");

				list = lookups.GetTypeOfApplicationCodeList(ControllingMessageTypeList.Codes.NX601);
				NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.Null.Or.Empty, "NX601 Value is empty - should be [null] or [empty]");
			});
		}

		[ExpectNoExceptions]
		public void TestN5108_ErrorCodeList()
		{
			var list = lookups.N5108_ErrorCodeList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(54), "Count");
				NUnit.Framework.Assert.That(list, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<N5108_ErrorCode>()), "Cached");
			});
		}

		MessageHelperLookups lookups;
		protected override void SetUp()
		{
			base.SetUp();
			lookups = new MessageHelperLookups(Factory.New<TWMessage>());
		}
	}
}
