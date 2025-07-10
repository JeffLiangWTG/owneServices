using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ExternalRequestType))]
	internal class ExternalRequestTypeTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		public override BusinessObject GetNewBusinessObjectSafeSaving()
		{
			return externalRequestType;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return externalRequestType;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return externalRequestType;
		}

		protected override void SetUp()
		{
			base.SetUp();

			externalRequestType = Factory.NewWithValidTestData<ExternalRequestType>();
			externalRequestType.RQT_Code = "XXX";
			externalRequestType.RQT_Description = "XXX1 DESC";
			externalRequestType.RQT_JobType = ExternalRequestTypeJobTypes.Codes.ORD;
			externalRequestType.RQT_IsActive = true;
			externalRequestType.RQT_Assignee = DocAddressTypes.Codes.BuyerDocumentaryAddress;
			externalRequestType.RQT_Reviewer = DocAddressTypes.Codes.SupplierDocumentaryAddress;
		}

		ExternalRequestType externalRequestType;

		#endregion

		public void TestVerifyTextChange_ExternalRequestTypeToRequestType()
		{
			var externalRequestTypeHumanReadableName = Factory.NewWithValidTestData<ExternalRequestType>().HumanReadableName.ToString();
			NUnit.Framework.Assert.Multiple(() =>
			{
				AssertEquals("Request Type", externalRequestTypeHumanReadableName.Remove(externalRequestTypeHumanReadableName.Length - 3).TrimEnd());
				AssertEquals("Request Types", ModuleIDs.ExternalRequestTypes.Description.ToString());
			});
		}

		public void TestCheckReadOnlyFields_RequestTypeCode()
		{
			var requestType = Factory.New<ExternalRequestType>();
			requestType.RQT_Code = "RON";
			requestType.RQT_Description = "Readonly Desc";
			requestType.RQT_JobType = ExternalRequestTypeJobTypes.Codes.SBK;
			AssertEquals(false, requestType.RQT_CodeInfo.ReadOnly);

			Factory.Save();

			AssertEquals(true, requestType.RQT_CodeInfo.ReadOnly);
		}

		public void TestCheckReadOnlyFields_RequestTypeJobType()
		{
			var requestType = Factory.New<ExternalRequestType>();
			requestType.RQT_Code = "RON";
			requestType.RQT_Description = "Readonly Desc";
			requestType.RQT_JobType = ExternalRequestTypeJobTypes.Codes.SBK;
			Factory.Save();

			AssertEquals(false, requestType.RQT_JobTypeInfo.ReadOnly);

			CreateRequestData(requestType.PK);
			Factory.Save();

			AssertEquals(true, requestType.RQT_JobTypeInfo.ReadOnly);
		}

		void CreateRequestData(ZGuid requestTypePk)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORGBKDKB1";
			Factory.Save();

			var externalRequest = Factory.New<ExternalRequest>();
			externalRequest.REQ_RequestID = "REQ00001";
			externalRequest.REQ_ParentID = ZGuid.NewZGuid();
			externalRequest.REQ_ParentTableCode = "JSB";
			externalRequest.REQ_Description = "test request";
			externalRequest.REQ_OH_AssignedOrganization = org.PK;
			externalRequest.REQ_OH_ReviewerOrganization = org.PK;
			externalRequest.REQ_RQT_Type = requestTypePk;
			Factory.Save();
		}
	}
}
