using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbBookingConsignmentDocumentSupporter : DtbTransportDocumentSupporter<DtbBookingConsignment>
	{
		public DtbBookingConsignmentDocumentSupporter(DtbBookingConsignment consignment)
			: base(consignment)
		{
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;
			if (dataContext == Core.Constants.DataContext.GenericFreightJobServices)
			{
				result = Consignment.GetServiceWrappersForDocBuilder(((IHaveServices)Consignment).ServiceParent, Core.Constants.DataContext.GenericFreightJob);
			}
			else if (dataContext == Constants.DataContext.GenericFreightJob && commandBeingRun != null)
			{
				result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Transport);
			}
			return result;
		}

		#region BusinessContext

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.DtbConsignment; }
		}

		#endregion

		#region CustomisationSecurityCheckpoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.DtbBookingConsignmentCustomiseDocuments; }
		}

		#endregion

		#region GetContactOrganisation

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			return GetDocumentDeliveryContact(contactType, Consignment);
		}

		public static IDocumentDeliveryContact GetDocumentDeliveryContact(IContactType contactType, DtbBookingConsignment consignment)
		{
			OrgHeaderContact orgHeaderContact = null;

			if (contactType == ContactType.Consignor)
			{
				var instruction = consignment.PickupInstruction;
				if (instruction != null)
				{
					var address = instruction.Address;
					if (address != null)
					{
						var organisation = address.Organisation;
						if (organisation != null)
						{
							if (organisation.OrganisationTypes == OrganisationTypes.Consignor)
							{
								orgHeaderContact = new OrgHeaderContact(organisation, address.Address);
							}
						}
					}
				}
			}

			return orgHeaderContact;
		}

		#endregion

		#region ShowReasonForNotPrintingCore

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return true;
		}

		#endregion

		#region GetBODocDataProvidersNotFoundMessage

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var message = base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);

			if (dataContextValue.DataContext == Constants.DataContext.GenericFreightJobServices)
			{
				message = Res.GetString("GetBODocDataProvidersNotFoundMessage-NoService", "Cannot find Job Service.");
			}

			return message;
		}

		#endregion

		internal DtbBookingConsignment Consignment
		{
			get { return Transport; }
		}
	}
}
