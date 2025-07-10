using System;
using CargoWise.Common;

namespace Enterprise.MasterFiles.Business
{
	public class CustomLabelProvider
	{
		public CustomLabelProvider(OrgCustomLabelsCollection collection)
		{
			Argument.NotNull(collection, "collection");
			this.collection = collection;
		}
		readonly OrgCustomLabelsCollection collection;

		public OrgCustomLabels GetLabelFallbackToCompanyOrgProxy(string fieldName)
		{
			if (GlbCompany.CurrentCompany == null)
			{
				throw new InvalidOperationException("CurrentCompany should not be null in GetLabelFallbackToCompanyOrgProxy.");
			}
			return GetLabelFallbackToCompanyOrgProxy(GlbCompany.CurrentCompany, fieldName);
		}

		public OrgCustomLabels GetLabelFallbackToCompanyOrgProxy(GlbCompany company, string fieldName)
		{
			return GetLabelFallbackToCompanyOrgProxy(company, fieldName, string.Empty);
		}

		public OrgCustomLabels GetLabelFallbackToCompanyOrgProxy(GlbCompany company, string fieldName, string labelType)
		{
			OrgCustomLabelsCollection customLabels = collection;

			//falls back to OrgProxy only if there is *ZERO* configuration against organisation for a specified label type
			var orgProxy = company.Factory.ThreadSentry.IsOwner ? company.OrgProxy : collection.Factory.Load<OrgHeader>(company.GC_OH_OrgProxy);
			if (orgProxy != null && ((collection.Count == 0 || !string.IsNullOrEmpty(labelType) && !collection.HasLabelType(labelType))))
			{
				customLabels = orgProxy.CustomLabels;
			}

			return customLabels.FindByFieldNameAndType(fieldName, labelType);
		}
	}
}
