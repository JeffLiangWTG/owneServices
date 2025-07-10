using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(DpsStatusUpdateSetting))]
	public class DpsStatusUpdateSettingTest : RegistryBusinessObjectTemplateTestCase<DpsStatusUpdateSetting>
	{
		public void TestPhasesSynchronized()
		{
			var phaseSecurity = new PhaseSecurity();
			phaseSecurity.RuleLocations.AddPair("Bedroom", "There is a bed");
			var phase = phaseSecurity.Phases.AddNew();
			phase.Code = "PH1";
			phase.Description = (NoResString)"Phase 1";
			phase.Rules.AddNew().Location = "Bedroom";

			using (ForwardingConfigurationRegistry.Instance.ShipmentPhaseSecurity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, phaseSecurity))
			{
				var setting = new DpsStatusUpdateSetting(ForwardingConfigurationRegistry.Instance.ShipmentPhaseSecurity);
				Assert(setting.JobUpdateSettings.Cast<JobPhaseSetting>().Any(x => x.Code == phase.Code));
				AssertEquals(1, setting.JobUpdateSettings.Count);
			}

			using (ForwardingConfigurationRegistry.Instance.ConsolPhaseSecurity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, phaseSecurity))
			{
				var setting = new DpsStatusUpdateSetting(ForwardingConfigurationRegistry.Instance.ConsolPhaseSecurity);
				Assert(setting.JobUpdateSettings.Cast<JobPhaseSetting>().Any(x => x.Code == phase.Code));
				AssertEquals(1, setting.JobUpdateSettings.Count);
			}
		}

		public void TestOption()
		{
			var setting = new DpsStatusUpdateSetting();
			AssertEquals("DAB", setting.Option);
			setting.Option = "ABC";
			AssertEquals("ABC", setting.Option);
		}

		public void TestNoErrorOccurWhenPhaseDescriptionLengthIsMax()
		{
			var phaseSecurity = new PhaseSecurity();
			phaseSecurity.RuleLocations.AddPair("Bedroom", "There is a bed");
			var phase = phaseSecurity.Phases.AddNew();
			phase.Code = "PH1";
			phase.Description = (NoResString)new string('A', phase.Description_MaxLength);
			phase.Rules.AddNew().Location = "Bedroom";

			using (ForwardingConfigurationRegistry.Instance.ShipmentPhaseSecurity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, phaseSecurity))
			{
				var setting = new DpsStatusUpdateSetting(ForwardingConfigurationRegistry.Instance.ShipmentPhaseSecurity);
				AssertNoExceptionThrown(() => setting.JobUpdateSettings.Cast<JobPhaseSetting>().Single(x => x.Code == phase.Code));
				AssertEquals(1, setting.JobUpdateSettings.Count);
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DpsStatusUpdateSetting();
		}

		protected override DpsStatusUpdateSetting GetBusinessObjectToClone()
		{
			return (DpsStatusUpdateSetting)GetNewBusinessObject();
		}

		protected override DpsStatusUpdateSetting GetBusinessObjectToSerialise()
		{
			return (DpsStatusUpdateSetting)GetNewBusinessObject();
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
