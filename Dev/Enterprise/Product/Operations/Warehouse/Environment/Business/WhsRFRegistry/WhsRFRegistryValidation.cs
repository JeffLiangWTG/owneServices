using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsRFRegistryValidation : AutoWhsRFRegistryValidation
	{
		public WhsRFRegistryValidation(AutoWhsRFRegistry parent) : base(parent)
		{
		}

		protected override void CheckWRR_UOMPackType()
		{
			base.CheckWRR_UOMPackType();
			ListValidation.ErrorIfInvalidCode(Parent.WRR_UOMPackTypeInfo);
		}
	}
}
