using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.ZA.Business
{
	public class ZAMessage : EDIMessage,
		Integration.Customs.ZA.IZAMessage,
		IVOCAfterValues,
		IVOCBeforeValues,
		ICusCodeDataTypeSupporter
	{
		public new class Schema : EDIMessage.Schema
		{
			public const string Description = "Description";
			public const string CIFValueBefore = "CIFValueBefore";
			public const string CIFValueAfter = "CIFValueAfter";
			public const string CustomsValueBefore = "CustomsValueBefore";
			public const string CustomsValueAfter = "CustomsValueAfter";
			public const string CustomsDutyNoS1P2BBefore = "CustomsDutyNoS1P2BBefore";
			public const string CustomsDutyNoS1P2BAfter = "CustomsDutyNoS1P2BAfter";
			public const string S1P2BDutyBefore = "S1P2BDutyBefore";
			public const string S1P2BDutyAfter = "S1P2BDutyAfter";
			public const string ValueAddedTaxBefore = "ValueAddedTaxBefore";
			public const string ValueAddedTaxAfter = "ValueAddedTaxAfter";
			public const string ProvisionalPaymentAmountBefore = "ProvisionalPaymentAmountBefore";
			public const string ProvisionalPaymentAmountAfter = "ProvisionalPaymentAmountAfter";
			public const string PenaltyAmountBefore = "PenaltyAmountBefore";
			public const string PenaltyAmountAfter = "PenaltyAmountAfter";
			public const string ParentMessageNumber = "ParentMessageNumber";
			public const string LocalReferenceNumber = "LocalReferenceNumber";
		}

		public ZAMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Override

		public new static readonly ZAMessageTypeDecider TypeDecider = new ZAMessageTypeDecider();

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.SouthAfricanCustoms;
		}

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var result = base.NoteTypesCore;
				result.Add(PredefinedNoteTypes.Instance.ZAVOCReason);
				return result;
			}
		}

		protected override EDIMessageValidation GetNewValidation() => new ZAEDIMessageValidation(this);

		public new ZAEDIMessageValidation Validation => (ZAEDIMessageValidation)base.Validation;

		#endregion

		#region VOCReason

		public ZString VOCReason
		{
			get { return VOCReasonNote?.ST_NoteText ?? ZString.Empty; }
		}

		internal void SetVOCReason(ZString vOCReason)
		{
			if (VOCReasonNote == null)
			{
				vocReasonNote = this.Notes.AddNew(false, PredefinedNoteTypes.Instance.ZAVOCReason.ToString(), vOCReason);
			}
		}

		StmNote VOCReasonNote
		{
			get
			{
				if (vocReasonNote == null)
				{
					var notes = Notes.FindByDescription(PredefinedNoteTypes.Instance.ZAVOCReason.Description);
					vocReasonNote = notes.Length > 0 ? notes[0] : null;
				}
				return vocReasonNote;
			}
		}
		StmNote vocReasonNote;

		#endregion

		public virtual ZString EntryStatus => ZString.Empty;

		public ZDateTime PreparationDate
		{
			get
			{
				if (preparationDate == null)
				{
					var interchange = Interchange;
					var dateTimePrep = interchange?.UNB?.DateTimeOfPreparation ?? new Edifact.Generic.DateTimeOfPreparationElements();
					ZDateTime date;
					preparationDate = ZDateTime.TryParseExact(dateTimePrep.Date + dateTimePrep.Time, out date, "yyyyMMddHHmm") ? date : ZDateTime.TryParseExact(dateTimePrep.Date + dateTimePrep.Time, out date, "yyMMddHHmm") ? date : ZDateTime.Invalid;
				}
				return preparationDate.Value;
			}
		}
		ZDateTime? preparationDate;

		public override ZGuid EM_EI
		{
			get { return base.EM_EI; }
			set
			{
				var oldValue = EM_EI;
				base.EM_EI = value;
				if (!IsCopying && oldValue != EM_EI)
				{
					preparationDate = null;
				}
			}
		}

		public virtual ZString EntryStatusDescription => ZString.Empty;

		#region CIFValue
		public ZDecimal CIFValueBefore
		{
			get { return VoucherOfCorrectionValueBefores.GetValue(VOCValueTypeList.Codes.CIFValue); }
			set { VoucherOfCorrectionValueBefores.SetValue(VOCValueTypeList.Codes.CIFValue, value, CIFValueBeforeInfo); }
		}

		public ZPropertyInfo CIFValueBeforeInfo => GetZPropertyInfo(Schema.CIFValueBefore);

		public ZDecimal CIFValueAfter
		{
			get { return VoucherOfCorrectionValueAfters.GetValue(VOCValueTypeList.Codes.CIFValue); }
			set { VoucherOfCorrectionValueAfters.SetValue(VOCValueTypeList.Codes.CIFValue, value, CIFValueAfterInfo); }
		}

		public ZPropertyInfo CIFValueAfterInfo => GetZPropertyInfo(Schema.CIFValueAfter);
		#endregion

		#region CustomsValue
		public ZDecimal CustomsValueBefore
		{
			get { return VoucherOfCorrectionValueBefores.GetValue(VOCValueTypeList.Codes.CustomsValue); }
			set { VoucherOfCorrectionValueBefores.SetValue(VOCValueTypeList.Codes.CustomsValue, value, CustomsValueBeforeInfo); }
		}

		public ZPropertyInfo CustomsValueBeforeInfo => GetZPropertyInfo(Schema.CustomsValueBefore);

		public ZDecimal CustomsValueAfter
		{
			get { return VoucherOfCorrectionValueAfters.GetValue(VOCValueTypeList.Codes.CustomsValue); }
			set { VoucherOfCorrectionValueAfters.SetValue(VOCValueTypeList.Codes.CustomsValue, value, CustomsValueAfterInfo); }
		}

		public ZPropertyInfo CustomsValueAfterInfo => GetZPropertyInfo(Schema.CustomsValueAfter);
		#endregion

		#region CustomsDutyNoS1P2B
		public ZDecimal CustomsDutyNoS1P2BBefore
		{
			get { return VoucherOfCorrectionValueBefores.GetValue(VOCValueTypeList.Codes.Duty); }
			set { VoucherOfCorrectionValueBefores.SetValue(VOCValueTypeList.Codes.Duty, value, CustomsDutyNoS1P2BBeforeInfo); }
		}

		public ZPropertyInfo CustomsDutyNoS1P2BBeforeInfo => GetZPropertyInfo(Schema.CustomsDutyNoS1P2BBefore);

		public ZDecimal CustomsDutyNoS1P2BAfter
		{
			get { return VoucherOfCorrectionValueAfters.GetValue(VOCValueTypeList.Codes.Duty); }
			set { VoucherOfCorrectionValueAfters.SetValue(VOCValueTypeList.Codes.Duty, value, CustomsDutyNoS1P2BAfterInfo); }
		}

		public ZPropertyInfo CustomsDutyNoS1P2BAfterInfo => GetZPropertyInfo(Schema.CustomsDutyNoS1P2BAfter);
		#endregion

		#region S1P2BDuty
		public ZDecimal S1P2BDutyBefore
		{
			get { return VoucherOfCorrectionValueBefores.GetValue(VOCValueTypeList.Codes.S1P2BDuty); }
			set { VoucherOfCorrectionValueBefores.SetValue(VOCValueTypeList.Codes.S1P2BDuty, value, S1P2BDutyBeforeInfo); }
		}

		public ZPropertyInfo S1P2BDutyBeforeInfo => GetZPropertyInfo(Schema.S1P2BDutyBefore);

		public ZDecimal S1P2BDutyAfter
		{
			get { return VoucherOfCorrectionValueAfters.GetValue(VOCValueTypeList.Codes.S1P2BDuty); }
			set { VoucherOfCorrectionValueAfters.SetValue(VOCValueTypeList.Codes.S1P2BDuty, value, S1P2BDutyAfterInfo); }
		}

		public ZPropertyInfo S1P2BDutyAfterInfo => GetZPropertyInfo(Schema.S1P2BDutyAfter);
		#endregion

		#region ValueAddedTax
		public ZDecimal ValueAddedTaxBefore
		{
			get { return VoucherOfCorrectionValueBefores.GetValue(VOCValueTypeList.Codes.VAT); }
			set { VoucherOfCorrectionValueBefores.SetValue(VOCValueTypeList.Codes.VAT, value, ValueAddedTaxBeforeInfo); }
		}

		public ZPropertyInfo ValueAddedTaxBeforeInfo => GetZPropertyInfo(Schema.ValueAddedTaxBefore);

		public ZDecimal ValueAddedTaxAfter
		{
			get { return VoucherOfCorrectionValueAfters.GetValue(VOCValueTypeList.Codes.VAT); }
			set { VoucherOfCorrectionValueAfters.SetValue(VOCValueTypeList.Codes.VAT, value, ValueAddedTaxAfterInfo); }
		}

		public ZPropertyInfo ValueAddedTaxAfterInfo => GetZPropertyInfo(Schema.ValueAddedTaxAfter);
		#endregion

		#region ProvisionalPaymentAmount

		public ZDecimal ProvisionalPaymentAmountBefore
		{
			get { return VoucherOfCorrectionValueBefores.GetValue(VOCValueTypeList.Codes.ProvisionalPayment); }
			set { VoucherOfCorrectionValueBefores.SetValue(VOCValueTypeList.Codes.ProvisionalPayment, value, ProvisionalPaymentAmountBeforeInfo); }
		}

		public ZPropertyInfo ProvisionalPaymentAmountBeforeInfo => GetZPropertyInfo(Schema.ProvisionalPaymentAmountBefore);

		public ZDecimal ProvisionalPaymentAmountAfter
		{
			get { return VoucherOfCorrectionValueAfters.GetValue(VOCValueTypeList.Codes.ProvisionalPayment); }
			set { VoucherOfCorrectionValueAfters.SetValue(VOCValueTypeList.Codes.ProvisionalPayment, value, ProvisionalPaymentAmountAfterInfo); }
		}

		public ZPropertyInfo ProvisionalPaymentAmountAfterInfo => GetZPropertyInfo(Schema.ProvisionalPaymentAmountAfter);

		#endregion

		#region PenaltyAmount

		public ZDecimal PenaltyAmountBefore
		{
			get { return VoucherOfCorrectionValueBefores.GetValue(VOCValueTypeList.Codes.Penalty); }
			set { VoucherOfCorrectionValueBefores.SetValue(VOCValueTypeList.Codes.Penalty, value, PenaltyAmountBeforeInfo); }
		}

		public ZPropertyInfo PenaltyAmountBeforeInfo => GetZPropertyInfo(Schema.PenaltyAmountBefore);

		public ZDecimal PenaltyAmountAfter
		{
			get { return VoucherOfCorrectionValueAfters.GetValue(VOCValueTypeList.Codes.Penalty); }
			set { VoucherOfCorrectionValueAfters.SetValue(VOCValueTypeList.Codes.Penalty, value, PenaltyAmountAfterInfo); }
		}

		public ZPropertyInfo PenaltyAmountAfterInfo => GetZPropertyInfo(Schema.PenaltyAmountAfter);

		#endregion

		#region ParentMessageNumber

		public virtual ZString ParentMessageNumber => ZString.Empty;

		public ZPropertyInfo ParentMessageNumberInfo => GetZPropertyInfo(Schema.ParentMessageNumber);

		#endregion

		#region LocalReferenceNumber

		public virtual ZString LocalReferenceNumber => ZString.Empty;

		public ZPropertyInfo LocalReferenceNumberInfo => GetZPropertyInfo(Schema.LocalReferenceNumber);

		#endregion

		[ChildEditable]
		public VoucherOfCorrectionValueAfterCollection<ZAMessage> VoucherOfCorrectionValueAfters
		{
			get
			{
				if (voucherOfCorrectionValueAfters == null)
				{
					voucherOfCorrectionValueAfters = new VoucherOfCorrectionValueAfterCollection<ZAMessage>(this);
					voucherOfCorrectionValueAfters.Load();
					RegisterEditableChildObject(voucherOfCorrectionValueAfters);
				}
				return voucherOfCorrectionValueAfters;
			}
		}
		VoucherOfCorrectionValueAfterCollection<ZAMessage> voucherOfCorrectionValueAfters;

		[ChildEditable]
		public VoucherOfCorrectionValueBeforeCollection<ZAMessage> VoucherOfCorrectionValueBefores
		{
			get
			{
				if (voucherOfCorrectionValueBefores == null)
				{
					voucherOfCorrectionValueBefores = new VoucherOfCorrectionValueBeforeCollection<ZAMessage>(this);
					voucherOfCorrectionValueBefores.Load();
					RegisterEditableChildObject(voucherOfCorrectionValueBefores);
				}
				return voucherOfCorrectionValueBefores;
			}
		}
		VoucherOfCorrectionValueBeforeCollection<ZAMessage> voucherOfCorrectionValueBefores;

		#region IVOCAfterValues

		ZDecimal IVOCAfterValues.CIFValue
		{
			get { return CIFValueAfter; }
		}

		ZDecimal IVOCAfterValues.CustomsValue
		{
			get { return CustomsValueAfter; }
		}

		ZDecimal IVOCAfterValues.CustomsDutyNoS1P2B
		{
			get { return CustomsDutyNoS1P2BAfter; }
		}

		ZDecimal IVOCAfterValues.S1P2BDuty
		{
			get { return S1P2BDutyAfter; }
		}

		ZDecimal IVOCAfterValues.ValueAddedTax
		{
			get { return ValueAddedTaxAfter; }
		}

		ZDecimal IVOCAfterValues.ProvisionalPaymentAmount
		{
			get { return ProvisionalPaymentAmountAfter; }
		}

		ZDecimal IVOCAfterValues.PenaltyAmount
		{
			get { return PenaltyAmountAfter; }
		}

		#endregion

		#region IVOCBeforeValues

		ZDecimal IVOCBeforeValues.CIFValue
		{
			get { return CIFValueBefore; }
		}

		ZDecimal IVOCBeforeValues.CustomsValue
		{
			get { return CustomsValueBefore; }
		}

		ZDecimal IVOCBeforeValues.CustomsDutyNoS1P2B
		{
			get { return CustomsDutyNoS1P2BBefore; }
		}

		ZDecimal IVOCBeforeValues.S1P2BDuty
		{
			get { return S1P2BDutyBefore; }
		}

		ZDecimal IVOCBeforeValues.ValueAddedTax
		{
			get { return ValueAddedTaxBefore; }
		}

		ZDecimal IVOCBeforeValues.ProvisionalPaymentAmount
		{
			get { return ProvisionalPaymentAmountBefore; }
		}

		ZDecimal IVOCBeforeValues.PenaltyAmount
		{
			get { return PenaltyAmountBefore; }
		}

		#endregion

		internal void CopyVOCValues(IVOCBeforeValues beforeValues, IVOCAfterValues afterValues)
		{
			CIFValueAfter = afterValues.CIFValue;
			CIFValueBefore = beforeValues.CIFValue;
			CustomsValueAfter = afterValues.CustomsValue;
			CustomsValueBefore = beforeValues.CustomsValue;
			CustomsDutyNoS1P2BAfter = afterValues.CustomsDutyNoS1P2B;
			CustomsDutyNoS1P2BBefore = beforeValues.CustomsDutyNoS1P2B;
			S1P2BDutyAfter = afterValues.S1P2BDuty;
			S1P2BDutyBefore = beforeValues.S1P2BDuty;
			ValueAddedTaxAfter = afterValues.ValueAddedTax;
			ValueAddedTaxBefore = beforeValues.ValueAddedTax;
			ProvisionalPaymentAmountAfter = afterValues.ProvisionalPaymentAmount;
			ProvisionalPaymentAmountBefore = beforeValues.ProvisionalPaymentAmount;
			PenaltyAmountAfter = afterValues.PenaltyAmount;
			PenaltyAmountBefore = beforeValues.PenaltyAmount;
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies() => GetAdditionalBusinessObjectFetchStrategies();

		protected virtual IEnumerable<IBusinessObjectFetchStrategy> GetAdditionalBusinessObjectFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return GetCusCodeDataTypesCore();
		}

		protected virtual IDictionary<ZString, Type> GetCusCodeDataTypesCore()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.VOCValueAfter, typeof(VoucherOfCorrectionValueAfter));
			result.Add(CusCodeDataTypeList.Codes.VOCValueBefore, typeof(VoucherOfCorrectionValueBefore));
			return result;
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}
	}
}
