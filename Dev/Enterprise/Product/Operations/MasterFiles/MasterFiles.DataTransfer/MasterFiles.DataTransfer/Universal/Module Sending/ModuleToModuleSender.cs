using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public abstract class ModuleToModuleSender<T>
		where T : BusinessObject, IWorkflowProvider, IModuleToModule
	{
		protected ModuleToModuleSender()
		{
		}

		#region CreateNewEntityFromParent

		public PublishToUniversalResult CreateNewEntityFromParent(T parentEntity)
		{
			UniversalEvent[] events = null;

			var error = CanNotSendUniversalShipmentReason(parentEntity);
			if (error.IsEmpty)
			{
				try
				{
					events = GetUniversalEvents(parentEntity);
				}
				catch (MessageProcessingBusinessFailureException ex)
				{
					error = Res.GetString("5730726d-aa9f-432e-a059-010f67ee49c1", "Universal Processing Failure occurred:\r\n{0}", ex.Message);
				}
				catch (ZSaveException ex)
				{
					error = Res.GetString("cf61e8d4-bd41-4618-b999-0e28a6d50983", "Save Exception occurred:\r\n{0}", ex.Message);
				}
			}

			return events != null ? PublishToUniversalResult.New(events, EntityTypeToLoad, ErrorPrefix) : PublishToUniversalResult.New(ErrorPrefix, error);
		}

		#endregion

		#region CanNotSendUniversalShipmentReason

		ZString CanNotSendUniversalShipmentReason(T parentEntity)
		{
			ZString error;

			if (parentEntity != null)
			{
				var iParentEntity = (IModuleToModule)parentEntity;
				iParentEntity.CanExportData(out error);
			}
			else
			{
				error = Res.GetString("afa23aae-1661-4477-8e72-6cb225b388a9", "Null Entity");
			}

			return error;
		}

		#endregion

		protected abstract ZString ErrorPrefix { get; }
		protected abstract DataContextType EntityTypeToLoad { get; }
		protected abstract UniversalEvent[] GetUniversalEvents(T parentEntity);
	}
}
