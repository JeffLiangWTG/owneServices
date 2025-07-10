namespace Enterprise.MasterFiles.Business
{
	public partial class TriggerUserContextList
	{
		public static bool IsTypeNeedingUserContext(string code)
		{
			switch (code)
			{
				case TriggerUserContextList.Codes.Specified:
					return true;
				default:
					return false;
			}
		}
	}
}
