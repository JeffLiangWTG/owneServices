using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSShipmentDocumentSupporter : CommonShipmentDocumentSupporter, ILabelDocumentSupporter
	{
		public CFSShipmentDocumentSupporter(CFSShipment shipment)
			: base(shipment)
		{
		}

		public CFSShipment CFSShipment
		{
			get { return (CFSShipment)BusinessObject; }
		}

		#region Overrides

		protected override void InitialiseCore(IDocumentEvents documentEventSource)
		{
			documentEventSource.DocumentPrintRequested += new DocumentCancelEventHandler(DocumentEventSource_DocumentPrintRequested);
			base.InitialiseCore(documentEventSource);
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CFSShipmentReceival; }
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[]
			{
				Core.Constants.DataContext.ShipmentReceival,
				Core.Constants.DataContext.ContainerRego,
				Core.Constants.DataContext.PackUnpackContainerRego,
				Core.Constants.DataContext.CFSContainerLeg,
				Core.Constants.DataContext.CartageAdvice,
				Core.Constants.DataContext.CFSAndForwardingShipment,
				Core.Constants.DataContext.RequestForService,
				Core.Constants.DataContext.Service,
				Core.Constants.DataContext.GenericFreightJob,
				Core.Constants.DataContext.GenericFreightJobServices,
				Core.Constants.DataContext.GenericFreightJobByPackages
			};
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Core.Constants.DataContext.GenericFreightJobServices)
			{
				return CFSShipment.DocsAndCartage.GetServiceWrappersForDocBuilder(((IHaveServices)CFSShipment.DocsAndCartage).ServiceParent, Core.Constants.DataContext.GenericFreightJob);
			}

			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, CFSShipment);
			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			switch (dataContext)
			{
				case Core.Constants.DataContext.ShipmentReceival:
				case Core.Constants.DataContext.ContainerRego:
				case Core.Constants.DataContext.PackUnpackContainerRego:
				case Core.Constants.DataContext.CartageAdvice:
				case Core.Constants.DataContext.CFSAndForwardingShipment:
				case Core.Constants.DataContext.Service:
					return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.ShipmentReceival, CFSShipment) };

				case Core.Constants.DataContext.CFSContainerLeg:
					DocumentPickupDeliveryConfirmCollection documentConfirms = new DocumentPickupDeliveryConfirmCollection(Shipment.OriginCFSArrivals);
					DocumentPickupDeliveryConfirmOptions options = new DocumentPickupDeliveryConfirmOptions(documentConfirms);
					return GetWrappersByConfirm(options, dataContext);

				case Core.Constants.DataContext.GenericFreightJobByPackages:
					return GetGenericWrapperForOuterPacks();

				case Core.Constants.DataContext.RequestForService:
					return Shipment.DocsAndCartage.GetServiceWrappers(Core.Constants.DataContext.Shipment);

				default:
					return null;
			}
		}

		public DocumentWrapper[] GetGenericWrapperForOuterPacks()
		{
			var result = new List<DocumentWrapper>();
			var pieceCount = 0;
			Array.ForEach(GetPackLines(Shipment.OuterPackLines), p => result.AddRange(GetPackLinesWrapper(p)));

			foreach (var wrapper in result)
			{
				wrapper.DocNumber = ++pieceCount;
			}

			return result.ToArray();
		}

		protected override IDocumentEventsHandler[] GetDocumentEventsHandlers()
		{
			var result = new List<IDocumentEventsHandler>(base.GetDocumentEventsHandlers());

			var labelDocumentEventsHandler = ObjectFactory.Get<IDocumentEventsHandler>("IDocumentEventsHandler.Labels");
			labelDocumentEventsHandler.DocumentSupporter = this;
			result.Add(labelDocumentEventsHandler);

			return result.ToArray();
		}

		static PackLine[] GetPackLines(OuterPackLineCollection packLines)
		{
			var result = new List<PackLine>();

			foreach (PackLine packLine in packLines)
			{
				if (!packLine.JL_PackageCount.IsEmpty && packLine.JL_PackageCount > 0)
				{
					for (int i = 1; i <= packLine.JL_PackageCount; i++)
					{
						result.Add(packLine);
					}
				}
			}

			return result.ToArray();
		}

		DocumentWrapper[] GetPackLinesWrapper(PackLine package)
		{
			DocumentWrapper[] result;
			var wrappers = DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, CFSShipment);
			if (wrappers.Length > 0)
			{
				var wrapper = wrappers[0];
				var iPackLineOverrider = (IPackLineOverrider)wrapper;
				iPackLineOverrider.SetPackageOverride(package);
				result = new[] { wrapper };
			}
			else
			{
				result = Array.Empty<DocumentWrapper>();
			}

			return result;
		}

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menu, BusinessContext businessContext, IStmMenuItem childCommand)
		{
			IDocumentSupportable[] result = null;

			if (menu == null)
			{
				ErrorReporter.ReportOnce("{CBBED063-1A93-4049-85DD-52C0839DFE6C}", "Unexpected Null Menu");
				result = null;
			}
			else if (menu.SU_MenuName.StartsWith((NoResString)"Arrival Notice by Coload House Bills", StringComparison.Ordinal))
			{
				result = (IDocumentSupportable[])Shipment.CoLoadShipments.ToArray(typeof(IDocumentSupportable));
			}
			else if (menu.SU_MenuName.Contains((NoResString)"Cartage Advice", StringComparison.Ordinal))
			{
				result = GetChildCollectionForCartageAdvice(menu);
			}

			return result;
		}

		IDocumentSupportable[] GetChildCollectionForCartageAdvice(IStmMenuItem menu)
		{
			IDocumentSupportable[] result;

			if (Shipment.RequiresContainerCartageAdvice(menu.SU_DocumentDirection))
			{
				CommonContainerCollection containers = new CommonContainerCollection(Factory);
				CommonContainer[] selectedContainers = ContainersToPrint(menu.SU_DocumentDirection);
				if (selectedContainers != null)
				{
					foreach (CommonContainer container in selectedContainers)
					{
						CommonContainer documentSupportableContainer = Factory.Load<CFSContainer>(container.PK);
						documentSupportableContainer.LinkedShipment = Shipment;
						containers.Add(documentSupportableContainer);
					}
					result = (IDocumentSupportable[])containers.ToArray(typeof(IDocumentSupportable));
				}
				else
				{
					result = (Array.Empty<IDocumentSupportable>());
				}
			}
			else
			{
				result = (new CommonShipment[] { Shipment });
			}
			return result;
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			switch (dataContextValue.DataContext)
			{
				case Core.Constants.DataContext.CFSContainerLeg:
					{
						var confirms = Shipment.OriginCFSArrivals;
						return confirms.Any()
							? Res.GetString("4feb9294-db3e-4306-a3fb-3a1cc981555b", "Please choose at least one confirm for print.")
							: Res.GetString("7fc1ed09-9fa7-40b0-a040-1325ad37973b", "This shipment does not have any Arrival information entered within the Arrival tab.");
					}

				case Core.Constants.DataContext.GenericFreightJobByPackages:
					{
						if (!Shipment.OuterPackLines.Any())
						{
							return Res.GetString("efc1a474-d6d9-4b49-842a-790c0181e7ae", "This shipment does not have any packlines.");
						}

						break;
					}

				case Core.Constants.DataContext.RequestForService:
					{
						return Shipment.DocsAndCartage.Services.Any()
							? Res.GetString("1afec76a-d72c-4dad-a4b9-3ac18557b0a0", "Please choose at least one service for print.")
							: Res.GetString("51219f2e-9885-468b-8c67-9cc4456ede0d", "This shipment does not have any Services information entered within the Shipment > Services grid.");
					}
			}

			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != Core.Constants.DataContext.ShipmentReceival
				&& dataContext != Core.Constants.DataContext.GenericFreightJob
				&& dataContext != Core.Constants.DataContext.GenericFreightJobServices
				&& dataContext != Core.Constants.DataContext.ContainerRego
				&& dataContext != Core.Constants.DataContext.PackUnpackContainerRego
				&& dataContext != Core.Constants.DataContext.CartageAdvice
				&& dataContext != Core.Constants.DataContext.CFSAndForwardingShipment
				&& dataContext != Core.Constants.DataContext.Service
				&& base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded menu name constant")]
		protected virtual void DocumentEventSource_DocumentPrintRequested(object sender, DocumentCancelEventArgs e)
		{
			if (e.MenuItem.SU_MenuName == "Export Label")
			{
				ZInt defaultNumberOfCopies = 0;

				if (CFSShipment.OuterPackLines.Count > 0)
				{
					foreach (CFSPackLine line in CFSShipment.OuterPackLines)
					{
						defaultNumberOfCopies += line.JL_PackageCount;
					}
				}
				else
				{
					defaultNumberOfCopies = CFSShipment.JS_OuterPacks;
				}

				ZShort copies = 0;
				if (!ZShort.TryParse(defaultNumberOfCopies.ToString(), out copies))
				{
					copies = 0;
				}

				e.MenuItem.NumberOfCopies = copies == 0 ? (ZShort)1 : copies;
			}
		}

		#region ISupportLabels Members

		bool ILabelDocumentSupporter.SupportImportLabels
		{
			get { return CFSShipment.IsImport(); }
		}

		bool ILabelDocumentSupporter.SupportOnForwardingLabels
		{
			get { return CFSShipment.IsOnForwarding(); }
		}

		bool ILabelDocumentSupporter.SupportTranshipmentLabels
		{
			get { return CFSShipment.IsTranshipment(); }
		}

		string ILabelDocumentSupporter.ImportLabelErrorMessage
		{
			get { return Res.GetString("e05fb39d-bfa8-4f38-a993-ba64f9d4ab3d", "This is not an Import shipment with at least one pack line."); }
		}

		string ILabelDocumentSupporter.OnForwardingErrorMessage
		{
			get { return Res.GetString("58932a91-391c-4d5f-836a-4701a0f96905", "This is not an On Forwarding shipment with at least one pack line."); }
		}

		string ILabelDocumentSupporter.TranshipmentErrorMessage
		{
			get { return Res.GetString("c21f8ee6-5e72-4863-970b-79d7759f41f0", "This is not a Transhipment with at least one pack line."); }
		}

		#endregion
	}
}
