using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	sealed class CusEntryPayInfoDataObjectReader : DataObjectReader<UniversalCustoms.EntryHeaderPaymentInformation, CusEntryPayInfo>
	{
		public CusEntryPayInfoDataObjectReader(UniversalCustoms.EntryHeaderPaymentInformation dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, CusEntryHeader entryHeader) : base(dataObject, logger, factory)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		}

		protected override CusEntryPayInfo GetExistingBusinessObject()
		{
			Func<CusEntryPayInfo, bool> matchPredicate = x =>
				x.C9_TransactionType == dataObject.TransactionType.GetCodeAsUpperCase() &&
				x.C9_IncomingPayResponseNo == dataObject.IncomingPaymentResponseNumber.GetValueOrDefault() &&
				x.C9_PaymentParty == dataObject.PaymentParty.GetCodeAsUpperCase();
			return entryHeader.EntryPayInfos.FirstOrDefault(matchPredicate);
		}

		protected override CusEntryPayInfo GetNewBusinessObject() => entryHeader.EntryPayInfos.AddNew();

		protected override void PopulateBusinessObject(CusEntryPayInfo targetBO)
		{
			var columnIndexer = GetColumnIndexer(targetBO);
			SetValue(columnIndexer, CusEntryPayInfoSchema.C9_CusResReceived, dataObject.CustomsResponseReceived);
			SetValue(columnIndexer, CusEntryPayInfoSchema.C9_IncomingPayResponseNo, dataObject.IncomingPaymentResponseNumber);
			SetValue(columnIndexer, CusEntryPayInfoSchema.C9_PaymentAmount, dataObject.PaymentAmount);
			SetValue(columnIndexer, CusEntryPayInfoSchema.C9_PaymentDate, dataObject.PaymentDate);
			SetValue(columnIndexer, CusEntryPayInfoSchema.C9_PaymentParty, dataObject.PaymentParty);
			SetValue(columnIndexer, CusEntryPayInfoSchema.C9_PaymentReference, dataObject.PaymentReference);
			SetValue(columnIndexer, CusEntryPayInfoSchema.C9_PaymentStatus, dataObject.PaymentStatus);
			SetValue(columnIndexer, CusEntryPayInfoSchema.C9_ReceiptDate, dataObject.EntryReleaseDate);
			SetValue(columnIndexer, CusEntryPayInfoSchema.C9_RemAdvReceived, dataObject.RemittanceAdviceReceived);
			SetValue(columnIndexer, CusEntryPayInfoSchema.C9_TransactionType, dataObject.TransactionType);
		}

		readonly CusEntryHeader entryHeader;
	}
}
