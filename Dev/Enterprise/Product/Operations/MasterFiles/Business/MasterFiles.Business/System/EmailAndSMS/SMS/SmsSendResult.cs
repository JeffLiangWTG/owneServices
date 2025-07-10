using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public struct SmsSendResult
	{
		public SmsSendResult(bool success, ZString message)
		{
			Success = success;
			Message = message;
		}

		public bool Success;
		public ZString Message;
	}
}
