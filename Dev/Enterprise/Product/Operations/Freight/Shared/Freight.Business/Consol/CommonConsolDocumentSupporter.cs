using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business
{
	public class CommonConsolDocumentSupporter : DocumentSupporter
	{
		#region SuppressResourceStringsCheckRegion

		public static class DocumentNames
		{
			public const string LaserMAWB = "Laser MAWB";
		}

		public static class DocumentTemplateTitles
		{
			public static class France
			{
				public const string Original1 = "ORIGINAL 1 - (TRANSPORTEUR ÉMETTEUR)";
				public const string Original2 = "ORIGINAL 2 - (POUR LE DESTINATAIRE)";
				public const string Original3 = "ORIGINAL 3 - (POUR L’EXPÉDITEUR)";
				public const string Copy4 = "COPY 4 – (RÉCÉPISSÉ DE LIVRAISON)";
				public const string Copy5 = "COPY 5 – (COPIE SUPPLÉMENTAIRE)";
				public const string Copy6 = "COPY 6 – (COPIE SUPPLÉMENTAIRE)";
				public const string Copy7 = "COPY 7 – (COPIE SUPPLÉMENTAIRE)";
				public const string Copy8 = "COPY 8 – (POUR L’AGENT)";
			}

			public static class Spanish
			{
				public const string Original1 = "ORIGINAL 1  -(PARA EL TRANSPORTISTA EMISOR)";
				public const string Original2 = "ORIGINAL 2  -(PARA EL CONSIGNATARIO)";
				public const string Original3 = "ORIGINAL 3 - (PARA EL  EXPEDIDOR/REMITENTE)";
				public const string Copy4 = "COPIA 4 – (RECIBO DE ENTREGA)";
				public const string Copy5 = "COPIA 5 – (COPIA ADICIONAL)";
				public const string Copy6 = "COPIA 6 – (COPIA ADICIONAL)";
				public const string Copy7 = "COPIA 7 – (COPIA ADICIONAL)";
				public const string Copy8 = "COPIA 8 – (PARA EL AGENTE)";
			}

			public const string CoverPage = "Cover Page";
			public const string FaxCopy = "Fax Copy";
			public const string EmailCopy = "Email Copy";

			public const string ForwardingInstructionStandard = "Forwarding Instruction Standard";
			public const string ForwardingInstructionDetailed = "Forwarding Instruction Detailed";
		}

		#endregion

		#region Constructors

		public CommonConsolDocumentSupporter(CommonConsol consol)
			: base(consol)
		{
		}

		#endregion

		#region New

		public static CommonConsolDocumentSupporter New(CommonConsol consol)
		{
			CommonConsolDocumentSupporter result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(consol);
			}
			else if (consol != null)
			{
				result = new CommonConsolDocumentSupporter(consol);
			}

			return result;
		}

		protected delegate CommonConsolDocumentSupporter NewDelegate(CommonConsol consol);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		#region Query Provider

		protected ICommonConsolDocumentSupporterQueryProvider QueryProvider
		{
			get
			{
				return Factory.IsInSaveTransaction
					? GetNonGuiQueryProvider()
					: queryProvider ?? (queryProvider = GetQueryProvider());
			}
		}
		ICommonConsolDocumentSupporterQueryProvider queryProvider;

		protected virtual ICommonConsolDocumentSupporterQueryProvider GetQueryProvider()
		{
			return Factory.GetValue<ICommonConsolDocumentSupporterQueryProvider>() ?? GetNonGuiQueryProvider();
		}

		protected virtual ICommonConsolDocumentSupporterQueryProvider GetNonGuiQueryProvider()
		{
			return new CommonConsolDocumentSupporterQueryProvider();
		}

		#endregion

		#region Overrides

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.INVALID; }
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[] { Core.Constants.DataContext.None };
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.MaintainConsolCustomiseDocuments; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return null;
		}

		protected override void InitialiseCore(IDocumentEvents documentEventSource)
		{
			base.InitialiseCore(documentEventSource);
			documentEventSource.DocumentPrinted += new DocumentPrintedEventHandler(DocumentEventSource_DocumentPrinted);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document header")]
		void DocumentEventSource_DocumentPrinted(object sender, DocumentPrintedEventArgs e)
		{
			ZString menuName = e.MenuItem.SU_MenuName;

			if (DocumentWasPrinted(e.DeliveryInstructionDestinationType) && menuName.Contains((NoResString)"Cartage Advice"))
			{
				BusinessObjectCollection containers = null;

				if (SelectedContainersToPrint.Count > 0)
				{
					containers = SelectedContainersToPrint;
				}
				else if (menuName == "Multi-Container Cartage Advice" || menuName == "Multi-Container Cartage Advice w Receipt")
				{
					containers = Consol.Containers;
				}

				if (containers != null)
				{
					if (e.MenuItem.SU_DocumentDirection == nameof(DocumentDirection.DEP))
					{
						foreach (CommonContainer container in containers)
						{
							if (container.JC_ContainerMode == Core.Constants.ContainerModes.FCL)
							{
								CommonContainer realContainer = (CommonContainer)Consol.Containers.FindByPK(container.PK);
								realContainer.JC_DepartureCartageAdvised = ZDateTime.Now;
							}
						}
					}
					else
					{
						foreach (CommonContainer container in containers)
						{
							if (container.JC_ContainerMode == Core.Constants.ContainerModes.FCL)
							{
								CommonContainer realContainer = (CommonContainer)Consol.Containers.FindByPK(container.PK);
								realContainer.JC_ArrivalCartageAdvised = ZDateTime.Now;
							}
						}
					}

					try
					{
						Consol.Factory.Save();
					}
					catch (ZSaveConcurrencyException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
		}

		bool DocumentWasPrinted(DeliveryInstructionDestination destinationType)
		{
			return destinationType == DeliveryInstructionDestination.Print
				|| destinationType == DeliveryInstructionDestination.TakenFromContact
				|| destinationType == DeliveryInstructionDestination.Auto;
		}

		#region Branding

		public override IOrgHeader GetBrandedOrganisation(IContactType contactType, DocumentDirection direction)
		{
			IOrgHeader result = null;

			if (contactType != null && ((ContactType)contactType).BrandingType == ContactBrandingType.Agent)
			{
				result = (direction == DocumentDirection.DEP) ? Consol.SendingForwarder : Consol.ReceivingForwarder;
			}

			return result;
		}

		#endregion

		#region GetContactOrganisation

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			IDocumentDeliveryContact result = null;

			if (contact == ContactType.ExportFreightAgent ||
				contact == ContactType.ExportAirFreightAgent ||
				contact == ContactType.ExportSeaFreightAgent ||
				contact == ContactType.Payables)
			{
				result = Consol.SendingForwarder != null ? new OrgHeaderContact(Consol.SendingForwarder, Consol.SendingForwarderAddress) : null;
			}
			else if (contact == ContactType.ImportFreightAgent ||
				contact == ContactType.ImportAirFreightAgent ||
				contact == ContactType.ImportSeaFreightAgent ||
				contact == ContactType.Receivables)
			{
				result = Consol.ReceivingForwarder != null ? new OrgHeaderContact(Consol.ReceivingForwarder, Consol.ReceivingForwarderAddress) : null;
			}
			else if (contact == ContactType.ShippingLine)
			{
				result = new OrgHeaderContact(Consol.ShippingLine, Consol.ShippingLineAddress);
			}
			else if (contact == ContactType.ExportDepot || contact == ContactType.ExportAirDepot || contact == ContactType.ExportSeaDepot)
			{
				result = ExportDepotContact;
			}
			else if (contact == ContactType.ImportDepot || contact == ContactType.ImportAirDepot || contact == ContactType.ImportSeaDepot)
			{
				result = ImportDepotContact;
			}
			else if (contact == ContactType.Depot)
			{
				result = (direction == DocumentDirection.DEP) ? ExportDepotContact : ImportDepotContact;
			}
			else if (contact == ContactType.CTO)
			{
				result = (direction == DocumentDirection.DEP) ? GetOrgHeaderContactFromAddress(Consol.DepartureCTOAddress) : GetOrgHeaderContactFromAddress(Consol.ArrivalCTOAddress);
			}

			return result;
		}

		protected OrgHeaderContact ExportDepotContact
		{
			get
			{
				OrgAddress address = Consol.PackDepotAddress;

				if (address == null &&
					Consol.JK_TransportMode == Core.Constants.TransportModes.Sea &&
					(Consol.JK_ConsolMode == Core.Constants.ContainerModes.FCL ||
					Consol.JK_ConsolMode == Core.Constants.ContainerModes.BuyersConsol))
				{
					address = Consol.DepartureCTOAddress;
				}

				return GetOrgHeaderContactFromAddress(address);
			}
		}

		protected OrgHeaderContact ImportDepotContact
		{
			get
			{
				OrgAddress address = Consol.UnpackDepotAddress;

				if (address == null &&
					Consol.JK_TransportMode == Core.Constants.TransportModes.Sea &&
					(Consol.JK_ConsolMode == Core.Constants.ContainerModes.FCL ||
					Consol.JK_ConsolMode == Core.Constants.ContainerModes.BuyersConsol))
				{
					address = Consol.ArrivalCTOAddress;
				}

				return GetOrgHeaderContactFromAddress(address);
			}
		}

		protected OrgHeaderContact GetOrgHeaderContactFromAddress(OrgAddress orgAddress)
		{
			return (orgAddress != null) ? new OrgHeaderContact(orgAddress.Header, orgAddress) : null;
		}

		#endregion

		#endregion

		#region GetContainerDocBusinessObjects

		protected DocumentWrapper[] GetContainerDocBusinessObjects(IStmMenuItem commandBeingRun)
		{
			var containersToPrintOptions = commandBeingRun == null
				? new ContainersToPrintOptions
				{
					ContainersToPrint = ContainersToSelectFrom.Select(c => c.Container).ToArray(),
					IncludeUnContainerised = false
				}
				: QueryProvider.GetContainersToPrint(ContainersToSelectFrom, false);
			return containersToPrintOptions != null ? GenerateWrappersFromContainersToPrintOptions(containersToPrintOptions) : null;
		}

		#endregion

		#region Get Containers

		protected ContainerToSelectFromForPrintingCollection ContainersToSelectFrom
		{
			get
			{
				BusinessObjectFactory factoryForSelectingContainers = new BusinessObjectFactory();
				ContainerToSelectFromForPrintingCollection collection = new ContainerToSelectFromForPrintingCollection(factoryForSelectingContainers);

				foreach (CommonContainer currentContainer in Consol.Containers)
				{
					ContainerToSelectFromForPrinting container = new ContainerToSelectFromForPrinting(currentContainer);
					if (container != null)
					{
						collection.Add(container);
					}
				}

				return collection;
			}
		}

#if DEBUG
		internal
#endif
 ContainerNonDependentCollection SelectedContainersToPrint
		{
			get { return selectedContainersToPrint ?? (selectedContainersToPrint = new ContainerNonDependentCollection(Factory)); }
		}

		ContainerNonDependentCollection selectedContainersToPrint;

		protected DocumentWrapper[] GetWrappersByContainer(ContainerToSelectFromForPrintingCollection containersToSelectFrom)
		{
			return GenerateWrappersFromContainersToPrintOptions(QueryProvider.GetContainersToPrint(containersToSelectFrom, false));
		}

		protected DocumentWrapper[] GenerateWrappersFromContainersToPrintOptions(ContainersToPrintOptions containersToPrintOptions)
		{
			DocumentWrapper[] result = null;
			SelectedContainersToPrint.RemoveAll();

			if (containersToPrintOptions != null && containersToPrintOptions.ContainersToPrint != null)
			{
				result = new DocumentWrapper[containersToPrintOptions.ContainersToPrint.Length];

				for (int i = 0; i < containersToPrintOptions.ContainersToPrint.Length; i++)
				{
					result.SetValue(CreateContainerWrapper(containersToPrintOptions.ContainersToPrint[i]), i);
				}

				SelectedContainersToPrint.AddRange(containersToPrintOptions.ContainersToPrint);
			}

			return result;
		}

		protected virtual DocumentWrapper CreateContainerWrapper(CommonContainer container)
		{
			return DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.Container, container);
		}

		protected IDocumentSupportable[] GetChildCollectionByContainer(ContainerToSelectFromForPrintingCollection containersToSelectFrom)
		{
			IDocumentSupportable[] result = null;

			SelectedContainersToPrint.RemoveAll();

			ContainersToPrintOptions containersToPrintOptions = QueryProvider.GetContainersToPrint(containersToSelectFrom, false);

			if (containersToPrintOptions != null && containersToPrintOptions.ContainersToPrint != null && containersToPrintOptions.ContainersToPrint.Length > 0)
			{
				List<IDocumentSupportable> containers = new List<IDocumentSupportable>();

				foreach (CommonContainer container in containersToPrintOptions.ContainersToPrint)
				{
					CommonContainer documentSupportableContainer = CreateContainerDocumentSupportable(container);

					if (documentSupportableContainer != null)
					{
						containers.Add((IDocumentSupportable)documentSupportableContainer);
					}
				}

				result = containers.ToArray();
				SelectedContainersToPrint.AddRange(containersToPrintOptions.ContainersToPrint);
			}
			else
			{
				result = System.Array.Empty<IDocumentSupportable>();
			}

			return result;
		}

		protected virtual CommonContainer CreateContainerDocumentSupportable(CommonContainer container)
		{
			return (CommonContainer)Consol.Factory.Load(Consol.Containers.GetTypeOfElementsFromPK(container.PK), container.PK);
		}

		#endregion

		#region TransportMode, ContainerMode, Foreign and Local Ports

		public override string TransportMode
		{
			get { return Consol.JK_TransportMode; }
		}

		public override string ContainerMode
		{
			get { return Consol.JK_ConsolMode; }
		}

		public override string ForeignPort(IContactType contact, DocumentDirection direction)
		{
			string result;

			if (IsImportContact(contact) || (!IsExportContact(contact) && direction == DocumentDirection.ARV))
			{
				result = Consol.JK_RL_NKLoadPort;
			}
			else if (IsExportContact(contact) || (!IsImportContact(contact) && direction == DocumentDirection.DEP))
			{
				result = Consol.JK_RL_NKDischargePort;
			}
			else
			{
				result = "";
			}

			return result;
		}

		public override string LocalPort(IContactType contact, DocumentDirection direction)
		{
			string result;

			if (IsImportContact(contact) || (!IsExportContact(contact) && direction == DocumentDirection.ARV))
			{
				result = Consol.JK_RL_NKDischargePort;
			}
			else if (IsExportContact(contact) || (!IsImportContact(contact) && direction == DocumentDirection.DEP))
			{
				result = Consol.JK_RL_NKLoadPort;
			}
			else
			{
				result = "";
			}

			return result;
		}

		bool IsImportContact(IContactType contact)
		{
			return contact == ContactType.Consignee ||
				contact == ContactType.ImportFreightAgent ||
				contact == ContactType.ImportSeaFreightAgent ||
				contact == ContactType.ImportAirFreightAgent ||
				contact == ContactType.ImportDepot ||
				contact == ContactType.ImportSeaDepot ||
				contact == ContactType.ImportAirDepot ||
				contact == ContactType.ImportBroker ||
				contact == ContactType.NotifyParty;
		}

		bool IsExportContact(IContactType contact)
		{
			return contact == ContactType.Consignor ||
				contact == ContactType.ExportFreightAgent ||
				contact == ContactType.ExportSeaFreightAgent ||
				contact == ContactType.ExportAirFreightAgent ||
				contact == ContactType.ExportDepot ||
				contact == ContactType.ExportSeaDepot ||
				contact == ContactType.ExportAirDepot ||
				contact == ContactType.ExportBroker;
		}

		#endregion

		#region Get Filter Value

		public override string GetFilterValue(DocumentFilters filterName)
		{
			switch (filterName)
			{
				case DocumentFilters.MOD:
					return Consol.JK_TransportMode;

				case DocumentFilters.CTY:
					return GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				case DocumentFilters.MYDO:
					foreach (CommonShipment shipment in Consol.Shipments)
					{
						if (shipment.JS_TransportMode == Core.Constants.TransportModes.Sea &&
							shipment.JS_PackingMode == Core.Constants.ContainerModes.LCL &&
							GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Malaysia)
						{
							return ZBool.True.ToString();
						}
					}
					return ZBool.False.ToString();

				case DocumentFilters.BCN:
					return new ZBool(Consol.JK_ConsolMode == Core.Constants.ContainerModes.BuyersConsol).ToString();

				case DocumentFilters.CTYMOD:
					return GlbCompany.CurrentCompany.GC_RN_NKCountryCode + Consol.JK_TransportMode;
				case DocumentFilters.HIDE:
					return ZBool.False.ToString();
				case DocumentFilters.AUBLK:
					return new ZBool(IsCountryAU && IsBulk).ToString();
				case DocumentFilters.AUNOTBLK:
					return new ZBool(IsCountryAU && !IsBulk).ToString();

				case DocumentFilters.CountryDirection:
					return GlbCompany.CurrentCompany.GC_RN_NKCountryCode + GetImportExport();

				default:
					return base.GetFilterValue(filterName);
			}
		}

		ZString GetImportExport()
		{
			var direction = "";
			if (Consol.IsImport())
			{
				direction = "IMP";
			}
			else if (Consol.IsExport())
			{
				direction = "EXP";
			}
			return direction;
		}

		bool IsBulk
		{
			get
			{
				return (Consol.JK_ConsolMode == Core.Constants.ContainerModes.BreakBulk
						|| Consol.JK_ConsolMode == Core.Constants.ContainerModes.Bulk
						|| Consol.JK_ConsolMode == Core.Constants.ContainerModes.Liquid);
			}
		}

		bool IsCountryAU
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia; }
		}

		#endregion

		#region GetDocumentTitlesForPivot

		public override TitleCopyCountPair GetDocumentTitlesForPivot(ZString parentDocumentMenuName, IDocumentSupportable parentBusinessObject, IStmMenuTemplatePivot pivot)
		{
			var menu = pivot.MenuItem;

			if (menu.SU_IsSystemDefined && menu.SU_MenuName.EqualsIgnoringCase(DocumentNames.LaserMAWB))
			{
				return GetMAWBDocumentTitle(pivot);
			}

			return base.GetDocumentTitlesForPivot(parentDocumentMenuName, parentBusinessObject, pivot);
		}

		AWBDocumentTitle MAWBTitle
		{
			get { return Env.Registry.Freight.AirWaybill.GetMAWBDocumentTitles(); }
		}

		TitleCopyCountPair GetMAWBDocumentTitle(IStmMenuTemplatePivot pivot)
		{
			TitleCopyCountPair result = null;

			if (MAWBTitle != null)
			{
				if (MAWBTitle.Original1.Printed
					&& (pivot.SI_DocumentTitle == MAWBTitle.Original1.Name || pivot.SI_DocumentTitle == DocumentTemplateTitles.France.Original1 || pivot.SI_DocumentTitle == DocumentTemplateTitles.Spanish.Original1))
				{
					result = new TitleCopyCountPair(pivot.SI_DocumentTitle);
				}
				else if (MAWBTitle.Original2.Printed
					&& (pivot.SI_DocumentTitle == MAWBTitle.Original2.Name || pivot.SI_DocumentTitle == DocumentTemplateTitles.France.Original2 || pivot.SI_DocumentTitle == DocumentTemplateTitles.Spanish.Original2))
				{
					result = new TitleCopyCountPair(pivot.SI_DocumentTitle);
				}
				else if (MAWBTitle.Original3.Printed
					&& (pivot.SI_DocumentTitle == MAWBTitle.Original3.Name || pivot.SI_DocumentTitle == DocumentTemplateTitles.France.Original3 || pivot.SI_DocumentTitle == DocumentTemplateTitles.Spanish.Original3))
				{
					result = new TitleCopyCountPair(pivot.SI_DocumentTitle);
				}
				else if (MAWBTitle.Copy4.Printed
					&& (pivot.SI_DocumentTitle == MAWBTitle.Copy4.Name || pivot.SI_DocumentTitle == DocumentTemplateTitles.France.Copy4 || pivot.SI_DocumentTitle == DocumentTemplateTitles.Spanish.Copy4))
				{
					result = new TitleCopyCountPair(pivot.SI_DocumentTitle);
				}
				else if (MAWBTitle.Copy5.Printed
					&& (pivot.SI_DocumentTitle == MAWBTitle.Copy5.Name || pivot.SI_DocumentTitle == DocumentTemplateTitles.France.Copy5 || pivot.SI_DocumentTitle == DocumentTemplateTitles.Spanish.Copy5))
				{
					result = new TitleCopyCountPair(pivot.SI_DocumentTitle);
				}
				else if (MAWBTitle.Copy6.Printed
					&& (pivot.SI_DocumentTitle == MAWBTitle.Copy6.Name || pivot.SI_DocumentTitle == DocumentTemplateTitles.France.Copy6 || pivot.SI_DocumentTitle == DocumentTemplateTitles.Spanish.Copy6))
				{
					result = new TitleCopyCountPair(pivot.SI_DocumentTitle);
				}
				else if (MAWBTitle.Copy7.Printed
					&& (pivot.SI_DocumentTitle == MAWBTitle.Copy7.Name || pivot.SI_DocumentTitle == DocumentTemplateTitles.France.Copy7 || pivot.SI_DocumentTitle == DocumentTemplateTitles.Spanish.Copy7))
				{
					result = new TitleCopyCountPair(pivot.SI_DocumentTitle);
				}
				else if (MAWBTitle.Copy8.Printed
					&& (pivot.SI_DocumentTitle == MAWBTitle.Copy8.Name || pivot.SI_DocumentTitle == DocumentTemplateTitles.France.Copy8 || pivot.SI_DocumentTitle == DocumentTemplateTitles.Spanish.Copy8))
				{
					result = new TitleCopyCountPair(pivot.SI_DocumentTitle);
				}
				else if (pivot.SI_DocumentTitle == DocumentTemplateTitles.CoverPage)
				{
					result = new TitleCopyCountPair(pivot.SI_DocumentTitle);
				}
				else if (pivot.SI_DocumentTitle == DocumentTemplateTitles.EmailCopy)
				{
					result = new TitleCopyCountPair(pivot.SI_DocumentTitle);
				}
				else if (pivot.SI_DocumentTitle == DocumentTemplateTitles.FaxCopy)
				{
					result = new TitleCopyCountPair(pivot.SI_DocumentTitle);
				}
				else
				{
					result = new NothingToPrint();
				}
			}

			return result;
		}

		#endregion

		protected CommonConsol Consol
		{
			get { return (CommonConsol)BusinessObject; }
		}
	}
}
