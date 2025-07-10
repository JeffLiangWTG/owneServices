using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	public class CommonShipmentDocumentSupporter : DocumentSupporter
	{
		public CommonShipmentDocumentSupporter(CommonShipment shipment)
			: base(shipment)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.INVALID; }
		}

		public override BusinessContext[] SupportedChildBusinessContexts
		{
			get { return Array.Empty<BusinessContext>(); }
		}

		protected virtual ZString GetCartageAdviceDocumentDataStateMessage() => ZString.Empty;

		protected override DataContext[] GetSupportedDataContexts()
		{
			return Array.Empty<DataContext>();
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Core.Constants.DataContext.Service)
			{
				return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.Shipment, Shipment) };
			}
			return Array.Empty<DocumentWrapper>();
		}

		public static CommonShipmentDocumentSupporter New(CommonShipment shipment)
		{
			CommonShipmentDocumentSupporter result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(shipment);
			}
			else if (shipment != null)
			{
				result = new CommonShipmentDocumentSupporter(shipment);
			}

			return result;
		}

		protected delegate CommonShipmentDocumentSupporter NewDelegate(CommonShipment shipment);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#region SuppressResourceStringsCheckRegion

		public static class DocumentNames
		{
			public const string CartageAdvice = "Cartage Advice";
			public const string BillOfLading = "Bill Of Lading";
			public const string ManufacturerBillOfLading = "Manufacturer Bill Of Lading";
			public const string LegacyBillOfLading = "Legacy Bill Of Lading";
			public const string LegacyManufacturerBillOfLading = "Legacy Manufacturer Bill Of Lading";
			public const string LegacyManufacturerBillOfLadingPreprinted = "Legacy Manufacturer BoL To Preprinted";
			public const string LegacyBillOfLadingPreprinted = "Legacy Bill Of Lading Preprinted";
			public const string NeutralHAWB = "Neutral HAWB";
			public const string LaserHAWB = "Laser HAWB";
			public const string LaserHAWBWithFollowOnPage = "Laser HAWB with Follow on Page";
			public const string ShipperDocumentPack = "Shipper Document Pack";
			public const string SendElectronicOriginalBillOfLading = "Send Electronic Original Bill of Lading";
			public const string LandedCosting = "Landed Costing";
			public const string AuthorityToDeal = "Authority To Deal";
		}

		#endregion

		protected CommonShipment Shipment
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (CommonShipment)BusinessObject; }
		}

		#region Query Provider

		protected ICommonShipmentDocumentSupporterQueryProvider QueryProvider
		{
			get
			{
				return Factory.IsInSaveTransaction
					? GetNonGuiQueryProvider()
					: queryProvider ?? (queryProvider = GetQueryProvider());
			}
		}
		ICommonShipmentDocumentSupporterQueryProvider queryProvider;

		protected virtual ICommonShipmentDocumentSupporterQueryProvider GetQueryProvider()
		{
			return Factory.GetValue<ICommonShipmentDocumentSupporterQueryProvider>() ?? new CommonShipmentDocumentSupporterQueryProvider();
		}

		protected virtual ICommonShipmentDocumentSupporterQueryProvider GetNonGuiQueryProvider()
		{
			return new CommonShipmentDocumentSupporterQueryProvider();
		}

		#endregion

		#region Overrides

		protected override void InitialiseCore(IDocumentEvents documentEventSource)
		{
			base.InitialiseCore(documentEventSource);
			documentEventSource.DocumentPrePrinted += new DocumentPrintedEventHandler(DocumentEventSource_DocumentPrePrinted);
			documentEventSource.DocumentPrinted += new DocumentPrintedEventHandler(DocumentEventSource_DocumentPrinted);
			documentEventSource.DocumentPrintRequested += new DocumentCancelEventHandler(DocumentEventSource_DocumentPrintRequested);
		}

		void DocumentEventSource_DocumentPrePrinted(object sender, DocumentPrintedEventArgs e)
		{
			ZString menuName = e.MenuItem.SU_MenuName;

			bool notDraftAndHBLIssueDateIsEmpty = !e.IsDraft && Shipment.JS_HouseBillIssueDate.IsEmpty;
			if (WasPrinted(e.DeliveryInstructionDestinationType)
				&& notDraftAndHBLIssueDateIsEmpty
				&& (IsPrintingHBL(menuName) || IsPrintingEHBL(menuName)))
			{
				ZExceptionReporting.ProcessWithConcurrencyHandling(() =>
				{
					Shipment.JS_HouseBillIssueDate = ZDateTime.Today;
					Shipment.Factory.Save();
				}, () =>
				{
					Shipment.Reload();
					(Shipment as IPackLineSynchroniseProvider)?.PackLineSynchronise?.CleanForConcurrency();
				});
			}
		}

		public override IDocAddress GetOverriddenDeliveryDetails(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			if (contact == ContactType.Consignee && Shipment.ConsigneeDocumentaryAddress.E2_AddressOverride)
			{
				return Shipment.ConsigneeDocumentaryAddress;
			}
			else if (contact == ContactType.Consignor && Shipment.ConsignorDocumentaryAddress.E2_AddressOverride)
			{
				return Shipment.ConsignorDocumentaryAddress;
			}
			else if (contact == ContactType.LocalTransport)
			{
				return GetLocalTransportContactAddress(direction);
			}
			else
			{
				return base.GetOverriddenDeliveryDetails(menuName, contact, direction);
			}
		}

		public override string GetMenuTemplateFilterValue(MenuTemplateFilterType filterType, IBODocDataProvider docDataProvider)
		{
			switch (filterType)
			{
				case MenuTemplateFilterType.PrintStandard:
					return ZBool.True.ToString();

				case MenuTemplateFilterType.HBL:
					return Shipment.JS_HouseBillOfLadingType;

				case MenuTemplateFilterType.CNCTY:
					if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China)
					{
						return ZBool.True.ToString();
					}
					else
					{
						return ZBool.False.ToString();
					}

				case MenuTemplateFilterType.CNAIR:
					if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China && Shipment.IsAir)
					{
						return ZBool.True.ToString();
					}
					else
					{
						return ZBool.False.ToString();
					}

				case MenuTemplateFilterType.CNSEA:
					if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China && Shipment.IsSea)
					{
						return ZBool.True.ToString();
					}
					else
					{
						return ZBool.False.ToString();
					}

				case MenuTemplateFilterType.CONTINUE:
					ZString result = (NoResString)"No";
					if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.SouthAfrica)
					{
						BusinessObject[] entryLine = (BusinessObject[])Factory.Load<Enterprise.Integration.Customs.ZA.ICusEntryLine>(new ZQuery(CusEntryLineSchema.CL_CH, docDataProvider.ParentBusinessObject[CusEntryHeaderSchema.Constants.PK]));
						if (entryLine.Length > 1)
						{
							result = (NoResString)"Yes";
						}
					}
					return result;

				case MenuTemplateFilterType.AUCountry:
					if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia)
					{
						return (NoResString)"Yes";
					}
					else
					{
						return (NoResString)"No";
					}

				case MenuTemplateFilterType.BOE:
					if (Shipment.DeclarationForDocuments != null)
					{
						return GlbCompany.CurrentCompany.GC_RN_NKCountryCode + Shipment.DeclarationForDocuments[JobDeclarationSchema.Constants.JE_MessageType].ToString();
					}
					else
					{
						return "";
					}

				case MenuTemplateFilterType.MULTISUPPLIER:
					ZString isMultiSupplier = (NoResString)"No";
					if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.SouthAfrica)
					{
						if (Shipment.DeclarationForDocuments != null && Shipment.DeclarationForDocuments[JobDeclarationSchema.Constants.JE_MessageType].ToString() == "IMP")
						{
							BusinessObject cusEntryHeader = (BusinessObject)Factory.Load<Enterprise.Integration.Customs.ZA.ICusEntryHeader>(new ZGuid(docDataProvider.ParentBusinessObject[CusEntryHeaderSchema.Constants.PK]));
							if (cusEntryHeader != null)
							{
								OrgHeaderCollection collection = (OrgHeaderCollection)cusEntryHeader["Suppliers"];
								if (collection.Count > 1)
								{
									isMultiSupplier = (NoResString)"Yes";
								}
							}
						}
					}
					return isMultiSupplier;
			}
			return null;
		}

		public override TitleCopyCountPair GetDocumentTitlesForPivot(ZString parentDocumentMenuName, IDocumentSupportable parentBusinessObject, IStmMenuTemplatePivot pivot)
		{
			if (parentDocumentMenuName.StartsWith(DocumentNames.ShipperDocumentPack, StringComparison.OrdinalIgnoreCase))
			{
				DocumentWrapper[] wrappers = Shipment.GetWrappersForARInvoice(Shipment.Consignor);
				if (wrappers == null || wrappers.Length == 0 || (!Shipment.IsAir && Shipment.JS_HouseBillOfLadingType.IsEmpty))
				{
					string reason =
						Shipment.IsAir
						? Res.GetString("b57f1dc9-5de2-4356-b727-0f1c00b0e1af", "A Shipper Document Pack cannot be created.\r\nNo Invoice(s) against the Consignor or the Consignor's Bill-To-Party exists.")
						: Res.GetString("104243e1-0fe2-4528-a647-11d509175087", "A Shipper Document Pack cannot be created.\r\nEither no HBL Type has been specified or no Invoice(s) against the Consignor or the Consignor's Bill-To-Party exists.");
					return new NothingToPrint(reason);
				}
			}

			IStmMenuItem menu = ((StmMenuTemplatePivot)pivot).Factory.Load<StmMenuItemBase>(pivot.SI_SU);

			if (IsPrintingHBL(menu.SU_MenuName))
			{
				return GetHBLDocumentTitle(parentDocumentMenuName, parentBusinessObject, pivot);
			}

			if (menu.SU_MenuName.EqualsIgnoringCase(DocumentNames.LaserHAWB))
			{
				return GetHAWBDocumentTitle(parentDocumentMenuName, parentBusinessObject, pivot);
			}

			if (IsPrintingEHBL(menu.SU_MenuName) && Shipment.JS_ReleaseType == Core.Constants.ShipmentReleaseTypes.ExpressBofL)
			{
				return new TitleCopyCountPair("EXPRESS");
			}

			return null;
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.MaintainShipmentCustomiseDocuments; }
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			string result = string.Empty;

			switch (filterName)
			{
				case DocumentFilters.MOD:
					result = Shipment.IsSeaAirAndHasFirstAirLegLoadingInCurrentCountry ? TransportModes.Air : Shipment.JS_TransportMode;
					break;

				case DocumentFilters.MSC:
					if (Shipment.ShipmentJobHeader != null)
					{
						result = "INV";
					}
					break;

				case DocumentFilters.CNT:
					result = Shipment.JS_PackingMode;
					break;

				case DocumentFilters.FCLBCN:
					result = ((ZBool)(Shipment.JS_PackingMode == Core.Constants.ContainerModes.FCL || Shipment.JS_PackingMode == Core.Constants.ContainerModes.BuyersConsol)).ToString();
					break;

				case DocumentFilters.BKR:
					result = Shipment.IsBrokerageAndDeclarationExists.ToString();
					break;

				case DocumentFilters.MYPENSRR:
					result = ZBool.False.ToString();
					if (GlbBranch.CurrentBranch.GB_RL_NKHomePort == "MYPEN")
					{
						if (Shipment.JS_TransportMode == Core.Constants.TransportModes.Sea ||
							Shipment.JS_TransportMode == Core.Constants.TransportModes.Road ||
							Shipment.JS_TransportMode == Core.Constants.TransportModes.Rail)
						{
							result = ZBool.True.ToString();
						}
					}
					break;

				case DocumentFilters.MYDO:
					if (Shipment.JS_TransportMode == Core.Constants.TransportModes.Sea &&
						Shipment.JS_PackingMode == Core.Constants.ContainerModes.LCL &&
						GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Malaysia)
					{
						result = ZBool.True.ToString();
					}
					else
					{
						result = ZBool.False.ToString();
					}

					break;

				case DocumentFilters.AUBKR:
					if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia &&
						Shipment.IsBrokerageAndDeclarationExists)
					{
						result = ZBool.True.ToString();
					}
					else
					{
						result = ZBool.False.ToString();
					}

					break;

				case DocumentFilters.CTY:
					result = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					break;

				case DocumentFilters.CTYEG:
					result = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnion(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) ? EconomicGroupList.Codes.EuropeanUnion : string.Empty;
					break;

				case DocumentFilters.CTYBKR:
					result = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) + Shipment.IsBrokerageAndDeclarationExists.ToString();
					break;
				case DocumentFilters.DECTP:
					if (Shipment.DeclarationForDocuments != null)
					{
						result = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) + Shipment.DeclarationForDocuments[JobDeclarationSchema.Constants.JE_MessageType].ToString();
					}
					break;

				case DocumentFilters.CO:
					result = GlbCompany.CurrentCompany.GC_Code;
					break;

				case DocumentFilters.BUY:
					if (Shipment.Consignor != null)
					{
						result = Shipment.Consignor.OH_Code;
					}
					break;

				case DocumentFilters.SUP:
					if (Shipment.Consignee != null)
					{
						result = Shipment.Consignee.OH_Code;
					}
					break;

				case DocumentFilters.EBL:
					if (Shipment.JS_TransportMode == Core.Constants.TransportModes.Sea &&
						Shipment.Consignor != null)
					{
						result = Shipment.Consignor.MiscServ.OM_EXAllowedToPrintOriginalBL.ToString();
					}
					break;

				case DocumentFilters.MSGBKR:
					result = Shipment.DecForDocMessageType;
					break;

				case DocumentFilters.MSGBKRCTY:
					result = Shipment.DecForDocMessageType;
					if (!string.IsNullOrEmpty(result))
					{
						result += Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					}

					break;

				case DocumentFilters.MSGBKRCTYAPP:
					result = Shipment.DecForDocMessageType;
					result += Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					if (Shipment.DeclarationForDocuments != null)
					{
						result += Shipment.DeclarationForDocuments[JobDeclarationSchema.Constants.JE_ApplicationCode].ToString();
					}

					break;

				case DocumentFilters.MSGBKRLCO:
					result = Shipment.DecForDocMessageType;
					var landedCostDec = Shipment.DeclarationForDocuments as ILandedCostHeader;
					if (landedCostDec != null)
					{
						result += landedCostDec.LandedCostType;
					}

					break;

				case DocumentFilters.BKRCTY:
					result = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					break;

				case DocumentFilters.BKRCTYAPP:
					result = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					if (Shipment.DeclarationForDocuments != null)
					{
						result += Shipment.DeclarationForDocuments[JobDeclarationSchema.Constants.JE_ApplicationCode].ToString();
					}

					break;

				case DocumentFilters.EXPBKRLIC:
					result = "N";
					if (Shipment.IsBrokerageAndDeclarationExists)
					{
						string messageType = Shipment.DeclarationForDocuments[JobDeclarationSchema.Constants.JE_MessageType].ToString();
						if (messageType == "EXP")
						{
							result = "Y";
						}
					}
					break;
				case DocumentFilters.CTYMOD:
					result = !string.IsNullOrEmpty(GlbBranch.CurrentBranch?.Country?.Code) ? GlbBranch.CurrentBranch?.Country?.Code + Shipment.JS_TransportMode : string.Empty;
					break;
				case DocumentFilters.INTAIR:
					result = !Shipment.IsDomestic() && Shipment.IsAir ? "Y" : "N";
					break;
				case DocumentFilters.DOMAIR:
					result = Shipment.IsDomestic() && Shipment.IsAir ? "Y" : "N";
					break;
				case DocumentFilters.DOMAIRORROAD:
					result = Shipment.IsDomestic() && (Shipment.IsAir || Shipment.IsRoad) ? "Y" : "N";
					break;
				case DocumentFilters.CAIMP:
					result = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada && Shipment.IsImport() ? "Y" : "N";
					break;
				case DocumentFilters.AUMSGNXD:
					if (Shipment.DeclarationForDocuments is Enterprise.Integration.Customs.AU.IJobDeclaration auDeclaration)
					{
						result = Shipment.DecForDocMessageType + (auDeclaration.IsNEXDOCSActive ? "Y" : "N");
					}
					break;
				case DocumentFilters.CAAsAccountedDataSupport:
				case DocumentFilters.CACurrentDataSupport:
					if (Shipment.DeclarationForDocuments is Enterprise.Integration.Customs.CA.IJobDeclaration caDeclaration && caDeclaration.JE_MessageType == Core.Constants.Customs.JobMessageTypes.Codes.Import)
					{
						if (caDeclaration.IsCADEnabled)
						{
							result = Core.Constants.Customs.EntryHeaderTypes.Codes.CommercialAccountingDeclaration;
						}
						else
						{
							result = Core.Constants.Customs.EntryHeaderTypes.Codes.B3CUSDEC;
						}
					}
					break;
				case DocumentFilters.PrintSocialSecurityNumberAllowed:
					result = Shipment.IsImport()
						&& Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == CountryCodes.UnitedStates
						&& Env.Security.USCustomsPrintSSN.IsAllowed ? "Y" : "N";
					break;
				default:
					result = base.GetFilterValue(filterName);
					break;
			}

			return result;
		}

		protected CommonContainer[] ContainersToPrint(string docDirection)
		{
			CommonContainerCollection containers = null;

			if (docDirection == nameof(DocumentDirection.ARV))
			{
				containers = Shipment.ContainersOnConsol(Shipment.ArrivalConsolForDocuments);
			}
			else if (docDirection == nameof(DocumentDirection.DEP))
			{
				containers = Shipment.ContainersOnConsol(Shipment.DepartureConsolForDocuments);
			}

			CommonContainer[] result = null;

			if (containers != null)
			{
				ContainerToSelectFromForPrintingCollection containersToSelectFrom = new ContainerToSelectFromForPrintingCollection(Factory);

				foreach (CommonContainer currentContainer in containers)
				{
					ContainerToSelectFromForPrinting container = new ContainerToSelectFromForPrinting(currentContainer);
					containersToSelectFrom.Add(container);
				}

				ContainersToPrintOptions containersToPrintOptions = QueryProvider.GetContainersToPrint(containersToSelectFrom, false);

				result = containersToPrintOptions != null ? containersToPrintOptions.ContainersToPrint : null;
			}

			return result;
		}

		public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			DocumentSupporterDataState result = base.GetDataStateBeforeRun(commandAboutToBeRun);
			if (result.IsValid && commandAboutToBeRun != null)
			{
				ZString errorMessage = ZString.Empty;

				if (IsCustomsDoc(commandAboutToBeRun))
				{
					result = GetDocumentDataStateForDocumentFromDeclaration(commandAboutToBeRun);

					if (!result.IsValid)
					{
						return result;
					}

					errorMessage = result.ErrorMessage;
				}
				else if (commandAboutToBeRun.SU_MenuName.Contains(DocumentNames.CartageAdvice))
				{
					errorMessage = GetCartageAdviceDocumentDataStateMessage();
				}
				else
				{
					if (commandAboutToBeRun.SU_MenuName.Contains("HAWB") || commandAboutToBeRun.SU_MenuName.EndsWith((NoResString)"House Bill"))
					{
						errorMessage = GetHAWBDocumentDataStateMessage();
					}
					else if (IsPrintingEHBL(commandAboutToBeRun.SU_MenuName))
					{
						errorMessage = GetElectronicBLDocumentDataStateMessage();
					}
					else if (IsPrintingHBL(commandAboutToBeRun.SU_MenuName))
					{
						errorMessage = GetBillOfLadingDocumentDataStateMessage();
					}
					else if (commandAboutToBeRun.SU_MenuName.StartsWith(DocumentNames.LandedCosting, StringComparison.OrdinalIgnoreCase)
						|| commandAboutToBeRun.SU_MenuName.StartsWith(DocumentNames.AuthorityToDeal, StringComparison.OrdinalIgnoreCase))
					{
						if (Shipment.DeclarationForDocuments != null)
						{
							errorMessage = ((IDocumentSupportable)Shipment.DeclarationForDocuments).DocumentSupporter.GetDataStateBeforeRun(commandAboutToBeRun).ErrorMessage;
						}
					}
				}

				result = new DocumentSupporterDataState(errorMessage.IsEmpty, errorMessage);
			}

			return result;
		}

		public override bool AdditionalExcludeFilter(IStmMenuItem commandAboutToBeRun, ZGuid orgDocumentPK)
		{
			if (commandAboutToBeRun == null)
			{
				return true;
			}

			if (IsPrintingEHBL(commandAboutToBeRun.SU_MenuName))
			{
				OrgDocument doc = Factory.Load<OrgDocument>(orgDocumentPK);

				if (doc == null || doc.OD_SU_MenuItem != commandAboutToBeRun.PK)
				{
					return true;
				}
			}

			return base.AdditionalExcludeFilter(commandAboutToBeRun, orgDocumentPK);
		}

		protected override List<DocumentSupporterQuestion> GenerateQuestionsToAskUsersBeforeRunningDocumentCore(IStmMenuItem menuItem)
		{
			if (IsCustomsDoc(menuItem))
			{
				var declaration = (IDocumentSupportable)Shipment.DeclarationForDocuments;
				if (declaration != null)
				{
					return declaration.DocumentSupporter.GenerateQuestionsToAskUsersBeforeRunningDocument(menuItem);
				}
			}
			return base.GenerateQuestionsToAskUsersBeforeRunningDocumentCore(menuItem);
		}

		#region Customs Docs

		protected bool IsCustomsDoc(IStmMenuItem menuItem)
		{
			var docLists = new CustomsDocList[] { new CACustomsDocList(), new NZCustomsDocList(), new USCustomsDocList() };
			return docLists.Any(list => list.IsCustomsDoc(menuItem));
		}

		public abstract class CustomsDocList
		{
			public bool IsCustomsDoc(IStmMenuItem menuItem)
			{
				return (menuItem.SU_BusinessContext == nameof(BusinessContext.Customs)
						|| (menuItem.SU_MenuPath.Contains((NoResString)"Customs") && menuItem.SU_FilterList.Contains(CountryCode)))
						 && ListOfDocuments.Any(doc => menuItem.SU_MenuName.Contains(doc));
			}

			protected abstract string CountryCode { get; }
			protected abstract IEnumerable<string> ListOfDocuments { get; }
		}

		#region CA

		public class CACustomsDocList : CustomsDocList
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name")]
			public const string B3CurrentData = "B3 (Current Data)";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name")]
			public const string B3AsLodged = "B3 (As Accounted)";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name")]
			public const string CADCurrentData = "CAD (Current Data)";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name")]
			public const string CADAsLodged = "CAD (As Accounted)";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name")]
			public const string ReleaseStatusDocument = "Customs Release Status";

			protected override string CountryCode
			{
				get { return Core.Constants.CountryCodes.Canada; }
			}

			protected override IEnumerable<string> ListOfDocuments
			{
				get { return new[] { B3CurrentData, B3AsLodged, ReleaseStatusDocument, CADCurrentData, CADAsLodged }; }
			}
		}

		#endregion

		#region NZ

		public class NZCustomsDocList : CustomsDocList
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name")]
			public const string EntryPrint = "Entry Print";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name")]
			public const string CustomsCertificate = "Customs Certificate";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name")]
			public const string DissectionReport = "Dissection Report";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name")]
			public const string MAFCoverSheet = "MPI Application Cover Sheet";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name")]
			public const string DeliveryOrder = "Delivery Order";

			protected override string CountryCode
			{
				get { return Core.Constants.CountryCodes.NewZealand; }
			}

			protected override IEnumerable<string> ListOfDocuments
			{
				get { return new[] { EntryPrint, CustomsCertificate, DissectionReport, MAFCoverSheet, DeliveryOrder }; }
			}
		}

		#endregion

		#region US

		public class USCustomsDocList : CustomsDocList
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Partial Document Name")]
			public const string EntryPrint = "Entry";

			protected override string CountryCode
			{
				get { return Core.Constants.CountryCodes.UnitedStates; }
			}

			protected override IEnumerable<string> ListOfDocuments
			{
				get { return new[] { EntryPrint, Customs.Common.US.DocumentNames.EntrySummary7501, Customs.Common.US.DocumentNames.FDARecap, Customs.Common.US.DocumentNames.PPQForm368NoticeOfArrival }; }
			}
		}

		#endregion

		#endregion

		public override string LocalPort(IContactType contact, DocumentDirection direction)
		{
			string result = "";

			if (contact == ContactType.FreightAgent || contact == ContactType.LocalTransport)
			{
				switch (direction)
				{
					case DocumentDirection.ARV:
						result = Shipment.JS_RL_NKDestination;
						break;

					case DocumentDirection.DEP:
						result = Shipment.JS_RL_NKOrigin;
						break;

					default:
						result = "";
						break;
				}
			}
			else if (contact == ContactType.Consignee ||
				contact == ContactType.ImportAirFreightAgent ||
				contact == ContactType.ImportFreightAgent ||
				contact == ContactType.ImportSeaFreightAgent ||
				contact == ContactType.NotifyParty)
			{
				result = Shipment.JS_RL_NKDestination;
			}
			else if (contact == ContactType.Consignor ||
				contact == ContactType.ExportAirFreightAgent ||
				contact == ContactType.ExportFreightAgent ||
				contact == ContactType.ExportSeaFreightAgent)
			{
				result = Shipment.JS_RL_NKOrigin;
			}
			else
			{
				result = "";
			}

			return result;
		}

		public override string TransportMode
		{
			get
			{
				if (Shipment.JS_TransportMode == Core.Constants.TransportModes.SeaAir)
				{
					return Core.Constants.TransportModes.Sea;
				}
				else if (Shipment.JS_TransportMode == Core.Constants.TransportModes.AirSea)
				{
					return Core.Constants.TransportModes.Air;
				}

				return Shipment.TransportMode;
			}
		}

		public override string ContainerMode
		{
			get { return Shipment.PackingMode; }
		}

		public override bool IsImport
		{
			get { return Shipment.IsImport(); }
		}

		public override string ForeignPort(IContactType contact, DocumentDirection direction)
		{
			string result = "";

			if (contact == ContactType.Consignee ||
				contact == ContactType.ImportAirFreightAgent ||
				contact == ContactType.ImportFreightAgent ||
				contact == ContactType.ImportSeaFreightAgent ||
				contact == ContactType.NotifyParty)
			{
				result = Shipment.JS_RL_NKOrigin;
			}
			else if (contact == ContactType.Consignor ||
				contact == ContactType.ExportAirFreightAgent ||
				contact == ContactType.ExportFreightAgent ||
				contact == ContactType.ExportSeaFreightAgent)
			{
				result = Shipment.JS_RL_NKDestination;
			}
			else
			{
				switch (direction)
				{
					case DocumentDirection.ARV:
						result = Shipment.JS_RL_NKOrigin;
						break;

					case DocumentDirection.DEP:
						result = Shipment.JS_RL_NKDestination;
						break;

					default:
						result = "";
						break;
				}
			}

			return result;
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			return GetContactOrganisationCore(menuName, contact, direction);
		}

		IDocumentDeliveryContact GetContactOrganisationCore(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			IDocumentDeliveryContact result = null;

			if (contact == ContactType.Consignee)
			{
				return GetConsigneeContactOrganisation();
			}
			else if (contact == ContactType.Consignor)
			{
				result = new OrgHeaderContact(Shipment.Consignor, Shipment.Consignee, Shipment.ConsignorDocumentaryAddress.RealAddress);
			}
			else if (contact == ContactType.NotifyParty)
			{
				JobDocAddress docAddress = Shipment.NotifyPartyDocumentaryAddress;
				OrgHeader org = null;
				OrgAddress address = null;

				if (!docAddress.E2_AddressOverride)
				{
					org = docAddress.Organisation;
					address = docAddress.RealAddress;
				}

				result = new OrgHeaderContact(org, address);
			}
			else if (contact == ContactType.ExportSeaFreightAgent || contact == ContactType.ExportAirFreightAgent || contact == ContactType.ExportFreightAgent)
			{
				if (Shipment.DepartureConsolForDocuments != null)
				{
					result = new OrgHeaderContact(Shipment.DepartureConsolForDocuments.SendingForwarder, Shipment.DepartureConsolForDocuments.SendingForwarderAddress);
				}
			}
			else if (contact == ContactType.ImportFreightAgent ||
				contact == ContactType.ImportAirFreightAgent ||
				contact == ContactType.ImportSeaFreightAgent)
			{
				result = GetImportFreightAgentContactOrganisation(direction);
			}
			else if (contact == ContactType.LocalTransport)
			{
				result = GetLocalTransportContactOrganisation(direction);
			}
			else if (contact == ContactType.ShippingLine)
			{
				OrgHeader shippingLine = GetShippingLine(menuName);
				var address = GetShippingLineAddress(menuName);
				if (shippingLine != null)
				{
					result = new OrgHeaderContact(shippingLine, address);
				}
			}
			else if (contact == ContactType.Receivables)
			{
				if (Shipment.ShipmentJobHeader != null && Shipment.ShipmentJobHeader.LocalCharges != null)
				{
					result = new OrgHeaderContact(Shipment.ShipmentJobHeader.LocalCharges, Shipment.ShipmentJobHeader.LocalChargesAddr);
				}
			}
			else if (contact == ContactType.Payables)
			{
				if (Shipment.ShipmentJobHeader != null)
				{
					result = new OrgHeaderContact(Shipment.ShipmentJobHeader.AgentCollect, Shipment.ShipmentJobHeader.AgentCollectAddr);
				}
			}
			else if (contact == ContactType.ImportSeaDepot ||
				contact == ContactType.ImportAirDepot ||
				contact == ContactType.ImportDepot)
			{
				result = ImportDepotContact;
			}
			else if (contact == ContactType.ExportSeaDepot ||
				contact == ContactType.ExportAirDepot ||
				contact == ContactType.ExportDepot)
			{
				result = ExportDepotContact;
			}
			else if (contact == ContactType.LocalClient)
			{
				if (Shipment.ShipmentJobHeader != null)
				{
					result = new OrgHeaderContact(Shipment.ShipmentJobHeader.LocalCharges, Shipment.ShipmentJobHeader.LocalChargesAddr);
				}
			}
			else if (contact == ContactType.CTO)
			{
				if (direction == DocumentDirection.DEP)
				{
					if (Shipment.DepartureConsolForDocuments != null)
					{
						result = GetOrgHeaderContactFromAddress(Shipment.DepartureConsolForDocuments.DepartureCTOAddress);
					}
				}
				else
				{
					if (Shipment.ArrivalConsolForDocuments != null)
					{
						result = GetOrgHeaderContactFromAddress(Shipment.ArrivalConsolForDocuments.ArrivalCTOAddress);
					}
				}
			}
			else if (contact == ContactType.ExportBroker)
			{
				if (Shipment.ExportBroker != null)
				{
					result = new OrgHeaderContact(Shipment.ExportBroker, null);
				}
			}
			else if (contact == ContactType.ImportBroker)
			{
				if (Shipment.ImportBroker != null)
				{
					result = new OrgHeaderContact(Shipment.ImportBroker, null);
				}
			}
			else if (contact == ContactType.Depot)
			{
				if (direction == DocumentDirection.DEP)
				{
					return ExportDepotContact;
				}
				else
				{
					return ImportDepotContact;
				}
			}
			else if (contact == ContactType.ControllingCustomer)
			{
				var controllingCustomerAddress = Shipment.ControllingCustomerAddress;
				OrgHeader org = null;
				OrgAddress address = null;

				if (!controllingCustomerAddress.E2_AddressOverride)
				{
					org = controllingCustomerAddress.Organisation;
					address = controllingCustomerAddress.Address;
				}

				result = new OrgHeaderContact(org, address);
			}
			else if (contact == ContactType.ControllingAgent)
			{
				var docAddress = Shipment.DocAddresses.FindByDocAddressType(DocAddressType.ControllingAgent);
				if (docAddress != null)
				{
					OrgHeader org = null;
					OrgAddress address = null;

					if (!docAddress.E2_AddressOverride)
					{
						org = docAddress.Organisation;
						address = docAddress.Address;
					}

					result = new OrgHeaderContact(org, address);
				}
			}

			return result;
		}

		#region GetDepotContactOrganisation
		protected OrgHeaderContact ExportDepotContact
		{
			get
			{
				OrgHeaderContact result = null;
				if (Shipment.ExportReceivingDepot != null)
				{
					result = GetOrgHeaderContactFromAddress(Shipment.ExportReceivingDepot);
				}
				else if (Shipment.DepartureConsolForDocuments != null)
				{
					result = GetOrgHeaderContactFromAddress(Shipment.DepartureConsolForDocuments.PackDepotAddress);
				}

				return result;
			}
		}

		protected OrgHeaderContact ImportDepotContact
		{
			get
			{
				OrgHeaderContact result = null;
				if (Shipment.ImportReleaseDepot != null)
				{
					result = GetOrgHeaderContactFromAddress(Shipment.ImportReleaseDepot);
				}
				else if (Shipment.ArrivalConsolForDocuments != null)
				{
					result = GetOrgHeaderContactFromAddress(Shipment.ArrivalConsolForDocuments.UnpackDepotAddress);
				}

				return result;
			}
		}

		protected OrgHeaderContact GetOrgHeaderContactFromAddress(OrgAddress orgAddress)
		{
			return (orgAddress != null) ? new OrgHeaderContact(orgAddress.Header, orgAddress) : null;
		}
		#endregion

		#endregion

		#region Container Printing

		public ContainerToSelectFromForPrintingCollection GetContainersToSelectFrom(ZBool allContainers)
		{
			ContainerToSelectFromForPrintingCollection collection = new ContainerToSelectFromForPrintingCollection(Factory);
			if (allContainers)
			{
				foreach (CommonContainer currentContainer in Shipment.Containers)
				{
					ContainerToSelectFromForPrinting container = new ContainerToSelectFromForPrinting(currentContainer);
					collection.Add(container);
				}
			}
			else
			{
				if (Shipment.DepartureConsol != null)
				{
					foreach (CommonContainer exportContainer in Shipment.DepartureConsol.Containers)
					{
						if (exportContainer.PackLines.Cast<PackLine>().Any(p => p.JL_JS == this.PK && p.JL_RH_NKCommodityCode == Core.Constants.CargoTypes.Hazardous))
						{
							ContainerToSelectFromForPrinting containerToAdd = new ContainerToSelectFromForPrinting(exportContainer);
							collection.Add(containerToAdd);
						}
					}
				}
			}
			return collection;
		}

		#endregion

		#region Implementation

		#region GetContactOrganisations

		OrgHeaderContact GetConsigneeContactOrganisation()
		{
			OrgHeaderContact result = null;
			if (Shipment.CoLoadMasterShipment != null)
			{
				if (Shipment.CoLoadMasterShipment.Consignee != null)
				{
					if (!Shipment.CoLoadMasterShipment.Consignee.MiscServ.OM_FWDealDirectlyWithUltimates)
					{
						result = new OrgHeaderContact(Shipment.CoLoadMasterShipment.Consignee, Shipment.CoLoadMasterShipment.Consignor, Shipment.CoLoadMasterShipment.ConsigneeDocumentaryAddress.RealAddress);
					}
					else
					{
						result = new OrgHeaderContact(Shipment.Consignee, Shipment.Consignor, Shipment.ConsigneeDocumentaryAddress.RealAddress);
					}
				}
			}
			else
			{
				result = new OrgHeaderContact(Shipment.Consignee, Shipment.Consignor, Shipment.ConsigneeDocumentaryAddress.RealAddress);
			}

			return result;
		}

		OrgHeaderContact GetImportFreightAgentContactOrganisation(DocumentDirection direction)
		{
			OrgHeaderContact result = null;

			if (Shipment.DeliveryAgent != null)
			{
				result = new OrgHeaderContact(Shipment.DeliveryAgent, null);
			}
			else if (Shipment.ArrivalConsolForDocuments != null && Shipment.ArrivalConsolForDocuments.ReceivingForwarder != null)
			{
				result = new OrgHeaderContact(Shipment.ArrivalConsolForDocuments.ReceivingForwarder, Shipment.ArrivalConsolForDocuments.ReceivingForwarderAddress);
			}

			return result;
		}

		OrgHeaderContact GetLocalTransportContactOrganisation(DocumentDirection direction)
		{
			OrgHeaderContact result = null;
			if (direction == DocumentDirection.ARV && Shipment.DocsAndCartage.DeliveryCartageCo != null)
			{
				result = new OrgHeaderContact(Shipment.DocsAndCartage.DeliveryCartageCo, Shipment.DocsAndCartage.DeliveryCartageCoAddr);
			}
			else if (direction == DocumentDirection.DEP && Shipment.DocsAndCartage.PickupCartageCo != null)
			{
				result = new OrgHeaderContact(Shipment.DocsAndCartage.PickupCartageCo, Shipment.DocsAndCartage.PickupCartageCoAddr);
			}
			return result;
		}

		IDocAddress GetLocalTransportContactAddress(DocumentDirection direction)
		{
			IDocAddress result = null;
			if (direction == DocumentDirection.ARV && Shipment.DocsAndCartage.DeliveryCartageCoAddr != null)
			{
				result = Shipment.DocsAndCartage.DeliveryCartageCoAddr;
			}
			else if (direction == DocumentDirection.DEP && Shipment.DocsAndCartage.PickupCartageCoAddr != null)
			{
				result = Shipment.DocsAndCartage.PickupCartageCoAddr;
			}
			return result;
		}

		protected virtual OrgHeader GetShippingLine(ZString menuName)
		{
			return (Shipment.DepartureConsol != null) ? Shipment.DepartureConsol.ShippingLine : null;
		}

		protected virtual OrgAddress GetShippingLineAddress(ZString menuName)
		{
			return Shipment.DepartureConsol?.ShippingLineAddress;
		}

		#endregion

		#region DocumentEventSource_DocumentPrinted

		protected virtual void DocumentEventSource_DocumentPrinted(object sender, DocumentPrintedEventArgs e)
		{
			ZString menuName = e.MenuItem.SU_MenuName;

			if (WasPrinted(e.DeliveryInstructionDestinationType))
			{
				if (menuName.Contains(DocumentNames.CartageAdvice))
				{
					if (SelectedContainersToPrint.Count > 0)
					{
						if (e.MenuItem.SU_DocumentDirection == nameof(DocumentDirection.DEP))
						{
							foreach (CommonContainer container in SelectedContainersToPrint)
							{
								container.JC_DepartureCartageAdvised = ZDateTime.Now;
							}
						}
						else
						{
							foreach (CommonContainer container in SelectedContainersToPrint)
							{
								container.JC_ArrivalCartageAdvised = ZDateTime.Now;
							}
						}
					}
					else
					{
						if (e.MenuItem.SU_DocumentDirection == nameof(DocumentDirection.DEP))
						{
							if (Shipment.DocsAndCartage.JP_PickupCartageAdvised.IsEmpty)
							{
								Shipment.DocsAndCartage.JP_PickupCartageAdvised = ZDateTime.Now;
							}
						}
						else
						{
							if (Shipment.DocsAndCartage.JP_DeliveryCartageAdvised.IsEmpty)
							{
								Shipment.DocsAndCartage.JP_DeliveryCartageAdvised = ZDateTime.Now;
							}
						}
					}

					try
					{
						if (Globals.IsUserInteractive)
						{
							Shipment.Factory.Save();
						}
					}
					catch (ZSaveException ex) when (!ex.IsCriticalException())
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
					catch (ZCannotSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
		}

		protected bool WasPrinted(DeliveryInstructionDestination destinationType)
		{
			return destinationType == DeliveryInstructionDestination.Print
				|| destinationType == DeliveryInstructionDestination.TakenFromContact
				|| destinationType == DeliveryInstructionDestination.Auto;
		}

		#region DocumentEventSource_DocumentPrintRequested

		void DocumentEventSource_DocumentPrintRequested(object sender, DocumentCancelEventArgs e)
		{
			ZString menuName = e.MenuItem.SU_MenuName;

			if (IsPrintingHBL(menuName)
				|| IsPrintingHAWB(menuName))
			{
				e.Cancel = Shipment.AWBOrHBLPrintingShouldBeConfirmed() && !QueryProvider.ConfirmBOLPrinting(Shipment);
			}
		}

		#endregion

		protected bool IsPrintingHAWB(ZString documentName)
		{
			return documentName.StartsWith(DocumentNames.NeutralHAWB, StringComparison.OrdinalIgnoreCase)
				|| documentName.StartsWith(DocumentNames.LaserHAWB, StringComparison.OrdinalIgnoreCase)
				|| documentName.StartsWith(DocumentNames.LaserHAWBWithFollowOnPage, StringComparison.OrdinalIgnoreCase);
		}

		protected bool IsPrintingHBL(string documentName)
		{
			return documentName.StartsWith(DocumentNames.BillOfLading, StringComparison.OrdinalIgnoreCase)
				|| documentName.StartsWith(DocumentNames.LegacyBillOfLading, StringComparison.OrdinalIgnoreCase)
				|| documentName.StartsWith(DocumentNames.LegacyBillOfLadingPreprinted, StringComparison.OrdinalIgnoreCase)
				|| documentName.StartsWith(DocumentNames.ManufacturerBillOfLading, StringComparison.OrdinalIgnoreCase)
				|| documentName.StartsWith(DocumentNames.LegacyManufacturerBillOfLading, StringComparison.OrdinalIgnoreCase)
				|| documentName.StartsWith(DocumentNames.LegacyManufacturerBillOfLadingPreprinted, StringComparison.OrdinalIgnoreCase);
		}

		protected bool IsPrintingEHBL(string documentName)
		{
			return documentName.StartsWith(DocumentNames.SendElectronicOriginalBillOfLading, StringComparison.OrdinalIgnoreCase);
		}

		#endregion

		#region GetDocWrappers Methods

		#region GetGenericWrapperForPacks

		protected DocumentWrapper[] GetGenericWrapperForPacks(DataContext dataContext, bool returnSingleDocument)
		{
			var result = GetDocumentWrappers(returnSingleDocument);
			if (!result.Any())
			{
				return result;
			}

			var documentShipment = QueryProvider.GetDocumentOptions(new DocumentShipment(Shipment, dataContext));
			if (documentShipment != null)
			{
				var pieceCount = 0;
				var newWrapper = new List<DocumentWrapper>();

				if (returnSingleDocument)
				{
					var wrapper = result.Single();
					wrapper.DocNumber = 1;
					ApplyOptions(wrapper);
					AdjustPackageCount(wrapper);
					SetPackageNumber(wrapper, 1);
					newWrapper.Add(wrapper);
				}
				else
				{
					foreach (var wrapper in result)
					{
						wrapper.DocNumber = ++pieceCount;
						ApplyOptions(wrapper);
						SetPackageNumber(wrapper, wrapper.DocNumber);
						newWrapper.Add(wrapper);

						if (wrapper.DocNumberOfLabelsToPrint > 0 && pieceCount >= wrapper.DocNumberOfLabelsToPrint)
						{
							return newWrapper.ToArray();
						}
					}
				}
			}
			else
			{
				SetPackageNumber(result);
			}

			return result.ToArray();

			void ApplyOptions(DocumentWrapper wrapper)
			{
				wrapper.DocIncludeConsignee = Globals.IsWeb || documentShipment.IncludeConsignee;
				wrapper.DocIncludeConsignor = Globals.IsWeb || documentShipment.IncludeConsignor;
				wrapper.DocIncludeNone = documentShipment.IncludeNone;
				wrapper.DocNumberOfLabelsToPrint = documentShipment.NumberOfLabelsToPrint;
			}
		}

		protected DocumentWrapper[] GetGenericWrapperForPacksInRange(DataContext dataContext, bool returnSingleDocument)
		{
			var documentShipment = QueryProvider.GetDocumentOptions(new DocumentShipment(Shipment, dataContext));
			return GetGenericWrapperForPacksInRange(documentShipment, returnSingleDocument);
		}

		DocumentWrapper[] GetGenericWrapperForPacksInRange(DocumentShipment documentShipment, bool returnSingleDocument)
		{
			var result = GetDocumentWrappers(returnSingleDocument);
			if (!result.Any())
			{
				return result;
			}

			if (documentShipment == null)
			{
				SetPackageNumber(result);
				return result;
			}

			var pieceCount = 0;
			var labelNumber = documentShipment.LabelRangeFrom;
			var newWrappers = new List<DocumentWrapper>();

			if (returnSingleDocument)
			{
				var wrapper = result.Single();
				wrapper.DocNumber = 1;
				ApplyDocumentOptionsToWrapper(wrapper, documentShipment);
				AdjustPackageCount(wrapper);
				SetPackageNumber(wrapper, labelNumber);
				newWrappers.Add(wrapper);
			}
			else
			{
				foreach (var wrapper in result)
				{
					wrapper.DocNumber = labelNumber++;
					SetPackageNumber(wrapper, wrapper.DocNumber);
					ApplyDocumentOptionsToWrapper(wrapper, documentShipment);
					newWrappers.Add(wrapper);
					pieceCount++;

					if (HasAllLabelsPrinted(wrapper, pieceCount))
					{
						break;
					}
				}
			}

			return newWrappers.ToArray();
		}

		static void AdjustPackageCount(DocumentWrapper wrapper)
		{
			if (wrapper.DocNumberOfLabelsToPrint > 0)
			{
				var packageCollection = (BusinessObjectCollection)wrapper["Packages"];
				var packagesToRemove = packageCollection.Count - wrapper.DocNumberOfLabelsToPrint;
				if (packagesToRemove > 0)
				{
					for (int i = 0; i < packagesToRemove; i++)
					{
						packageCollection.Remove(packageCollection.Last());
					}
				}
			}
		}

		static void SetPackageNumber(DocumentWrapper[] result)
		{
			var packageNumber = 1;
			foreach (var document in result)
			{
				var packageCollection = (BusinessObjectCollection)document["Packages"];
				if (packageCollection != null)
				{
					foreach (var package in packageCollection)
					{
						package["PackageNumber"] = packageNumber++;
					}
				}
			}
		}

		static void SetPackageNumber(DocumentWrapper wrapper, int startNumber)
		{
			var packageCollection = (BusinessObjectCollection)wrapper["Packages"];
			if (packageCollection != null)
			{
				var packageNumber = startNumber;
				foreach (var package in packageCollection)
				{
					package["PackageNumber"] = packageNumber++;
				}
			}
		}

		DocumentWrapper[] GetDocumentWrappers(bool returnSingleDocument)
		{
			var packLines = GetPackLines(Shipment.OuterPackLines);

			if (returnSingleDocument)
			{
				return GetPackLinesWrapper(packLines.ToList());
			}

			return packLines.SelectMany(GetPackLinesWrapper)
				.ToArray();
		}

		void ApplyDocumentOptionsToWrapper(DocumentWrapper wrapper, DocumentShipment documentShipment)
		{
			wrapper.DocIncludeConsignee = Globals.IsWeb || documentShipment.IncludeConsignee;
			wrapper.DocIncludeConsignor = Globals.IsWeb || documentShipment.IncludeConsignor;
			wrapper.DocIncludeNone = documentShipment.IncludeNone;

			wrapper.DocNumberOfLabelsToPrint = documentShipment.NumberOfLabelsToPrint;
			if (documentShipment.LabelRangeTo != 0)
			{
				wrapper.DocNumberOfLabelsToPrint = documentShipment.LabelRangeTo - documentShipment.LabelRangeFrom + 1;
			}
		}

		bool HasAllLabelsPrinted(DocumentWrapper wrapper, ZInt labelsPrinted)
		{
			return wrapper.DocNumberOfLabelsToPrint > 0 && labelsPrinted >= wrapper.DocNumberOfLabelsToPrint;
		}

		PackLine[] GetPackLines(PackLineCollection packLines)
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

		DocumentWrapper[] GetPackLinesWrapper(PackLine packLine)
		{
			DocumentWrapper[] result;
			var wrappers = GetGenericWrappers();
			if (wrappers.Length > 0)
			{
				var wrapper = wrappers[0];
				var iPackLineOverrider = wrapper as IPackLineOverrider;
				if (iPackLineOverrider != null)
				{
					iPackLineOverrider.SetPackageOverride(packLine);
				}

				result = new[] { wrapper };
			}
			else
			{
				result = Array.Empty<DocumentWrapper>();
			}

			return result;
		}

		DocumentWrapper[] GetPackLinesWrapper(List<PackLine> packLines)
		{
			if (packLines.Any())
			{
				var wrappers = GetGenericWrappers();
				if (wrappers.Any())
				{
					var wrapper = wrappers[0];
					var iPackLineOverrider = wrapper as IPackLineOverrider;
					if (iPackLineOverrider != null)
					{
						iPackLineOverrider.SetPackageCollectionOverride(packLines);
					}

					return new[] { wrapper };
				}
			}

			return Array.Empty<DocumentWrapper>();
		}

		protected virtual DocumentWrapper[] GetGenericWrappers()
		{
			return DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, Shipment);
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline, but refactored to only do count once")]
		protected DocumentWrapper[] GetContainerWrappers(ZBool includeUnContainerised, ZBool allContainers, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] wrappers = null;

			ContainerToSelectFromForPrintingCollection containers = GetContainersToSelectFrom(allContainers);

			var containersCount = containers.Count;

			if (containersCount == 0 && includeUnContainerised)
			{
				wrappers = new DocumentWrapper[] { DocumentWrapperFactory.CreateIMOShipmentWrapper(Shipment, Shipment.DepartureConsol) };
			}
			else if (includeUnContainerised || containersCount > 0)
			{
				var containersToPrintOptions = commandBeingRun == null
					? new ContainersToPrintOptions
					{
						ContainersToPrint = containers.Cast<ContainerToSelectFromForPrinting>().Select(c => c.Container).ToArray(),
						IncludeUnContainerised = includeUnContainerised
					}
					: QueryProvider.GetContainersToPrint(containers, includeUnContainerised);

				if (containersToPrintOptions != null && containersToPrintOptions.ContainersToPrint != null)
				{
					ZInt numberOfWrappersToCreate = containersToPrintOptions.IncludeUnContainerised ? containersToPrintOptions.ContainersToPrint.Length + 1 : containersToPrintOptions.ContainersToPrint.Length;

					wrappers = new DocumentWrapper[numberOfWrappersToCreate];

					int i = 0;
					for (; i < containersToPrintOptions.ContainersToPrint.Length; i++)
					{
						DocumentWrapper containerWrapper = DocumentWrapperFactory.CreateContainerWrapperWithShipment(containersToPrintOptions.ContainersToPrint[i], Shipment);
						wrappers.SetValue(containerWrapper, i);
					}

					if (containersToPrintOptions.IncludeUnContainerised)
					{
						wrappers.SetValue(DocumentWrapperFactory.CreateIMOShipmentWrapper(Shipment, Shipment.DepartureConsol), i++);
					}
				}
			}

			return wrappers;
		}

		protected DocumentWrapper[] GetDocWrappersForCartageAdviceContext(IStmMenuItem commandBeingRun, DataContext dataContext)
		{
			if (commandBeingRun == null)
			{
				return null;
			}

			DocumentDirection direction = (DocumentDirection)Enum.Parse(typeof(DocumentDirection), commandBeingRun.SU_DocumentDirection);

			DocumentWrapper[] result = null;
			CommonContainerCollection containers = new CommonContainerCollection(Factory);
			SelectedContainersToPrint.RemoveAll();

			if (commandBeingRun.SU_MenuName.Contains((NoResString)"Confirm") || dataContext == Core.Constants.DataContext.GenericPickupDeliveryConfirm)
			{
				DocumentPickupDeliveryConfirmCollection documentConfirms;
				if (Shipment.JS_PackingMode == Core.Constants.ContainerModes.FCL || (direction == DocumentDirection.ARV && Shipment.JS_PackingMode == Core.Constants.ContainerModes.BuyersConsol))
				{
					documentConfirms = new DocumentPickupDeliveryConfirmCollection(Factory);

					foreach (CommonContainer container in Shipment.Containers.Cast<CommonContainer>())
					{
						documentConfirms.Add(new DocumentPickupDeliveryConfirm(direction == DocumentDirection.DEP ? container.OriginConfirm : container.DestinationConfirm));
					}
				}
				else
				{
					documentConfirms = new DocumentPickupDeliveryConfirmCollection(direction == DocumentDirection.DEP ? Shipment.PickupConfirms : Shipment.DeliveryConfirms);
				}

				DocumentPickupDeliveryConfirmOptions options = new DocumentPickupDeliveryConfirmOptions(documentConfirms);
				return GetWrappersByConfirm(options, dataContext);
			}
			else
			{
				if (direction == DocumentDirection.ARV)
				{
					ZBool doesShipmentHaveBCNConsol = ZBool.False;

					if (Shipment.ArrivalConsolForDocuments != null && Shipment.ArrivalConsolForDocuments.JK_ConsolMode == Core.Constants.ContainerModes.BuyersConsol)
					{
						doesShipmentHaveBCNConsol = ZBool.True;
					}
					if (Shipment.JS_PackingMode == Core.Constants.ContainerModes.FCL || doesShipmentHaveBCNConsol)
					{
						if (Shipment.ArrivalConsolForDocuments != null)
						{
							containers = Shipment.ContainersOnConsol(Shipment.ArrivalConsolForDocuments);
						}
					}
					else
					{
						result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.Shipment, Shipment) };
					}
				}
				else if (direction == DocumentDirection.DEP)
				{
					if (Shipment.JS_PackingMode == Core.Constants.ContainerModes.FCL)
					{
						if (Shipment.DepartureConsolForDocuments != null)
						{
							containers = Shipment.ContainersOnConsol(Shipment.DepartureConsolForDocuments);
						}
					}
					else
					{
						result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.Shipment, Shipment) };
					}
				}
				else if (direction == DocumentDirection.ANY)
				{
					if (containers.Count > 0)
					{
						result = new DocumentWrapper[] { DocumentWrapperFactory.CreateContainerWrapperWithShipment(containers[0], Shipment) };
					}
				}

				if (result == null)
				{
					ContainerToSelectFromForPrintingCollection containersToSelectFrom = new ContainerToSelectFromForPrintingCollection(Factory);

					foreach (CommonContainer currentContainer in containers)
					{
						ContainerToSelectFromForPrinting container = new ContainerToSelectFromForPrinting(currentContainer);
						containersToSelectFrom.Add(container);
					}

					ContainersToPrintOptions containersToPrintOptions = QueryProvider.GetContainersToPrint(containersToSelectFrom, false);

					if (containersToPrintOptions != null && containersToPrintOptions.ContainersToPrint != null)
					{
						result = new DocumentWrapper[containersToPrintOptions.ContainersToPrint.Length];

						for (int i = 0; i < containersToPrintOptions.ContainersToPrint.Length; i++)
						{
							DocumentWrapper containerWrapper = DocumentWrapperFactory.CreateContainerWrapperWithShipment(containersToPrintOptions.ContainersToPrint[i], Shipment);
							result.SetValue(containerWrapper, i);
						}

						SelectedContainersToPrint.AddRange(containersToPrintOptions.ContainersToPrint);
					}
				}
			}

			return result;
		}

		#region GetWrappersByConfirm

		protected DocumentWrapper[] GetWrappersByConfirm(DocumentPickupDeliveryConfirmOptions documentCartageLegOptions, DataContext dataContext)
		{
			DocumentWrapper[] wrappers = null;

			DocumentPickupDeliveryConfirm[] confirmsToPrint = QueryProvider.GetConfirmsToPrint(Shipment, documentCartageLegOptions);

			if (confirmsToPrint != null)
			{
				wrappers = GetDocumentWrappersForSelectedConfirms(confirmsToPrint, dataContext);
			}

			return wrappers;
		}

		DocumentWrapper[] GetDocumentWrappersForSelectedConfirms(DocumentPickupDeliveryConfirm[] confirmToPrint, DataContext dataContext)
		{
			List<DocumentWrapper> wrappers = new List<DocumentWrapper>();

			foreach (DocumentPickupDeliveryConfirm documentConfirm in confirmToPrint)
			{
				if (dataContext == Core.Constants.DataContext.GenericPickupDeliveryConfirm)
				{
					wrappers.Add(DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Shipment, documentConfirm.Confirm)[0]);
				}
				else
				{
					wrappers.Add(DocumentWrapperFactory.CreateWrapperWithParent(Core.Constants.DataContext.PickupDeliveryConfirm, documentConfirm.Confirm, Shipment));
				}
			}

			return wrappers.ToArray();
		}

		#endregion

		protected delegate bool ContainerSelectionDelegate(CommonContainer container);

		protected DocumentWrapper[] GetDocWrappersForGenericFreightJobByContainerIfFCL(DocumentDirection direction, ContainerSelectionDelegate containerSelectionDelegate)
		{
			SelectedContainersToPrint.RemoveAll();

			ZBool isBCN = ZBool.False;
			CommonConsol consol = null;
			switch (direction)
			{
				case DocumentDirection.ARV:
					CommonConsol arrivalConsol = Shipment.ArrivalConsolForDocuments;
					isBCN = (arrivalConsol != null && arrivalConsol.JK_ConsolMode == Core.Constants.ContainerModes.BuyersConsol)
								|| Shipment.JS_PackingMode == Core.Constants.ContainerModes.BuyersConsol;
					consol = arrivalConsol ?? Shipment.DepartureConsolForDocuments;
					break;

				case DocumentDirection.DEP:
					consol = Shipment.DepartureConsolForDocuments ?? Shipment.ArrivalConsolForDocuments;
					break;

				default:
					consol = Shipment.ArrivalConsolForDocuments ?? Shipment.DepartureConsolForDocuments;
					break;
			}

			if (consol != null && (Shipment.JS_PackingMode == Core.Constants.ContainerModes.FCL || isBCN))
			{
				CommonContainerCollection containers = Shipment.ContainersOnConsol(consol);

				ContainerToSelectFromForPrintingCollection containersToSelectFrom = new ContainerToSelectFromForPrintingCollection(Factory);
				foreach (CommonContainer container in containers)
				{
					if (containerSelectionDelegate == null || containerSelectionDelegate(container))
					{
						containersToSelectFrom.Add(new ContainerToSelectFromForPrinting(container));
					}
				}

				ContainersToPrintOptions containersToPrintOptions = QueryProvider.GetContainersToPrint(containersToSelectFrom, false);

				if (containersToPrintOptions != null && containersToPrintOptions.ContainersToPrint != null)
				{
					List<DocumentWrapper> result = new List<DocumentWrapper>();

					foreach (CommonContainer container in containersToPrintOptions.ContainersToPrint)
					{
						DocumentWrapper[] containerWrappers = DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, Shipment, container);

						if (containerWrappers != null)
						{
							foreach (DocumentWrapper containerWrapper in containerWrappers)
							{
								result.Add(containerWrapper);
							}
						}
					}

					SelectedContainersToPrint.AddRange(containersToPrintOptions.ContainersToPrint);
					return result.ToArray();
				}
			}
			else
			{
				return DocumentWrapperFactory.GenerateGenericWrappers(Enterprise.Core.Constants.DataContext.GenericFreightJob, Shipment);
			}

			return null;
		}

#if DEBUG
		internal
#endif
		ContainerNonDependentCollection SelectedContainersToPrint
		{
			get { return selectedContainersToPrint ?? (selectedContainersToPrint = new ContainerNonDependentCollection(Factory)); }
		}
		ContainerNonDependentCollection selectedContainersToPrint;

		#endregion

		#region Get HBL & HAWB Document Titles

		protected virtual AWBDocumentTitle HAWBTitle
		{
			get { return Env.Registry.Freight.AirWaybill.GetHAWBDocumentTitles(); }
		}

		protected virtual TitleCopyCountPair GetHAWBDocumentTitle(ZString parentDocumentMenuName, IDocumentSupportable parentBusinessObject, IStmMenuTemplatePivot pivot)
		{
			TitleCopyCountPair result = null;

			if (HAWBTitle != null)
			{
				if (HAWBTitle.Original1.Name == pivot.SI_DocumentTitle && HAWBTitle.Original1.Printed)
				{
					result = new TitleCopyCountPair(HAWBTitle.Original1.Title);
				}
				else if (HAWBTitle.Original2.Name == pivot.SI_DocumentTitle && HAWBTitle.Original2.Printed)
				{
					result = new TitleCopyCountPair(HAWBTitle.Original2.Title);
				}
				else if (HAWBTitle.Original3.Name == pivot.SI_DocumentTitle && HAWBTitle.Original3.Printed)
				{
					result = new TitleCopyCountPair(HAWBTitle.Original3.Title);
				}
				else if (HAWBTitle.Copy4.Name == pivot.SI_DocumentTitle && HAWBTitle.Copy4.Printed)
				{
					result = new TitleCopyCountPair(HAWBTitle.Copy4.Title);
				}
				else if (HAWBTitle.Copy5.Name == pivot.SI_DocumentTitle && HAWBTitle.Copy5.Printed)
				{
					result = new TitleCopyCountPair(HAWBTitle.Copy5.Title);
				}
				else if (HAWBTitle.Copy6.Name == pivot.SI_DocumentTitle && HAWBTitle.Copy6.Printed)
				{
					result = new TitleCopyCountPair(HAWBTitle.Copy6.Title);
				}
				else if (HAWBTitle.Copy7.Name == pivot.SI_DocumentTitle && HAWBTitle.Copy7.Printed)
				{
					result = new TitleCopyCountPair(HAWBTitle.Copy7.Title);
				}
				else if (HAWBTitle.Copy8.Name == pivot.SI_DocumentTitle && HAWBTitle.Copy8.Printed)
				{
					result = new TitleCopyCountPair(HAWBTitle.Copy8.Title);
				}
				else if (pivot.SI_DocumentTitle.ToUpper().StartsWith("EMAIL"))
				{
					result = new TitleCopyCountPair((NoResString)"EMAIL COPY");
				}
				else if (pivot.SI_DocumentTitle.ToUpper().StartsWith("FAX"))
				{
					result = new TitleCopyCountPair((NoResString)"FAX COPY");
				}
				else
				{
					result = new NothingToPrint();
				}
			}

			return result;
		}

		protected ZString GetTitleForHAWBDocPacksFromConsols(IStmMenuTemplatePivot pivot)
		{
			if (pivot.SI_DocumentTitle.ToUpper().StartsWith("EMAIL"))
			{
				return (NoResString)"EMAIL COPY";
			}
			else if (pivot.SI_DocumentTitle.ToUpper().StartsWith("FAX"))
			{
				return (NoResString)"FAX COPY";
			}
			else
			{
				return HAWBTitle.Copy8.Title;
			}
		}

		protected virtual TitleCopyCountPair GetHBLDocumentTitle(ZString parentDocumentMenuName, IDocumentSupportable parentBusinessObject, IStmMenuTemplatePivot pivot)
		{
			TitleCopyCountPair titles = null;

			if (Shipment.JS_ReleaseType == Core.Constants.ShipmentReleaseTypes.ExpressBofL)
			{
				if (pivot.SI_PrintCopyType == nameof(PrintCopyType.PRN) || pivot.SI_PrintCopyType == nameof(PrintCopyType.ALL))
				{
					if (pivot.SI_DocumentTitle.ToUpper() == "COPY")
					{
						titles = new TitleCopyCountPair("EXPRESS", Shipment.JS_NoCopyBills);
					}
					else
					{
						titles = new NothingToPrint();
					}
				}
				else
				{
					titles = new TitleCopyCountPair("EXPRESS");
				}
			}
			else if (pivot.SI_PrintCopyType == nameof(PrintCopyType.PRN) || pivot.SI_PrintCopyType == nameof(PrintCopyType.ALL))
			{
				if (pivot.SI_DocumentTitle.ToUpper() == "ORIGINAL")
				{
					titles = new TitleCopyCountPair("ORIGINAL", GetCopyCount(pivot, Shipment.JS_NoOriginalBills));
				}
				else if (pivot.SI_DocumentTitle.ToUpper() == "COPY")
				{
					titles = new TitleCopyCountPair("COPY", GetCopyCount(pivot, Shipment.JS_NoCopyBills));
				}
			}
			else
			{
				titles = new TitleCopyCountPair("COPY");
			}

			return titles;
		}

		short GetCopyCount(IStmMenuTemplatePivot pivot, short noOfCopies)
		{
			var template = Factory.Load<StmTemplate>(pivot.SI_SO);
			if (template != null && (template.SO_Name.ToUpper().Contains((NoResString)"DOT MATRIX") || template.SO_Name.ToUpper().Contains("DOT-MATRIX"))) // HACK! If dot matrix HBL always print only one copy
			{
				return Math.Min((short)1, noOfCopies);
			}

			return noOfCopies;
		}

		protected ZBool IsCorrectPivotTypeForHAWBDocPacksFromConsols(IStmMenuTemplatePivot pivot)
		{
			return (HAWBTitle.Copy8.Name == pivot.SI_DocumentTitle && HAWBTitle.Copy8.Printed)
				|| pivot.SI_DocumentTitle.ToUpper().StartsWith("EMAIL")
				|| pivot.SI_DocumentTitle.ToUpper().StartsWith("FAX");
		}
		#endregion

		#region GetDocumentSupporterDataState Message

		public ZString GetHAWBDocumentDataStateMessage()
		{
			bool allConsolsDirect = Shipment.Consols.Count > 0;

			foreach (CommonConsol consol in Shipment.Consols)
			{
				if (consol.JK_AgentType != Core.Constants.AgentType.Direct)
				{
					allConsolsDirect = false;
				}
			}

			return allConsolsDirect ? Res.GetString("c7002e21-1451-4aff-aedf-99d40347825a", "This Shipment is on a direct consol. Please select the MAWB document from the consol.") : "";
		}

		public DocumentSupporterDataState GetDocumentDataStateForDocumentFromDeclaration(IStmMenuItem menuItem)
		{
			if (Shipment.DeclarationForDocuments is IDocumentSupportable { DocumentSupporter: not null } declaration)
			{
				var declarationDocumentSupporter = declaration.DocumentSupporter;
				using (declarationDocumentSupporter.SuspendDeliveryRestrictionCheck())
				{
					return declarationDocumentSupporter.GetDataStateBeforeRun(menuItem);
				}
			}
			else
			{
				return new DocumentSupporterDataState(false, Res.GetString("597facec-b845-4818-acc4-0f613f8f6ac5", "{0} can not be generated until after the Declaration is made.", menuItem.SU_MenuName));
			}
		}

		protected ZString GetElectronicBLDocumentDataStateMessage()
		{
			ZString errorMessage = ZString.Empty;
			ZBool allowSendOriginalBl = (Shipment.Consignor != null && Shipment.Consignor.MiscServ.OM_EXAllowedToPrintOriginalBL);
			if (allowSendOriginalBl)
			{
				if (!IsConsignorContactSetToReceiveElectronicBL)
				{
					errorMessage = Res.GetString("8560f531-d3a3-4603-b10c-ec457ee159a6", "This Consignor does not have a Contact set up to receive Electronic Bill of Lading.");
					errorMessage += "\n";
					errorMessage += Res.GetString("8d0e5985-9fae-43c3-b97a-0e12a844345d", "Please set the selected Consignor's contact to include the 'Send Electronic Original Bill of Lading' document menu in Organizations / Contacts / Documents To Receive.");
				}
			}
			else
			{
				errorMessage = Res.GetString("b3d761fe-11fd-42c2-b75a-58fe1aea2196", "This Consignor is not approved to print the Original Bill of Lading.");
				errorMessage += "\n";
				errorMessage += Res.GetString("b529d6ca-530f-427c-8192-1209ca2c3d89", "You can modify this in the Consignor tab of the Organization form.");
			}

			return errorMessage;
		}

		protected ZString GetBillOfLadingDocumentDataStateMessage()
		{
			ZBool isAllContainersValid = ZBool.True;

			if (Shipment.DepartureConsolForDocuments != null)
			{
				CommonContainerCollection departureContainers = Shipment.ContainersOnConsol(Shipment.DepartureConsolForDocuments);

				foreach (CommonContainer currentContainer in departureContainers)
				{
					if (currentContainer.JC_RC.IsEmpty)
					{
						isAllContainersValid = ZBool.False;
						break;
					}
				}
			}

			return isAllContainersValid ? "" : Res.GetString("1c628016-f8c1-4ac9-bfd3-0df2e3badecf", "Containers attached to Shipment {0} do not have a Type entered.\r\nPlease select a valid Type for each Container before printing a Bill of Lading.", Shipment.JS_UniqueConsignRef);
		}

		ZBool IsConsignorContactSetToReceiveElectronicBL
		{
			get
			{
				if (Shipment.Consignor != null)
				{
					foreach (OrgContact consignorContact in Shipment.Consignor.Contacts)
					{
						foreach (OrgDocument documentToReceive in consignorContact.Documents)
						{
							if (documentToReceive.MenuItem != null &&
								documentToReceive.MenuItem.SU_MenuName.StartsWith((NoResString)"Send Electronic Original Bill of Lading"))
							{
								return ZBool.True;
							}
						}
					}
				}

				return ZBool.False;
			}
		}

		#endregion

		#endregion
	}
}
