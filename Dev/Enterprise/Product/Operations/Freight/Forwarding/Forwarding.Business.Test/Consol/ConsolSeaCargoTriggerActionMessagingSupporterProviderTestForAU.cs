using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using IAUCusSCAOceanBill = Enterprise.Integration.Customs.AU.ICusSCAOceanBill;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ConsolSeaCargoTriggerActionMessagingSupporterProviderTestForAU : TriggerActionMessagingSupporterProviderTest<ForwardingConsol>
	{
		protected override ForwardingConsol GetNewSupporter()
		{
			var result = Factory.New<ForwardingConsol>();
			result.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var seaCargo = Factory.New<IAUCusSCAOceanBill>();
			seaCargo.CB_ParentId = result.PK;
			seaCargo.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			return result;
		}

		protected override string ExpectedStringInNotifications
		{
			get { return "Generating Cargo messages for Job"; }
		}

		protected override ZString CountryCode
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}

		protected override IEnumerable<string> ExpectedTriggerTypes
		{
			get { yield return WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK).Staff.AddNew().GS_EmailAddress = "abc@abc.com";
		}
	}
}
