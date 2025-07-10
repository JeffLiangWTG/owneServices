using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccAlternateChart : AutoAccAlternateChart, IAuditParent
	{
		public AccAlternateChart(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region AlternateChartFormats

		[ChildEditable(true)]
		public AccAlternateChartFormatDependentCollection AlternateChartFormats
		{
			get
			{
				if (fAlternateChartFormats == null)
				{
					fAlternateChartFormats = new AccAlternateChartFormatDependentCollection(this);
					fAlternateChartFormats.Load();
					fAlternateChartFormats.SetReadOnlyIncludingChildren(AAC_ReadOnly);
					RegisterEditableChildObject(fAlternateChartFormats);
				}
				return fAlternateChartFormats;
			}
		}
		AccAlternateChartFormatDependentCollection fAlternateChartFormats;

		#endregion

		#region AccAlternateChartCurrencyTranslations

		[ChildEditable(true)]
		public AccAlternateChartCurrencyTranslationDependentCollection AccAlternateChartCurrencyTranslations
		{
			get
			{
				if (fAccAlternateChartCurrencyTranslations == null)
				{
					fAccAlternateChartCurrencyTranslations = new AccAlternateChartCurrencyTranslationDependentCollection(this);
					fAccAlternateChartCurrencyTranslations.Load();
					RegisterEditableChildObject(fAccAlternateChartCurrencyTranslations);
				}
				return fAccAlternateChartCurrencyTranslations;
			}
		}
		AccAlternateChartCurrencyTranslationDependentCollection fAccAlternateChartCurrencyTranslations;

		#endregion

		public ZString SecondReportStartAccountNum => AAC_AGA_SecondReportStartAccount == ZGuid.Empty ? ZString.Empty : SecondReportStartAccount.AGA_AccountNum;

		public override void Delete()
		{
			if (Factory.Exists(typeof(AccAlternateGLAccount), new ZQuery(AccAlternateGLAccountSchema.AGA_AAC_AlternateChart, this.PK))
				|| Factory.Exists(typeof(AccReportingBook), new ZQuery(AccReportingBookSchema.ARB_AAC_AlternateChart, this.PK)))
			{
				throw new NotSupportedException("Can't delete Alternate Chart of Account because Reporting Books or Alternate GL Accounts are using this chart. Please remove this chart from all Reporting Books and delete all its Alternate GL Account before deleting.");
			}
			AlternateChartFormats.RemoveAndDeleteAll();
			base.Delete();
		}

		[MaxLength(3)]
		[List("Lookups.BaseBalanceSheetStyleList")]
		public override ZString AAC_BalanceSheetStyle { get => base.AAC_BalanceSheetStyle; set => base.AAC_BalanceSheetStyle = value; }

		[List("Lookups.ReportOrderList")]
		[MaxLength(3)]
		public override ZString AAC_ReportOrder { get => base.AAC_ReportOrder; set => base.AAC_ReportOrder = value; }

		[MaxLength(255)]
		public override ZString AAC_Description { get => base.AAC_Description; set => base.AAC_Description = value; }

		[ReadOnlyMember(nameof(AAC_ReadOnly))]
		public override ZBool AAC_IsFixedLength { get => base.AAC_IsFixedLength; set => base.AAC_IsFixedLength = value; }

		[MaxLength(10)]
		public override ZString AAC_Code { get => base.AAC_Code; set => base.AAC_Code = value; }

		bool AAC_ReadOnly => Factory.Exists(typeof(AccAlternateGLAccount), new ZQuery(AccAlternateGLAccountSchema.AGA_AAC_AlternateChart, this.PK));

		internal bool AAC_ReadOnlyForTest => AAC_ReadOnly;

		public override ZBool AAC_IsGlobal
		{
			get => base.AAC_IsGlobal;
			set
			{
				base.AAC_IsGlobal = value;
				AAC_GC_Company = AAC_IsGlobal ? ZGuid.Empty : GlbCompany.CurrentCompany.PK;
			}
		}

		internal bool AAC_IsGlobal_ReadOnly => IsInDatabase;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AAC_IsGlobal = false;
		}

		#region IAuditParent Members

		public IEnumerable<AuditChildInfo> RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(AccAlternateChartFormatSchema.ANF_AAC_AlternateChart, null);
				yield return new AuditChildInfo(AccAlternateChartCurrencyTranslationSchema.ART_AAC_AlternateChart, null);
			}
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			this.AAC_Code = ZGuid.NewZGuid().ToString().Substring(0, 3);
			this.AAC_Description = "MGT";
			this.AAC_IsFixedLength = true;
			this.AAC_IsGlobal = true;
			this.AAC_BalanceSheetStyle = "ELA";
			this.AAC_ReportOrder = "BTP";
		}
#endif
	}
}
