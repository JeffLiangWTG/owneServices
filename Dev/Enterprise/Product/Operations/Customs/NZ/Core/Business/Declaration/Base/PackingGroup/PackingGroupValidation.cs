using Enterprise.Customs.Business;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class PackingGroupValidation : CusDecHouseContainerPivotValidation
	{
		public PackingGroupValidation(PackingGroup packingGroup)
			: base(packingGroup)
		{
		}

		protected new PackingGroup Parent
		{
			get { return (PackingGroup)base.Parent; }
		}

		protected override void CheckCR_CO_Container()
		{
			base.CheckCR_CO_Container();
			foreach (Package package in Parent.Packages)
			{
				package.Validation.ValidateCW_PackQty();
			}
		}
	}
}
