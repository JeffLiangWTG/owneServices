using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class PropertyChangeLogger : IService
	{
		#region GetInstance

		protected PropertyChangeLogger(BusinessObjectFactory factory)
		{
			this.factory = factory;
			propertyChangeSubscriptionList = PropertyChangeSubscriptionList.GetInstance(factory);
			factory.Saving += Factory_Saving;
			factory.Saved += Factory_Saved;
		}

		public static PropertyChangeLogger GetInstance(BusinessObjectFactory factory)
		{
			PropertyChangeLogger result = LastPropertyChangeLogger.Target as PropertyChangeLogger;
			if (result == null || result.factory != factory)
			{
				result = factory.ServiceContainer.GetService<PropertyChangeLogger>();
				if (result == null)
				{
					result = new PropertyChangeLogger(factory);
					factory.ServiceContainer.AddService(result);
				}
				LastPropertyChangeLogger.Target = result;
			}
			return result;
		}

		static WeakReference LastPropertyChangeLogger
		{
			get { return lastPropertyChangeLogger ?? (lastPropertyChangeLogger = new WeakReference(null)); }
		}

		[ThreadStatic]
		static WeakReference lastPropertyChangeLogger;

#if DEBUG
		internal static void SetInstance(BusinessObjectFactory factory, PropertyChangeLogger value)
		{
			factory.ServiceContainer.RemoveService<PropertyChangeLogger>();
			factory.ServiceContainer.AddService(value);
		}
#endif

		#endregion

		#region Initialize

		public static void Initialize()
		{
			if (!isInitialized)
			{
				PropertyChangeSubscription.PropertyChanged += PropertyChangeSubscription_PropertyChanged;
				isInitialized = true;
			}
		}

		public static bool IsInitialized
		{
			get { return isInitialized; }
		}
		[ThreadStatic]
		static bool isInitialized;

		static void PropertyChangeSubscription_PropertyChanged(object sender, ZPropertyValueChangedEventArgs e)
		{
			LogChange(e.Property, e.OldValue);
		}

		#endregion

		#region LogChange

		static void LogChange(ZPropertyInfo property, IZType oldValue)
		{
			BusinessObjectFactory factory = property.BizObj.Factory;
			if (factory != null)
			{
				PropertyChangeLogger logger = GetInstance(factory);
				logger.LogChangeCore(property, oldValue);
			}
		}

#if DEBUG
		protected virtual
#endif
		void LogChangeCore(ZPropertyInfo property, IZType oldValue)
		{
			List<OldNewValues> properties = (List<OldNewValues>)changedProperties[property.BizObj];
			OldNewValues existingProperty = properties != null ? Find(properties, property.Name) : null;
			if (existingProperty != null)
			{
				existingProperty.NewValue = property.Value;
			}
			else if (property.BizObj.HasChanges && propertyChangeSubscriptionList.ShouldLogChanges(property))
			{
				if (properties == null)
				{
					properties = new List<OldNewValues>();
					changedProperties[property.BizObj] = properties;
				}
				properties.Add(new OldNewValues(property.Name, oldValue, property.Value));
			}
		}

		OldNewValues Find(IEnumerable<OldNewValues> properties, string propertyName)
		{
			return properties.FirstOrDefault(next => next.PropertyName == propertyName);
		}

		#endregion

		#region Factory Saving / Saved

		void Factory_Saving(BusinessObjectFactory factory)
		{
			foreach (DictionaryEntry entry in changedProperties)
			{
				var bizObj = (BusinessObject)entry.Key;
				var changeList = (List<OldNewValues>)entry.Value;
				if ((bizObj.IsInDatabase || WorkflowDataRegistry.Instance.AllowFieldChangeTriggersToFireForNewJobs.Value) && changeList.Count > 0 && !bizObj.IsDeleted)
				{
					CreateChangeLog(bizObj, changeList);
				}
			}
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (!savedSuccessfully)
			{
				foreach (StmChangeLog fieldChangeLog in toDeleteOnSaveFailure)
				{
					fieldChangeLog.Delete();
				}
			}
			else
			{
				changedProperties.Clear();
			}
			toDeleteOnSaveFailure.Clear();
		}

		void CreateChangeLog(BusinessObject bizObj, List<OldNewValues> changeList)
		{
			bizObj = GetParentForAddInfo(bizObj);
			StmChangeLog changeLog = factory.New<StmChangeLog>();
			changeLog.SY_ParentID = bizObj.PK;
			changeLog.SY_ParentTableCode = bizObj.TablePrefix;
			changeLog.SY_GS_NKUser = GlbStaff.CurrentUser.GS_Code;
			toDeleteOnSaveFailure.Add(changeLog);

			bool fieldChangeLogged = false;
			foreach (OldNewValues change in changeList)
			{
				if (!object.Equals(change.OldValue, change.NewValue))
				{
					CreateFieldChangeLog(changeLog, change);
					fieldChangeLogged = true;
				}
			}
			if (!fieldChangeLogged)
			{
				changeLog.Delete();
			}
		}

		BusinessObject GetParentForAddInfo(BusinessObject bizObj)
		{
			if (bizObj is Enterprise.Integration.Customs.IAddInfoBase)
			{
				var parent = bizObj as Enterprise.Integration.Customs.IAddInfoBase;
				if (parent != null && parent.Parent != null)
				{
					bizObj = parent.Parent;
				}
			}
			return bizObj;
		}

		void CreateFieldChangeLog(StmChangeLog changeLog, OldNewValues oldNewValues)
		{
			StmFieldChangeLog fieldChangeLog = changeLog.FieldChanges.AddNew();
			fieldChangeLog.PropertyName = oldNewValues.PropertyName;
			fieldChangeLog.OldValue = oldNewValues.OldValue;
			fieldChangeLog.NewValue = oldNewValues.NewValue;
		}

		#endregion

		#region Implementation

		readonly BusinessObjectFactory factory;
		readonly PropertyChangeSubscriptionList propertyChangeSubscriptionList;
		readonly List<StmChangeLog> toDeleteOnSaveFailure = new List<StmChangeLog>();
		readonly HybridDictionary changedProperties = new HybridDictionary();

		class OldNewValues
		{
			public OldNewValues(string propertyName, IZType oldValue, IZType newValue)
			{
				PropertyName = propertyName;
				OldValue = oldValue;
				NewValue = newValue;
			}

			public readonly string PropertyName;
			public readonly IZType OldValue;
			public IZType NewValue;
		}

		#endregion
	}
}
