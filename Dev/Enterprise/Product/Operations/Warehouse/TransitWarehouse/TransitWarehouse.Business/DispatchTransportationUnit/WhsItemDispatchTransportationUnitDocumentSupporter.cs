using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemDispatchTransportationUnitDocumentSupporter : DocumentSupporter
	{
		public WhsItemDispatchTransportationUnitDocumentSupporter(WhsItemDispatchTransportationUnit transitDispatchTransportationUnit)
			: base(transitDispatchTransportationUnit)
		{
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;

			if (dataContext == Constants.DataContext.GenericFreightJob && commandBeingRun != null)
			{
				result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, WhsItemDispatchTransportationUnit);
			}

			return result;
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		#region GetSupportedDataContexts

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Constants.DataContext.GenericFreightJob };
		}

		#endregion

		#region BusinessContext

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.TransitDispTranspUnt; }
		}

		#endregion

		#region SecurityCheckpoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.WhsItemDispatchTransportationUnitCustomizeDocuments;

		#endregion

		#region GetContactOrganisation

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			OrgHeaderContact orgHeaderContact = null;
			if (contactType == ContactType.TransitWarehouse)
			{
				orgHeaderContact = new OrgHeaderContact(WhsItemDispatchTransportationUnit.Warehouse.WarehouseAddress.Header, WhsItemDispatchTransportationUnit.Warehouse.WarehouseAddress);
			}
			else if (contactType == ContactType.LocalTransport)
			{
				orgHeaderContact = WhsItemDispatchTransportationUnit.TransportCompany.GetOrgHeaderContact();
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
				case BusinessContext.TransitDspConsignmnt:
					result = WhsItemDispatchTransportationUnit.PackageStates
						.Select(p => p.DispatchConsignment)
						.Where(c => c != null)
						.OrderBy(c => string.IsNullOrWhiteSpace(c.HouseBillNumber) ? c.WDC_ConsignmentID : c.HouseBillNumber)
						.Distinct()
						.ToArray<IDocumentSupportable>();
					break;
				case BusinessContext.TransitDspLoadList:
					result = WhsItemDispatchTransportationUnit.DispatchLoadLists.ToArray<IDocumentSupportable>();
					break;
				default:
					result = base.GetChildCollection(menuToBeRun, BusinessContext, childCommand);
					break;
			}

			return result;
		}

		#endregion

		#region SupportedChildBusinessContexts

		public override BusinessContext[] SupportedChildBusinessContexts => new[] { BusinessContext.TransitDspConsignmnt, BusinessContext.TransitDspLoadList };

		#endregion

		#region WhsItemDispatchTransportationUnit

		protected WhsItemDispatchTransportationUnit WhsItemDispatchTransportationUnit
		{
			get { return (WhsItemDispatchTransportationUnit)BusinessObject; }
		}

		#endregion
	}
}
