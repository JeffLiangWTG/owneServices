using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Environment.Business
{
	public class ReadOnlyWhsCartonGroupSizeLinkCollection : ActiveBusinessObjectCollection<WhsCartonGroupSizeLink>
	{
		public ReadOnlyWhsCartonGroupSizeLinkCollection(WhsCartonGroup master)
			: base(master)
		{
		}

		protected override bool AllowNew => false;
	}
}
