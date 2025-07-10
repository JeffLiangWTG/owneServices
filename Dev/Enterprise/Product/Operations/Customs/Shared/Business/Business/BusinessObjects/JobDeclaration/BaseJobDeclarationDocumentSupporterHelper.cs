using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	static class BaseJobDeclarationDocumentSupporterHelper
	{
		public static IDocumentDeliveryContact GetContactOrganisation(BaseJobDeclaration declaration, IContactType contact)
		{
			IDocumentDeliveryContact result = null;

			if (contact == ContactType.Consignee)
			{
				result = new OrgHeaderContact(declaration.Consignee, declaration.Consignor, null);
			}
			else if (contact == ContactType.LocalTransport)
			{
				result = new OrgHeaderContact(GetCartageOrganisation(declaration), null);
			}
			else if (contact == ContactType.Consignor)
			{
				result = new OrgHeaderContact(declaration.Consignor, declaration.Consignee, null);
			}
			else if (contact == ContactType.ExportFreightAgent || contact == ContactType.ImportFreightAgent)
			{
				result = new OrgHeaderContact(declaration.Forwarder, null);
			}
			else if (contact == ContactType.Receivables)
			{
				if (declaration.Job != null)
				{
					result = new OrgHeaderContact(declaration.Job.LocalCharges, null);
				}
				else
				{
					var jobHeaderForeignKeyLink = GetJobHeaderForeignKeyLink(declaration);

					if (jobHeaderForeignKeyLink != null)
					{
						result = new OrgHeaderContact(jobHeaderForeignKeyLink.LocalCharges, null);
					}
				}
			}
			else if (contact == ContactType.CTO)
			{
				if (!declaration.ContainerTerminalOperatorDocAddress.E2_AddressOverride)
				{
					result = new OrgHeaderContact(declaration.ContainerTerminalOperatorDocAddress.Organisation, null);
				}
			}
			else if (contact == ContactType.ShippingLine)
			{
				result = new OrgHeaderContact(declaration.ShippingLine, null);
			}
			else if (contact == ContactType.ImportBroker || contact == ContactType.ExportBroker)
			{
				var shipment = declaration.Shipment;
				if (shipment != null)
				{
					result = shipment.IsExport() ? new OrgHeaderContact(shipment.ExportBroker, null) : new OrgHeaderContact(shipment.ImportBroker, null);
				}
				else
				{
					result = declaration.Company != null ? new OrgHeaderContact(declaration.Company.OrgProxy, null) : null;
				}
			}
			else if (contact == ContactType.LocalClient)
			{
				var job = declaration.Job;
				if (job != null)
				{
					result = new OrgHeaderContact(job.LocalCharges, job.LocalChargesAddr);
				}
			}
			else if (contact == ContactType.Declarant)
			{
				if (declaration.DeclarantAddress != null)
				{
					result = new OrgHeaderContact(declaration.DeclarantAddress.Header, declaration.DeclarantAddress);
				}
			}
			else if (contact == ContactType.ControllingCustomer)
			{
				if (declaration.ControllingCustomer is { } controllingCustomer)
				{
					result = new OrgHeaderContact(controllingCustomer, null);
				}
			}

			return result;
		}

		static OrgHeader GetCartageOrganisation(BaseJobDeclaration declaration)
		{
			var branchPK = declaration.RegistryBranchPK;
			var result = declaration.DeliveryOrPickupCartageCo;
			if (result == null)
			{
				if (declaration.TransportMode == Core.Constants.TransportModes.Sea)
				{
					switch (declaration.JE_ContainerMode)
					{
						case Core.Constants.ContainerModes.Containerised:
							result = GetCartageCompanyFromContainerType(declaration);
							break;

						case Core.Constants.ContainerModes.FCL:
							var defaultFCLCartageCompanyPK = FreightDataRegistry.Instance.FCLCartageCompany.GetValueWithoutFallback(Guid.Empty, branchPK, Guid.Empty);
							if (defaultFCLCartageCompanyPK != Guid.Empty)
							{
								result = declaration.Factory.Load<OrgHeader>(defaultFCLCartageCompanyPK);
							}
							break;

						case Core.Constants.ContainerModes.Bulk:
						case Core.Constants.ContainerModes.Liquid:
							result = null;
							break;

						default:
							var defaultLCLCartageCompanyPK = FreightDataRegistry.Instance.LCLCartageCompany.GetValueWithoutFallback(Guid.Empty, branchPK, Guid.Empty);
							if (defaultLCLCartageCompanyPK != Guid.Empty)
							{
								result = declaration.Factory.Load<OrgHeader>(defaultLCLCartageCompanyPK);
							}
							break;
					}
				}
				else if (declaration.TransportMode == Core.Constants.TransportModes.Air)
				{
					var defaultAIRCartageCompanyPK = FreightDataRegistry.Instance.AIRCartageCompany.GetValueWithoutFallback(Guid.Empty, branchPK, Guid.Empty);
					if (defaultAIRCartageCompanyPK != Guid.Empty)
					{
						result = declaration.Factory.Load<OrgHeader>(defaultAIRCartageCompanyPK);
					}
				}
				else
				{
					result = null;
				}
			}
			return result;
		}

		static OrgHeader GetCartageCompanyFromContainerType(BaseJobDeclaration declaration)
		{
			var companyPK = declaration.RegistryCompanyPK;
			var branchPK = declaration.RegistryBranchPK;
			OrgHeader result = null;
			ZString containerType = "";
			if (declaration.HasCusContainers)
			{
				containerType = declaration.CusContainers[0].CO_FCL_LCL_AIR;
			}
			if (containerType == Core.Constants.ContainerModes.FCL || containerType == Core.Constants.ContainerModes.FCLMixedShipper)
			{
				var defaultFCLCartageCompanyPK = FreightDataRegistry.Instance.FCLCartageCompany.GetValueWithoutFallback(Guid.Empty, branchPK, Guid.Empty);
				if (defaultFCLCartageCompanyPK != Guid.Empty)
				{
					result = declaration.Factory.Load<OrgHeader>(defaultFCLCartageCompanyPK);
				}
			}
			else
			{
				var defaultLCLCartageCompanyPK = FreightDataRegistry.Instance.LCLCartageCompany.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
				if (defaultLCLCartageCompanyPK != Guid.Empty)
				{
					result = declaration.Factory.Load<OrgHeader>(defaultLCLCartageCompanyPK);
				}
			}
			return result;
		}

		public static JobHeader GetJobHeaderForeignKeyLink(BaseJobDeclaration declaration)
		{
			var filter = new ZQuery();
			filter.AddToFilter(JobHeaderSchema.JH_ParentID, declaration.PK);
			filter.AddToFilter(JobHeaderSchema.JH_GC, declaration.Branch.GB_GC);
			filter.AddToFilter(JobHeaderSchema.JH_ParentTableCode, JobDeclarationSchema.Constants.Prefix);
			return declaration.Factory.LoadTop1<JobHeader>(filter);
		}
	}
}
