using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class PackageValidation : Customs.Business.CusDecHouseContainerPackValidation
	{
		public PackageValidation(Package package)
			: base(package)
		{
		}

		public new Package Parent
		{
			get { return (Package)base.Parent; }
		}

		protected override void CheckCW_PackType()
		{
			base.CheckCW_PackType();
			ListValidation.WarnIfInvalidCode(Parent.CW_PackTypeInfo, Parent.Lookups.PackTypeList, ValidationConstants.InvalidPackTypeMessage);
		}
	}
}
