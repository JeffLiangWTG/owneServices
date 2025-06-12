using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Models
{
	public class TransformationMappingView
	{
		public eHubTransformationMapping Mapping { get; set; }
		public string TransformationTypeName { get; set; }
	}
}