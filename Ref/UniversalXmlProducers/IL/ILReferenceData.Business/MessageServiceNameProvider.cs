namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public static class MessageServiceNameProvider
	{
		public static string GetServiceName(string messageSubType)
		{
			switch (messageSubType)
			{
				case Constants.MessageSubType.ExRate:
					return Constants.ServiceName.ExRate;
				case Constants.MessageSubType.CustomCodes:
					return Constants.ServiceName.CustomCodes;
				default:
					return string.Empty;
			}
		}
	}
}
