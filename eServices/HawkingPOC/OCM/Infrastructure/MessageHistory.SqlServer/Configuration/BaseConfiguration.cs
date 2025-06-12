using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;

namespace OcmPoc.Infrastructure.MessageHistory.SqlServer.Configuration
{
	public class BaseConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
		where TEntity : BaseEntity
	{
		public virtual void Configure(EntityTypeBuilder<TEntity> builder)
		{
			builder.HasAlternateKey(e => e.Guid);
		}
	}
}
