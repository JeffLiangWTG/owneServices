using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public interface IBusinessObjectLogging
	{
		ZString GetLogReference();
	}

	public static class BusinessObjectLoggingExtension
	{
		public static ZString GetLogStatusShortDescription(this BusinessObject bizobject)
		{
			ZString result = ZString.Empty;
			ZString code = GetLogStatusCode(bizobject);

			if (code == Events.DeletedARecordInTheSystem.Code)
			{
				result = DeleteLogStatus;
			}
			else if (code == Events.AddedARecordToTheSystem.Code)
			{
				result = AddedLogStatus;
			}
			else if (code == Events.EditedARecord.Code)
			{
				result = EditedLogStatus;
			}

			return result;
		}

		public static ZString DeleteLogStatus => Res.GetString("0141A400-8C67-40BE-AE78-FA75E9A3165C", "Deleted");

		public static ZString AddedLogStatus => Res.GetString("AD783D32-B8F1-409B-BF67-D2A68EF00134", "Added");

		public static ZString EditedLogStatus => Res.GetString("8D0971D7-CB6A-463F-9B11-549137351F21", "Edited");

		public static ZString GetLogInfo(this BusinessObject bizObject, Func<string, bool, string> logFormat)
		{
			if (bizObject.GetLogStatusCode() == Events.EditedARecord.Code)
			{
				var originalValues = logFormat(BusinessObjectLoggingExtension.DeleteLogStatus, true);

				var currentValues = logFormat(BusinessObjectLoggingExtension.AddedLogStatus, false);

				return originalValues + "\r\n" + currentValues;
			}
			else
			{
				return logFormat(bizObject.GetLogStatusShortDescription(), false);
			}
		}

		public static ZString GetLogStatusCode(this BusinessObject bizobject)
		{
			return bizobject.GetLogStatus().Code;
		}

		public static Event GetLogStatus(this BusinessObject bizObject)
		{
			if (bizObject.IsDeleting)
			{
				return Events.DeletedARecordInTheSystem;
			}

			if (IsHasChangesFalseOnAllProperties(bizObject))
			{
				return Events.AddedARecordToTheSystem;
			}
			return Events.EditedARecord;
		}

		static bool IsHasChangesFalseOnAllProperties(this BusinessObject bizobject)
		{
			foreach (ZPropertyInfo property in bizobject.ZPropertyInfoHash)
			{
				if (property.HasChanges)
				{
					return false;
				}
			}
			return true;
		}

		public static T GetOriginalValue<T>(this BusinessObject bizObject, ZGuid originalPK, T currentValue) where T : BusinessObject
		{
			if (originalPK == currentValue?.PK)
			{
				return currentValue;
			}
			return bizObject.Factory.Load<T>(originalPK);
		}
	}
}

