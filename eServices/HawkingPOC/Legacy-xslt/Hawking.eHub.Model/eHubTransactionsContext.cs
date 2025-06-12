using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubTransactionsContext : DbContext, IeHubTransactionsContext
    {
        const string eHubTransactionsContextConnectionStringName = "eHubTransactionsContext";

        readonly IConfiguration configuration;

        public eHubTransactionsContext(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public virtual DbSet<eHubAirConnection> eHubAirConnection { get; set; }
        public virtual DbSet<eHubAirConnectionPerBranch> eHubAirConnectionPerBranch { get; set; }
        public virtual DbSet<eHubAirDefaultServiceProvider> eHubAirDefaultServiceProvider { get; set; }
        public virtual DbSet<eHubAirReferenceNumber> eHubAirReferenceNumber { get; set; }
        public virtual DbSet<eHubAirServiceProviderMapping> eHubAirServiceProviderMapping { get; set; }
        public virtual DbSet<eHubCertificate> eHubCertificate { get; set; }
        public virtual DbSet<eHubClient> eHubClient { get; set; }
        public virtual DbSet<eHubClientAccessRestriction> eHubClientAccessRestriction { get; set; }
        public virtual DbSet<eHubClientBatching> eHubClientBatching { get; set; }
        public virtual DbSet<eHubClientCode> eHubClientCode { get; set; }
        public virtual DbSet<eHubClientDialogue> eHubClientDialogue { get; set; }
        public virtual DbSet<eHubClientRegistration> eHubClientRegistration { get; set; }
        public virtual DbSet<eHubClientSystem> eHubClientSystem { get; set; }
        public virtual DbSet<eHubClientSystemRegistration> eHubClientSystemRegistration { get; set; }
        public virtual DbSet<eHubCodeMapKey> eHubCodeMapKey { get; set; }
        public virtual DbSet<eHubCodeMapValue> eHubCodeMapValue { get; set; }
        public virtual DbSet<eHubCodeSet> eHubCodeSet { get; set; }
        public virtual DbSet<eHubCodeSetResult> eHubCodeSetResult { get; set; }
        public virtual DbSet<eHubCounter> eHubCounter { get; set; }
        public virtual DbSet<eHubInterfaceCounter> eHubInterfaceCounter { get; set; }
        public virtual DbSet<eHubITCustomsJobStatus> eHubITCustomsJobStatus { get; set; }
        public virtual DbSet<eHubMessageCopyRegistry> eHubMessageCopyRegistry { get; set; }
        public virtual DbSet<eHubMessageReferenceRegistry> eHubMessageReferenceRegistry { get; set; }
        public virtual DbSet<eHubMessageType> eHubMessageType { get; set; }
        public virtual DbSet<eHubRegistrationType> eHubRegistrationType { get; set; }
        public virtual DbSet<eHubRoutingRule> eHubRoutingRule { get; set; }
        public virtual DbSet<eHubRoutingRuleFact> eHubRoutingRuleFact { get; set; }
        public virtual DbSet<eHubSequenceNumber> eHubSequenceNumber { get; set; }
        public virtual DbSet<eHubServiceOperator> eHubServiceOperator { get; set; }
        public virtual DbSet<eHubServiceOperatorRegistration> eHubServiceOperatorRegistration { get; set; }
        public virtual DbSet<eHubServiceProvider> eHubServiceProvider { get; set; }
        public virtual DbSet<eHubServiceProviderRequiredRegistration> eHubServiceProviderRequiredRegistration { get; set; }
        public virtual DbSet<eHubSubscriptionAutoSubscribe> eHubSubscriptionAutoSubscribe { get; set; }
        public virtual DbSet<eHubSubscriptionBroadcaster> eHubSubscriptionBroadcaster { get; set; }
        public virtual DbSet<eHubSubscriptionLookup> eHubSubscriptionLookup { get; set; }
        public virtual DbSet<eHubSubscriptionType> eHubSubscriptionType { get; set; }
        public virtual DbSet<eHubSubscriptionValue> eHubSubscriptionValue { get; set; }
        public virtual DbSet<eHubTransformationMapping> eHubTransformationMapping { get; set; }
        public virtual DbSet<eHubTransformationSet> eHubTransformationSet { get; set; }
        public virtual DbSet<eHubTransformationType> eHubTransformationType { get; set; }
        public virtual DbSet<eHubUSCustomsRegistry> eHubUSCustomsRegistry { get; set; }
        public virtual DbSet<eHubZone> eHubZone { get; set; }

        // Unable to generate entity type for table 'dbo.eHubRegistrationLog'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.Temp_eHubClient'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.Temp_eHubMessageType'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.Temp_eHubTransformationSet'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.Temp_eHubTransformationType'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.Temp_eHubTransformationMapping'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.Temp_eHubCodeSet'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.Temp_eHubCodeMapKey'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.Temp_eHubCodeMapValue'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.Temp_eHubCodeSetResult'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.eHubClientSystemRegistration'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.DJC_AsycudaPlusPlus'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.DJC_eHubTransformationset'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.eHubTransformationMappingDanielAsyTemp'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.DJC_eHubMessageType'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.DJC_eHubTransformationType'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.DJC_ehubtransformationMapping'. Please see the warning messages.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(configuration.GetConnectionString(
                    configuration.GetConnectionString(eHubTransactionsContextConnectionStringName)));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<eHubAirConnection>(entity =>
            {
                entity.HasKey(e => new { e.AC_CC_Client, e.AC_CC_AirServiceProvider });

                entity.Property(e => e.AC_PASSWORD)
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.AC_PIMA)
                    .IsRequired()
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.HasOne(d => d.AC_CC_AirServiceProviderNavigation)
                    .WithMany(p => p.eHubAirConnectionAC_CC_AirServiceProviderNavigation)
                    .HasForeignKey(d => d.AC_CC_AirServiceProvider)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_eHubClientAirConnection_eHubClient2");

                entity.HasOne(d => d.AC_CC_ClientNavigation)
                    .WithMany(p => p.eHubAirConnectionAC_CC_ClientNavigation)
                    .HasForeignKey(d => d.AC_CC_Client)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_eHubClientAirConnection_eHubClient1");
            });

            modelBuilder.Entity<eHubAirConnectionPerBranch>(entity =>
            {
                entity.HasKey(e => new { e.AB_CC_Client, e.AB_CC_AirServiceProvider, e.AB_IssuingCarrierAgentIATACode });

                entity.Property(e => e.AB_IssuingCarrierAgentIATACode)
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.AB_PASSWORD)
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.AB_PIMA)
                    .IsRequired()
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.HasOne(d => d.AB_CC_)
                    .WithMany(p => p.eHubAirConnectionPerBranch)
                    .HasForeignKey(d => new { d.AB_CC_Client, d.AB_CC_AirServiceProvider })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_eHubAirConnectionPerBranch_eHubAirConnection");
            });

            modelBuilder.Entity<eHubAirDefaultServiceProvider>(entity =>
            {
                entity.HasKey(e => new { e.AD_CC_Client, e.AD_DT_MessageType });

                entity.HasOne(d => d.AD_CC_AirServiceProviderNavigation)
                    .WithMany(p => p.eHubAirDefaultServiceProviderAD_CC_AirServiceProviderNavigation)
                    .HasForeignKey(d => d.AD_CC_AirServiceProvider)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_eHubAirDefaultServiceProvide_eHubClient1");

                entity.HasOne(d => d.AD_CC_ClientNavigation)
                    .WithMany(p => p.eHubAirDefaultServiceProviderAD_CC_ClientNavigation)
                    .HasForeignKey(d => d.AD_CC_Client)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_eHubAirDefaultServiceProvide_eHubClient");

                entity.HasOne(d => d.AD_DT_MessageTypeNavigation)
                    .WithMany(p => p.eHubAirDefaultServiceProvider)
                    .HasForeignKey(d => d.AD_DT_MessageType)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_eHubAirDefaultServiceProvide_eHubMessageType");
            });

            modelBuilder.Entity<eHubAirReferenceNumber>(entity =>
            {
                entity.HasKey(e => new { e.AN_CC_Sender, e.AN_CC_AirServiceProvider });

                entity.HasOne(d => d.AN_CC_AirServiceProviderNavigation)
                    .WithMany(p => p.eHubAirReferenceNumberAN_CC_AirServiceProviderNavigation)
                    .HasForeignKey(d => d.AN_CC_AirServiceProvider)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_eHubAirReferenceNumber_eHubClient1");

                entity.HasOne(d => d.AN_CC_SenderNavigation)
                    .WithMany(p => p.eHubAirReferenceNumberAN_CC_SenderNavigation)
                    .HasForeignKey(d => d.AN_CC_Sender)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_eHubAirReferenceNumber_eHubClient");
            });

            modelBuilder.Entity<eHubAirServiceProviderMapping>(entity =>
            {
                entity.HasKey(e => new { e.AM_CC_Client, e.AM_CC_Airline, e.AM_DT_MessageType, e.AM_CC_AirServiceProvider });

                entity.HasOne(d => d.AM_CC_AirServiceProviderNavigation)
                    .WithMany(p => p.eHubAirServiceProviderMappingAM_CC_AirServiceProviderNavigation)
                    .HasForeignKey(d => d.AM_CC_AirServiceProvider)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_eHubAirServiceProviderMapping_eHubClient2");

                entity.HasOne(d => d.AM_CC_AirlineNavigation)
                    .WithMany(p => p.eHubAirServiceProviderMappingAM_CC_AirlineNavigation)
                    .HasForeignKey(d => d.AM_CC_Airline)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_eHubAirServiceProviderMapping_eHubClient1");

                entity.HasOne(d => d.AM_CC_ClientNavigation)
                    .WithMany(p => p.eHubAirServiceProviderMappingAM_CC_ClientNavigation)
                    .HasForeignKey(d => d.AM_CC_Client)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_eHubAirServiceProviderMapping_eHubClient");

                entity.HasOne(d => d.AM_DT_MessageTypeNavigation)
                    .WithMany(p => p.eHubAirServiceProviderMapping)
                    .HasForeignKey(d => d.AM_DT_MessageType)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_eHubAirServiceProviderMapping_eHubMessageType");
            });

            modelBuilder.Entity<eHubAlert>(entity =>
            {
                entity.HasKey(e => e.EA_PK)
                    .ForSqlServerIsClustered(false);

                entity.Property(e => e.EA_PK).ValueGeneratedNever();

                entity.Property(e => e.EA_ErrorType)
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.EA_Source)
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.HasOne(d => d.EA_CC_RecipientNavigation)
                    .WithMany(p => p.eHubAlertEA_CC_RecipientNavigation)
                    .HasForeignKey(d => d.EA_CC_Recipient)
                    .HasConstraintName("eHubError_EE_EI_Recipient_FK2_eHubClient_RRR_121");

                entity.HasOne(d => d.EA_CC_SenderNavigation)
                    .WithMany(p => p.eHubAlertEA_CC_SenderNavigation)
                    .HasForeignKey(d => d.EA_CC_Sender)
                    .HasConstraintName("eHubError_EE_EI_Sender_FK2_eHubClient_RRR_121");
            });

            modelBuilder.Entity<eHubAlertSubscriber>(entity =>
            {
                entity.HasKey(e => e.ES_PK)
                    .ForSqlServerIsClustered(false);

                entity.Property(e => e.ES_PK).ValueGeneratedNever();

                entity.Property(e => e.ES_Email)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.HasOne(d => d.ES_EA_AlertNavigation)
                    .WithMany(p => p.eHubAlertSubscriber)
                    .HasForeignKey(d => d.ES_EA_Alert)
                    .HasConstraintName("eHubAlert_EA_EA_Alert_FK2_eHubAlert_RRR_121");
            });

            modelBuilder.Entity<eHubCertificate>(entity =>
            {
                entity.HasKey(e => e.CE_PK);

                entity.Property(e => e.CE_PK).ValueGeneratedNever();

                entity.Property(e => e.CE_ActiveFromUTC).HasColumnType("datetime");

                entity.Property(e => e.CE_AddedUTC).HasColumnType("datetime");

                entity.Property(e => e.CE_Category)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.CE_ContainerType)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CE_ID)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.CE_Issuer)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.CE_Password).HasMaxLength(250);

                entity.Property(e => e.CE_SerialNumber)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.CE_SubjectKeyIdentifier)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.CE_TextContainer).IsUnicode(false);

                entity.Property(e => e.CE_Thumbprint)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.CE_ValidFromUTC).HasColumnType("datetime");

                entity.Property(e => e.CE_ValidToUTC).HasColumnType("datetime");

                entity.HasOne(d => d.CE_CC_OwnerNavigation)
                    .WithMany(p => p.eHubCertificate)
                    .HasForeignKey(d => d.CE_CC_Owner)
                    .HasConstraintName("eHubCertificate_CE_CC_Owner_FK2_eHubClient_RRR_121");

                entity.HasOne(d => d.CE_EH_OwnerNavigation)
                    .WithMany(p => p.eHubCertificate)
                    .HasForeignKey(d => d.CE_EH_Owner)
                    .HasConstraintName("eHubCertificate_CE_EH_Owner_FK2_eHubClientSystem_RRR_121");
            });

            modelBuilder.Entity<eHubClient>(entity =>
            {
                entity.HasKey(e => e.CC_PK)
                    .ForSqlServerIsClustered(false);

                entity.HasIndex(e => e.CC_AirlineCode)
                    .HasName("IX_UX__CC_AirlineCode")
                    .IsUnique()
                    .HasFilter("([CC_AirlineCode] IS NOT NULL)");

                entity.HasIndex(e => e.CC_AirlinePrefix)
                    .HasName("IX_UX__CC_AirlinePrefix")
                    .IsUnique()
                    .HasFilter("([CC_AirlinePrefix] IS NOT NULL)");

                entity.HasIndex(e => e.CC_ID)
                    .HasName("DF_CC_ID")
                    .IsUnique()
                    .ForSqlServerIsClustered();

                entity.HasIndex(e => new { e.CC_PK, e.CC_DistributionZone })
                    .HasName("NR_RX__CC_DistributionZone");

                entity.Property(e => e.CC_PK).ValueGeneratedNever();

                entity.Property(e => e.CC_AS2_Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CC_AirlineCode)
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.CC_AirlinePrefix)
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.CC_EmailAddress)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.CC_FriendlyName)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.CC_ID)
                    .IsRequired()
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.CC_OwnerCategory)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CC_Password)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.CC_SCAC_Code)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CC_SystemCategory)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.CC_AirServiceProviderNavigation)
                    .WithMany(p => p.InverseCC_AirServiceProviderNavigation)
                    .HasForeignKey(d => d.CC_AirServiceProvider)
                    .HasConstraintName("FK_eHubClient_eHubClient");

                entity.HasOne(d => d.CC_RRNavigation)
                    .WithMany(p => p.eHubClient)
                    .HasForeignKey(d => d.CC_RR)
                    .HasConstraintName("eHubClient_CC_RR_FK2_eHubRoutingRule");
            });

            modelBuilder.Entity<eHubClientAccessRestriction>(entity =>
            {
                entity.HasKey(e => e.CP_PK)
                    .ForSqlServerIsClustered(false);

                entity.Property(e => e.CP_PK).ValueGeneratedNever();

                entity.Property(e => e.CP_Restriction)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.HasOne(d => d.CP_ClientNavigation)
                    .WithMany(p => p.eHubClientAccessRestriction)
                    .HasForeignKey(d => d.CP_Client)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubClientAccessRestriction_CP_Client_FK2_eHubClient_RRR_121");
            });

            modelBuilder.Entity<eHubClientBatching>(entity =>
            {
                entity.HasKey(e => e.CB_CC_Recipient);

                entity.Property(e => e.CB_CC_Recipient).ValueGeneratedNever();

                entity.Property(e => e.CB_EnvelopeEndTag)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.CB_EnvelopeStartTag)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.Property(e => e.CB_NextActivationUTC).HasColumnType("datetime");

                entity.Property(e => e.CB_ReleaseScheduleInterval).HasColumnType("time(0)");

                entity.Property(e => e.CB_ReleaseScheduleTimeOfDay).HasColumnType("time(0)");

                entity.HasOne(d => d.CB_CC_RecipientNavigation)
                    .WithOne(p => p.eHubClientBatching)
                    .HasForeignKey<eHubClientBatching>(d => d.CB_CC_Recipient)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubClientBatching_CB_CC_Recipient_FK2_eHubClient_RRR_121");
            });

            modelBuilder.Entity<eHubClientCode>(entity =>
            {
                entity.HasKey(e => e.CM_PK)
                    .ForSqlServerIsClustered(false);

                entity.Property(e => e.CM_PK).ValueGeneratedNever();

                entity.Property(e => e.CM_ClientCode)
                    .IsRequired()
                    .HasMaxLength(35)
                    .IsUnicode(false);

                entity.HasOne(d => d.CM_CCNavigation)
                    .WithMany(p => p.eHubClientCode)
                    .HasForeignKey(d => d.CM_CC)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubClientCode_CM_CC_FK2_eHubClient_RRR_121");
            });

            modelBuilder.Entity<eHubClientDialogue>(entity =>
            {
                entity.HasKey(e => new { e.D1_ClientPK, e.D1_DialogueHandle, e.D1_ServicePK })
                    .ForSqlServerIsClustered(false);
            });

            modelBuilder.Entity<eHubClientRegistration>(entity =>
            {
                entity.HasKey(e => e.CX_PK)
                    .ForSqlServerIsClustered(false);

                entity.HasIndex(e => new { e.CX_RT, e.CX_CC, e.CX_Qualifier })
                    .HasName("IX_UX__CX_RT_CC_CX_Qualifier")
                    .IsUnique()
                    .ForSqlServerIsClustered();

                entity.Property(e => e.CX_PK).ValueGeneratedNever();

                entity.Property(e => e.CX_Attr1).HasMaxLength(250);

                entity.Property(e => e.CX_Code)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.CX_Password1).HasMaxLength(250);

                entity.Property(e => e.CX_Qualifier).HasMaxLength(250);

                entity.HasOne(d => d.CX_CCNavigation)
                    .WithMany(p => p.eHubClientRegistration)
                    .HasForeignKey(d => d.CX_CC)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubClientRegistration_CX_CC_FK2_eHubClient_RRR_121");

                entity.HasOne(d => d.CX_RTNavigation)
                    .WithMany(p => p.eHubClientRegistration)
                    .HasForeignKey(d => d.CX_RT)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubClientRegistration_CX_RT_FK2_eHubRegistrationType_RRR_121");
            });

            modelBuilder.Entity<eHubClientSystem>(entity =>
            {
                entity.HasKey(e => e.EH_PK);

                entity.Property(e => e.EH_PK).ValueGeneratedNever();

                entity.Property(e => e.EH_ID)
                    .IsRequired()
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.EH_InsertUTC)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.EH_LastUpdateUTC)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.EH_URL)
                    .IsRequired()
                    .HasMaxLength(500)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<eHubClientSystemRegistration>(entity =>
            {
                entity.HasKey(e => e.CD_PK)
                    .ForSqlServerIsClustered(false);

                entity.HasIndex(e => new { e.CD_RT, e.CD_EH, e.CD_Code })
                    .HasName("DF_eHubClientSystemRegistration_CD_RT_CD_EH_CD_Code")
                    .IsUnique();

                entity.Property(e => e.CD_PK).ValueGeneratedNever();

                entity.Property(e => e.CD_Attr1).HasMaxLength(250);

                entity.Property(e => e.CD_Attr2).HasMaxLength(250);

                entity.Property(e => e.CD_Code)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.CD_Qualifier).HasMaxLength(250);

                entity.Property(e => e.CD_RV)
                    .IsRequired()
                    .IsRowVersion();

                entity.HasOne(d => d.CD_EHNavigation)
                    .WithMany(p => p.eHubClientSystemRegistration)
                    .HasForeignKey(d => d.CD_EH)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubClientSystemRegistration_CD_EH_FK2_eHubClientSystem_RRR_121");

                entity.HasOne(d => d.CD_RTNavigation)
                    .WithMany(p => p.eHubClientSystemRegistration)
                    .HasForeignKey(d => d.CD_RT)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubClientSystemRegistration_CD_RT_FK2_eHubRegistrationType_RRR_121");
            });

            modelBuilder.Entity<eHubCodeMapKey>(entity =>
            {
                entity.HasKey(e => e.CK_PK);

                entity.HasIndex(e => new { e.CK_CS, e.CK_Order })
                    .HasName("IX_UX__CK_CS_CK_Order")
                    .IsUnique();

                entity.HasIndex(e => new { e.CK_CS, e.CK_Key1Value, e.CK_Key2Value, e.CK_Key3Value, e.CK_Key4Value, e.CK_Key5Value })
                    .HasName("IX_UX__CK_CS_CK_KeyValue")
                    .IsUnique();

                entity.Property(e => e.CK_PK).ValueGeneratedNever();

                entity.Property(e => e.CK_Key1Value).HasMaxLength(50);

                entity.Property(e => e.CK_Key2Value).HasMaxLength(50);

                entity.Property(e => e.CK_Key3Value).HasMaxLength(50);

                entity.Property(e => e.CK_Key4Value).HasMaxLength(50);

                entity.Property(e => e.CK_Key5Value).HasMaxLength(50);

                entity.HasOne(d => d.CK_CSNavigation)
                    .WithMany(p => p.eHubCodeMapKey)
                    .HasForeignKey(d => d.CK_CS)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubCodeMapKey_CK_CS_FK2_eHubCodeSet_RRR_121");
            });

            modelBuilder.Entity<eHubCodeMapValue>(entity =>
            {
                entity.HasKey(e => new { e.CV_CK, e.CV_CR });

                entity.Property(e => e.CV_OutputCode).HasMaxLength(250);

                entity.HasOne(d => d.CV_CKNavigation)
                    .WithMany(p => p.eHubCodeMapValue)
                    .HasForeignKey(d => d.CV_CK)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubCodeMapValue_CV_CK_FK2_eHubCodeMapKey_RRR_121");

                entity.HasOne(d => d.CV_CRNavigation)
                    .WithMany(p => p.eHubCodeMapValue)
                    .HasForeignKey(d => d.CV_CR)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubCodeMapValue_CV_CR_FK2_eHubCodeSetResult_RRR_121");
            });

            modelBuilder.Entity<eHubCodeSet>(entity =>
            {
                entity.HasKey(e => e.CS_PK)
                    .ForSqlServerIsClustered(false);

                entity.HasIndex(e => new { e.CS_Name, e.CS_TS, e.CS_CC_Sender, e.CS_CC_Recipient })
                    .HasName("IX_UX__CS_CodeNameTsSenderRecipient")
                    .IsUnique();

                entity.Property(e => e.CS_PK).ValueGeneratedNever();

                entity.Property(e => e.CS_Key1Name).HasMaxLength(50);

                entity.Property(e => e.CS_Key2Name).HasMaxLength(50);

                entity.Property(e => e.CS_Key3Name).HasMaxLength(50);

                entity.Property(e => e.CS_Key4Name).HasMaxLength(50);

                entity.Property(e => e.CS_Key5Name).HasMaxLength(50);

                entity.Property(e => e.CS_Name)
                    .IsRequired()
                    .HasMaxLength(25);

                entity.HasOne(d => d.CS_CC_RecipientNavigation)
                    .WithMany(p => p.eHubCodeSetCS_CC_RecipientNavigation)
                    .HasForeignKey(d => d.CS_CC_Recipient)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubCodeSet_CS_CC_Recipient_FK2_eHubClient_RRR_121");

                entity.HasOne(d => d.CS_CC_SenderNavigation)
                    .WithMany(p => p.eHubCodeSetCS_CC_SenderNavigation)
                    .HasForeignKey(d => d.CS_CC_Sender)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubCodeSet_CS_CC_Sender_FK2_eHubClient_RRR_121");

                entity.HasOne(d => d.CS_TSNavigation)
                    .WithMany(p => p.eHubCodeSet)
                    .HasForeignKey(d => d.CS_TS)
                    .HasConstraintName("eHubCodeSet_CS_TS_FK2_eHubTransformationSet_RRR_121");
            });

            modelBuilder.Entity<eHubCodeSetResult>(entity =>
            {
                entity.HasKey(e => e.CR_PK);

                entity.HasIndex(e => new { e.CR_CS, e.CR_Name })
                    .HasName("IX_UX__CR_CS_CR_Name")
                    .IsUnique();

                entity.Property(e => e.CR_PK).ValueGeneratedNever();

                entity.Property(e => e.CR_Name)
                    .IsRequired()
                    .HasMaxLength(25);

                entity.HasOne(d => d.CR_CSNavigation)
                    .WithMany(p => p.eHubCodeSetResult)
                    .HasForeignKey(d => d.CR_CS)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubCodeSetResult_CR_CS_FK2_eHubCodeSet_RRR_121");
            });

            modelBuilder.Entity<eHubCounter>(entity =>
            {
                entity.HasKey(e => e.CN_Name);

                entity.Property(e => e.CN_Name)
                    .HasMaxLength(120)
                    .IsUnicode(false)
                    .ValueGeneratedNever();
            });

            modelBuilder.Entity<eHubError>(entity =>
            {
                entity.HasKey(e => e.EE_PK)
                    .ForSqlServerIsClustered(false);

                entity.HasIndex(e => new { e.EE_Source, e.EE_EI_Inbox })
                    .HasName("NR_RX__EE_EI_Inbox");

                entity.HasIndex(e => new { e.EE_Source, e.EE_OI_Outbox })
                    .HasName("NR_RX__EE_OI_Outbox");

                entity.HasIndex(e => new { e.EE_PK, e.EE_DateTimeUTC, e.EE_Alerted })
                    .HasName("NR_RX__EE_Alerted");

                entity.Property(e => e.EE_PK).ValueGeneratedNever();

                entity.Property(e => e.EE_Alerted)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.EE_DateTimeUTC).HasColumnType("datetime");

                entity.Property(e => e.EE_Description)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.EE_ErrorDetail).HasColumnType("xml");

                entity.Property(e => e.EE_ErrorType)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.EE_Source)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<eHubInboxMessage>(entity =>
            {
                entity.HasKey(e => e.EI_PK)
                    .ForSqlServerIsClustered(false);

                entity.HasIndex(e => e.EI_ApplicationCode)
                    .HasName("NR_RX__EI_ApplicationCode");

                entity.HasIndex(e => e.EI_CC_Recipient)
                    .HasName("NR_RX__EI_CC_Recipient");

                entity.HasIndex(e => e.EI_CC_Sender)
                    .HasName("NR_RX__EI_CC_Sender");

                entity.HasIndex(e => e.EI_EnvelopeTrackingID)
                    .HasName("NR_RX__EI_EnvelopeTrackingID");

                entity.HasIndex(e => e.EI_InsertUTC)
                    .HasName("NR_RX__EI_InsertUTC")
                    .ForSqlServerIsClustered();

                entity.HasIndex(e => e.EI_MessageTrackingID)
                    .HasName("NR_RX__EI_MessageTrackingID");

                entity.HasIndex(e => new { e.EI_PK, e.EI_CC_Sender, e.EI_CC_Recipient, e.EI_InsertUTC, e.EI_Status, e.EI_ApplicationCode })
                    .HasName("NR_RX__EI_StatusApplicationCode");

                entity.Property(e => e.EI_PK).ValueGeneratedNever();

                entity.Property(e => e.EI_ApplicationCode)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.EI_EmailSubjectOverride).HasMaxLength(512);

                entity.Property(e => e.EI_EnvelopeTrackingID)
                    .IsRequired()
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.EI_FileNameOverride).HasMaxLength(512);

                entity.Property(e => e.EI_InsertUTC)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.EI_MessageTrackingID)
                    .IsRequired()
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.EI_MessageType)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.HasOne(d => d.EI_CC_RecipientNavigation)
                    .WithMany(p => p.eHubInboxMessageEI_CC_RecipientNavigation)
                    .HasForeignKey(d => d.EI_CC_Recipient)
                    .HasConstraintName("eHubInboxMessage_EI_CC_Recipient_FK2_eHubClient_RRR_121");

                entity.HasOne(d => d.EI_CC_SenderNavigation)
                    .WithMany(p => p.eHubInboxMessageEI_CC_SenderNavigation)
                    .HasForeignKey(d => d.EI_CC_Sender)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubInboxMessage_EI_CC_Sender_FK2_eHubClient_RRR_121");
            });

            modelBuilder.Entity<eHubInboxMessageArchive>(entity =>
            {
                entity.HasKey(e => e.EI_PK)
                    .ForSqlServerIsClustered(false);

                entity.HasIndex(e => e.EI_InsertUTC)
                    .HasName("NR_RX__AR_EI_InsertUTC")
                    .ForSqlServerIsClustered();

                entity.HasIndex(e => e.EI_MessageType)
                    .HasName("NR_RX__AR_EI_MessageType");

                entity.Property(e => e.EI_PK).ValueGeneratedNever();

                entity.Property(e => e.EI_ApplicationCode)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.EI_EmailSubjectOverride).HasMaxLength(512);

                entity.Property(e => e.EI_EnvelopeTrackingID)
                    .IsRequired()
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.EI_FileNameOverride).HasMaxLength(512);

                entity.Property(e => e.EI_InsertUTC).HasColumnType("datetime");

                entity.Property(e => e.EI_MessageTrackingID)
                    .IsRequired()
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.EI_MessageType)
                    .HasMaxLength(200)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<eHubInboxXmlContent>(entity =>
            {
                entity.HasKey(e => e.EX_EI_Inbox)
                    .ForSqlServerIsClustered(false);

                entity.Property(e => e.EX_EI_Inbox).ValueGeneratedNever();

                entity.Property(e => e.EX_XmlContent)
                    .IsRequired()
                    .HasColumnType("xml");

                entity.HasOne(d => d.EX_DT_SourceNavigation)
                    .WithMany(p => p.eHubInboxXmlContent)
                    .HasForeignKey(d => d.EX_DT_Source)
                    .HasConstraintName("eHubInboxXmlContent_EX_DT_Source_FK2_eHubMessageType_RRR_121");
            });

            modelBuilder.Entity<eHubInterfaceCounter>(entity =>
            {
                entity.HasKey(e => new { e.CT_TS_PK, e.CT_Name });

                entity.Property(e => e.CT_Name).HasMaxLength(200);

                entity.Property(e => e.CT_LastUpdateUTC).HasColumnType("datetime");
            });

            modelBuilder.Entity<eHubITCustomsJobStatus>(entity =>
            {
                entity.HasKey(e => e.IT_PK);

                entity.Property(e => e.IT_PK).ValueGeneratedNever();

                entity.Property(e => e.IT_FileLastModifiedUTC).HasColumnType("datetime");

                entity.Property(e => e.IT_FileName)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.IT_JobID)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.IT_LastStatus)
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.IT_MessageType)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.IT_PollingStartUTC).HasColumnType("datetime");

                entity.Property(e => e.IT_ReferenceID)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.IT_Version)
                    .IsRequired()
                    .IsRowVersion();

                entity.HasOne(d => d.IT_CC_SenderNavigation)
                    .WithMany(p => p.eHubITCustomsJobStatus)
                    .HasForeignKey(d => d.IT_CC_Sender)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubITCustomsJobStatus_IT_CC_Sender_FK2_eHubClient_RRR_121");

                entity.HasOne(d => d.IT_EH_ClientSystemNavigation)
                    .WithMany(p => p.eHubITCustomsJobStatus)
                    .HasForeignKey(d => d.IT_EH_ClientSystem)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubITCustomsJobStatus_IT_EH_ClientSystem_FK2_eHubClientSystem_RRR_121");
            });

            modelBuilder.Entity<eHubMessageCopyRegistry>(entity =>
            {
                entity.HasKey(e => new { e.MC_CC_Sender, e.MC_CC_Recipient, e.MC_CC_OriginalMessage_Copy_Recipient });

                entity.Property(e => e.MC_InboxLikePattern).HasMaxLength(2000);

                entity.Property(e => e.MC_OutboxLikePattern).HasMaxLength(2000);

                entity.Property(e => e.MC_OutboxMessageXpath).HasMaxLength(2000);

                entity.HasOne(d => d.MC_CC_OriginalMessage_Copy_RecipientNavigation)
                    .WithMany(p => p.eHubMessageCopyRegistryMC_CC_OriginalMessage_Copy_RecipientNavigation)
                    .HasForeignKey(d => d.MC_CC_OriginalMessage_Copy_Recipient)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_eHubMessageCopyRegistry_eHubClient2");

                entity.HasOne(d => d.MC_CC_RecipientNavigation)
                    .WithMany(p => p.eHubMessageCopyRegistryMC_CC_RecipientNavigation)
                    .HasForeignKey(d => d.MC_CC_Recipient)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_eHubMessageCopyRegistry_eHubClient1");

                entity.HasOne(d => d.MC_CC_SenderNavigation)
                    .WithMany(p => p.eHubMessageCopyRegistryMC_CC_SenderNavigation)
                    .HasForeignKey(d => d.MC_CC_Sender)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_eHubMessageCopyRegistry_eHubClient");
            });

            modelBuilder.Entity<eHubMessageReferenceRegistry>(entity =>
            {
                entity.HasKey(e => e.CR_PK);

                entity.HasIndex(e => new { e.CR_ApplicationCode, e.CR_MessageReference })
                    .HasName("IX_UX__CR_ApplicationCodeMessageReference")
                    .IsUnique();

                entity.Property(e => e.CR_PK).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CR_ApplicationCode)
                    .IsRequired()
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.CR_MessageReference)
                    .IsRequired()
                    .HasMaxLength(160)
                    .IsUnicode(false);

                entity.Property(e => e.CR_Password)
                    .HasMaxLength(160)
                    .IsUnicode(false);

                entity.HasOne(d => d.CR_CC_ClientNavigation)
                    .WithMany(p => p.eHubMessageReferenceRegistry)
                    .HasForeignKey(d => d.CR_CC_Client)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_eHubClientMessageReference_eHubClient");
            });

            modelBuilder.Entity<eHubMessageType>(entity =>
            {
                entity.HasKey(e => e.DT_PK)
                    .ForSqlServerIsClustered(false);

                entity.Property(e => e.DT_PK).ValueGeneratedNever();

                entity.Property(e => e.DT_Charset)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DT_Code)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.DT_EnvelopeXpath)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.HasOne(d => d.DT_DT_InnerTypeNavigation)
                    .WithMany(p => p.InverseDT_DT_InnerTypeNavigation)
                    .HasForeignKey(d => d.DT_DT_InnerType)
                    .HasConstraintName("eHubMessageType_DT_DT_InnerType_FK2_eHubMessageType_RRR_121");

                entity.HasOne(d => d.DT_DT_PostAssembleWrapperNavigation)
                    .WithMany(p => p.InverseDT_DT_PostAssembleWrapperNavigation)
                    .HasForeignKey(d => d.DT_DT_PostAssembleWrapper)
                    .HasConstraintName("eHubMessageType_DT_DT_PostAssembleWrapper_FK2_eHubMessageType_RRR_121");
            });

            modelBuilder.Entity<eHubOutboxMessage>(entity =>
            {
                entity.HasKey(e => e.OI_PK)
                    .ForSqlServerIsClustered(false);

                entity.HasIndex(e => e.OI_BatchEnvelopeTrackingID)
                    .HasName("NR_RX__OI_OutboxUpdateBatch");

                entity.HasIndex(e => e.OI_EI_InboxPK)
                    .HasName("NR_RX__OI_EI_InboxPK");

                entity.HasIndex(e => e.OI_EnvelopeTrackingID)
                    .HasName("NR_RX__OI_EnvelopeTrackingID");

                entity.HasIndex(e => e.OI_InsertUTC)
                    .HasName("NR_RX__OI_InsertUTC")
                    .ForSqlServerIsClustered();

                entity.HasIndex(e => new { e.OI_CC_Recipient, e.OI_Status })
                    .HasName("NR_RX__OI_RecipientStatus");

                entity.Property(e => e.OI_PK).ValueGeneratedNever();

                entity.Property(e => e.OI_BatchEnvelopeTrackingID)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.OI_CompressedLength).HasComputedColumnSql("(len([OI_CONTENT]))");

                entity.Property(e => e.OI_Content).IsUnicode(false);

                entity.Property(e => e.OI_EI_InboxPK)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.OI_EnvelopeTrackingID)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.OI_InsertUTC)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.OI_LastUpdateUTC)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.OI_MessageTrackingID)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.OI_OverrideEmailSubject).HasMaxLength(512);

                entity.Property(e => e.OI_OverrideFilename).HasMaxLength(512);

                entity.Property(e => e.OI_XmlContent).HasColumnType("xml");

                entity.HasOne(d => d.OI_CC_RecipientNavigation)
                    .WithMany(p => p.eHubOutboxMessageOI_CC_RecipientNavigation)
                    .HasForeignKey(d => d.OI_CC_Recipient)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubOutboxMessage_OI_CC_Recipient_FK2_eHubClient_RRR_121");

                entity.HasOne(d => d.OI_CC_SenderNavigation)
                    .WithMany(p => p.eHubOutboxMessageOI_CC_SenderNavigation)
                    .HasForeignKey(d => d.OI_CC_Sender)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubOutboxMessage_OI_CC_Sender_FK2_eHubClient_RRR_121");

                entity.HasOne(d => d.OI_DT_TargetNavigation)
                    .WithMany(p => p.eHubOutboxMessage)
                    .HasForeignKey(d => d.OI_DT_Target)
                    .HasConstraintName("eHubOutboxMessage_OI_DT_Target_FK2_eHubMessageType_RRR_121");
            });

            modelBuilder.Entity<eHubOutboxMessageArchive>(entity =>
            {
                entity.HasKey(e => e.OI_PK)
                    .ForSqlServerIsClustered(false);

                entity.HasIndex(e => e.OI_EI_InboxPK)
                    .HasName("NR_RX__AR_OI_EI_InboxPK")
                    .ForSqlServerIsClustered();

                entity.Property(e => e.OI_PK).ValueGeneratedNever();

                entity.Property(e => e.OI_BatchEnvelopeTrackingID)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.OI_Content).IsUnicode(false);

                entity.Property(e => e.OI_EI_InboxPK)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.OI_EnvelopeTrackingID)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.OI_InsertUTC).HasColumnType("datetime");

                entity.Property(e => e.OI_LastUpdateUTC).HasColumnType("datetime");

                entity.Property(e => e.OI_MessageTrackingID)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.OI_OverrideEmailSubject).HasMaxLength(512);

                entity.Property(e => e.OI_OverrideFilename).HasMaxLength(512);

                entity.Property(e => e.OI_XmlContent).HasColumnType("xml");
            });

            modelBuilder.Entity<eHubReferenceFileCache>(entity =>
            {
                entity.HasKey(e => e.RC_PK)
                    .ForSqlServerIsClustered(false);

                entity.HasIndex(e => new { e.RC_RF, e.RC_ReceivedUTC })
                    .HasName("IX_UX__RC_RF_ReceivedUTC")
                    .IsUnique();

                entity.Property(e => e.RC_PK).ValueGeneratedNever();

                entity.Property(e => e.RC_Data).IsRequired();

                entity.Property(e => e.RC_ReceivedUTC).HasColumnType("datetime");

                entity.HasOne(d => d.RC_RFNavigation)
                    .WithMany(p => p.eHubReferenceFileCache)
                    .HasForeignKey(d => d.RC_RF)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubReferenceFileCachce_RC_RF_FK2_eHubReferenceFile_RRR_121");
            });

            modelBuilder.Entity<eHubReferenceFileQuery>(entity =>
            {
                entity.HasKey(e => e.RF_PK)
                    .ForSqlServerIsClustered(false);

                entity.Property(e => e.RF_PK).ValueGeneratedNever();

                entity.Property(e => e.RF_ApplicationCode)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.RF_Description)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.RF_Query)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.RF_ReferenceID)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RF_Type)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.HasOne(d => d.RF_CC_ProviderNavigation)
                    .WithMany(p => p.eHubReferenceFileQueryRF_CC_ProviderNavigation)
                    .HasForeignKey(d => d.RF_CC_Provider)
                    .HasConstraintName("eHubReferenceFile_RF_CC_Provider_FK2_eHubClient_RRR_121");

                entity.HasOne(d => d.RF_CC_RequestorNavigation)
                    .WithMany(p => p.eHubReferenceFileQueryRF_CC_RequestorNavigation)
                    .HasForeignKey(d => d.RF_CC_Requestor)
                    .HasConstraintName("eHubReferenceFile_RF_CC_Requestor_FK2_eHubClient_RRR_121");
            });

            modelBuilder.Entity<eHubRegistrationType>(entity =>
            {
                entity.HasKey(e => e.RT_PK);

                entity.HasIndex(e => e.RT_ID)
                    .HasName("IX_UX__RT_ID")
                    .IsUnique();

                entity.Property(e => e.RT_PK).ValueGeneratedNever();

                entity.Property(e => e.RT_Description)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.RT_ID)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.RT_RegistrantType)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<eHubRoutingRule>(entity =>
            {
                entity.HasKey(e => e.RR_PK);

                entity.Property(e => e.RR_PK).ValueGeneratedNever();

                entity.Property(e => e.RR_Failed_ErrorCode).HasMaxLength(50);

                entity.Property(e => e.RR_Failed_ErrorDescription).HasMaxLength(2000);

                entity.Property(e => e.RR_Result_Value).HasMaxLength(2000);

                entity.HasOne(d => d.RR_Failed_RR_SubRuleNavigation)
                    .WithMany(p => p.InverseRR_Failed_RR_SubRuleNavigation)
                    .HasForeignKey(d => d.RR_Failed_RR_SubRule)
                    .HasConstraintName("eHubRoutingRule_RR_Failed_RR_SubRule_FK2_eHubRoutingRule");

                entity.HasOne(d => d.RR_Group_RR_GroupRuleNavigation)
                    .WithMany(p => p.InverseRR_Group_RR_GroupRuleNavigation)
                    .HasForeignKey(d => d.RR_Group_RR_GroupRule)
                    .HasConstraintName("eHubRoutingRule_RR_Group_RR_GroupRule_FK2_eHubRoutingRule");

                entity.HasOne(d => d.RR_Success_CC_RecipientNavigation)
                    .WithMany(p => p.eHubRoutingRule)
                    .HasForeignKey(d => d.RR_Success_CC_Recipient)
                    .HasConstraintName("eHubRoutingRule_RR_Success_CC_Recipient_FK2_eHubClient");

                entity.HasOne(d => d.RR_Success_RR_SubRuleNavigation)
                    .WithMany(p => p.InverseRR_Success_RR_SubRuleNavigation)
                    .HasForeignKey(d => d.RR_Success_RR_SubRule)
                    .HasConstraintName("eHubRoutingRule_RR_Success_RR_SubRule_FK2_eHubRoutingRule");

                entity.HasOne(d => d.RR_Success_SP_ProviderNavigation)
                    .WithMany(p => p.eHubRoutingRule)
                    .HasForeignKey(d => d.RR_Success_SP_Provider)
                    .HasConstraintName("eHubRoutingRule_RR_Success_SP_Provider_FK2_eHubServiceProvider");
            });

            modelBuilder.Entity<eHubRoutingRuleFact>(entity =>
            {
                entity.HasKey(e => e.RX_PK);

                entity.Property(e => e.RX_PK).ValueGeneratedNever();

                entity.Property(e => e.RX_Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.RX_Type)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.RX_RRNavigation)
                    .WithMany(p => p.eHubRoutingRuleFactRX_RRNavigation)
                    .HasForeignKey(d => d.RX_RR)
                    .HasConstraintName("eHubRoutingRuleFact_RX_RR_FK2_eHubRoutingRule");

                entity.HasOne(d => d.RX_RR_ComputeRuleNavigation)
                    .WithMany(p => p.eHubRoutingRuleFactRX_RR_ComputeRuleNavigation)
                    .HasForeignKey(d => d.RX_RR_ComputeRule)
                    .HasConstraintName("eHubRoutingRuleFact_RX_RR_ComputeRule_FK2_eHubRoutingRule");
            });

            modelBuilder.Entity<eHubSequenceNumber>(entity =>
            {
                entity.HasKey(e => new { e.SN_CC_Sender, e.SN_CC_Recipient });
            });

            modelBuilder.Entity<eHubServiceOperator>(entity =>
            {
                entity.HasKey(e => e.SO_PK);

                entity.Property(e => e.SO_PK).ValueGeneratedNever();

                entity.Property(e => e.SO_ID)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.SO_Name)
                    .IsRequired()
                    .HasMaxLength(250);
            });

            modelBuilder.Entity<eHubServiceOperatorRegistration>(entity =>
            {
                entity.HasKey(e => new { e.SR_SO, e.SR_RT });

                entity.Property(e => e.SR_Code).HasMaxLength(250);

                entity.HasOne(d => d.SR_RTNavigation)
                    .WithMany(p => p.eHubServiceOperatorRegistration)
                    .HasForeignKey(d => d.SR_RT)
                    .HasConstraintName("eHubServiceOperatorRegistration_SR_RT_FK2_eHubRegistrationType");

                entity.HasOne(d => d.SR_SONavigation)
                    .WithMany(p => p.eHubServiceOperatorRegistration)
                    .HasForeignKey(d => d.SR_SO)
                    .HasConstraintName("eHubServiceOperatorRegistration_SR_SO_FK2_eHubServiceOperator");
            });

            modelBuilder.Entity<eHubServiceProvider>(entity =>
            {
                entity.HasKey(e => e.SP_PK);

                entity.HasIndex(e => new { e.SP_CC_Service, e.SP_CC_Provider })
                    .HasName("IX_UX__SP_CC_Service_Provider")
                    .IsUnique();

                entity.Property(e => e.SP_PK).ValueGeneratedNever();

                entity.HasOne(d => d.SP_CC_ProviderNavigation)
                    .WithMany(p => p.eHubServiceProviderSP_CC_ProviderNavigation)
                    .HasForeignKey(d => d.SP_CC_Provider)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubServiceProvider_SP_CC_Provider_FK2_eHubClient_RRR_121");

                entity.HasOne(d => d.SP_CC_ServiceNavigation)
                    .WithMany(p => p.eHubServiceProviderSP_CC_ServiceNavigation)
                    .HasForeignKey(d => d.SP_CC_Service)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubServiceProvider_SP_CC_Service_FK2_eHubClient_RRR_121");
            });

            modelBuilder.Entity<eHubServiceProviderRequiredRegistration>(entity =>
            {
                entity.HasKey(e => new { e.SX_SP, e.SX_RT });

                entity.Property(e => e.SX_LookupFactName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.SX_QualifierFactName).HasMaxLength(50);

                entity.HasOne(d => d.SX_RTNavigation)
                    .WithMany(p => p.eHubServiceProviderRequiredRegistration)
                    .HasForeignKey(d => d.SX_RT)
                    .HasConstraintName("eHubServiceProviderRequiredRegistration_SX_RT_FK2_eHubRegistrationType");

                entity.HasOne(d => d.SX_SPNavigation)
                    .WithMany(p => p.eHubServiceProviderRequiredRegistration)
                    .HasForeignKey(d => d.SX_SP)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubServiceProviderRequiredRegistration_SX_SP_FK2_eHubServiceProvider");
            });

            modelBuilder.Entity<eHubSubscriptionAutoSubscribe>(entity =>
            {
                entity.HasKey(e => e.SA_PK)
                    .ForSqlServerIsClustered(false);

                entity.Property(e => e.SA_PK).ValueGeneratedNever();

                entity.Property(e => e.SA_ReferenceProperty).HasMaxLength(500);

                entity.Property(e => e.SA_ReferenceXpath).HasMaxLength(2000);

                entity.Property(e => e.SA_ValueProperty).HasMaxLength(500);

                entity.Property(e => e.SA_ValueXpath).HasMaxLength(2000);

                entity.HasOne(d => d.SA_DTNavigation)
                    .WithMany(p => p.eHubSubscriptionAutoSubscribe)
                    .HasForeignKey(d => d.SA_DT)
                    .HasConstraintName("eHubSubscriptionAutoSubscribe_SA_DT_FK2_eHubMessageType_RRR_121");

                entity.HasOne(d => d.SA_STNavigation)
                    .WithMany(p => p.eHubSubscriptionAutoSubscribe)
                    .HasForeignKey(d => d.SA_ST)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubSubscriptionAutoSubscribe_SA_ST_FK2_eHubSubscriptionType_RRR_121");
            });

            modelBuilder.Entity<eHubSubscriptionBroadcaster>(entity =>
            {
                entity.HasKey(e => e.SB_PK);

                entity.Property(e => e.SB_PK).ValueGeneratedNever();

                entity.HasOne(d => d.SB_CCNavigation)
                    .WithMany(p => p.eHubSubscriptionBroadcaster)
                    .HasForeignKey(d => d.SB_CC)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubSubscriptionBroadcaster_SB_CC_FK2_eHubClient_RRR_121");

                entity.HasOne(d => d.SB_STNavigation)
                    .WithMany(p => p.eHubSubscriptionBroadcaster)
                    .HasForeignKey(d => d.SB_ST)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubSubscriptionBroadcaster_SB_ST_FK2_eHubSubscriptionType_RRR_121");
            });

            modelBuilder.Entity<eHubSubscriptionLookup>(entity =>
            {
                entity.HasKey(e => e.SL_PK)
                    .ForSqlServerIsClustered(false);

                entity.Property(e => e.SL_PK).ValueGeneratedNever();

                entity.Property(e => e.SL_ValueProperty).HasMaxLength(500);

                entity.Property(e => e.SL_ValueXpath).HasMaxLength(2000);

                entity.HasOne(d => d.SL_DTNavigation)
                    .WithMany(p => p.eHubSubscriptionLookup)
                    .HasForeignKey(d => d.SL_DT)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubSubscriptionLookup_SL_DT_FK2_eHubMessageType_RRR_121");

                entity.HasOne(d => d.SL_STNavigation)
                    .WithMany(p => p.eHubSubscriptionLookup)
                    .HasForeignKey(d => d.SL_ST)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubSubscriptionLookup_SL_ST_FK2_eHubSubscriptionType_RRR_121");
            });

            modelBuilder.Entity<eHubSubscriptionType>(entity =>
            {
                entity.HasKey(e => e.ST_PK)
                    .ForSqlServerIsClustered(false);

                entity.HasIndex(e => new { e.ST_ID, e.ST_Name })
                    .HasName("IX_UX__ST_ID_Name")
                    .IsUnique()
                    .ForSqlServerIsClustered();

                entity.Property(e => e.ST_PK).ValueGeneratedNever();

                entity.Property(e => e.ST_ID)
                    .IsRequired()
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.ST_Name)
                    .IsRequired()
                    .HasMaxLength(36)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<eHubSubscriptionValue>(entity =>
            {
                entity.HasKey(e => e.SV_PK)
                    .ForSqlServerIsClustered(false);

                entity.HasIndex(e => new { e.SV_ST, e.SV_CC_Sender, e.SV_CC_Recipient, e.SV_Value, e.SV_ReferenceType })
                    .HasName("IX_UX__SV_ST_CC_Value")
                    .IsUnique();

                entity.Property(e => e.SV_PK).ValueGeneratedNever();

                entity.Property(e => e.SV_ExpiryUTC).HasColumnType("datetime");

                entity.Property(e => e.SV_ReferenceType)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.SV_SubscribedUTC).HasColumnType("datetime");

                entity.Property(e => e.SV_Value)
                    .IsRequired()
                    .HasMaxLength(160);

                entity.HasOne(d => d.SV_CC_RecipientNavigation)
                    .WithMany(p => p.eHubSubscriptionValueSV_CC_RecipientNavigation)
                    .HasForeignKey(d => d.SV_CC_Recipient)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubSubscriptionValue_SV_CC_Recipient_FK2_eHubClient_RRR_121");

                entity.HasOne(d => d.SV_CC_SenderNavigation)
                    .WithMany(p => p.eHubSubscriptionValueSV_CC_SenderNavigation)
                    .HasForeignKey(d => d.SV_CC_Sender)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubSubscriptionValue_SV_CC_Sender_FK2_eHubClient_RRR_121");

                entity.HasOne(d => d.SV_STNavigation)
                    .WithMany(p => p.eHubSubscriptionValue)
                    .HasForeignKey(d => d.SV_ST)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubSubscriptionValue_SV_ST_FK2_eHubSubscriptionType_RRR_121");
            });

            modelBuilder.Entity<eHubTransformationMapping>(entity =>
            {
                entity.HasKey(e => new { e.TM_TS_PK, e.TM_Order, e.TM_TT_PK })
                    .ForSqlServerIsClustered(false);

                entity.HasOne(d => d.TM_TS_PKNavigation)
                    .WithMany(p => p.eHubTransformationMapping)
                    .HasForeignKey(d => d.TM_TS_PK)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubTransformationMapping_TM_TS_FK2_eHubTransformationSet_RRR_121");

                entity.HasOne(d => d.TM_TT_PKNavigation)
                    .WithMany(p => p.eHubTransformationMapping)
                    .HasForeignKey(d => d.TM_TT_PK)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubTransformationMapping_TM_TT_FK2_eHubTransformationType_RRR_121");
            });

            modelBuilder.Entity<eHubTransformationSet>(entity =>
            {
                entity.HasKey(e => e.TS_PK)
                    .ForSqlServerIsClustered(false);

                entity.HasIndex(e => new { e.TS_Name, e.TS_CC_Sender, e.TS_CC_Recipient })
                    .HasName("IX_UX__TS_NameSenderRecipient")
                    .IsUnique();

                entity.Property(e => e.TS_PK).ValueGeneratedNever();

                entity.Property(e => e.TS_BillingElement).HasMaxLength(35);

                entity.Property(e => e.TS_BillingFee).HasColumnType("smallmoney");

                entity.Property(e => e.TS_BillingInterfaceName).HasMaxLength(250);

                entity.Property(e => e.TS_BillingXPathSource).HasMaxLength(2048);

                entity.Property(e => e.TS_BillingXPathTarget).HasMaxLength(2048);

                entity.Property(e => e.TS_Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.TS_XPathPredicate).HasMaxLength(1024);

                entity.HasOne(d => d.TS_CC_BillOtherNavigation)
                    .WithMany(p => p.eHubTransformationSetTS_CC_BillOtherNavigation)
                    .HasForeignKey(d => d.TS_CC_BillOther)
                    .HasConstraintName("eHubTransformationSet_TS_CC_BillOther_FK2_eHubClient_RRR_121");

                entity.HasOne(d => d.TS_CC_RecipientNavigation)
                    .WithMany(p => p.eHubTransformationSetTS_CC_RecipientNavigation)
                    .HasForeignKey(d => d.TS_CC_Recipient)
                    .HasConstraintName("eHubTransformationSet_TS_CC_Recipient_FK2_eHubClient_RRR_121");

                entity.HasOne(d => d.TS_CC_SenderNavigation)
                    .WithMany(p => p.eHubTransformationSetTS_CC_SenderNavigation)
                    .HasForeignKey(d => d.TS_CC_Sender)
                    .HasConstraintName("eHubTransformationSet_TS_CC_Sender_FK2_eHubClient_RRR_121");

                entity.HasOne(d => d.TS_DT_SourceNavigation)
                    .WithMany(p => p.eHubTransformationSet)
                    .HasForeignKey(d => d.TS_DT_Source)
                    .HasConstraintName("eHubTransformationSet_TS_DT_Source_FK2_eHubMessageType_RRR_121");
            });

            modelBuilder.Entity<eHubTransformationType>(entity =>
            {
                entity.HasKey(e => e.TT_PK)
                    .ForSqlServerIsClustered(false);

                entity.Property(e => e.TT_PK).ValueGeneratedNever();

                entity.Property(e => e.TT_TransformationType)
                    .HasMaxLength(512)
                    .IsUnicode(false);

                entity.HasOne(d => d.TT_DT_SourceNavigation)
                    .WithMany(p => p.eHubTransformationTypeTT_DT_SourceNavigation)
                    .HasForeignKey(d => d.TT_DT_Source)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubTransformationType_TT_DT_Source_FK2_eHubMessageType_RRR_121");

                entity.HasOne(d => d.TT_DT_TargetNavigation)
                    .WithMany(p => p.eHubTransformationTypeTT_DT_TargetNavigation)
                    .HasForeignKey(d => d.TT_DT_Target)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("eHubTransformationType_TT_DT_Target_FK2_eHubMessageType_RRR_121");
            });

            modelBuilder.Entity<eHubUSCustomsRegistry>(entity =>
            {
                entity.HasKey(e => e.ER_PK)
                    .ForSqlServerIsClustered(false);

                entity.Property(e => e.ER_PK).ValueGeneratedNever();

                entity.Property(e => e.ER_ApplicationCode)
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.ER_Name)
                    .IsRequired()
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.ER_Value)
                    .IsRequired()
                    .HasMaxLength(160)
                    .IsUnicode(false);

                entity.HasOne(d => d.ER_CC_ClientNavigation)
                    .WithMany(p => p.eHubUSCustomsRegistry)
                    .HasForeignKey(d => d.ER_CC_Client)
                    .HasConstraintName("eHubRegistry_ER_CC_Client_FK2_eHubClient_RRR_121");
            });

            modelBuilder.Entity<eHubZone>(entity =>
            {
                entity.HasKey(e => e.ZZ_PK)
                    .ForSqlServerIsClustered(false);

                entity.Property(e => e.ZZ_PK).ValueGeneratedNever();

                entity.Property(e => e.ZZ_ID)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });
        }

        public DbConnection GetDbConnection()
        {
            return base.Database.GetDbConnection();
        }

        #region IDisposable

        bool disposed = false;
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

            base.Dispose();
        }  

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                DisposeInternal();
            }

            disposed = true;
        }

        void DisposeInternal()
        {
        }

        #endregion
    }
}
