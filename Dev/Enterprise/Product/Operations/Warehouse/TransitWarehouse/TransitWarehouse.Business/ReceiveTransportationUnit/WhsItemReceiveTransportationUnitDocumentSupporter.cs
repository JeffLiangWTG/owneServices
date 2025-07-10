using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemReceiveTransportationUnitDocumentSupporter : DocumentSupporter
	{
		public WhsItemReceiveTransportationUnitDocumentSupporter(WhsItemReceiveTransportationUnit receiveTransportationUnit)
			: base(receiveTransportationUnit)
		{
		}

		#region BusinessContext

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.TransitRecTranspUnt; }
		}

		#endregion

		#region SecurityCheckpoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.WhsItemReceiveTransportationUnitCustomizeDocuments;

		#endregion

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;
			if (commandBeingRun != null)
			{
				if (dataContext == Constants.DataContext.GenericFreightJob)
				{
					result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, TransportationUnit);
				}
				else if (dataContext == Constants.DataContext.GenericNewPackageID || dataContext == Constants.DataContext.GenericNewPackageIDs1Doc)
				{
					var matchedLog = GetServiceRequestEvent(TransportationUnit);
					if (matchedLog != null)
					{
						var userRequestedLabelQty = GetUserRequestedPackageIDQuantity(matchedLog);
						result = TransportPackageLabelHelper.CreateDocumentWrapperForNewPackageIDs(TransportationUnit,
							userRequestedLabelQty,
							dataContext == Constants.DataContext.GenericNewPackageIDs1Doc);

						CancelServiceRequestEvent(matchedLog);
					}
					else
					{
						result = TransportPackageLabelHelper.CreateDocumentWrapperForNewPackageID(TransportationUnit);
					}
				}
			}

			return result;
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Constants.DataContext.GenericFreightJob };
		}

		#region GetUserRequestedPackageIDQuantity

		static StmALog GetServiceRequestEvent(WhsItemReceiveTransportationUnit transportationUnit)
		{
			StmALog matchedLog = null;
			var currentUser = GlbStaff.CurrentUser;
			if (currentUser != null)
			{
				var query = new ZQuery(StmALogSchema.SL_GS_NKUser, currentUser.GS_Code);
				query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
				query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "TYP=Document|RES=Package Label (Generate New ID)|");

				matchedLog = transportationUnit.GetLogs().MostRecentLogByEventTime(Events.ServiceRequested, query);
			}

			return matchedLog;
		}

		static int GetUserRequestedPackageIDQuantity(StmALog log)
		{
			var qty = 1;
			if (log != null)
			{
				var regex = new Regex("QTY=[0-9]+");
				var matchedValue = regex.Match(log.SL_Reference).Value;
				if (!string.IsNullOrEmpty(matchedValue))
				{
					qty = Convert.ToInt32(matchedValue.Replace("QTY=", ""), CultureInfo.InvariantCulture);
				}
			}

			return qty;
		}

		void CancelServiceRequestEvent(StmALog log)
		{
			log.Cancel();
			log.Factory.Save();
		}

		#endregion

		#region Consignment

		protected WhsItemReceiveTransportationUnit TransportationUnit
		{
			get { return (WhsItemReceiveTransportationUnit)BusinessObject; }
		}

		#endregion

		#region GetContactOrganisation

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			OrgHeaderContact orgHeaderContact = null;
			if (contactType == ContactType.TransitWarehouse)
			{
				orgHeaderContact = new OrgHeaderContact(TransportationUnit.Warehouse.WarehouseAddress.Header, TransportationUnit.Warehouse.WarehouseAddress);
			}
			else if (contactType == ContactType.LocalTransport)
			{
				orgHeaderContact = TransportationUnit.TransportCompany.GetOrgHeaderContact();
			}

			return orgHeaderContact;
		}

		#endregion

		#region GetChildCollection

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menuToBeRun, BusinessContext businessContext, IStmMenuItem childCommand)
		{
			IDocumentSupportable[] result;
			switch (businessContext)
			{
				case BusinessContext.TransitRcvConsignmnt:
					result = ReceiveTransportationUnit.ReceiveConsignments.ToArray<IDocumentSupportable>();
					break;
				case BusinessContext.TransitReceiveASN:
					result = ReceiveTransportationUnit.ReceiveASNs.ToArray<IDocumentSupportable>();
					break;
				default:
					result = base.GetChildCollection(menuToBeRun, BusinessContext, childCommand);
					break;
			}

			return result;
		}

		#endregion

		#region SupportedChildBusinessContexts

		public override BusinessContext[] SupportedChildBusinessContexts => new[] { BusinessContext.TransitRcvConsignmnt, BusinessContext.TransitReceiveASN };

		#endregion

		#region ReceiveTransportationUnit

		protected WhsItemReceiveTransportationUnit ReceiveTransportationUnit
		{
			get { return (WhsItemReceiveTransportationUnit)BusinessObject; }
		}

		#endregion
	}
}
