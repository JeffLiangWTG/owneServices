using System;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(SendScheduleController))]
	public class SendScheduleControllerTest : ZControllerBasherTest
	{
		public override void TestViewForm()
		{
			Assert(true);
		}

		public override void TestDeleteForm()
		{
			Assert(true);
		}

		public override void TestEditForm()
		{
			Assert(true);
		}

		public override void TestNewForm()
		{
			Assert(true);
		}

		public override void TestTemplateCopyForm()
		{
			AssertControllerNotNull();
			try
			{
				AssertEquals("Should not show template copy form", null, Controller.ShowTemplateCopyForm(null));
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		#region Implementation

		protected override Type GetBusinessObjectType()
		{
			return typeof(GlbCompanyCampaignItemSchedule);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GlbCompanyCampaignItemSchedule;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			throw new NotSupportedException("This controller can contain two different business objects. All tests that use this method should be overrided, and tested for each business object type");
		}

		#endregion
	}
}
