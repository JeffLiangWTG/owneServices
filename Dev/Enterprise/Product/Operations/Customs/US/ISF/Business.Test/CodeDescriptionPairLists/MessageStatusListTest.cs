using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class MessageStatusListTest : TestCase
	{
		public void TestGetCodeFrom()
		{
			var values = new string[][]
			{
				new string[] { MessageStatusList.Codes.ClearISFAdd, EM_MessageSubTypeList.Codes.ISFAdd, ISFMessageStatus.Codes.Accepted },
				new string[] { MessageStatusList.Codes.ClearWithWarningISFAdd, EM_MessageSubTypeList.Codes.ISFAdd, ISFMessageStatus.Codes.AcceptedWithWarning },
				new string[] { MessageStatusList.Codes.ErrorISFAdd, EM_MessageSubTypeList.Codes.ISFAdd, ISFMessageStatus.Codes.Rejected },
				new string[] { MessageStatusList.Codes.ClearISFReplace, EM_MessageSubTypeList.Codes.ISFReplace, ISFMessageStatus.Codes.Accepted },
				new string[] { MessageStatusList.Codes.ClearWithWarningISFReplace, EM_MessageSubTypeList.Codes.ISFReplace, ISFMessageStatus.Codes.AcceptedWithWarning },
				new string[] { MessageStatusList.Codes.ErrorISFReplace, EM_MessageSubTypeList.Codes.ISFReplace, ISFMessageStatus.Codes.Rejected },
				new string[] { MessageStatusList.Codes.ClearISFDelete, EM_MessageSubTypeList.Codes.ISFDelete, ISFMessageStatus.Codes.Accepted },
				new string[] { MessageStatusList.Codes.ClearWithWarningISFDelete, EM_MessageSubTypeList.Codes.ISFDelete, ISFMessageStatus.Codes.AcceptedWithWarning },
				new string[] { MessageStatusList.Codes.ErrorISFDelete, EM_MessageSubTypeList.Codes.ISFDelete, ISFMessageStatus.Codes.Rejected }
			};
			foreach (string[] value in values)
			{
				AssertEquals("MessageStatusList.GetCodeFrom(" + value[1] + ", " + value[2] + ")", value[0], MessageStatusList.GetCodeFrom(value[1], value[2]));
			}
		}

		public void TestIsStatusClear()
		{
			AssertEquals("IsStatusClear", true, MessageStatusList.IsStatusClear(MessageStatusList.Codes.ClearISFDelete));
			AssertEquals("IsStatusClear", false, MessageStatusList.IsStatusClear(MessageStatusList.Codes.ErrorISFDelete));
			AssertEquals("IsStatusClear", true, MessageStatusList.IsStatusClear(MessageStatusList.Codes.ClearISFAdd));
			AssertEquals("IsStatusClear", false, MessageStatusList.IsStatusClear(MessageStatusList.Codes.ErrorISFAdd));
			AssertEquals("IsStatusClear", true, MessageStatusList.IsStatusClear(MessageStatusList.Codes.ClearISFReplace));
			AssertEquals("IsStatusClear", false, MessageStatusList.IsStatusClear(MessageStatusList.Codes.ErrorISFReplace));
		}

		public void TestIsStatusClearWithWarning()
		{
			AssertEquals("IsStatusClear", true, MessageStatusList.IsStatusClearWithWarning(MessageStatusList.Codes.ClearWithWarningISFDelete));
			AssertEquals("IsStatusClear", false, MessageStatusList.IsStatusClearWithWarning(MessageStatusList.Codes.ErrorISFDelete));
			AssertEquals("IsStatusClear", true, MessageStatusList.IsStatusClearWithWarning(MessageStatusList.Codes.ClearWithWarningISFAdd));
			AssertEquals("IsStatusClear", false, MessageStatusList.IsStatusClearWithWarning(MessageStatusList.Codes.ErrorISFAdd));
			AssertEquals("IsStatusClear", true, MessageStatusList.IsStatusClearWithWarning(MessageStatusList.Codes.ClearWithWarningISFReplace));
			AssertEquals("IsStatusClear", false, MessageStatusList.IsStatusClearWithWarning(MessageStatusList.Codes.ErrorISFReplace));
		}

		public void TestIsWaitingForResponse()
		{
			AssertEquals("IsWaitingForResponse", true, MessageStatusList.IsWaitingForResponse(MessageStatusList.Codes.AwaitingISFDelete));
			AssertEquals("IsWaitingForResponse", false, MessageStatusList.IsWaitingForResponse(MessageStatusList.Codes.ErrorISFDelete));
			AssertEquals("IsWaitingForResponse", true, MessageStatusList.IsWaitingForResponse(MessageStatusList.Codes.AwaitingISFAdd));
			AssertEquals("IsWaitingForResponse", false, MessageStatusList.IsWaitingForResponse(MessageStatusList.Codes.ErrorISFAdd));
			AssertEquals("IsWaitingForResponse", true, MessageStatusList.IsWaitingForResponse(MessageStatusList.Codes.AwaitingISFReplace));
			AssertEquals("IsWaitingForResponse", false, MessageStatusList.IsWaitingForResponse(MessageStatusList.Codes.ErrorISFReplace));
		}

		public void TestIsDeleteStatus()
		{
			AssertEquals("IsWithdrawalStatus", true, MessageStatusList.IsDeleteStatus(MessageStatusList.Codes.AwaitingISFDelete));
			AssertEquals("IsWithdrawalStatus", false, MessageStatusList.IsDeleteStatus(MessageStatusList.Codes.AwaitingISFAdd));
			AssertEquals("IsWithdrawalStatus", true, MessageStatusList.IsDeleteStatus(MessageStatusList.Codes.ErrorISFDelete));
			AssertEquals("IsWithdrawalStatus", false, MessageStatusList.IsDeleteStatus(MessageStatusList.Codes.ErrorISFAdd));
			AssertEquals("IsWithdrawalStatus", true, MessageStatusList.IsDeleteStatus(MessageStatusList.Codes.ClearISFDelete));
			AssertEquals("IsWithdrawalStatus", false, MessageStatusList.IsDeleteStatus(MessageStatusList.Codes.ErrorISFReplace));
		}

		public void TestHasBeenLodgedAtCustoms()
		{
			AssertEquals(false, MessageStatusList.HasBeenLodgedAtCustoms(""));
			AssertEquals(true, MessageStatusList.HasBeenLodgedAtCustoms(MessageStatusList.Codes.AwaitingISFDelete));
			AssertEquals(false, MessageStatusList.HasBeenLodgedAtCustoms(MessageStatusList.Codes.AwaitingISFAdd));
			AssertEquals(true, MessageStatusList.HasBeenLodgedAtCustoms(MessageStatusList.Codes.AwaitingISFReplace));
			AssertEquals(false, MessageStatusList.HasBeenLodgedAtCustoms(MessageStatusList.Codes.ErrorISFAdd));
			AssertEquals(true, MessageStatusList.HasBeenLodgedAtCustoms(MessageStatusList.Codes.ErrorISFDelete));
			AssertEquals(false, MessageStatusList.HasBeenLodgedAtCustoms(MessageStatusList.Codes.NotSentISF));
		}

		public void TestHasActiveMessageInCustoms()
		{
			AssertEquals(false, MessageStatusList.HasActiveMessageInCustoms(""));
			AssertEquals(true, MessageStatusList.HasActiveMessageInCustoms(MessageStatusList.Codes.AwaitingISFDelete));
			AssertEquals(false, MessageStatusList.HasActiveMessageInCustoms(MessageStatusList.Codes.AwaitingISFAdd));
			AssertEquals(true, MessageStatusList.HasActiveMessageInCustoms(MessageStatusList.Codes.AwaitingISFReplace));
			AssertEquals(false, MessageStatusList.HasActiveMessageInCustoms(MessageStatusList.Codes.ErrorISFAdd));
			AssertEquals(true, MessageStatusList.HasActiveMessageInCustoms(MessageStatusList.Codes.ErrorISFDelete));
			AssertEquals(false, MessageStatusList.HasActiveMessageInCustoms(MessageStatusList.Codes.ClearISFDelete));
			AssertEquals(false, MessageStatusList.HasActiveMessageInCustoms(MessageStatusList.Codes.ClearWithWarningISFDelete));
		}

		public void TestICodeDescriptionPairListProviderMembers()
		{
			var list = new MessageStatusList();
			AssertEquals(list, list.GetCodeDescriptionPairList());
		}
	}
}
