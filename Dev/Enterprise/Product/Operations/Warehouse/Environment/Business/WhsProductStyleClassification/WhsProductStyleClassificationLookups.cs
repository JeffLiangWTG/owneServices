namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsProductStyleClassificationLookups : AutoWhsProductStyleClassificationLookups
	{
		public WhsProductStyleClassificationLookups(AutoWhsProductStyleClassification parent)
			: base(parent)
		{
		}

		#region Parent

		public new WhsProductStyleClassification Parent
		{
			get { return (WhsProductStyleClassification)base.Parent; }
		}

		#endregion
	}
}
