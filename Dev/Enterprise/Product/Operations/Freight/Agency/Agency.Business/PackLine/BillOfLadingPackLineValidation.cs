namespace Enterprise.Freight.Agency.Business
{
	public sealed class BillOfLadingPackLineValidation : AgencyShipmentPackLineValidation
	{
		public BillOfLadingPackLineValidation(BillOfLadingPackLine parent)
			: base(parent)
		{
		}

		protected override void CheckJL_JC()
		{
			base.CheckJL_JC();

			if (!Parent.JL_JC.IsEmpty)
			{
				if (Parent.Container != null && Parent.Container.JC_IsEmptyContainer)
				{
					Parent.JL_JCInfo.AddError(Res.GetString("723fda05-ad82-4a35-b4ef-fafa90b01412", "This container has been marked as empty."));
				}
			}
		}

		#region Implementation

		new BillOfLadingPackLine Parent
		{
			get { return (BillOfLadingPackLine)base.Parent; }
		}

		#endregion
	}
}


