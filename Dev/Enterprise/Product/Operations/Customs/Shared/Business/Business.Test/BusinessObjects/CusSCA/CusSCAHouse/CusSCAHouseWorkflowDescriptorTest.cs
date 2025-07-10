using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusSCAHouseWorkflowDescriptor))]
	sealed class CusSCAHouseWorkflowDescriptorTest : WorkflowDescriptorTestCase<CusSCAHouseWorkflowDescriptor>
	{
		#region ID/requirement
		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Sea Cargo House", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", "SCU", WorkflowDescriptor.Code);
		}

		public void TestControllerID()
		{
			AssertControllerID(Core.Constants.CountryCodes.Australia, ControllerIDs.Customs.AU.SeaCargoHouseController);
			AssertControllerID(Core.Constants.CountryCodes.SouthAfrica, null);

			void AssertControllerID(string countryCode, ControllerID expectedControllerID)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					AssertEquals($"ControllerID for {expectedControllerID}", expectedControllerID, WorkflowDescriptor.ControllerID);
				}
			}
		}

		public override void TestRequiresBranch()
		{
			Assert("Requires Branch", WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			Assert("Requires Client", WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			Assert("Doesn't require Department", !WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresPorts()
		{
			Assert("Requires Port 1", WorkflowDescriptor.RequiresPort1);
			Assert("Requires Port 2", WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSubTypes()
		{
			AssertEquals("No subtypes", 0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestSupportsEventTracking()
		{
			Assert("Supports Event Tracking", WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestWorkflowProviderType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var provider = (IWorkflowProvider)Factory.New<Integration.Customs.AU.ICusSCAHouse>();
				Assert(WorkflowDescriptor.WorkflowProviderType.IsAssignableFrom(provider.GetType()));
				AssertEquals(WorkflowDescriptor.Code, provider.WorkflowType);
				AssertEquals(ObjectFactory.GetType<Integration.Customs.AU.ICusSCAHouse>(), WorkflowDescriptor.WorkflowProviderType);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				var provider = (IWorkflowProvider)Factory.New<Integration.Customs.NZ.ICusSCAHouse>();
				Assert(WorkflowDescriptor.WorkflowProviderType.IsAssignableFrom(provider.GetType()));
				AssertEquals(WorkflowDescriptor.Code, provider.WorkflowType);
				AssertEquals(typeof(BaseCusSCAHouse), WorkflowDescriptor.WorkflowProviderType);
			}
		}
		#endregion

		new CusSCAHouseWorkflowDescriptor WorkflowDescriptor => (CusSCAHouseWorkflowDescriptor)base.WorkflowDescriptor;

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest() => new[] { (IWorkflowProvider)Factory.New<Integration.Customs.AU.ICusSCAHouse>() };

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.OrgProxy;
	}
}
