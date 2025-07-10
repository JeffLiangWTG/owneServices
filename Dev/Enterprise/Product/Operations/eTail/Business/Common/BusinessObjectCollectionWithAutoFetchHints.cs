using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.eTail.Business
{
	public abstract class BusinessObjectCollectionWithAutoFetchHints<TChild, TParent> : DependentBusinessObjectCollection<TChild, TParent>
		where TChild : BusinessObject where TParent : BusinessObject, ILinkable
	{
		public BusinessObjectCollectionWithAutoFetchHints(TParent master) : base(master)
		{
		}

		public override void Load()
		{
			if (Master.IsInDatabase)
			{
				var serviceContainerType = typeof(ZServiceContainer);
				var allocatorService = serviceContainerType
					.GetMethod(nameof(ZServiceContainer.GetService))
					.MakeGenericMethod(GenericTypeOfService)
					.Invoke(Factory.ServiceContainer, null) as HVLVFetchHintAllocatorService;
				if (allocatorService == null)
				{
					var service = Activator.CreateInstance(GenericTypeOfService, Factory, ChildClusterKeyColumn) as HVLVFetchHintAllocatorService;
					allocatorService = Factory.ServiceContainer.AddService(service, GenericTypeOfService);
				}

				allocatorService.IncrementCount((ZInt)Master[ParentClusterKeyColumn]);
			}

			base.Load();
		}

		Type GenericTypeOfService => typeof(HVLVFetchHintAllocatorService<>).MakeGenericType(GetType());

		[ThreadSafe]
		static readonly SchemaIntColumn ChildClusterKeyColumn = GetClusterKeyColumn<TChild>();

		[ThreadSafe]
		static readonly SchemaIntColumn ParentClusterKeyColumn = GetClusterKeyColumn<TParent>();

		static SchemaIntColumn GetClusterKeyColumn<T>()
		{
			var type = typeof(T);
			var tablePrefix = BusinessObjectFactory.GetTableCodeFromType(type);
			var tableSchema = BusinessObjectFactory.GetTableSchemaFromType(type);
			return tableSchema.GetSchemaColumn(tablePrefix + Schema.ClusterKeyColumnSuffix) as SchemaIntColumn;
		}
	}
}
