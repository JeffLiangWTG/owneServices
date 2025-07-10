using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class BaseJobDeclarationDocumentSupporter : DocumentSupporter
	{
		public BaseJobDeclarationDocumentSupporter(BaseJobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		protected BaseJobDeclaration BaseJobDeclaration
		{
			get { return (BaseJobDeclaration)BusinessObject; }
		}

		protected virtual ZString MessageForInvaildEntryPrintDataState(IStmMenuItem menuItem)
		{
			return Res.GetString("93e6aa9e-5555-49d0-b433-0a06dc5336f7", "Cannot print {0} as the declaration has not been Merged", menuItem.SU_MenuName);
		}

		protected override void InitialiseCore(IDocumentEvents documentEventSource)
		{
			base.InitialiseCore(documentEventSource);
			documentEventSource.DocumentPrinted += new DocumentPrintedEventHandler(DocumentEventSource_DocumentPrinted);

			bookingDeliveryCancelled = false;
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new DataContext[]
				{
					DataContext.Declaration,
					DataContext.Notes,
					DataContext.CommercialInvoice,
					DataContext.ChargeSheet,
					DataContext.ComInvoiceHeader,
					DataContext.PreAlert,
					DataContext.CusEntryHeader,
					DataContext.CusEntryHeaderENS,
					DataContext.DeclarationWithCusEntryHeaders,
					DataContext.ShipperDepartureNotice,
					DataContext.LandedCostHeader,
					DataContext.LandedCostEntryHeaders,
					DataContext.ShipmentDeclaration,
					DataContext.CartageAdvice,
					DataContext.CombinedCartageAdvice,
					DataContext.RequestForMissingDocuments,
					DataContext.Worksheet,
					DataContext.RequestForService,
					DataContext.Service,
					DataContext.TimeSlotRequest,
					DataContext.GenericFreightJob,
					DataContext.GenericCommercialInvoice,
					DataContext.GenericFreightJobByContainerIfFCL,
					DataContext.ARInvoice,
					DataContext.GenericFreightJobByComInv,
					DataContext.GenericFreightJobInvoice
				};
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Customs; }
		}

		public override BusinessContext[] SupportedChildBusinessContexts
		{
			get { return new[] { BusinessContext.Shipment, BusinessContext.Customs, BusinessContext.DtbBooking, BusinessContext.CusEntryHeader }; }
		}

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menu, BusinessContext businessContext, IStmMenuItem childCommandBeingRun)
		{
			IDocumentSupportable[] result = null;

			switch (businessContext)
			{
				case BusinessContext.Shipment:
					result = Array.Empty<IDocumentSupportable>();
					break;
				case BusinessContext.Customs:
					result = new IDocumentSupportable[] { BaseJobDeclaration };
					break;
				case BusinessContext.DtbBooking:
					bookings = TransportBookingLoader.GetBookingsToDeliver(BaseJobDeclaration, menu);
					bookingDeliveryCancelled = !bookings.Any();
					result = bookings.Cast<IDocumentSupportable>().ToArray();
					break;
				case BusinessContext.CusEntryHeader:
					var entryHeaders = BaseJobDeclaration.CustomsEntryHeaders;
					result = entryHeaders.Cast<IDocumentSupportable>().ToArray();
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
					var factory = BaseJobDeclaration.Factory;
					var reloadedBooking = (IDocumentSupportable)factory.Load<IDtbBooking>(b.PK);
					if (reloadedBooking == null)
					{
						ErrorReporter.ReportOnce("BaseJobDeclarationDocumentSupporter_NullBooking", $"Booking {b.HumanReadableName} from factory '{b.Factory.NameForDebugging}' could not be loaded by factory '{factory.NameForDebugging}' belonging to {BaseJobDeclaration.HumanReadableName}");
					}
					return reloadedBooking;
				}).Where(b => b != null).ToArray();
			}
		}
		IEnumerable<IDtbBooking> bookings = Array.Empty<IDtbBooking>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			switch (dataContextValue.DataContext)
			{
				case DataContext.CommercialInvoice:
				case DataContext.ComInvoiceHeader:
				case DataContext.GenericCommercialInvoice:
				case DataContext.GenericFreightJob:
				case DataContext.GenericFreightJobByComInv:
					return Res.GetString("9AF4144D-F12D-4BE7-88E9-5EFD577DC5BF", "Invoice Header cannot be found.");
				case DataContext.LandedCostHeader:
				case DataContext.LandedCostEntryHeaders:
					return Res.GetString("9E76C2C0-99F9-4D27-9E92-2D97A0AE5FD4", "Landed Costing Header cannot be found.");
				case DataContext.CusEntryHeader:
				case DataContext.CusEntryHeaderENS:
					return Res.GetString("00873B39-98CE-4B31-8334-F9817C4C295E", "Entry Header cannot be found.");
				case DataContext.DeclarationWithCusEntryHeaders:
					return DocDataProvidersNotFoundMessageForDeclarationWithCusEntryHeaders;
				case DataContext.GenericFreightJobByContainerIfFCL:
					return Res.GetString("8219518C-F2B6-4D1F-8261-96A0226AF4F9", "There is no container data associated with this declaration.");
				case DataContext.GenericFreightJobInvoice:
				case DataContext.ARInvoice:
					return GetProperMessageForARInvoice(commandBeingRun, BaseJobDeclaration);
			}
			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		protected virtual ZString DocDataProvidersNotFoundMessageForDeclarationWithCusEntryHeaders
		{
			get { return Res.GetString("ED0C548F-6AB6-4571-B67F-4C42CCEAB55F", "Entry Header cannot be found."); }
		}

		ZString GetProperMessageForARInvoice(IStmMenuItem commandBeingRun, BaseJobDeclaration declaration)
		{
			var result = ZString.Empty;
			if (commandBeingRun != null)
			{
				if (commandBeingRun.SU_ContactType == ContactType.Consignee.Code)
				{
					if (declaration.Consignee == null)
					{
						result = Res.GetString("C5F99561-53BE-4E70-B338-F6C1B27D9681", "This declaration does not have a Consignee.");
					}
				}
				else if (commandBeingRun.SU_ContactType == ContactType.Consignor.Code)
				{
					if (declaration.Consignor == null)
					{
						result = Res.GetString("C64C2301-08EE-4550-828A-76D757319019", "This declaration does not have a Consignor.");
					}
				}
				else if (declaration.Job != null)
				{
					if (commandBeingRun.SU_ContactType.IsEmpty || commandBeingRun.SU_ContactType == ContactType.All.Code || commandBeingRun.SU_ContactType == ContactType.NoContactType.Code || commandBeingRun.SU_ContactType == ContactType.Receivables.Code)
					{
						if (declaration.Job.LocalCharges == null)
						{
							result = Res.GetString("2BFB9A22-6DC0-4BDE-A73A-12E4F89D738D", "This declaration does not have a Local Client on the Billing tab page.");
						}
					}
					else if (commandBeingRun.SU_ContactType == ContactType.ExportBroker.Code || commandBeingRun.SU_ContactType == ContactType.ImportBroker.Code)
					{
						if (declaration.Job.AgentCollect == null)
						{
							result = Res.GetString("579921C0-17AB-4263-9978-ED6F0C3AA2F7", "This shipment does not have a Overseas Agent on the Billing tab page.");
						}
					}
				}
			}
			return result;
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return !bookingDeliveryCancelled
				&& dataContext != Core.Constants.DataContext.PreAlert
				&& dataContext != Core.Constants.DataContext.CartageAdvice
				&& dataContext != Core.Constants.DataContext.RequestForService
				&& dataContext != Core.Constants.DataContext.Declaration
				&& dataContext != Core.Constants.DataContext.Notes
				&& dataContext != Core.Constants.DataContext.ChargeSheet
				&& dataContext != Core.Constants.DataContext.ShipperDepartureNotice
				&& dataContext != Core.Constants.DataContext.ShipmentDeclaration
				&& dataContext != Core.Constants.DataContext.CombinedCartageAdvice
				&& dataContext != Core.Constants.DataContext.RequestForMissingDocuments
				&& dataContext != Core.Constants.DataContext.Worksheet
				&& dataContext != Core.Constants.DataContext.Service
				&& dataContext != Core.Constants.DataContext.TimeSlotRequest
				&& base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		bool bookingDeliveryCancelled;

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Core.Constants.DataContext.GenericFreightJobInvoice)
			{
				return JobDeclarationDocumentSupporterHelper.GetWrappersForInvoice(commandBeingRun, true, BaseJobDeclaration);
			}

			if (dataContext == Core.Constants.DataContext.ARInvoice)
			{
				return JobDeclarationDocumentSupporterHelper.GetWrappersForInvoice(commandBeingRun, false, BaseJobDeclaration);
			}

			if (dataContext == DataContext.GenericFreightJobByComInv)
			{
				List<DocumentWrapper> invoiceWrappers = new List<DocumentWrapper>();
				foreach (BaseJobComInvoiceHeader invoiceHeader in BaseJobDeclaration.Invoices)
				{
					DocumentWrapper[] wrappers = DocumentWrapperFactory.GenerateGenericWrappers(DataContext.GenericFreightJob, BaseJobDeclaration, invoiceHeader);
					invoiceWrappers.AddRange(wrappers);
				}

				return invoiceWrappers.ToArray();
			}

			if (dataContext == DataContext.GenericFreightJobByContainerIfFCL)
			{
				return GetWrappersForGenericFreightJobByContainerIfFCL();
			}

			if (dataContext == DataContext.GenericFreightJobServices)
			{
				return BaseJobDeclaration.DocsAndCartage.GetServiceWrappersForDocBuilder(((IHaveServices)BaseJobDeclaration.DocsAndCartage).ServiceParent, Constants.DataContext.GenericFreightJob);
			}

			DocumentWrapper[] result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);

			if (result == null)
			{
				switch (dataContext)
				{
					case DataContext.Declaration:
					case DataContext.Notes:
					case DataContext.ChargeSheet:
					case DataContext.PreAlert:
					case DataContext.ShipperDepartureNotice:
					case DataContext.LandedCostEntryHeaders:
					case DataContext.ShipmentDeclaration:
					case DataContext.CombinedCartageAdvice:
					case DataContext.RequestForMissingDocuments:
					case DataContext.Worksheet:
					case DataContext.Service:
					case DataContext.TimeSlotRequest:
						DocumentWrapper declarationWrapper = DocumentWrapperFactory.CreateCustomsWrapper(Constants.DataContext.Declaration, BaseJobDeclaration, BaseJobDeclaration.Country.Code);
						if (declarationWrapper != null)
						{
							result = new DocumentWrapper[] { declarationWrapper };
						}
						break;
					case DataContext.CommercialInvoice:
					case DataContext.ComInvoiceHeader:
						List<DocumentWrapper> list = new List<DocumentWrapper>();
						for (int i = 0; i < BaseJobDeclaration.Invoices.Count; i++)
						{
							DocumentWrapper invoiceHeaderWrapper = DocumentWrapperFactory.CreateCustomsWrapper(Constants.DataContext.JobComInvoiceHeader, BaseJobDeclaration.Invoices[i], BaseJobDeclaration.Country.Code);
							if (invoiceHeaderWrapper != null)
							{
								list.Add(invoiceHeaderWrapper);
							}
						}
						result = list.ToArray();
						break;
					case DataContext.LandedCostHeader:
						IDocumentSupportable landedCostHeaderForDocumentsCached = LandedCostHeaderForDocuments;
						if (landedCostHeaderForDocumentsCached != null)
						{
							return landedCostHeaderForDocumentsCached.DocumentSupporter.GetDocumentWrappers(dataContext, commandBeingRun);
						}
						break;
					case DataContext.CartageAdvice:
						result = GetWrappersForCartageAdvice();
						break;
					case DataContext.RequestForService:
						result = GetServiceWrappers();
						break;
					case DataContext.SystemSectionRepository:
						result = DocumentWrapperFactory.GenerateGenericWrappers(Constants.DataContext.GenericFreightJob, BusinessObject);
						break;
				}
			}
			return result;
		}

		#region GetDocWrappers Methods

		DocumentWrapper[] GetWrappersForCartageAdvice()
		{
			DocumentWrapper[] result = null;

			if (BaseJobDeclaration.ShouldDefaultFCLCartageCo)
			{
				DocumentCusContainerCollection documentCusContainerCollection = new DocumentCusContainerCollection(Factory, BaseJobDeclaration.CusContainers);
				DocumentCusContainerCollectionHeader documentCusContainerCollectionHeader = new DocumentCusContainerCollectionHeader(documentCusContainerCollection);

				CusContainersToPrintEventArgs cusContainersEventArg = new CusContainersToPrintEventArgs(documentCusContainerCollectionHeader);

				BaseJobDeclaration.RaiseOnGetCusContainersToPrint(cusContainersEventArg);

				if (cusContainersEventArg.ContinueToPrint)
				{
					int containersToPrintCount = 0;
					foreach (DocumentCusContainer documentCusContainer in cusContainersEventArg.DocumentCusContainerCollectionHeader.DocumentCusContainerCollection)
					{
						if (documentCusContainer.PrintContainer)
						{
							containersToPrintCount++;
						}
					}
					List<DocumentWrapper> list = new List<DocumentWrapper>();
					foreach (DocumentCusContainer documentCusContainer in cusContainersEventArg.DocumentCusContainerCollectionHeader.DocumentCusContainerCollection)
					{
						if (documentCusContainer.PrintContainer)
						{
							DocumentWrapper cusContainerWrapper = DocumentWrapperFactory.CreateCustomsContainerWrapperWithDeclaration(documentCusContainer.Container, BaseJobDeclaration, BaseJobDeclaration.CountryCode);
							if (cusContainerWrapper != null)
							{
								list.Add(cusContainerWrapper);
							}
						}
					}
					result = list.ToArray();
				}
			}
			else
			{
				DocumentWrapper jobDeclarationwrapper = DocumentWrapperFactory.CreateCustomsWrapper(Core.Constants.DataContext.Declaration, BaseJobDeclaration, BaseJobDeclaration.CountryCode);
				if (jobDeclarationwrapper != null)
				{
					result = new DocumentWrapper[] { jobDeclarationwrapper };
				}
			}
			return result;
		}

		DocumentWrapper[] GetWrappersForGenericFreightJobByContainerIfFCL()
		{
			DocumentWrapper[] result = null;

			if (BaseJobDeclaration.ShouldDefaultFCLCartageCo)
			{
				DocumentCusContainerCollection documentCusContainerCollection = new DocumentCusContainerCollection(Factory, BaseJobDeclaration.CusContainers);
				DocumentCusContainerCollectionHeader documentCusContainerCollectionHeader = new DocumentCusContainerCollectionHeader(documentCusContainerCollection);

				CusContainersToPrintEventArgs cusContainersEventArg = new CusContainersToPrintEventArgs(documentCusContainerCollectionHeader);

				BaseJobDeclaration.RaiseOnGetCusContainersToPrint(cusContainersEventArg);

				if (cusContainersEventArg.ContinueToPrint)
				{
					int containersToPrintCount = 0;
					foreach (DocumentCusContainer documentCusContainer in cusContainersEventArg.DocumentCusContainerCollectionHeader.DocumentCusContainerCollection)
					{
						if (documentCusContainer.PrintContainer)
						{
							containersToPrintCount++;
						}
					}
					List<DocumentWrapper> list = new List<DocumentWrapper>();
					foreach (DocumentCusContainer documentCusContainer in cusContainersEventArg.DocumentCusContainerCollectionHeader.DocumentCusContainerCollection)
					{
						if (documentCusContainer.PrintContainer)
						{
							DocumentWrapper[] containerWrappers = DocumentWrapperFactory.GenerateGenericWrappers(DataContext.GenericFreightJob, BaseJobDeclaration, documentCusContainer.Container);
							if (containerWrappers != null)
							{
								foreach (DocumentWrapper containerWrapper in containerWrappers)
								{
									list.Add(containerWrapper);
								}
							}
						}
					}
					result = list.ToArray();
				}
			}
			else
			{
				result = DocumentWrapperFactory.GenerateGenericWrappers(DataContext.GenericFreightJob, BaseJobDeclaration);
			}
			return result;
		}

		DocumentWrapper parentServiceWrapper;
		DocumentWrapper GetServiceWrapper(JobService service)
		{
			return DocumentWrapperFactory.CreateServiceWrapperWithParent(service, parentServiceWrapper ??
				(parentServiceWrapper = DocumentWrapperFactory.CreateCustomsWrapper(Core.Constants.DataContext.Declaration, BaseJobDeclaration, BaseJobDeclaration.CountryCode)));
		}

		protected DocumentWrapper[] GetServiceWrappers()
		{
			parentServiceWrapper = null;
			return BaseJobDeclaration.DocsAndCartage.GetServiceWrappers(GetServiceWrapper);
		}

		#endregion

		public override string GetMenuTemplateFilterValue(MenuTemplateFilterType filterType, IBODocDataProvider docWrapperForCurrentPivot)
		{
			if (filterType == MenuTemplateFilterType.AUCountry)
			{
				return AUCountryMenuTemplateFilterValue;
			}

			return base.GetMenuTemplateFilterValue(filterType, docWrapperForCurrentPivot);
		}

		protected override IDocumentEventsHandler[] GetDocumentEventsHandlers()
		{
			var result = new List<IDocumentEventsHandler>(base.GetDocumentEventsHandlers());
			result.Add(new BaseJobDeclarationCartageAdviceDocumentEventsHandler() { DocumentSupporter = this });
			return result.ToArray();
		}

		protected virtual string AUCountryMenuTemplateFilterValue
		{
			get { return (NoResString)"No"; }
		}

		protected virtual IEnumerable<ZString> HideFilterCodeList => Array.Empty<ZString>();

		protected virtual bool HideFilter() => false;

		public override string GetFilterValue(DocumentFilters filterName)
		{
			if (HideFilter())
			{
				return null;
			}

			var countryCode = BaseJobDeclaration.CountryCode;

			return filterName switch
			{
				DocumentFilters.MOD => BaseJobDeclaration.JE_TransportMode,
				DocumentFilters.MSC =>
					(BaseJobDeclaration.JE_MessageType == Customs.Business.JobMessageTypeList.Codes.Export || BaseJobDeclaration.JE_MessageType == Customs.Business.JobMessageTypeList.Codes.Import)
						? "EXPIMP"
						: "OTH",
				DocumentFilters.LGR =>
					(BaseJobDeclaration.Job != null || BaseJobDeclarationDocumentSupporterHelper.GetJobHeaderForeignKeyLink(BaseJobDeclaration) != null)
						? "INV"
						: "",
				DocumentFilters.CTY => countryCode,
				DocumentFilters.BKRCTY => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode),
				DocumentFilters.MSGBKR => BaseJobDeclaration.MessageTypeForDocumentFilter,
				DocumentFilters.MSGBKRCTY => BaseJobDeclaration.MessageTypeForDocumentFilter + Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode),
				DocumentFilters.MSGBKRCTYMOD => BaseJobDeclaration.MessageTypeForDocumentFilter + Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode) + BaseJobDeclaration.JE_TransportMode,
				DocumentFilters.MSGBKRCTYAPP => BaseJobDeclaration.MessageTypeForDocumentFilter + Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode) + BaseJobDeclaration.JE_ApplicationCode,
				DocumentFilters.MSGBKRLCO => BaseJobDeclaration.MessageTypeForDocumentFilter + ((ILandedCostHeader)BaseJobDeclaration).LandedCostType,
				DocumentFilters.EXPBKRLIC
					=> (BaseJobDeclaration.IsExport)
						? "Y"
						: "N",
				DocumentFilters.BKRCTYAPP => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode) + BaseJobDeclaration.JE_ApplicationCode,
				DocumentFilters.ISINTEGRATED
					=> BaseJobDeclaration.IsDeclarationIntegrated
						? "Y"
						: "N",
				DocumentFilters.ASYCUDA
					=> ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry(countryCode)
						? "Y"
						: "N",
				DocumentFilters.HIDE => "Y",
				_ => base.GetFilterValue(filterName),
			};
		}

		public sealed override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			if (!hasLoadedChildEditableObjects)
			{
				hasLoadedChildEditableObjects = true;
				BaseJobDeclaration.LoadChildEditableObjects();
			}
			return GetDataStateBeforeRunCore(commandAboutToBeRun);
		}
		bool hasLoadedChildEditableObjects;

		protected virtual DocumentSupporterDataState GetDataStateBeforeRunCore(IStmMenuItem commandAboutToBeRun)
		{
			var result = base.GetDataStateBeforeRun(commandAboutToBeRun);
			if (result.IsValid && commandAboutToBeRun != null)
			{
				if (BaseJobDeclaration.CustomsEntryHeaders.Count == 0 && commandAboutToBeRun.Documents.OfType<IStmMenuTemplatePivot>().Any(x => (x.Template?.SO_DataContext ?? ZString.Empty).Contains(CusEntryHeaderSchema.Constants.TableName, StringComparison.OrdinalIgnoreCase)))
				{
					result = new DocumentSupporterDataState(false, MessageForInvaildEntryPrintDataState(commandAboutToBeRun));
				}
				else if (commandAboutToBeRun.SU_MenuName.StartsWith(LandedCostingMenuText) && commandAboutToBeRun.SU_MenuName != LandedCostingOldMenuText)
				{
					IDocumentSupportable landedCostHeaderForDocumentsCached = LandedCostHeaderForDocuments;
					if (landedCostHeaderForDocumentsCached != null)
					{
						result = landedCostHeaderForDocumentsCached.DocumentSupporter.GetDataStateBeforeRun(commandAboutToBeRun);
					}
					else
					{
						result = new DocumentSupporterDataState(false, LandedCostingErrorMessage);
					}
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public const string LandedCostingMenuText = "Landed Costing";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public const string LandedCostingOldMenuText = "Landed Costing - Old";

		public static string LandedCostingErrorMessage
		{
			get { return Res.GetString("f25339b7-75a0-4ee6-92a8-bf64f918aab5", "Please create a Landed Costing job first. Please click 'Landed Costing' tab to create."); }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.CustomsDeclarationCustomiseDocument; }
		}

		public override string TransportMode
		{
			get
			{
				var transportMode = BaseJobDeclaration.JE_TransportMode;

				switch (transportMode)
				{
					case Constants.TransportModes.Air:
					case Constants.TransportModes.Courier:
					case Constants.TransportModes.Sea:
					case Constants.TransportModes.Rail:
					case Constants.TransportModes.Road:
					case "":
						return transportMode;
					default:
						return Core.Constants.TransportModes.Other;
				}
			}
		}

		public override string ContainerMode
		{
			get
			{
				return (BaseJobDeclaration.JE_TransportMode == Constants.TransportModes.Sea)
					? BaseJobDeclaration.JE_ContainerMode.ToString()
					: string.Empty;
			}
		}

		public override string LocalPort(IContactType contact, DocumentDirection direction)
		{
			string result = "";
			if (contact == ContactType.Consignee || contact == ContactType.ImportFreightAgent)
			{
				result = BaseJobDeclaration.JE_RL_NKPortOfArrival;
			}
			else if (contact == ContactType.Consignor || contact == ContactType.ExportFreightAgent)
			{
				result = BaseJobDeclaration.JE_RL_NKPortOfLoading;
			}
			else if (contact == ContactType.LocalTransport || contact == ContactType.CTO || contact == ContactType.ShippingLine || contact == ContactType.FreightAgent)
			{
				if (direction == DocumentDirection.ARV)
				{
					result = BaseJobDeclaration.JE_RL_NKFinalDestination;
				}
				else if (direction == DocumentDirection.DEP)
				{
					result = BaseJobDeclaration.JE_RL_NKOrigin;
				}
			}
			return result;
		}

		public override string ForeignPort(IContactType contact, DocumentDirection direction)
		{
			string result = "";

			if (contact == ContactType.Consignee || contact == ContactType.ImportFreightAgent)
			{
				result = BaseJobDeclaration.JE_RL_NKPortOfLoading;
			}
			else if (contact == ContactType.Consignor || contact == ContactType.ExportFreightAgent)
			{
				result = BaseJobDeclaration.JE_RL_NKPortOfArrival;
			}
			else if (contact == ContactType.LocalTransport || contact == ContactType.CTO || contact == ContactType.ShippingLine || contact == ContactType.FreightAgent)
			{
				if (direction == DocumentDirection.ARV)
				{
					result = BaseJobDeclaration.JE_RL_NKOrigin;
				}
				else if (direction == DocumentDirection.DEP)
				{
					result = BaseJobDeclaration.JE_RL_NKFinalDestination;
				}
			}
			return result;
		}

		public override bool IsImport
		{
			get { return BaseJobDeclaration.IsImport; }
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			return BaseJobDeclarationDocumentSupporterHelper.GetContactOrganisation(BaseJobDeclaration, contact);
		}

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			List<DataContextValue> result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(".MasterBill"));
			result.Add(new DataContextValue(".CusEntryHeader"));
			result.Add(new DataContextValue(".LandedCostHeader"));
			result.Add(new DataContextValue(".LandedCostEntryHeaders"));
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.Equals(new DataContextValue(".MasterBill")))
			{
				List<IBODocDataProvider> result = new List<IBODocDataProvider>();
				foreach (Bill bill in BaseJobDeclaration.Bills)
				{
					if (bill.IsMasterBill)
					{
						result.Add(BODocDataProvider.Get(bill));
					}
				}
				return result.ToArray();
			}
			else if (dataContextValue.Equals(new DataContextValue(".CusEntryHeader")))
			{
				return new List<BusinessObject>(BaseJobDeclaration.ActiveEntryHeaders).ConvertAll(x => BODocDataProvider.Get(x)).ToArray();
			}
			else if (dataContextValue.Equals(new DataContextValue(".LandedCostHeader")))
			{
				IBODocDataProvider lCHeader = BODocDataProvider.Get((BusinessObject)LandedCostHeaderForDocuments);
				return lCHeader == null ? Array.Empty<IBODocDataProvider>() : new IBODocDataProvider[] { lCHeader };
			}
			else if (dataContextValue.Equals(new DataContextValue(".LandedCostEntryHeaders")))
			{
				return new IBODocDataProvider[] { BODocDataProvider.Get(BaseJobDeclaration) };
			}
			return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
		}

		public override bool StorageDocsAreEditableIfInRelated
		{
			get { return true; }
		}

		#region Line to Print

		protected DocumentSupporterDataState GetDataStateForLinesToPrintDocument()
		{
			if (BaseJobDeclaration.LinesToPrint.Count > 1)
			{
				var args = new System.ComponentModel.CancelEventArgs();
				BaseJobDeclaration.FireOnGetLinesToPrint(args);
				return new DocumentSupporterDataState(!args.Cancel, string.Empty);
			}
			return null;
		}

		#endregion

		#region Transport to Print

		protected DocumentSupporterDataState GetDataStateForTransportToPrintDocument()
		{
			var transportsCount = BaseJobDeclaration.TransportsIncludingRelated.Count;
			if (transportsCount == 1)
			{
				BaseJobDeclaration.TransportToPrint = BaseJobDeclaration.TransportsIncludingRelated[0];
			}
			else if (transportsCount > 1)
			{
				var args = new System.ComponentModel.CancelEventArgs();
				BaseJobDeclaration.FireOnGetTransportToPrint(args);
				return new DocumentSupporterDataState(!args.Cancel, string.Empty);
			}
			return null;
		}

		#endregion

		#region Implementation

		public IDocumentSupportable LandedCostHeaderForDocuments
		{
			get { return BaseJobDeclaration.LandedCostHeaderForDocuments as IDocumentSupportable; }
		}

		#region DocumentPrinted

		// TODO: This looks like something that should be done atomically with the printing
		protected internal void DocumentEventSource_DocumentPrinted(object sender, DocumentPrintedEventArgs e)
		{
			if (e.DeliveryInstructionDestinationType == DeliveryInstructionDestination.Print
				|| e.DeliveryInstructionDestinationType == DeliveryInstructionDestination.TakenFromContact
				|| e.DeliveryInstructionDestinationType == DeliveryInstructionDestination.Auto)
			{
				if (e.MenuItem.SU_MenuName.Contains((NoResString)"Cartage Advice") && BaseJobDeclaration.JP_Calc_CartageAdvised.IsEmpty)
				{
					var factory = new BusinessObjectFactory();
					var dec = factory.Load<BaseJobDeclaration>(BaseJobDeclaration.PK);
					if (dec != null)
					{
						using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(dec))
						{
							dec.JP_Calc_CartageAdvised = ZDateTime.Now;
						}

						ZExceptionReporting.ProcessWithSaveExceptionHandling(() => factory.Save(), null, true);
					}
				}
			}
		}

		#endregion

		#endregion
	}
}
