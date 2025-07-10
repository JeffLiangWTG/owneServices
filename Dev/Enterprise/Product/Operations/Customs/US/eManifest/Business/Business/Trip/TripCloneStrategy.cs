using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.eManifest.Business
{
	class TripCloneStrategy : BusinessObjectCloneStrategy
	{
		public TripCloneStrategy(Trip bizObj)
			: base(bizObj) { }

		internal Trip Clone()
		{
			return (Trip)Clone(new BusinessObjectCloneArgs());
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var tripToClone = (Trip)bizObjToClone;
			var clonedTrip = (Trip)base.CloneInternal(new TripCloneArgs());

			using (clonedTrip.SuspendSettingHasChanges())
			using (clonedTrip.GetValidationSuspender())
			{
				#region Conveyance / Equipment

				var equipmentCloneArgs = new EquipmentCloneArgs();
				clonedTrip.Conveyance.CopyPersistentValuesFrom(tripToClone.Conveyance, equipmentCloneArgs);
				clonedTrip.Equipment.AddRange(tripToClone.Equipment.Select(e => e.Clone(equipmentCloneArgs)));

				#endregion

				#region Crew Members

				var crewMemberCloneArgs = new CrewMemberCloneArgs();
				var addressCloneArgs = new AddressCloneArgs();

				foreach (var crewMemberToClone in tripToClone.CrewMembers)
				{
					var clonedCrewMember = (CrewMember)crewMemberToClone.Clone(crewMemberCloneArgs);
					using (clonedCrewMember.SuspendSettingHasChanges())
					using (clonedCrewMember.GetValidationSuspender())
					{
						clonedCrewMember.USAddress.CopyPersistentValuesFrom(crewMemberToClone.USAddress, addressCloneArgs);
					}
					clonedTrip.CrewMembers.Add(clonedCrewMember);
				}

				#endregion

				#region Shipments

				var shipmentCloneArgs = new ShipmentCloneArgs();
				var commodityCloneArgs = new CommodityCloneArgs();
				var cusCodeDataCloneArgs = new CusCodeDataCloneArgs();
				var undgDataItemCloneArgs = new UNDGDataItemCloneArgs();
				var inBondCloneArgs = new InBondCloneArgs();

				foreach (var shipmentToClone in tripToClone.Shipments)
				{
					var clonedShipment = (Shipment)shipmentToClone.Clone(shipmentCloneArgs);
					clonedTrip.Shipments.Add(clonedShipment);

					using (clonedShipment.SuspendSettingHasChanges())
					using (clonedShipment.GetValidationSuspender())
					{
						clonedShipment.Consignee.CopyPersistentValuesFrom(shipmentToClone.Consignee, addressCloneArgs);
						clonedShipment.Shipper.CopyPersistentValuesFrom(shipmentToClone.Shipper, addressCloneArgs);
						clonedShipment.Parties.AddRange(shipmentToClone.Parties.Select(p => p.Clone(addressCloneArgs)));

						foreach (var commodityToClone in shipmentToClone.Commodities)
						{
							var clonedCommodity = (Commodity)commodityToClone.Clone(commodityCloneArgs);
							clonedShipment.Commodities.Add(clonedCommodity);
							using (clonedCommodity.SuspendSettingHasChanges())
							using (clonedCommodity.GetValidationSuspender())
							{
								clonedCommodity.HarmonizedNumbers.AddRange(commodityToClone.HarmonizedNumbers.Select(c => c.Clone(cusCodeDataCloneArgs)));
								clonedCommodity.C4Codes.AddRange(commodityToClone.C4Codes.Select(c => c.Clone(cusCodeDataCloneArgs)));
								clonedCommodity.UNDGs.AddRange(commodityToClone.UNDGs.Select(c => c.Clone(undgDataItemCloneArgs)));
							}
						}

						if (shipmentToClone.B0_ShipmentType == ShipmentTypes.Codes.Inbond)
						{
							clonedShipment.InBond.CopyPersistentValuesFrom(shipmentToClone.InBond, inBondCloneArgs);
						}
					}
				}

				if (clonedTrip.AllEquipmentIncludingMainConveyance.Count > 0)
				{
					clonedTrip.PopulateCommodityWithEquipment(clonedTrip.AllEquipmentIncludingMainConveyance[0].PK);
				}

				#endregion

				#region Notes

				foreach (StmNote note in tripToClone.Notes.GetAllNotes())
				{
					clonedTrip.Notes.Add(note.Clone());
				}

				#endregion
			}

			return clonedTrip;
		}

		#region Clone Args

		class TripCloneArgs : BusinessObjectCloneArgs
		{
			internal TripCloneArgs()
				: base(new[]
				{
					Trip.Schema.BH_JobReference,
					Trip.Schema.BH_ReleaseStatus,
					Trip.Schema.BH_MessageStatus,
					Trip.Schema.BH_ETA
				})
			{ }
		}

		class EquipmentCloneArgs : BusinessObjectCloneArgs
		{
			internal EquipmentCloneArgs()
				: base(new[]
				{
					Equipment.Schema.BJ_BH_Header,
					Equipment.Schema.BJ_IITEntityIndicators,
					Equipment.Schema.BJ_InsuranceAmount,
					Equipment.Schema.BJ_InsuranceName,
					Equipment.Schema.BJ_InsurancePolicyNumber,
					Equipment.Schema.BJ_InsuranceYearPolicyIssue
				})
			{ }
		}

		class CrewMemberCloneArgs : BusinessObjectCloneArgs
		{
			internal CrewMemberCloneArgs()
				: base(new[] { CusInBondPerson.Schema.CP_BH_Header }) { }
		}

		class AddressCloneArgs : BusinessObjectCloneArgs
		{
			internal AddressCloneArgs()
				: base(new[]
				{
					JobDocAddress.Schema.E2_ParentID,
					JobDocAddress.Schema.E2_ParentTableCode
				})
			{ }
		}

		class ShipmentCloneArgs : BusinessObjectCloneArgs
		{
			internal ShipmentCloneArgs()
				: base(new[]
				{
					Shipment.Schema.B0_BH,
					Shipment.Schema.B0_MasterBillNumber,
					Shipment.Schema.B0_ReferenceID,
					Shipment.Schema.B0_ManifestQty,
					Shipment.Schema.B0_BoardedQuantity,
					Shipment.Schema.B0_Weight,
					Shipment.Schema.B0_Volume,
					Shipment.Schema.B0_DateOfExport,
					Shipment.Schema.B0_ReleaseStatus,
					Shipment.Schema.B0_ReleaseStatusDate
				})
			{ }
		}

		class CommodityCloneArgs : BusinessObjectCloneArgs
		{
			internal CommodityCloneArgs()
				: base(new[]
				{
					Commodity.Schema.BY_ParentID,
					Commodity.Schema.BY_ParentTableCode,
					Commodity.Schema.BY_MonetaryValue,
					Commodity.Schema.BY_PieceCount,
					Commodity.Schema.BY_GrossWeight,
					Commodity.Schema.BY_BJ_Equipment
				})
			{ }
		}

		class InBondCloneArgs : BusinessObjectCloneArgs
		{
			internal InBondCloneArgs()
				: base(new[]
				{
					InBond.Schema.BM_BH,
					InBond.Schema.BM_ExportDate
				})
			{ }
		}

		class CusCodeDataCloneArgs : BusinessObjectCloneArgs
		{
			internal CusCodeDataCloneArgs()
				: base(new[]
				{
					CusCodeData.Schema.CY_ParentID,
					CusCodeData.Schema.CY_ParentTableCode
				})
			{ }
		}

		class UNDGDataItemCloneArgs : BusinessObjectCloneArgs
		{
			internal UNDGDataItemCloneArgs()
				: base(new[]
				{
					UNDGDataItem.Schema.DI_ParentID,
					UNDGDataItem.Schema.DI_ParentTableCode
				})
			{ }
		}

		#endregion
	}
}
