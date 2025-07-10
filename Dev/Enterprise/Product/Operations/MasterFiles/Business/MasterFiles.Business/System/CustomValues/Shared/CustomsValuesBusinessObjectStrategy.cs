using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	class CustomsValuesBusinessObjectStrategy : IBusinessObjectStrategy
	{
		void IBusinessObjectStrategy.FetchForLoad(BusinessObject businessObject)
		{
			var factory = businessObject.Factory;
			if (factory != null && businessObject.IsInDatabase)
			{
				var type = businessObject.GetType();
				if (SystemDefinedValuesAttribute.IsEnabled(businessObject))
				{
					if (!factory.IsCustomsValuesFetchHintSuspended(type))
					{
						businessObject.Factory.AddFetchHint(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID, businessObject.PK);
					}
				}

				if (UserDefinedValuesAttribute.IsEnabled(businessObject))
				{
					if (!factory.IsCustomsValuesFetchHintSuspended(type))
					{
						businessObject.Factory.AddFetchHint(typeof(GenCustomAddOnValue), GenCustomAddOnValueSchema.XV_ParentID, businessObject.PK);
					}
				}
			}
		}

		void IBusinessObjectStrategy.OnDelete(BusinessObject businessObject)
		{
			var factory = businessObject.Factory;
			if (factory != null)
			{
				if (SystemDefinedValuesAttribute.IsEnabled(businessObject))
				{
					var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, businessObject.PK);
					query.FetchOnlyFromLocalCache = !businessObject.IsInDatabase;
					foreach (GenAddOnColumn bizO in factory.Load<GenAddOnColumn>(query))
					{
						bizO.Delete();
					}
				}

				if (UserDefinedValuesAttribute.IsEnabled(businessObject))
				{
					var query = new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, businessObject.PK);
					query.FetchOnlyFromLocalCache = !businessObject.IsInDatabase;
					foreach (GenCustomAddOnValue bizO in factory.Load<GenCustomAddOnValue>(query))
					{
						bizO.Delete();
					}
				}
			}
		}

		#region Unused

		void IBusinessObjectStrategy.BeforeSuccessfulDelete(BusinessObject businessObject)
		{
		}

		DeleteDetails IBusinessObjectStrategy.DeleteDetails(BusinessObject businessObject)
		{
			return null;
		}

		void IBusinessObjectStrategy.OnFactorySaved(BusinessObject businessObject, bool saveSucceeded)
		{
		}

		void IBusinessObjectStrategy.OnFactorySaving(BusinessObject businessObject)
		{
		}

		void IBusinessObjectStrategy.OnSaveRollback(BusinessObject businessObject)
		{
		}

		void IBusinessObjectStrategy.OnSaved(BusinessObject businessObject, bool saveSucceeded)
		{
		}

		void IBusinessObjectStrategy.OnSaving(BusinessObject businessObject)
		{
		}

		void IBusinessObjectStrategy.OnSavingInObjectsWithLateChanges(BusinessObject businessObject)
		{
		}

		#endregion
	}

	public static class CustomsValuesBusinessObjectStrategyHelper
	{
		public static IDisposable SuspendCustomsValuesFetchHint(this BusinessObjectFactory factory, Type businessObjectType)
		{
			IDisposable result = null;
			if (factory == null)
			{
				result = DisposableAction.NoAction;
			}
			else
			{
				var dictionary = factory.GetFetchHintSuspenders();
				var bizObjType = businessObjectType;
				result = new DisposableAction(() =>
				{
					if (dictionary.TryGetValue(bizObjType, out int count))
					{
						dictionary[bizObjType] = ++count;
					}
					else
					{
						dictionary.Add(bizObjType, 1);
					}
				}, () =>
				{
					if (dictionary.TryGetValue(bizObjType, out int count))
					{
						dictionary[bizObjType] = --count;
					}
				});
			}
			return result;
		}

		public static bool IsCustomsValuesFetchHintSuspended(this BusinessObjectFactory factory, Type bizObjType)
		{
			var result = false;
			if (factory != null && bizObjType != null)
			{
				var dictionary = factory.GetFetchHintSuspenders();
				var bizObjTypeToCheck = bizObjType;
				var enterpriseBusinessObjectType = typeof(EnterpriseBusinessObject);
				var businessObjectType = typeof(BusinessObject);
				while (bizObjTypeToCheck != null && bizObjTypeToCheck != enterpriseBusinessObjectType && bizObjTypeToCheck != businessObjectType)
				{
					if (dictionary.TryGetValue(bizObjTypeToCheck, out int count))
					{
						result = count > 0;
						break;
					}
					bizObjTypeToCheck = bizObjTypeToCheck.BaseType;
				}
			}
			return result;
		}

		static Dictionary<Type, int> GetFetchHintSuspenders(this BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("CustomsValuesBusinessObjectStrategyFetchHintSuspenders", () => new Dictionary<Type, int>());
		}
	}
}
