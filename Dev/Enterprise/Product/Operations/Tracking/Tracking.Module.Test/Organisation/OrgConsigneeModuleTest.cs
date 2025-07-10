using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Modules;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(OrgConsigneeModule))]
	sealed class OrgConsigneeModuleTest : OrganisationModuleTest
	{
		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			Factory.Save();
		}

		#endregion Setup

		#region Overrides

		protected override bool AllowActiveStatusFilterTest()
		{
			return false;
		}

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.OrgConsigneeTracking; }
		}

		protected override ZBool ExpectMatchConsignors
		{
			get { return ZBool.False; }
		}

		#endregion
	}
}
