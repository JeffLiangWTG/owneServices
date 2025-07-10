using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class CustomsNumberViewStmNumsLookups : ViewStmNumsLookups
	{
		public CustomsNumberViewStmNumsLookups(CustomsNumberViewStmNums parent)
			: base(parent)
		{
		}

		public virtual IBusinessObjectCollection OwnerCollection
		{
			get
			{
				return Parent.Provider?.GetOwnerCollection(Factory, Parent)
					?? throw new DeveloperNotificationException("You must set Provider before access to the owner, consider using CustomsNumberViewStmNumsHelper to create or load ViewStmNums, which will set the Provider for you.");
			}
		}

		protected new CustomsNumberViewStmNums Parent
		{
			get { return (CustomsNumberViewStmNums)base.Parent; }
		}
	}
}
