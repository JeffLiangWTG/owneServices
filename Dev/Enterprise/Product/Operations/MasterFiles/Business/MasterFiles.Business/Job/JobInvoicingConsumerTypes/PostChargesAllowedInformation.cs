namespace Enterprise.MasterFiles.Business
{
	public class PostChargesAllowedInformation
	{
		public PostChargesAllowedInformation()
			: this(true, string.Empty) { }

		public PostChargesAllowedInformation(bool postAllowed, string reason)
		{
			PostAllowed = postAllowed;
			if (!PostAllowed)
			{
				ReasonForDisallowing = reason;
			}
		}

		public readonly bool PostAllowed;
		public readonly string ReasonForDisallowing;
	}
}
