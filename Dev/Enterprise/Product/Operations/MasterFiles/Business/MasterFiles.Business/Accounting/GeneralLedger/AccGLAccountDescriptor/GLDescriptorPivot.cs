using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business
{
	public class GLDescriptorPivot : AccGLDescriptorPivot
	{
		public new class Schema : AutoAccGLDescriptorPivot.Schema
		{
			public const string ReportType = "ReportType";
			public const string ReportCategory = "ReportCategory";
			public const string ReportTypeDescription = "ReportTypeDescription";
			public const string ReportCategoryDescription = "ReportCategoryDescription";
		}

		public GLDescriptorPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public override void Delete()
		{
			if (!IsDeleted)
			{
				if (GLAccountDescriptor != null && !GLAccountDescriptor.IsDeleted &&
						GLAccountDescriptor.AJ_ReportType != AccountingMasterFilesConstants.ReportCodeOfLocalReport.ChartOfAccount)
				{
					GLAccountDescriptor.Delete();
				}
				base.Delete();
			}
		}

		#region ReportCategory

		public ComplianceReportsSetupCategoryCollection ReportCategory_List
		{
			get
			{
				return !ReportType.IsEmpty && ReportType_List != null ?
							   ReportType_List.GetReportTypeCategoriesFromCode(ReportType) :
							   new ComplianceReportsSetupCategoryCollection();
			}
		}

		[MaxLength(3)]
		[List("ReportCategory_List")]
		[BusinessObjectTestExclude]
		public ZString ReportCategory
		{
			get
			{
				return GLAccountDescriptor == null ? ZString.Empty : GLAccountDescriptor.AJ_ReportCategory;
			}
			set
			{
				if (GLAccountDescriptor != null)
				{
					GLAccountDescriptor.AJ_ReportCategory = value;
					if (!IsValidationSuspended)
					{
						((GLDescriptorPivotValidation)Validation).ValidateReportCategory();
					}
				}
				ReportCategoryInfo.RefreshBinding();
				ReportCategoryDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ReportCategoryInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.ReportCategory); }
		}

		protected virtual bool ReportCategory_ReadOnly
		{
			get
			{
				return ReportType_ReadOnly || ReportType == AccountingMasterFilesConstants.ReportCodeOfLocalReport.ChartOfAccount || ReportType.IsEmpty;
			}
		}

		[ReadOnly(true)]
		public ZString ReportCategoryDescription
		{
			get
			{
				return ReportCategory_ReadOnly ? ZString.Empty : ReportCategory_List.GetDescriptionFromCode(ReportCategory);
			}
		}

		public ZPropertyInfo ReportCategoryDescriptionInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.ReportCategoryDescription); }
		}

		#endregion

		#region ReportType

		public ComplianceReportTypeCollection ReportType_List
		{
			get
			{
				fReportType_List = new ComplianceReportTypeCollection();

				var collectionCN = GLAccountDescriptor == null ||
					(GLAccountDescriptor != null &&
					GLAccountDescriptor.AJ_Language == SharedConstants.Languages.ChineseSimplified &&
					GlbCompany.CurrentCompany.Country.Code == Constants.CountryCodes.China) ?
					AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsCN.Value : new ComplianceReportTypeCollection();

				foreach (ComplianceReportType complianceReportType in collectionCN)
				{
					fReportType_List.Add(complianceReportType);
				}

				var collection = AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsUserDefined.Value;
				foreach (ComplianceReportType complianceReportType in collection)
				{
					fReportType_List.Add(complianceReportType);
				}

				return fReportType_List;
			}
		}

		protected ComplianceReportTypeCollection fReportType_List;

		[MaxLength(3)]
		[List("ReportType_List")]
		[BusinessObjectTestExclude]
		public ZString ReportType
		{
			get
			{
				return (GLAccountDescriptor == null) ? ZString.Empty : GLAccountDescriptor.AJ_ReportType;
			}
			set
			{
				if (GLAccountDescriptor != null)
				{
					if (GLAccountDescriptor.AJ_ReportType != value)
					{
						GLAccountDescriptor.AJ_ReportType = value;
						if (!IsValidationSuspended)
						{
							((GLDescriptorPivotValidation)Validation).ValidateReportType();
						}
						ReportCategory = AccountingMasterFilesConstants.DefaultReportCategory.Undefined;
					}
				}
				ReportTypeInfo.RefreshBinding();
				ReportTypeDescriptionInfo.RefreshBinding();
				ReportCategoryInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ReportTypeInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.ReportType); }
		}

		protected virtual bool ReportType_ReadOnly
		{
			get
			{
				return GLAccountDescriptor == null;
			}
		}

		[ReadOnly(true)]
		public ZString ReportTypeDescription
		{
			get
			{
				return GLAccountDescriptor == null || ReportType == AccountingMasterFilesConstants.ReportCodeOfLocalReport.ChartOfAccount ?
																											ZString.Empty :
																											ReportType_List.GetDescriptionFromCode(ReportType);
			}
		}

		public ZPropertyInfo ReportTypeDescriptionInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.ReportTypeDescription); }
		}
		#endregion

		#region Validation

		protected override AccGLDescriptorPivotValidation GetNewValidation()
		{
			return new GLDescriptorPivotValidation(this);
		}

		#endregion
	}
}
