using System;
using System.Globalization;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business
{
	public class GatePassShipmentDocumentSupporter : CFSShipmentDocumentSupporter
	{
		public GatePassShipmentDocumentSupporter(GatePassShipment gatePassShipment)
			: base(gatePassShipment)
		{
		}

		protected GatePassShipment GatePassShipment
		{
			get { return (GatePassShipment)BusinessObject; }
		}

		#region Overrides

		protected override void InitialiseCore(IDocumentEvents documentEventSource)
		{
			documentEventSource.DocumentPrintRequested += new DocumentCancelEventHandler(DocumentEventSource_DocumentPrintRequested);
			base.InitialiseCore(documentEventSource);
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.CFSShipmentCustomiseDocuments; }
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[]
			{
				Core.Constants.DataContext.GatePassShipment,
				Core.Constants.DataContext.GatePassContainerLeg,
				Core.Constants.DataContext.Service,
				Core.Constants.DataContext.GenericFreightJob,
				Core.Constants.DataContext.GenericPickupDeliveryConfirm
			};
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.GatePass; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Core.Constants.DataContext.GenericPickupDeliveryConfirm)
			{
				return GetConfirmWrappersToPrint(dataContext);
			}

			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, GatePassShipment);
			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			switch (dataContext)
			{
				case Core.Constants.DataContext.GatePassShipment:
				case Core.Constants.DataContext.Service:
					return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.GatePassShipment, GatePassShipment) };
				case Core.Constants.DataContext.GatePassContainerLeg:
					//Should maybe store these, so they can be used to add events etc.
					return GetConfirmWrappersToPrint(dataContext);
				default:
					return null;
			}
		}

		protected DocumentWrapper[] GetConfirmWrappersToPrint(Constants.DataContext dataContext)
		{
			DocumentPickupDeliveryConfirmCollection documentConfirms = new DocumentPickupDeliveryConfirmCollection(Shipment.DestinationCFSDepartures);
			foreach (DocumentPickupDeliveryConfirm documentConfirm in documentConfirms)
			{
				documentConfirm.PrintConfirm = !documentConfirm.IsInDatabase && documentConfirm.Confirm.UniqueID.IsEmpty;
			}
			DocumentPickupDeliveryConfirmOptions options = new DocumentPickupDeliveryConfirmOptions(documentConfirms);
			return GetWrappersByConfirm(options, dataContext);
		}

		public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			DocumentSupporterDataState state = base.GetDataStateBeforeRun(commandAboutToBeRun);
			if (state.IsValid && GatePassShipment.DestinationCFSDepartures.Count == 0)
			{
				state = new DocumentSupporterDataState(false, CannotPrintWhenNoDeliveriesError);
			}
			return state;
		}

		public string CannotPrintWhenNoDeliveriesError = Res.GetString("c7588a6b-1741-44a0-bfd6-a532fb07bc8e", "Please register at least one delivery before attempting to print a Gate Pass.");

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			switch (dataContextValue.DataContext)
			{
				case Core.Constants.DataContext.GatePassContainerLeg:
				case Core.Constants.DataContext.GenericPickupDeliveryConfirm:
					{
						var confirms = Shipment.DestinationCFSDepartures;
						if (!confirms.Any())
						{
							return CannotPrintWhenNoDeliveriesError;
						}

						break;
					}
			}

			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != Core.Constants.DataContext.GatePassShipment
				&& dataContext != Core.Constants.DataContext.Service
				&& dataContext != Core.Constants.DataContext.GenericFreightJob
				&& base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		#endregion

		protected override void DocumentEventSource_DocumentPrintRequested(object sender, DocumentCancelEventArgs e)
		{
			if (e.MenuItem.SU_MenuName.Contains((NoResString)"Gate Pass", StringComparison.Ordinal))
			{
				ZShort copies = 3;

				if (!e.MenuItem.SU_MenuName.Contains((NoResString)"Singapore", StringComparison.Ordinal))
				{
					var standardNumberOfCopies = CFSDataRegistry.Instance.GatepassCopiesToPrint.Value;
					if (!ZShort.TryParse(standardNumberOfCopies.ToString(CultureInfo.InvariantCulture), out copies))
					{
						copies = 0;
					}
				}

				e.MenuItem.NumberOfCopies = copies == 0 ? (ZShort)1 : copies;
			}
		}
	}
}
