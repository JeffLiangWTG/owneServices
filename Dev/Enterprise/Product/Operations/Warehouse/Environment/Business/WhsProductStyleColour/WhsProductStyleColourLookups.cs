namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsProductStyleColourLookups : AutoWhsProductStyleColourLookups
	{
		public WhsProductStyleColourLookups(AutoWhsProductStyleColour parent)
			: base(parent)
		{
		}

		#region Parent

		public new WhsProductStyleColour Parent
		{
			get { return (WhsProductStyleColour)base.Parent; }
		}

		#endregion
	}
}
