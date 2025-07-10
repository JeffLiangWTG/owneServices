using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	class CartageProcessTaskCollection : ProcessTaskCollection
	{
		public CartageProcessTaskCollection(CommonCartage cartage)
			: base(cartage)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public new CartageProcessTask this[int index]
		{
			get { return (CartageProcessTask)Elements[index]; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public new CartageProcessTask AddNew()
		{
			return (CartageProcessTask)base.AddNew();
		}
	}
}
