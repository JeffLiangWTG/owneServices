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
	public class WhsItemDispatchConsignmentDocumentSupporter : DocumentSupporter
	{
		public WhsItemDispatchConsignmentDocumentSupporter(WhsItemDispatchConsignment receiveConsignment)
			: base(receiveConsignment)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.TransitDspConsignmnt; }
		}

		#region SecurityCheckpoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.WhsItemDispatchConsignmentCustomizeDocuments;

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
					PkgPackageJob.CheckAndFixSequence(Consignment, Consignment.OuterPackages);
					result = TransportPackageLabelHelper.CreateDocumentWrapperForNewPackageID(Consignment);
				}
				else if (dataContext == Constants.DataContext.GenericNewAndExistingPackageIDs || dataContext == Constants.DataContext.GenericNewAndExistingPackageIDs1Doc)
				{
					var outerPackages = Consignment.OuterPackages;
					PkgPackageJob.CheckAndFixSequence(Consignment, outerPackages);
					result = TransportPackageLabelHelper.CreateDocumentWrapperForNewAndExistingPackageIDs(Consignment,
						outerPackages,
						outerPackages.Where(p => p.KP_PackageID.IsEmpty),
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

		protected WhsItemDispatchConsignment Consignment
		{
			get { return (WhsItemDispatchConsignment)BusinessObject; }
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
				var consignorContacts = Consignment.PackageStates.Select(p => p.ReceiveConsignment?.ConsignorDocAddress.GetOrgHeaderContact()).Where(c => c != null).ToArray();
				if (consignorContacts.GroupBy(c => c.OrgAddress).Count() == 1)
				{
					orgHeaderContact = consignorContacts.FirstOrDefault();
				}
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
