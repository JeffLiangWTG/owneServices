using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CustomDocumentsCollection : OrgCustomLabelsCollection
	{
		public CustomDocumentsCollection(OrgHeader parent)
			: base(parent)
		{
		}

		public CustomDocumentsCollection(OrgHeader parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery result = base.CreateAdditionalFilter();
			result.AddToFilter(JoinCondition.And, OrgCustomLabelsSchema.OT_Type, SQLComparisonOperator.Equal, OrgConstants.CustomLabelType.Document);
			return result;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((OrgCustomLabels)child).OT_Type = OrgConstants.CustomLabelType.Document;
		}
	}
}
