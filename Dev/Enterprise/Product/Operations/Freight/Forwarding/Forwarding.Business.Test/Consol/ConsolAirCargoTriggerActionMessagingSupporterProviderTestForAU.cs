using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using IAUCusMAWB = Enterprise.Integration.Customs.AU.ICusMAWB;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ConsolAirCargoTriggerActionMessagingSupporterProviderTestForAU : TriggerActionMessagingSupporterProviderTest<ForwardingConsol>
	{
		public override void TestTriggerActionMessaging()
		{
			base.TestTriggerActionMessaging();
			Assert(new LogsForNominatedEvent(((BusinessObject)consol.AUCusMAWB).GetLogs(), Events.DeferredScheduledMessage).Count > 0);
		}

		protected override ForwardingConsol GetNewSupporter()
		{
			return consol;
		}

		protected override string ExpectedStringInNotifications
		{
			get { return ""; }
		}

		protected override ZString CountryCode
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}

		protected override IEnumerable<string> ExpectedTriggerTypes
		{
			get
			{
				yield return WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage;
				yield return WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUAirCargoOutturnMessage;
			}
		}

		ForwardingConsol consol;

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var airCargo = Factory.New<IAUCusMAWB>();
			airCargo.CM_JK = consol.PK;
		}
	}
}
