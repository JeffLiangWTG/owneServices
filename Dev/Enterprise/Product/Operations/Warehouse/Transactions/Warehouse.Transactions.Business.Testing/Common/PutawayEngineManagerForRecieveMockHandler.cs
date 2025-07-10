using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public static class PutawayEngineManagerForReceiveMockHandler
	{
		public static Mock<IPutawayEngineManagerForReceive> GetMockPutawayEngineManagerForReceive(
			Func<IEnumerable<WhsReceive>, bool> receivesCondition = null,
			Func<IEnumerable<WhsReceiveLine>, bool> receiveLinesCondition = null,
			Func<INotifications, bool> iNotificationsCondition = null,
			Func<RefEquipment, bool> refEquipmentCondition = null,
			Func<IEnumerable<ZGuid>, bool> skipLocationPksCondition = null,
			Func<bool, bool> concurrencyHandlingCondition = null,
			Func<bool, bool> rebuildLocationCacheCondition = null,
			Action<IEnumerable<WhsReceive>, IEnumerable<WhsReceiveLine>, INotifications, RefEquipment, IEnumerable<ZGuid>, bool, bool> action = null)
		{
			var mockPutawayEngine = new Mock<IPutawayEngineManagerForReceive>(MockBehavior.Strict);
			SetMockPutawayEngineManagerForReceive(mockPutawayEngine, receivesCondition, receiveLinesCondition, iNotificationsCondition, refEquipmentCondition, skipLocationPksCondition, concurrencyHandlingCondition, rebuildLocationCacheCondition, action);
			return mockPutawayEngine;
		}

		static void SetMockPutawayEngineManagerForReceive(
			Mock<IPutawayEngineManagerForReceive> mockPutawayEngine,
			Func<IEnumerable<WhsReceive>, bool> receivesCondition = null,
			Func<IEnumerable<WhsReceiveLine>, bool> receiveLinesCondition = null,
			Func<INotifications, bool> iNotificationsCondition = null,
			Func<RefEquipment, bool> refEquipmentCondition = null,
			Func<IEnumerable<ZGuid>, bool> skipLocationPksCondition = null,
			Func<bool, bool> concurrencyHandlingCondition = null,
			Func<bool, bool> rebuildLocationCacheCondition = null,
			Action<IEnumerable<WhsReceive>, IEnumerable<WhsReceiveLine>, INotifications, RefEquipment, IEnumerable<ZGuid>, bool, bool> action = null)
		{
			mockPutawayEngine
			.Setup(putawayEngine => putawayEngine.Putaway(
					It.Is<IEnumerable<WhsReceive>>(x => receivesCondition == null || receivesCondition(x)),
					It.Is<IEnumerable<WhsReceiveLine>>(x => receiveLinesCondition == null || receiveLinesCondition(x)),
					It.Is<INotifications>(x => iNotificationsCondition == null || iNotificationsCondition(x)),
					It.Is<RefEquipment>(x => refEquipmentCondition == null || refEquipmentCondition(x)),
					It.Is<IEnumerable<ZGuid>>(x => skipLocationPksCondition == null || skipLocationPksCondition(x)),
					It.Is<bool>(x => concurrencyHandlingCondition == null || concurrencyHandlingCondition(x)),
					It.Is<bool>(x => rebuildLocationCacheCondition == null || rebuildLocationCacheCondition(x))))
			.Callback((Action<IEnumerable<WhsReceive>, IEnumerable<WhsReceiveLine>, INotifications, RefEquipment, IEnumerable<ZGuid>, bool, bool>)((
				receiveParam,
				receiveLines,
				iNotifications,
				refEquipment,
				locationPKs,
				checkChangeLocationID,
				needRebuildLocationCache) =>
			{
				if (action != null)
				{
					action(receiveParam, receiveLines, iNotifications, refEquipment, locationPKs, checkChangeLocationID, needRebuildLocationCache);
				}
				else
				{
					var receive = receiveParam.First();

					var warehouse = receive.Warehouse;
					foreach (var receiveLine in receiveLines)
					{
						receiveLine.WE_WL = receive.IsCustomsTransaction
							? warehouse.DefaultLocationInBondedArea.PK
							: warehouse.DefaultLocation.PK;
					}
				}
			}))
			.Verifiable();
		}
	}
}
