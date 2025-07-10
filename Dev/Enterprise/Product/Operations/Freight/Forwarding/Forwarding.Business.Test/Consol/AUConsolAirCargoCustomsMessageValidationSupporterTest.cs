using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Workflow.ValidationAction.Testing;
using IAUCusMAWB = Enterprise.Integration.Customs.AU.ICusMAWB;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class AUConsolAirCargoCustomsMessageValidationSupporterTest : CustomsMessageValidationSupporterTest<ForwardingConsol>
	{
		protected override IEnumerable<string> ExpectedTriggerTypes
		{
			get
			{
				yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForAUCargoMessaging;
				yield return WorkflowTriggerActionTypeConstants.Codes.ReconcileOutturn;
			}
		}

		protected override ZString CountryCode
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}

		protected override ForwardingConsol GetValidateForCustomsMessagingSupporter()
		{
			var result = Factory.New<ForwardingConsol>();
			result.JK_TransportMode = Core.Constants.TransportModes.Air;
			var airCargo = Factory.New<IAUCusMAWB>();
			airCargo.CM_JK = result.PK;
			return result;
		}
	}
}
