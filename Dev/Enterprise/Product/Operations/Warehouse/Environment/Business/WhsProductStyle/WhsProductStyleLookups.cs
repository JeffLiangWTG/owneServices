namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsProductStyleLookups : AutoWhsProductStyleLookups
	{
		public WhsProductStyleLookups(AutoWhsProductStyle parent)
			: base(parent)
		{
		}

		#region Parent

		public new WhsProductStyle Parent
		{
			get { return (WhsProductStyle)base.Parent; }
		}

		#endregion
	}
}
