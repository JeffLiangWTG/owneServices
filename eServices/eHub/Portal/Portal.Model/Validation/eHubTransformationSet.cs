using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CargoWise.eHub.Portal.Models.eHubTransactions
{
	[MetadataType(typeof(eHubTransformationSet_Validation))]
	public partial class eHubTransformationSet
	{
		internal class eHubTransformationSet_Validation
		{
			[Required(ErrorMessage = "PK is required")]
			public virtual System.Guid TS_PK{ get;	set; }

			[Required(ErrorMessage = "Name is required")]
			[StringLength(50, ErrorMessage = "Name may not be longer than 100 characters")]
			public virtual string TS_Name{ get;	set; }

			[StringLength(1024, ErrorMessage = "XPathPredicate may not be longer than 1024 characters")]
			public virtual string TS_XPathPredicate{ get;	set; }

			[RegularExpression("^[0-9]{0,10}$", ErrorMessage = "Value must be an interger")]
			public virtual string TS_BillingNumMessagesIncluded { get; set; }

			[DisplayFormat(DataFormatString = "{0:F4}", ApplyFormatInEditMode = true)]
			public virtual decimal TS_BillingFee { get; set; }
		}
	}
}