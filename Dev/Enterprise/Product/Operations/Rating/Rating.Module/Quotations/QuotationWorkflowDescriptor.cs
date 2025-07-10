using CargoWise.Definitions;
using CargoWiseOne.ResourceStrings;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer;
using Enterprise.Rating.Module.Quotations;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public class QuotationWorkflowDescriptor : RatingHeaderWorkflowDescriptor<Quote>
	{
		#region ID / Description

		public override string Code
		{
			get { return WorkflowDescriptors.QuotationWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("Rating|QuotationWorkflowDescriptor|Description", "Quotation"); }
		}

		#endregion

		#region Workflow Trigger

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new[] { BusinessContext.Quotation }; }
		}

		protected override IValueObjectDataAdapter GetValueObjectDataAdapterCore()
		{
			return new QuotationValueObjectDataAdapter();
		}

		#endregion

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Quotations; }
		}

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new QuotationFormCustomisationSettingsProvider(this);
		}
	}
}

