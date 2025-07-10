using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class BasePackingGroupUniqueIndexFailureHandler : IUniqueIndexFailureHandler
	{
		readonly BasePackingGroup packingGroup;

		public BasePackingGroupUniqueIndexFailureHandler(BasePackingGroup packingGroup)
		{
			this.packingGroup = Argument.NotNull(packingGroup, nameof(packingGroup));
		}

		public IEnumerable<string> HandledUniqueIndexNames
		{
			get
			{
				yield return CusDecHouseContainerPivotSchema.Constants.Indexes.FK_UX__CR_CU_HouseBill_CR_CO_Container_CR_CEQ_Equipment;
			}
		}

		public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			var existingItem = GetExistingBasePackingGroup();
			if (existingItem != null)
			{
				TryToResolve(existingItem);
				notifier.ReportInformation(GenerateMessage(existingItem), Res.GetString("b947a40b-2e7c-4561-99a0-8ee888399545", "Save Error"));
			}
		}

		void TryToResolve(BasePackingGroup existingItem)
		{
			var houseBill = packingGroup.Bill;
			var query = new ZDBOnlyQuery(typeof(BasePackage));
			query.AddToFilter(CusDecHouseContainerPackSchema.CW_ClusterKey, existingItem.CR_ClusterKey);
			query.AddToFilter(CusDecHouseContainerPackSchema.CW_CR_HouseContainer, existingItem.PK);
			var existingPackages = existingItem.Factory.Load<BasePackage>(query);
			packingGroup.Delete();
			var declaration = houseBill.Declaration;
			declaration.PackingGroups.Add(existingItem);
			houseBill.PackingGroups.Add(existingItem);
			declaration.Packages.AddRange(existingPackages);
		}

		string GenerateMessage(BasePackingGroup existingItem)
		{
			if (existingItem.Container is BaseCusContainer container)
			{
				return Res.GetString("c9b69833-22ab-4912-92b2-c8ec4c7f4ab7",
					"Container Number ({0}) has already been linked to Bill Number ({1}) by another user ({2}). Duplicate packing details have been deleted, please review your changes and save again.",
					container.CO_ContainerNumber,
					existingItem.Bill.CU_BillNum,
					GetLastEditUserAndTime(existingItem));
			}
			else if (existingItem.Equipment is CusEquipment equipment)
			{
				return Res.GetString("FB56DAFC-8B67-4F4D-889F-C52DF31769A5",
					"Equipment ({0}) has already been linked to Bill Number ({1}) by another user ({2}). Duplicate packing details have been deleted, please review your changes and save again.",
					equipment.CEQ_IdentificationNumber,
					existingItem.Bill.CU_BillNum,
					GetLastEditUserAndTime(existingItem));
			}
			else
			{
				return Res.GetString("8F583BB3-1E27-479D-860B-BBC45DBB9EEE",
					"Another user ({0}) has already linked to Bill Number ({1}) a record without Container or Equipment details. Duplicate packing details have been deleted, please review your changes and save again.",
					GetLastEditUserAndTime(existingItem),
					existingItem.Bill.CU_BillNum);
			}
		}

		string GetLastEditUserAndTime(BasePackingGroup existingItem)
		{
			var declaration = existingItem.Declaration;
			return declaration.JE_SystemLastEditUser + " @ " + declaration.JE_SystemLastEditTimeUtc;
		}

		BasePackingGroup GetExistingBasePackingGroup()
		{
			var containerPK = packingGroup.CR_CO_Container;
			var equipmentPK = packingGroup.CR_CEQ_Equipment;
			var query = new ZDBOnlyQuery(typeof(BasePackingGroup));
			query.AddToFilter(CusDecHouseContainerPivotSchema.CR_CU_HouseBill, packingGroup.CR_CU_HouseBill);
			query.AddToFilter(CusDecHouseContainerPivotSchema.CR_CO_Container, containerPK.IsEmpty ? DBNull.Value : containerPK.ToGuid());
			query.AddToFilter(CusDecHouseContainerPivotSchema.CR_CEQ_Equipment, equipmentPK.IsEmpty ? DBNull.Value : equipmentPK.ToGuid());
			query.AddToFilter(CusDecHouseContainerPivotSchema.PK, SQLComparisonOperator.NotEqual, packingGroup.PK);
			return packingGroup.Factory.LoadTop1<BasePackingGroup>(query);
		}
	}
}
