using CargoWiseOne.ResourceStrings;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public class ClientRateWorkflowDescriptor : RatingHeaderWorkflowDescriptor<ClientRate>
	{
		#region ID / Description

		public override string Code
		{
			get { return WorkflowDescriptors.ClientRateWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("Rating|ClientRateWorkflowDescriptor|Description", "Client Rate"); }
		}

		#endregion

		#region Workflow Trigger

		protected override IValueObjectDataAdapter GetValueObjectDataAdapterCore()
		{
			return new ClientRatesValueObjectDataAdapter();
		}

		#endregion

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.ClientRates; }
		}
	}
}

