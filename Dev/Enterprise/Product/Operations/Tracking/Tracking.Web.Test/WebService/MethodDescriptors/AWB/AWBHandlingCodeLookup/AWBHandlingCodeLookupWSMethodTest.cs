using System.Collections.Generic;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Tracking.Web.ServerServices;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class AWBHandlingCodeLookupWSMethodTest : TrackingWebServiceMethodTest<AWBHandlingCodeLookupWSMethod>
	{
		#region Implementation

		protected override void SetMethodParametersAndExpectedResponseTokens(Dictionary<string, WebServiceResponse> setting)
		{
			var testParameters = new AWBHandlingCodeLookupParameters();
			testParameters.CodeControlID = "";
			testParameters.CodeValue = "";
			testParameters.DescriptionControlID = "";
			WebServiceResponse expectedResponse = new WebServiceResponse();
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters.CodeControlID = "CodeControlID";
			expectedResponse = new WebServiceResponse();
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters.DescriptionControlID = "DescriptionControlID";
			expectedResponse = new WebServiceResponse();
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters.CodeValue = AWBSpecialHandlingCodeDescriptionPairList.Codes.AircraftOnGround;
			expectedResponse = new WebServiceResponse();
			var updateToken = new UpdateValueResponseToken("DescriptionControlID", AWBSpecialHandlingCodeDescriptionPairList.Descriptions.AircraftOnGround);
			updateToken.Conditions.Add(new ResponseConditionEqualToken("CodeControlID", AWBSpecialHandlingCodeDescriptionPairList.Codes.AircraftOnGround));
			expectedResponse.Add(updateToken);
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters.CodeValue = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoAircraftOnly;
			expectedResponse = new WebServiceResponse();
			updateToken = new UpdateValueResponseToken("DescriptionControlID", AWBSpecialHandlingCodeDescriptionPairList.Descriptions.CargoAircraftOnly);
			updateToken.Conditions.Add(new ResponseConditionEqualToken("CodeControlID", AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoAircraftOnly));
			expectedResponse.Add(updateToken);
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters.CodeValue = "TST";
			expectedResponse = new WebServiceResponse();
			setting.Add(testParameters.ToString(), expectedResponse);
		}

		protected override bool MethodNeverReturnsErrors
		{
			get { return true; }
		}

		protected override string GetExpectedMethodSpecificServiceScriptFileName()
		{
			return "LookupAWBHandlingCodeWSM.js";
		}

		protected override string GetExpectedMethodName()
		{
			return "LookupAWBHandlingCode";
		}

		protected override AWBHandlingCodeLookupWSMethod GetNewWebServiceMethod()
		{
			return new AWBHandlingCodeLookupWSMethod();
		}

		#endregion
	}
}
