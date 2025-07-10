using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Integration;

namespace Enterprise.MasterFiles.Module
{
	public class AssignWhsPutawayGroupMethodApplicator : OperationalActionMethodApplicator
	{
		public AssignWhsPutawayGroupMethodApplicator(string name, BusinessObjectFactory factory)
			: base(name, factory)
		{
		}

		#region WhsPutawayGroupPK

		[List("Lookups.WhsPutawayGroups")]
		[ResourceStringData("AssignWhsPutawayGroupMethodApplicator|WhsPutawayGroupPK", Caption = "Warehouse Putaway Group", ShortCaption = "Whs. Putaway Group")]
		public ZGuid WhsPutawayGroupPK
		{
			get { return whsPutawayGroupPK; }
			set
			{
				SetNonPersistentPropertyValue(WhsPutawayGroupPKInfo, ref whsPutawayGroupPK, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateWhsPutawayGroupPK();
				}
			}
		}

		public ZPropertyInfo WhsPutawayGroupPKInfo
		{
			get { return GetZPropertyInfo(nameof(WhsPutawayGroupPK)); }
		}

		ZGuid whsPutawayGroupPK;

		#endregion

		#region WarehousePK

		[List("Lookups.Warehouses")]
		[ResourceStringData("AssignWhsPutawayGroupMethodApplicator|WarehousePK", Caption = "Warehouse")]
		public ZGuid WarehousePK
		{
			get { return warehousePK; }
			set
			{
				SetNonPersistentPropertyValue(WarehousePKInfo, ref warehousePK, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateWarehousePK();
				}
			}
		}

		public ZPropertyInfo WarehousePKInfo
		{
			get { return GetZPropertyInfo(nameof(WarehousePK)); }
		}

		ZGuid warehousePK;

		#endregion

		#region ApplyCore

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] products)
		{
			Validation.ValidateAll();

			if (!HasErrors)
			{
				log.SetSectionProgressMax(products.Length);

				var isUnassigning = WhsPutawayGroupPK.IsEmpty;
				foreach (OrgSupplierPart product in products)
				{
					log.BumpSectionProgress();
					if (product.RelatedOrganisations.Count == 0)
					{
						log.Notify(OperationalActionLogErrorLevel.Error, Res.GetString("8f642c9c-b1f9-4988-8a08-9d30b64a614a", "Product {0} does not have any Related Organizations.", product.OP_PartNum));
					}
					else
					{
						UpdateProductParamsAndLog(log, isUnassigning, product);
					}
				}
			}
			else
			{
				log.Notify(OperationalActionLogErrorLevel.Error, Res.GetString("bde3d6b9-293b-43d5-9527-6275c69a3023", "Enter a Warehouse."));
			}
		}

		void UpdateProductParamsAndLog(IOperationalActionSectionLog log, bool isUnassigning, OrgSupplierPart product)
		{
			var paramsByWhsAndClients = ObjectFactory.New<IWhsProductParamsByWhsAndClientCollection>(product, product.Factory);
			var relatedOrganisationPKs = product.RelatedOrganisations.Cast<OrgPartRelation>().Where(r => r.OU_Relationship == "OWN" || r.OU_Relationship == "BTH").Select(ro => ro.OU_OH);
			var relatedOrgParams = paramsByWhsAndClients.FindWhsProductParamsByWhsAndClients(relatedOrganisationPKs, WarehousePK);

			if (isUnassigning && paramsByWhsAndClients.Count <= 0)
			{
				log.Notify(OperationalActionLogErrorLevel.Informational, Res.GetString("ae3f9f95-d8bc-47f2-acd1-58ce8f1ef324", "No Product/Warehouse/Client parameters to remove Warehouse Putaway Group from for {0}.", product.OP_PartNum));
			}
			else if (isUnassigning && paramsByWhsAndClients.Count > 0)
			{
				relatedOrgParams.ForEach(p => p.W3_WPG_PutawayGroup = ZGuid.Empty);
				log.Notify(OperationalActionLogErrorLevel.Informational, Res.GetString("5a7b2e8d-2087-4f8b-bd74-35ba69a1b2c6", "Unassigned Warehouse Putaway Group from {0}.", product.OP_PartNum));
			}
			else
			{
				CreateOrUpdateThenLogWhsProductParamsPutwayGroup(log, product, paramsByWhsAndClients, relatedOrganisationPKs, relatedOrgParams);
			}
		}

		void CreateOrUpdateThenLogWhsProductParamsPutwayGroup(IOperationalActionSectionLog log, OrgSupplierPart product, IWhsProductParamsByWhsAndClientCollection paramsByWhsAndClients, IEnumerable<ZGuid> relatedOrganisationPKs, IEnumerable<IWhsProductParamsByWhsAndClient> relatedOrgParams)
		{
			var createdParams = 0;
			var assignedParams = 0;
			foreach (var relatedOrganisationPK in relatedOrganisationPKs)
			{
				var matchedParam = relatedOrgParams.FirstOrDefault(param => param.W3_OH == relatedOrganisationPK);
				if (matchedParam == null)
				{
					var newParam = (IWhsProductParamsByWhsAndClient)paramsByWhsAndClients.AddNew();
					newParam.W3_OH = relatedOrganisationPK;
					newParam.W3_WW = WarehousePK;
					newParam.W3_WPG_PutawayGroup = WhsPutawayGroupPK;

					createdParams++;
				}
				else
				{
					matchedParam.W3_WPG_PutawayGroup = WhsPutawayGroupPK;
					assignedParams++;
				}
			}

			if (createdParams > 0)
			{
				log.Notify(OperationalActionLogErrorLevel.Informational, Res.GetString("22bddfdc-5d47-4538-93b2-05dc8d03afd6", "Created Product/Warehouse/Client {0} parameter(s) for {1} and assigned Warehouse Putaway Group.", createdParams, product.OP_PartNum));
			}

			if (assignedParams > 0)
			{
				log.Notify(OperationalActionLogErrorLevel.Informational, Res.GetString("bccb7d92-fa3b-480f-8a8c-c84e535803b3", "Assigned Warehouse Putaway Group to {0} Product/Warehouse/Client parameter(s) for {1}.", assignedParams, product.OP_PartNum));
			}
		}

		#endregion

		#region Validation

		public AssignWhsPutawayGroupValidation Validation
		{
			get { return new AssignWhsPutawayGroupValidation(this); }
		}

		#endregion

		#region Lookups

		public AssignWhsPutawayGroupLookups Lookups
		{
			get { return lookups ?? (lookups = new AssignWhsPutawayGroupLookups(this)); }
		}
		AssignWhsPutawayGroupLookups lookups;

		#endregion
	}
}
