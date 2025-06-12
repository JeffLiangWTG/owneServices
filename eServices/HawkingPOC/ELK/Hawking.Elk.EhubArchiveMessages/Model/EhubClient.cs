using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hawking.Elk.Common.Model;

namespace Hawking.Elk.EhubArchiveMessages.Model
{
    [Table("eHubClient")]
    public partial class EhubClient : IEntity
    {
        [Key]
        public Guid TrackingId { get; set; }

        [Required]
        public string CC_ID { get; set; }

        [Required]
        public string CC_FriendlyName { get; set; }
        public Guid CC_Odyssey_OH { get; set; }
        public Guid? CC_DistributionZone { get; set; }

        [Required]
        public string CC_EmailAddress { get; set; }

        [Required]
        public string CC_Password { get; set; }
        public bool? CC_IsAirServiceProvider { get; set; }
        public string CC_AirlineCode { get; set; }
        public Guid? CC_AirServiceProvider { get; set; }
        public string CC_AirlinePrefix { get; set; }
        public bool? CC_USCustomsRecipient { get; set; }
        public string CC_AS2_Code { get; set; }
        public string CC_SCAC_Code { get; set; }

        [Required]
        public string CC_OwnerCategory { get; set; }

        [Required]
        public string CC_SystemCategory { get; set; }
        public Guid? CC_RR { get; set; }
        public bool CC_RequireStatusResponse { get; set; }
    }
}
