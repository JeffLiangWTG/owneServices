using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(TrackingHAWBFilterBusinessObject))]
	[HttpContextEnabledTest]
	sealed class TrackingHAWBFilterStripTest : FilterStripBusinessObjectTestCase
	{
		#region Branch

		public void TestBranchFilter()
		{
			var testHelper = new TestHelper(Factory);
			((TrackingHAWBFilterBusinessObject)FilterStripBizO).LoggedInWebUser = testHelper.TestSiteUser;

			var branch1 = GlbBranch.CurrentBranch;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();

			var hawb1 = Factory.NewWithValidTestData<ExportAWBHeader>();
			hawb1.EH_AWBType = AWBTypeList.Codes.House;
			hawb1.EH_GB_UserBranch = branch1.PK;

			var hawb2 = Factory.NewWithValidTestData<ExportAWBHeader>();
			hawb2.EH_AWBType = AWBTypeList.Codes.House;
			hawb2.EH_GB_UserBranch = branch2.PK;

			Factory.Save();

			var filter = (ModuleGuidFilter)FilterStripBizO["Branch"];

			var hawbs = new TrackingMAWBHeaderCollection(Factory);
			hawbs.Load(FilterStripBizO.Filter);

			AssertEquals("HAWB1 is in Collection", true, hawbs.Contains(hawb1.PK));
			AssertEquals("HAWB2 is not in Collection", false, hawbs.Contains(hawb2.PK));

			var securityRight = GetSecurityContacts(testHelper.TestSiteUser.LoggedInUser, WebSecurityRightsList.WebHAWBAdmin);
			securityRight.OZ_Granted = true;

			ResetFilterStripBizO();

			filter = (ModuleGuidFilter)FilterStripBizO["Branch"];

			hawbs = new TrackingMAWBHeaderCollection(Factory);
			hawbs.Load(FilterStripBizO.Filter);

			AssertEquals("HAWB1 is in Collection", true, hawbs.Contains(hawb1.PK));
			AssertEquals("HAWB2 is in Collection", true, hawbs.Contains(hawb2.PK));
		}

		#endregion

		#region Implementation

		OrgSecurityContacts GetSecurityContacts(OrgContact contact, WebSecurityRight securityRight)
		{
			OrgSecurityContacts result = null;
			foreach (OrgSecurityContacts security in contact.SecurityRightsForBindingOnly)
			{
				if (security.Security.OX_SecurityItemName == securityRight.Code)
				{
					result = security;
					break;
				}
			}
			AssertNotNull("Failed to find WebSecurityRight - " + securityRight.Code, result);
			return result;
		}

		FilterStripBusinessObject FilterStripBizO
		{
			get
			{
				if (fFilterStripBizO == null)
				{
					fFilterStripBizO = GetNewFilterStripBusinessObject();
				}
				return fFilterStripBizO;
			}
		}
		FilterStripBusinessObject fFilterStripBizO;

		void ResetFilterStripBizO()
		{
			fFilterStripBizO = null;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new TrackingHAWBFilterBusinessObject();
		}

		#endregion
	}
}
