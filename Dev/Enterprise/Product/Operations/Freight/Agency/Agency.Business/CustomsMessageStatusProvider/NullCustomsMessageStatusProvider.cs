namespace Enterprise.Freight.Agency.Business
{
	internal class NullCustomsMessageStatusProvider : ICustomsMessageStatusProvider
	{
		public string GetCustomsStatus(BillOfLading billOfLading)
		{
			return "";
		}

		public string GetMessageStatus(BillOfLading billOfLading)
		{
			return "";
		}

		public string GetUserFriendlyStatusMessage(BillOfLading billOfLading)
		{
			return "";
		}

		public bool ShouldShow(BillOfLading billOfLading)
		{
			return false;
		}
	}
}


