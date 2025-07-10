using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class AsycudaMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var typeDecider = new AsycudaMessageTypeDecider();
			var message = Factory.New<TWMessage>();
			var row = ((INeedRow)message).Row;
			var testCases = new Dictionary<string, Type>
			{
				{ MessageTypeList.Codes.FCF, typeof(AsycudaMessage) },
				{ MessageTypeList.Codes.FHR, typeof(AsycudaMessage) },
				{ MessageTypeList.Codes.FHM, typeof(AsycudaMessage) },
				{ string.Empty, typeof(TWMessage) }
			};

			CombineAssertions(() =>
			{
				foreach (var testCase in testCases)
				{
					message.EM_MessageType = testCase.Key;
					AssertEquals($"MessageType For {(string.IsNullOrEmpty(testCase.Key) ? "Empty" : testCase.Key)}", testCase.Value, typeDecider.GetTypeForLoad(row, Factory));
				}
			});
		}
	}
}
