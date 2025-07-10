using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class GenericJobExtension
	{
		#region Generic Extensions

		public static T LoadGenericJob<T>(this BusinessObjectFactory factory, JobHeader job) where T : IGenericJob
		{
			T result = default(T);

			if (job != null)
			{
				result = LoadGenericJob<T>(factory, job.JH_ParentID, job.JH_ParentTableCode);
			}
			return result;
		}

		public static T LoadGenericJob<T>(this JobHeader job) where T : IGenericJob
		{
			return LoadGenericJob<T>(job.Factory, job.JH_ParentID, job.JH_ParentTableCode);
		}

		public static T LoadGenericJob<T>(this BusinessObjectFactory factory, ZGuid parentJobPK, ZString parentTableCode) where T : IGenericJob
		{
			T result = default(T);

			if (!parentJobPK.IsEmpty && !IsParentTableCodeEmpty(parentTableCode))
			{
				IGenericJob[] genericJobs = factory.Load<IGenericJob>(GetGenericJobQuery(parentJobPK, parentTableCode));
				result = (T)genericJobs.FirstOrDefault();
			}

			return result;
		}

		static bool IsParentTableCodeEmpty(ZString parentTableCode)
		{
			bool result = false;

			if (parentTableCode.IsEmpty)
			{
				result = true;
				ErrorReporter.ReportOnce("GenericJobExtentionViewGenericJobByEmptyTableCode", "ViewGenericJob should not be queried by empty parent table code.");
			}

			return result;
		}

		public static void AddGenericJobQueryHint(this BusinessObjectFactory factory, ZGuid parentJobPK, ZString parentTableCode)
		{
			if (parentJobPK.IsValid && !parentTableCode.IsEmpty)
			{
				var query = GetGenericJobQuery(parentJobPK, parentTableCode);
				factory.AddFetchHint(new GenericJobQueryHint(query, parentTableCode));
			}
		}

		public static void AddGenericJobQueryHint(this BusinessObjectFactory factory, Type businessObjectType, ZGuid parentJobPK, ZString parentTableCode)
		{
			if (parentJobPK.IsValid && !parentTableCode.IsEmpty)
			{
				var query = GetGenericJobQuery(parentJobPK, parentTableCode);
				factory.AddFetchHint(new ImmediateGenericJobQueryHint(businessObjectType, query, parentTableCode));
			}
		}

		class ImmediateGenericJobQueryHint : GenericJobQueryHint, IImmediateHint
		{
			public ImmediateGenericJobQueryHint(Type businessObjectType, ZQuery query, string tablePrefix)
				: base(query, tablePrefix)
			{
				this.businessObjectType = businessObjectType;
			}
			readonly Type businessObjectType;

			public Type BusinessObjectType
			{
				get { return businessObjectType; }
			}
		}

		class GenericJobQueryHint : ZQueryFetchHint, IFetchHint
		{
			public GenericJobQueryHint(ZQuery query, string tablePrefix)
				: base(ViewGenericJobSchema.Instance, query)
			{
				this.builderKey = "GenericJobQueryHint:" + tablePrefix;
			}
			readonly string builderKey;

			string IFetchHint.BuilderKey
			{
				get { return builderKey; }
			}
		}

		static ZQuery GetGenericJobQuery(ZGuid parentJobPK, ZString parentTableCode)
		{
			var tableSchema = parentTableCode != "" ? ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchemaFromColumnNamePrefix(parentTableCode) : null;
			ZString parentTableName = tableSchema != null ? tableSchema.TableName : "";
			ZQuery genericJobQuery = new ZQuery(ViewGenericJobSchema.PK, parentJobPK);
			genericJobQuery.AddToFilter(ViewGenericJobSchema.VJ_TableName, parentTableName);

			return genericJobQuery;
		}

		#endregion

		#region IGenericJob

		public static IGenericJob LoadGenericJob(this BusinessObjectFactory factory, JobHeader job)
		{
			return factory.LoadGenericJob<IGenericJob>(job);
		}

		public static IGenericJob LoadGenericJob(this JobHeader job)
		{
			return job.Factory.LoadGenericJob<IGenericJob>(job);
		}

		public static IGenericJob LoadGenericJob(this BusinessObjectFactory factory, ZGuid pk, ZString parentTableCode)
		{
			return factory.LoadGenericJob<IGenericJob>(pk, parentTableCode);
		}

		#endregion
	}
}
