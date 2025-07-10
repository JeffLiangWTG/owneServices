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
	[TestedType(typeof(TrackingMAWBFilterBusinessObject))]
	[HttpContextEnabledTest]
	sealed class TrackingMAWBFilterStripTest : FilterStripBusinessObjectTestCase
	{
		#region Branch

		public void TestBranchFilter()
		{
			var testHelper = new TestHelper(Factory);
			((TrackingMAWBFilterBusinessObject)FilterStripBizO).LoggedInWebUser = testHelper.TestSiteUser;

			var branch1 = GlbBranch.CurrentBranch;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();

			var mawb1 = Factory.NewWithValidTestData<ExportAWBHeader>();
			mawb1.EH_AWBType = AWBTypeList.Codes.AgentMaster;
			mawb1.EH_GB_UserBranch = branch1.PK;

			var mawb2 = Factory.NewWithValidTestData<ExportAWBHeader>();
			mawb2.EH_AWBType = AWBTypeList.Codes.AgentMaster;
			mawb2.EH_GB_UserBranch = branch2.PK;

			Factory.Save();

			var filter = (ModuleGuidFilter)FilterStripBizO["Branch"];

			var mawbs = new TrackingMAWBHeaderCollection(Factory);
			mawbs.Load(FilterStripBizO.Filter);

			AssertEquals("MAWB1 is in Collection", true, mawbs.Contains(mawb1.PK));
			AssertEquals("MAWB2 is not in Collection", false, mawbs.Contains(mawb2.PK));

			var securityRight = GetSecurityContacts(testHelper.TestSiteUser.LoggedInUser, WebSecurityRightsList.WebMAWBAdmin);
			securityRight.OZ_Granted = true;

			ResetFilterStripBizO();

			filter = (ModuleGuidFilter)FilterStripBizO["Branch"];

			mawbs = new TrackingMAWBHeaderCollection(Factory);
			mawbs.Load(FilterStripBizO.Filter);

			AssertEquals("MAWB1 is in Collection", true, mawbs.Contains(mawb1.PK));
			AssertEquals("MAWB2 is in Collection", true, mawbs.Contains(mawb2.PK));
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

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new TrackingMAWBFilterBusinessObject();
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

		#endregion
	}
}
