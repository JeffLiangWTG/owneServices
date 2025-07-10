using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Tracking.Business
{
	#region Auto

	public abstract class AutoOrgSupplierPartFilterBusinessObject : FilterBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string TableName = "OrgSupplierPartFilterBusinessObject";
			public const string PK = "PK";

			public const string Active = "Active";
			public const string Both = "Both";
			public const string Buyer = "Buyer";
			public const string CodeContains = "CodeContains";
			public const string CodeExact = "CodeExact";
			public const string CodeStartWith = "CodeStartWith";
			public const string DescContains = "DescContains";
			public const string DescExact = "DescExact";
			public const string DescStartsWith = "DescStartsWith";
			public const string Inactive = "Inactive";

			public const string OP_Desc = "OP_Desc";
			public const string OP_PartNum = "OP_PartNum";

			public const string OrgAnd = "OrgAnd";
			public const string OrgOr = "OrgOr";
			public const string Supplier = "Supplier";
		}

		#endregion

		protected AutoOrgSupplierPartFilterBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region PK

		public override string PK_ColumnName
		{
			get { return Schema.PK; }
		}

		#endregion

		#region Properties

		#region Active

		public virtual ZBool Active
		{
			get { return new ZBool(BizOInternals.GetValueFromRowSafely(ActiveInfo)); }
			set
			{
				SetPropertyValue(ActiveInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateActive();
				}
			}
		}

		public virtual void ValidateActive()
		{
			ActiveInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo ActiveInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.Active); }
		}

		#endregion

		#region Both

		public virtual ZBool Both
		{
			get { return new ZBool(BizOInternals.GetValueFromRowSafely(BothInfo)); }
			set
			{
				SetPropertyValue(BothInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateBoth();
				}
			}
		}

		public virtual void ValidateBoth()
		{
			BothInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo BothInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.Both); }
		}

		#endregion

		#region Buyer

		public virtual ZGuid Buyer
		{
			get { return new ZGuid(BizOInternals.GetValueFromRowSafely(BuyerInfo)); }
			set
			{
				SetPropertyValue(BuyerInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateBuyer();
				}
			}
		}

		public virtual void ValidateBuyer()
		{
			BuyerInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(BuyerInfo);
		}

		public virtual ZPropertyInfo BuyerInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.Buyer); }
		}

		#endregion

		#region CodeContains

		public virtual ZBool CodeContains
		{
			get { return new ZBool(BizOInternals.GetValueFromRowSafely(CodeContainsInfo)); }
			set
			{
				SetPropertyValue(CodeContainsInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateCodeContains();
				}
			}
		}

		public virtual void ValidateCodeContains()
		{
			CodeContainsInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo CodeContainsInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.CodeContains); }
		}

		#endregion

		#region CodeExact

		public virtual ZBool CodeExact
		{
			get { return new ZBool(BizOInternals.GetValueFromRowSafely(CodeExactInfo)); }
			set
			{
				SetPropertyValue(CodeExactInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateCodeExact();
				}
			}
		}

		public virtual void ValidateCodeExact()
		{
			CodeExactInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo CodeExactInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.CodeExact); }
		}

		#endregion

		#region CodeStartWith

		public virtual ZBool CodeStartWith
		{
			get { return new ZBool(BizOInternals.GetValueFromRowSafely(CodeStartWithInfo)); }
			set
			{
				SetPropertyValue(CodeStartWithInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateCodeStartWith();
				}
			}
		}

		public virtual void ValidateCodeStartWith()
		{
			CodeStartWithInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo CodeStartWithInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.CodeStartWith); }
		}

		#endregion

		#region DescContains

		public virtual ZBool DescContains
		{
			get { return new ZBool(BizOInternals.GetValueFromRowSafely(DescContainsInfo)); }
			set
			{
				SetPropertyValue(DescContainsInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateDescContains();
				}
			}
		}

		public virtual void ValidateDescContains()
		{
			DescContainsInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo DescContainsInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.DescContains); }
		}

		#endregion

		#region DescExact

		public virtual ZBool DescExact
		{
			get { return new ZBool(BizOInternals.GetValueFromRowSafely(DescExactInfo)); }
			set
			{
				SetPropertyValue(DescExactInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateDescExact();
				}
			}
		}

		public virtual void ValidateDescExact()
		{
			DescExactInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo DescExactInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.DescExact); }
		}

		#endregion

		#region DescStartsWith

		public virtual ZBool DescStartsWith
		{
			get { return new ZBool(BizOInternals.GetValueFromRowSafely(DescStartsWithInfo)); }
			set
			{
				SetPropertyValue(DescStartsWithInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateDescStartsWith();
				}
			}
		}

		public virtual void ValidateDescStartsWith()
		{
			DescStartsWithInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo DescStartsWithInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.DescStartsWith); }
		}

		#endregion

		#region Inactive

		public virtual ZBool Inactive
		{
			get { return new ZBool(BizOInternals.GetValueFromRowSafely(InactiveInfo)); }
			set
			{
				SetPropertyValue(InactiveInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateInactive();
				}
			}
		}

		public virtual void ValidateInactive()
		{
			InactiveInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo InactiveInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.Inactive); }
		}

		#endregion

		#region OP_Desc

		public virtual ZString OP_Desc
		{
			get { return new ZString(BizOInternals.GetValueFromRowSafely(OP_DescInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(OP_DescInfo, value);
				SetPropertyValue(OP_DescInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateOP_Desc();
				}
			}
		}

		public virtual void ValidateOP_Desc()
		{
			OP_DescInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo OP_DescInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.OP_Desc); }
		}

		#endregion

		#region OP_PartNum

		public virtual ZString OP_PartNum
		{
			get { return new ZString(BizOInternals.GetValueFromRowSafely(OP_PartNumInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(OP_PartNumInfo, value);
				SetPropertyValue(OP_PartNumInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateOP_PartNum();
				}
			}
		}

		public virtual void ValidateOP_PartNum()
		{
			OP_PartNumInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo OP_PartNumInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.OP_PartNum); }
		}

		#endregion

		#region OrgAnd

		public virtual ZBool OrgAnd
		{
			get { return new ZBool(BizOInternals.GetValueFromRowSafely(OrgAndInfo)); }
			set
			{
				SetPropertyValue(OrgAndInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateOrgAnd();
				}
			}
		}

		public virtual void ValidateOrgAnd()
		{
			OrgAndInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo OrgAndInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.OrgAnd); }
		}

		#endregion

		#region OrgOr

		public virtual ZBool OrgOr
		{
			get { return new ZBool(BizOInternals.GetValueFromRowSafely(OrgOrInfo)); }
			set
			{
				SetPropertyValue(OrgOrInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateOrgOr();
				}
			}
		}

		public virtual void ValidateOrgOr()
		{
			OrgOrInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo OrgOrInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.OrgOr); }
		}

		#endregion

		#region Supplier

		public virtual ZGuid Supplier
		{
			get { return new ZGuid(BizOInternals.GetValueFromRowSafely(SupplierInfo)); }
			set
			{
				SetPropertyValue(SupplierInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateSupplier();
				}
			}
		}

		public virtual void ValidateSupplier()
		{
			SupplierInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(SupplierInfo);
		}

		public virtual ZPropertyInfo SupplierInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.Supplier); }
		}

		#endregion

		#endregion

		#region Set Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			DataRow row = ((IBusinessObjectInternals)this).Row;
			row[Schema.Active] = false;
			row[Schema.Both] = false;
			row[Schema.Buyer] = DBNull.Value;
			row[Schema.CodeContains] = false;
			row[Schema.CodeExact] = false;
			row[Schema.CodeStartWith] = false;
			row[Schema.DescContains] = false;
			row[Schema.DescExact] = false;
			row[Schema.DescStartsWith] = false;
			row[Schema.Inactive] = false;
			row[Schema.OP_Desc] = "";
			row[Schema.OP_PartNum] = "";
			row[Schema.OrgAnd] = false;
			row[Schema.OrgOr] = false;
			row[Schema.Supplier] = DBNull.Value;
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			ValidateActive();
			ValidateBoth();
			ValidateBuyer();
			ValidateCodeContains();
			ValidateCodeExact();
			ValidateCodeStartWith();
			ValidateDescContains();
			ValidateDescExact();
			ValidateDescStartsWith();
			ValidateInactive();
			ValidateOP_Desc();
			ValidateOP_PartNum();
			ValidateOrgAnd();
			ValidateOrgOr();
			ValidateSupplier();

			base.RunPreSaveValidationCore(); // call RunPreSaveValidation() on all children then fire OnNotificationsChanged()
		}

		#endregion

		#region Filters

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				query.AddToFilter(ProductDescriptionGroupBoxFilter);
				query.AddToFilter(ProductCodeGroupBoxFilter);
				query.AddToFilter(ActiveFilterGroupBoxFilter);
				query.AddToFilter(OrganisationalDetailsGroupBoxFilter);

				return query;
			}
		}

		protected virtual ZQuery ProductDescriptionGroupBoxFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				query.AddToFilter(ProductDescPanelFilter);

				return query;
			}
		}

		protected virtual ZQuery ProductDescPanelFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				return query;
			}
		}

		protected virtual ZQuery ProductCodeGroupBoxFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				query.AddToFilter(ProductCodePanelFilter);

				return query;
			}
		}

		protected virtual ZQuery ProductCodePanelFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				return query;
			}
		}

		protected virtual ZQuery ActiveFilterGroupBoxFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				return query;
			}
		}

		protected virtual ZQuery OrganisationalDetailsGroupBoxFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				return query;
			}
		}

		#endregion
	}

	#endregion

	[CodeAlive("Used Code")]
	public class OldOrgSupplierPartFilterBusinessObject : AutoOrgSupplierPartFilterBusinessObject
	{
		public OldOrgSupplierPartFilterBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZQuery ProductCodePanelFilter
		{
			get { return BuildQuery(CodeContains, CodeStartWith, CodeExact, OP_PartNum, OrgSupplierPartSchema.OP_PartNum); }
		}

		protected override ZQuery ProductDescPanelFilter
		{
			get { return BuildQuery(DescContains, DescStartsWith, DescExact, OP_Desc, OrgSupplierPartSchema.OP_Desc); }
		}

		public static string ExpensiveQueryWarningText
		{
			get { return Enterprise.Tracking.Web.Res.GetString("a8db75ca-956c-4da5-86c9-d90411a23a43", "The search parameters entered include an Organization. Searching on this field may take a long time. Do you wish to continue?"); }
		}

		#region BuildQuery

		ZQuery BuildQuery(ZBool contains, ZBool startsWith, ZBool exact, ZString key, SchemaColumn column)
		{
			ZQuery query = new ZQuery();

			SQLComparisonOperator comparisonOperator = null;

			if (contains)
			{
				comparisonOperator = SQLComparisonOperator.Contains;
			}
			else if (startsWith)
			{
				comparisonOperator = SQLComparisonOperator.StartsWith;
			}
			else if (exact)
			{
				comparisonOperator = SQLComparisonOperator.Equal;
			}

			if (comparisonOperator != null && key.Trim() != String.Empty)
			{
				query.AddToFilter(column, comparisonOperator, key);
			}

			return query;
		}

		#endregion
		#region Lookups

		public OrgHeaderCollection BuyerList
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public OrgHeaderCollection SupplierList
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		#endregion

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			if (!Active && !Inactive && !Both)
			{
				Both = true;
			}

			if (!CodeExact && !CodeContains)
			{
				CodeStartWith = true;
			}

			if (!DescExact && !DescStartsWith)
			{
				DescContains = true;
			}

			if (!OrgAnd && !OrgOr)
			{
				OrgAnd = CustomsDataRegistry.Instance.EnableExactMatchForProduct.Value;
				OrgOr = !OrgAnd;
			}
		}

		public override bool IsExpensiveQuery
		{
			get { return CodeContains && !OP_PartNum.IsEmpty; }
		}

		public override string ExpensiveQueryWarning
		{
			get { return ExpensiveQueryWarningText; }
		}

		#endregion

		#region Filters

		#region Org Details

		protected override ZQuery OrganisationalDetailsGroupBoxFilter
		{
			get
			{
				ZQuery result = base.OrganisationalDetailsGroupBoxFilter;
				if (OrgAnd && !Buyer.IsEmpty && !Supplier.IsEmpty)
				{
					result.AddToFilter(OrgSupplierPartCollection.GetSupplierOwnerMatchesQuery(Supplier, Buyer));
				}
				else
				{
					if (!Buyer.IsEmpty)
					{
						ZQuery buyerQuery = GetProductRelationshipFilter(Buyer, OrgPartRelation.RelationshipTypes.Owner);
						result.AddToFilter(buyerQuery);
					}
					if (!Supplier.IsEmpty)
					{
						ZQuery supplierQuery = GetProductRelationshipFilter(Supplier, OrgPartRelation.RelationshipTypes.Supplier);
						result.AddToFilter(supplierQuery, JoinCondition.Or);
					}
				}
				return result;
			}
		}

		ZQuery GetProductRelationshipFilter(ZGuid orgPK, ZString relationship)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			ZDBOnlySubQuery subQueryOrganisation = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
			subQueryOrganisation.AddToFilter(OrgPartRelationSchema.OU_OH, orgPK);
			if (relationship == OrgPartRelation.RelationshipTypes.Owner || relationship == OrgPartRelation.RelationshipTypes.Supplier)
			{
				ZQuery bothRelationship = new ZQuery(OrgPartRelationSchema.OU_Relationship, SQLComparisonOperator.Equal, relationship);
				bothRelationship.AddToFilter(JoinCondition.Or, OrgPartRelationSchema.OU_Relationship, SQLComparisonOperator.Equal, OrgPartRelation.RelationshipTypes.Both);
				subQueryOrganisation.AddToFilter(bothRelationship, JoinCondition.And);
			}
			else
			{
				subQueryOrganisation.AddToFilter(JoinCondition.And, OrgPartRelationSchema.OU_Relationship, SQLComparisonOperator.Equal, relationship);
			}
			result.AddSubQuery(subQueryOrganisation, JoinCondition.And);
			return result;
		}

		#endregion

		#region Active / Inactive

		protected override ZQuery ActiveFilterGroupBoxFilter
		{
			get
			{
				ZQuery query = new ZQuery();

				if (Active)
				{
					query.AddToFilter(OrgSupplierPartSchema.OP_IsActive, ZBool.True);
				}
				else if (Inactive)
				{
					query.AddToFilter(OrgSupplierPartSchema.OP_IsActive, ZBool.False);
				}

				return query;
			}
		}

		#endregion

		#endregion
	}
}
