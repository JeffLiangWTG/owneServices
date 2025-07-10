using System.Collections.Generic;
using System.Net;
using System.Net.Security;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	class S8ClientForGeneralTest : S8Client
	{
		public S8ClientForGeneralTest() : base()
		{
		}

		public MethodCallResult<int> TestRequest()
		{
			string serviceCall(IFlightScheduleClient client, string token) => TestServiceCall();
			MethodCallResult<int> parse(string response) => TestParse();

			return RequestAndParse(serviceCall, parse, new S8ServiceRequestManager(), false);
		}

		public List<string> ServiceCallResults { get; set; }
		public List<MethodCallResult<int>> ParseResults { get; set; }

		int serviceCallResultIndex;
		int parseResultIndex;

		string TestServiceCall()
		{
			var result = ServiceCallResults[serviceCallResultIndex++];

			if (serviceCallResultIndex == ServiceCallResults.Count)
			{
				serviceCallResultIndex = 0;
			}

			return result;
		}

		MethodCallResult<int> TestParse()
		{
			var result = ParseResults[parseResultIndex++];

			if (parseResultIndex == ParseResults.Count)
			{
				parseResultIndex = 0;
			}

			return result;
		}

		public string LoginToken { get; set; }
		public bool LoginCalled { get; set; }
		public RemoteCertificateValidationCallback ServerCertificateValidationResult { get; set; }

		protected override string LoginServiceCall()
		{
			ServerCertificateValidationResult = ServicePointManager.ServerCertificateValidationCallback;
			LoginCalled = true;
			return LoginToken;
		}

		public bool DefaultCredentialsAreUsedOrNoProxy { get; private set; }
	}
}
