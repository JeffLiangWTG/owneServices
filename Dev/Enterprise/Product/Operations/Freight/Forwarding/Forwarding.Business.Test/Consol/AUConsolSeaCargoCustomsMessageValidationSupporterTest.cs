using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Workflow.ValidationAction.Testing;
using Enterprise.ZArchitecture.Schema;
using IAUCusSCAOceanBill = Enterprise.Integration.Customs.AU.ICusSCAOceanBill;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class AUConsolSeaCargoCustomsMessageValidationSupporterTest : CustomsMessageValidationSupporterTest<ForwardingConsol>
	{
		protected override IEnumerable<string> ExpectedTriggerTypes
		{
			get { yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForAUCargoMessaging; }
		}

		protected override ZString CountryCode
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}

		protected override ForwardingConsol GetValidateForCustomsMessagingSupporter()
		{
			var result = Factory.New<ForwardingConsol>();
			result.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var seaCargo = Factory.New<IAUCusSCAOceanBill>();
			seaCargo.CB_ParentId = result.PK;
			seaCargo.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			return result;
		}
	}
}
