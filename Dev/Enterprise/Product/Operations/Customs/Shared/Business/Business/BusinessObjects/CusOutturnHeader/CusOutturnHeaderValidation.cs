using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusOutturnHeaderValidation : AutoCusOutturnHeaderValidation
	{
		public CusOutturnHeaderValidation(AutoCusOutturnHeader parent) : base(parent)
		{
		}

		protected new CusOutturnHeader Parent => (CusOutturnHeader)base.Parent;

		#region Vessel Name

		protected override void CheckC6_VesselName()
		{
			base.CheckC6_VesselName();
			ListValidation.ErrorIfInvalidCode(Parent.C6_VesselNameInfo, Parent.Lookups.VesselNames);
		}

		#endregion
	}
}
