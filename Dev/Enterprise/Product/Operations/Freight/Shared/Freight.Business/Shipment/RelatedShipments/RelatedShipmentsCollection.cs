using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	/// <summary>
	/// Collection of Sub Shipments used in CommonShipment related shipments findbox
	/// </summary>
	[ZArchitecture.ComponentModel.ModuleID(ModuleId.JobShipment)]
	public abstract class RelatedShipmentsCollection<T> : ActiveBusinessObjectCollection<T>, IFilterModuleExtraNotificationProvider, IRelatedShipmentsCollection
		where T : CommonShipment
	{
		protected RelatedShipmentsCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public RelatedShipmentsCollection(T referenceShipment, bool isMasterShipmentCollection)
			: this(referenceShipment.Factory)
		{
			ReferenceShipment = referenceShipment;
			IsMasterShipmentCollection = isMasterShipmentCollection;
			CreateAdditionalFilter();
		}

		/// <summary>
		/// To be set when this collection is used in a Related Shipment / Master Shipment findbox list from a Shipment.
		/// </summary>
		public T ReferenceShipment
		{
			get { return referenceShipment; }
			set { referenceShipment = value; }
		}
#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		T referenceShipment;

		/// <summary>
		/// Determines whether this is a collection of potential master shipment, or potential sub-shipments
		/// </summary>
		public ZBool IsMasterShipmentCollection
		{
			get { return isMasterShipmentCollection; }
			set { isMasterShipmentCollection = value; }
		}
#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		ZBool isMasterShipmentCollection;

		#region CreateAdditionalFilter

		void CreateAdditionalFilter()
		{
			AdditionalFilter.AddToFilter(JobShipmentSchema.JS_IsCancelled, ZBool.False);
			AdditionalFilter.AddToFilter(GetShipmentTypeFilter());

			if (!AdditionalFilter.IsNoResultQuery && ReferenceShipment != null)
			{
				AdditionalFilter.AddToFilter(JobShipmentSchema.PK, SQLComparisonOperator.NotEqual, ReferenceShipment.PK);

				if (IsMasterShipmentCollection)
				{
					// Exclude circular references
					if (AllSubs.Count > 0)
					{
						AdditionalFilter.AddToFilter(JobShipmentSchema.PK, SQLComparisonOperator.NotEqual, AllSubs);
					}
				}
				else
				{
					AdditionalFilter.AddToFilter(JobShipmentSchema.JS_JS_ColoadMasterShipment, null);

					// Exclude circular references
					if (AllMasters.Count > 0)
					{
						AdditionalFilter.AddToFilter(JobShipmentSchema.PK, SQLComparisonOperator.NotEqual, AllMasters);
					}
				}
			}
		}

		ZQuery GetShipmentTypeFilter()
		{
			var filter = new ZQuery();

			if (IsMasterShipmentCollection)
			{
				filter.AddToFilter(JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.NotEqual, new[]
					{
						Constants.ShipmentTypes.StandardHouse,
						Constants.ShipmentTypes.HighVolumeLowValue,
						Constants.ShipmentTypes.HighVolumeLowValueLegacy
					});

				if (ReferenceShipment != null)
				{
					if (ReferenceShipment.IsBuyersConsolLead)
					{
						filter.AddToFilter(ZQuery.NoResultQuery);
					}
					else if (ReferenceShipment.IsLeadOrMaster)
					{
						filter.AddToFilter(JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.NotEqual, Constants.ShipmentTypes.CoLoadMaster);
						filter.AddToFilter(JoinCondition.And, JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.NotEqual, Constants.ShipmentTypes.BlindCoLoadMaster);
					}
				}
			}
			else
			{
				filter.AddToFilter(JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.NotEqual, Constants.ShipmentTypes.BuyersConsolLead);

				if (ReferenceShipment != null)
				{
					if (!ReferenceShipment.IsLeadOrMaster)
					{
						filter.AddToFilter(ZQuery.NoResultQuery);
					}
					else if (ReferenceShipment.IsCoLoadMaster)
					{
						filter.AddToFilter(JobShipmentSchema.JS_ShipmentType, Constants.ShipmentTypes.StandardHouse);
						filter.AddToFilter(JoinCondition.Or, JobShipmentSchema.JS_ShipmentType, Constants.ShipmentTypes.HighVolumeLowValue);
						filter.AddToFilter(JoinCondition.Or, JobShipmentSchema.JS_ShipmentType, Constants.ShipmentTypes.HighVolumeLowValueLegacy);
					}
				}
			}

			return filter;
		}

		List<ZGuid> AllMasters
		{
			get
			{
				if (allMasters == null)
				{
					allMasters = new List<ZGuid>();

					if (ReferenceShipment != null)
					{
						var master = ReferenceShipment.CoLoadMasterShipment;
						while (master != null && !allMasters.Contains(master.PK))
						{
							allMasters.Add(master.PK);
							master = master.CoLoadMasterShipment;
						}
					}
				}

				return allMasters;
			}
		}
#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		List<ZGuid> allMasters;

		List<ZGuid> AllSubs
		{
			get
			{
				if (allSubs == null)
				{
					allSubs = GetAllSubs(ReferenceShipment);
				}

				return allSubs;
			}
		}
#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		List<ZGuid> allSubs;

		static List<ZGuid> GetAllSubs(T shipment)
		{
			List<ZGuid> result = new List<ZGuid>();

			if (shipment != null)
			{
				Queue<T> pending = new Queue<T>();
				pending.Enqueue(shipment);

				while (pending.Count > 0)
				{
					T current = pending.Dequeue();

					if (!result.Contains(current.PK))
					{
						result.Add(current.PK);

						foreach (T sub in current.CoLoadShipments)
						{
							pending.Enqueue(sub);
						}
					}
				}
			}

			result.Remove(shipment.PK);

			return result;
		}

		#endregion

		#region AddNotificationWhenAdditionalFilterNotMet

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);

			var selectedShipment = (T)selectedBusinessObject;
			if (ReferenceShipment != null && selectedShipment != null)
			{
				if (selectedShipment.PK == ReferenceShipment.PK)
				{
					errors.Add(Res.GetString("f8c28f95-edf4-4265-b914-f05e320c4eb0",
						"A Shipment cannot be its own Lead or Master."));
				}
				else if (IsMasterShipmentCollection)
				{
					if (!selectedShipment.IsLeadOrMaster)
					{
						errors.Add(Res.GetString("90f7f0b7-b949-4583-a4f4-cd902c5322c5",
							"A {0} shipment cannot be a Lead or Master shipment.", GetShipmentType(selectedShipment)));
					}
					else if ((selectedShipment.IsCoLoadMaster || selectedShipment.IsBlindCoLoadMaster) && ReferenceShipment.IsLeadOrMaster)
					{
						errors.Add(Res.GetString("1c594955-0040-4f58-9863-f6b0fe1ef05e",
							"Only Standard House and High Volume Low Value shipments can be sub-shipments of a Co-Load Master shipment."));
					}
					else if (ReferenceShipment.IsBuyersConsolLead)
					{
						errors.Add(Res.GetString("e45f5e39-2969-48d7-a7aa-909c7eac328c",
							"Only Standard House, Co-Load Master and Assembly Master shipments can be sub-shipments of a Lead or Master shipment."));
					}
					else if (AllSubs.Contains(selectedShipment.PK))
					{
						errors.Add(Res.GetString("57980784-58f2-4bb3-9c4f-80ebe0830669",
							"This Shipment cannot be a Lead or Master shipment of the current shipment as it would create a circular reference."));
					}
				}
				else
				{
					if (selectedShipment.IsBuyersConsolLead)
					{
						errors.Add(Res.GetString("E27A1AB2-EB38-456B-88AD-5AEB8A14038C",
							"Only Standard House, Co-Load Master and Assembly Master shipments can be sub-shipments of a Lead or Master shipment."));
					}
					else if (!ReferenceShipment.IsLeadOrMaster)
					{
						errors.Add(Res.GetString("3cfd6e85-dbf0-4c96-a903-287cb6477abf",
							"A {0} shipment cannot be a Lead or Master shipment.", GetShipmentType(ReferenceShipment)));
					}
					else if (selectedShipment.IsLeadOrMaster && (ReferenceShipment.IsCoLoadMaster || ReferenceShipment.IsBlindCoLoadMaster))
					{
						errors.Add(Res.GetString("60941d19-1c72-4767-ad3f-bd6f7330de56",
							"Only Standard House and High Volume Low Value shipments can be sub-shipments of a Co-Load Master shipment."));
					}
					else if (selectedShipment.JS_JS_ColoadMasterShipment.IsValid)
					{
						errors.Add(Res.GetString("ac41853f-0c4a-4116-bc06-dd789744088b",
							"This shipment already has a Lead / Master shipment. You must detach this shipment from its Lead / Master shipment before you can select it here."));
					}
					else if (AllMasters.Contains(selectedShipment.PK))
					{
						errors.Add(Res.GetString("ed09f59d-8460-4b90-a3c3-d97128af3197",
							"This Shipment cannot be added as a sub-shipment as it would create a circular reference."));
					}
				}
			}
		}

		string STDName
		{
			get { return Res.GetString("4973bbce-2904-4030-ade1-5c300085b635", "Standard House"); }
		}

		string HVLVName
		{
			get { return Res.GetString("24bacceb-bb40-4581-b264-31a1b2e20194", "High Volume Low Value"); }
		}

		string GetShipmentType(T shipment)
		{
			return shipment.IsStandardHouse ? STDName : HVLVName;
		}

		#endregion

		#region IFilterModuleExtraNotificationProvider

		INotification IFilterModuleExtraNotificationProvider.GetExtraNotification(BusinessObject businessObject)
		{
			var shipment = businessObject as T;
			if (shipment != null && ReferenceShipment != null)
			{
				var message = string.Empty;
				if (!FreightShipmentVsConsolMessageHelper.Instance.IsAllowedToAttachSubShipment(out message, ReferenceShipment, shipment))
				{
					return new Notification(CargoWise.ComponentModel.NotificationType.Error, message);
				}
			}

			return null;
		}

		#endregion
	}
}
