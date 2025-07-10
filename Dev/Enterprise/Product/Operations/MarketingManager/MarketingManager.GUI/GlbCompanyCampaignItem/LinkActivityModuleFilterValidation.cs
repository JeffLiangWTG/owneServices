using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MarketingManager.GUI
{
	public class LinkActivityModuleFilterValidation : ModuleFilterDateValidation
	{
		public LinkActivityModuleFilterValidation(LinkActivityModuleFilter parent)
			: base(parent)
		{
		}

		new LinkActivityModuleFilter Parent
		{
			get { return (LinkActivityModuleFilter)base.Parent; }
		}

		#region ValidateTypeProperty

		public void ValidateTypeProperty()
		{
			ValidateCalculatedProperty(Parent.TypePropertyInfo);
		}

		protected void CheckTypeProperty()
		{
			if (Parent.LinkTrackingList != null)
			{
				ListValidation.ErrorIfInvalidCode(Parent.TypePropertyInfo);
			}
			else
			{
				Parent.TypePropertyInfo.AddError(Res.GetString("6009dc21-e7ac-4449-8e7e-1bf233f96d6e", "Please select a Source Campaign"));
			}
		}

		#endregion

		public override Type AutoValidationType
		{
			get { return this.GetType(); }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateTypeProperty();
		}
	}
}
