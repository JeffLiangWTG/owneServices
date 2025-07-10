using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class DeliveryAgentOrgHeaderDocumentSupporter : OrgHeaderDocumentSupporter
	{
		public DeliveryAgentOrgHeaderDocumentSupporter(DeliveryAgentOrgHeader deliveryAgentOrgHeader)
			: base(deliveryAgentOrgHeader)
		{
		}

		protected DeliveryAgentOrgHeader DeliveryAgentOrgHeader
		{
			get { return (DeliveryAgentOrgHeader)BusinessObject; }
		}

		#region Overrides

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.DeliveryAgent; }
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[]
			{
				Core.Constants.DataContext.Consol,
				Core.Constants.DataContext.ARInvoice,
				Core.Constants.DataContext.GenericFreightJob,
				Core.Constants.DataContext.GenericFreightJobInvoice
			};
		}

		public override BusinessContext[] SupportedChildBusinessContexts
		{
			get { return new[] { BusinessContext.Shipment }; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (DeliveryAgentOrgHeader.Consol != null)
			{
				switch (dataContext)
				{
					case Core.Constants.DataContext.Consol:
					case Core.Constants.DataContext.GenericFreightJob:
						DocumentWrapper[] documentWrappers = DeliveryAgentOrgHeader.Consol.DocumentSupporter.GetDocumentWrappers(dataContext, commandBeingRun);
						foreach (DocumentWrapper documentWrapper in documentWrappers)
						{
							(documentWrapper as IConsolDeliveryAgent)?.SetDeliveryAgent(DeliveryAgentOrgHeader);
						}
						return documentWrappers;

					case Core.Constants.DataContext.ARInvoice:
						return GetWrappersForARInvoice(Core.Constants.DataContext.ARInvoice, false);
					case Core.Constants.DataContext.GenericFreightJobInvoice:
						return GetWrappersForARInvoice(Core.Constants.DataContext.GenericFreightJobInvoice, true);
				}
			}
			return null;
		}

		protected DocumentWrapper[] GetWrappersForARInvoice(Core.Constants.DataContext dataContext, bool useDocBuilderInvoice)
		{
			ZQuery filter = new ZQuery();
			ZQuery jobUniqueRefQuery = new ZQuery(AccTransactionHeaderSchema.AH_JobNumber, DeliveryAgentOrgHeader.Consol.JK_UniqueConsignRef);
			jobUniqueRefQuery.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_JH, null);
			filter.AddToFilter(jobUniqueRefQuery);

			ZQuery shipmentFilter = new ZQuery();
			shipmentFilter.DefaultJoinCondition = JoinCondition.Or;
			foreach (ForwardingShipment shipment in DeliveryAgentOrgHeader.Shipments) // Or condition needs to be used here to search for JS_UniqueConsignRef correctly
			{
				jobUniqueRefQuery = new ZQuery(AccTransactionHeaderSchema.AH_JobNumber, shipment.JS_UniqueConsignRef);
				jobUniqueRefQuery.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_JH, SQLComparisonOperator.NotEqual, null);
				shipmentFilter.AddToFilter(jobUniqueRefQuery);
			}
			filter.AddToFilter(shipmentFilter, JoinCondition.Or);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_OH, SQLComparisonOperator.Equal, DeliveryAgentOrgHeader.PK);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);

			AccTransactionHeader[] transactionHeaders = (AccTransactionHeader[])Factory.Load(typeof(AccTransactionHeader), filter);

			if (transactionHeaders.Length > 0)
			{
				var wrappers = new List<DocumentWrapper>();
				for (int i = 0; i < transactionHeaders.Length; i++)
				{
					if (useDocBuilderInvoice)
					{
						BusinessObject invoice = Factory.Load(ObjectFactory.GetType(typeof(Accounting.Integration.IARInvoice)), transactionHeaders[i].PK);
						DocumentWrapper[] invoiceWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, invoice);
						if (invoiceWrappers != null)
						{
							wrappers.AddRange(invoiceWrappers);
						}
						else
						{
							wrappers.Add(DocumentWrapperFactory.CreateWrapper(dataContext, transactionHeaders[i]));
						}
					}
					else
					{
						DocumentWrapper invoiceWrapper = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.ARInvoice, transactionHeaders[i]);
						wrappers.Add(invoiceWrapper);
					}
				}
				return wrappers.Count == 0 ? null : wrappers.ToArray<DocumentWrapper>();
			}
			else
			{
				return null;
			}
		}

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menuToBeRun, BusinessContext businessContext, IStmMenuItem childCommand)
		{
			if (businessContext == BusinessContext.Shipment)
			{
				return (IDocumentSupportable[])DeliveryAgentOrgHeader.Shipments.ToArray(typeof(IDocumentSupportable));
			}
			else
			{
				return null;
			}
		}

		#endregion
	}
}
