using CargoWiseOne.ResourceStrings;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public class CompanyTariffsWorkflowDescriptor : RatingHeaderWorkflowDescriptor<CompanyTariff>
	{
		#region ID / Description

		public override string Code
		{
			get { return WorkflowDescriptors.CompanyTariffsWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("Rating|GlobalRatesWorkflowDescriptor|Description", "Company Tariff"); }
		}

		#endregion

		#region Workflow Trigger

		protected override IValueObjectDataAdapter GetValueObjectDataAdapterCore()
		{
			return new CompanyTariffValueObjectDataAdapter();
		}

		#endregion

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.GlobalRates; }
		}
	}
}

