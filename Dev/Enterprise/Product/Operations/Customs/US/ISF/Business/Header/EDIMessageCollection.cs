using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.ISF.Business
{
	public class EDIMessageCollection : US.Business.EDIMessageCollection
	{
		public EDIMessageCollection(CusISFHeader header)
			: base(header)
		{
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);

			var header = Master as CusISFHeader;
			if (header != null && !header.IsDeleted)
			{
				header.ClearBF_CustomsReference_ReadOnlyCache();
			}
		}
	}
}
