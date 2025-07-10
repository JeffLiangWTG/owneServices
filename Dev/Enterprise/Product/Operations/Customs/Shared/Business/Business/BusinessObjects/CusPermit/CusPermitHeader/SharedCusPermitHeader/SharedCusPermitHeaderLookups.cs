using System.Collections;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class SharedCusPermitHeaderLookups : CusPermitHeaderLookups
	{
		public SharedCusPermitHeaderLookups(SharedCusPermitHeader parent)
			: base(parent)
		{
		}

		public new SharedCusPermitHeader Parent => (SharedCusPermitHeader)base.Parent;

		public CodeDescriptionPairList PermitQtyValIndicators => Parent.GetCountrySpecificInstruction()?.GetQtyValIndicatorList(Parent.CPH_Type, Parent.CPH_SubType);

		public CodeDescriptionPairList PermitTransactionCategories => Factory.GetCachedValue<PermitTransactionCategoryList>();

		public virtual CodeDescriptionPairList PermitTypes => Parent.GetCountrySpecificInstruction()?.GetTypeList();

		public virtual CodeDescriptionPairList PermitSubTypes => Parent.GetCountrySpecificInstruction()?.GetSubTypeList(Parent.CPH_Type);

		public OrgHeaderCollection AppliesToList => new OrgHeaderCollection(Factory);

		public virtual ICollection PermitNumberCollection => Parent.GetCountrySpecificInstruction()?.GetPermitNumberCollection(Parent);
	}
}
