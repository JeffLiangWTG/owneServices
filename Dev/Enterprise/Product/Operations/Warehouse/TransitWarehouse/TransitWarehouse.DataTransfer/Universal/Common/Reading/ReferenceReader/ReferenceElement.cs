using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class ReferenceElement
	{
		public ZString Reference { get; set; }
		public ZString Type { get; set; }
		public ZString Status { get; set; }
		public ZString CountryCode { get; set; }
		public ZString ContextInformation { get; set; }
		public ZDateTime IssueDate { get; set; }
		public ZString SubType { get; set; }
		public ZString ReferencedEntityDescription { get; set; }
		public ZShort Order { get; set; }
		public ZBool IsOverridden { get; set; }
		public List<Date> DateCollection { get; set; }
		public string Category { get; set; }
		public ZString EntryLineReference { get; set; }
		public ZBool EntryIsSystemGenerated { get; set; }
		public ZDateTime ExpiryDate { get; set; }
		public ZString SourceType { get; set; }
	}
}
