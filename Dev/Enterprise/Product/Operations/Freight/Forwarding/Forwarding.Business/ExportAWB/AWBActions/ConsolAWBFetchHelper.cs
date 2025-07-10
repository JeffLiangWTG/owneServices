using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class ConsolAWBFetchHelper
	{
		public static void AddFetchHintsForConsols(List<ForwardingConsol> consols)
		{
			if (consols == null)
			{
				return;
			}

			foreach (ForwardingConsol consol in consols)
			{
				AddFetchHintsForConsol(consol);
			}

			foreach (ForwardingConsol consol in consols)
			{
				if (consol.IsAWBHeaderAccessible)
				{
					consol.Factory.AddFetchHint(ExportAWBHeaderSchema.EH_EH_Parent, consol.AWBHeader.PK);
					consol.Factory.AddFetchHint(ExportAWBAccountingInformationSchema.EA_EH, consol.AWBHeader.PK);
					consol.Factory.AddFetchHint(ExportAWBOtherChargesSchema.EO_EH, consol.AWBHeader.PK);
					consol.Factory.AddFetchHint(ExportAWBRateLineSchema.ER_EH, consol.AWBHeader.PK);
					consol.Factory.AddFetchHint(ExportAWBSpecialHandlingSchema.EP_EH, consol.AWBHeader.PK);
					consol.Factory.AddFetchHint(ExportAWBSecurityStatusLineSchema.EAS_EH, consol.AWBHeader.PK);
				}
			}
		}

		public static void AddFetchHintsForConsol(ForwardingConsol consol)
		{
			if (consol == null)
			{
				return;
			}

			consol.Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, consol.PK);
			consol.Factory.AddFetchHint(JobContainerSchema.JC_JK, consol.PK);
			consol.Factory.AddFetchHint(ExportAWBHeaderSchema.EH_ParentID, consol.PK);
			consol.Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, consol.PK);
			consol.Factory.AddFetchHint(JobMawbSchema.JM_ParentID, consol.PK);

			foreach (ForwardingShipment shipment in consol.Shipments)
			{
				consol.Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, shipment.JS_RL_NKOrigin);
				consol.Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, shipment.JS_RL_NKDestination);

				foreach (PackLine line in shipment.OuterPackLines)
				{
					consol.Factory.AddFetchHint(UNDGDataItemSchema.DI_ParentID, line.PK);
				}
			}

			foreach (ForwardingShipment shipment in consol.Shipments)
			{
				if (shipment.Origin != null)
				{
					consol.Factory.AddFetchHint(RefCountrySchema.RN_Code, shipment.Origin.RL_RN_NKCountryCode);
				}

				if (shipment.Destination != null)
				{
					consol.Factory.AddFetchHint(RefCountrySchema.RN_Code, shipment.Destination.RL_RN_NKCountryCode);
				}
			}
		}

		public static void AddFetchHintsForShipments(List<ForwardingShipment> shipments)
		{
			if (shipments == null)
			{
				return;
			}

			foreach (ForwardingShipment shipment in shipments)
			{
				shipment.Factory.AddFetchHint(ExportAWBHeaderSchema.EH_ParentID, shipment.PK);
				shipment.Factory.AddFetchHint(JobConsolTransportSchema.JW_ParentGUID, shipment.PK);
				shipment.Factory.AddFetchHint(JobCartageSchema.JJ_ParentID, shipment.PK);
				shipment.Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, shipment.JS_RL_NKOrigin);
				shipment.Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, shipment.JS_RL_NKDestination);

				if (shipment.JobHeader != null)
				{
					shipment.Factory.AddFetchHint(JobChargeSchema.JR_JH, shipment.JobHeader.PK);
				}

				if (shipment.ConsignorDocumentaryAddress.HasRealOrganisation)
				{
					FetchOrganization(shipment, shipment.ConsignorPK);
					FetchOrganizationClosestPort(shipment, shipment.ConsignorDocumentaryAddress);
				}

				if (shipment.ConsigneeDocumentaryAddress.HasRealOrganisation)
				{
					FetchOrganization(shipment, shipment.ConsigneePK);
					FetchOrganizationClosestPort(shipment, shipment.ConsigneeDocumentaryAddress);
				}
			}

			foreach (ForwardingShipment shipment in shipments)
			{
				if (shipment.Consignor != null)
				{
					FetchOrgAddressCapability(shipment, shipment.Consignor);
				}

				if (shipment.Consignee != null)
				{
					FetchOrgAddressCapability(shipment, shipment.Consignee);
				}
			}

			foreach (ForwardingShipment shipment in shipments)
			{
				FetchBusinessObjectsWithRelatedNotes(shipment);

				if (shipment.Origin != null)
				{
					shipment.Factory.AddFetchHint(RefCountrySchema.RN_Code, shipment.Origin.RL_RN_NKCountryCode);
				}

				if (shipment.Destination != null)
				{
					shipment.Factory.AddFetchHint(RefCountrySchema.RN_Code, shipment.Destination.RL_RN_NKCountryCode);
				}

				if (shipment.ConsignorDocumentaryAddress.HasRealAddress)
				{
					FetchOrganizationAddressRelatedPort(shipment, shipment.ConsignorDocumentaryAddress);
					FetchContactDocuments(shipment, shipment.ConsignorDocumentaryAddress);
				}

				if (shipment.ConsigneeDocumentaryAddress.HasRealAddress)
				{
					FetchOrganizationAddressRelatedPort(shipment, shipment.ConsigneeDocumentaryAddress);
					FetchContactDocuments(shipment, shipment.ConsigneeDocumentaryAddress);
				}
			}
		}

		static void FetchOrgAddressCapability(ForwardingShipment shipment, OrgHeader header)
		{
			foreach (var address in header.Addresses)
			{
				shipment.Factory.AddFetchHint(OrgAddressCapabilitySchema.PZ_OA, address.PK);
			}
		}

		static void FetchContactDocuments(ForwardingShipment shipment, JobDocAddress documentaryAddress)
		{
			if (documentaryAddress.Organisation != null)
			{
				foreach (var contact in documentaryAddress.Organisation.Contacts)
				{
					shipment.Factory.AddFetchHint(OrgDocumentSchema.OD_OC, contact.PK);
				}
			}
		}

		static void FetchOrganizationAddressRelatedPort(ForwardingShipment shipment, JobDocAddress documentaryAddress)
		{
			if (documentaryAddress.Address != null)
			{
				shipment.Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, documentaryAddress.Address.OA_RL_NKRelatedPortCode);
			}
		}

		static void FetchOrganizationClosestPort(ForwardingShipment shipment, JobDocAddress documentaryAddress)
		{
			if (documentaryAddress.Organisation != null)
			{
				shipment.Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, documentaryAddress.Organisation.OH_RL_NKClosestPort);
			}
		}

		static void FetchOrganization(ForwardingShipment shipment, ZGuid organizationPK)
		{
			shipment.Factory.AddFetchHint(OrgAddressSchema.OA_OH, organizationPK);
			shipment.Factory.AddFetchHint(OrgMiscServSchema.OM_OH, organizationPK);
			shipment.Factory.AddFetchHint(OrgContactSchema.OC_OH, organizationPK);
			shipment.Factory.AddFetchHint(OrgCountryDataSchema.OV_OH_OrgHeader, organizationPK);
		}

		static void FetchBusinessObjectsWithRelatedNotes(ForwardingShipment shipment)
		{
			var businessObjects = shipment.BusinessObjectsWithRelatedNotes;

			foreach (var businessObject in businessObjects)
			{
				shipment.Factory.AddFetchHint(StmNoteSchema.ST_ParentID, businessObject.PK);
			}
		}
	}
}
