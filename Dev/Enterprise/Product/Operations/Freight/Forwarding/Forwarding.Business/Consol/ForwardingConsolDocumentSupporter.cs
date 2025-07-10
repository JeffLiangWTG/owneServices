using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingConsolDocumentSupporter : CommonConsolDocumentSupporter
	{
		public ForwardingConsolDocumentSupporter(ForwardingConsol forwardingConsol)
			: base(forwardingConsol)
		{
		}

		protected ForwardingConsol ForwardingConsol
		{
			get { return (ForwardingConsol)BusinessObject; }
		}

		public new static CommonConsolDocumentSupporter New(CommonConsol consol)
		{
			ForwardingConsolDocumentSupporter result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = (ForwardingConsolDocumentSupporter)overridden(consol);
			}
			else if (consol != null)
			{
				result = new ForwardingConsolDocumentSupporter((ForwardingConsol)consol);
			}

			return result;
		}

		#region Query Provider

		protected new IForwardingConsolDocumentSupporterQueryProvider QueryProvider
		{
			get { return (IForwardingConsolDocumentSupporterQueryProvider)base.QueryProvider; }
		}

		protected override ICommonConsolDocumentSupporterQueryProvider GetQueryProvider()
		{
			return Factory.GetValue<IForwardingConsolDocumentSupporterQueryProvider>() ?? new ForwardingConsolDocumentSupporterQueryProvider();
		}

		protected override ICommonConsolDocumentSupporterQueryProvider GetNonGuiQueryProvider()
		{
			return new ForwardingConsolDocumentSupporterQueryProvider();
		}

		#endregion

		#region DocumentEventsHandlers

		class AWBDocumentEventsHandler : IDocumentEventsHandler
		{
			public DocumentSupporter DocumentSupporter { get; set; }

			[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Menu path / name")]
			public bool CanHandleMenuItem(IStmMenuItem menuItem)
			{
				if (menuItem == null)
				{
					return false;
				}

				// TODO:  CanHandleMenuItem should not be dependant on the menu path
				if (menuItem.SU_MenuPath == "Departure/Document Pack" && menuItem.SU_MenuName == "Consol Agent Pack (Air)")
				{
					return true;
				}

				return menuItem.SU_MenuPath.Contains("AWB", StringComparison.Ordinal);
			}

			public void HandleDocumentPrePreviewed(object sender, DocumentPrintedEventArgs e)
			{
				ForEachForwardingShipment(e.Source, shipment => shipment.PopulateAWBForDocuments(), includedInPrint: true);
			}

			public void HandleDocumentPrePrinted(object sender, DocumentPrintedEventArgs e)
			{
				ForEachForwardingShipment(e.Source, shipment => shipment.PopulateAWBForDocuments(), includedInPrint: true);
			}

			public void HandleDocumentPrintRequested(object sender, DocumentCancelEventArgs e)
			{
				var forwardingConsolDocumentSupporter = DocumentSupporter as ForwardingConsolDocumentSupporter;
				if (forwardingConsolDocumentSupporter != null)
				{
					forwardingConsolDocumentSupporter.ForwardingConsol.PopulateAWB();
					forwardingConsolDocumentSupporter.ForwardingConsol.Shipments.Cast<ForwardingShipment>().ForEach(shipment => shipment.AWBForDocumentsPopulated = false);
				}
			}

			public void HandleDocumentPrinted(object sender, DocumentPrintedEventArgs e)
			{
				ForEachForwardingShipment(e.Source, shipment => shipment.AWBForDocumentsPopulated = false);
			}

			static void ForEachForwardingShipment(object source, Action<ForwardingShipment> action, bool? includedInPrint = null)
			{
				IEnumerable<BusinessObject> businessObjectsForPrintJob = null;
				if (source is DocumentPack documentPack)
				{
					businessObjectsForPrintJob = documentPack.BusinessObjectsForPrintJob(includedInPrint);
				}
				else if (source is DocumentPrintSet documentPrintSet)
				{
					businessObjectsForPrintJob = documentPrintSet.GetDocumentPacks().SelectMany(pack => pack.BusinessObjectsForPrintJob(includedInPrint));
				}
				businessObjectsForPrintJob?.OfType<ForwardingShipment>().ForEach(action);
			}
		}

		#endregion

		#region Overrides

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Consol; }
		}

		protected override IDocumentEventsHandler[] GetDocumentEventsHandlers()
		{
			var handlers = base.GetDocumentEventsHandlers().ToList();

			handlers.Add(new AWBDocumentEventsHandler() { DocumentSupporter = this });

			return handlers.ToArray();
		}

		public override bool MatchFilterValue(string filterValue)
		{
			var globalManifestMatch = GetGlobalManifestMatch(filterValue);
			return globalManifestMatch == null ? base.MatchFilterValue(filterValue) : Consol.GetGlobalManifestHeaders().Any(x => globalManifestMatch(x));
		}

		public static Func<ASYCUDA.IAsycudaManifestHeader, bool> GetGlobalManifestMatch(ZString filterValue)
		{
			Func<ASYCUDA.IAsycudaManifestHeader, bool> result = null;
			var valuePairs = filterValue.Split('|');
			if (valuePairs.Length == 2)
			{
				var matchValue = valuePairs[1].ToUpperInvariant();
				switch (valuePairs[0])
				{
					case GlobalManifestContryAndManifestType:
						result = new Func<ASYCUDA.IAsycudaManifestHeader, bool>(x => IsMatchMultipleManifestTypes((x.AMA_RN_NKCountry + x.AMA_ManifestType).ToUpperInvariant(), matchValue));
						break;
					case GlobalManifestContryManifestTypeAndMode:
						result = new Func<ASYCUDA.IAsycudaManifestHeader, bool>(x => (x.AMA_RN_NKCountry + x.AMA_ManifestType + x.AMA_TransportMode).ToUpperInvariant() == matchValue);
						break;
					case GlobalManifestContryAndMode:
						result = new Func<ASYCUDA.IAsycudaManifestHeader, bool>(x => (x.AMA_RN_NKCountry + x.AMA_TransportMode).ToUpperInvariant() == matchValue);
						break;
					case GlobalManifestContry:
						result = new Func<ASYCUDA.IAsycudaManifestHeader, bool>(x => x.AMA_RN_NKCountry.ToUpperInvariant() == matchValue);
						break;
				}
			}
			return result;
		}

		const string GlobalManifestContryAndManifestType = "ASYCTYMAN";
		const string GlobalManifestContryManifestTypeAndMode = "ASYCTYMANMOD";
		const string GlobalManifestContry = "ASYCTY";
		const string GlobalManifestContryAndMode = "ASYCTYMOD";

		static bool IsMatchMultipleManifestTypes(ZString value, ZString filter)
		{
			var filters = filter.Split('*').ToList();
			return filters.Any(x => x == value);
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			if (filterName == DocumentFilters.JPAFR)
			{
				var forwardingConsol = ForwardingConsol;
				ZBool result = forwardingConsol != null && forwardingConsol.IsEligibleForJPAFR();
				return result.ToString();
			}
			else if (filterName == DocumentFilters.CAeManifest)
			{
				var forwardingConsol = ForwardingConsol;
				ZBool result = forwardingConsol != null && forwardingConsol.IsDestinationToCanada();
				return result.ToString();
			}
			else
			{
				var result = base.GetFilterValue(filterName);
				if (string.IsNullOrEmpty(result))
				{
					result = GetResultFromAdditionalBODataSourceDocumentSupporters(filterName);
				}
				return result;
			}
		}

		string GetResultFromAdditionalBODataSourceDocumentSupporters(DocumentFilters filterName)
		{
			foreach (var additionalBODataSource in AdditionalBODataSourceDocumentSupporters)
			{
				var result = additionalBODataSource.GetFilterValue(filterName);
				if (!string.IsNullOrEmpty(result))
				{
					return result;
				}
			}

			return string.Empty;
		}

		protected override List<DocumentSupporter> AdditionalBODataSourceDocumentSupporters
		{
			get
			{
				var nctsHeader = (IDocumentSupportable)ForwardingConsol.NctsHeaderForDocuments ?? (IDocumentSupportable)Factory.GetNull(ObjectFactory.GetType<EU.NCTS.ICusInBondHeader>());
				return new List<DocumentSupporter> { nctsHeader.DocumentSupporter };
			}
		}

		public override string GetMenuTemplateFilterValue(MenuTemplateFilterType filterType, IBODocDataProvider docDataProvider)
		{
			switch (filterType)
			{
				case MenuTemplateFilterType.PrintStandard:
					return ZBool.True.ToString();

				case MenuTemplateFilterType.PrintClientSpecific:
					return ZBool.False.ToString();

				default:
					if (Consol.NctsHeaderForDocuments is IDocumentSupportable documentSupportable)
					{
						return documentSupportable.DocumentSupporter.GetMenuTemplateFilterValue(filterType, docDataProvider);
					}
					return null;
			}
		}

		protected override void InitialiseCore(IDocumentEvents documentEventSource)
		{
			base.InitialiseCore(documentEventSource);
			documentEventSource.DocumentPrintRequested += new DocumentCancelEventHandler(DocumentEventSource_DocumentPrintRequested);
			documentEventSource.DocumentPrinted += new DocumentPrintedEventHandler(DocumentEventSource_DocumentPrinted);
			documentEventSource.DocumentPrePrinted += new DocumentPrintedEventHandler(DocumentEventSource_DocumentPrePrinted);
		}

		#region GetDataStateBeforeRun

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "String comparison")]
		public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			DocumentSupporterDataState dataStateResult = null;

			if (commandAboutToBeRun.SU_MenuName.StartsWith((NoResString)"Delivery Agent Pack", StringComparison.Ordinal)) // Menu Name
			{
				dataStateResult = new DocumentSupporterDataState();

				DeliveryAgentsToPrint.RemoveAll();
				var agentsToPrint = QueryProvider.GetDeliveryAgentsToPrint(DeliveryAgentsToSelectFrom);

				if (agentsToPrint == null || agentsToPrint.Length == 0)
				{
					dataStateResult.IsValid = false;
					dataStateResult.ErrorMessage = Res.GetString("ffdb7cce-d0fb-47f9-93c1-ef613214ff93", "No Delivery Agents selected for printing.");
				}
				else
				{
					dataStateResult.IsValid = true;
					DeliveryAgentsToPrint.AddRange(agentsToPrint);
				}
			}
			else if (commandAboutToBeRun.PK == new ZGuid(GbAirlineDeliveryScheduleStmMenuItemPK)) // Airline Delivery Schedule - ADS - GB only
			{
				dataStateResult = new DocumentSupporterDataState();
				var cannotPrintReason = GetReasonForCannotPrintGbAirlineDeliverySchedule();

				dataStateResult.IsValid = cannotPrintReason.IsEmpty;
				dataStateResult.ErrorMessage = cannotPrintReason;
			}
			else if (commandAboutToBeRun.SU_MenuName.Equals("Print Final Master") || commandAboutToBeRun.SU_MenuName.Equals("AWB Barcode Label")) // Menu Name
			{
				var jobConsolModuleId = ModuleIDs.JobConsol;

				using (var module = ObjectFactory.Get<IModuleFactory>().Create(jobConsolModuleId))
				{
					if (module != null && module.SecurityCheckpoint != Env.Security.None)
					{
						var parentSecurityCheckpoint = Env.Security.FindOrCreateDocumentsCheckpoint(jobConsolModuleId, module.SecurityCheckpoint);

						var printFinalMasterCheckpoint = Env.Security.FindOrCreateDocumentCheckpoint(
							commandAboutToBeRun.PK.ToGuid(),
							commandAboutToBeRun.SU_MenuNameMultilingual,
							ModuleIDs.JobConsol,
							parentSecurityCheckpoint);

						if (!printFinalMasterCheckpoint.IsAllowed)
						{
							dataStateResult = new DocumentSupporterDataState();
							dataStateResult.IsValid = false;
							dataStateResult.ErrorMessage = printFinalMasterCheckpoint.ErrorMessageForNotAllowed;
						}
					}
				}
			}

			return dataStateResult ?? base.GetDataStateBeforeRun(commandAboutToBeRun);
		}

		public const string GbAirlineDeliveryScheduleStmMenuItemPK = "CFBB417E-F01A-44EF-AD9D-61FCCE2A19C9";

		public const string ChGroupDeliveryNoteStmMenuItemPK = "58828BC0-B4FC-4B10-B9DD-856441596642";

		#endregion

		#region GetDocBusinessObjects

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] genericWrappers;

			if (dataContext == Core.Constants.DataContext.GenericFreightJobInvoice)
			{
				genericWrappers = GetWrappersForARInvoice(ForwardingConsol.ReceivingForwarderPK, Core.Constants.DataContext.GenericFreightJob, true);
			}
			else
			{
				genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Consol);
			}

			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			DocumentWrapper result = null;

			switch (dataContext)
			{
				case Core.Constants.DataContext.AWB:
					if (ForwardingConsol.AWBHeader != null)
					{
						result = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.AWB, ForwardingConsol.AWBHeader);
					}
					break;

				case Core.Constants.DataContext.ERA:
				case Core.Constants.DataContext.LTL:
					return GetWrappersByContainer(ContainersToSelectFrom);

				case Core.Constants.DataContext.IMO:
					return GetIMOContainerDocBusinessObjects();

				case Core.Constants.DataContext.Container:
					return GetContainerDocBusinessObjects(commandBeingRun);

				case Core.Constants.DataContext.ARInvoice:
					return GetWrappersForARInvoice(ForwardingConsol.ReceivingForwarderPK, dataContext, false);

				case Core.Constants.DataContext.NZCustoms:
					result = DocumentWrapperFactory.CreateNZConsolWrapper(ForwardingConsol);
					break;

				case Core.Constants.DataContext.CHCustoms:
					result = DocumentWrapperFactory.CreateCHConsolWrapper(ForwardingConsol);
					break;

				case Core.Constants.DataContext.ImportCargoLabel:
					result = GetWrapperForImportCargoLabel();
					break;

				case Core.Constants.DataContext.LoadListDocument:
					result = GetWrapperForCommonConsol(dataContext);
					break;

				case Core.Constants.DataContext.Consol:
				case Core.Constants.DataContext.CommonConsol:
				case Core.Constants.DataContext.ForwardingConsol:
				case Core.Constants.DataContext.Notes:
				case Core.Constants.DataContext.ForwardingPreAdvice:
				case Core.Constants.DataContext.ForwardingShipmentAndConsol:
				case Core.Constants.DataContext.RequestForMissingDocuments:
				case Core.Constants.DataContext.TimeSlotRequest:
				case Core.Constants.DataContext.CombinedCartageAdvice:
				case Core.Constants.DataContext.ShippingOrder:
					if (commandBeingRun != null && commandBeingRun.SU_MenuName == MenuNameOfStandardShippingNote)
					{
						return GetWrappersForStandardShippingNoteDocument();
					}
					else
					{
						result = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.ForwardingConsol, ForwardingConsol);
						break;
					}

				case Core.Constants.DataContext.ProfitShareDetail:
					return GetWrappersForProfitShare();

				case Core.Constants.DataContext.ForwardingConsolGbADS:
					result = DocumentWrapperFactory.CreateWrapperWithoutException((NoResString)"Enterprise.Customs.GB.DocumentWrappers.Freight.DocForwardingConsolGB, Enterprise.Customs.GB.DocumentWrappers", ForwardingConsol);  // This is a namespace and class name
					break;

				case Core.Constants.DataContext.AsycudaManifestHeader:
					result = DocumentWrapperFactory.CreateWrapper((NoResString)"Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeaderDocWrapper, Enterprise.Customs.ASYCUDA.Business", new Object[] { ForwardingConsol, commandBeingRun?.SU_MenuName ?? ZString.Empty, commandBeingRun?.SU_FilterList ?? ZString.Empty }); // This is a namespace and class name
					break;

				case Core.Constants.DataContext.EuNcts:
					var nctsHeaderDocumentSupportable = Consol.NctsHeaderForDocuments as IDocumentSupportable;
					return nctsHeaderDocumentSupportable?.DocumentSupporter.GetDocumentWrappers(dataContext, commandBeingRun);
			}

			return (result == null) ? null : new DocumentWrapper[] { result };
		}

		#region Implementation

		DocumentWrapper[] GetIMOContainerDocBusinessObjects()
		{
			DocumentWrapper[] result = null;

			ForwardingShipment[] uncontainerisedShipments = Consol.Shipments.Cast<ForwardingShipment>().Where(HasUncontainerisedHazPackLines).ToArray();
			ContainersToPrintOptions containersToPrintOptions = QueryProvider.GetContainersToPrint(ContainersToSelectFrom, uncontainerisedShipments.Length > 0);

			if (containersToPrintOptions != null && containersToPrintOptions.ContainersToPrint != null)
			{
				DocumentWrapper[] containerisedShipmentsWrappers = GenerateWrappersFromContainersToPrintOptions(containersToPrintOptions);

				DocumentWrapper[] uncontainerisedHazShipmnentsWrappers = containersToPrintOptions.IncludeUnContainerised ?
						uncontainerisedShipments.Select((shipment) => DocumentWrapperFactory.CreateIMOShipmentWrapper(shipment, Consol)).ToArray() :
						Array.Empty<DocumentWrapper>();

				result = containerisedShipmentsWrappers.Union(uncontainerisedHazShipmnentsWrappers).ToArray();
			}

			return result;
		}

		protected bool HasUncontainerisedHazPackLines(ForwardingShipment shipment)
		{
			PackLine[] packLines = shipment.OuterPackLines.ToArray<PackLine>();
			return packLines.All((packLine) => packLine.GetContainer(Consol) == null) &&
				packLines.Any((packLine) => packLine.JL_RH_NKCommodityCode == Core.Constants.CargoTypes.Hazardous);
		}

		DocumentWrapper[] GetWrappersForProfitShare()
		{
			IProfitShareDocumentWrapperProvider profitShareDocProvider = ObjectFactory.Get<IProfitShareDocumentWrapperProvider>();
			return profitShareDocProvider.GetDocumentWrappers(ForwardingConsol, Factory);
		}

		DocumentWrapper[] GetWrappersForStandardShippingNoteDocument()
		{
			DocumentWrapper[] result = null;

			if (Consol.Containers.Count == 0)
			{
				result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.ForwardingConsol, Consol) };
			}
			else
			{
				ContainersToPrintOptions containersToPrintOptions = QueryProvider.GetContainersToPrint(ContainersToSelectFrom, false);

				if (containersToPrintOptions != null && containersToPrintOptions.ContainersToPrint != null && containersToPrintOptions.ContainersToPrint.Length > 0)
				{
					List<DocumentWrapper> wrappers = new List<DocumentWrapper>();

					foreach (CommonContainer container in containersToPrintOptions.ContainersToPrint)
					{
						DocumentCommonConsol docConsol = new DocumentCommonConsol(Consol, Core.Constants.DataContext.ForwardingConsol);
						docConsol.ContainerToPrint = container;

						DocumentWrapper consolWrapper = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.ForwardingConsol, docConsol);
						wrappers.Add(consolWrapper);
					}

					result = wrappers.ToArray();
				}
			}

			return result;
		}

		protected DocumentWrapper[] GetWrappersForARInvoice(ZGuid orgHeaderForInvoices, Constants.DataContext dataContext, bool useDocBuilderInvoice)
		{
			AccTransactionHeaderCollection transactionHeaders = new InvoiceLoader(Factory).GetInvoicesForUniqueRef(new ZGuid[] { orgHeaderForInvoices }, ForwardingConsol.JK_UniqueConsignRef);

			if (transactionHeaders.Count > 0)
			{
				var wrappers = new List<DocumentWrapper>();

				for (int i = 0; i < transactionHeaders.Count; i++)
				{
					if (useDocBuilderInvoice)
					{
						DocumentWrapper[] invoiceWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, transactionHeaders[i]);

						if (invoiceWrappers != null)
						{
							wrappers.AddRange(invoiceWrappers);
						}
					}
					else
					{
						wrappers.Add(DocumentWrapperFactory.CreateWrapper(dataContext, transactionHeaders[i]));
					}
				}

				return wrappers.Count == 0 ? null : wrappers.ToArray<DocumentWrapper>();
			}

			return null;
		}

		#endregion

		#endregion

		#region Supported Contexts

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[]
			{
				Constants.DataContext.BaseConsol,
				Constants.DataContext.Consol,
				Constants.DataContext.AWB,
				Constants.DataContext.Container,
				Constants.DataContext.ARInvoice,
				Constants.DataContext.Notes,
				Constants.DataContext.ERA,
				Constants.DataContext.LTL,
				Constants.DataContext.IMO,
				Constants.DataContext.ForwardingPreAdvice,
				Constants.DataContext.NZCustoms,
				Constants.DataContext.CHCustoms,
				Constants.DataContext.ImportCargoLabel,
				Constants.DataContext.LoadListDocument,
				Constants.DataContext.ShippingOrder,
				Constants.DataContext.CommonConsol,
				Constants.DataContext.ForwardingConsol,
				Constants.DataContext.CommonConsol,
				Constants.DataContext.CartageAdvice,
				Constants.DataContext.ForwardingShipmentAndConsol,
				Constants.DataContext.RequestForMissingDocuments,
				Constants.DataContext.TimeSlotRequest,
				Constants.DataContext.GenericFreightJob,
				Constants.DataContext.CombinedCartageAdvice,
				Constants.DataContext.ProfitShareDetail,
				Constants.DataContext.ForwardingConsolGbADS,
				Constants.DataContext.GenericFreightJobInvoice,
				Constants.DataContext.GbCcsuk,
				Constants.DataContext.AsycudaManifestHeader,
				Constants.DataContext.EuNcts
			};
		}

		public override BusinessContext[] SupportedChildBusinessContexts
		{
			get
			{
				return new[]
				{
					BusinessContext.Shipment,
					BusinessContext.DeliveryAgent,
					BusinessContext.ConsolAgent,
					BusinessContext.ForwardingContainer,
					BusinessContext.JPAFRHeader,
					BusinessContext.CusMAWB,
					BusinessContext.CAeManifest
				};
			}
		}

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			return GetSupportedBODataSourcesFor(typeof(ForwardingConsol));
		}

		#endregion

		public override bool SupportDocBuilderInvoiceAsChildCommand
		{
			get { return true; }
		}

		#region GetChildCollection

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menu, BusinessContext businessContext, IStmMenuItem childCommand)
		{
			IDocumentSupportable[] result = null;

			switch (businessContext)
			{
				case BusinessContext.Shipment:
					result = GetShipmentChildCollection(menu, childCommand);
					break;

				case BusinessContext.DeliveryAgent:
					foreach (DeliveryAgentOrgHeader deliveryAgentToPrint in DeliveryAgentsToPrint)
					{
						deliveryAgentToPrint.LoadShipmentCollection(ForwardingConsol);
					}
					result = (IDocumentSupportable[])DeliveryAgentsToPrint.ToArray(typeof(IDocumentSupportable));
					break;

				case BusinessContext.ConsolAgent:
					result = GetShipmentInvoicesForConsolAgent(BusinessContext.ConsolAgent);
					break;

				case BusinessContext.Consol:
					result = new IDocumentSupportable[] { Consol };
					break;

				case BusinessContext.ForwardingContainer:
					result = GetChildCollectionByContainer(ContainersToSelectFrom);
					break;

				case BusinessContext.JPAFRHeader:
					var header = ForwardingConsol.GetAFRHeader() as IDocumentSupportable;
					result = header == null ? Array.Empty<IDocumentSupportable>() : new IDocumentSupportable[] { header };
					break;

				case BusinessContext.CAeManifest:
					var caMHmaster = ForwardingConsol.CusCAeMHMaster as IDocumentSupportable;
					result = caMHmaster == null ? Array.Empty<IDocumentSupportable>() : new IDocumentSupportable[] { caMHmaster };
					break;

				case BusinessContext.CusMAWB:
					result = ForwardingConsol.GBCusMAWBs.Cast<IDocumentSupportable>().ToArray();
					break;
			}

			return result;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "harded coded document title., Menu Name")]
		IDocumentSupportable[] GetShipmentChildCollection(IStmMenuItem menu, IStmMenuItem childCommand)
		{
			IDocumentSupportable[] result = null;

			if (childCommand != null && childCommand.SU_MenuName.EqualsIgnoringCase("DocBuilder Invoice"))
			{
				result = GetShipmentInvoicesForConsolAgent(BusinessContext.Shipment);
			}
			else
			{
				ShipmentCollection shipmentsToSelectFrom = new ShipmentCollection(Factory);
				switch (ForwardingConsol.JK_PrintOptionForColoadsOnOtherDocs)
				{
					case FreightConstants.PrintOptionForCoLoads.All:
						shipmentsToSelectFrom.AddRange(ForwardingConsol.Shipments);
						break;

					case FreightConstants.PrintOptionForCoLoads.MastersOnly: // maybe be Forwarding specific
						shipmentsToSelectFrom.AddRange(ForwardingConsol.ShipmentsForTotalling);
						break;

					case FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly: // maybe be Forwarding specific
						foreach (ForwardingShipment shipment in ForwardingConsol.Shipments)
						{
							if (!shipment.IsLeadOrMaster)
							{
								shipmentsToSelectFrom.Add(shipment);
							}
						}
						break;
				}

				shipmentsToSelectFrom.Sort(JobShipmentSchema.Constants.JS_UniqueConsignRef, System.ComponentModel.ListSortDirection.Ascending);

				foreach (ForwardingShipment shipment in shipmentsToSelectFrom)
				{
					((ICreditControlledDocumentDelivery)shipment).GetDocumentLogin += (sender, e) =>
						((ICreditControlledDocumentDelivery)ForwardingConsol).RaiseOnGetDocumentLogin(e);
				}

				if (menu.SU_MenuName.StartsWith("NorthPort", StringComparison.Ordinal) || menu.SU_MenuName.StartsWith("WestPort", StringComparison.Ordinal) || menu.SU_MenuName.StartsWith("North/WestPort", StringComparison.Ordinal))
				{
					ShipmentCollection coll = new ShipmentCollection(Factory);
					foreach (ForwardingShipment shipment in shipmentsToSelectFrom)
					{
						if (shipment.IsSea && shipment.JS_PackingMode == Core.Constants.ContainerModes.LCL)
						{
							coll.Add(shipment);
						}
					}
					result = (IDocumentSupportable[])coll.ToArray(typeof(IDocumentSupportable));
				}
				else
				{
					if (menu.SU_MenuName.StartsWith((NoResString)"Export Receival Advice", StringComparison.Ordinal))
					{
						foreach (ForwardingShipment shipment in shipmentsToSelectFrom)
						{
							shipment.CurrentConsolForDocuments = this.ForwardingConsol;
						}
					}
					result = (IDocumentSupportable[])shipmentsToSelectFrom.ToArray(typeof(IDocumentSupportable));
				}
			}

			return result;
		}

		public override bool IsImport
		{
			get { return ForwardingConsol.IsImport(); }
		}

		IDocumentSupportable[] GetShipmentInvoicesForConsolAgent(BusinessContext context)
		{
			ArrayList results = new ArrayList();

			if (ForwardingConsol.ReceivingForwarder != null)
			{
				foreach (ForwardingShipment shipment in ForwardingConsol.Shipments)
				{
					ZQuery jobUniqueRefQuery = new ZQuery();
					jobUniqueRefQuery.AddToFilter(AccTransactionHeaderSchema.AH_JobNumber, shipment.JS_UniqueConsignRef);
					jobUniqueRefQuery.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_JH, SQLComparisonOperator.NotEqual, null);

					ZQuery shipmentFilter = new ZQuery();
					shipmentFilter.AddToFilter(jobUniqueRefQuery);
					shipmentFilter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_OH, SQLComparisonOperator.Equal, ForwardingConsol.ReceivingForwarder.PK);
					shipmentFilter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
					shipmentFilter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);

					AccTransactionHeader transactionHeader = Factory.LoadTop1<AccTransactionHeader>(shipmentFilter);
					if (transactionHeader != null)
					{
						results.Add(new ForwardingShipmentWrapperWithConsolAgent(shipment, ForwardingConsol.ReceivingForwarder, context));
					}
				}
			}

			return (IDocumentSupportable[])results.ToArray(typeof(IDocumentSupportable));
		}

		#endregion

		#region GetBODocDataProvidersNotFoundMessage

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a document menu name")]
		const string MenuNameOfStandardShippingNote = "Standard Shipping Note";

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (commandBeingRun.SU_MenuName == MenuNameOfStandardShippingNote)
			{
				return Res.GetString("d960fef7-3ca0-4b4d-8238-a3b3f67dc2fa", "Please choose a container to print.");
			}

			switch (dataContextValue.DataContext)
			{
				case Core.Constants.DataContext.AWB:
					{
						if (ForwardingConsol.AWBHeader == null)
						{
							return Res.GetString("bba31a02-d28d-47f4-b56f-fc8979cfbc73", "This Consol is not associated with a AWB data.");
						}

						break;
					}

				case Core.Constants.DataContext.ERA:
				case Core.Constants.DataContext.LTL:
				case Core.Constants.DataContext.IMO:
				case Core.Constants.DataContext.Container:
					{
						if (ForwardingConsol.Containers.Any())
						{
							if (dataContextValue.DataContext == Core.Constants.DataContext.IMO
								|| dataContextValue.DataContext == Core.Constants.DataContext.Container)
							{
								return Res.GetString("45defab2-fa4c-49f1-8136-5aaf232503a7", "Please choose a container to print.");
							}
						}
						else
						{
							return Res.GetString("59198591-edca-4270-bf60-c35878fe912b", "This consol does not have any containers.");
						}

						break;
					}

				case Core.Constants.DataContext.ARInvoice:
				case Core.Constants.DataContext.GenericFreightJobInvoice:
					{
						var transactionHeaders = new InvoiceLoader(Factory).GetInvoicesForUniqueRef(new[] { ForwardingConsol.ReceivingForwarderPK }, ForwardingConsol.JK_UniqueConsignRef);
						if (!transactionHeaders.Any())
						{
							return Res.GetString("a1403975-37ed-413f-a38f-c643066663fc", "There is no invoice data associated with this consol.");
						}

						break;
					}
			}

			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		#endregion

		#region ShowReasonForNotPrinting

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (commandBeingRun != null && commandBeingRun.SU_MenuName == MenuNameOfStandardShippingNote)
			{
				return true;
			}
			else
			{
				return dataContext != Core.Constants.DataContext.NZCustoms
					&& dataContext != Core.Constants.DataContext.ImportCargoLabel
					&& dataContext != Core.Constants.DataContext.LoadListDocument
					&& dataContext != Core.Constants.DataContext.Consol
					&& dataContext != Core.Constants.DataContext.CommonConsol
					&& dataContext != Core.Constants.DataContext.ForwardingConsol
					&& dataContext != Core.Constants.DataContext.Notes
					&& dataContext != Core.Constants.DataContext.ForwardingPreAdvice
					&& dataContext != Core.Constants.DataContext.ForwardingShipmentAndConsol
					&& dataContext != Core.Constants.DataContext.RequestForMissingDocuments
					&& dataContext != Core.Constants.DataContext.TimeSlotRequest
					&& dataContext != Core.Constants.DataContext.CombinedCartageAdvice
					&& dataContext != Core.Constants.DataContext.ShippingOrder
					&& dataContext != Core.Constants.DataContext.ProfitShareDetail
					&& dataContext != Core.Constants.DataContext.ForwardingConsolGbADS
					&& dataContext != Core.Constants.DataContext.AsycudaManifestHeader
					&& dataContext != Core.Constants.DataContext.EuNcts
					&& base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
			}
		}

		#endregion

		#endregion

		#region GetContactOrganisation

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			IDocumentDeliveryContact result;

			if (contact == ContactType.ShippingLine)
			{
				result = ColoadContact;
			}
			else if (contact == ContactType.LocalTransport)
			{
				result = GetLocalTransportContactOrganisation(direction);
			}
			else
			{
				result = base.GetContactOrganisation(menuName, contact, direction);
			}

			return result;
		}

		public override IOrgContact GetOverriddenDeliveryContact(IStmMenuItem menuItem)
		{
			if (menuItem != null && menuItem.SU_MenuName == AWB.AWBActions.DocumentNames.ConsignmentSecurityDeclaration)
			{
				var contact = new DefaultContactFinder(null).DefaultContact(ContactType.NoContactType);
				contact.OC_NotifyMode = Constants.ContactNotifyModes.Print;
				return contact;
			}

			return base.GetOverriddenDeliveryContact(menuItem);
		}

		protected OrgHeaderContact ColoadContact
		{
			get
			{
				OrgHeaderContact result = null;

				if (Consol.IsCoLoad && Consol.Creditor != null)
				{
					result = new OrgHeaderContact(Consol.Creditor, Consol.CreditorAddress);
				}
				else
				{
					result = new OrgHeaderContact(Consol.ShippingLine, Consol.ShippingLineAddress);
				}

				return result;
			}
		}

		OrgHeaderContact GetLocalTransportContactOrganisation(DocumentDirection direction)
		{
			OrgHeaderContact result = null;
			if (direction == DocumentDirection.ARV && Consol.ArrivalUnpackCFSTransport != null)
			{
				result = new OrgHeaderContact(Consol.ArrivalUnpackCFSTransport, Consol.ArrivalUnpackCFSTransportAddress);
			}
			else if (direction == DocumentDirection.DEP && Consol.DeparturePackCFSTransport != null)
			{
				result = new OrgHeaderContact(Consol.DeparturePackCFSTransport, Consol.DeparturePackCFSTransportAddress);
			}
			return result;
		}

		#endregion

		#region AWBs

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Doc Name")]
		protected ZBool PrintingNeutralMaster(ZString documentName)
		{
			return (documentName == "Neutral MAWB" || documentName == "Laser MAWB");
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Doc Name")]
		protected ZBool PrintingFinalMaster(ZString documentName)
		{
			return documentName == "Print Final Master";
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Doc Name")]
		protected ZBool IsPrintingAWBBarcodeLabel(ZString documentName)
		{
			return (documentName == "AWB Barcode Label");
		}

		protected ZBool IsPrintingHAWB(ZString documentName)
		{
			return documentName.StartsWith((NoResString)"Laser HAWB", StringComparison.Ordinal) // Doc Name
				|| documentName.StartsWith((NoResString)"Neutral HAWB", StringComparison.Ordinal); // Doc Name
		}

		protected ZBool HasShipmentsWithOuterPacks
		{
			get
			{
				foreach (CommonShipment shipment in ForwardingConsol.Shipments)
				{
					if (shipment.JS_OuterPacks > 0)
					{
						return true;
					}
				}

				return false;
			}
		}

		#endregion

		#region DocumentPrintRequested

		protected void DocumentEventSource_DocumentPrintRequested(object sender, DocumentCancelEventArgs e)
		{
			Func<Func<ZString, ZBool>, ZBool> isPrintingSystemDefinedDocument = predicate => predicate(e.MenuItem.SU_MenuName) && e.MenuItem.SU_IsSystemDefined;

			if (isPrintingSystemDefinedDocument(PrintingNeutralMaster))
			{
				if (!ForwardingConsol.JK_IsNeutralMaster && (ForwardingConsol.IsAgentOrDirect || ForwardingConsol.IsAWBCoload))
				{
					e.Cancel = true;

					string message = Res.GetString("8ef65fc5-4d6f-4f2b-85ef-70e221417880", "This is a not a Neutral Master Air Waybill.\r\n\r\nPrinting of the Neutral Master Air Waybill is not allowed for Carrier Air Waybills.\r\n\r\nSelect 'Is Neutral Master' to allocate a Neutral Air Waybill number.");
					QueryProvider.ShowMessage(message, Res.GetString("821abeb0-9293-4762-b539-8c35cbf4dda5", "MAWB Printing"));
				}
			}
			else if (isPrintingSystemDefinedDocument(PrintingFinalMaster))
			{
				e.Cancel = true;
				GetShipmentToConfirmAWBOrHBL(e);
				QueryProvider.PrintFinalMaster(ForwardingConsol, e.MenuItem.SU_MenuPath);
			}
			else if (isPrintingSystemDefinedDocument(IsPrintingAWBBarcodeLabel))
			{
				if (!HasShipmentsWithOuterPacks)
				{
					e.Cancel = true;
					string message = Res.GetString("060083ee-ccef-4e42-b695-1f69914c4338", "No labels will be printed.\r\n\r\nLabels will only be printed if this Consol has at least one Shipment attached with Outer Packs > 0.");
					QueryProvider.ShowMessage(message, Res.GetString("021f4f7d-7751-40eb-97e1-f28235aa7e68", "AWB Barcode Label Printing"));
				}
				else
				{
					e.Cancel = true;
					QueryProvider.PrintAWBLabels(ForwardingConsol);
				}
			}
			else if (e.MenuItem.SU_MenuName.ToUpper().StartsWith((NoResString)"BILL OF LADING", StringComparison.Ordinal) || isPrintingSystemDefinedDocument(IsPrintingHAWB)) // Doc Name
			{
				GetShipmentToConfirmAWBOrHBL(e);
			}
			else if (e.MenuItem.PK == new ZGuid(ChGroupDeliveryNoteStmMenuItemPK))
			{
				e.Cancel = !CanPrintChGroupDeliveryNoteForConsol();
			}

			foreach (ForwardingShipment shipment in ForwardingConsol.Shipments)
			{
				shipment.CurrentConsolForDocuments = ForwardingConsol;
			}
		}

		void GetShipmentToConfirmAWBOrHBL(DocumentCancelEventArgs e)
		{
			ForwardingShipment shipmentToConfirmAWBOrHBL = ForwardingConsol.Shipments.Cast<ForwardingShipment>().FirstOrDefault(shipment => shipment.AWBOrHBLPrintingShouldBeConfirmed());

			if (shipmentToConfirmAWBOrHBL != null)
			{
				e.Cancel = !QueryProvider.ConfirmBOLPrinting(shipmentToConfirmAWBOrHBL);
			}
		}

		#endregion

		#region DocumentPrePrinted

		protected void DocumentEventSource_DocumentPrePrinted(object sender, DocumentPrintedEventArgs e)
		{
			foreach (ForwardingShipment shipment in ForwardingConsol.Shipments)
			{
				shipment.CurrentConsolForDocuments = ForwardingConsol;
			}
		}

		#endregion

		#region DocumentPrinted

		protected void DocumentEventSource_DocumentPrinted(object sender, DocumentPrintedEventArgs e)
		{
			if (IsPrintingHAWB(e.MenuItem.SU_MenuName))
			{
				ForwardingConsol.GetLogs().AddNew(Events.DocumentSent, ForwardingConsol.HAWBPrintedLogReference);
			}
		}

		#endregion

		#region GetDocWrappers

		protected DocumentWrapper GetWrapperForImportCargoLabel()
		{
			DocumentImportCargoLabel importCargoLabelToPrint = QueryProvider.GetImportCargoLabelToPrint(new DocumentImportCargoLabel(ForwardingConsol));

			return importCargoLabelToPrint != null ? DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.ForwardingConsol, importCargoLabelToPrint) : null;
		}

		// OK (was protected virtual, need to check SG Customs)
		DocumentWrapper GetWrapperForCommonConsol(Constants.DataContext dataContext)
		{
			DocumentCommonConsol consolToPrint = QueryProvider.GetConsolToPrint(new DocumentCommonConsol(ForwardingConsol, dataContext));

			return consolToPrint != null ? DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.ForwardingConsol, consolToPrint) : null;
		}

		#endregion

		#region Delivery Agents

		public DeliveryAgentToSelectFromForPrintingCollection DeliveryAgentsToSelectFrom
		{
			get
			{
				List<ZGuid> deliveryAgentPks = new List<ZGuid>();

				foreach (ForwardingShipment shipment in ForwardingConsol.Shipments)
				{
					if (shipment.DeliveryAgent == null && ForwardingConsol.ReceivingForwarderPK != ZGuid.Empty)
					{
						deliveryAgentPks.Add(ForwardingConsol.ReceivingForwarderPK);
					}
					else if (shipment.DeliveryAgent != null)
					{
						deliveryAgentPks.Add(shipment.DeliveryAgent.PK);
					}
				}

				DeliveryAgentToSelectFromForPrintingCollection collection = new DeliveryAgentToSelectFromForPrintingCollection(Factory);

				if (deliveryAgentPks.Count > 0)
				{
					collection.AddRange(Factory.Load<DeliveryAgentToSelectFromForPrinting>(new ZQuery(OrgHeaderSchema.PK, deliveryAgentPks.Distinct().ToArray())));
				}

				return collection;
			}
		}

		public DeliveryAgentToPrintCollection DeliveryAgentsToPrint
		{
			get { return deliveryAgentsToPrint ?? (deliveryAgentsToPrint = new DeliveryAgentToPrintCollection(Factory)); }
		}
		DeliveryAgentToPrintCollection deliveryAgentsToPrint;

		#endregion

		/// <summary>
		/// Asks GB customs whether the Airline Delivery Schedule can be printed.  If no, tell the user why they cannot print it.
		/// </summary>
		ZString GetReasonForCannotPrintGbAirlineDeliverySchedule()
		{
			var gbHelper = ObjectFactory.Get<GB.GBChief.IChiefExportConsolIntegrationWrapper>();
			return gbHelper.GetReasonWhyCannotPrintAirlineDeliveryScheduleForConsol((Enterprise.Integration.Forwarding.IForwardingConsol)Consol);
		}

		ZBool CanPrintChGroupDeliveryNoteForConsol()
		{
			var chHelper = ObjectFactory.Get<CH.IForwardingConsolIntegrationHelper>();
			return chHelper.CanPrintGroupDeliveryNoteForConsol((Enterprise.Integration.Forwarding.IForwardingConsol)Consol, QueryProvider);
		}
	}
}
