using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class MessageTypeCheckTest : TestCase
	{
		public void TestNeedToCreateInterchangeHeaderText()
		{
			string[] messageTypesNeedToCreateInterchangeHeaderText = { TRMessageTypes.Codes.TCD, TRMessageTypes.Codes.TRB, TRMessageTypes.Codes.TRD, TRMessageTypes.Codes.TRE, TRMessageTypes.Codes.TRI, TRMessageTypes.Codes.TRL, TRMessageTypes.Codes.TRQ, TRMessageTypes.Codes.TRS, TRMessageTypes.Codes.TSP };
			var messageTypesets = new HashSet<string>(messageTypesNeedToCreateInterchangeHeaderText);
			foreach (var typeCode in messageTypes.GetAllCodes())
			{
				if (messageTypesets.Contains(typeCode))
				{
					AssertEquals(true, TRMessageTypes.NeedToCreateInterchangeHeaderText(typeCode));
				}
				else
				{
					AssertEquals(false, TRMessageTypes.NeedToCreateInterchangeHeaderText(typeCode));
				}
			}
		}

		public void TestNeedToPreprocessMessageText()
		{
			string[] messageTypesNeedToPreprocessMessageText = { TRMessageTypes.Codes.T1O, TRMessageTypes.Codes.T2O, TRMessageTypes.Codes.T3O, TRMessageTypes.Codes.TRM };
			var messageTypesets = new HashSet<string>(messageTypesNeedToPreprocessMessageText);
			foreach (var typeCode in messageTypes.GetAllCodes())
			{
				if (messageTypesets.Contains(typeCode))
				{
					AssertEquals(true, TRMessageTypes.NeedToPreprocessMessageText(typeCode));
				}
				else
				{
					AssertEquals(false, TRMessageTypes.NeedToPreprocessMessageText(typeCode));
				}
			}
		}

		readonly TRMessageTypes messageTypes = new TRMessageTypes();
	}
}
