using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.MasterFiles.Business
{
	public class OrgHeaderDocumentSupporter : DocumentSupporter
	{
		public OrgHeaderDocumentSupporter(OrgHeader orgHeader)
			: base(orgHeader)
		{
		}

		protected OrgHeader OrgHeader
		{
			get { return (OrgHeader)BusinessObject; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.OrganisationCustomiseDocuments; }
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new DataContext[]
				{
					DataContext.Organisation,
					DataContext.OrgSupplierLink,
					DataContext.OrgBuyerLink,
					DataContext.Notes,
					DataContext.OrganisationIRS1099,
					DataContext.GenericFreightJob
				};
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Organisation; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			ArrayList result = new ArrayList();

			switch (dataContext)
			{
				case DataContext.Organisation:
				case DataContext.Notes:
					result.Add(DocumentWrapperFactory.CreateWrapper(DataContext.Organisation, OrgHeader));
					break;
				case DataContext.OrgSupplierLink:
					result.AddRange(GetRoutingOrderDocs());
					break;
				case DataContext.OrgBuyerLink:
					result.AddRange(GetRoutingRecommendationToBuyerDocs());
					break;
				case DataContext.OrganisationIRS1099:
					result.Add(DocumentWrapperFactory.CreateWrapper(DataContext.OrganisationIRS1099, OrgHeader));
					break;
				case DataContext.GenericFreightJob:
					DocumentWrapper[] wrappersToWrap = null;
					if (menuItemPrinted == PrintMenuItem.RoutingRecommendationToConsignee)
					{
						wrappersToWrap = (DocumentWrapper[])GetRoutingRecommendationToBuyerDocs().ToArray(typeof(DocumentWrapper));

						foreach (var wrapper in wrappersToWrap)
						{
							result.Add(DocumentWrapperFactory.CreateOrganisationWrapperWithSupplierBuyer(OrgHeader, null, wrapper));
						}
					}
					else if (menuItemPrinted == PrintMenuItem.RoutingRecommendationToConsignor || menuItemPrinted == PrintMenuItem.RoutingOrder || menuItemPrinted == PrintMenuItem.ReplacementRoutingOrder)
					{
						wrappersToWrap = (DocumentWrapper[])GetRoutingOrderDocs().ToArray(typeof(DocumentWrapper));

						foreach (var wrapper in wrappersToWrap)
						{
							result.Add(DocumentWrapperFactory.CreateOrganisationWrapperWithSupplierBuyer(OrgHeader, wrapper, null));
						}
					}
					else
					{
						result.AddRange(DocumentWrapperFactory.GenerateGenericWrappers(dataContext, OrgHeader));
					}
					break;
			}

			return (DocumentWrapper[])result.ToArray(typeof(DocumentWrapper));
		}

		IEnumerable<DocumentSupporter> JobRequiredDocumentDocumentSupporters
		{
			get
			{
				DocumentSupporter documentSupporter;
				var providers = ObjectFactory.Get<Hashtable>(JobRequiredDocument.DocumentSupportersName).Cast<DictionaryEntry>();

				foreach (var provider in providers)
				{
					documentSupporter = (provider.Value as ObjectHandle)?.GetObject(OrgHeader) as DocumentSupporter;
					if (documentSupporter != null)
					{
						yield return documentSupporter;
					}
				}
			}
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var result = new List<IBODocDataProvider>();
			foreach (var documentSupporter in JobRequiredDocumentDocumentSupporters)
			{
				var boDocDataProviders = documentSupporter.GetBODocDataProviders(dataContextValue, commandBeingRun);
				if (boDocDataProviders?.Any() ?? false)
				{
					result.AddRange(boDocDataProviders);
				}
			}
			return result.Any() ? result.ToArray() : base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
		}

		protected override List<DocumentSupporter> AdditionalBODataSourceDocumentSupporters
		{
			get
			{
				var result = new List<DocumentSupporter>();
				var boDataSourceDocumentSupporters = base.AdditionalBODataSourceDocumentSupporters;
				if (boDataSourceDocumentSupporters?.Any() ?? false)
				{
					result.AddRange(boDataSourceDocumentSupporters);
				}
				var jobRequiredDocumentDocumentSupporters = JobRequiredDocumentDocumentSupporters;
				if (jobRequiredDocumentDocumentSupporters?.Any() ?? false)
				{
					result.AddRange(jobRequiredDocumentDocumentSupporters);
				}
				return result;
			}
		}

		ArrayList GetRoutingRecommendationToBuyerDocs()
		{
			ArrayList result = new ArrayList();

			ClearReplacementRoutingOrderLinks();

			OrgHeaderCollection buyersWithoutAgents = new OrgHeaderCollection(new BusinessObjectFactory());
			foreach (OrgSupplierBuyerLink buyerLink in OrgHeader.BuyerLinks)
			{
				if (buyerLink.SelectedForPrinting)
				{
					DocumentWrapper buyerLinkWrapper = null;
					if (buyerLink.Buyer.UNLOCO != null)
					{
						foreach (OrgSupBuyLinkTrnMode linkMode in buyerLink.OrgSupBuyLinkTrnModes)
						{
							if (buyerLink.Buyer.UNLOCO.GetPublishedAgent(linkMode.PF_TransportMode, AgentDirectionList.Codes.Export) != null)
							{
								buyerLinkWrapper = DocumentWrapperFactory.CreateWrapper(DataContext.OrgBuyerLink, buyerLink);
								break;
							}
						}
					}

					if (buyerLinkWrapper != null)
					{
						result.Add(buyerLinkWrapper);
					}
					else
					{
						buyersWithoutAgents.Add(buyerLink.Buyer);
					}
				}
			}

			if (buyersWithoutAgents.Count > 0 && OnRoutingOrderPrintingNoAgents != null)
			{
				OnRoutingOrderPrintingNoAgents(this, new SuppliersBuyersWithNoAgentsEventArgs(buyersWithoutAgents));
			}

			return result;
		}

		ArrayList GetRoutingOrderDocs()
		{
			ArrayList result = new ArrayList();

			if (supplierLinksForReplacementRoutingOrder != null)
			{
				foreach (OrgSupplierBuyerLink supplierLink in supplierLinksForReplacementRoutingOrder)
				{
					result.Add(DocumentWrapperFactory.CreateWrapper(DataContext.OrgSupplierLink, supplierLink));
				}
			}
			else
			{
				ClearReplacementRoutingOrderLinks();

				OrgHeaderCollection suppliersWithoutAgents = new OrgHeaderCollection(new BusinessObjectFactory());
				foreach (OrgSupplierBuyerLink supplierLink in OrgHeader.SupplierLinks)
				{
					if (supplierLink.SelectedForPrinting)
					{
						DocumentWrapper supplierLinkWrapper = null;
						if (supplierLink.Supplier.UNLOCO != null)
						{
							foreach (OrgSupBuyLinkTrnMode linkMode in supplierLink.OrgSupBuyLinkTrnModes)
							{
								if (supplierLink.Supplier.UNLOCO.GetPublishedAgent(linkMode.PF_TransportMode, AgentDirectionList.Codes.Import) != null)
								{
									supplierLinkWrapper = DocumentWrapperFactory.CreateWrapper(DataContext.OrgSupplierLink, supplierLink);
									break;
								}
							}
						}

						if (supplierLinkWrapper != null)
						{
							result.Add(supplierLinkWrapper);
						}
						else
						{
							suppliersWithoutAgents.Add(supplierLink.Supplier);
						}
					}
				}

				if (suppliersWithoutAgents.Count > 0 && OnRoutingOrderPrintingNoAgents != null)
				{
					OnRoutingOrderPrintingNoAgents(this, new SuppliersBuyersWithNoAgentsEventArgs(suppliersWithoutAgents));
				}
			}

			return result;
		}

		protected override void InitialiseCore(IDocumentEvents documentEventSource)
		{
			base.InitialiseCore(documentEventSource);
			documentEventSource.DocumentPrintRequested += new DocumentCancelEventHandler(DocumentEventSource_DocumentPrintRequested);
		}

#if DEBUG
		public DocumentSupporterDataState BaseResultForTest
		{
			get;
			set;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Menu Name")]
#endif

		public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			DocumentSupporterDataState result = base.GetDataStateBeforeRun(commandAboutToBeRun);
#if DEBUG
			result = BaseResultForTest ?? result;
#endif
			if (result.IsValid)
			{
				menuItemPrinted = PrintMenuItem.Default;

				if (commandAboutToBeRun.SU_MenuPath.IndexOf("Sales") > -1)
				{
					result = new DocumentSupporterDataState(OrgHeader.OH_IsSalesLead, Res.GetString("3f7b6606-9dac-48fb-86ca-e8bb70c9cb52", "Sales Manager documents can only be run on organizations marked as Sales."));
				}
				else if (commandAboutToBeRun.SU_MenuName == "Routing Order")
				{
					menuItemPrinted = PrintMenuItem.RoutingOrder;
					result = new DocumentSupporterDataState(OrgHeader.OH_IsConsignee && OrgHeader.SupplierLinks.Count > 0, Res.GetString("ced44f18-ee96-4e08-8c3a-fbb135818e12", "This document can only be run on a Consignee organization that has at least 1 supplier / consignor specified."));

					if (OnRoutingOrderRecommendationPrinted != null)
					{
						RoutingOrderRecommenationPrintingEventArgs args = new RoutingOrderRecommenationPrintingEventArgs(SuppliersBuyersType.Suppliers);
						OnRoutingOrderRecommendationPrinted(this, args);
						if (args.Cancel)
						{
							if (!args.ErrorMessage.IsEmpty)
							{ result = new DocumentSupporterDataState(false, args.ErrorMessage); }
							else
							{ result = new DocumentSupporterDataState(false, Res.GetString("11362045-8f61-4c43-a8db-1df40dd24da6", "Printing of this document was canceled.")); }
						}
					}
				}
				else if (commandAboutToBeRun.SU_MenuName == Res.GetString("8c77f90e-b2a9-4664-a0b8-93f63bdd9e2c", "Routing Recommendation To Consignee"))
				{
					menuItemPrinted = PrintMenuItem.RoutingRecommendationToConsignee;
					result = new DocumentSupporterDataState(OrgHeader.OH_IsConsignor && OrgHeader.BuyerLinks.Count > 0, Res.GetString("37d0792c-8930-4632-9caf-3b9ab1df95ae", "This document can only be run on a Consignor organization that has at least 1 buyer / consignee specified."));

					if (OnRoutingOrderRecommendationPrinted != null)
					{
						RoutingOrderRecommenationPrintingEventArgs args = new RoutingOrderRecommenationPrintingEventArgs(SuppliersBuyersType.Buyers);
						OnRoutingOrderRecommendationPrinted(this, args);
						if (args.Cancel)
						{
							if (!args.ErrorMessage.IsEmpty)
							{ result = new DocumentSupporterDataState(false, args.ErrorMessage); }
							else
							{ result = new DocumentSupporterDataState(false, Res.GetString("1f3fc328-4b70-4693-9c61-a8dcb28aa17a", "Printing of this document was canceled.")); }
						}
					}
				}
				else if (commandAboutToBeRun.SU_MenuName == Res.GetString("2b629b22-f2d4-43cb-8e54-295a2781021e", "Routing Recommendation To Consignor"))
				{
					menuItemPrinted = PrintMenuItem.RoutingRecommendationToConsignor;
					result = new DocumentSupporterDataState(OrgHeader.OH_IsConsignee && OrgHeader.SupplierLinks.Count > 0, Res.GetString("9fa3dd9c-b571-48b7-a2bb-1fb407919733", "This document can only be run on a Consignee organization that has at least 1 supplier / consignor specified."));

					if (OnRoutingOrderRecommendationPrinted != null)
					{
						RoutingOrderRecommenationPrintingEventArgs args = new RoutingOrderRecommenationPrintingEventArgs(SuppliersBuyersType.Suppliers);
						OnRoutingOrderRecommendationPrinted(this, args);
						if (args.Cancel)
						{
							if (!args.ErrorMessage.IsEmpty)
							{ result = new DocumentSupporterDataState(false, args.ErrorMessage); }
							else
							{ result = new DocumentSupporterDataState(false, Res.GetString("20d99294-9ebf-43c1-865c-33f1687c836e", "Printing of this document was canceled.")); }
						}
					}
				}
				else if (commandAboutToBeRun.SU_MenuName == "Replacement Routing Order")
				{
					menuItemPrinted = PrintMenuItem.ReplacementRoutingOrder;
					result = new DocumentSupporterDataState(OrgHeader.OH_IsForwarder, Res.GetString("eec8ae02-58f5-4a20-a96e-ab604e98543b", "This document should be run from the Forwarder / Agent organization that you wish to replace with a new agent."));
				}
			}

			return result;
		}

#if DEBUG
		internal
#endif
		PrintMenuItem menuItemPrinted;

		internal enum PrintMenuItem
		{
			Default,
			RoutingOrder,
			ReplacementRoutingOrder,
			RoutingRecommendationToConsignee,
			RoutingRecommendationToConsignor
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			return new OrgHeaderContact(OrgHeader, null);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Menu Name")]
		void DocumentEventSource_DocumentPrintRequested(object sender, DocumentCancelEventArgs e)
		{
			if (e.MenuItem.SU_MenuName == "Replacement Routing Order")
			{
				SetupReplacementAgentsForRoutingOrder();
			}
		}

		internal void SetupReplacementAgentsForRoutingOrder()
		{
			ClearReplacementRoutingOrderLinks();

			if (OnRoutingOrderReplacementPrinted != null)
			{
				RoutingOrderReplacementPrintedEventArgs args = new RoutingOrderReplacementPrintedEventArgs();
				OnRoutingOrderReplacementPrinted(this, args);

				UpdateReplacementAgents(args.ReplacementAgent);
			}
		}

		void UpdateReplacementAgents(OrgHeader replacementAgent)
		{
			if (replacementAgent != null)
			{
				OrgSupplierBuyerLink[] supplierLinks = GetAgentSupplierLinks();

				foreach (OrgSupplierBuyerLink link in supplierLinks)
				{
					link.ReplacementAgent = replacementAgent;
				}

				supplierLinksForReplacementRoutingOrder = supplierLinks;
			}
		}

		OrgSupplierBuyerLink[] GetAgentSupplierLinks()
		{
			ZQuery filter = GetAgentSupplierLinksFilter();
			return Factory.Load<OrgSupplierBuyerLink>(filter);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		ZQuery GetAgentSupplierLinksFilter()
		{
			ZDBOnlyQuery supplierLinkQuery = new ZDBOnlyQuery(typeof(OrgSupplierBuyerLink));

			ZDBOnlySubQuery consigneeSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgSupplierBuyerLinkSchema.OL_OH_Buyer);
			consigneeSubQuery.AddToFilter(OrgHeaderSchema.OH_IsConsignee, ZBool.True);

			ZDBOnlySubQuery consignorSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgSupplierBuyerLinkSchema.OL_OH_Supplier);

			ZQuery publishedPortFilter = new ZQuery();

			foreach (OrgAppointedAgentPorts tempAppAgPorts in OrgHeader.AppointedAgentPorts)
			{
				if (tempAppAgPorts.O5_IsPublishedAirAgent || tempAppAgPorts.O5_IsPublishedSeaAgent)
				{
					string port = !tempAppAgPorts.O5_PortOrCountry.IsEmpty ? tempAppAgPorts.O5_PortOrCountry : OrgHeader.OH_RL_NKClosestPort;
					publishedPortFilter.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, port);
				}
			}

			consignorSubQuery.AddToFilter(publishedPortFilter);

			supplierLinkQuery.AddSubQuery(consigneeSubQuery, JoinCondition.And);
			supplierLinkQuery.AddSubQuery(consignorSubQuery, JoinCondition.And);

			return supplierLinkQuery;
		}

		void ClearReplacementRoutingOrderLinks()
		{
			if (supplierLinksForReplacementRoutingOrder != null)
			{
				foreach (OrgSupplierBuyerLink link in supplierLinksForReplacementRoutingOrder)
				{
					link.ReplacementAgent = null;
				}

				supplierLinksForReplacementRoutingOrder = null;
			}
		}

		OrgSupplierBuyerLink[] supplierLinksForReplacementRoutingOrder;

		#region Routing Order / Recommendation Events

		#region Routing Order/Recommendation Printing Event

		/// <summary>
		/// EventArgs to use for the RoutingOrderRecommendationPrintedEventHandler Event Handler.
		/// </summary>
		public class RoutingOrderRecommenationPrintingEventArgs : CancelEventArgs
		{
			public RoutingOrderRecommenationPrintingEventArgs(SuppliersBuyersType type)
			{
				this.OrgType = type;
			}

			public SuppliersBuyersType OrgType { get; set; }
			public ZString ErrorMessage { get; set; }
		}

		/// <summary>
		/// Event is fired when a Routing Order print is requested.
		/// Currently used to display a dialog at the GUI that allows users to choose which supplier(s)
		/// to print the Routing Order for.
		/// </summary>
		public event RoutingOrderRecommendationPrintedEventHandler OnRoutingOrderRecommendationPrinted;

		/// <summary>
		/// Event Handler to use for the OnRoutingOrderRecommendationPrinted event.
		/// </summary>
		public delegate void RoutingOrderRecommendationPrintedEventHandler(object sender, RoutingOrderRecommenationPrintingEventArgs e);

		public enum SuppliersBuyersType
		{
			Suppliers,
			Buyers
		}

		public event RoutingOrderReplacementPrintedEventHandler OnRoutingOrderReplacementPrinted;

		public delegate void RoutingOrderReplacementPrintedEventHandler(object sender, RoutingOrderReplacementPrintedEventArgs e);

		public class RoutingOrderReplacementPrintedEventArgs : EventArgs
		{
			public RoutingOrderReplacementPrintedEventArgs()
			{
			}

			public OrgHeader ReplacementAgent { get; set; }
		}

		#endregion

		#region Routing Order/Recommendation Missing Agents Event

		/// <summary>
		/// EventArgs to use for the RoutingOrderPrintingNoAgentsEventHandler Event Handler.
		/// </summary>
		public class SuppliersBuyersWithNoAgentsEventArgs : EventArgs
		{
			public SuppliersBuyersWithNoAgentsEventArgs(OrgHeaderCollection parties)
			{
				this.Parties = parties;
			}

			public OrgHeaderCollection Parties { get; set; }
		}

		/// <summary>
		/// Event is fired when an attempt is made to print a Routing Order for a supplier where there is no
		/// appointed or published agent / office in the UNLOCO of the supplier.
		/// </summary>
		public event RoutingOrderPrintingNoAgentsEventHandler OnRoutingOrderPrintingNoAgents;

		/// <summary>
		/// Event Handler to use for the OnRoutingOrderPrintingNoAgents event.
		/// </summary>
		public delegate void RoutingOrderPrintingNoAgentsEventHandler(object sender, SuppliersBuyersWithNoAgentsEventArgs e);

		#endregion

		#endregion
	}
}
