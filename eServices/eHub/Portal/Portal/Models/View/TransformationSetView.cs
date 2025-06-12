using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Models
{
	public class TransformationSetView
	{
		public eHubTransformationSet TransformationSet { get; set; }
		public List<TransformationMappingView> MappingList { get; set; }

		public SelectValueView Sender { get; set; }
		public SelectValueView Recipient { get; set; }
		public MessageTypeView Source { get; set; }
		public SelectValueView BillOther { get; set; }
	}
}