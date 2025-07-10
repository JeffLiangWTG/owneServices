using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentWrapperWithConsolAgentDocumentSupporter : ForwardingShipmentDocumentSupporter
	{
		public ForwardingShipmentWrapperWithConsolAgentDocumentSupporter(ForwardingShipmentWrapperWithConsolAgent forwardingShipmentWrapperWithConsolAgent)
			: base(forwardingShipmentWrapperWithConsolAgent.Shipment)
		{
			fWrapper = forwardingShipmentWrapperWithConsolAgent;
		}

		#region Related Objects

		public ForwardingShipmentWrapperWithConsolAgent Wrapper
		{
			get { return fWrapper; }
		}
		readonly ForwardingShipmentWrapperWithConsolAgent fWrapper;

		#endregion

		public override bool SupportDocBuilderInvoiceAsChildCommand
		{
			get { return true; }
		}

		public override BusinessContext BusinessContext
		{
			get { return fWrapper.BusinessContext; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Constants.DataContext.ARInvoice && Wrapper.ConsolAgentForARInvoice != null)
			{
				return Wrapper.Shipment.GetWrappersForARInvoice(Wrapper.ConsolAgentForARInvoice);
			}
			else if ((dataContext == Constants.DataContext.GenericFreightJobInvoice || dataContext == Constants.DataContext.GenericFreightJob) && Wrapper.ConsolAgentForARInvoice != null)
			{
				return Wrapper.Shipment.GetWrappersForARInvoice(Wrapper.ConsolAgentForARInvoice, true);
			}
			else
			{
				return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
			}
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			IDocumentDeliveryContact result = null;

			if ((contact == ContactType.ExportAirFreightAgent ||
				contact == ContactType.ExportSeaFreightAgent) &&
				Wrapper.ConsolAgentForARInvoice != null)
			{
				result = new OrgHeaderContact(Wrapper.ConsolAgentForARInvoice, null);
			}
			else
			{
				result = base.GetContactOrganisation(menuName, contact, direction);
			}
			return result;
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			switch (dataContextValue.DataContext)
			{
				case Constants.DataContext.ARInvoice:
				case Constants.DataContext.GenericFreightJobInvoice:
					{
						if (Wrapper.ConsolAgentForARInvoice == null)
						{
							return Res.GetString("6322b621-22cd-4503-ab9c-a73a0c6852f6", "The related consol does not have a Receiving Agent.");
						}

						return ZString.Empty;
					}
			}

			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != Constants.DataContext.GenericFreightJob && base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}
	}
}
