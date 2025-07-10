using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestCusEntryPayInfo_EntryHeaderGuardClause()
		{
			AssertExceptionThrown<ArgumentNullException>(
				"When entryHeader is null",
				() => new CusEntryPayInfoDataObjectReader(new EntryHeaderPaymentInformation(), logger, Factory, entryHeader: null));
		}

		public void TestCusEntryPayInfo_FieldsMappings()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			Factory.SaveForTesting();
			AssertEquals("PRE-CONDITION: EntryPayInfo Items Count", 0, entryHeader.EntryPayInfos.Count);

			var universalPayInfo = new EntryHeaderPaymentInformation
			{
				CustomsResponseReceived = true,
				EntryReleaseDate = new ZDateTime(2024, 01, 01),
				IncomingPaymentResponseNumber = "123",
				PaymentAmount = 999.99m,
				PaymentDate = new ZDateTime(2024, 02, 01),
				PaymentParty = new CodeDescriptionPair { Code = "PP" },
				PaymentReference = "PR",
				PaymentStatus = new CodeDescriptionPair { Code = "PS" },
				RemittanceAdviceReceived = true,
				TransactionType = new CodeDescriptionPair { Code = "TT" }
			};

			new CusEntryPayInfoDataObjectReader(universalPayInfo, logger, Factory, entryHeader)
				.ReadIntoBusinessObject();

			AssertEquals("POST-CONDITION: EntryPayInfo Items Count", 1, entryHeader.EntryPayInfos.Count);
			CombineAssertions("POST-CONDITION: Single EntryPayInfo Values", () =>
			{
				var entryPayInfo = entryHeader.EntryPayInfos[0];
				AssertEquals("C9_CH", entryHeader.PK, entryPayInfo.C9_CH);
				AssertEquals("C9_ClusterKey", entryHeader.CH_ClusterKey, entryPayInfo.C9_ClusterKey);
				AssertEquals("C9_CusResReceived", true, entryPayInfo.C9_CusResReceived);
				AssertEquals("C9_IncomingPayResponseNo", "123", entryPayInfo.C9_IncomingPayResponseNo);
				AssertEquals("C9_PaymentAmount", 999.99m, entryPayInfo.C9_PaymentAmount);
				AssertEquals("C9_PaymentDate", new ZDateTime(2024, 02, 01), entryPayInfo.C9_PaymentDate);
				AssertEquals("C9_PaymentParty", "PP", entryPayInfo.C9_PaymentParty);
				AssertEquals("C9_PaymentReference", "PR", entryPayInfo.C9_PaymentReference);
				AssertEquals("C9_PaymentStatus", "PS", entryPayInfo.C9_PaymentStatus);
				AssertEquals("C9_ReceiptDate", new ZDateTime(2024, 01, 01), entryPayInfo.C9_ReceiptDate);
				AssertEquals("C9_RemAdvReceived", true, entryPayInfo.C9_RemAdvReceived);
				AssertEquals("C9_TransactionType", "TT", entryPayInfo.C9_TransactionType);
			});
		}

		public void TestCusEntryPayInfo_UpdateExistingRecordMatchedByKey()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryPayInfo = entryHeader.EntryPayInfos.AddNew();
			entryPayInfo.C9_TransactionType = "TT";
			entryPayInfo.C9_IncomingPayResponseNo = "123";
			entryPayInfo.C9_PaymentParty = "G";
			entryPayInfo.C9_PaymentAmount = 1m;
			AssertEquals("PRE-CONDITION: EntryPayInfo Items Count", 1, entryHeader.EntryPayInfos.Count);

			var universalPayInfo = new EntryHeaderPaymentInformation
			{
				IncomingPaymentResponseNumber = "123",
				PaymentAmount = 2m,
				PaymentParty = new CodeDescriptionPair { Code = "G" },
				TransactionType = new CodeDescriptionPair { Code = "TT" }
			};

			new CusEntryPayInfoDataObjectReader(universalPayInfo, logger, Factory, entryHeader)
				.ReadIntoBusinessObject();

			CombineAssertions("POST-CONDITION", () =>
			{
				AssertEquals("EntryPayInfo Items Count", 1, entryHeader.EntryPayInfos.Count);
				AssertEquals("Single EntryPayInfo C9_PaymentAmount", 2m, entryPayInfo.C9_PaymentAmount);
			});
		}

		public void TestCusEntryPayInfo_CreateNewRecordWhenDataObjectKeyIsDifferentOnC9_TransactionType()
		{
			Action<CusEntryPayInfo> setUpEntryPayInfo = (entryPayInfo) =>
			{
				entryPayInfo.C9_TransactionType = "TT";
				entryPayInfo.C9_IncomingPayResponseNo = "123";
				entryPayInfo.C9_PaymentParty = "G";
				entryPayInfo.C9_PaymentAmount = 1m;
			};

			var universalPayInfo = new EntryHeaderPaymentInformation
			{
				TransactionType = new CodeDescriptionPair { Code = "XX" },
				IncomingPaymentResponseNumber = "123",
				PaymentParty = new CodeDescriptionPair { Code = "G" },
				PaymentAmount = 2m,
			};

			SetUpEntryPayInfoAndAssertNewRecordCreated(setUpEntryPayInfo, universalPayInfo);
		}

		public void TestCusEntryPayInfo_CreateNewRecordWhenDataObjectKeyIsDifferentOnC9_IncomingPayResponseNo()
		{
			Action<CusEntryPayInfo> setUpEntryPayInfo = (entryPayInfo) =>
			{
				entryPayInfo.C9_TransactionType = "TT";
				entryPayInfo.C9_IncomingPayResponseNo = "123";
				entryPayInfo.C9_PaymentParty = "G";
				entryPayInfo.C9_PaymentAmount = 1m;
			};

			var universalPayInfo = new EntryHeaderPaymentInformation
			{
				TransactionType = new CodeDescriptionPair { Code = "TT" },
				IncomingPaymentResponseNumber = "XXX",
				PaymentParty = new CodeDescriptionPair { Code = "G" },
				PaymentAmount = 2m,
			};

			SetUpEntryPayInfoAndAssertNewRecordCreated(setUpEntryPayInfo, universalPayInfo);
		}

		public void TestCusEntryPayInfo_CreateNewRecordWhenDataObjectKeyIsDifferentOnC9_PaymentParty()
		{
			Action<CusEntryPayInfo> setUpEntryPayInfo = (entryPayInfo) =>
			{
				entryPayInfo.C9_TransactionType = "TT";
				entryPayInfo.C9_IncomingPayResponseNo = "123";
				entryPayInfo.C9_PaymentParty = "G";
				entryPayInfo.C9_PaymentAmount = 1m;
			};

			var universalPayInfo = new EntryHeaderPaymentInformation
			{
				TransactionType = new CodeDescriptionPair { Code = "TT" },
				IncomingPaymentResponseNumber = "123",
				PaymentParty = new CodeDescriptionPair { Code = "X" },
				PaymentAmount = 2m,
			};

			SetUpEntryPayInfoAndAssertNewRecordCreated(setUpEntryPayInfo, universalPayInfo);
		}

		void SetUpEntryPayInfoAndAssertNewRecordCreated(Action<CusEntryPayInfo> setUpEntryPayInfo, EntryHeaderPaymentInformation universalPayInfo)
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryPayInfo = entryHeader.EntryPayInfos.AddNew();
			setUpEntryPayInfo(entryPayInfo);

			AssertEquals("PRE-CONDITION: EntryPayInfo Items Count", 1, entryHeader.EntryPayInfos.Count);

			new CusEntryPayInfoDataObjectReader(universalPayInfo, logger, Factory, entryHeader)
				.ReadIntoBusinessObject();

			AssertEquals("POST-CONDITION: EntryPayInfo Items Count", 2, entryHeader.EntryPayInfos.Count);
		}
	}
}
