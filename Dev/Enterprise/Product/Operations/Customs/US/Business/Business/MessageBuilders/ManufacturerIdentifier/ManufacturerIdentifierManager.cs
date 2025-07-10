namespace Enterprise.Customs.US.Business
{
	public static class ManufacturerIdentifierManager
	{
		public static void GenerateAddMessage(ManufacturerAddMessageData messageData)
		{
			MQEDIMessage message = ManufacturerIdentifierMessageBuilder.Generate(messageData, Constants.UpdateActionCode.Add);
			messageData.wrapper.Messages.Add(message);
		}

		public static void GenerateUpdateMessage(ManufacturerAddMessageData messageData)
		{
			MQEDIMessage message = ManufacturerIdentifierMessageBuilder.Generate(messageData, Constants.UpdateActionCode.Update);
			messageData.wrapper.Messages.Add(message);
		}

		public static class Constants
		{
			public static class UpdateActionCode
			{
				public const string Add = "A";
				public const string Update = "U";
			}
		}
	}
}
