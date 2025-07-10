using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentDocumentSupporter : DocumentSupporter
	{
		public DtbConsignmentDocumentSupporter(DtbConsignment consignment)
			: base(consignment)
		{
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;
			if (dataContext == Constants.DataContext.GenericFreightJobServices)
			{
				result = Consignment.GetServiceWrappersForDocBuilder(((IHaveServices)Consignment).ServiceParent, Core.Constants.DataContext.GenericFreightJob);
			}
			else if (dataContext == Constants.DataContext.GenericFreightJob && commandBeingRun != null)
			{
				result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Consignment);
			}
			return result;
		}

		#region ShowReasonForNotPrintingCore

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		#endregion

		#region BusinessContext

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.LTConsignment; }
		}

		#endregion

		#region CustomisationSecurityCheckpoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.DtbConsignmentCustomiseDocuments; }
		}

		#endregion

		#region GetContactOrganisation

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			return GetDocumentDeliveryContact(contactType, Consignment);
		}

		public static IDocumentDeliveryContact GetDocumentDeliveryContact(IContactType contactType, DtbConsignment consignment)
		{
			if (contactType == ContactType.Consignor)
			{
				return CreateOrgHeaderContactFromJobDocAddress(consignment.PickupAddress?.Address, OrganisationTypes.Consignor);
			}
			else if (contactType == ContactType.Consignee)
			{
				return CreateOrgHeaderContactFromJobDocAddress(consignment.DeliveryAddress?.Address, OrganisationTypes.Consignee);
			}
			else if (contactType == ContactType.LocalClient)
			{
				var localClientJobHeader = consignment.Job?.LocalCharges;
				if (localClientJobHeader != null)
				{
					return new OrgHeaderContact(localClientJobHeader, localClientJobHeader.MainAddress);
				}
			}

			return null;
		}

		static OrgHeaderContact CreateOrgHeaderContactFromJobDocAddress(JobDocAddress jobDocAddress, OrganisationTypes targetOrganisationType)
		{
			var orgHeader = jobDocAddress?.Organisation;

			if (orgHeader?.OrganisationTypes.HasFlag(targetOrganisationType) ?? false)
			{
				return new OrgHeaderContact(orgHeader, jobDocAddress.Address);
			}

			return null;
		}

		#endregion

		internal DtbConsignment Consignment
		{
			get { return (DtbConsignment)BusinessObject; }
		}

		#region Implementation

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Constants.DataContext.GenericFreightJob, Constants.DataContext.GenericFreightJobServices };
		}

		#endregion
	}
}
