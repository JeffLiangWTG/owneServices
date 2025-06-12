using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;

namespace OcmPoc.Infrastructure.MessageHistory.SqlServer.Configuration
{
	public static class ConfigurationExtensions
    {
		public static void UsesFieldForCollection<TEntity, TCollection>(this EntityTypeBuilder<TEntity> builder, 
			Expression<Func<TEntity, TCollection>> collectionProperty)
			where TEntity: BaseEntity
		{
			if (collectionProperty.Body is MemberExpression me)
			{
				builder.Metadata
					   .FindNavigation(me.Member.Name)
					   .SetPropertyAccessMode(PropertyAccessMode.Field);
			}
		}
	}
}
