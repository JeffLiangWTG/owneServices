using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(RefDataGrouping.Schema.ZZZ_DataGrouping)]
	[DescriptionProperty(RefDataGrouping.Schema.ZZZ_Description)]
	public class RefDataGrouping : AutoRefDataGrouping
	{
		public RefDataGrouping(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public RefDataGrouping Parent
		{
			get { return Factory.Load<RefDataGrouping>(ZZZ_ZZZ_Grouping); }
		}

		public ZString ParentName
		{
			get { return Parent?.ZZZ_Description ?? ZString.Empty; }
		}

		public static RefDataGrouping[] GetChildDataGroupings(BusinessObjectFactory factory, ZString parentDataGroupingCode)
		{
			RefDataGrouping[] result = null;
			parentDataGroupingCode = Argument.NotNullOrEmpty(parentDataGroupingCode, "parentDataGroupingCode");
			var parentDataGrouping = factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, parentDataGroupingCode);
			if (parentDataGrouping != null)
			{
				var query = new ZQuery(RefDataGroupingSchema.ZZZ_ZZZ_Grouping, parentDataGrouping.PK);
				result = factory.Load<RefDataGrouping>(query);
			}
			return result ?? Array.Empty<RefDataGrouping>();
		}

		public static RefDataGrouping GetParentDataGrouping(BusinessObjectFactory factory, ZString dataGrouping)
		{
			var countrySubQuery = new ZDBOnlySubQuery(typeof(RefDataGrouping), RefDataGroupingSchema.ZZZ_ZZZ_Grouping);
			countrySubQuery.AddToFilter(RefDataGroupingSchema.ZZZ_DataGrouping, dataGrouping);

			var query = new ZDBOnlyQuery(typeof(RefDataGrouping));
			query.AddSubQuery(countrySubQuery, JoinCondition.And);

			return factory.LoadTop1<RefDataGrouping>(query);
		}

		public static ZString GetParentDataGroupingCode(BusinessObjectFactory factory, ZString dataGrouping)
		{
			var parentDataGrouping = RefDataGrouping.GetParentDataGrouping(factory, dataGrouping);
			return parentDataGrouping?.ZZZ_DataGrouping ?? ZString.Empty;
		}

		public static ZQuery GetQueryIncludeParentDataGrouping(BusinessObjectFactory factory, SchemaStringColumn foreignNkColumn, ZString dataGrouping)
		{
			return new ZQuery(foreignNkColumn, GetDataGroupingIncludingParent(factory, dataGrouping));
		}

		public static ZString[] GetDataGroupingIncludingParent(BusinessObjectFactory factory, ZString dataGrouping)
		{
			var dataGroupings = new List<ZString>();
			dataGroupings.Add(dataGrouping);

			if (!dataGrouping.IsEmpty)
			{
				var parentDataGrouping = factory.GetCachedValue("ParentDataGrouping_" + dataGrouping, () => { return GetParentDataGrouping(factory, dataGrouping)?.ZZZ_DataGrouping ?? ZString.Empty; });
				if (!parentDataGrouping.IsEmpty)
				{
					dataGroupings.Add(parentDataGrouping);
				}
			}
			return dataGroupings.ToArray();
		}

		[ChildEditable]
		public RefDataGroupingCollection DataGroupingMembers
		{
			get
			{
				if (dataGroupingMembers == null)
				{
					dataGroupingMembers = new RefDataGroupingCollection(this);
					RegisterEditableChildObject(dataGroupingMembers);
				}
				return dataGroupingMembers;
			}
		}
		RefDataGroupingCollection dataGroupingMembers;

		public ICodeDescriptionPairList ApplicableDataGroupingList => applicableDataGroupingList ?? (applicableDataGroupingList = GetApplicableDataGroupingList());
		ICodeDescriptionPairList applicableDataGroupingList;

		ICodeDescriptionPairList GetApplicableDataGroupingList()
		{
			var result = new CodeDescriptionPairList();
			result.Add(this);
			result.AddRange(DataGroupingMembers.OrderBy(x => x.ZZZ_DataGrouping).ToArray());
			return result;
		}
	}
}
