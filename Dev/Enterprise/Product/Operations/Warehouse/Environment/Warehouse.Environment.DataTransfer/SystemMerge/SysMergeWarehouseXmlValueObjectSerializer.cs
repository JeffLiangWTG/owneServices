using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Environment.DataTransfer
{
	public class SysMergeWarehouseXmlValueObjectSerializer : XmlValueObjectSerializer
	{
		public SysMergeWarehouseXmlValueObjectSerializer(Type valueObjectType)
			: base(valueObjectType)
		{
		}

		protected override BusinessObject CreateOrUpdateFromValueObject(IValueObjectDataAdapter dataAdapter, IBusinessObjectCollection collection, IValueObject valueObject, IValueObjectImportContext context)
		{
			context.FactoryProvider.CreateNewWithoutSave();
			BusinessObject topLevelBizObj = null;
			try
			{
				topLevelBizObj = dataAdapter.CreateOrUpdateFromValueObject(valueObject, context);
				SaveIfNotInTheDatabaseAndNotify(topLevelBizObj, context);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				topLevelBizObj = null;
				context.Notify(new InfoNotification("\r\n"));
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
					context.Notify(GetImportedSuccessfullyNotification(GetWarehouseDisplayInfo(topLevelBizObj)));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var message = string.Format(Culture.Invariant, "{0}\r\n\r\n{1}", GetWarehouseDisplayInfo(topLevelBizObj), ex.Message);
					throw new InvalidOperationException(message, ex);
				}
			}
		}

		#region GetImportedSuccessfullyNotification

		InfoNotification GetImportedSuccessfullyNotification(string objectDescription)
		{
			return new InfoNotification(Res.GetString("03398e9c-b57e-4382-8287-513bf2f40314", "{0} imported successfully", objectDescription) + "\r\n");
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

		string GetWarehouseDisplayInfo(BusinessObject topLevelBizObj)
		{
			var warehouse = (WhsWarehouse)topLevelBizObj;
			return Res.GetString("eda1edfd-cb5a-4419-9393-33fa57dd7fe2", "Warehouse: [({0}) - {1} - {2}]", warehouse.PK.ToString(), warehouse.WW_WarehouseCode, warehouse.WW_WarehouseNameMultilingual);
		}

		#endregion

		#endregion
	}
}
