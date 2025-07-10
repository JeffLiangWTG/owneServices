using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	[GlowDataDefinition("IUSDrawbackAdditionalImportTariffNumber")]
	public class DrawbackAdditionalImportTariffNumber : AutoDrawbackAdditionalImportTariffNumber
		, IHugeSequenceNumberLine
	{
		public DrawbackAdditionalImportTariffNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : Customs.Business.MultiLineAddInfos.CusAddInfo<DrawbackAdditionalImportTariffNumberAddInfo>.Schema
		{
			public const string US_FormattedTariff = "US_FormattedTariff";

			public const int US_FormattedTariffMaxLength = 12;
		}

		#region Related

		public JobComInvoiceLine InvoiceLine
		{
			get { return Factory.Load<JobComInvoiceLine>(B7_ParentID); }
		}

		#endregion
		#region New Properties

		[List(nameof(AddInfoLookups) + "." + nameof(DrawbackAdditionalImportTariffNumberAddInfoLookups.ImportTariffs))]
		[MaxLength(Schema.US_FormattedTariffMaxLength)]
		public ZString US_FormattedTariff
		{
			get { return TariffFormatter.DisplayFormat(US_Tariff); }
			set { US_Tariff = TariffFormatter.Format(value); }
		}

		public override ZString US_Tariff
		{
			get => base.US_Tariff;
			set
			{
				var oldValue = US_Tariff;
				base.US_Tariff = value;
				if (oldValue != value && !IsCopying)
				{
					InvoiceLine?.RefreshUS_DRWAdValoremRate();
				}
			}
		}

		public ZPropertyInfo US_FormattedTariffInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_FormattedTariff, (x) => US_TariffInfo); }
		}

		TariffFormatter TariffFormatter
		{
			get { return fTariffFormatter ?? (fTariffFormatter = new TariffFormatter()); }
		}
		TariffFormatter fTariffFormatter;

		public USCTariff Tariff
		{
			get
			{
				USCTariff result = null;
				var tariff = US_Tariff;
				if (!tariff.IsEmpty)
				{
					var dateForDutyRate = InvoiceLine?.EffectiveDateForDutyRate ?? ZDate.Today;
					return new USCTariff.Loader(Factory).LoadCachedBestMatch(tariff, dateForDutyRate);
				}

				return result;
			}
		}

		#endregion

		#region Override Properties

		public override ZDecimal US_Quantity1
		{
			get { return base.US_Quantity1; }
			set
			{
				var oldValue = US_Quantity1;
				base.US_Quantity1 = value;
				if (oldValue != value && !IsCopying)
				{
					if (value.IsEmpty)
					{
						US_AllowableQty1 = ZDecimal.Zero;
						US_SubstitutedValue1 = ZDecimal.Zero;
					}
				}
			}
		}

		public override ZDecimal US_Quantity2
		{
			get { return base.US_Quantity2; }
			set
			{
				var oldValue = US_Quantity2;
				base.US_Quantity2 = value;
				if (oldValue != value && !IsCopying)
				{
					if (value.IsEmpty)
					{
						US_AllowableQty2 = ZDecimal.Zero;
						US_SubstitutedValue2 = ZDecimal.Zero;
					}
				}
			}
		}

		public override ZDecimal US_Quantity3
		{
			get { return base.US_Quantity3; }
			set
			{
				var oldValue = US_Quantity3;
				base.US_Quantity3 = value;
				if (oldValue != value && !IsCopying)
				{
					if (value.IsEmpty)
					{
						US_AllowableQty3 = ZDecimal.Zero;
						US_SubstitutedValue3 = ZDecimal.Zero;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsQuantity2Empty))]
		public override ZDecimal US_AllowableQty2
		{
			get { return base.US_AllowableQty2; }
			set { base.US_AllowableQty2 = value; }
		}

		[ReadOnlyMember(nameof(IsQuantity2Empty))]
		public override ZDecimal US_ValuePerUnit2
		{
			get { return base.US_ValuePerUnit2; }
			set { base.US_ValuePerUnit2 = value; }
		}

		[ReadOnlyMember(nameof(IsQuantity2Empty))]
		public override ZDecimal US_SubstitutedValue2
		{
			get { return base.US_SubstitutedValue2; }
			set { base.US_SubstitutedValue2 = value; }
		}

		bool IsQuantity2Empty
		{
			get { return US_Quantity2.IsEmpty; }
		}

		[ReadOnlyMember(nameof(IsQuantity3Empty))]
		public override ZDecimal US_AllowableQty3
		{
			get { return base.US_AllowableQty3; }
			set { base.US_AllowableQty3 = value; }
		}

		[ReadOnlyMember(nameof(IsQuantity3Empty))]
		public override ZDecimal US_ValuePerUnit3
		{
			get { return base.US_ValuePerUnit3; }
			set { base.US_ValuePerUnit3 = value; }
		}

		[ReadOnlyMember(nameof(IsQuantity3Empty))]
		public override ZDecimal US_SubstitutedValue3
		{
			get { return base.US_SubstitutedValue3; }
			set { base.US_SubstitutedValue3 = value; }
		}

		bool IsQuantity3Empty
		{
			get { return US_Quantity3.IsEmpty; }
		}

		public override ZInt US_LineNo
		{
			get { return base.US_LineNo; }
			set
			{
				if (value > 0 && value <= short.MaxValue)
				{
					var oldValue = US_LineNo;
					base.US_LineNo = value;
					var invoiceLine = InvoiceLine;
					if (!IsCopying && invoiceLine != null)
					{
						invoiceLine.AdditionalImportTariffSequenceGenerator.RecalculateWhenRenumbered(this, (ZShort)oldValue);
					}
				}
			}
		}

		public override ZGuid B7_ParentID
		{
			get { return base.B7_ParentID; }
			set
			{
				var oldInvoiceLine = InvoiceLine;
				if (!IsCopying && B7_ParentID != value)
				{
					base.B7_ParentID = value;
					SetOrderOnSettingB7_ParentID(oldInvoiceLine);
				}
			}
		}

		void SetOrderOnSettingB7_ParentID(JobComInvoiceLine oldInvoiceLine)
		{
			if (oldInvoiceLine != null)
			{
				oldInvoiceLine.AdditionalImportTariffSequenceGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}

			var invoiceLine = InvoiceLine;
			if (invoiceLine != null)
			{
				invoiceLine.AdditionalImportTariffSequenceGenerator.RecalculateWhenAdded(this);
			}
		}

		#endregion

		#region Override Method

		public override void Delete()
		{
			var invoiceLine = InvoiceLine;
			if (invoiceLine != null)
			{
				invoiceLine.AdditionalImportTariffSequenceGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}
			base.Delete();
		}

		#endregion

		#region IHugeSequenceNumberLine Members

		ZGuid ISequenceNumberLine.FKToHeader => B7_ParentID;

		ZInt ISequenceNumberLine<ZInt>.SequenceNumber
		{
			get => US_LineNo;
			set => US_LineNo = value;
		}

		#endregion
	}
}
