using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.GUI
{
	class FullRatingXmlValueObjectSerializer : XmlValueObjectSerializer
	{
		public FullRatingXmlValueObjectSerializer(Type valueObjectType)
			: base(valueObjectType)
		{
		}

		protected override BusinessObject CreateOrUpdateFromValueObject(IValueObjectDataAdapter dataAdapter, IBusinessObjectCollection collection, IValueObject valueObject, IValueObjectImportContext context)
		{
			try
			{
				context.FactoryProvider.CreateNewWithoutSave(); // This is within the try catch when it triggers an exception when a errors is triggered by TG_CheckNoRateEntryOverlaps
				BusinessObject bizObj = dataAdapter.CreateOrUpdateFromValueObject(valueObject, context);
				SaveIfNotInTheDatabaseAndNotify(bizObj, context);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				context.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
			}

			return null;
		}

		void SaveIfNotInTheDatabaseAndNotify(BusinessObject bizObj, IValueObjectImportContext context)
		{
			if (!bizObj.IsInDatabase)
			{
				BusinessObjectCreatedOrUpdatedNotification createdBizObjNotification = new BusinessObjectCreatedOrUpdatedNotification(bizObj);
				context.FactoryProvider.SaveCurrentAndUpdateRecordCounts();
				context.Notify(createdBizObjNotification);
			}
		}
	}
}
