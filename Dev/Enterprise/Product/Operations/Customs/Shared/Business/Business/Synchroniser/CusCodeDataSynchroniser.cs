namespace Enterprise.Customs.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public class CusCodeDataSynchroniser : BusinessObjectSynchroniser
	{
		public CusCodeDataSynchroniser(CusCodeData destination, CusCodeData source)
			: base(destination, source)
		{
		}

		public new CusCodeData Source
		{
			get { return (CusCodeData)base.Source; }
		}

		public new CusCodeData Destination
		{
			get { return (CusCodeData)base.Destination; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			this.Synchronisers.Add(new FieldSynchroniser(this.Destination.CY_CodeInfo, this.Source.CY_CodeInfo));
			this.Synchronisers.Add(new FieldSynchroniser(this.Destination.CY_DataInfo, this.Source.CY_DataInfo));
		}
	}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
