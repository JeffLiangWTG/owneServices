using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business
{
	/// <summary>
	/// Summary description for DocumentShipment.
	/// </summary>

	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	public class DocumentShipment : NonPersistentBusinessObject, IObsoleteValidation, IDefaultNumberOfDecimalsSupporter
	{
		#region Schema

		public static class Schema
		{
			public const string IncludeConsignee = "IncludeConsignee";
			public const string IncludeConsignor = "IncludeConsignor";
			public const string IncludeNone = "IncludeNone";
			public const string IncludeSendingAgent = "IncludeSendingAgent";
			public const string NumberOfLabelsToPrint = "NumberOfLabelsToPrint";
			public const string LabelRangeFrom = "LabelRangeFrom";
			public const string LabelRangeTo = "LabelRangeTo";
			public const string TotalNumberOfLabelsDescription = "TotalNumberOfLabelsDescription";
			public const string TotalNumberOfLabels = "TotalNumberOfLabels";

			public const string OldMarksAndNumbers = "OldMarksAndNumbers";
			public const string OldGoodsDescription = "OldGoodsDescription";
			public const string OldWeight = "OldWeight";
			public const string OldWeightUnit = "OldWeightUnit";
			public const string OldVolume = "OldVolume";
			public const string OldVolumeUnit = "OldVolumeUnit";
			public const string NewMarksAndNumbers = "NewMarksAndNumbers";
			public const string NewGoodsDescription = "NewGoodsDescription";
			public const string NewWeight = "NewWeight";
			public const string NewWeightUnit = "NewWeightUnit";
			public const string NewVolume = "NewVolume";
			public const string NewVolumeUnit = "NewVolumeUnit";
			public const string ChangeMarksAndNumbers = "ChangeMarksAndNumbers";
			public const string ChangeGoodsDescription = "ChangeGoodsDescription";
			public const string ChangeWeight = "ChangeWeight";
			public const string ChangeVolume = "ChangeVolume";

			public const string Debtor = "Debtor";
			public const string DebtorsToPrint = "DebtorsToPrint";

			public const string ConfirmationToPrint = "ConfirmationToPrint";
			public const string IsBillOfLading = "IsBillOfLading";
		}

		#endregion

		public DocumentShipment(CommonShipment shipment, Constants.DataContext dataContext, bool isBillOfLading = false)
			: base(shipment.Factory)
		{
			fShipment = shipment;
			IsBillOfLading = isBillOfLading;
			DataContext = dataContext;
		}

		public Constants.DataContext DataContext { get; }

		public void SetDefaultsFromDataContext()
		{
			if (DataContext == Constants.DataContext.FreightLabels
				|| DataContext == Constants.DataContext.GenericFreightJobByPackages
				|| DataContext == Constants.DataContext.GenericFreightJobByPackages1Doc
				|| DataContext == Constants.DataContext.GenericFreightJobBySelectedPackages
				|| DataContext == Constants.DataContext.GenericFreightJobBySelectedPkgs1Doc)
			{
				IncludeConsignee = ZBool.True;
				NumberOfLabelsToPrint = Shipment.JS_OuterPacks;
				LabelRangeFrom = 1;
				LabelRangeTo = Shipment.JS_OuterPacks;
			}
			else if (DataContext == Constants.DataContext.LetterOfIndemnity)
			{
				OldMarksAndNumbers = Shipment.JS_MarksAndNumbers;
				OldGoodsDescription = Shipment.DetailedGoodsDescriptionNoteText.SubstringSafe(0, OldGoodsDescriptionInfo.MaxLength);
				OldWeight = Shipment.JS_ActualWeight;
				OldWeightUnit = Shipment.JS_UnitOfWeight;
				OldVolume = Shipment.JS_ActualVolume;
				OldVolumeUnit = Shipment.JS_UnitOfVolume;
				ChangeMarksAndNumbers = false;
				ChangeGoodsDescription = false;
				ChangeWeight = false;
				ChangeVolume = false;
			}
			else if (DataContext == Constants.DataContext.ChargeSheet)
			{
				fDebtorsToPrint = Shipment.Debtors;
			}
		}

		public CommonShipment Shipment
		{
			get { return fShipment; }
		}
		protected CommonShipment fShipment;

		public Transport SelectedTransport;

		#region Properties for binding

		#region Freight Labels

		#region Include Consignee

		public ZBool IncludeConsignee
		{
			get { return fIncludeConsignee; }
			set
			{
				fIncludeConsignee = value;
				IncludeConsigneeInfo.RefreshBinding();
			}
		}
		ZBool fIncludeConsignee;

		public ZPropertyInfo IncludeConsigneeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.IncludeConsignee); }
		}

		#endregion

		#region Include Consignor

		public ZBool IncludeConsignor
		{
			get { return fIncludeConsignor; }
			set
			{
				fIncludeConsignor = value;
				IncludeConsignorInfo.RefreshBinding();
			}
		}
		ZBool fIncludeConsignor;

		public ZPropertyInfo IncludeConsignorInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.IncludeConsignor); }
		}

		#endregion

		#region Include None

		public ZBool IncludeNone
		{
			get { return fIncludeNone; }
			set
			{
				fIncludeNone = value;
				IncludeNoneInfo.RefreshBinding();
			}
		}
		ZBool fIncludeNone;

		public ZPropertyInfo IncludeNoneInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.IncludeNone); }
		}

		#endregion

		#region Include Sending Agent

		public ZBool IncludeSendingAgent
		{
			get { return fIncludeSendingAgent; }
			set
			{
				fIncludeSendingAgent = value;
				IncludeSendingAgentInfo.RefreshBinding();
			}
		}
		ZBool fIncludeSendingAgent;

		public ZPropertyInfo IncludeSendingAgentInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.IncludeSendingAgent); }
		}

		#endregion

		#region Number Of Labels To Print

		public ZInt NumberOfLabelsToPrint
		{
			get { return fNumberOfLabelsToPrint; }
			set
			{
				fNumberOfLabelsToPrint = value;
				if (!IsValidationSuspended)
				{
					ValidateNumberOfLabelsToPrint();
				}
				NumberOfLabelsToPrintInfo.RefreshBinding();
			}
		}
		ZInt fNumberOfLabelsToPrint;

		public void ValidateNumberOfLabelsToPrint()
		{
			NumberOfLabelsToPrintInfo.ClearAllNotifications();
			if ((ZInt)NumberOfLabelsToPrintInfo.Value <= 0)
			{
				NumberOfLabelsToPrintInfo.AddError(Res.GetString("29907e0b-5ed1-4cd2-b707-a48c419de81b", "It is not possible to print Zero or less labels"));
			}

			if ((ZInt)NumberOfLabelsToPrintInfo.Value > TotalNumberOfLabels)
			{
				NumberOfLabelsToPrintInfo.AddError(Res.GetString("5b58d076-e2c6-4585-88b4-2fc34d9b2c7f", "You cannot print more labels than Outer Packs"));
			}
		}

		public ZPropertyInfo NumberOfLabelsToPrintInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.NumberOfLabelsToPrint); }
		}

		#endregion

		#region Label Range From

		public ZInt LabelRangeFrom
		{
			get { return labelRangeFrom; }
			set
			{
				labelRangeFrom = value;
				if (!IsValidationSuspended)
				{
					ValidateLabelRangeFrom();
				}
				LabelRangeFromInfo.RefreshBinding();
			}
		}
		ZInt labelRangeFrom;

		public void ValidateLabelRangeFrom()
		{
			LabelRangeFromInfo.ClearAllNotifications();
			if (LabelRangeFrom < 1)
			{
				LabelRangeFromInfo.AddError(Res.GetString("d34acde0-15aa-4b89-84ea-3897bf6af8b0", "Start range needs to be greater or equal to 1"));
			}
			else if (LabelRangeFrom > TotalNumberOfLabels)
			{
				LabelRangeFromInfo.AddError(Res.GetString("76cd9305-13e9-49e9-a0d2-2fd922bb4094", "Start range needs to be less or equal to the total number of  labels: {0}", TotalNumberOfLabels));
			}
			else if (LabelRangeTo < LabelRangeFrom)
			{
				LabelRangeToInfo.AddError(Res.GetString("bb5b5482-12eb-4171-80b2-c47b97df7349", "End range needs to be greater or equal to the start range"));
			}
		}

		public ZPropertyInfo LabelRangeFromInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.LabelRangeFrom); }
		}

		#endregion

		#region Label Range To

		public ZInt LabelRangeTo
		{
			get { return labelRangeTo; }
			set
			{
				labelRangeTo = value;
				if (!IsValidationSuspended)
				{
					ValidateLabelRangeTo();
				}
				LabelRangeToInfo.RefreshBinding();
			}
		}
		ZInt labelRangeTo;

		public void ValidateLabelRangeTo()
		{
			LabelRangeToInfo.ClearAllNotifications();
			if (LabelRangeTo < 1)
			{
				LabelRangeToInfo.AddError(Res.GetString("c0719616-62a7-473f-bad1-c568e35219d1", "End range needs to be greater or equal to 1"));
			}
			else if (LabelRangeTo > TotalNumberOfLabels)
			{
				LabelRangeToInfo.AddError(Res.GetString("d9eaec1a-5c6b-498f-933f-90547b345589", "End range needs to be less or equal to the total number of labels: {0}", TotalNumberOfLabels));
			}
			else if (LabelRangeTo < LabelRangeFrom)
			{
				LabelRangeToInfo.AddError(Res.GetString("bb5b5482-12eb-4171-80b2-c47b97df7349", "End range needs to be greater or equal to the start range"));
			}
		}

		public ZPropertyInfo LabelRangeToInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.LabelRangeTo); }
		}

		#endregion

		#region Total Number of Labels

		public ZString TotalNumberOfLabelsDescription
		{
			get { return Res.GetString("DocumentShipment|TotalNumberOfLabelsDescription", "of {0}", TotalNumberOfLabels); }
		}

		public ZInt TotalNumberOfLabels
		{
			get
			{
				if (fTotalNumberOfLabels == 0 && Shipment != null)
				{
					foreach (PackLine line in Shipment.OuterPackLines)
					{
						fTotalNumberOfLabels += line.JL_PackageCount;
					}
				}
				return fTotalNumberOfLabels;
			}
		}
		ZInt fTotalNumberOfLabels;

		public ZPropertyInfo TotalNumberOfLabelsDescriptionInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.TotalNumberOfLabelsDescription); }
		}

		#endregion

		#endregion

		#region New Details - Letter of Indemnity

		#region Old Marks And Numbers

		[ReadOnly(true)]
		[MaxLength(10000)]
		public ZString OldMarksAndNumbers
		{
			get { return fOldMarksAndNumbers; }
			set
			{
				CheckMaximumLength(OldMarksAndNumbersInfo, value);
				fOldMarksAndNumbers = value;
				OldMarksAndNumbersInfo.RefreshBinding();
			}
		}
		ZString fOldMarksAndNumbers;

		public ZPropertyInfo OldMarksAndNumbersInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.OldMarksAndNumbers); }
		}

		#endregion

		#region Old Goods Description

		[ReadOnly(true)]
		[MaxLength(10000)]
		public ZString OldGoodsDescription
		{
			get { return fOldGoodsDescription; }
			set
			{
				CheckMaximumLength(OldGoodsDescriptionInfo, value);
				fOldGoodsDescription = value;
				OldGoodsDescriptionInfo.RefreshBinding();
			}
		}
		ZString fOldGoodsDescription;

		public ZPropertyInfo OldGoodsDescriptionInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.OldGoodsDescription); }
		}

		#endregion

		#region Old Weight

		[ReadOnly(true)]
		public ZDecimal OldWeight
		{
			get { return fOldWeight; }
			set
			{
				fOldWeight = this.GetRoundedValue(OldWeightInfo, value);
				OldWeightInfo.RefreshBinding();
			}
		}
		ZDecimal fOldWeight;

		public ZPropertyInfo OldWeightInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.OldWeight); }
		}

		#endregion

		#region Old Weight Unit

		[ReadOnly(true)]
		[List("Shipment.Lookups.JS_UnitOfWeight_List")]
		[MaxLength(CommonShipment.Schema.JS_UnitOfWeightMaxLength)]
		public ZString OldWeightUnit
		{
			get { return fOldWeightUnit; }
			set
			{
				CheckMaximumLength(OldWeightUnitInfo, value);
				fOldWeightUnit = value;

				this.SetRoundedValue(OldWeightInfo);

				OldWeightUnitInfo.RefreshBinding();
			}
		}
		ZString fOldWeightUnit;

		public ZPropertyInfo OldWeightUnitInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.OldWeightUnit); }
		}

		#endregion

		#region Old Volume

		[ReadOnly(true)]
		public ZDecimal OldVolume
		{
			get { return fOldVolume; }
			set
			{
				fOldVolume = this.GetRoundedValue(OldVolumeInfo, value);
				OldVolumeInfo.RefreshBinding();
			}
		}
		ZDecimal fOldVolume;

		public ZPropertyInfo OldVolumeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.OldVolume); }
		}

		#endregion

		#region Old Volume Unit

		[ReadOnly(true)]
		[List("Shipment.Lookups.JS_UnitOfVolume_List")]
		[MaxLength(CommonShipment.Schema.JS_UnitOfVolumeMaxLength)]
		public ZString OldVolumeUnit
		{
			get { return fOldVolumeUnit; }
			set
			{
				CheckMaximumLength(OldVolumeUnitInfo, value);
				fOldVolumeUnit = value;

				this.SetRoundedValue(OldVolumeInfo);

				OldVolumeUnitInfo.RefreshBinding();
			}
		}
		ZString fOldVolumeUnit;

		public ZPropertyInfo OldVolumeUnitInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.OldVolumeUnit); }
		}

		#endregion

		#region New Marks And Numbers

		[MaxLength(10000)]
		public ZString NewMarksAndNumbers
		{
			get { return fNewMarksAndNumbers; }
			set
			{
				CheckMaximumLength(NewMarksAndNumbersInfo, value);
				fNewMarksAndNumbers = value;
				NewMarksAndNumbersInfo.RefreshBinding();
			}
		}
		ZString fNewMarksAndNumbers;

		public ZPropertyInfo NewMarksAndNumbersInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.NewMarksAndNumbers); }
		}

		protected bool NewMarksAndNumbers_ReadOnly
		{
			get { return !ChangeMarksAndNumbers; }
		}

		#endregion

		#region New Goods Description

		[MaxLength(10000)]
		public ZString NewGoodsDescription
		{
			get { return fNewGoodsDescription; }
			set
			{
				CheckMaximumLength(NewGoodsDescriptionInfo, value);
				fNewGoodsDescription = value;
				NewGoodsDescriptionInfo.RefreshBinding();
			}
		}
		ZString fNewGoodsDescription;

		public ZPropertyInfo NewGoodsDescriptionInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.NewGoodsDescription); }
		}

		protected bool NewGoodsDescription_ReadOnly
		{
			get { return !ChangeGoodsDescription; }
		}

		#endregion

		#region New Weight

		[ReadOnlyMember(nameof(NotChangeWeight))]
		[MeasureUnit(Schema.NewWeightUnit, MeasureUnitType.Weight)]
		public ZDecimal NewWeight
		{
			get { return fNewWeight; }
			set
			{
				fNewWeight = this.GetRoundedValue(NewWeightInfo, value);

				if (!IsValidationSuspended)
				{
					ValidateNewWeight();
				}
				if (fNewWeight > 0.0M && NewWeightUnit.IsEmpty)
				{
					NewWeightUnit = OldWeightUnit;
				}
				NewWeightInfo.RefreshBinding();
			}
		}
		ZDecimal fNewWeight;

		public virtual void ValidateNewWeight()
		{
			NewWeightInfo.ClearAllNotifications();
			TypeValidation.CheckValidDecimal(NewWeightInfo, 9, 3);
		}

		public ZPropertyInfo NewWeightInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.NewWeight); }
		}

		#endregion

		#region New Weight Unit

		[ReadOnlyMember(nameof(NotChangeWeight))]
		[List("Shipment.Lookups.JS_UnitOfWeight_List")]
		[MaxLength(CommonShipment.Schema.JS_UnitOfWeightMaxLength)]
		public ZString NewWeightUnit
		{
			get { return fNewWeightUnit; }
			set
			{
				CheckMaximumLength(NewWeightUnitInfo, value);
				fNewWeightUnit = value;

				fNewWeight = this.GetRoundedValue(NewWeightInfo, NewWeight);

				if (!IsValidationSuspended)
				{
					ValidateNewWeightUnit();
				}
				NewWeightUnitInfo.RefreshBinding();
			}
		}
		ZString fNewWeightUnit;

		public void ValidateNewWeightUnit()
		{
			NewWeightUnitInfo.ClearAllNotifications();
			MandatoryValidation.CheckUnitEntered(NewWeightUnitInfo, NewWeightInfo);
			ListValidation.ErrorIfInvalidCode(NewWeightUnitInfo, Shipment.Lookups.JS_UnitOfWeight_List);
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(NewWeightUnitInfo);
		}

		public ZPropertyInfo NewWeightUnitInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.NewWeightUnit); }
		}

		#endregion

		#region New Volume

		[ReadOnlyMember(nameof(NotChangeVolume))]
		[MeasureUnit(Schema.NewVolumeUnit, MeasureUnitType.Volume)]
		public ZDecimal NewVolume
		{
			get { return fNewVolume; }
			set
			{
				fNewVolume = this.GetRoundedValue(NewVolumeInfo, value);

				if (!IsValidationSuspended)
				{
					ValidateNewVolume();
				}
				if (fNewVolume > 0.0M && NewVolumeUnit.IsEmpty)
				{
					NewVolumeUnit = OldVolumeUnit;
				}
				NewVolumeInfo.RefreshBinding();
			}
		}
		ZDecimal fNewVolume;

		public virtual void ValidateNewVolume()
		{
			NewVolumeInfo.ClearAllNotifications();
			TypeValidation.CheckValidDecimal(NewVolumeInfo, 9, 3);
		}

		public ZPropertyInfo NewVolumeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.NewVolume); }
		}

		#endregion

		#region New Volume Unit

		[ReadOnlyMember(nameof(NotChangeVolume))]
		[List("Shipment.Lookups.JS_UnitOfVolume_List")]
		[MaxLength(CommonShipment.Schema.JS_UnitOfVolumeMaxLength)]
		public ZString NewVolumeUnit
		{
			get { return fNewVolumeUnit; }
			set
			{
				CheckMaximumLength(NewVolumeUnitInfo, value);
				fNewVolumeUnit = value;

				fNewVolume = this.GetRoundedValue(NewVolumeInfo, NewVolume);

				if (!IsValidationSuspended)
				{
					ValidateNewVolumeUnit();
				}
				NewVolumeUnitInfo.RefreshBinding();
			}
		}
		ZString fNewVolumeUnit;

		public void ValidateNewVolumeUnit()
		{
			NewVolumeUnitInfo.ClearAllNotifications();
			MandatoryValidation.CheckUnitEntered(NewVolumeUnitInfo, NewVolumeInfo);
			ListValidation.ErrorIfInvalidCode(NewVolumeUnitInfo, Shipment.Lookups.JS_UnitOfVolume_List);
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(NewVolumeUnitInfo);
		}

		public ZPropertyInfo NewVolumeUnitInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.NewVolumeUnit); }
		}

		#endregion

		#region Change Marks And Numbers

		public ZBool ChangeMarksAndNumbers
		{
			get { return fChangeMarksAndNumbers; }
			set
			{
				fChangeMarksAndNumbers = value;
				ChangeMarksAndNumbersInfo.RefreshBinding();
				NewMarksAndNumbersInfo.RefreshBinding();
			}
		}
		ZBool fChangeMarksAndNumbers;

		public ZPropertyInfo ChangeMarksAndNumbersInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ChangeMarksAndNumbers); }
		}

		#endregion

		#region Change Goods Description

		public ZBool ChangeGoodsDescription
		{
			get { return fChangeGoodsDescription; }
			set
			{
				fChangeGoodsDescription = value;
				ChangeGoodsDescriptionInfo.RefreshBinding();
				NewGoodsDescriptionInfo.RefreshBinding();
			}
		}
		ZBool fChangeGoodsDescription;

		public ZPropertyInfo ChangeGoodsDescriptionInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ChangeGoodsDescription); }
		}

		#endregion

		#region Change Weight

		public ZBool ChangeWeight
		{
			get { return fChangeWeight; }
			set
			{
				fChangeWeight = value;
				ChangeWeightInfo.RefreshBinding();
				NewWeightInfo.RefreshBinding();
				if (NewWeightUnit.IsEmpty || NewWeightUnitInfo.HasErrors())
				{
					NewWeightUnit = OldWeightUnit;
				}
				if (NewWeightInfo.HasErrors())
				{
					NewWeight = OldWeight;
				}
				NewWeightUnitInfo.RefreshBinding();
			}
		}
		ZBool fChangeWeight;

		public ZPropertyInfo ChangeWeightInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ChangeWeight); }
		}

		protected bool NotChangeWeight
		{
			get { return !ChangeWeight; }
		}

		#endregion

		#region Change Volume

		public ZBool ChangeVolume
		{
			get { return fChangeVolume; }
			set
			{
				fChangeVolume = value;
				ChangeVolumeInfo.RefreshBinding();
				NewVolumeInfo.RefreshBinding();
				if (NewVolumeUnit.IsEmpty || NewVolumeUnitInfo.HasErrors())
				{
					NewVolumeUnit = OldVolumeUnit;
				}
				if (NewVolumeInfo.HasErrors())
				{
					NewVolume = OldVolume;
				}
				NewVolumeUnitInfo.RefreshBinding();
			}
		}
		ZBool fChangeVolume;

		public ZPropertyInfo ChangeVolumeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ChangeVolume); }
		}

		protected bool NotChangeVolume
		{
			get { return !ChangeVolume; }
		}

		#endregion

		#endregion

		#region Charge Sheet

		#region Debtor

		public DebtorToSelectFromForPrinting Debtor
		{
			get { return fDebtor; }
			set
			{
				fDebtor = value;
			}
		}
		DebtorToSelectFromForPrinting fDebtor;

		#endregion

		#region DebtorCollection

		public DebtorToSelectFromForPrintingCollection DebtorsToPrint
		{
			get
			{
				if (fDebtorsToPrint == null)
				{
					fDebtorsToPrint = new DebtorToSelectFromForPrintingCollection(new OrgHeaderCollection(Factory));
				}
				return fDebtorsToPrint;
			}
		}
		DebtorToSelectFromForPrintingCollection fDebtorsToPrint;

		#endregion

		#endregion

		#region Standard Shipping Note

		public CommonPickupDeliveryConfirm ConfirmationToPrint { get; set; }

		#endregion

		#region IsBillOfLading

		public bool IsBillOfLading { get; }

		#endregion

		#endregion

		#region IDefaultNumberOfDecimalsSupporter Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get
			{
				IDefaultNumberOfDecimalsSupporter parentShipment = Shipment;
				var result = (parentShipment != null) ? parentShipment.TransportMode : ZString.Empty;

				return result;
			}
		}

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.OldWeight:
					unitOfMeasure = OldWeightUnit;
					break;

				case Schema.OldVolume:
					unitOfMeasure = OldVolumeUnit;
					break;

				case Schema.NewWeight:
					unitOfMeasure = NewWeightUnit;
					break;

				case Schema.NewVolume:
					unitOfMeasure = NewVolumeUnit;
					break;

				default:
					break;
			}

			return unitOfMeasure;
		}

		public int GetDefaultNumberOfDecimals(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		ZDecimal IDefaultNumberOfDecimalsSupporter.GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundedValue(this, property, value);
		}

		void IDefaultNumberOfDecimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged()
		{
			// no change in transport mode expected during the lifecycle of this bizObj
		}

		#endregion
	}
}
