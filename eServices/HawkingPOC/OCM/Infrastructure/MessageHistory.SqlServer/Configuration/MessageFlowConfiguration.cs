using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;

namespace OcmPoc.Infrastructure.MessageHistory.SqlServer.Configuration
{
	public class MessageFlowConfiguration : BaseConfiguration<MessageFlow>
	{
		public override void Configure(EntityTypeBuilder<MessageFlow> builder)
		{
			base.Configure(builder);
			builder.Property(e => e.Status).HasConversion<string>();
			builder.UsesFieldForCollection(e => e.Events);
		}
	}
}
