using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	class ReferenceModuleFilter : ModuleTextFilter
	{
		#region Construction

		public ReferenceModuleFilter(ZString description)
			: base(description, DummyQuery)
		{
		}

		#endregion

		#region ReferenceType

		[List("ReferenceTypeList")]
		public ZString ReferenceType
		{
			get { return refType; }
			set
			{
				refType = value;
				ReferenceTypeInfo.RefreshBinding();
				InvalidateCachedQuery();
			}
		}
		ZString refType;

		public ZPropertyInfo ReferenceTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ReferenceType)); }
		}

		#endregion

		#region Reference Type List

		public CodeDescriptionPairList ReferenceTypeList
		{
			get
			{
				if (referenceTypeList == null)
				{
					referenceTypeList = new InvoiceHeaderRefsTypeList();
				}
				return referenceTypeList;
			}
		}
		CodeDescriptionPairList referenceTypeList;

		#endregion

		#region Clear

		protected override void ClearCore()
		{
			base.ClearCore();
			ReferenceType = "";
		}

		#endregion

		#region Dummies

		static ZQuery DummyQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery();
		}

		#endregion

		#region Property_ReadOnly

		protected override bool Property_ReadOnly => base.Property_ReadOnly || ComparisonOperator == ComparisonConstants.NotFound;

		#endregion

		#region AllowedComparisonOperators

		public override IReadOnlyList<string> AllowedComparisonOperators
		{
			get
			{
				var operators = new List<string>(base.AllowedComparisonOperators);
				operators.Add(ComparisonConstants.NotFound);
				return operators;
			}
		}

		#endregion

		#region Query

		protected override ZQuery GetQuery()
		{
			ZQuery query = new ZQuery();

			if (!Property.IsEmpty || !ReferenceType.IsEmpty)
			{
				query.AddToFilter(GetReferenceQuery());
			}
			return query;
		}

		ZDBOnlyQuery GetReferenceQuery()
		{
			bool excludeMatchingReferences = ComparisonOperator == ComparisonConstants.NotFound;

			SQLComparisonOperator sqlOperatorToUse = SqlComparisonOperator;
			ZString referenceNumber = Property;

			ZDBOnlyQuery headerQuery = new ZDBOnlyQuery(typeof(BaseJobComInvoiceHeader));

			ZDBOnlySubQuery invoiceSubQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeaderRefs), JobComInvoiceHeaderRefsSchema.J2_JZ, excludeMatchingReferences);
			if (!ReferenceType.IsEmpty)
			{
				invoiceSubQuery.AddToFilter(JobComInvoiceHeaderRefsSchema.J2_ReferenceType, ReferenceType);
			}
			if (!excludeMatchingReferences)
			{
				invoiceSubQuery.AddToFilter(JobComInvoiceHeaderRefsSchema.J2_ReferenceNumber, sqlOperatorToUse, referenceNumber);
			}

			headerQuery.AddSubQuery(invoiceSubQuery, JoinCondition.Or);

			return headerQuery;
		}

		#endregion
	}
}
