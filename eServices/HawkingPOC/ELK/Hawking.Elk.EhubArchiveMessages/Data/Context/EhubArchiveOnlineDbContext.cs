using System.Configuration;
using Hawking.Elk.EhubArchiveMessages.Model;
using Microsoft.EntityFrameworkCore;

namespace Hawking.Elk.EhubArchiveMessages.Data.Context
{
    public partial class EhubArchiveOnlineDbContext : DbContext
    {
        const string DefaultConnectionString = "ArchiveDbContext";

        public EhubArchiveOnlineDbContext(DbContextOptions options)
            : base(DefaultDbContextOptions(options))
        {
        }

        static DbContextOptions DefaultDbContextOptions(DbContextOptions options)
        {
            if (options == null)
            {
                return
                    SqlServerDbContextOptionsExtensions
                    .UseSqlServer(
                        new DbContextOptionsBuilder(),
                        ConfigurationManager.ConnectionStrings[DefaultConnectionString].ConnectionString).Options;
            }

            return options;
        }

        public virtual DbSet<EhubArchiveMessage> EhubArchiveMessage { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var config = modelBuilder.Entity<EhubArchiveMessage>();

            config.ToTable("eHubArchiveMessage");
            config.Property(e => e.TrackingId).HasColumnName("AM_PK");
            config.Property(e => e.ErrorMessage).HasColumnName("AM_ErrorMessage").IsUnicode(false);
            config.Property(e => e.ApplicationCode).HasColumnName("AM_ApplicationCode").IsUnicode(false);
            config.Property(e => e.CC_SenderInbox).HasColumnName("AM_CC_SenderInbox");
            config.Property(e => e.CC_RecipientOutbox).HasColumnName("AM_CC_RecipientOutbox");
            config.Property(e => e.DT_SenderMessageType).HasColumnName("AM_DT_SenderMessageType");
            config.Property(e => e.DT_RecipientMessageType).HasColumnName("AM_DT_RecipientMessageType");
            config.Property(e => e.SenderMessageRaw).HasColumnName("AM_SenderMessageRaw").IsUnicode(false);
            config.Property(e => e.SenderMessageXML).HasColumnName("AM_SenderMessageXML");
            config.Property(e => e.RecipientMessageRaw).HasColumnName("AM_RecipientMessageRaw").IsUnicode(false);
            config.Property(e => e.RecipientMessageXML).HasColumnName("AM_RecipientMessageXML");
            config.Property(e => e.ReceivedFromSenderUTC).HasColumnName("AM_ReceivedFromSenderUTC");
            config.Property(e => e.SentToRecipientUTC).HasColumnName("AM_SentToRecipientUTC");
            config.Property(e => e.ArchivedUTC).HasColumnName("AM_ArchivedUTC");
            config.Property(e => e.Status).HasColumnName("AM_Status");
            config.Property(e => e.OutboxMessageTrackingID).HasColumnName("AM_OutboxMessageTrackingID");
            config.Property(e => e.EmailSubjectOverride).HasColumnName("AM_EmailSubjectOverride");
            config.Property(e => e.InboxFileNameOverride).HasColumnName("AM_InboxFileNameOverride");
            config.Property(e => e.SenderMessageUncompressedLength).HasColumnName("AM_SenderMessageUncompressedLength");
            config.Property(e => e.RecipientMessageUncompressedLength).HasColumnName("AM_RecipientMessageUncompressedLength");
            config.Property(e => e.BillingElementCount).HasColumnName("AM_BillingElementCount");
            config.Property(e => e.TS).HasColumnName("AM_TS");
            config.Property(e => e.OutboxFileNameOverride).HasColumnName("AM_OutboxFileNameOverride");
            config.Property(e => e.ReadyForDeliveryUTC).HasColumnName("AM_ReadyForDeliveryUTC");
            config.Property(e => e.EI_PK).HasColumnName("AM_EI_PK");
            config.Property(e => e.InboxMessageTrackingID).HasColumnName("AM_InboxMessageTrackingID");
            config.Property(e => e.BatchEnvelopeTrackingID).HasColumnName("AM_BatchEnvelopeTrackingID");
            config.Property(e => e.CC_RecipientInbox).HasColumnName("AM_CC_RecipientInbox");
            config.Property(e => e.CC_SenderOutbox).HasColumnName("AM_CC_SenderOutbox");
        }
    }
}
