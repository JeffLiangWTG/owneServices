using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class UpdateActionCodeConverterTest : TestCase
	{
		public void TestConvertToString()
		{
			AssertEquals(UpdateActionCodeConverter.AddCode, UpdateActionCodeConverter.ConvertToString(UpdateActionCode.Add));
			AssertEquals(UpdateActionCodeConverter.DeleteCode, UpdateActionCodeConverter.ConvertToString(UpdateActionCode.Delete));
			AssertEquals(UpdateActionCodeConverter.ReplaceCode, UpdateActionCodeConverter.ConvertToString(UpdateActionCode.Replace));
			AssertEquals(UpdateActionCodeConverter.UpdateCode, UpdateActionCodeConverter.ConvertToString(UpdateActionCode.Update));
			AssertEquals(UpdateActionCodeConverter.MerchandiseZoneStatusChangeCode, UpdateActionCodeConverter.ConvertToString(UpdateActionCode.MerchandiseZoneStatusChange));
		}
	}
}
