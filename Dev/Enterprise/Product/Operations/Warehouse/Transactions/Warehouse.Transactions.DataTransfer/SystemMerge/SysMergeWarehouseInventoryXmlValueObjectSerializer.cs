using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class SysMergeWarehouseInventoryXmlValueObjectSerializer : XmlValueObjectSerializer
	{
		public SysMergeWarehouseInventoryXmlValueObjectSerializer(Type valueObjectType)
			: base(valueObjectType)
		{
		}

		protected override BusinessObject CreateOrUpdateFromValueObject(IValueObjectDataAdapter dataAdapter, IBusinessObjectCollection collection, IValueObject valueObject, IValueObjectImportContext context)
		{
			context.FactoryProvider.CreateNewWithoutSave();
			BusinessObject topLevelBizObj;
			try
			{
				topLevelBizObj = dataAdapter.CreateOrUpdateFromValueObject(valueObject, context);
				SaveIfNotInTheDatabaseAndNotify(topLevelBizObj, context);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				topLevelBizObj = null;
				context.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
			}

			return topLevelBizObj;
		}

		#region SaveIfNotInTheDatabaseAndNotify

		/// <summary>
		/// Only Save Factory and Notify if the Top Level Business Object is a new imported one
		/// (as opposed to an existing one, which has been loaded and skipped from import)
		/// </summary>
		void SaveIfNotInTheDatabaseAndNotify(BusinessObject topLevelBizObj, IValueObjectImportContext context)
		{
			if (!topLevelBizObj.IsInDatabase)
			{
				try
				{
					SaveAndNotify(topLevelBizObj, context);
					context.Notify(GetImportedSuccessfullyNotification(GetReceiveDisplayInfo(topLevelBizObj)));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var message = string.Format(CultureInfo.InvariantCulture, "{0}\r\n\r\n{1}", GetReceiveDisplayInfo(topLevelBizObj), ex.Message);
					throw new InvalidOperationException(message, ex);
				}
			}
		}

		#region GetImportedSuccessfullyNotification

		InfoNotification GetImportedSuccessfullyNotification(string objectDescription)
		{
			return new InfoNotification(Res.GetString("c74a25e5-37fb-421d-a37b-2ca0ebe698d1", "{0} imported successfully", objectDescription) + "\r\n");
		}

		#endregion

		#region SaveAndNotify

		/// <summary>
		/// Saves and add a create business object notification in order to increment the count of imported top level objects.
		/// </summary>
		void SaveAndNotify(BusinessObject topLevelBizObj, IValueObjectImportContext context)
		{
			var createdBizObjNotification = new BusinessObjectCreatedOrUpdatedNotification(topLevelBizObj);
			context.FactoryProvider.SaveCurrentAndUpdateRecordCounts();
			context.Notify(createdBizObjNotification);
		}

		#endregion

		#region GetWarehouseDisplayInfo

		string GetReceiveDisplayInfo(BusinessObject topLevelBizObj)
		{
			var receive = (WhsReceive)topLevelBizObj;
			return Res.GetString("d9ab2602-1e94-462c-801a-03e009cdaeb7", "Receive [({0}) - {1}]", receive.PK.ToString(), receive.WD_ExternalReference);
		}

		#endregion

		#endregion
	}
}
