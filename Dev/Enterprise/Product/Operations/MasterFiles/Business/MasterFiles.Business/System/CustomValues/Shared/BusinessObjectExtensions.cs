using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public static class BusinessObjectExtensions
	{
		public static IEnumerable<IPropertyValue> GetSystemDefinedValues(this BusinessObject businessObject)
		{
			return new SystemDefinedPropertyManager(businessObject).GetValues();
		}

		public static void SetSystemDefinedValue(this BusinessObject businessObject, string propertyName, string propertyType, IZType value)
		{
			new SystemDefinedPropertyManager(businessObject).SetValue(propertyName, propertyType, value);
		}

		public static void SetSystemDefinedValue(this BusinessObject businessObject, string propertyName, IZType value)
		{
			new SystemDefinedPropertyManager(businessObject).SetValue(propertyName, null, value);
		}

		public static T GetSystemDefinedValue<T>(this BusinessObject businessObject, string propertyName, bool reLoadData = false)
		{
			return new SystemDefinedPropertyManager(businessObject).GetValue<T>(propertyName, reLoadData);
		}

		public static IEnumerable<IPropertyValue> GetUserDefinedValues(this BusinessObject businessObject)
		{
			return new UserDefinedPropertyManager(businessObject).GetValues();
		}

		public static void SetUserDefinedValue(this BusinessObject businessObject, string propertyName, string propertyType, IZType value, INotifications notifications = null)
		{
			var propertyManager = new UserDefinedPropertyManager(businessObject);
			var customAddOnValue = propertyManager.SetValue(propertyName, propertyType, value, notifications);
			TryUpdateCustomBusinessObject(businessObject, customAddOnValue, propertyManager);
		}

		public static void SetUserDefinedValue(this BusinessObject businessObject, string propertyName, IZType value, INotifications notifications = null)
		{
			var propertyManager = new UserDefinedPropertyManager(businessObject);
			var customAddOnValue = propertyManager.SetValue(propertyName, null, value, notifications);
			TryUpdateCustomBusinessObject(businessObject, customAddOnValue, propertyManager);
		}

		public static void SetUserDefinedValue(this BusinessObject businessObject, string propertyName, string propertyType, IZType value, IXmlImportLogger logger)
		{
			var propertyManager = new UserDefinedPropertyManager(businessObject);
			var customAddOnValue = propertyManager.SetValue(propertyName, propertyType, value, logger);
			TryUpdateCustomBusinessObject(businessObject, customAddOnValue, propertyManager);
		}

		public static T GetUserDefinedValue<T>(this BusinessObject businessObject, string propertyName)
		{
			return new UserDefinedPropertyManager(businessObject).GetValue<T>(propertyName);
		}

		public static GenCustomAddOnValue GetUserDefinedProperty(this BusinessObject businessObject, string propertyName, string propertyType)
		{
			return new UserDefinedPropertyManager(businessObject).GetProperty(propertyName, propertyType);
		}

		public static T[] GetRelatedObjectsViaPivot<T>(this BusinessObject businessObject, string genPivotType) where T : BusinessObject
		{
			var query = new ZDBOnlyQuery(typeof(T));
			var subQuery = new ZDBOnlySubQuery(typeof(GenPivot), GenPivotSchema.XX_Relation2ID);
			AddPivotFiltersToSubQuery<T>(businessObject, genPivotType, subQuery);

			query.AddSubQuery(subQuery, JoinCondition.And);
			return businessObject.Factory.Load<T>(query);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Current method doesn't require a new object with Type parameter to be created.")]
		public static void UpdatePivotsToRelatedObjects<T>(this BusinessObject businessObject, string genPivotType, params ZGuid[] relatedObjectPKs)
		{
			var existingPivotsQuery = new ZQuery();
			AddPivotFiltersToSubQuery<T>(businessObject, genPivotType, existingPivotsQuery);

			var existingPivots = businessObject.Factory.Load<GenPivot>(existingPivotsQuery);

			foreach (var relatedObjectPK in relatedObjectPKs)
			{
				var existingPivot = existingPivots.Where(x => x.XX_Relation2ID == relatedObjectPK).FirstOrDefault();

				if (existingPivot == null)
				{
					var newPivot = businessObject.Factory.New<GenPivot>();
					newPivot.XX_Relation1ID = businessObject.PK;
					newPivot.XX_Relation1TableCode = businessObject.TablePrefix;
					newPivot.XX_Relation2ID = relatedObjectPK;
					newPivot.XX_Relation2TableCode = BusinessObjectFactory.GetTableCodeFromType(typeof(T));
					newPivot.XX_RelationType = genPivotType;
				}
			}

			existingPivots.Where(x => !relatedObjectPKs.Contains(x.XX_Relation2ID)).DeleteAll();
		}

		static void AddPivotFiltersToSubQuery<T>(this BusinessObject businessObject, string genPivotType, ZQuery query)
		{
			query.AddToFilter(GenPivotSchema.XX_RelationType, genPivotType);
			query.AddToFilter(GenPivotSchema.XX_Relation1ID, businessObject.PK);
			query.AddToFilter(GenPivotSchema.XX_Relation1TableCode, businessObject.TablePrefix);
			query.AddToFilter(GenPivotSchema.XX_Relation2TableCode, BusinessObjectFactory.GetTableCodeFromType(typeof(T)));
		}

		static void TryUpdateCustomBusinessObject(BusinessObject businessObject, GenCustomAddOnValue value, UserDefinedPropertyManager propertyManager)
		{
			if (value != null && !value.IsDeleted)
			{
				var customBusinessObject = (businessObject as ICustomFieldProvider)?.GetCustomBusinessObject();
				if (customBusinessObject != null)
				{
					var properties = ICustomColumnDefinitionExtensions.CreatePropertyManagedCustomProperties(value.GetCustomColumnDefinition(), propertyManager, businessObject);
					customBusinessObject.AddCustomProperties(properties);
				}
			}
		}
	}
}
