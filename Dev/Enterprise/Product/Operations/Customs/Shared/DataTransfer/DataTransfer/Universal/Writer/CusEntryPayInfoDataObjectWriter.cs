using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CusEntryPayInfoDataObjectWriter : DataObjectWriter<CusEntryPayInfo, EntryHeaderPaymentInformation>
	{
		public CusEntryPayInfoDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper)
			: base(manager)
		{
			this.helper = Argument.NotNull(helper, "UniversalDataObjectWriterHelper helper");
		}

		protected readonly UniversalDataObjectWriterHelper helper;

		protected override EntryHeaderPaymentInformation PopulateDataObject(CusEntryPayInfo sourceBO)
		{
			var entryPayInfoLookups = sourceBO.Lookups;
			return new EntryHeaderPaymentInformation()
			{
				PaymentDate = sourceBO.C9_PaymentDate,
				EntryReleaseDate = sourceBO.C9_ReceiptDate,
				TransactionType = ListHelper.GetWithDescription<CodeDescriptionPair>(sourceBO.C9_TransactionType, entryPayInfoLookups.TransactionTypeList),
				PaymentParty = ListHelper.GetWithDescription<CodeDescriptionPair>(sourceBO.C9_PaymentParty, entryPayInfoLookups.PaymentPartyList),
				PaymentStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(sourceBO.C9_PaymentStatus, entryPayInfoLookups.PaymentStatusList),
				CustomsResponseReceived = sourceBO.C9_CusResReceived,
				RemittanceAdviceReceived = sourceBO.C9_RemAdvReceived,
				PaymentAmount = sourceBO.C9_PaymentAmount,
				PaymentReference = sourceBO.C9_PaymentReference,
				IncomingPaymentResponseNumber = sourceBO.C9_IncomingPayResponseNo
			};
		}
	}
}
