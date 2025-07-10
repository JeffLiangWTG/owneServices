using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business
{
	public class AIILine : AutoAIILine,
		ILinePriceCalculationFieldSettingSupporter,
		ICusCodeDataTypeSupporter
	{
		public AIILine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoAIILine.Schema
		{
			public const string BasisUnitPrice = "BasisUnitPrice";
			public const string US_InvQtyUQ = "US_InvQtyUQ";
			public const string US_CustomsQtyUQ = "US_CustomsQtyUQ";
			public const string US_SecondQtyUQ = "US_SecondQtyUQ";
			public const string US_ThirdQtyUQ = "US_ThirdQtyUQ";
			public const string US_Tariff = "US_Tariff";
			public const string US_ExtendedDesc = "US_ExtendedDesc";
			public const string US_RX_NKItemAmountCurr = "US_RX_NKItemAmountCurr";
			public const string US_TSCAName = "US_TSCAName";
			public const string US_TSCAIndicator = "US_TSCAIndicator";
		}

		#region New Properties

		#region US_ArticleNo

		public override ZString US_ArticleNoA
		{
			get
			{
				return this.GetEffectiveValue(base.US_ArticleNoA, JobComInvoiceLine.Schema.US_ArticleNoA);
			}
			set
			{
				base.US_ArticleNoA = this.SetEffectiveValue(value, JobComInvoiceLine.Schema.US_ArticleNoA);
			}
		}

		public override ZString US_ArticleNoB
		{
			get
			{
				return this.GetEffectiveValue(base.US_ArticleNoB, JobComInvoiceLine.Schema.US_ArticleNoB);
			}
			set
			{
				base.US_ArticleNoB = this.SetEffectiveValue(value, JobComInvoiceLine.Schema.US_ArticleNoB);
			}
		}

		#endregion

		#region BasisUnitPrice

		[DecimalPlaces(4)]
		public ZDecimal BasisUnitPrice
		{
			get { return US_UnitPrice + US_98InvCurrPerUnit; }
		}

		public ZPropertyInfo BasisUnitPriceInfo
		{
			get { return GetZPropertyInfo(Schema.BasisUnitPrice); }
		}
		#endregion

		#region US_InvQtyUQ
		[List(nameof(AddInfoLookups) + "." + nameof(USAIILineAddInfoLookups.US_UnitOfMeasureList))]
		public ZString US_InvQtyUQ
		{
			get
			{
				JobComInvoiceLine invoiceLine = Parent;
				return invoiceLine == null ? ZString.Empty : invoiceLine.JI_InvoiceUQ;
			}
		}

		public ZPropertyInfo US_InvQtyUQInfo
		{
			get { return GetZPropertyInfo(Schema.US_InvQtyUQ); }
		}
		#endregion

		#region US_CustomsQtyUQ
		[List(nameof(AddInfoLookups) + "." + nameof(USAIILineAddInfoLookups.US_UnitOfMeasureList))]
		public ZString US_CustomsQtyUQ
		{
			get
			{
				JobComInvoiceLine invoiceLine = Parent;
				return invoiceLine == null ? ZString.Empty : (US_SupLine ? invoiceLine.US_SupUQ1 : invoiceLine.JI_CustomsUnitQty);
			}
		}

		public ZPropertyInfo US_CustomsQtyUQInfo
		{
			get { return GetZPropertyInfo(Schema.US_CustomsQtyUQ); }
		}
		#endregion

		#region US_SecondQtyUQ
		[List(nameof(AddInfoLookups) + "." + nameof(USAIILineAddInfoLookups.US_UnitOfMeasureList))]
		public ZString US_SecondQtyUQ
		{
			get
			{
				JobComInvoiceLine invoiceLine = Parent;
				return invoiceLine == null ? ZString.Empty : (US_SupLine ? invoiceLine.US_SupUQ2 : invoiceLine.JI_CustomsSecondUnitQty);
			}
		}

		public ZPropertyInfo US_SecondQtyUQInfo
		{
			get { return GetZPropertyInfo(Schema.US_SecondQtyUQ); }
		}
		#endregion

		#region US_ThirdQtyUQ
		[List(nameof(AddInfoLookups) + "." + nameof(USAIILineAddInfoLookups.US_UnitOfMeasureList))]
		public ZString US_ThirdQtyUQ
		{
			get
			{
				JobComInvoiceLine invoiceLine = Parent;
				return invoiceLine == null ? ZString.Empty : (US_SupLine ? invoiceLine.US_SupUQ3 : invoiceLine.JI_CustomsThirdUnitQty);
			}
		}

		public ZPropertyInfo US_ThirdQtyUQInfo
		{
			get { return GetZPropertyInfo(Schema.US_ThirdQtyUQ); }
		}
		#endregion

		#region US_Tariff
		public ZString US_Tariff
		{
			get
			{
				JobComInvoiceLine invoiceLine = Parent;
				return invoiceLine == null ? ZString.Empty : (US_SupLine ? invoiceLine.US_SupTariff : invoiceLine.JI_FormattedTariff);
			}
		}

		public ZPropertyInfo US_TariffInfo
		{
			get { return GetZPropertyInfo(Schema.US_Tariff); }
		}
		#endregion

		#region US_Desc
		public override ZString US_Desc
		{
			get { return Parent != null && (IsNonLineGroupingAndFirstAIILineOrOnlyOneAIILine || base.US_Desc.IsEmpty) ? Parent.JI_Description : base.US_Desc; }
			set
			{
				if (IsNonLineGroupingAndFirstAIILineOrOnlyOneAIILine)
				{
					Parent.JI_Description = value;
					base.US_Desc = ZString.Empty;
					DeleteExtendedDescriptionNoteIfNeeded();
				}
				else
				{
					ZString newValue = value;
					if (!newValue.IsEmpty)
					{
						JobComInvoiceLine invoiceLine = Parent;
						if (invoiceLine != null && invoiceLine.JI_Description == value)
						{
							newValue = ZString.Empty;
						}
					}
					base.US_Desc = newValue;
					if (value.IsEmpty)
					{
						DeleteExtendedDescriptionNoteIfNeeded();
					}
				}
			}
		}
		#endregion

		#region US_ExtendedDesc
		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(US_ExtendedDesc_ReadOnly))]
		[MaxLength(StmNote.Schema.ST_NoteTextMaxLength)]
		public ZString US_ExtendedDesc
		{
			get
			{
				ZString result = ZString.Empty;
				if (IsNonLineGroupingAndFirstAIILineOrOnlyOneAIILine || US_ExtendedDesc_ReadOnly)
				{
					JobComInvoiceLine invoiceLine = Parent;
					result = invoiceLine == null ? ZString.Empty : invoiceLine.JI_ExtraInfoForClassification;
				}
				else if (US_IsExtCommDescEnabled)
				{
					result = ExtendedDescriptionNote.ST_NoteText;
				}
				return result;
			}
			set
			{
				ZString oldValue = US_ExtendedDesc;
				if (!US_ExtendedDesc_ReadOnly)
				{
					if (IsNonLineGroupingAndFirstAIILineOrOnlyOneAIILine)
					{
						Parent.JI_ExtraInfoForClassification = value;
						US_IsExtCommDescEnabled = false;
					}
					else
					{
						if (!value.IsEmpty)
						{
							JobComInvoiceLine invoiceLine = Parent;
							if (invoiceLine != null && invoiceLine.JI_ExtraInfoForClassification == value)
							{
								DeleteExtendedDescriptionNoteIfNeeded();
								value = ZString.Empty;
							}
							else
							{
								US_IsExtCommDescEnabled = true;
							}
						}
						else
						{
							DeleteExtendedDescriptionNoteIfNeeded();
						}
						if (US_IsExtCommDescEnabled)
						{
							ExtendedDescriptionNote.ST_NoteText = value;
						}
					}
				}
				US_ExtendedDescInfo.RefreshBinding(oldValue);
			}
		}

		bool US_ExtendedDesc_ReadOnly
		{
			get { return IsNonLineGroupingAndFirstAIILineOrOnlyOneAIILine ? US_Desc.IsEmpty : base.US_Desc.IsEmpty; }
		}

		public ZPropertyInfo US_ExtendedDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_ExtendedDesc); }
		}
		#endregion

		#region US_RX_NKItemAmountCurr
		[List(nameof(AddInfoLookups) + "." + nameof(USAIILineAddInfoLookups.CurrencyList))]
		public ZString US_RX_NKItemAmountCurr
		{
			get
			{
				JobComInvoiceLine invoiceLine = Parent;
				return invoiceLine == null ? ZString.Empty : invoiceLine.JI_RX_NKLinePriceCurr;
			}
		}

		public ZPropertyInfo US_RX_NKItemAmountCurrInfo
		{
			get { return GetZPropertyInfo(Schema.US_RX_NKItemAmountCurr); }
		}
		#endregion

		#region US_TSCAName
		public ZString US_TSCAName
		{
			get
			{
				JobComInvoiceLine invoiceLine = Parent;
				return invoiceLine == null ? ZString.Empty : invoiceLine.US_TSCAName;
			}
		}

		public ZPropertyInfo US_TSCANameInfo
		{
			get { return GetZPropertyInfo(Schema.US_TSCAName); }
		}
		#endregion

		#region US_TSCAIndicator
		public ZString US_TSCAIndicator
		{
			get
			{
				JobComInvoiceLine invoiceLine = Parent;
				return invoiceLine == null ? ZString.Empty : invoiceLine.US_TSCAIndicator;
			}
		}

		public ZPropertyInfo US_TSCAIndicatorInfo
		{
			get { return GetZPropertyInfo(Schema.US_TSCAIndicator); }
		}
		#endregion

		#endregion

		[BusinessObjectTestExclude]
		public override ZString B7_ParentTableCode
		{
			get { return base.B7_ParentTableCode; }
			set
			{
				if (!value.IsEmpty && value != JobComInvoiceLineSchema.Constants.Prefix)
				{
					throw new NotSupportedException("Setting AIILine.B7_ParentTableCode is not supported.");
				}
				base.B7_ParentTableCode = value;
			}
		}

		[BusinessObjectTestExclude]
		public override ZGuid B7_ParentID
		{
			get { return base.B7_ParentID; }
			set
			{
				if (!IsCopying && value.IsEmpty)
				{
					b7_ParentIDCachedOnRelationshipResetByCore = base.B7_ParentID;
				}
				ZGuid oldValue = B7_ParentID;
				base.B7_ParentID = value;
				if (!IsCopying && oldValue != B7_ParentID)
				{
					if (!oldValue.IsEmpty && !B7_ParentID.IsEmpty)
					{
						throw new NotSupportedException("Setting AIILine.B7_ParentID is not supported.");
					}
				}
			}
		}
		ZGuid b7_ParentIDCachedOnRelationshipResetByCore;

		public bool IsSettingInvAmount
		{
			get { return IsFieldSettingInProgress(USLinePriceCalculationFieldSettingType.AIILineItemAmount); }
		}

		public ZDecimal US_InvAmountInLocalCurrency
		{
			get { return US_InvAmountInLocalCurrencyMoney.Amount; }
		}

		public Money US_InvAmountMoney
		{
			get
			{
				Money result = Money.Empty;
				JobComInvoiceLine invoiceLine = Parent;
				if (invoiceLine != null)
				{
					JobComInvoiceHeader invoice = invoiceLine.InvoiceHeader;
					if (invoice != null)
					{
						result = new Money(US_InvAmount, invoice.Invoice_Currency);
						if (invoiceLine.IsInvoiceCurrExRateUserEnterable)
						{
							result = ConvertToLocalAmountExact(result);
						}
					}
				}
				return result;
			}
		}

		public Money US_InvAmountInLocalCurrencyMoney
		{
			get { return ConvertToLocalAmountExact(US_InvAmountMoney); }
		}

		[ReadOnlyMember(nameof(US_LineNo_ReadOnly))]
		public override ZShort US_LineNo
		{
			get { return base.US_LineNo; }
			set { base.US_LineNo = value; }
		}

		protected bool US_LineNo_ReadOnly
		{
			get { return true; }
		}

		public override ZDecimal US_CustomsQty
		{
			get { return IsNonLineGroupingAndFirstAIILineOrOnlyOneAIILine ? Parent.JI_CustomsQuantity : base.US_CustomsQty; }
			set
			{
				if (IsNonLineGroupingAndFirstAIILineOrOnlyOneAIILine)
				{
					Parent.JI_CustomsQuantity = value;
					base.US_CustomsQty = ZDecimal.Zero;
				}
				else
				{
					base.US_CustomsQty = value;
				}
			}
		}

		[ReadOnlyMember(nameof(US_CustomsQty_ReadOnly))]
		protected bool US_CustomsQty_ReadOnly
		{
			get
			{
				JobComInvoiceLine invoiceLine = Parent;
				return invoiceLine == null || US_SupLine && invoiceLine.US_SupQty1_ReadOnly || !US_SupLine && invoiceLine.JI_CustomsQuantity_ReadOnly;
			}
		}

		[ReadOnlyMember(nameof(US_SecondQty_ReadOnly))]
		public override ZDecimal US_SecondQty
		{
			get { return IsNonLineGroupingAndFirstAIILineOrOnlyOneAIILine ? Parent.JI_CustomsSecondQuantity : base.US_SecondQty; }
			set
			{
				if (IsNonLineGroupingAndFirstAIILineOrOnlyOneAIILine)
				{
					if (Parent.JI_CustomsSecondQuantity != value)
					{
						Parent.JI_CustomsSecondQuantity = value;
					}
					base.US_SecondQty = ZDecimal.Zero;
				}
				else
				{
					base.US_SecondQty = value;
				}
			}
		}

		protected bool US_SecondQty_ReadOnly
		{
			get
			{
				JobComInvoiceLine invoiceLine = Parent;
				return invoiceLine == null || US_SupLine && invoiceLine.US_SupQty2_ReadOnly || !US_SupLine && invoiceLine.JI_CustomsSecondQuantity_ReadOnly;
			}
		}

		[ReadOnlyMember(nameof(US_ThirdQty_ReadOnly))]
		public override ZDecimal US_ThirdQty
		{
			get { return IsNonLineGroupingAndFirstAIILineOrOnlyOneAIILine ? Parent.JI_CustomsThirdQuantity : base.US_ThirdQty; }
			set
			{
				if (IsNonLineGroupingAndFirstAIILineOrOnlyOneAIILine)
				{
					Parent.JI_CustomsThirdQuantity = value;
					base.US_ThirdQty = ZDecimal.Zero;
				}
				else
				{
					base.US_ThirdQty = value;
				}
			}
		}

		protected bool US_ThirdQty_ReadOnly
		{
			get
			{
				JobComInvoiceLine invoiceLine = Parent;
				return invoiceLine == null || US_SupLine && invoiceLine.US_SupQty3_ReadOnly || !US_SupLine && invoiceLine.JI_CustomsThirdQuantity_ReadOnly;
			}
		}

		public new JobComInvoiceLine Parent
		{
			get
			{
				if (fParent == null || fParent.PK != B7_ParentID)
				{
					ZGuid reference = B7_ParentID.IsEmpty ? b7_ParentIDCachedOnRelationshipResetByCore : B7_ParentID;
					fParent = Factory.Load<JobComInvoiceLine>(reference);
				}
				return fParent != null && !fParent.IsDeleted ? fParent : null;
			}
		}
		JobComInvoiceLine fParent;

		[ChildEditable(true)]
		public RegoNumberCollection RegoNumbers
		{
			get
			{
				if (fRegoNumbers == null)
				{
					fRegoNumbers = new RegoNumberCollection(this);
					fRegoNumbers.Load();
					RegisterEditableChildObject(fRegoNumbers);
				}
				return fRegoNumbers;
			}
		}
		RegoNumberCollection fRegoNumbers;

		public override void Delete()
		{
			if (!IsDeleted)
			{
				RegoNumbers.RemoveAndDeleteAll();
			}
			base.Delete();
		}

		#region Implementation

		protected T GetEffectiveValue<T>(T baseValue, string parentFieldName) where T : IZType
		{
			T result = baseValue;

			if (result.IsEmpty && this.Parent != null)
			{
				result = (T)this.Parent[parentFieldName];
			}

			return result;
		}

		protected T SetEffectiveValue<T>(T valuePassed, string parentFieldName) where T : IZType
		{
			T result = valuePassed;

			if (!result.IsEmpty && this.Parent != null && this.Parent[parentFieldName].Equals(valuePassed))
			{
				result = (T)valuePassed.Default;
			}
			if (this.IsNonLineGroupingAndFirstAIILineOrOnlyOneAIILine)
			{
				this.Parent[parentFieldName] = result;
				result = (T)result.Default;
			}

			return result;
		}

		protected override bool US_98GoodsValue_ReadOnlyCore
		{
			get
			{
				JobComInvoiceLine invoiceLine = Parent;
				return invoiceLine != null && invoiceLine.US_98GoodsValue_ReadOnly;
			}
		}

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				NoteTypeCollection noteTypes = base.NoteTypesCore;
				noteTypes.Add(PredefinedNoteTypes.Instance.ExtendedCommercialDescription);
				return noteTypes;
			}
		}

		void DeleteExtendedDescriptionNoteIfNeeded()
		{
			if (US_IsExtCommDescEnabled)
			{
				ExtendedDescriptionNote.Delete();
				US_IsExtCommDescEnabled = false;
			}
		}

		StmNote ExtendedDescriptionNote
		{
			get
			{
				if (fExtendedDescriptionNote == null || fExtendedDescriptionNote.IsDeleted)
				{
					fExtendedDescriptionNote = null;
					foreach (StmNote extendedDescriptionNote in Notes.FindByDescription(PredefinedNoteTypes.Instance.ExtendedCommercialDescription.Description))
					{
						if (!extendedDescriptionNote.IsDeleted)
						{
							fExtendedDescriptionNote = extendedDescriptionNote;
							break;
						}
					}
					if (fExtendedDescriptionNote == null)
					{
						fExtendedDescriptionNote = Notes.AddNew(false, PredefinedNoteTypes.Instance.ExtendedCommercialDescription.Description, ZString.Empty);
					}
				}
				return fExtendedDescriptionNote;
			}
		}
		StmNote fExtendedDescriptionNote;

		protected override bool IsDataEmpty
		{
			get { return LineGroupRef == null; }
		}

		internal bool IsNonLineGroupingAndFirstAIILineOrOnlyOneAIILine
		{
			get
			{
				JobComInvoiceLine invoiceLine = Parent;
				return invoiceLine != null
					&& invoiceLine.FirstAIILine == this
					&& invoiceLine.IsNonLineGroupingOrOnlyOneAIILine;
			}
		}

		Money ConvertToLocalAmountExact(Money moneyAmount)
		{
			Money result = Money.Empty;
			JobComInvoiceLine invoiceLine = Parent;
			if (invoiceLine != null)
			{
				CurrencyConverter converter = invoiceLine.CurrencyConverter;
				if (converter != null)
				{
					result = converter.ConvertExact(moneyAmount, JobDeclaration.GetLocalCurrency());
				}
			}

			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			base.B7_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			base.US_UnitBasis = 1;
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			Factory.AddFetchHint(CusCodeDataSchema.PK, US_CY_LineGroupRef);
		}

		#endregion

		#region ILinePriceCalculationFieldSettingSupporter

		void ILinePriceCalculationFieldSettingSupporter.Start(object type)
		{
			if (type != null)
			{
				if (!FieldSettingTypes.ContainsKey(type))
				{
					FieldSettingTypes.Add(type, 0);
				}
				FieldSettingTypes[type]++;
			}
		}

		void ILinePriceCalculationFieldSettingSupporter.Stop(object type)
		{
			if (type != null)
			{
				if (FieldSettingTypes.ContainsKey(type))
				{
					FieldSettingTypes[type]--;
				}
			}
		}

		bool IsFieldSettingInProgress(object type)
		{
			int index;
			return FieldSettingTypes.TryGetValue(type, out index) && index > 0;
		}

		Dictionary<object, int> FieldSettingTypes
		{
			get { return fieldSettingTypes ?? (fieldSettingTypes = new Dictionary<object, int>()); }
		}
		Dictionary<object, int> fieldSettingTypes;

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.RegoNumber, typeof(RegoNumber));
			return result;
		}

		#endregion
	}
}
