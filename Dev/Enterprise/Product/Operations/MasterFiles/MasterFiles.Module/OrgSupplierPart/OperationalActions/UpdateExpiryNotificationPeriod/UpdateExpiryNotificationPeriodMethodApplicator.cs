using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Module
{
	public class UpdateExpiryNotificationPeriodMethodApplicator : OperationalActionMethodApplicator
	{
		public UpdateExpiryNotificationPeriodMethodApplicator(string name, BusinessObjectFactory factory)
			: base(name, factory)
		{
		}

		#region WarehousePK

		[List("Lookups.Warehouses")]
		[ResourceStringData("UpdateExpiryNotificationPeriodMethodApplicator|WarehousePK", Caption = "Warehouse")]
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
		[ResourceStringData("UpdateExpiryNotificationPeriodMethodApplicator|ClientPK", Caption = "Organization")]
		public ZGuid ClientPK
		{
			get { return clientPK; }
			set
			{
				SetNonPersistentPropertyValue(ClientPKInfo, ref clientPK, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateClientPK();
					if (!ClientPKInfo.HasErrors())
					{
						var client = Factory.Load<OrgHeader>(value);
						if (ExpiryNotificationPeriod == 0)
						{
							ExpiryNotificationPeriod = client.MiscServ.OM_WhsDefaultExpiryNotificationPeriodInDays;
						}
					}
				}
			}
		}

		public ZPropertyInfo ClientPKInfo => GetZPropertyInfo(nameof(ClientPK));

		ZGuid clientPK;

		#endregion

		#region ExpiryNotificationPeriod

		[ResourceStringData("UpdateExpiryNotificationPeriodMethodApplicator|ExpiryNotificationPeriod", Caption = "Expiry Notification Period (Days)")]
		public ZInt ExpiryNotificationPeriod
		{
			get { return expiryNotificationPeriod; }
			set
			{
				SetNonPersistentPropertyValue(ExpiryNotificationPeriodInfo, ref expiryNotificationPeriod, value);
				if (value == 0)
				{
					OverrideNonZeroExpiryNotificationPeriod = true;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateExpiryNotificationPeriod();
					Validation.ValidateOverrideExistingNonZeroValues();
				}
			}
		}

		public ZPropertyInfo ExpiryNotificationPeriodInfo => GetZPropertyInfo(nameof(ExpiryNotificationPeriod));

		ZInt expiryNotificationPeriod;

		#endregion

		#region OverrideExistingNonZeroValues

		[ResourceStringData("UpdateExpiryNotificationPeriodMethodApplicator|OverrideExistingNonZeroValues", Caption = "Override Existing Non-Zero Periods")]
		public ZBool OverrideNonZeroExpiryNotificationPeriod
		{
			get { return overrideNonZeroExpiryNotificationPeriod; }
			set
			{
				SetNonPersistentPropertyValue(OverrideNonZeroExpiryNotificationPeriodInfo, ref overrideNonZeroExpiryNotificationPeriod, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateOverrideExistingNonZeroValues();
				}
			}
		}

		public ZPropertyInfo OverrideNonZeroExpiryNotificationPeriodInfo => GetZPropertyInfo(nameof(OverrideNonZeroExpiryNotificationPeriod));

		ZBool overrideNonZeroExpiryNotificationPeriod = true;

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

					if (ExpiryNotificationPeriod > 0)
					{
						if (clientWhsRelationship == null)
						{
							if (relatedOrganisationPKs.Contains(ClientPK))
							{
								var newParam = (IWhsProductParamsByWhsAndClient)paramsByWhsAndClients.AddNew();
								newParam.W3_OH = ClientPK;
								newParam.W3_WW = WarehousePK;
								newParam.W3_ExpiryNotificationPeriod = ExpiryNotificationPeriod;

								log.Notify(OperationalActionLogErrorLevel.Informational, Res.GetString("a1e3b86e-f25d-4148-86b6-1fb8799cee5a", "Created new Product/Warehouse/Client parameter for {0} and assigned Expiry Notification Period to {1} day(s).", product.OP_PartNum, ExpiryNotificationPeriod));
							}
							else
							{
								log.Notify(OperationalActionLogErrorLevel.Warning, Res.GetString("e5120b8b-1b59-426f-b324-dc5335d6ae3d", "Unable to update Product/Warehouse/Client parameter as Client is not an owner of {0}.", product.OP_PartNum));
							}
						}
						else if (clientWhsRelationship.W3_ExpiryNotificationPeriod == 0 || OverrideNonZeroExpiryNotificationPeriod)
						{
							clientWhsRelationship.W3_ExpiryNotificationPeriod = ExpiryNotificationPeriod;
							log.Notify(OperationalActionLogErrorLevel.Informational, Res.GetString("5fa00ed7-96ff-4c81-8132-6b4b127050b0", "Updated Expiry Notification Period on Product/Warehouse/Client parameter for {0} to {1} day(s).", product.OP_PartNum, ExpiryNotificationPeriod));
						}
						else
						{
							log.Notify(OperationalActionLogErrorLevel.Informational, Res.GetString("c471c6e4-2a65-4df7-a09a-6de5fca65888", "Expiry Notification Period on Product/Warehouse/Client parameter previously set for {0}. Value was left unchanged.", product.OP_PartNum));
						}
					}
					else
					{
						if (clientWhsRelationship != null)
						{
							clientWhsRelationship.W3_ExpiryNotificationPeriod = 0;
							log.Notify(OperationalActionLogErrorLevel.Informational, Res.GetString("1fa02e89-2737-4127-8ee4-f4975e0db850", "Cleared Expiry Notification Period on Product/Warehouse/Client parameter for {0}.", product.OP_PartNum));
						}
						else
						{
							log.Notify(OperationalActionLogErrorLevel.Warning, Res.GetString("84abf07d-e1dd-4ff4-9f5a-e006cee1986c", "Unable to clear Expiry Notification Period on Product/Warehouse/Client parameter as Client is not an owner of {0}.", product.OP_PartNum));
						}
					}
				}
			}
			else
			{
				log.Notify(OperationalActionLogErrorLevel.Error, Res.GetString("7ef668bb-cce3-4855-9c68-7f8c8d66c9b8", "Enter valid arguments."));
			}
		}

		#endregion

		#region Validation

		public UpdateExpiryNotificationPeriodValidation Validation => new UpdateExpiryNotificationPeriodValidation(this);

		#endregion

		#region Lookups

		public WarehouseClientLookups Lookups => lookups ?? (lookups = new WarehouseClientLookups(this));

		WarehouseClientLookups lookups;

		#endregion
	}
}
