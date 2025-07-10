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
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemReceiveConsignmentDocumentSupporter : DocumentSupporter
	{
		public WhsItemReceiveConsignmentDocumentSupporter(WhsItemReceiveConsignment receiveConsignment)
			: base(receiveConsignment)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.TransitRcvConsignmnt; }
		}

		#region SecurityCheckpoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.WhsItemReceiveConsignmentCustomizeDocuments;

		#endregion

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;
			if (commandBeingRun != null)
			{
				if (dataContext == Constants.DataContext.GenericFreightJob)
				{
					result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Consignment);
				}
				else if (dataContext == Constants.DataContext.GenericNewPackageID)
				{
					PkgPackageJob.CheckAndFixSequence(Consignment);
					result = TransportPackageLabelHelper.CreateDocumentWrapperForNewPackageID(Consignment);
				}
				else if (dataContext == Constants.DataContext.GenericNewPackageIDs || dataContext == Constants.DataContext.GenericNewPackageIDs1Doc)
				{
					PkgPackageJob.CheckAndFixSequence(Consignment);
					result = TransportPackageLabelHelper.CreateDocumentWrapperForNewPackageIDs(Consignment,
						dataContext == Constants.DataContext.GenericNewPackageIDs1Doc);
				}
				else if (dataContext == Constants.DataContext.GenericNewAndExistingPackageIDs || dataContext == Constants.DataContext.GenericNewAndExistingPackageIDs1Doc)
				{
					PkgPackageJob.CheckAndFixSequence(Consignment);
					var consignmentPackages = Consignment.OuterPackages.Select(ps => ps.Package);
					result = TransportPackageLabelHelper.CreateDocumentWrapperForNewAndExistingPackageIDs(
						Consignment,
						consignmentPackages,
						returnSingleDocument: dataContext == Constants.DataContext.GenericNewAndExistingPackageIDs1Doc);
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

		#region Consignment

		protected WhsItemReceiveConsignment Consignment
		{
			get { return (WhsItemReceiveConsignment)BusinessObject; }
		}

		#endregion

		#region GetContactOrganisation

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			OrgHeaderContact orgHeaderContact = null;
			if (contactType == ContactType.TransitWarehouse)
			{
				orgHeaderContact = new OrgHeaderContact(Consignment.Warehouse.WarehouseAddress.Header, Consignment.Warehouse.WarehouseAddress);
			}
			else if (contactType == ContactType.Consignor)
			{
				orgHeaderContact = Consignment.ConsignorDocAddress.GetOrgHeaderContact();
			}
			else if (contactType == ContactType.Consignee)
			{
				orgHeaderContact = Consignment.ConsigneeDocAddress.GetOrgHeaderContact();
			}
			else if (contactType == ContactType.ExportFreightAgent
					|| contactType == ContactType.ImportFreightAgent
					|| contactType == ContactType.ImportSeaFreightAgent
					|| contactType == ContactType.ImportAirFreightAgent
					|| contactType == ContactType.ExportSeaFreightAgent
					|| contactType == ContactType.ExportAirFreightAgent)
			{
				orgHeaderContact = Consignment.BookingPartyDocAddress.GetOrgHeaderContact();
			}
			else if (contactType == ContactType.LocalClient)
			{
				if (Consignment.JobHeader?.LocalZAddressWithContact?.OrgHeader != null)
				{
					var localClient = (OrgHeader)Consignment.JobHeader.LocalZAddressWithContact.OrgHeader;
					var localClientAddress = (OrgAddress)Consignment.JobHeader.LocalZAddressWithContact.OrgAddress;
					orgHeaderContact = new OrgHeaderContact(localClient, localClientAddress);
				}
				else
				{
					orgHeaderContact = Consignment.ClientRequestedBillToPartyDocAddress.GetOrgHeaderContact();
				}
			}

			return orgHeaderContact;
		}

		#endregion
	}
}
