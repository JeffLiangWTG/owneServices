using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Integration.Customs;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentDocumentSupporter : CommonShipmentDocumentSupporter
	{
		public ForwardingShipmentDocumentSupporter(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		protected override void InitialiseCore(IDocumentEvents documentEventSource)
		{
			base.InitialiseCore(documentEventSource);

			bookingDeliveryCancelled = false;
		}

		public new static CommonShipmentDocumentSupporter New(CommonShipment shipment)
		{
			ForwardingShipmentDocumentSupporter result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = (ForwardingShipmentDocumentSupporter)overridden(shipment);
			}
			else if (shipment != null)
			{
				result = new ForwardingShipmentDocumentSupporter((ForwardingShipment)shipment);
			}

			return result;
		}

		public new ForwardingShipment Shipment
		{
			get { return (ForwardingShipment)base.Shipment; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Shipment; }
		}

		public override BusinessContext[] SupportedChildBusinessContexts
		{
			get { return new[] { BusinessContext.Shipment, BusinessContext.SubShipment, BusinessContext.Customs, BusinessContext.DtbBooking, BusinessContext.CusHAWB }; }
		}

		#region Query Provider

		protected new IForwardingShipmentDocumentSupporterQueryProvider QueryProvider
		{
			get { return (IForwardingShipmentDocumentSupporterQueryProvider)base.QueryProvider; }
		}

		protected override ICommonShipmentDocumentSupporterQueryProvider GetQueryProvider()
		{
			return Factory.GetValue<IForwardingShipmentDocumentSupporterQueryProvider>() ?? new ForwardingShipmentDocumentSupporterQueryProvider();
		}

		protected override ICommonShipmentDocumentSupporterQueryProvider GetNonGuiQueryProvider()
		{
			return new ForwardingShipmentDocumentSupporterQueryProvider();
		}

		#endregion

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menu, BusinessContext businessContext, IStmMenuItem childCommand)
		{
			IDocumentSupportable[] result = null;

			switch (businessContext)
			{
				case BusinessContext.Shipment:
					result = new IDocumentSupportable[] { Shipment };
					break;
				case BusinessContext.Customs:
					if (Shipment.DeclarationForDocuments != null)
					{
						result = new[] { (IDocumentSupportable)Shipment.DeclarationForDocuments };
					}
					else
					{
						result = Array.Empty<IDocumentSupportable>();
					}
					break;
				case BusinessContext.DtbBooking:
					bookings = TransportBookingLoader.GetBookingsToDeliver(Shipment, menu);
					bookingDeliveryCancelled = !bookings.Any();
					result = bookings.Cast<IDocumentSupportable>().ToArray();
					break;
				case BusinessContext.SubShipment:
					result = (Shipment.IsMasterShipmentRepresentingAllChildShipments || Shipment.IsBuyersConsolLead)
						? Shipment.CoLoadShipments.Cast<IDocumentSupportable>().ToArray()
						: Array.Empty<IDocumentSupportable>();
					break;
				case BusinessContext.CusHAWB:
					result = Shipment.GBCusHAWBs?.Cast<IDocumentSupportable>().ToArray() ?? Array.Empty<IDocumentSupportable>();
					break;
			}

			return result;
		}

		public IDocumentSupportable[] Bookings
		{
			get
			{
				return bookings.Select(b =>
				{
					// Use factory belonging to the booking parent, as it has the factory that gets saved at the end of a non user-interactive process
					var factory = Shipment.Factory;
					var reloadedBooking = (IDocumentSupportable)factory.Load<IDtbBooking>(b.PK);
					if (reloadedBooking == null)
					{
						ErrorReporter.ReportOnce("ForwardingShipmentDocumentSupporter_NullBooking", $"Booking {b.HumanReadableName} from factory '{b.Factory.NameForDebugging}' could not be loaded by factory '{factory.NameForDebugging}' belonging to {Shipment.HumanReadableName}");
					}
					return reloadedBooking;
				}).Where(b => b != null).ToArray();
			}
		}
		IEnumerable<IDtbBooking> bookings = Array.Empty<IDtbBooking>();

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override ZBool ShowReasonForNotPrinting(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return !bookingDeliveryCancelled
				&& dataContext != Core.Constants.DataContext.BusinessObject
				&& dataContext != Core.Constants.DataContext.Shipment
				&& dataContext != Core.Constants.DataContext.Notes
				&& dataContext != Core.Constants.DataContext.PreAlert
				&& dataContext != Core.Constants.DataContext.CartageAdvice
				&& dataContext != Core.Constants.DataContext.ForwardingPreAdvice
				&& dataContext != Core.Constants.DataContext.ShipperDepartureNotice
				&& dataContext != Core.Constants.DataContext.ShipmentDeclaration
				&& dataContext != Core.Constants.DataContext.CombinedCartageAdvice
				&& dataContext != Core.Constants.DataContext.ShippingOrder
				&& dataContext != Core.Constants.DataContext.ERA
				&& dataContext != Core.Constants.DataContext.CFSAndForwardingShipment
				&& dataContext != Core.Constants.DataContext.RequestForMissingDocuments
				&& dataContext != Core.Constants.DataContext.ForwardingShipmentAndConsol
				&& dataContext != Core.Constants.DataContext.GenericPickupDeliveryConfirm
				&& dataContext != Core.Constants.DataContext.GenericFreightJobByReleaseStatus
				&& dataContext != Core.Constants.DataContext.ForwardingShipment
				&& dataContext != Core.Constants.DataContext.Worksheet
				&& dataContext != Core.Constants.DataContext.Service
				&& dataContext != Core.Constants.DataContext.TimeSlotRequest
				&& dataContext != Core.Constants.DataContext.InBond7512Departure
				&& dataContext != Core.Constants.DataContext.FreightLabels
				&& dataContext != Core.Constants.DataContext.LetterOfIndemnity
				&& dataContext != Core.Constants.DataContext.IMO
				&& dataContext != Core.Constants.DataContext.RequestForService
				&& dataContext != Core.Constants.DataContext.SystemSectionRepository
				&& dataContext != Core.Constants.DataContext.GenericCommercialInvoice
				&& base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		bool bookingDeliveryCancelled;

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[]
				{
					Core.Constants.DataContext.Shipment,
					Core.Constants.DataContext.AWB,
					Core.Constants.DataContext.Declaration,
					Core.Constants.DataContext.NZCustoms,
					Core.Constants.DataContext.DeclarationWithCusEntryHeaders,
					Core.Constants.DataContext.Container,
					Core.Constants.DataContext.Notes,
					Core.Constants.DataContext.CartageAdvice,
					Core.Constants.DataContext.CommercialInvoice,
					Core.Constants.DataContext.ChargeSheet,
					Core.Constants.DataContext.CusEntryHeader,
					Core.Constants.DataContext.ComInvoiceHeader,
					Core.Constants.DataContext.PreAlert,
					Core.Constants.DataContext.ForwardingPreAdvice,
					Core.Constants.DataContext.ARInvoice,
					Core.Constants.DataContext.ShipperDepartureNotice,
					Core.Constants.DataContext.LandedCostEntryHeaders,
					Core.Constants.DataContext.LandedCostHeader,
					Core.Constants.DataContext.ShipmentDeclaration,
					Core.Constants.DataContext.FreightLabels,
					Core.Constants.DataContext.ShippingOrder,
					Core.Constants.DataContext.ATD,
					Core.Constants.DataContext.EFTPaymentAdvice,
					Core.Constants.DataContext.LetterOfIndemnity,
					Core.Constants.DataContext.CombinedCartageAdvice,
					Core.Constants.DataContext.ERA,
					Core.Constants.DataContext.CFSAndForwardingShipment,
					Core.Constants.DataContext.RequestForMissingDocuments,
					Core.Constants.DataContext.Worksheet,
					Core.Constants.DataContext.ForwardingShipment,
					Core.Constants.DataContext.IMO,
					Core.Constants.DataContext.RequestForService,
					Core.Constants.DataContext.ForwardingShipmentAndConsol,
					Core.Constants.DataContext.Service,
					Core.Constants.DataContext.TimeSlotRequest,
					Core.Constants.DataContext.GenericFreightJob,
					Core.Constants.DataContext.GenericFreightJobRouting,
					Core.Constants.DataContext.GenericPickupDeliveryConfirm,
					Core.Constants.DataContext.GenericFreightJobByContainerIfFCL,
					Core.Constants.DataContext.GenericCommercialInvoice,
					Core.Constants.DataContext.SGPrintPermit,
					Core.Constants.DataContext.SGRefundInfo,
					Core.Constants.DataContext.EUR1,
					Core.Constants.DataContext.SADH,
					Core.Constants.DataContext.ESSADH,
					Core.Constants.DataContext.LiquidationDetails,
					Core.Constants.DataContext.GbTaxEstimator,
					Core.Constants.DataContext.GenericChargeSheet,
					Core.Constants.DataContext.GenericFreightJobFrmShipByContIfFCL,
					Core.Constants.DataContext.InBond7512Departure,
					Core.Constants.DataContext.GenericFreightJobByComInv,
					Core.Constants.DataContext.GenericFreightJobInvoice,
					Core.Constants.DataContext.GenericFreightJobByPackages,
					Core.Constants.DataContext.GenericFreightJobByPackages1Doc,
					Core.Constants.DataContext.GenericFreightJobBySelectedPackages,
					Core.Constants.DataContext.GenericFreightJobBySelectedPkgs1Doc,
					Core.Constants.DataContext.GenericImportCargoLabel,
					Core.Constants.DataContext.GbCcsuk,
					Core.Constants.DataContext.EuNcts,
					Core.Constants.DataContext.FRSADH,
					Core.Constants.DataContext.IEImportAccompanyingDocument,
					Core.Constants.DataContext.IEIADClearanceSlip,
					Core.Constants.DataContext.ZADA306,
				};
		}

		internal const string InBond7512Departure = ".CBPForm7512";

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			List<DataContextValue> result = GetSupportedBODataSourcesFor(typeof(ForwardingShipment));
			result.Add(new DataContextValue(InBond7512Departure));

			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.ToString().StartsWith(InBond7512Departure, StringComparison.Ordinal))
			{
				return GetBODocDataFor7512Print(dataContextValue, commandBeingRun);
			}

			return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
		}

		IBODocDataProvider[] GetBODocDataFor7512Print(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var result = new List<IBODocDataProvider>();

			var inBondHeader = GetUSInBondHeader();

			if (inBondHeader != null)
			{
				result.AddRange(((IDocumentSupportable)inBondHeader).DocumentSupporter.GetBODocDataProviders(dataContextValue, commandBeingRun));
			}

			return result.ToArray();
		}

		BusinessObject GetUSInBondHeader()
		{
			return (BusinessObject)Shipment.GetInBondHeader(CusInBondApplicationCodeList.Codes.InBond);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmantainableCode")]

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a document menu name, Document menu name")]
		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			StmMenuItem concreteCommandBeingRun = commandBeingRun as StmMenuItem;

			if (dataContext == Core.Constants.DataContext.GenericFreightJobInvoice)
			{
				return GetWrappersForInvoice(commandBeingRun, true);
			}

			if (dataContext == Core.Constants.DataContext.GenericPickupDeliveryConfirm)
			{
				using (Shipment.SuspendDeclarationForDocuments())
				{
					return GetDocWrappersForCartageAdviceContext(commandBeingRun, dataContext);
				}
			}

			if (dataContext == Core.Constants.DataContext.GenericFreightJobRouting)
			{
				DocumentShipment shipmentBizObject = new DocumentShipment(Shipment, dataContext);

				Transport transportToPrint = QueryProvider.GetTransportToPrint(shipmentBizObject);

				return transportToPrint != null ? DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJobRouting, Shipment, transportToPrint) : null;
			}

			if (dataContext == Core.Constants.DataContext.GenericFreightJobByContainerIfFCL)
			{
				ContainerSelectionDelegate containerSelectionDelegate = null;
				if (commandBeingRun != null && commandBeingRun.SU_MenuName.EqualsIgnoringCase("IMO Dangerous Goods Declaration"))
				{
					containerSelectionDelegate = ContainerSelectionForDangerousGoods;
				}

				if (commandBeingRun != null && commandBeingRun.SU_MenuName.EqualsIgnoringCase("Multimodal Dangerous Goods Form"))
				{
					containerSelectionDelegate = (container) =>
					{
						return container.PackLines.OfType<ForwardingPackLine>().Any(p => p.JL_JS == PK
							&& (p.JL_RH_NKCommodityCode == Core.Constants.CargoTypes.Hazardous || p.UNDGs.Count > 0));
					};
				}

				return GetDocWrappersForGenericFreightJobByContainerIfFCL(concreteCommandBeingRun == null ? DocumentDirection.ANY : concreteCommandBeingRun.GetDocumentDirection(), containerSelectionDelegate);
			}

			if (dataContext == Core.Constants.DataContext.GenericFreightJobFrmShipByContIfFCL)
			{
				using (Shipment.SuspendDeclarationForDocuments())
				{
					return GetDocWrappersForGenericFreightJobByContainerIfFCL(concreteCommandBeingRun == null ? DocumentDirection.ANY : concreteCommandBeingRun.GetDocumentDirection(), null);
				}
			}

			if (dataContext == Core.Constants.DataContext.GenericFreightJobServices)
			{
				var docs = Shipment.DocsAndCartage
					?? throw new ArgumentNullException(nameof(dataContext), "Shipment.DocsAndCartage cannot be null");

				return docs.GetServiceWrappersForDocBuilder(((IHaveServices)docs).ServiceParent, Core.Constants.DataContext.GenericFreightJob);
			}

			if (dataContext == Core.Constants.DataContext.GenericChargeSheet && concreteCommandBeingRun != null)
			{
				return GetGenericChargeSheetWrappers(concreteCommandBeingRun.GetDocumentDirection());
			}

			if (dataContext == Core.Constants.DataContext.NZCustoms)
			{
				dataContext = Core.Constants.DataContext.Declaration;
			}

			if (commandBeingRun != null
				&& commandBeingRun.SU_MenuName.EqualsIgnoringCase("Shi Lian Dan")
				&& !QueryProvider.PrintShiLianDan(Shipment))
			{
				return null;
			}

			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Shipment);
			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			DocumentWrapper result = null;

			switch (dataContext)
			{
				case Core.Constants.DataContext.EUR1:
					result = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.EUR1, Shipment);
					break;

				case Core.Constants.DataContext.EuNcts:
					var nctsHeaderDocumentSupportable = Shipment.NctsHeaderForDocuments as IDocumentSupportable;
					return nctsHeaderDocumentSupportable?.DocumentSupporter.GetDocumentWrappers(dataContext, commandBeingRun);

				case Core.Constants.DataContext.AWB:
					{
						if (commandBeingRun != null && Shipment.AWBHeader != null && (commandBeingRun.SU_MenuName != AWBActions.DocumentNames.AWBBarcodeLabels || QueryProvider.PrintAWBBarcodeLabel()))
						{
							Shipment.PopulateAWBForDocuments();
							result = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.AWB, Shipment.AWBHeader);
						}
						break;
					}

				case Core.Constants.DataContext.Shipment:
					{
						var documentOptions = new DocumentShipment(
							Shipment,
							dataContext,
							concreteCommandBeingRun != null && IsPrintingHBL(concreteCommandBeingRun.SU_MenuName));

						result = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.ForwardingShipment, documentOptions);
						break;
					}
				case Core.Constants.DataContext.Notes:
				case Core.Constants.DataContext.PreAlert:
				case Core.Constants.DataContext.ForwardingPreAdvice:
				case Core.Constants.DataContext.ShipperDepartureNotice:
				case Core.Constants.DataContext.ShipmentDeclaration:
				case Core.Constants.DataContext.CombinedCartageAdvice:
				case Core.Constants.DataContext.ShippingOrder:
				case Core.Constants.DataContext.ERA:
				case Core.Constants.DataContext.CFSAndForwardingShipment:
				case Core.Constants.DataContext.RequestForMissingDocuments:
				case Core.Constants.DataContext.ForwardingShipmentAndConsol:
				case Core.Constants.DataContext.ForwardingShipment:
				case Core.Constants.DataContext.Worksheet:
				case Core.Constants.DataContext.Service:
				case Core.Constants.DataContext.TimeSlotRequest:
				case Core.Constants.DataContext.InBond7512Departure:
					result = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.ForwardingShipment, Shipment);
					break;

				case Core.Constants.DataContext.Declaration:
				case Core.Constants.DataContext.SGPrintPermit:
				case Core.Constants.DataContext.SGRefundInfo:
				case Core.Constants.DataContext.DeclarationWithCusEntryHeaders:
				case Core.Constants.DataContext.CommercialInvoice:
				case Core.Constants.DataContext.CusEntryHeader:
				case Core.Constants.DataContext.ATD:
				case Core.Constants.DataContext.EFTPaymentAdvice:
				case Core.Constants.DataContext.ComInvoiceHeader:
				case Core.Constants.DataContext.LandedCostEntryHeaders:
				case Core.Constants.DataContext.LandedCostHeader:
				case Core.Constants.DataContext.SADH:
				case Core.Constants.DataContext.ESSADH:
				case Core.Constants.DataContext.FRSADH:
				case Core.Constants.DataContext.LiquidationDetails:
				case Core.Constants.DataContext.GbTaxEstimator:
				case Core.Constants.DataContext.GenericFreightJobByComInv:
				case Core.Constants.DataContext.IEImportAccompanyingDocument:
				case Core.Constants.DataContext.IEIADClearanceSlip:
					if (Shipment.DeclarationForDocuments != null)
					{
						var documentSupportable = Shipment.DeclarationForDocuments as IDocumentSupportable
							?? throw new ArgumentNullException(nameof(dataContext), "Shipment.DeclarationForDocuments as IDocumentSupportable cannot be null");

						var documentSupporter = documentSupportable.DocumentSupporter
							?? throw new ArgumentNullException(nameof(dataContext), "Shipment.DeclarationForDocuments.DocumentSupporter cannot be null");

						return documentSupporter.GetDocumentWrappers(dataContext, commandBeingRun);
					}
					break;

				case Core.Constants.DataContext.Container:
					return GetContainerWrappers(ZBool.False, ZBool.True, commandBeingRun);

				case Core.Constants.DataContext.CartageAdvice:
					return GetDocWrappersForCartageAdviceContext(commandBeingRun, dataContext);

				case Core.Constants.DataContext.ARInvoice:
					return GetWrappersForInvoice(commandBeingRun, false);

				case Core.Constants.DataContext.GenericFreightJobByPackages:
				case Core.Constants.DataContext.GenericFreightJobByPackages1Doc:
					using (Shipment.SuspendDeclarationForDocuments())
					{
						return GetGenericWrapperForPacks(dataContext,
							dataContext == Core.Constants.DataContext.GenericFreightJobByPackages1Doc);
					}

				case Core.Constants.DataContext.GenericFreightJobBySelectedPackages:
				case Core.Constants.DataContext.GenericFreightJobBySelectedPkgs1Doc:
					using (Shipment.SuspendDeclarationForDocuments())
					{
						return GetGenericWrapperForPacksInRange(dataContext,
							dataContext == Core.Constants.DataContext.GenericFreightJobBySelectedPkgs1Doc);
					}

				case Core.Constants.DataContext.FreightLabels:
					{
						DocumentShipment documentOptions = QueryProvider.GetDocumentOptions(new DocumentShipment(Shipment, dataContext));

						if (documentOptions != null)
						{
							result = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.Shipment, documentOptions);
						}
						break;
					}
				case Core.Constants.DataContext.LetterOfIndemnity:
					{
						DocumentShipment docShipmentBizObject = new DocumentShipment(Shipment, dataContext);

						LetterOfIndemnityOptions options = QueryProvider.GetLetterOfIndemnityOptions(docShipmentBizObject);

						if (options != null)
						{
							if (options.ChangeMarksAndNumbers)
							{
								Shipment.JS_MarksAndNumbers = options.NewMarksAndNumbers;
							}

							if (options.ChangeGoodsDescription)
							{
								Shipment.DetailedGoodsDescriptionNoteText = options.NewGoodsDescription;
							}

							if (options.ChangeWeight)
							{
								Shipment.JS_ActualWeight = options.NewWeight;
								Shipment.JS_UnitOfWeight = options.NewWeightUnit;
							}

							if (options.ChangeVolume)
							{
								Shipment.JS_ActualVolume = options.NewVolume;
								Shipment.JS_UnitOfVolume = options.NewVolumeUnit;
							}

							result = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.Shipment, docShipmentBizObject);
						}
						break;
					}

				case Core.Constants.DataContext.ChargeSheet:
					if (concreteCommandBeingRun != null && concreteCommandBeingRun.GetDocumentDirection() == DocumentDirection.DEP)
					{
						DocumentShipment docShipmentBizObject = new DocumentShipment(Shipment, Core.Constants.DataContext.ChargeSheet);
						docShipmentBizObject.SetDefaultsFromDataContext();

						DebtorToSelectFromForPrinting[] debtorsToPrint = QueryProvider.GetDebtorsToPrint(docShipmentBizObject);

						DocumentWrapper[] wrappers = null;

						if (debtorsToPrint != null && debtorsToPrint.Length > 0)
						{
							wrappers = debtorsToPrint.Select(debtor =>
							{
								docShipmentBizObject.Debtor = debtor;
								return DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.Shipment, docShipmentBizObject);
							}).ToArray();
						}

						return wrappers;
					}
					else
					{
						result = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.ForwardingShipment, Shipment);
						break;
					}

				case Core.Constants.DataContext.IMO:
					var packs = Shipment.OuterPackLines
						?? throw new ArgumentNullException(nameof(dataContext), "Shipment.OuterPackLines cannot be null");

					bool hasUnContainerisedPacks = Shipment.OuterPackLines.Any((packLineBizObj) =>
					{
						PackLine pack = (PackLine)packLineBizObj;
						return pack.CommodityCode != null &&
							pack.CommodityCode.RH_Code == Core.Constants.CargoTypes.Hazardous &&
							pack.GetContainer(Shipment.DepartureConsol) == null;
					});

					return GetContainerWrappers(hasUnContainerisedPacks, false, commandBeingRun);

				case Core.Constants.DataContext.RequestForService:
					return Shipment.DocsAndCartage.GetServiceWrappers(Core.Constants.DataContext.Shipment);

				case Core.Constants.DataContext.SystemSectionRepository:
					return DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, BusinessObject);

				case Core.Constants.DataContext.GbCcsuk:
					if (Shipment.GBCusHAWBs != null && Shipment.GBCusHAWBs.Count > 0)
					{
						var lastHawb = (BusinessObject)Shipment.GBCusHAWBs.Last();
						var hawb = Factory.Load<GB.CCSUK.ICusHAWB>(lastHawb.PK);
						var documentSupportable = hawb as IDocumentSupportable
							?? throw new NotSupportedException("Shipment.GBCusHAWBs.Last()");
						var documentSupporter = documentSupportable.DocumentSupporter
							?? throw new NotSupportedException("Shipment.GBCusHAWBs.Last().DocumentSupporter");
						return documentSupporter.GetDocumentWrappers(dataContext, commandBeingRun);
					}
					break;

				case Core.Constants.DataContext.GenericImportCargoLabel:

					return GetWrapperForImportCargoLabel();

				case Core.Constants.DataContext.ZADA306:
					result = DocumentWrapperFactory.CreateCustomsWrapperZA(dataContext, Shipment);
					break;

				default:
					break; // Currently unsupported DataContext
			}

			return result != null ? new DocumentWrapper[] { result } : null;
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			var result = base.GetFilterValue(filterName);
			if (string.IsNullOrEmpty(result))
			{
				foreach (DocumentSupporter additionalBODataSource in AdditionalBODataSourceDocumentSupporters)
				{
					result = additionalBODataSource.GetFilterValue(filterName);
					if (!string.IsNullOrEmpty(result))
					{
						break;
					}
				}
			}
			return result;
		}

		public override string GetMenuTemplateFilterValue(MenuTemplateFilterType filterType, IBODocDataProvider docDataProvider)
		{
			var result = base.GetMenuTemplateFilterValue(filterType, docDataProvider);
			if (string.IsNullOrEmpty(result) && Shipment.NctsHeaderForDocuments is IDocumentSupportable documentSupportable)
			{
				result = documentSupportable.DocumentSupporter.GetMenuTemplateFilterValue(filterType, docDataProvider);
			}
			return result;
		}

		protected DocumentWrapper[] GetWrapperForImportCargoLabel()
		{
			if (Shipment.JS_OuterPacks <= 0)
			{
				return null;
			}

			var label = new DocumentImportCargoLabel(Shipment);
			var importCargoLabelToPrint = QueryProvider.GetImportCargoLabelToPrint(label);

			if (importCargoLabelToPrint != null)
			{
				var warppers = new List<DocumentWrapper>();

				var pieceCount = 0;

				for (var i = 0; i < Shipment.JS_OuterPacks; i++)
				{
					var wrapper = GetGenericWrappers().FirstOrDefault();
					if (wrapper != null)
					{
						wrapper.DocIncludeConsignee = Globals.IsWeb || importCargoLabelToPrint.IncludeConsignee;
						wrapper.DocIncludeConsignor = Globals.IsWeb || importCargoLabelToPrint.IncludeConsignor;
						wrapper.DocIncludeHouseBill = Globals.IsWeb || importCargoLabelToPrint.IncludeHouseBill;
						wrapper.DocIncludeCFSName = Globals.IsWeb || importCargoLabelToPrint.IncludeCFSName;
						wrapper.DocNumber = ++pieceCount;
						wrapper.DocNumberOfLabelsToPrint = Globals.IsWeb ? Shipment.JS_OuterPacks : importCargoLabelToPrint.NoOfLabelsToPrint;

						warppers.Add(wrapper);

						if (wrapper.DocNumberOfLabelsToPrint > 0 && pieceCount >= wrapper.DocNumberOfLabelsToPrint)
						{
							return warppers.ToArray();
						}
					}
				}
			}

			return null;
		}

		DocumentWrapper[] GetWrappersForInvoice(IStmMenuItem commandBeingRun, bool useDocBuilderInvoice)
		{
			DocumentWrapper[] result = null;

			if (commandBeingRun != null)
			{
				if (commandBeingRun.SU_ContactType == ContactType.Consignee.Code)
				{
					result = Shipment.GetWrappersForARInvoice(Shipment.Consignee, useDocBuilderInvoice);
				}
				else if (commandBeingRun.SU_ContactType == ContactType.Consignor.Code)
				{
					result = Shipment.GetWrappersForARInvoice(Shipment.Consignor, useDocBuilderInvoice);
				}
				else if (Shipment.Job != null)
				{
					if (commandBeingRun.SU_ContactType.IsEmpty ||
						commandBeingRun.SU_ContactType == ContactType.All.Code ||
						commandBeingRun.SU_ContactType == ContactType.NoContactType.Code ||
						commandBeingRun.SU_ContactType == ContactType.Receivables.Code)
					{
						result = Shipment.GetWrappersForARInvoice(Shipment.Job.LocalCharges, useDocBuilderInvoice);
					}
					else if (commandBeingRun.SU_ContactType == ContactType.ImportAirFreightAgent.Code ||
							commandBeingRun.SU_ContactType == ContactType.ImportSeaFreightAgent.Code ||
							commandBeingRun.SU_ContactType == ContactType.ExportAirFreightAgent.Code ||
							commandBeingRun.SU_ContactType == ContactType.ExportSeaFreightAgent.Code)
					{
						result = Shipment.GetWrappersForARInvoice(Shipment.Job.AgentCollect, useDocBuilderInvoice);
					}
				}
			}

			return result;
		}

		bool ContainerSelectionForDangerousGoods(CommonContainer container)
		{
			bool allowToPrint = false;
			foreach (PackLine line in container.PackLines)
			{
				if (line.JL_JS == this.PK)
				{
					if (line.UNDGs.Count > 0)
					{
						allowToPrint = true;
						break;
					}
				}
			}

			return allowToPrint;
		}

		DocumentWrapper[] GetGenericChargeSheetWrappers(DocumentDirection direction)
		{
			DocumentWrapper[] wrappers = null;

			if (direction == DocumentDirection.DEP)
			{
				DocumentShipment documentShipment = new DocumentShipment(Shipment, Core.Constants.DataContext.ChargeSheet);
				documentShipment.SetDefaultsFromDataContext();

				DebtorToSelectFromForPrinting[] debtorsToPrint = QueryProvider.GetDebtorsToPrint(documentShipment);

				if (debtorsToPrint != null && debtorsToPrint.Length > 0)
				{
					wrappers = debtorsToPrint.Select((debtor) => DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericChargeSheet, Shipment, debtor.Debtor)[0]).ToArray();
				}
			}
			else
			{
				wrappers = Shipment.Job != null ? DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericChargeSheet, Shipment, Shipment.Job.LocalCharges) :
					DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericChargeSheet, Shipment);
			}

			return wrappers;
		}

		protected override TitleCopyCountPair GetHBLDocumentTitle(ZString parentDocumentMenuName, IDocumentSupportable parentBusinessObject, IStmMenuTemplatePivot pivot)
		{
			TitleCopyCountPair result;

			if (IsPrintingHBL(parentDocumentMenuName))
			{
				result = base.GetHBLDocumentTitle(parentDocumentMenuName, parentBusinessObject, pivot);
			}
			else
			{
				var parentType = parentBusinessObject.GetType();
				var parentBusObjAsFC = parentBusinessObject as ForwardingConsol;
				ForwardingConsol consol = parentBusObjAsFC ?? ((parentType == typeof(DeliveryAgentOrgHeader)) ? ((DeliveryAgentOrgHeader)parentBusinessObject).Consol : null);
				if (((consol != null && PrintColoadsOnManifest(consol)) || Shipment.JS_JS_ColoadMasterShipment.IsEmpty)
					&& (pivot.SI_DocumentTitle.ToUpper() == "COPY" || pivot.SI_DocumentTitle.ToUpper().StartsWith("EMAIL", StringComparison.Ordinal) || pivot.SI_DocumentTitle.ToUpper().StartsWith("FAX", StringComparison.Ordinal)))
				{
					result = new TitleCopyCountPair(Shipment.JS_ReleaseType == Core.Constants.ShipmentReleaseTypes.ExpressBofL ? "EXPRESS" : "COPY");
				}
				else
				{
					result = new NothingToPrint();
				}
			}

			return result;
		}

		protected virtual bool PrintColoadsOnManifest(ForwardingConsol consol)
		{
			return consol.JK_PrintOptionForColoadsOnManifest != FreightConstants.PrintOptionForCoLoads.MastersOnly;
		}

		protected override TitleCopyCountPair GetHAWBDocumentTitle(ZString parentDocumentMenuName, IDocumentSupportable parentBusinessObject, IStmMenuTemplatePivot pivot)
		{
			TitleCopyCountPair result = null;

			if (parentDocumentMenuName.ToUpper().StartsWith((NoResString)"LASER HAWB", StringComparison.Ordinal)) // Document Name
			{
				result = base.GetHAWBDocumentTitle(parentDocumentMenuName, parentBusinessObject, pivot);
			}
			else
			{
				ForwardingConsol consol = parentBusinessObject as ForwardingConsol;
				if (((consol != null && PrintColoadsOnManifest(consol)) || Shipment.JS_JS_ColoadMasterShipment.IsEmpty) && IsCorrectPivotTypeForHAWBDocPacksFromConsols(pivot))
				{
					result = new TitleCopyCountPair(GetTitleForHAWBDocPacksFromConsols(pivot));
				}
				else
				{
					result = new NothingToPrint();
				}
			}

			return result;
		}

		public override TitleCopyCountPair GetDocumentTitlesForPivot(ZString parentDocumentMenuName, IDocumentSupportable parentBusinessObject, IStmMenuTemplatePivot pivot)
		{
			if (parentBusinessObject is ICusEntryHeader)
			{
				var entryHeaderObject = (BusinessObject)parentBusinessObject;
				var mrn = (ZString)entryHeaderObject["MovementReferenceNumber"];
				var code = mrn.IsEmpty ? entryHeaderObject["CH_BGMReference"] : mrn;
				return new TitleCopyCountPair(parentDocumentMenuName + " - " + code, 1);
			}
			else if (parentBusinessObject is ES.IESDocC10Header)
			{
				var doc10Header = (BusinessObject)parentBusinessObject;
				var entryHeaderObject = (BusinessObject)doc10Header["entryHeader"];
				var mrn = (ZString)entryHeaderObject["MovementReferenceNumber"];
				var code = mrn.IsEmpty ? entryHeaderObject["CH_BGMReference"] : mrn;
				return new TitleCopyCountPair(parentDocumentMenuName + " - " + code, 1);
			}
			return base.GetDocumentTitlesForPivot(parentDocumentMenuName, parentBusinessObject, pivot);
		}

		protected override List<DocumentSupporter> AdditionalBODataSourceDocumentSupporters
		{
			get
			{
				var declaration = (IDocumentSupportable)Shipment.DeclarationForDocuments ?? (IDocumentSupportable)Factory.GetNull(ObjectFactory.GetType<IBaseJobDeclaration>());
				var nctsHeader = (IDocumentSupportable)Shipment.NctsHeaderForDocuments ?? (IDocumentSupportable)Factory.GetNull(ObjectFactory.GetType<EU.NCTS.ICusInBondHeader>());
				return new List<DocumentSupporter>() { declaration.DocumentSupporter, nctsHeader.DocumentSupporter };
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
		protected override IDocumentEventsHandler[] GetDocumentEventsHandlers()
		{
			var result = new List<IDocumentEventsHandler>(base.GetDocumentEventsHandlers());
			result.Add(new ForwardingShipmentCartageAdviceDocumentEventsHandler() { DocumentSupporter = this });
			result.Add(new AWBDocumentEventsHandler() { DocumentSupporter = this });

			if (Shipment.DeclarationForDocuments != null)
			{
				var documentSupportable = Shipment.DeclarationForDocuments as IDocumentSupportable
					?? throw new ArgumentNullException("documentSupportable is null since Shipment.DeclarationForDocuments cannot be cast as IDocumentSupportable");

				var documentSupporter = documentSupportable.DocumentSupporter
					?? throw new ArgumentNullException("Shipment.DeclarationForDocuments.DocumentSupporter cannot be null");

				result.AddRange(documentSupporter.DocumentEventsHandlers);
			}

			if (QueryProvider.PrintAWBBarcodeLabel())
			{
				var awbBarcodeLabelDocumentEventsHandler = ObjectFactory.Get<IDocumentEventsHandler>("IDocumentEventsHandler.AWBBarcodeLabel");
				awbBarcodeLabelDocumentEventsHandler.DocumentSupporter = this;
				result.Add(awbBarcodeLabelDocumentEventsHandler);
			}

			return result.ToArray();
		}

		#region DocumentPrinted

		protected override void DocumentEventSource_DocumentPrinted(object sender, DocumentPrintedEventArgs e)
		{
			base.DocumentEventSource_DocumentPrinted(sender, e);

			if (WasPrinted(e.DeliveryInstructionDestinationType)
				&& !e.IsDraft
				&& Shipment != null
				&& Shipment.JS_HouseBillIssueDate.IsEmpty
				&& IsPrintingHAWB(e.MenuItem.SU_MenuName))
			{
				SetShipmentIssueDate();
			}
		}

		void SetShipmentIssueDate()
		{
			if (FreightConfigurationRegistry.Instance.DefaultShipmentIssueDateFromHAWB.Value
				&& Shipment.AWBHeader != null)
			{
				Shipment.JS_HouseBillIssueDate = Shipment.AWBHeader.EH_AWBIssueDate;
			}
			else
			{
				Shipment.JS_HouseBillIssueDate = ZDateTime.Today;
			}

			try
			{
				if (Globals.IsUserInteractive)
				{
					Shipment.Factory.Save();
				}
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		#endregion

		#region DocumentEventsHandlers

		class AWBDocumentEventsHandler : IDocumentEventsHandler
		{
			public bool CanHandleMenuItem(IStmMenuItem menuItem)
			{
				return menuItem.SU_MenuName.Contains("AWB", StringComparison.Ordinal);
			}

			public void HandleDocumentPrePreviewed(object sender, DocumentPrintedEventArgs e)
			{
			}

			public void HandleDocumentPrePrinted(object sender, DocumentPrintedEventArgs e)
			{
			}

			public void HandleDocumentPrintRequested(object sender, DocumentCancelEventArgs e)
			{
				var forwardingShipmentDocumentSupporter = DocumentSupporter as ForwardingShipmentDocumentSupporter;
				if (forwardingShipmentDocumentSupporter != null)
				{
					forwardingShipmentDocumentSupporter.Shipment.PopulateAWBForDocuments();
					forwardingShipmentDocumentSupporter.Shipment.AWBForDocumentsPopulated = false;
				}
			}

			public void HandleDocumentPrinted(object sender, DocumentPrintedEventArgs e)
			{
				var forwardingShipmentDocumentSupporter = DocumentSupporter as ForwardingShipmentDocumentSupporter;
				if (forwardingShipmentDocumentSupporter != null)
				{
					forwardingShipmentDocumentSupporter.Shipment.AWBForDocumentsPopulated = false;
				}
			}

			public DocumentSupporter DocumentSupporter { get; set; }
		}

		#endregion

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Menu name")]
		public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			DocumentSupporterDataState result = base.GetDataStateBeforeRun(commandAboutToBeRun);

			if (result.IsValid && commandAboutToBeRun != null && (commandAboutToBeRun.SU_MenuName.Equals("AWB Barcode Label")
				|| commandAboutToBeRun.SU_MenuName.Equals("Neutral HAWB") || commandAboutToBeRun.SU_MenuName.Equals("Laser HAWB with Follow on Page")))
			{
				var jobShipmentModuleId = ModuleIDs.JobShipment;

				using (var module = ObjectFactory.Get<IModuleFactory>().Create(jobShipmentModuleId))
				{
					if (module != null && module.SecurityCheckpoint != Env.Security.None)
					{
						var parentSecurityCheckpoint = Env.Security.FindOrCreateDocumentsCheckpoint(jobShipmentModuleId, module.SecurityCheckpoint);

						var printFinalMasterCheckpoint = Env.Security.FindOrCreateDocumentCheckpoint(
							commandAboutToBeRun.PK.ToGuid(),
							commandAboutToBeRun.SU_MenuNameMultilingual,
							ModuleIDs.JobShipment,
							parentSecurityCheckpoint);

						if (!printFinalMasterCheckpoint.IsAllowed)
						{
							result = new DocumentSupporterDataState();
							result.IsValid = false;
							result.ErrorMessage = printFinalMasterCheckpoint.ErrorMessageForNotAllowed;
						}
					}
				}
			}
			else if (result.IsValid && commandAboutToBeRun != null)
			{
				var declarationForDocuments = Shipment.DeclarationForDocuments;
				if (commandAboutToBeRun.SU_MenuName.Contains("NCH1", StringComparison.OrdinalIgnoreCase)) // Menu Name
				{
					result = new DocumentSupporterDataState();
					var cannotPrintReason = GetReasonCannotPrintNch1DocumentGbCustoms(declarationForDocuments);
					if (cannotPrintReason.IsEmpty)
					{
						result.IsValid = true;
					}
					else
					{
						result.IsValid = false;
						result.ErrorMessage = cannotPrintReason;
					}
				}

				if (result.IsValid && declarationForDocuments is TW.IJobDeclaration declaration)
				{
					var documentOptionsToPrint = ObjectFactory.Get<TW.IDocumentOptionsToPrint>();
					result.IsValid = documentOptionsToPrint.GetDocumentOptionsToPrintCustomsDeclaration(declaration, commandAboutToBeRun.Documents);
				}
			}
			return result;
		}

		ZString GetReasonCannotPrintNch1DocumentGbCustoms(IBaseJobDeclaration declarationForDocuments)
		{
			if (declarationForDocuments != null && declarationForDocuments is GB.IJobDeclaration)
			{
				var gbDeclaration = (GB.IJobDeclaration)(Shipment.DeclarationForDocuments);
				return gbDeclaration.ReasonWhyCannotPrintNch1Document;
			}
			return "";
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			switch (dataContextValue.DataContext)
			{
				case Core.Constants.DataContext.AWB:
					{
						if (Shipment.AWBHeader == null || (commandBeingRun.SU_MenuName == AWBActions.DocumentNames.AWBBarcodeLabels && !QueryProvider.PrintAWBBarcodeLabel()))
						{
							return Res.GetString("08fe5eec-6c0d-4aa8-a6a4-7c6d8b75a2f6", "This document requires AWB data.");
						}

						break;
					}

				case Core.Constants.DataContext.Container:
					{
						if (!Shipment.Containers.Any())
						{
							return Res.GetString("c7d4b40a-611d-4d8c-8c6c-84c1c59ca8a8", "This document requires container data. Ensure the related Consol has containers.");
						}

						break;
					}

				case Core.Constants.DataContext.GenericFreightJobRouting:
					{
						if (!Shipment.TransportsIncludingRelated.Any())
						{
							return Res.GetString("3fb63c4e-e0d5-463d-a652-e7983acf4608", "This shipment does not have any Transport information entered on the Routing tab.");
						}

						break;
					}

				case Core.Constants.DataContext.GenericFreightJobServices:
					{
						if (!Shipment.Services.Any())
						{
							return Res.GetString("b2ae1ec7-c429-49b3-8f44-d5b5cc7afa5e", "This shipment does not have any Services information entered within the Additional Details > Services grid.");
						}

						break;
					}

				case Core.Constants.DataContext.ARInvoice:
					{
						var message = GetProperMessageForARInvoice(commandBeingRun);
						if (!string.IsNullOrWhiteSpace(message))
						{
							return message;
						}

						break;
					}

				case Core.Constants.DataContext.GenericFreightJobByPackages:
				case Core.Constants.DataContext.GenericFreightJobByPackages1Doc:
				case Core.Constants.DataContext.GenericFreightJobBySelectedPackages:
				case Core.Constants.DataContext.GenericFreightJobBySelectedPkgs1Doc:
				case Core.Constants.DataContext.GenericImportCargoLabel:
					{
						if (!Shipment.OuterPackLines.Any())
						{
							return Res.GetString("4bd7b634-dbe4-47d3-b3ed-c29f33997933", "This shipment doest not have any packlines.");
						}

						break;
					}

				case Core.Constants.DataContext.ChargeSheet:
				case Core.Constants.DataContext.GenericChargeSheet:
					{
						var menuItem = commandBeingRun as StmMenuItem;
						if (menuItem != null && menuItem.GetDocumentDirection() == DocumentDirection.DEP)
						{
							return Res.GetString("08c92841-2750-4352-b2ab-5fd0234456be", "This document requires a debtor to be entered within the Billing tab.");
						}

						break;
					}

				case Core.Constants.DataContext.Declaration:
				case Core.Constants.DataContext.NZCustoms:
				case Core.Constants.DataContext.SGPrintPermit:
				case Core.Constants.DataContext.SGRefundInfo:
				case Core.Constants.DataContext.DeclarationWithCusEntryHeaders:
				case Core.Constants.DataContext.CommercialInvoice:
				case Core.Constants.DataContext.GenericFreightJobInvoice:
				case Core.Constants.DataContext.CusEntryHeader:
				case Core.Constants.DataContext.ATD:
				case Core.Constants.DataContext.EFTPaymentAdvice:
				case Core.Constants.DataContext.ComInvoiceHeader:
				case Core.Constants.DataContext.LandedCostEntryHeaders:
				case Core.Constants.DataContext.LandedCostHeader:
				case Core.Constants.DataContext.SADH:
				case Core.Constants.DataContext.ESSADH:
				case Core.Constants.DataContext.FRSADH:
				case Core.Constants.DataContext.LiquidationDetails:
				case Core.Constants.DataContext.GbTaxEstimator:
				case Core.Constants.DataContext.GenericFreightJobByComInv:
					{
						if (Shipment.DeclarationForDocuments == null)
						{
							return Res.GetString("c3a8ea61-6e96-4958-82ce-d629e70d052e", "This document is only available when this shipment has a Brokerage job.");
						}
						else
						{
							var documentSupportable = Shipment.DeclarationForDocuments as IDocumentSupportable;
							if (documentSupportable != null && documentSupportable.DocumentSupporter != null)
							{
								return documentSupportable.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
							}
						}

						break;
					}

				case Core.Constants.DataContext.GbCcsuk:
					{
						if (Shipment.GBCusHAWBs != null && Shipment.GBCusHAWBs.Count > 0)
						{
							var lastHawb = (BusinessObject)Shipment.GBCusHAWBs.Last();
							var hawb = Factory.Load<GB.CCSUK.ICusHAWB>(lastHawb.PK);
							if (hawb is IDocumentSupportable documentSupportable && documentSupportable.DocumentSupporter != null)
							{
								return documentSupportable.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
							}
						}
						else
						{
							return Res.GetString("fdfdde1f-5f10-4ead-bf46-f5fc29587b73", "This shipment does not have any HAWB data about GB Customs.");
						}

						break;
					}
			}

			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		ZString GetProperMessageForARInvoice(IStmMenuItem commandBeingRun)
		{
			if (commandBeingRun != null)
			{
				if (commandBeingRun.SU_ContactType == ContactType.Consignee.Code)
				{
					if (Shipment.Consignee == null)
					{
						return Res.GetString("6e061231-0cfb-415f-94a5-9288f5331d60", "This shipment does not have a Consignee.");
					}
				}
				else if (commandBeingRun.SU_ContactType == ContactType.Consignor.Code)
				{
					if (Shipment.Consignor == null)
					{
						return Res.GetString("71483a94-cef2-4b25-b77b-cde45de23a70", "This shipment does not have a Consignor.");
					}
				}
				else if (Shipment.Job != null)
				{
					if (commandBeingRun.SU_ContactType.IsEmpty
						|| commandBeingRun.SU_ContactType == ContactType.All.Code
						|| commandBeingRun.SU_ContactType == ContactType.NoContactType.Code
						|| commandBeingRun.SU_ContactType == ContactType.Receivables.Code)
					{
						if (Shipment.Job.LocalCharges == null)
						{
							return Res.GetString("d4d01b11-e900-43f2-bb71-0ce38a7fe5b7", "This shipment does not have a Local Client on the Billing tab page.");
						}
					}
					else if (commandBeingRun.SU_ContactType == ContactType.ImportAirFreightAgent.Code
						|| commandBeingRun.SU_ContactType == ContactType.ImportSeaFreightAgent.Code
						|| commandBeingRun.SU_ContactType == ContactType.ExportAirFreightAgent.Code
						|| commandBeingRun.SU_ContactType == ContactType.ExportSeaFreightAgent.Code)
					{
						if (Shipment.Job.AgentCollect == null)
						{
							return Res.GetString("dc589514-3694-49fc-84dd-a6d20313d273", "This shipment does not have a Overseas Agent on the Billing tab page.");
						}
					}
				}
			}

			return ZString.Empty;
		}
	}
}
