using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ContactItemProxyValidation : AutoContactItemProxyValidation
	{
		public ContactItemProxyValidation(AutoContactItemProxy parent)
			: base(parent)
		{
		}

		public new ContactItemProxy Parent
		{
			get { return (ContactItemProxy)base.Parent; }
		}

		protected override void CheckOI_Description()
		{
			base.CheckOI_Description();
			MandatoryValidation.CheckEntered(Parent.OI_DescriptionInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OI_DescriptionInfo, Parent.Lookups.DescriptionList);
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			if (info == Parent.OI_OCInfo)
			{
				return false;
			}

			return base.ShouldValidateFKToCancelledRecord(info);
		}
	}
}
