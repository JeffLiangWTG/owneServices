using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(Phase))]
	public class PhaseTest : RegistryBusinessObjectTestCaseBase
	{
		public void TestCanNotDeleteWhenDefinedAsDPSUpdatingPhase()
		{
			var phaseSecurity = DpsStatusUpdateSettingRegistryItemTest.GetPhaseSecurity();
			phaseSecurity.DependantsProvider = new ShipmentPhaseDependantsProvider();
			var phaseToDelete = phaseSecurity.Phases[0];

			using (ForwardingConfigurationRegistry.Instance.ShipmentPhaseSecurity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, phaseSecurity))
			{
				var dpsUpdateSetting = new DpsStatusUpdateSetting(ForwardingConfigurationRegistry.Instance.ShipmentPhaseSecurity);
				dpsUpdateSetting.JobUpdateSettings[0].ShouldUpdate = true;
				Factory.Save();
				using (ForwardingConfigurationRegistry.Instance.ShipmentDpsStatusUpdateSetting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dpsUpdateSetting))
				{
					Assert(!phaseToDelete.CanDelete);
					AssertEquals("This phase is selected as a DPS Updating Phase, please un-select it under \"Master Data -> Organizations -> Denied Party Screening -> Shipment DPS Update\" before deleting it.", phaseToDelete.ReasonForNotAbleToDelete.GetUnresolvedString());
				}
			}

			phaseSecurity = DpsStatusUpdateSettingRegistryItemTest.GetPhaseSecurity();
			phaseSecurity.DependantsProvider = new ConsolPhaseDependantsProvider();
			phaseToDelete = phaseSecurity.Phases[0];

			using (ForwardingConfigurationRegistry.Instance.ConsolPhaseSecurity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, phaseSecurity))
			{
				var dpsUpdateSetting = new DpsStatusUpdateSetting(ForwardingConfigurationRegistry.Instance.ConsolPhaseSecurity);
				dpsUpdateSetting.JobUpdateSettings[0].ShouldUpdate = true;
				Factory.Save();
				using (ForwardingConfigurationRegistry.Instance.ConsolDpsStatusUpdateSetting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dpsUpdateSetting))
				{
					Assert(!phaseToDelete.CanDelete);
					AssertEquals("This phase is selected as a DPS Updating Phase, please un-select it under \"Master Data -> Organizations -> Denied Party Screening -> Consol DPS Update\" before deleting it.", phaseToDelete.ReasonForNotAbleToDelete.GetUnresolvedString());
				}
			}
		}

		public void TestRules()
		{
			Phase phase = new Phase();
			AssertEquals(phase, phase.Rules.Parent);
		}

		public void TestIPhase()
		{
			Phase phase = new Phase();
			phase.Code = "AAA";
			phase.Description = (NoResString)"Hello";
			phase.Rules.AddNew().Location = "Chisinau";
			phase.Rules.AddNew().Location = "Moldova";

			IPhase iPhase = phase;    // iPhase, Ha-ha-ha!
			AssertEquals("AAA", iPhase.Code);
			AssertEquals("Hello", iPhase.Description);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "Chisinau", "Moldova" }, iPhase.Rules.Select(x => x.Location));
		}

		public void TestValidateCode()
		{
			Phase phase = new Phase();
			phase.Code = "AAA";
			AssertNoErrors(phase.CodeInfo);

			phase.Code = PhaseConstants.Phase.ALL;
			AssertHasError(phase.CodeInfo, string.Format("You can't customize security for the {0} code.", PhaseConstants.Phase.ALL));

			phase.Code = "BBB";
			AssertNoErrors(phase.CodeInfo);

			phase.Code = "X";
			AssertHasError(phase.CodeInfo, "Code should have 3 letters.");
		}

		public void TestValidateDescription()
		{
			var phase = new Phase();
			phase.Description = (NoResString)"Hello";
			AssertNoErrors(phase.DescriptionInfo);

			phase.Description = (NoResString)"";
			AssertHasErrors(phase.DescriptionInfo);

			phase.EnglishDescription = (NoResString)"Hello again";
			AssertNoErrors(phase.DescriptionInfo);

			phase.EnglishDescription = (NoResString)"";
			AssertHasErrors(phase.DescriptionInfo);
			Assert("Phase would have error from description - preventing save with empty one", phase.HasErrors);

			phase.EnglishDescription = (NoResString)"...and again";
			AssertNoErrors(phase.DescriptionInfo);
			Assert("Phase would not have error from description", !phase.HasErrors);
		}

		public void TestPreSaveValidation()
		{
			Phase phase = new Phase();
			phase.Code = "AAA";
			phase.RunPreSaveValidation();
			AssertHasRowError(phase, "You should have at least one rule for the phase.");

			phase.Rules.AddNew();
			phase.RunPreSaveValidation();
			AssertNoRowErrors(phase);
		}

		public void TestClone_Rules()
		{
			Phase phase = new Phase();
			phase.Rules.AddNew().Location = "AAA";
			phase.Rules.AddNew().Location = "BBB";

			Phase clonedPhase = (Phase)phase.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), phase.Factory);
			AssertContainsExactElementsInAnyOrder("Rules colleciton cloned", new ZString[] { "AAA", "BBB" }, clonedPhase.Rules.Cast<PhaseRule>().Select(x => x.Location));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new Phase();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return (Phase)GetNewBusinessObject();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return (Phase)GetNewBusinessObject();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
