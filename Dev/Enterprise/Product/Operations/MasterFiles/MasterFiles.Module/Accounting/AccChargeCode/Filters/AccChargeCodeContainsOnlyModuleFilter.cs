using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Module
{
	class AccChargeCodeContainsOnlyModuleFilter : ModuleTextFilter
	{
		#region Construction

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public AccChargeCodeContainsOnlyModuleFilter(ZString description, SchemaStringColumn filterColumn)
			: base(description, filterColumn)
		{
			SqlComparisonOperator = SQLComparisonOperator.Contains;
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public AccChargeCodeContainsOnlyModuleFilter(ZString description, SchemaStringColumn filterColumn, IList list)
			: base(description, filterColumn, list)
		{
			SqlComparisonOperator = SQLComparisonOperator.Contains;
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public AccChargeCodeContainsOnlyModuleFilter(ZString description, SchemaStringColumn filterColumn, GetList listDelegate)
			: base(description, filterColumn, listDelegate)
		{
			SqlComparisonOperator = SQLComparisonOperator.Contains;
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public AccChargeCodeContainsOnlyModuleFilter(ZString description, GetTextQueryWithOperator queryDelegate)
			: base(description, queryDelegate)
		{
			SqlComparisonOperator = SQLComparisonOperator.Contains;
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public AccChargeCodeContainsOnlyModuleFilter(ZString description, GetTextQuery queryDelegate, IList list)
			: base(description, queryDelegate, list)
		{
			SqlComparisonOperator = SQLComparisonOperator.Contains;
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public AccChargeCodeContainsOnlyModuleFilter(ZString description, GetTextQuery queryDelegate, GetList listDelegate)
			: base(description, queryDelegate, listDelegate)
		{
			SqlComparisonOperator = SQLComparisonOperator.Contains;
		}

		#endregion

		#region Comparison Operator

		public override ZString ComparisonOperator
		{
			get { return base.ComparisonOperator; }
			set
			{
				base.ComparisonOperator = (NoResString)"contains";
				ComparisonOperatorInfo.RefreshBinding();
			}
		}

		public override IReadOnlyList<string> AllowedComparisonOperators
		{
			get
			{
				return new[] { ModuleTextBaseFilter.ComparisonConstants.Contains };
			}
		}

		#endregion

		#region Expensive Query

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		#endregion
	}
}
