using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	[GlowDataDefinition("IUSDrawbackAdditionalExportTariffNumber")]
	public class DrawbackAdditionalExportTariffNumber : CusCodeData
		, IShortSequenceNumberLine
	{
		public DrawbackAdditionalExportTariffNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public const string CY_FormattedTariff = "CY_FormattedTariff";

			public const int CY_FormattedTariffMaxLength = 12;
		}

		#region Related

		public JobComInvoiceLine InvoiceLine
		{
			get { return Factory.Load<JobComInvoiceLine>(CY_ParentID); }
		}

		#endregion

		#region Override Methods

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.DrawbackAdditionalExportTariffNumber;
			CY_Code = CusCodeDataTypeList.Codes.DrawbackAdditionalExportTariffNumber;
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new DrawbackAdditionalExportTariffNumberValidation(this);
		}

		public override void Delete()
		{
			var invoiceLine = InvoiceLine;
			if (invoiceLine != null)
			{
				invoiceLine.AdditionalExportTariffSequenceGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}
			base.Delete();
		}

		#endregion

		#region Override Properties

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(JobComInvoiceLine)); }
		}

		[List(nameof(ExportTariffs))]
		[MaxLength(Schema.CY_FormattedTariffMaxLength)]
		public ZString CY_FormattedTariff
		{
			get { return TariffFormatter.DisplayFormat(CY_Data); }
			set { CY_Data = TariffFormatter.Format(value); }
		}

		public ZPropertyInfo CY_FormattedTariffInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CY_FormattedTariff, (x) => CY_DataInfo); }
		}

		TariffFormatter TariffFormatter
		{
			get { return fTariffFormatter ?? (fTariffFormatter = new TariffFormatter()); }
		}
		TariffFormatter fTariffFormatter;

		public BusinessObjectCollection ExportTariffs
		{
			get
			{
				var parent = Parent as JobComInvoiceLine;
				if (parent != null)
				{
					if (!parent.UseScheduleB)
					{
						return parent.Lookups.GetHTSExportTariffs(CY_Data);
					}
					else
					{
						return parent.Lookups.GetScheduleBTariffs(CY_Data);
					}
				}
				else
				{
					return Factory.GetTariffs(Universal.Constants.TariffTypes.ScheduleB, CY_Data, ZDateTime.Today);
				}
			}
		}

		public override ZShort CY_Order
		{
			get { return base.CY_Order; }
			set
			{
				if (value > 0 && value <= short.MaxValue)
				{
					var oldValue = CY_Order;
					base.CY_Order = value;
					var invoiceLine = InvoiceLine;
					if (!IsCopying && invoiceLine != null)
					{
						invoiceLine.AdditionalExportTariffSequenceGenerator.RecalculateWhenRenumbered(this, oldValue);
					}
				}
			}
		}

		public override ZGuid CY_ParentID
		{
			get { return base.CY_ParentID; }
			set
			{
				var oldInvoiceLine = InvoiceLine;
				if (!IsCopying && CY_ParentID != value)
				{
					base.CY_ParentID = value;
					SetOrderOnSettingCY_ParentID(oldInvoiceLine);
					MarkParentAsNeedingValidation();
				}
			}
		}

		void SetOrderOnSettingCY_ParentID(JobComInvoiceLine oldInvoiceLine)
		{
			if (oldInvoiceLine != null)
			{
				oldInvoiceLine.AdditionalExportTariffSequenceGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}

			var invoiceLine = InvoiceLine;
			if (invoiceLine != null)
			{
				invoiceLine.AdditionalExportTariffSequenceGenerator.RecalculateWhenAdded(this);
			}
		}

		#endregion

		#region IShortSequenceNumberLine Members

		ZGuid ISequenceNumberLine.FKToHeader => CY_ParentID;

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get => CY_Order;
			set => CY_Order = value;
		}

		#endregion
	}
}
