using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Integration;

namespace Enterprise.MasterFiles.Module
{
	public class UpdateDynamicPickFaceAreaMethodApplicator : OperationalActionMethodApplicator
	{
		public UpdateDynamicPickFaceAreaMethodApplicator(string name, BusinessObjectFactory factory)
			: base(name, factory)
		{
		}

		#region WarehousePK

		[List("Lookups.Warehouses")]
		[ResourceStringData("UpdateDynamicPickFaceAreaMethodApplicator|WarehousePK", Caption = "Warehouse")]
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

		public ZPropertyInfo WarehousePKInfo => GetZPropertyInfo(nameof(WarehousePK));

		ZGuid warehousePK;

		#endregion

		#region ClientPK

		[List("Lookups.Clients")]
		[ResourceStringData("UpdateDynamicPickFaceAreaMethodApplicator|ClientPK", Caption = "Organization")]
		public ZGuid ClientPK
		{
			get { return clientPK; }
			set
			{
				SetNonPersistentPropertyValue(ClientPKInfo, ref clientPK, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateClientPK();
				}
			}
		}

		public ZPropertyInfo ClientPKInfo => GetZPropertyInfo(nameof(ClientPK));

		ZGuid clientPK;

		#endregion

		#region DynamicPickAreaPK

		[List("Lookups.DynamicPickAreas")]
		[ResourceStringData("UpdateDynamicPickFaceAreaMethodApplicator|DynamicPickAreaPK", Caption = "Dynamic Pick Face Area")]
		public ZGuid DynamicPickAreaPK
		{
			get { return dynamicPickAreaPK; }
			set
			{
				if (value.IsEmpty)
				{
					OverrideNonEmptyDynamicPickArea = true;
				}

				SetNonPersistentPropertyValue(DynamicPickAreaPKInfo, ref dynamicPickAreaPK, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateDynamicPickAreaPK();
					Validation.ValidateOverrideNonEmptyDynamicPickArea();
				}
			}
		}

		public ZPropertyInfo DynamicPickAreaPKInfo => GetZPropertyInfo(nameof(DynamicPickAreaPK));

		ZGuid dynamicPickAreaPK;

		#endregion

		#region OverrideNonEmptyDynamicPickArea

		[ResourceStringData("UpdateDynamicPickFaceAreaMethodApplicator|OverrideNonEmptyDynamicPickArea", Caption = "Override Existing Dynamic Pick Face Area")]
		public ZBool OverrideNonEmptyDynamicPickArea
		{
			get { return overrideNonEmptyDynamicPickArea; }
			set
			{
				SetNonPersistentPropertyValue(OverrideNonEmptyDynamicPickAreaInfo, ref overrideNonEmptyDynamicPickArea, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateDynamicPickAreaPK();
					Validation.ValidateOverrideNonEmptyDynamicPickArea();
				}
			}
		}

		public ZPropertyInfo OverrideNonEmptyDynamicPickAreaInfo => GetZPropertyInfo(nameof(OverrideNonEmptyDynamicPickArea));

		ZBool overrideNonEmptyDynamicPickArea = true;

		#endregion

		#region ApplyCore

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] products)
		{
			Validation.ValidateAll();

			if (!HasErrors)
			{
				log.SetSectionProgressMax(products.Length);

				foreach (OrgSupplierPart product in products)
				{
					log.BumpSectionProgress();

					var paramsByWhsAndClients = ObjectFactory.New<IWhsProductParamsByWhsAndClientCollection>(product, product.Factory);
					var relatedOrganisationPKs = product.RelatedOrganisations.Cast<OrgPartRelation>().Where(r => r.OU_Relationship == OrgPartRelation.RelationshipTypes.Owner || r.OU_Relationship == OrgPartRelation.RelationshipTypes.Both).Select(ro => ro.OU_OH);
					var relatedOrgParams = paramsByWhsAndClients.FindWhsProductParamsByWhsAndClients(relatedOrganisationPKs, WarehousePK);

					var clientWhsRelationship = relatedOrgParams.FirstOrDefault(p => p.W3_OH == ClientPK);

					if (DynamicPickAreaPK.IsValid)
					{
						var dynamicPickAreaName = Factory.Load<IWhsArea>(DynamicPickAreaPK)?.WA_Name;
						if (!dynamicPickAreaName.HasValue || !dynamicPickAreaName.Value.IsValid)
						{
							log.Notify(OperationalActionLogErrorLevel.Error, Res.GetString("0983505f-f9cd-442e-94bd-4d5ad7454be3", "Not able to find the area.", product.OP_PartNum, dynamicPickAreaName));
							break;
						}
						if (clientWhsRelationship == null)
						{
							if (relatedOrganisationPKs.Contains(ClientPK))
							{
								var newParam = (IWhsProductParamsByWhsAndClient)paramsByWhsAndClients.AddNew();
								newParam.W3_OH = ClientPK;
								newParam.W3_WW = WarehousePK;
								newParam.W3_WA_DynamicPickFaceArea = DynamicPickAreaPK;

								LogAsInfo(Res.GetString("1cc20bd1-b088-4bc6-bf33-c6187ca946ba", "Created new Product/Warehouse/Client parameter for {0} and assigned Dynamic Pick Face Area to '{1}'.", product.OP_PartNum, dynamicPickAreaName));
							}
							else
							{
								LogAsWarning(Res.GetString("f5b91e4d-d0c1-43bf-ae25-043c8e3a4ee9", "Unable to update Product/Warehouse/Client parameter as Client is not an owner of {0}.", product.OP_PartNum));
							}
						}
						else if (clientWhsRelationship.W3_WA_DynamicPickFaceArea.IsEmpty || OverrideNonEmptyDynamicPickArea)
						{
							clientWhsRelationship.W3_WA_DynamicPickFaceArea = DynamicPickAreaPK;
							LogAsInfo(Res.GetString("be964f5b-5e6b-45c2-84f0-a12b5ccf30c9", "Updated Dynamic Pick Face Area on Product/Warehouse/Client parameter for {0} to '{1}'.", product.OP_PartNum, dynamicPickAreaName));
						}
						else
						{
							LogAsInfo(Res.GetString("c3497e8d-a857-4254-b227-3681174e5b83", "Dynamic Pick Face Area on Product/Warehouse/Client parameter previously set for {0}. Value was left unchanged.", product.OP_PartNum));
						}
					}
					else
					{
						if (clientWhsRelationship != null)
						{
							clientWhsRelationship.W3_WA_DynamicPickFaceArea = ZGuid.Empty;
							LogAsInfo(Res.GetString("1db310a6-73d3-4b65-a1a4-697dccd89fe5", "Cleared Dynamic Pick Face Area on Product/Warehouse/Client parameter for {0}.", product.OP_PartNum));
						}
						else
						{
							LogAsWarning(Res.GetString("0643509b-0302-4a6b-9f5b-05e4455a3402", "Unable to clear Dynamic Pick Face Area on Product/Warehouse/Client parameter as Client is not an owner of {0}.", product.OP_PartNum));
						}
					}
				}
			}
			else
			{
				log.Notify(OperationalActionLogErrorLevel.Error, Res.GetString("7ef668bb-cce3-4855-9c68-7f8c8d66c9b8", "Enter valid arguments."));
			}

			void LogAsWarning(string text) => log.Notify(OperationalActionLogErrorLevel.Warning, text);
			void LogAsInfo(string text) => log.Notify(OperationalActionLogErrorLevel.Informational, text);
		}

		#endregion

		#region Validation

		public UpdateDynamicPickFaceAreaValidation Validation => new UpdateDynamicPickFaceAreaValidation(this);

		#endregion

		#region Lookups

		public UpdateDynamicPickFaceAreaLookups Lookups => lookups ?? (lookups = new UpdateDynamicPickFaceAreaLookups(this));
		UpdateDynamicPickFaceAreaLookups lookups;

		#endregion
	}
}
