using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.JobHeader)]
	public class JobHeaderCollection : BusinessObjectCollection<JobHeader>
	{
		public JobHeaderCollection(BusinessObjectFactory factory, ZQuery query) : base(factory, query)
		{
		}

		public JobHeaderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var additionalFilter = base.CreateAdditionalFilter();
			additionalFilter.AddToFilter(JobHeaderSchema.JH_ParentTableCode, SQLComparisonOperator.NotEqual, RatingHeaderSchema.Constants.Prefix);
			additionalFilter.AddToFilter(JobHeaderSchema.JH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			additionalFilter.AddToFilter(JobHeaderSchema.JH_IsActive, SQLComparisonOperator.Equal, true);
			return additionalFilter;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var relationshipFilter = base.CreateRelationshipFilter();
			relationshipFilter.AddToFilter(JobHeaderSchema.JH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			relationshipFilter.AddToFilter(JobHeaderSchema.JH_IsActive, SQLComparisonOperator.Equal, true);
			return relationshipFilter;
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("You cannot directly add to this collection. You need to use the JobHeader.Loader to create a new Job.");
		}

		protected override BusinessObject AddNewCore(Type bizoType)
		{
			throw new NotSupportedException("You cannot directly add to this collection. You need to use the JobHeader.Loader to create a new Job.");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
