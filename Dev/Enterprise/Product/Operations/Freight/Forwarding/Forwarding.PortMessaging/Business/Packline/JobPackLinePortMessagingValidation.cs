namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	public class JobPackLinePortMessagingValidation : AutoJobPackLinePortMessagingValidation
	{
		public JobPackLinePortMessagingValidation(AutoJobPackLinePortMessaging parent)
			: base(parent)
		{
		}

		protected new PackLinePortMessaging Parent => base.Parent as PackLinePortMessaging;

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateDGTechnicalName();
		}

		#region DGTechnicalName

		public void ValidateDGTechnicalName()
		{
			ValidateCalculatedProperty(Parent.DGTechnicalNameInfo);
		}

		#endregion
	}
}
