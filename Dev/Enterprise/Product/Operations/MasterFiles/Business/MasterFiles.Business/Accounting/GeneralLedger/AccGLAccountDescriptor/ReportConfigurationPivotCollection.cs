using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ReportConfigurationPivotCollection : ActiveBusinessObjectCollection<GLDescriptorPivot>
	{
		public ReportConfigurationPivotCollection(BusinessObjectFactory factory, AccGLAccountDescriptor parent)
			: base(factory)
		{
			if (parent == null)
			{
				throw new ArgumentNullException(nameof(parent), "The Descriptor Pivot Collection requires a non-null parent Descriptor.");
			}
			AccGLDescriptorCOA = parent;
			UpdateAdditionalFilter();
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		protected AccGLAccountDescriptor AccGLDescriptorCOA;

		protected override bool MatchesFilterCore(GLDescriptorPivot element, bool fetchOnlyFromLocalCache)
		{
			bool result = base.MatchesFilterCore(element, fetchOnlyFromLocalCache);

			if (AccGLDescriptorCOA.IsDeleted ||
				(result && AccGLDescriptorCOA.ParentGLHeader == null ||
					(element.GLAccountDescriptor != null &&
						(element.ReportType == AccountingMasterFilesConstants.ReportCodeOfLocalReport.ChartOfAccount ||
						 AccGLDescriptorCOA.AJ_Language != element.GLAccountDescriptor.AJ_Language ||
						 AccGLDescriptorCOA.AJ_LocalAccountNumber != element.GLAccountDescriptor.AJ_LocalAccountNumber))))
			{
				result = false;
			}

			return result;
		}

		public void UpdateGLHeader(ZGuid gLHeaderPK)
		{
			RefreshAll(Factory);
			GLDescriptorPivot[] pivots = (from GLDescriptorPivot p in this select p).ToArray();
			foreach (GLDescriptorPivot gp in pivots)
			{
				gp.YJ_AG = gLHeaderPK;
			}

			UpdateAdditionalFilter();
		}

		protected void UpdateAdditionalFilter()
		{
			AdditionalFilter = new ZQuery(AccGLDescriptorPivotSchema.YJ_AG, AccGLDescriptorCOA.ParentGLHeaderPK);
			RefreshAll(Factory);
		}

		protected override void SetDefaultsForNewElementCore(GLDescriptorPivot newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			if (!AccGLDescriptorCOA.ParentGLHeaderPK.IsEmpty)
			{
				var reportTypeList = new ComplianceReportTypeCollection();

				var collectionForCN = AccGLDescriptorCOA.AJ_Language == Core.SharedConstants.Languages.ChineseSimplified && GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.China ? AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsCN.Value : new ComplianceReportTypeCollection();
				foreach (ComplianceReportType complianceReportType in collectionForCN)
				{
					reportTypeList.Add(complianceReportType);
				}

				var collection = AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsUserDefined.Value;
				foreach (ComplianceReportType complianceReportType in collection)
				{
					reportTypeList.Add(complianceReportType);
				}

				if (0 < reportTypeList.Count)
				{
					ZString reportType = reportTypeList[0].ReportType;
					if (Count > 0)
					{
						foreach (ComplianceReportType rType in reportTypeList)
						{
							reportType = rType.ReportType;
							ZString type = reportType;
							if (this.Any(pivot => pivot.GLAccountDescriptor != null && type == pivot.GLAccountDescriptor.AJ_ReportType))
							{
								reportType = ZString.Empty;
							}
							if (!reportType.IsEmpty)
							{
								break;
							}
						}
					}

					if (reportType.IsEmpty)
					{
						reportType = reportTypeList[0].ReportType;
					}

					AccGLAccountDescriptor accGLAccountDescriptor = AccGLDescriptorCOA.Clone() as AccGLAccountDescriptor;

					if (accGLAccountDescriptor != null)
					{
						accGLAccountDescriptor.AJ_ReportType = reportType;
						accGLAccountDescriptor.AJ_ReportCategory = AccountingMasterFilesConstants.DefaultReportCategory.Undefined;
						newElement.YJ_AJ = accGLAccountDescriptor.PK;
					}
					newElement.YJ_AG = AccGLDescriptorCOA.ParentGLHeaderPK;
				}
			}
		}
	}
}
