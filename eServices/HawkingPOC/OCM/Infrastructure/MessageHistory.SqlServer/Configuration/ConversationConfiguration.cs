using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;

namespace OcmPoc.Infrastructure.MessageHistory.SqlServer.Configuration
{
	public class ConversationConfiguration : BaseConfiguration<Conversation>
	{
		public override void Configure(EntityTypeBuilder<Conversation> builder)
		{
			base.Configure(builder);

			builder.Property(e => e.Status).HasConversion<string>();
			builder.UsesFieldForCollection(e => e.Flows);
		}
	}
}
