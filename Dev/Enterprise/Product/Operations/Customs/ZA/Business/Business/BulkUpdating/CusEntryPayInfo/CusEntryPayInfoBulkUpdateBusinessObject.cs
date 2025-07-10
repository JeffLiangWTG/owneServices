using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public class CusEntryPayInfoBulkUpdateBusinessObject : NonPersistentBusinessObject
	{
		public static class Schema
		{
			public const string ReceiptNumber = "ReceiptNumber";
			public const string ReceiptDate = "ReceiptDate";
			public const int ReceiptNumberMaxLength = 20;
		}

		public CusEntryPayInfoBulkUpdateBusinessObject(BusinessObjectFactory factory) : base(factory)
		{
		}

		[ChildEditable(true)]
		public CusEntryPayInfoBulkUpdateCollection SelectedPayInfos
		{
			get
			{
				if (fSelectedPayInfos == null)
				{
					fSelectedPayInfos = new CusEntryPayInfoBulkUpdateCollection(Factory);
					RegisterEditableChildObject(fSelectedPayInfos);
				}
				return fSelectedPayInfos;
			}
		}
		CusEntryPayInfoBulkUpdateCollection fSelectedPayInfos;

		#region Saving

		internal void SaveForTest() => OnFactorySaving();
		protected override void OnFactorySaving()
		{
			UpdateAllPayInfos();
		}

		void UpdateAllPayInfos()
		{
			foreach (CusEntryPayInfo payInfo in this.SelectedPayInfos)
			{
				payInfo.C9_PaymentReference = ReceiptNumber;
				payInfo.C9_ReceiptDate = ReceiptDate;
			}
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public CusEntryPayInfoBulkUpdateBusinessObjectValidation Validation
		{
			get { return GetNewValidation(); }
		}

		CusEntryPayInfoBulkUpdateBusinessObjectValidation GetNewValidation()
		{
			return new CusEntryPayInfoBulkUpdateBusinessObjectValidation(this);
		}

		#endregion

		public ZDate ReceiptDate
		{
			get { return fReceiptDate; }
			set
			{
				SetNonPersistentPropertyValue(ReceiptDateInfo, ref fReceiptDate, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateReceiptDate();
				}
			}
		}
		ZDate fReceiptDate;

		public ZPropertyInfo ReceiptDateInfo => GetZPropertyInfo(Schema.ReceiptDate);

		[MaxLength(CusEntryPayInfoBulkUpdateBusinessObject.Schema.ReceiptNumberMaxLength)]
		public ZString ReceiptNumber
		{
			get { return fReceiptNumber; }
			set
			{
				CheckMaximumLength(ReceiptNumberInfo, value);
				SetNonPersistentPropertyValue(ReceiptNumberInfo, ref fReceiptNumber, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateReceiptNumber();
				}
			}
		}
		ZString fReceiptNumber;

		public ZPropertyInfo ReceiptNumberInfo => GetZPropertyInfo(Schema.ReceiptNumber);
	}
}
