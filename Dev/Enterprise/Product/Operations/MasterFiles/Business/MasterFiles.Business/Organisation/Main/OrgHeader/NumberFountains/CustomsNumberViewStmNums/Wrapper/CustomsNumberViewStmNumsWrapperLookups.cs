using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class CustomsNumberViewStmNumsWrapperLookups : ZLookups
	{
		public CustomsNumberViewStmNumsWrapperLookups(CustomsNumberViewStmNumsWrapper parent)
			: base(parent)
		{
		}

		public virtual CodeDescriptionPairList TypeList => Parent.StmNums.Lookups.TypeList;

		public virtual IBusinessObjectCollection OwnerCollection => Parent.StmNums.Lookups.OwnerCollection;

		protected new CustomsNumberViewStmNumsWrapper Parent
		{
			get { return (CustomsNumberViewStmNumsWrapper)base.Parent; }
		}
	}
}