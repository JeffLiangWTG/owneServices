namespace Enterprise.Customs.US.Business
{
	public class USFSISLineValidation : Customs.Business.MultiLineAddInfos.CusAddInfoValidation
	{
		public USFSISLineValidation(USFSISLine parent)
			: base(parent)
		{ }

		new USFSISLine Parent
		{
			get { return (USFSISLine)base.Parent; }
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
		}
	}
}
