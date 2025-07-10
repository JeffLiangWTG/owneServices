namespace Enterprise.MasterFiles.Business.Testing
{
	public class DummySmsSender : SmsSender
	{
		public static void RegisterThisSubTypeOverride()
		{
			NewSmsSenderDelegate = GetNewDummySmsSender;
		}

		public static void UnregisterThisSubTypeOverride()
		{
			NewSmsSenderDelegate = null;
		}

		static DummySmsSender GetNewDummySmsSender()
		{
			return new DummySmsSender();
		}

		protected override SmsSendResult SendCore(Sms sms)
		{
			return new SmsSendResult();
		}
	}
}
