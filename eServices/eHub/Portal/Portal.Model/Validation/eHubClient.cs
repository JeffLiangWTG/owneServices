using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CargoWise.eHub.Portal.Models.eHubTransactions
{
	[MetadataType(typeof(eHubClient_Validation))]
	public partial class eHubClient
	{
		public string DisplayName
		{
			get
			{
				if (!String.IsNullOrEmpty(CC_FriendlyName) && !String.IsNullOrEmpty(CC_ID)) return String.Format("{0} ({1})", CC_FriendlyName, CC_ID);
				else return "";
			}
		}

		[Bind(Exclude = "CC_EmailAddress, CC_Password")]
		internal class eHubClient_Validation
		{
			[Required(ErrorMessage = "PK is required")]
			public virtual System.Guid CC_PK { get; set; }

			[Required(ErrorMessage = "ID is required")]
			[StringLength(36, ErrorMessage = "ID may not be longer than 36 characters")]
			public virtual string CC_ID{ get;	set; }

			[Required(ErrorMessage = "Name is required")]
			[StringLength(128, ErrorMessage = "Name may not be longer than 128 characters")]
			public virtual string CC_FriendlyName{ get;	set; }

            [StringLength(50, ErrorMessage = "AS2 Code may not be longer than 50 characters")]
            public virtual string CC_AS2_Code { get; set; }
		}
	}
}