using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CargoWise.eHub.Portal.Models.eHubTransactions
{
	[MetadataType(typeof(eHubMessageType_Validation))]
	public partial class eHubMessageType
	{
		[Bind(Exclude = "DT_Code")]
		internal class eHubMessageType_Validation
		{
			[Required(ErrorMessage = "IsFlatFile is required")]
			public virtual bool DT_IsFlatFile { get; set; }

			[Required(ErrorMessage = "IsEDI is required")]
			public virtual bool DT_IsEDI { get; set; }
		}
	}
}