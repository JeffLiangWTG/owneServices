using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EntityFramework.BusinessObjectFactory;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	[UniversalCopyWithExtendedEntities]
	public class GenCustomAddOnValue : AutoGenCustomAddOnValue, ICustomColumnDefinitionProvider, IAddOnColumn, IPartOfParentChangesForAdminLogging
	{
		public GenCustomAddOnValue(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		internal static void DeleteInstances(BusinessObject parent)
		{
			if (UserDefinedValuesAttribute.IsEnabled(parent))
			{
				var query = new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, parent.PK);
				query.FetchOnlyFromLocalCache = !parent.IsInDatabase;
				foreach (var obj in parent.Factory.Load<GenCustomAddOnValue>(query))
				{
					obj.Delete();
				}
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (!Globals.IsUserInteractive)
			{
				ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(XV_Data), ConcurrencyPolicy.Ignore);
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			OnGenCustomAddOnValueOnSaved(saveSucceeded);
		}

		protected virtual void OnGenCustomAddOnValueOnSaved(bool saveSucceeded)
		{
			GenCustomAddOnValueOnSaved?.Invoke(this.Factory, saveSucceeded);
		}

		public event SavedEventHandler GenCustomAddOnValueOnSaved;

		#region Properties override

		[UniversalCopyExtraMetadata(IsMandatory = true)]
		public override ZString XV_Name
		{
			get { return base.XV_Name; }
			set { base.XV_Name = value; }
		}

		[UniversalCopyExtraMetadata(IsMandatory = true)]
		public override ZString XV_Type
		{
			get { return base.XV_Type; }
			set { base.XV_Type = value; }
		}

		[MaxLength(AutoGenCustomAddOnValue.Schema.XV_DataMaxLength)]
		public override ZString XV_Data
		{
			get { return base.XV_Data; }
			set { base.XV_Data = value; }
		}

		#endregion

		#region Bizo overrides

		public override bool IsSavedByFactory
		{
			get { return base.IsSavedByFactory && (IsDeleted || (!XV_Name.IsEmpty && !XV_Type.IsEmpty)); }
		}

		public override bool SupportsNotes => false;

		protected override bool SupportsCloneCore() => true;

		#endregion

		#region ICustomColumnDefinitionProvider Members

		public ICustomColumnDefinition GetCustomColumnDefinition()
		{
			return new CustomColumnDefinition(
				PK,
				XV_Name,
				XV_Name,
				XV_Type,
				GenCustomAddOnValue.Schema.XV_DataMaxLength,
				sequence: null,
				XV_XR_Rule,
				Factory);
		}

		#endregion

		#region Unique index handling

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new UniqueValueOnParentUniqueIndexFailureHandler(this); }
		}

		class UniqueValueOnParentUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public UniqueValueOnParentUniqueIndexFailureHandler(GenCustomAddOnValue customAddOnValue)
			{
				this.customAddOnValue = customAddOnValue;
			}

			readonly GenCustomAddOnValue customAddOnValue;

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				var conflictingValuesQuery = new ZQuery(GenCustomAddOnValueSchema.XV_Name, customAddOnValue.XV_Name);
				conflictingValuesQuery.AddToFilter(GenCustomAddOnValueSchema.XV_ParentID, customAddOnValue.XV_ParentID);
				conflictingValuesQuery.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, customAddOnValue.XV_ParentTableCode);
				conflictingValuesQuery.AddToFilter(GenCustomAddOnValueSchema.PK, SQLComparisonOperator.NotEqual, customAddOnValue.PK);
				conflictingValuesQuery.ReLoadExistingRows = true;

				foreach (var conflictingValue in customAddOnValue.Factory.Load<GenCustomAddOnValue>(conflictingValuesQuery))
				{
					conflictingValue.Delete();
				}

				notifier.ReportInformation(
					Res.GetString("620ff537-082a-40f8-b38b-3d47f5df522d", "You changes to custom field '{0}' conflict with changes made by other user. If you want to override value entered by other user, press OK and try to save again. Otherwise cancel your changes and reopen this form to load already existing value.", customAddOnValue.XV_Name),
					Res.GetString("23068264-367c-486b-be23-54a07e2c8e17", "Custom field concurrency conflict"));
			}

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return GenCustomAddOnValueSchema.Constants.Indexes.NR_UX__XV_ParentID_XV_ParentTableCode_XV_Name_XV_Type; }
			}
		}

		#endregion

		#region IAddOnColumn Members
		BusinessObject IAddOnColumn.Parent
		{
			get => parent == null || parent.PK != XV_ParentID || parent.TablePrefix != XV_ParentTableCode ? null : parent;
			set
			{
				if (parent != value)
				{
					parent = value;
					var parentID = parent?.PK ?? ZGuid.Empty;
					var parentTableCode = parent?.TablePrefix ?? ZString.Empty;
					if (XV_ParentID != parentID)
					{
						XV_ParentID = parentID;
					}
					if (XV_ParentTableCode != parentTableCode)
					{
						XV_ParentTableCode = parentTableCode;
					}
				}
			}
		}

		BusinessObject parent;

		#endregion

		#region Test Helpers
#if DEBUG

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new MyBusinessObjectTestDataHelper();
		}

		class MyBusinessObjectTestDataHelper : BusinessObjectTestDataHelper
		{
			protected override void PopulateString(ZPropertyInfo property)
			{
				if (!property.Name.Equals(Schema.XV_ParentTableCode))
				{
					base.PopulateString(property);
				}
			}
		}

#endif
		#endregion
	}
}
