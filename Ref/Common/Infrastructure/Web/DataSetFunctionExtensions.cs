using System;
using Microsoft.OData.ModelBuilder;
using CommonArgument = CargoWise.RefDbRepo.Common.Argument.Argument;

namespace CargoWise.RefDbRepo.Common.Web
{
	public static class DataSetFunctionExtensions
	{
		public static EntityTypeConfiguration<T> EnableDataSetFunctions<T>(this EntityTypeConfiguration<T> entityType, string entitySetName) where T : class
		{
			CommonArgument.NotNull(entityType, nameof(entityType));
			return entityType.EnableGetCreatedBetween(entitySetName)
				.EnableGetCreatedTimeUTC()
				.EnableLatestUpdatedTimeUTC()
				.GetWithOptimizedExpand(entitySetName)
				.EnableGetModifiedBetween(entitySetName)
				.EnableDeleteOrExpire()
				.EnableForceDelete();
		}

		public static EntityTypeConfiguration<T> EnableDeleteOrExpire<T>(this EntityTypeConfiguration<T> entityType) where T : class
		{
			CommonArgument.NotNull(entityType, nameof(entityType));
			return entityType.EnableBatchExpire().EnableBatchInActive().EnableBatchDelete();
		}

		static EntityTypeConfiguration<T> EnableBatchExpire<T>(this EntityTypeConfiguration<T> entityType) where T : class
		{
			CommonArgument.NotNull(entityType, nameof(entityType));

			var collection = entityType.Collection;
			var action = collection.Action("BatchExpire");
			action.CollectionParameter<Guid>("IDs");
			action.Parameter<DateTime>("expiredTime");
			return entityType;
		}

		static EntityTypeConfiguration<T> EnableBatchInActive<T>(this EntityTypeConfiguration<T> entityType) where T : class
		{
			CommonArgument.NotNull(entityType, nameof(entityType));

			var collection = entityType.Collection;
			var action = collection.Action("BatchInActive");
			action.CollectionParameter<Guid>("IDs");
			return entityType;
		}

		static EntityTypeConfiguration<T> EnableBatchDelete<T>(this EntityTypeConfiguration<T> entityType) where T : class
		{
			CommonArgument.NotNull(entityType, nameof(entityType));

			var collection = entityType.Collection;
			var action = collection.Action("BatchDelete");
			action.CollectionParameter<Guid>("IDs");
			return entityType;
		}

		static EntityTypeConfiguration<T> EnableForceDelete<T>(this EntityTypeConfiguration<T> entityType) where T : class
		{
			CommonArgument.NotNull(entityType, nameof(entityType));

			var collection = entityType.Collection;
			var action = collection.Action("ForceDelete");
			action.CollectionParameter<Guid>("IDs");
			return entityType;
		}

		public static EntityTypeConfiguration<T> EnableCloneExpiredDependent<T>(this EntityTypeConfiguration<T> entityType) where T : class
		{
			CommonArgument.NotNull(entityType, nameof(entityType));

			var collection = entityType.Collection;
			var action = collection.Action("CloneExistingRecordChildrenIntoNewRecord");
			action.Parameter<string>("cloneProcessObjectsJson");
			return entityType;
		}

		static EntityTypeConfiguration<T> EnableGetCreatedBetween<T>(this EntityTypeConfiguration<T> entityType, string entitySetName) where T : class
		{
			CommonArgument.NotNull(entityType, nameof(entityType));

			var collection = entityType.Collection;
			var function = collection.Function("GetCreatedBetween");
			function.Parameter<DateTime?>("afterCreatedTimeUTC");
			function.Parameter<DateTime>("beforeOrEqualCreatedTimeUTC");
			function.ReturnsCollectionFromEntitySet<T>(entitySetName);
			return entityType;
		}

		static EntityTypeConfiguration<T> EnableGetModifiedBetween<T>(this EntityTypeConfiguration<T> entityType, string entitySetName) where T : class
		{
			CommonArgument.NotNull(entityType, nameof(entityType));

			var collection = entityType.Collection;
			var function = collection.Function("GetModifiedBetween");
			function.Parameter<DateTime?>("afterModifiedTimeUTC");
			function.Parameter<DateTime>("beforeOrEqualModifiedTimeUTC");
			function.ReturnsCollectionFromEntitySet<T>(entitySetName);
			return entityType;
		}

		public static EntityTypeConfiguration<T> GetWithOptimizedExpand<T>(this EntityTypeConfiguration<T> entityType, string entitySetName) where T : class
		{
			CommonArgument.NotNull(entityType, nameof(entityType));

			var collection = entityType.Collection;
			var function = collection.Function("GetWithOptimizedExpand");
			function.ReturnsCollectionFromEntitySet<T>(entitySetName);
			return entityType;
		}

		static EntityTypeConfiguration<T> EnableGetCreatedTimeUTC<T>(this EntityTypeConfiguration<T> entityType) where T : class
		{
			CommonArgument.NotNull(entityType, nameof(entityType));

			var collection = entityType.Collection;
			var function = collection.Function("GetLastestCreatedTimeUTC");
			function.Returns<DateTime>();
			return entityType;
		}

		static EntityTypeConfiguration<T> EnableLatestUpdatedTimeUTC<T>(this EntityTypeConfiguration<T> entityType) where T : class
		{
			CommonArgument.NotNull(entityType, nameof(entityType));

			var collection = entityType.Collection;
			var function = collection.Function("GetLatestUpdatedTimeUTC");
			function.Returns<DateTime>();
			return entityType;
		}
	}
}
