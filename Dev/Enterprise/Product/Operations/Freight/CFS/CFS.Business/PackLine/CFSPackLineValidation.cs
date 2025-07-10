using System.Globalization;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSPackLineValidation : PackLineValidation
	{
		public CFSPackLineValidation(CFSPackLine parent)
			: base(parent)
		{
		}

		public new CFSPackLine Parent
		{
			get { return (CFSPackLine)base.Parent; }
		}

		#region JL_PackageCount

		protected override void CheckJL_PackageCount()
		{
			base.CheckJL_PackageCount();

			int packagesDispatchedFromCFS = Parent.PackagesConfirmed_DispatchedFromDestinationCFS;
			if (Parent.JL_PackageCount < packagesDispatchedFromCFS)
			{
				Parent.JL_PackageCountInfo.AddWarning(Res.GetString("40ad13c3-6ab2-4e50-9c7f-a68d3630248e", "{0} packages have already been delivered.", packagesDispatchedFromCFS.ToString(CultureInfo.CurrentCulture)));
			}
		}

		#endregion

		#region JL_ContainerPackingOrder

		protected override void CheckJL_ContainerPackingOrder()
		{
			base.CheckJL_ContainerPackingOrder();
			CheckJL_ContainerPackingOrder_IsUnique();
		}

		#endregion

	}
}
