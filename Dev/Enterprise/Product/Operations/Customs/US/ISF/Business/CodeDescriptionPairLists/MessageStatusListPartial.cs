
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Business
{
	partial class MessageStatusList : Integration.Customs.US.ISF.IMessageStatusCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public static bool HasBeenLodgedAtCustoms(string status)
		{
			return !string.IsNullOrEmpty(status) &&
				status != Codes.NotSentISF &&
				status != Codes.AwaitingISFAdd &&
				status != Codes.ErrorISFAdd;
		}

		public static bool HasActiveMessageInCustoms(string status)
		{
			return HasBeenLodgedAtCustoms(status) &&
				status != Codes.ClearISFDelete &&
				status != Codes.ClearWithWarningISFDelete;
		}

		public static bool IsStatusClearWithWarning(string status)
		{
			return status == Codes.ClearWithWarningISFDelete ||
				status == Codes.ClearWithWarningISFAdd ||
				status == Codes.ClearWithWarningISFReplace;
		}

		public static bool IsStatusClear(string status)
		{
			return status == Codes.ClearISFDelete ||
				status == Codes.ClearISFAdd ||
				status == Codes.ClearISFReplace;
		}

		public static bool IsWaitingForResponse(string status)
		{
			return
				status == Codes.AwaitingISFDelete ||
				status == Codes.AwaitingISFAdd ||
				status == Codes.AwaitingISFReplace;
		}

		public static bool IsDeleteStatus(string status)
		{
			return
				status == Codes.AwaitingISFDelete ||
				status == Codes.ClearISFDelete ||
				status == Codes.ErrorISFDelete;
		}

		public static string GetCodeFrom(string messageSubType, string isfMessageStatus)
		{
			string result = "";
			if (!string.IsNullOrEmpty(messageSubType) && !string.IsNullOrEmpty(isfMessageStatus))
			{
				switch (messageSubType)
				{
					case EM_MessageSubTypeList.Codes.ISFAdd:
						switch (isfMessageStatus)
						{
							case ISFMessageStatus.Codes.Accepted:
								result = Codes.ClearISFAdd;
								break;
							case ISFMessageStatus.Codes.AcceptedWithWarning:
								result = Codes.ClearWithWarningISFAdd;
								break;
							case ISFMessageStatus.Codes.Rejected:
								result = Codes.ErrorISFAdd;
								break;
						}
						break;
					case EM_MessageSubTypeList.Codes.ISFDelete:
						switch (isfMessageStatus)
						{
							case ISFMessageStatus.Codes.Accepted:
								result = Codes.ClearISFDelete;
								break;
							case ISFMessageStatus.Codes.AcceptedWithWarning:
								result = Codes.ClearWithWarningISFDelete;
								break;
							case ISFMessageStatus.Codes.Rejected:
								result = Codes.ErrorISFDelete;
								break;
						}
						break;
					case EM_MessageSubTypeList.Codes.ISFReplace:
						switch (isfMessageStatus)
						{
							case ISFMessageStatus.Codes.Accepted:
								result = Codes.ClearISFReplace;
								break;
							case ISFMessageStatus.Codes.AcceptedWithWarning:
								result = Codes.ClearWithWarningISFReplace;
								break;
							case ISFMessageStatus.Codes.Rejected:
								result = Codes.ErrorISFReplace;
								break;
						}
						break;
				}
			}
			return result;
		}

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList() => this;
	}
}
