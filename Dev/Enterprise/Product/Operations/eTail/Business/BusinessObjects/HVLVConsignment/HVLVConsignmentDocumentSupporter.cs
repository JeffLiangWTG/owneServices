using System.Collections.Generic;
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
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentDocumentSupporter : DocumentSupporter
	{
		public HVLVConsignmentDocumentSupporter(HVLVConsignment consignment) : base(consignment) { }

		public HVLVConsignmentDocumentSupporter(HVLVItem item) : this(item.Consignment)
		{
			this.item = item;
		}

		readonly HVLVItem item;

		protected HVLVConsignment Consignment => (HVLVConsignment)BusinessObject;

		public override BusinessContext BusinessContext => BusinessContext.HVLVConsignment;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.HVLVBookingHeaderCustomiseDocuments;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard-coded document")]
		public const string HVLVDeliveryLabelMenuItemName = "HVLV Delivery Label";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard-coded document")]
		public const string HVLVRoutingLabelMenuItemName = "HVLV Routing Label";

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (commandBeingRun != null
			&& (commandBeingRun.SU_MenuName.EqualsIgnoringCase(HVLVDeliveryLabelMenuItemName)
				|| commandBeingRun.SU_MenuName.EqualsIgnoringCase(HVLVRoutingLabelMenuItemName)))
			{
				var result = new List<DocumentWrapper>();
				if (item != null)
				{
					result.AddRange(DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Consignment, item));
				}
				else
				{
					foreach (HVLVItem item in Consignment.Items)
					{
						result.AddRange(DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Consignment, item));
					}
				}

				return result.ToArray();
			}

			return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Consignment);
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			IDocumentDeliveryContact contact = null;
			if (contactType == ContactType.Consignee && Consignment.ConsigneeIsOrganisation)
			{
				contact = new OrgHeaderContact(Consignment.ConsigneeAddress.Header, Consignment.ConsigneeAddress);
			}
			else if (contactType == ContactType.Consignor && Consignment.ShipperIsOrganisation)
			{
				contact = new OrgHeaderContact(Consignment.ShipperAddress.Header, Consignment.ShipperAddress);
			}
			else if (contactType == ContactType.TransportServices && !Consignment.HVC_OH_LastMileCarrier.IsEmpty)
			{
				contact = new OrgHeaderContact(Consignment.LastMileCarrier, null);
			}
			else if (contactType == ContactType.ImportDepot && !Consignment.HVC_OA_DestinationDepot.IsEmpty)
			{
				contact = new OrgHeaderContact(Consignment.DestinationDepot.Header, Consignment.DestinationDepot);
			}
			return contact ?? base.GetContactOrganisation(menuName, contactType, direction);
		}

		public override IDocAddress GetOverriddenDeliveryDetails(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			JobDocAddress jobDocAddress = null;
			if (contactType == ContactType.Consignee && !Consignment.ConsigneeIsOrganisation)
			{
				jobDocAddress = Factory.New<JobDocAddress>();
				jobDocAddress.E2_AddressOverride = true;
				jobDocAddress.E2_CompanyName = Consignment.HVC_ConsigneeName.ToUpper();
				jobDocAddress.E2_Address1 = Consignment.HVC_ConsigneeAddress1.ToUpper();
				jobDocAddress.E2_Address2 = Consignment.HVC_ConsigneeAddress2.ToUpper();
				jobDocAddress.E2_Contact = Consignment.HVC_ConsigneeContact.ToUpper();
				jobDocAddress.E2_City = Consignment.HVC_ConsigneeCity.ToUpper();
				jobDocAddress.E2_State = Consignment.HVC_ConsigneeState.ToUpper();
				jobDocAddress.E2_Postcode = Consignment.HVC_ConsigneePostcode.ToUpper();
				jobDocAddress.E2_RN_NKCountryCode = Consignment.HVC_RN_NKConsigneeCountryCode;
				jobDocAddress.E2_Phone = Consignment.HVC_ConsigneePhone.ToUpper();
				jobDocAddress.E2_Mobile = Consignment.HVC_ConsigneeMobile.ToUpper();
				jobDocAddress.E2_Fax = Consignment.HVC_ConsigneeFax.ToUpper();
				jobDocAddress.E2_Email = Consignment.HVC_ConsigneeEmail.ToUpper();
			}
			else if (contactType == ContactType.Consignor && !Consignment.ShipperIsOrganisation)
			{
				jobDocAddress = Factory.New<JobDocAddress>();
				jobDocAddress.E2_AddressOverride = true;
				jobDocAddress.E2_CompanyName = Consignment.HVC_ShipperName.ToUpper();
				jobDocAddress.E2_Address1 = Consignment.HVC_ShipperAddress1.ToUpper();
				jobDocAddress.E2_Address2 = Consignment.HVC_ShipperAddress2.ToUpper();
				jobDocAddress.E2_Contact = Consignment.HVC_ShipperContact.ToUpper();
				jobDocAddress.E2_City = Consignment.HVC_ShipperCity.ToUpper();
				jobDocAddress.E2_State = Consignment.HVC_ShipperState.ToUpper();
				jobDocAddress.E2_Postcode = Consignment.HVC_ShipperPostcode.ToUpper();
				jobDocAddress.E2_RN_NKCountryCode = Consignment.HVC_RN_NKShipperCountryCode;
				jobDocAddress.E2_Phone = Consignment.HVC_ShipperPhone.ToUpper();
				jobDocAddress.E2_Mobile = Consignment.HVC_ShipperMobile.ToUpper();
				jobDocAddress.E2_Fax = Consignment.HVC_ShipperFax.ToUpper();
				jobDocAddress.E2_Email = Consignment.HVC_ShipperEmail.ToUpper();
			}
			return jobDocAddress ?? base.GetOverriddenDeliveryDetails(menuName, contactType, direction);
		}

		protected override DataContext[] GetSupportedDataContexts() => new[] { DataContext.GenericFreightJob };
	}
}
