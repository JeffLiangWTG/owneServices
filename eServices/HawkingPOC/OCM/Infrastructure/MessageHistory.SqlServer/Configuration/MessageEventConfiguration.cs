using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;

namespace OcmPoc.Infrastructure.MessageHistory.SqlServer.Configuration
{
	public class MessageEventConfiguration : BaseConfiguration<MessageEvent>
	{
		public override void Configure(EntityTypeBuilder<MessageEvent> builder)
		{
			base.Configure(builder);
			builder.Property(e => e.Type).HasConversion<string>();
		}
	}
}
