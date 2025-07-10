namespace Enterprise.Customs.US.Business
{
	public class APHISIdentityCollection : Customs.Business.CusCodeDataCollection<APHISIdentity>
	{
		public APHISIdentityCollection(APHISProduct parent)
			: base(parent, CusCodeDataTypeList.Codes.APHISIdentity)
		{
		}

		public new APHISProduct Master
		{
			get { return (APHISProduct)base.Master; }
		}

		protected override bool AllowNewCore
		{
			get
			{
				var header = Master.Header;
				return header != null && header.IsLiveAnimalsCategory;
			}
		}
	}
}
