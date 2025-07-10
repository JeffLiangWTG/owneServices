using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public class VAT404DocumentInstruction : AutoVAT404DocumentInstruction
	{
		#region ctor

		public VAT404DocumentInstruction(BusinessObjectFactory factory) : base(factory)
		{
			using (SuspendSettingHasChanges())
			using (GetValidationSuspender())
			{
				var currentDate = ZDateTime.Today;
				base.StartDate = new ZDateTime(currentDate.Year, currentDate.Month, 1);
				base.EndDate = currentDate;
			}
		}

		#endregion

		#region Override PrinterPK

		[List(nameof(Printers))]
		public override ZGuid PrinterPK
		{
			get { return base.PrinterPK; }
			set { base.PrinterPK = value; }
		}

		public CodeDescriptionPairList Printers
		{
			get { return printers ?? (printers = new DocumentEngine.DocDeliveryPrintDetails(Factory).PrinterNames); }
		}
		CodeDescriptionPairList printers;

		#endregion

		#region Search Filter Collections

		[ChildEditable]
		public NonPersistentBusinessObjectCollectionHolder<ImporterHolder> ImporterForFilter
		{
			get
			{
				if (importerForFilter == null)
				{
					importerForFilter = new NonPersistentBusinessObjectCollectionHolder<ImporterHolder>(this, (parent) => new ImporterHolder(parent));
					RegisterEditableChildObject(importerForFilter);
				}
				return importerForFilter;
			}
		}
		NonPersistentBusinessObjectCollectionHolder<ImporterHolder> importerForFilter;

		[ChildEditable]
		public NonPersistentBusinessObjectCollectionHolder<LocalReferenceNumberHolder> LocalReferenceNumbersForFilter
		{
			get
			{
				if (localReferenceNumbersForFilter == null)
				{
					localReferenceNumbersForFilter = new NonPersistentBusinessObjectCollectionHolder<LocalReferenceNumberHolder>(this, (parent) => new LocalReferenceNumberHolder(parent));
					RegisterEditableChildObject(localReferenceNumbersForFilter);
				}
				return localReferenceNumbersForFilter;
			}
		}
		NonPersistentBusinessObjectCollectionHolder<LocalReferenceNumberHolder> localReferenceNumbersForFilter;

		[ChildEditable]
		public NonPersistentBusinessObjectCollectionHolder<ReceiptNumberHolder> ReceiptNumbersForFilter
		{
			get
			{
				if (receiptNumbersForFilter == null)
				{
					receiptNumbersForFilter = new NonPersistentBusinessObjectCollectionHolder<ReceiptNumberHolder>(this, (parent) => new ReceiptNumberHolder(parent));
					RegisterEditableChildObject(receiptNumbersForFilter);
				}
				return receiptNumbersForFilter;
			}
		}
		NonPersistentBusinessObjectCollectionHolder<ReceiptNumberHolder> receiptNumbersForFilter;

		#endregion

		#region Search Operation

		public void PerformSearch()
		{
			var query = ConstructQuery();
			var eligiblePayInfos = new ActiveBusinessObjectCollection<CusEntryPayInfo>(Factory);
			eligiblePayInfos.AdditionalFilter = query;
			AggregatePayInfosIntoVAT404Document(eligiblePayInfos);
			VAT404Documents.RefreshBinding();
		}

		void AggregatePayInfosIntoVAT404Document(IEnumerable<CusEntryPayInfo> eligiblePayInfos)
		{
			VAT404Documents.RemoveAll();
			foreach (var payInfoGroup in eligiblePayInfos.GroupBy(x => x.Importer))
			{
				VAT404Documents.Add(new VAT404Document(payInfoGroup.Key.PK, payInfoGroup, this));
			}
		}

		#region QueryForSeaching

		ZQuery ConstructQuery()
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryPayInfo));
			ZDBOnlySubQuery queryOfEntryHeader = ConstructEntryHeaderSubQuery();
			query.AddSubQuery(CusEntryPayInfoSchema.C9_CH, queryOfEntryHeader, JoinCondition.And);

			query.AddToFilter(CusEntryPayInfoSchema.C9_PaymentDate, SQLComparisonOperator.GreaterThanOrEqualTo, StartDate);
			query.AddToFilter(CusEntryPayInfoSchema.C9_PaymentDate, SQLComparisonOperator.LessThan, EndDate.AddDays(1));

			query.AddToFilter(CusEntryPayInfoSchema.C9_PaymentAmount, SQLComparisonOperator.NotEqual, ZDecimal.Zero);
			query.AddToFilter(CusEntryPayInfoSchema.C9_TransactionType, SQLComparisonOperator.Equal, UniversalReferenceConstants.TaxOrFeeTypeCode.VAT);

			var receiptNumbers = ReceiptNumbersForFilter.OfType<ReceiptNumberHolder>().Where(x => !x.ReceiptNumber.IsEmpty).Select(x => x.ReceiptNumber);
			if (receiptNumbers.Any())
			{
				query.AddToFilter(CusEntryPayInfoSchema.C9_PaymentReference, SQLComparisonOperator.Equal, receiptNumbers);
			}
			else
			{
				query.AddToFilter(CusEntryPayInfoSchema.C9_PaymentReference, SQLComparisonOperator.NotEqual, ZString.Empty);
			}

			return query;
		}

		ZDBOnlySubQuery ConstructEntryHeaderSubQuery()
		{
			var queryOfEntryHeader = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.PK);
			var lrnNumbers = LocalReferenceNumbersForFilter.OfType<LocalReferenceNumberHolder>().Where(x => !x.LocalReferenceNumber.IsEmpty).Select(x => x.LocalReferenceNumber);
			if (lrnNumbers.Any())
			{
				queryOfEntryHeader.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, SQLComparisonOperator.Equal, lrnNumbers);
			}

			ZDBOnlySubQuery queryOfDeclaration = ConstructJobDeclarationSubQuery();

			queryOfEntryHeader.AddSubQuery(CusEntryHeaderSchema.CH_JE, queryOfDeclaration, JoinCondition.And);
			return queryOfEntryHeader;
		}

		ZDBOnlySubQuery ConstructJobDeclarationSubQuery()
		{
			var result = new ZDBOnlySubQuery(typeof(JobDeclaration), JobDeclarationSchema.PK);
			result.AddToFilter(JobDeclarationSchema.JE_GB, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.Branches.Select(x => x.PK));
			var importerPKs = ImporterForFilter.OfType<ImporterHolder>().Where(x => x.ImporterPK.IsValid).Select(x => x.ImporterPK);
			if (importerPKs.Any())
			{
				result.AddToFilter(JobDeclarationSchema.JE_OH_Importer, SQLComparisonOperator.Equal, importerPKs);
			}
			return result;
		}

		#endregion

		#endregion

		#region Search Result

		[ChildEditable]
		public BusinessObjectCollectionWrapper<VAT404Document> VAT404Documents
		{
			get
			{
				if (vat404Documents == null)
				{
					vat404Documents = new BusinessObjectCollectionWrapper<VAT404Document>();
					RegisterEditableChildObject(vat404Documents);
				}
				return vat404Documents;
			}
		}
		BusinessObjectCollectionWrapper<VAT404Document> vat404Documents;

		#endregion

		#region Delivery

		public bool RunPreDeliverCheck(IMessageNotificationCollector notification)
		{
			var isOKToSend = false;
			if ((VAT404Documents?.Count ?? 0) == 0)
			{
				notification.AddError(Res.GetString("00F779CC-6541-45C6-8070-40BA545F0F4F", "No eligible Proof Of Payment to be delivered"));
			}
			else
			{
				isOKToSend = VAT404Documents.OfType<VAT404Document>().All(x => x.DeliveryContacts.Count > 0);
				if (!isOKToSend)
				{
					notification.AddError(Res.GetString("B85B8FF2-173D-4863-AC98-F527546A435D", @"Some documents to be delivered doesn't have a recipient. 
Please make sure all documents has got the at least one valid recipient to be delivered to."));
				}
			}
			return isOKToSend;
		}

		public void Deliver(IMessageNotificationCollector notification)
		{
			foreach (var item in VAT404Documents.OfType<VAT404Document>())
			{
				item.DeliverDocument(notification);
			}
		}

		#endregion
	}

	public class VAT404DocumentInstructionValidation : AutoVAT404DocumentInstructionValidation
	{
		public VAT404DocumentInstructionValidation(AutoVAT404DocumentInstruction parent) : base(parent)
		{
		}

		public new VAT404DocumentInstruction Parent => base.Parent as VAT404DocumentInstruction;

		protected override void CheckEndDate()
		{
			base.CheckEndDate();
			var targetInfo = Parent.EndDateInfo;
			MandatoryValidation.CheckEntered(targetInfo);
			var startDate = Parent.StartDate;
			var endDate = Parent.EndDate;
			if (startDate.IsValid && endDate.IsValid)
			{
				if (endDate < startDate)
				{
					targetInfo.AddMessageError(Res.GetString("1D8A17C7-D38F-41B2-B7D1-657C9BF23439", "Period End Date shouldn't be before the Start Date."));
				}
				else if (endDate > ZDateTime.Today)
				{
					targetInfo.AddWarning(Res.GetString("EC5543CD-27CC-4D97-9A44-ACC424A04FF2", "Period End Date shouldn't be greater than Today"));
				}
			}
		}

		protected override void CheckStartDate()
		{
			base.CheckStartDate();
			var targetInfo = Parent.StartDateInfo;
			MandatoryValidation.CheckEntered(targetInfo);
			if (Parent.StartDate > ZDateTime.Today)
			{
				targetInfo.AddMessageError(Res.GetString("CDE50365-7681-4225-9697-B29A53FC8BD8", "Period Start Date shouldn't be greater than Today"));
			}
		}

		protected override void CheckPrinterPK()
		{
			base.CheckPrinterPK();
			var targetInfo = Parent.PrinterPKInfo;
			ListValidation.ErrorIfInvalidPK(targetInfo);
			if (Parent.VAT404Documents.OfType<VAT404Document>().Any(x => x.DeliveryContacts.OfType<DocDeliveryContact>().Any(y => y.DeliveryMethod == Core.Constants.ContactNotifyModes.Print)))
			{
				MandatoryValidation.CheckEntered(targetInfo);
			}
		}
	}
}
