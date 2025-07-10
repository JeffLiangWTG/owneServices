using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	[SingleObjectAroundARow]
	public sealed class GenAddOnColumn : AutoGenAddOnColumn, IAddOnColumn
	{
		public GenAddOnColumn(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static void DeleteInstances(BusinessObject parent)
		{
			if (SystemDefinedValuesAttribute.IsEnabled(parent))
			{
				GenAddOnColumnCollection collection = new GenAddOnColumnCollection(parent);
				collection.DeleteAll();
			}
		}

		public BusinessObject Parent
		{
			get => fParent == null || fParent.PK != XA_ParentID || fParent.TablePrefix != XA_ParentTableCode ? null : fParent;
			set
			{
				if (fParent != value)
				{
					fParent = value;
					var parentID = fParent?.PK ?? ZGuid.Empty;
					var parentTableCode = fParent?.TablePrefix ?? ZString.Empty;
					if (XA_ParentID != parentID)
					{
						XA_ParentID = parentID;
					}
					if (XA_ParentTableCode != parentTableCode)
					{
						XA_ParentTableCode = parentTableCode;
					}
				}
			}
		}
		BusinessObject fParent;

		[LightValidationTestExempt]
		[BusinessObjectTestExclude]
		public override ZString XA_ParentTableCode
		{
			get { return base.XA_ParentTableCode; }
			set
			{
				ZString oldValue = XA_ParentTableCode;
				BusinessObject oldParent = Parent;
				base.XA_ParentTableCode = value;
				if (!IsCopying && oldValue != XA_ParentTableCode)
				{
					MarkParentAsNeedingValidationIfNeeded(oldParent);
					MarkParentAsNeedingValidationIfNeeded(Parent);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is an exception message for developers")]
		public const string CustomValuesNotDefined = "CustomValues attribute must be defined on your object";

		public override ZString XA_Data
		{
			get { return base.XA_Data; }
			set
			{
				ZString oldValue = XA_Data;
				BusinessObject oldParent = Parent;
				base.XA_Data = value;
				if (!IsCopying && oldValue != XA_Data)
				{
					MarkParentAsNeedingValidationIfNeeded(oldParent);
					MarkParentAsNeedingValidationIfNeeded(Parent);
				}
			}
		}

		public override ZString XA_Name
		{
			get { return base.XA_Name; }
			set
			{
				ZString oldValue = XA_Name;
				BusinessObject oldParent = Parent;
				base.XA_Name = value;
				if (!IsCopying && oldValue != XA_Name)
				{
					MarkParentAsNeedingValidationIfNeeded(oldParent);
					MarkParentAsNeedingValidationIfNeeded(Parent);
				}
			}
		}

		public override ZGuid XA_ParentID
		{
			get { return base.XA_ParentID; }
			set
			{
				ZGuid oldValue = XA_ParentID;
				BusinessObject oldParent = Parent;
				base.XA_ParentID = value;
				if (!IsCopying && oldValue != XA_ParentID)
				{
					MarkParentAsNeedingValidationIfNeeded(oldParent);
					MarkParentAsNeedingValidationIfNeeded(Parent);
				}
			}
		}

		public override ZString XA_Type
		{
			get { return base.XA_Type; }
			set
			{
				ZString oldValue = XA_Type;
				BusinessObject oldParent = Parent;
				base.XA_Type = value;
				if (!IsCopying && oldValue != XA_Type)
				{
					MarkParentAsNeedingValidationIfNeeded(oldParent);
					MarkParentAsNeedingValidationIfNeeded(Parent);
				}
			}
		}

		void MarkParentAsNeedingValidationIfNeeded(BusinessObject parent)
		{
			if (parent != null)
			{
				parent.MarkAsNeedingValidation();
			}
		}

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new GenAddOnColumnFetchStrategy(this);
		}

		public class GenAddOnColumnFetchStrategy : EnterpriseBusinessObjectFetchStrategy
		{
			public GenAddOnColumnFetchStrategy(GenAddOnColumn addOnColumn)
				: base(addOnColumn)
			{
			}

			protected new GenAddOnColumn BusinessObject => (GenAddOnColumn)base.BusinessObject;

			protected override void FetchForFactorySaveCore()
			{
				base.FetchForFactorySaveCore();

				if (!BusinessObject.IsInDatabase && (BusinessObject.Parent?.IsInDatabase ?? false))
				{
					Factory.AddFetchHint(GenAddOnColumnSchema.Instance, GetDuplicateQuery(BusinessObject));
				}
			}
		}

		#endregion

		public override void OnSaving()
		{
			base.OnSaving();

			if (XA_Data.IsEmpty)
			{
				Delete();
			}
			else if (!IsInDatabase)
			{
				var duplicate = Factory.LoadTop1<GenAddOnColumn>(GetDuplicateQuery(this, shouldReload: false));
				if (duplicate != null)
				{
					duplicate.XA_Data = XA_Data;
					Delete();
				}
			}
		}

		static ZQuery GetDuplicateQuery(GenAddOnColumn addOnColumn, bool shouldReload = true)
		{
			var duplicateQuery = new ZQuery(GenAddOnColumnSchema.XA_ParentID, addOnColumn.XA_ParentID);
			duplicateQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, addOnColumn.XA_Name);
			duplicateQuery.AddToFilter(GenAddOnColumnSchema.PK, SQLComparisonOperator.NotEqual, addOnColumn.PK);
			duplicateQuery.ReLoadExistingRows = shouldReload;
			duplicateQuery.FetchOnlyFromLocalCache = !(addOnColumn.Parent?.IsInDatabase ?? true);
			return duplicateQuery;
		}

		public override bool SupportsNotes => false;

		#region IAddOnColumn Members
		BusinessObject IAddOnColumn.Parent
		{
			get => Parent;
			set => Parent = value;
		}

		#endregion

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new GenAddOnColumnUniqueIndexFailureHandler(this); }
		}

		class GenAddOnColumnUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public GenAddOnColumnUniqueIndexFailureHandler(GenAddOnColumn addOnColumn)
			{
				AddOnColumn = addOnColumn;
			}
			readonly GenAddOnColumn AddOnColumn;

			#region IUniqueIndexFailureHandler Members

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return GenAddOnColumnSchema.Constants.Indexes.NR_UC__XA_ParentID_XA_Name; }
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				var addOnColumnInDB = AddOnColumn.Factory.LoadTop1<GenAddOnColumn>(GetDuplicateQuery(AddOnColumn, true));

				if (addOnColumnInDB == null)
				{
					return;
				}
				else
				{
					var parentBOName = AddOnColumn.Parent?.HumanReadableName ?? ZString.Empty;
					addOnColumnInDB.XA_Data = AddOnColumn.XA_Data;

					AddOnColumn.Delete();

					notifier?.ReportInformation(Res.GetString("dde6c8bd-5221-4a04-b101-eb01e50535fd", "While you were working, the {0} was modified by another user. The system will now need to reload this information. Press OK to have this information loaded and then try saving again.", parentBOName), Res.GetString("35b53e57-fe7f-43c3-a728-3e8bad62c624", "Update Required"));
				}
			}

			#endregion
		}

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
				if (!property.Name.Equals(Schema.XA_ParentTableCode))
				{
					base.PopulateString(property);
				}
			}
		}

#endif
		#endregion
	}
}
