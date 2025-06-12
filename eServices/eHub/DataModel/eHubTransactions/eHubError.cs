using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
    public partial class eHubError
    {
        #region Fields
        [Key]
        public virtual Guid EE_PK { get; set; }
        public virtual DateTime EE_DateTimeUTC { get; set; }
        public virtual string EE_Source { get; set; }
        public virtual string EE_ErrorType { get; set; }
        public virtual string EE_Description { get; set; }
        [Column(TypeName = "xml")]
        public virtual string EE_ErrorDetail { get; set; }
        public virtual Guid? EE_EI_Inbox { get; set; }
        public virtual Guid? EE_OI_Outbox { get; set; }
        public virtual bool EE_Alerted { get; set; }

        #endregion

        #region Relationships
        [ForeignKey("EE_EI_Inbox")]
        public virtual eHubInboxMessage eHubInboxMessage { get; set; }
        [ForeignKey("EE_OI_Outbox")]
        public virtual eHubOutboxMessage eHubOutboxMessage { get; set; }

        [ForeignKey("EE_EI_Inbox")]
        public virtual eHubInboxMessageDisplay eHubInboxMessageDisplay { get; set; }
        [ForeignKey("EE_OI_Outbox")]
        public virtual eHubOutboxMessageDisplay eHubOutboxMessageDisplay { get; set; }
        #endregion

        #region Default Constructor
        public eHubError()
        {
        }
        #endregion

    }
}
