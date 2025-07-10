using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.Accounting;

namespace Enterprise.MasterFiles.Business
{
	public class AccReportingBook : AutoAccReportingBook
	{
		public AccReportingBook(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new abstract class Schema : AutoAccReportingBook.Schema
		{
			public const string ARB_IsGlobal = "ARB_IsGlobal";
			public const string ARB_CategorisWithChildren = "ARB_CategorisWithChildren";
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("94A960CE-F41E-4452-A8D9-CD53BE075DFF", "Reporting Book");
			}
		}

		#region Properties

		#region ARB_IsGlobal

		[BusinessObjectTestExclude]
		[ResourceStringData("AccReportingBook|ARB_IsGlobal", ShortCaption = "Is Global", Caption = "Is Global")]
		public ZBool ARB_IsGlobal
		{
			get
			{
				return fIsGlobal;
			}
			set
			{
				fIsGlobal = value;
				if (!value)
				{
					base.ARB_GC_CompanyOfPeriod = ZGuid.Empty;
				}
				ARB_GC_CompanyOfPeriodInfo.RefreshBinding();
			}
		}

		ZBool fIsGlobal;

		public virtual ZPropertyInfo ARB_IsGlobalInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ARB_IsGlobal); }
		}

		#endregion

		#region ARB_Code

		[MaxLength(10)]
		[ResourceStringData("AccReportingBook|ARB_Code", ShortCaption = "Code", Caption = "Code")]
		public override ZString ARB_Code
		{
			get
			{
				return base.ARB_Code;
			}
			set
			{
				base.ARB_Code = value;
			}
		}

		#endregion

		#region ARB_Description

		[MaxLength(255)]
		[ResourceStringData("AccReportingBook|ARB_Description", ShortCaption = "Description", Caption = "Description")]
		public override ZString ARB_Description
		{
			get
			{
				return base.ARB_Description;
			}
			set
			{
				base.ARB_Description = value;
			}
		}

		#endregion

		#region ARB_RX_NKCurrency

		[MaxLength(3)]
		[ResourceStringData("b3e1a8d2-4f5b-4c8e-9a3d-1f2e3b4c5d6f", ShortCaption = "Rep. Cur.", Caption = "Reporting Currency")]
		public override ZString ARB_RX_NKCurrency
		{
			get
			{
				return base.ARB_RX_NKCurrency;
			}
			set
			{
				base.ARB_RX_NKCurrency = value;
			}
		}

		public bool IsLocalCurrency => ARB_RX_NKCurrency.IsEmpty || ARB_RX_NKCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

		#endregion

		#region ARB_IncludePresentationJournals

		[MaxLength(3)]
		[ResourceStringData("AccReportingBook|ARB_IncludePresentationJournals", ShortCaption = "Presentation Journals", Caption = "Presentation Journals")]
		[List("Lookups.PresentationCategoryList")]
		public override ZString ARB_IncludePresentationJournals
		{
			get
			{
				return base.ARB_IncludePresentationJournals;
			}
			set
			{
				base.ARB_IncludePresentationJournals = value;
				if(ARB_IncludeChildPresentation_ReadOnly)
				{
					ARB_IncludeChildPresentation = ZBool.False;
				}
			}
		}

		#endregion

		#region ARB_IncludeChildPresentation

		[ResourceStringData("AccReportingBook|ARB_IncludeChildPresentation", ShortCaption = "Include Child Categories", Caption = "Include Child Categories")]
		public override ZBool ARB_IncludeChildPresentation
		{
			get
			{
				return base.ARB_IncludeChildPresentation;
			}
			set
			{
				base.ARB_IncludeChildPresentation = value;
			}
		}

		protected bool ARB_IncludeChildPresentation_ReadOnly
		{
			get { return base.ARB_IncludePresentationJournals == ObjectFactory.Get<IAccounting>().GetCategorisWithChildren(ARB_IncludePresentationJournals); }
		}

		#endregion

		#region ARB_CategorisWithChildren

		[ResourceStringData("AccReportingBook|ARB_CategorisWithChildren", ShortCaption = "Presentation Journals", Caption = "Presentation Journals")]
		public ZString ARB_CategorisWithChildren => ARB_IncludeChildPresentation ? ObjectFactory.Get<IAccounting>().GetCategorisWithChildren(ARB_IncludePresentationJournals) : ARB_IncludePresentationJournals;

		public virtual ZPropertyInfo ARB_CategorisWithChildrenInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ARB_CategorisWithChildren); }
		}

		#endregion

		#region ARB_AAC_AlternateChart

		[ResourceStringData("AccReportingBook|ARB_AAC_AlternateChart", ShortCaption = "Alternate Chart", Caption = "Alternate Chart")]
		public override ZGuid ARB_AAC_AlternateChart
		{
			get
			{
				return base.ARB_AAC_AlternateChart;
			}
			set
			{
				base.ARB_AAC_AlternateChart = value;
			}
		}

		#endregion

		#region ARB_IsActive

		[ResourceStringData("AccReportingBook|ARB_IsActive", ShortCaption = "Is Active", Caption = "Is Active")]
		public override ZBool ARB_IsActive
		{
			get
			{
				return base.ARB_IsActive;
			}
			set
			{
				base.ARB_IsActive = value;
			}
		}

		#endregion

		#region ARB_GC_CompanyOfPeriod

		[ResourceStringData("AccReportingBook|ARB_GC_CompanyOfPeriod", ShortCaption = "Reporting Period", Caption = "Reporting Period")]
		public override ZGuid ARB_GC_CompanyOfPeriod
		{
			get
			{
				return base.ARB_GC_CompanyOfPeriod;
			}
			set
			{
				base.ARB_GC_CompanyOfPeriod = value;
			}
		}

		protected bool ARB_GC_CompanyOfPeriod_ReadOnly => !ARB_IsGlobal;

		#endregion

		public ZGuid AlternateChartCompany => AlternateChart?.AAC_GC_Company ?? ZGuid.Empty;

		#endregion

		#region Override

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			base.ARB_IsActive = true;
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			fIsGlobal = AlternateChart?.AAC_IsGlobal ?? ZBool.False;
		}

		#endregion
	}
}
