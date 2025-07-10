using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Agency.Business
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public class AgencyShipmentDocumentSupporter : CommonShipmentDocumentSupporter
	{
		public AgencyShipmentDocumentSupporter(BillOfLading shipment)
			: base(shipment)
		{
			businessContext = CargoWise.Definitions.BusinessContext.AgencyDocumentation;
		}

		public AgencyShipmentDocumentSupporter(AgencyBooking shipment)
			: base(shipment)
		{
			businessContext = CargoWise.Definitions.BusinessContext.AgencyBooking;
		}

		#region Overrides

		#region BusinessContext

		public override CargoWise.Definitions.BusinessContext BusinessContext
		{
			get { return businessContext; }
		}
		readonly CargoWise.Definitions.BusinessContext businessContext;

		#endregion

		#region GetDocumentWrappersInternal

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result;

			switch (dataContext)
			{
				case Constants.DataContext.GenericFreightJobByContainerIfFCL:
					if (Shipment.JS_PackingMode == Constants.ContainerModes.FCL)
					{
						AgencyShipmentContainerDependentCollection collection = Shipment.IsBillOfLadingStage ? Shipment.RealContainers : Shipment.BookedContainers;
						List<DocumentWrapper> containerWrappers = new List<DocumentWrapper>();

						foreach (AgencyShipmentContainer container in collection)
						{
							containerWrappers.AddRange(DocumentWrapperFactory.GenerateGenericWrappers(dataContext, container));
						}

						result = containerWrappers.ToArray();
					}
					else
					{
						result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Shipment);
					}
					break;

				case Constants.DataContext.AgencyContainer:
				case Constants.DataContext.Container:
					result = GetContainerWrappers();
					break;

				case Constants.DataContext.IMO:
					result = GetWrappersForIMO();
					break;

				case Constants.DataContext.CartageAdvice:
					result = GetWrappersForCartageAdvice();
					break;

				case Constants.DataContext.AgencyShipment:
				case Constants.DataContext.Shipment:
				case Constants.DataContext.PreAlert:
					result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Constants.DataContext.AgencyShipment, Shipment) };
					break;

				default:
					result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Shipment);
					break;
			}
			return result;
		}

		#endregion

		#region GetSupportedDataContexts

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new DataContext[]
				{
					Constants.DataContext.AgencyContainer,
					Constants.DataContext.Container,
					Constants.DataContext.AgencyShipment,
					Constants.DataContext.Shipment,
					Constants.DataContext.IMO,
					Constants.DataContext.CartageAdvice,
					Constants.DataContext.PreAlert,
					Constants.DataContext.GenericFreightJob,
					Constants.DataContext.ARInvoice,
				};
		}

		#endregion

		#region GetFilterValue

		public override string GetFilterValue(DocumentFilters filterName)
		{
			return (filterName == DocumentFilters.CNT) ? (string)Shipment.JS_PackingMode : base.GetFilterValue(filterName);
		}

		#endregion

		#region GetMenuTemplateFilterValue

		public override string GetMenuTemplateFilterValue(MenuTemplateFilterType filterType, IBODocDataProvider docWrapper)
		{
			return (filterType == MenuTemplateFilterType.OBL) ? OBOLCode : base.GetMenuTemplateFilterValue(filterType, docWrapper);
		}

		#endregion

		#region GetAlternativeBranding

		public override ClientAndAgentBrandingBusinessObject GetAlternativeBranding()
		{
			return GetPrincipalBranding();
		}

		#endregion

		#region GetContactOrganisation

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			IDocumentDeliveryContact result;

			if (contact == ContactType.ExportFreightAgent || contact == ContactType.ExportSeaFreightAgent)
			{
				JobDocAddress address = SelectBookingParty(Shipment);
				result = new OrgHeaderContact(address.E2_AddressOverride ? null : address.Organisation, address.E2_AddressOverride ? null : address.Address);
			}
			else if (contact == ContactType.NotifyParty)
			{
				JobDocAddress address = SelectNotifyParty(Shipment);
				result = new OrgHeaderContact(address.E2_AddressOverride ? null : address.Organisation, address.E2_AddressOverride ? null : address.Address);
			}
			else
			{
				result = base.GetContactOrganisation(menuName, contact, direction);
			}

			return result;
		}

		#endregion

		#region GetOverriddenDeliveryDetails

		public override IDocAddress GetOverriddenDeliveryDetails(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			IDocAddress result;

			if (contact == ContactType.ExportFreightAgent || contact == ContactType.ExportSeaFreightAgent)
			{
				result = SelectBookingParty(Shipment);
			}
			else if (contact == ContactType.NotifyParty)
			{
				result = SelectNotifyParty(Shipment);
			}
			else
			{
				result = base.GetOverriddenDeliveryDetails(menuName, contact, direction);
			}

			return (result == null || !result.E2_AddressOverride) ? null : result;
		}

		#endregion

		#region GetMenuItemForVisualisationData

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "must be constant, hard coded constant")]
		public override IStmMenuItem GetMenuItemForVisualisationData(IStmMenuItem originalMenuItem)
		{
			IStmMenuItem copyDocumentMenuItem = null;

			if (BusinessContext == CargoWise.Definitions.BusinessContext.AgencyDocumentation)
			{
				switch (originalMenuItem.SU_MenuName)
				{
					case "Send Electronic Original Bill of Lading":
						var menuPathPrefix = (BusinessObject is BillOfLading billOfLading && billOfLading.UseNewFormBuilderBillOfLading) ? (Constants.DocumentEngine.MenuPaths.LegacyDocuments + "/") : string.Empty;
						copyDocumentMenuItem = Factory.LoadTop1<StmMenuItem>(GetCopyDocumentQuery((NoResString)"Bill of Lading", "AgencyDocumentation", menuPathPrefix + (NoResString)"Export", "", true, false, StmMenuItemTypes.Documents));
						break;
					default:
						break;
				}
			}
			return copyDocumentMenuItem ?? originalMenuItem;
		}

		ZQuery GetCopyDocumentQuery(string menuName, string context, string menuPath, ZString? staff, bool isSystemDefined, bool isClientSpecific, string menuType)
		{
			var query = new ZQuery();

			query.AddToFilter(StmMenuItemSchema.SU_MenuName, menuName);
			query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, context);
			query.AddToFilter(StmMenuItemSchema.SU_MenuPath, menuPath);
			query.AddToFilter(StmMenuItemSchema.SU_GS_NKStaffCode, staff);
			query.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, isSystemDefined);
			query.AddToFilter(StmMenuItemSchema.SU_IsClientSpecific, isClientSpecific);
			query.AddToFilter(StmMenuItemSchema.SU_MenuType, menuType);

			return query;
		}

		#endregion

		public override bool ShowDocumentsInDynamicMenu
		{
			get
			{
				return (BusinessContext == CargoWise.Definitions.BusinessContext.AgencyDocumentation && Shipment.IsBillOfLadingStage)
					|| BusinessContext == CargoWise.Definitions.BusinessContext.AgencyBooking;
			}
		}

		#endregion

		#region ShowReasonForNotPrinting

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != DataContext.GenericFreightJob
				&& dataContext != DataContext.AgencyShipment
				&& dataContext != DataContext.Shipment
				&& base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		#endregion

		#region GetBODocDataProvidersNotFoundMessage

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			switch (dataContextValue.DataContext)
			{
				case Constants.DataContext.AgencyContainer:
				case Constants.DataContext.Container:
				case Constants.DataContext.CartageAdvice when Shipment.JS_PackingMode == Constants.ContainerModes.FCL:
				case Constants.DataContext.GenericFreightJobByContainerIfFCL when Shipment.JS_PackingMode == Constants.ContainerModes.FCL:
					{
						if (!Shipment.ShippingContainers.Any())
						{
							return Res.GetString(
								"564409bd-7ad9-4912-b2a5-d7e36882bd66",
								"No containers are entered for {0}.",
								Shipment.HumanReadableName);
						}

						break;
					}

				case Constants.DataContext.IMO:
					{
						if (Shipment.OuterPackLines.Cast<PackLine>().All(c => c.JL_RH_NKCommodityCode != Constants.CargoTypes.Hazardous))
						{
							return Res.GetString(
								"85221626-44c4-48e5-a243-4c80ebbd4d18",
								"{0} doesn't have a packline with commodity {1}.",
								Shipment.HumanReadableName,
								Constants.CargoTypes.Hazardous);
						}

						break;
					}
			}

			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		#endregion

		#region Implementation

		#region SelectBookingParty

		static JobDocAddress SelectBookingParty(AgencyShipment shipment)
		{
			return SelectIfValid(shipment.BookingPartyDocumentaryAddress)
				?? shipment.ConsignorDocumentaryAddress;
		}

		#endregion

		#region SelectNotifyParty

		static JobDocAddress SelectNotifyParty(AgencyShipment shipment)
		{
			return SelectIfValid(shipment.NotifyPartyDocumentaryAddress)
				?? SelectIfValid(shipment.NotifyParty2DocumentaryAddress)
				?? SelectIfValid(shipment.NotifyParty3DocumentaryAddress)
				?? shipment.ConsigneeDocumentaryAddress;
		}

		#endregion

		#region SelectIfValid

		static JobDocAddress SelectIfValid(JobDocAddress address)
		{
			return IsValid(address) ? address : null;
		}

		#endregion

		#region IsValid

		static bool IsValid(JobDocAddress address)
		{
			if (address.E2_AddressOverride)
			{
				return
					!address.E2_CompanyName.IsEmpty
					&&
					(
						!address.E2_Address1.IsEmpty ||
						!address.E2_Fax.IsEmpty ||
						!address.E2_Email.IsEmpty
					);
			}
			else
			{
				return address.E2_OA_Address.IsValid;
			}
		}

		#endregion

		#region OBOLCode

		string OBOLCode
		{
			get
			{
				DocumentBrandingBusinessObject branding = GetPrincipalBranding();
				return branding == null ? "DEFAULT" : branding.Code.ToString();
			}
		}

		#endregion

		#region Shipment

		protected new AgencyShipment Shipment
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (AgencyShipment)base.Shipment; }
		}

		#endregion

		#region GetPrincipalBranding()

		DocumentBrandingBusinessObject GetPrincipalBranding()
		{
			return DocumentsDataRegistry.Instance.PrincipalDocumentBrand.FindBrandingForPrincipal(Shipment.JS_OH_DeliveryAgent);
		}

		#endregion

		#region GetContainerWrappers

		DocumentWrapper[] GetContainerWrappers()
		{
			List<DocumentWrapper> wrappers = new List<DocumentWrapper>();

			AgencyShipmentContainerDependentCollection containers = Shipment.IsBillOfLadingStage ? Shipment.RealContainers : Shipment.BookedContainers;
			foreach (AgencyShipmentContainer container in containers)
			{
				wrappers.Add(DocumentWrapperFactory.CreateWrapper(Constants.DataContext.AgencyContainer, container));
			}

			return wrappers.ToArray();
		}

		#endregion

		#region GetWrappersForCartageAdvice

		DocumentWrapper[] GetWrappersForCartageAdvice()
		{
			DocumentWrapper[] result;

			if (Shipment.JS_PackingMode == Constants.ContainerModes.FCL)
			{
				AgencyShipmentContainerDependentCollection containers = Shipment.IsBillOfLadingStage ? Shipment.RealContainers : Shipment.BookedContainers;
				result = new DocumentWrapper[containers.Count];
				for (int i = 0; i < containers.Count; i++)
				{
					result[i] = DocumentWrapperFactory.CreateWrapper(Constants.DataContext.AgencyContainer, containers[i]);
				}
			}
			else
			{
				result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Constants.DataContext.AgencyShipment, Shipment) };
			}

			return result;
		}

		#endregion

		#region GetWrappersForIMO

		DocumentWrapper[] GetWrappersForIMO()
		{
			bool includeUnContainerised = false;
			DocumentWrapper[] result = null;
			List<ZGuid> containers = new List<ZGuid>();

			foreach (AgencyShipmentPackLine packLine in Shipment.OuterPackLines)
			{
				if (packLine.CommodityCode != null && packLine.CommodityCode.RH_Code == Constants.CargoTypes.Hazardous)
				{
					ZGuid containerPK1 = packLine.JL_JC;
					if (containerPK1.IsValid)
					{
						if (!containers.Contains(containerPK1))
						{
							containers.Add(containerPK1);
						}
					}
					else
					{
						includeUnContainerised = true;
					}
				}
			}

			if (containers.Count > 0 || includeUnContainerised)
			{
				List<DocumentWrapper> wrappers = new List<DocumentWrapper>();
				foreach (ZGuid containerPK in containers)
				{
					BusinessObject container = Factory.Load<AgencyBookingContainer>(containerPK);
					wrappers.Add(DocumentWrapperFactory.CreateWrapper(Constants.DataContext.AgencyContainer, container));
				}

				if (includeUnContainerised)
				{
					wrappers.Add(DocumentWrapperFactory.CreateWrapper(Constants.DataContext.AgencyShipment, Shipment));
				}

				result = wrappers.ToArray();
			}

			return result;
		}

		#endregion

		#endregion
	}
}


