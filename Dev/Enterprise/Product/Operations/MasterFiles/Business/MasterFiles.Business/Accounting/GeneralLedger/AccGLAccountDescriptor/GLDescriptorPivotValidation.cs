
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GLDescriptorPivotValidation : AccGLDescriptorPivotValidation
	{
		public GLDescriptorPivotValidation(GLDescriptorPivot parent)
			: base(parent)
		{ }

		public new GLDescriptorPivot Parent
		{
			get { return (GLDescriptorPivot)base.Parent; }
		}

		public override void ValidateAll()
		{
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				base.ValidateAll();
				ValidateReportType();
				ValidateReportCategory();
			}
		}

		public void ValidateReportType()
		{
			((IValidationInternals)this).Validate(Parent.ReportTypeInfo, GetReportTypeValidationInvoker());
		}

		RunValidationInvoker GetReportTypeValidationInvoker()
		{
			return CheckReportType;
		}

		protected void CheckReportType()
		{
			if (!Parent.ReportTypeInfo.HasErrors() && Parent.ReportType != AccountingMasterFilesConstants.ReportCodeOfLocalReport.ChartOfAccount &&
							!Parent.ReportType_List.CodesAsString.Contains(Parent.ReportType))
			{
				Parent.ReportTypeInfo.AddError(Res.GetString("333f3fbb-6d83-7101-8056-96e7dc698e49",
										"The report type '{0}' is missing in the current language '{1}'. \r\r\nPlease contact support for assistance with this error.",
										Parent.ReportType, Parent.GLAccountDescriptor.AJ_Language));
			}
		}

		public void ValidateReportCategory()
		{
			((IValidationInternals)this).Validate(Parent.ReportCategoryInfo, GetReportCategoryValidationInvoker());
		}

		RunValidationInvoker GetReportCategoryValidationInvoker()
		{
			return CheckReportCategory;
		}

		protected void CheckReportCategory()
		{
			if (!Parent.ReportCategoryInfo.HasErrors() && (Parent.ReportCategory == AccountingMasterFilesConstants.DefaultReportCategory.Undefined || Parent.ReportCategory.IsEmpty))
			{
				Parent.ReportCategoryInfo.AddError(Res.GetString("321f3fbb-5d83-4101-9056-46e7dc691234", "Please select a valid category for the report type '{0}'.", Parent.ReportType));
			}
			else
			{
				if (Parent.GLAccountDescriptor != null && Parent.ReportType != AccountingMasterFilesConstants.ReportCodeOfLocalReport.ChartOfAccount && !Parent.ReportCategory.IsEmpty)
				{
					GLDescriptorPivot[] fDescriptorPivots = Parent.Factory.Load<GLDescriptorPivot>(new ZQuery(AccGLDescriptorPivotSchema.YJ_AG, Parent.YJ_AG));

					if (fDescriptorPivots.Length > 0)
					{
						var duplicates = new Dictionary<ZString, ZInt>();
						var duplicateCategories = new Dictionary<ZString, ZInt>();

						var language = Parent.GLAccountDescriptor.AJ_Language;

						foreach (GLDescriptorPivot pivot in fDescriptorPivots)
						{
							if (pivot.ReportType != AccountingMasterFilesConstants.ReportCodeOfLocalReport.ChartOfAccount &&
								pivot.GLAccountDescriptor != null && pivot.GLAccountDescriptor.AJ_Language == language)
							{
								if (!duplicates.ContainsKey(pivot.ReportType))
								{
									duplicates.Add(pivot.ReportType, 1);
								}
								else
								{
									duplicates[pivot.ReportType]++;
								}

								if (!duplicateCategories.ContainsKey(GetKey(pivot)))
								{
									duplicateCategories.Add(GetKey(pivot), 1);
								}
								else
								{
									duplicateCategories[GetKey(pivot)]++;
								}
							}
						}

						if (!Parent.ReportCategoryInfo.HasErrors() && duplicateCategories.ContainsKey(GetKey(Parent)) && duplicateCategories[GetKey(Parent)] > 1)
						{
							Parent.ReportCategoryInfo.AddError(Res.GetString("222f3fbb-5d83-4101-9056-46e7dc698899",
												"The category '{0}' is duplicate in the report type '{1}' for the current language.",
												Parent.ReportCategory,
												Parent.ReportType));
						}

						if (!Parent.ReportCategoryInfo.HasErrors() && Parent.ReportCategory_List[Parent.ReportCategory] == null)
						{
							Parent.ReportCategoryInfo.AddError(Res.GetString("222f3fbb-5d83-4101-9056-46e7dc698849",
									"The category '{0}' is missing in the report type '{1}' for the current language. \r\r\nPlease contact support for assistance with this error.",
									Parent.ReportCategory,
									Parent.ReportType));
						}

						if (!Parent.ReportCategoryInfo.HasErrors() &&
									duplicates.ContainsKey(Parent.ReportType) &&
									duplicates[Parent.ReportType] > 1 &&
									!Parent.ReportCategory.IsEmpty &&
									!Parent.ReportCategory_List[Parent.ReportCategory].AllowDuplicate)
						{
							Parent.ReportCategoryInfo.AddError(Res.GetString("432f3fbb-5d83-4101-9056-46e7dc69a567",
									"You cannot have more than one GL local Account '{0} - {1}' in the report type '{2}' for the current language.",
									Parent.GLAccountDescriptor.AJ_LocalAccountNumber,
									Parent.GLAccountDescriptor.AJ_AccountDescription,
									Parent.ReportType));
						}
					}
				}
			}

			ZString GetKey(GLDescriptorPivot pivot) => pivot.ReportType + ", " + pivot.ReportCategory;
		}
	}
}
