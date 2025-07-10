using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ExternalRequest))]
	internal class ExternalRequestTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		public override BusinessObject GetNewBusinessObjectSafeSaving()
		{
			return externalRequest;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return externalRequest;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return externalRequest;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORGBKDKB1";
			var type = Factory.NewWithValidTestData<ExternalRequestType>();
			Factory.Save();

			externalRequest = Factory.New<ExternalRequest>();
			externalRequest.REQ_RequestID = "REQ00001";
			externalRequest.REQ_ParentID = ZGuid.NewZGuid();
			externalRequest.REQ_ParentTableCode = "JSB";
			externalRequest.REQ_Description = "test request";
			externalRequest.REQ_OH_AssignedOrganization = org.PK;
			externalRequest.REQ_OH_ReviewerOrganization = org.PK;
			externalRequest.REQ_RQT_Type = type.PK;
		}

		ExternalRequest externalRequest;

		#endregion
	}
}
