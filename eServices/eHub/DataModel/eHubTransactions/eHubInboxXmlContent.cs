using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public class eHubInboxXmlContent
	{
		[Key]
		public Guid EX_EI_Inbox { get; set; }
		public Guid? EX_DT_Source { get; set; }
		public long? EX_UncompressedLength { get; set; }
		[Column(TypeName = "xml")]
		public string EX_XmlContent { get; set; }

		[ForeignKey("EX_DT_Source")]
        public virtual eHubMessageType eHubMessageType { get; set; }
	}
}
