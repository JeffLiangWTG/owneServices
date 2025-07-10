using System;
using System.Collections.Generic;
using System.Web;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.ServerServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class AWBOtherChargeCodeLookupWSMethodTest : TrackingWebServiceMethodTest<AWBOtherChargeCodeLookupWSMethod>
	{
		#region Implementation

		protected override void SetMethodParametersAndExpectedResponseTokens(Dictionary<string, WebServiceResponse> setting)
		{
			var testParameters = new AWBOtherChargeCodeLookupParameters();
			testParameters.CodeControlID = "";
			testParameters.CodeValue = "";
			testParameters.DescriptionControlID = "";
			testParameters.EntitlementCodeControlID = "";
			testParameters.PrepaidCollectFlag = "";
			testParameters.SessionIndex = "";
			WebServiceResponse expectedResponse = new WebServiceResponse();
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters.CodeControlID = "CodeControlID";
			expectedResponse = new WebServiceResponse();
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters.DescriptionControlID = "DescriptionControlID";
			expectedResponse = new WebServiceResponse();
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters.EntitlementCodeControlID = "EntitlementCodeControlID";
			expectedResponse = new WebServiceResponse();
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters.PrepaidCollectFlag = "C";
			expectedResponse = new WebServiceResponse();
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters.SessionIndex = "12345";
			expectedResponse = new WebServiceResponse();
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters.CodeValue = "TST";
			expectedResponse = new WebServiceResponse();
			setting.Add(testParameters.ToString(), expectedResponse);

			var awbChargeCodes = new CodeDescriptionPairList(OLookUpEditType.AWBChargeCodes);
			AssertNotNull(awbChargeCodes);
			Assert(awbChargeCodes.Count > 0);
			testParameters.CodeValue = awbChargeCodes[0].Code;
			expectedResponse = new WebServiceResponse();
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters.SessionIndex = SessionIndexer;
			expectedResponse = new WebServiceResponse();
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters.ChargePK = "abcdefg";
			expectedResponse = new WebServiceResponse();
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters.ChargePK = TestCharge.PK.ToString();
			expectedResponse = new WebServiceResponse();
			var updateDescriptionToken = new UpdateValueResponseToken("DescriptionControlID", awbChargeCodes[0].Description);
			updateDescriptionToken.Conditions.Add(new ResponseConditionEqualToken("CodeControlID", awbChargeCodes[0].Code));
			expectedResponse.Add(updateDescriptionToken);
			var updateEntitlementCodeToken = new UpdateValueResponseToken("EntitlementCodeControlID", "C");
			updateEntitlementCodeToken.Conditions.Add(new ResponseConditionEqualToken("CodeControlID", awbChargeCodes[0].Code));
			expectedResponse.Add(updateEntitlementCodeToken);
			setting.Add(testParameters.ToString(), expectedResponse);
		}

		protected override bool MethodNeverReturnsErrors
		{
			get { return true; }
		}

		protected override string GetExpectedMethodSpecificServiceScriptFileName()
		{
			return "LookupAWBOtherChargeCodeWSM.js";
		}

		protected override string GetExpectedMethodName()
		{
			return "LookupAWBOtherChargeCode";
		}

		protected override AWBOtherChargeCodeLookupWSMethod GetNewWebServiceMethod()
		{
			return new AWBOtherChargeCodeLookupWSMethod();
		}

		protected override void SetUp()
		{
			base.SetUp();
			SessionIndexer = Guid.NewGuid().ToString();
		}

		string SessionIndexer;

		void StoreMAWBInSession(TrackingMAWBHeader mawb, string indexer)
		{
			if (!string.IsNullOrEmpty(indexer))
			{
				HttpContext.Current.Session[indexer] = mawb;
			}
		}

		ExportAWBOtherCharges TestCharge
		{
			get
			{
				return TestMAWB.AWBOtherCharges[1];
			}
		}

		TrackingMAWBHeader TestMAWB
		{
			get
			{
				if (testMAWB == null)
				{
					testMAWB = Factory.NewWithValidTestData<TrackingMAWBHeader>();
					testMAWB.AWBOtherCharges.AddNew();
					testMAWB.AWBOtherCharges.AddNew();
					testMAWB.AWBOtherCharges.AddNew();
					StoreMAWBInSession(testMAWB, SessionIndexer);
				}
				return testMAWB;
			}
		}

		TrackingMAWBHeader testMAWB;

		#endregion
	}
}
