using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubTransformationType
	{
		#region Fields
		[Key]
		public virtual Guid TT_PK { get; set; }
		public virtual Guid TT_DT_Source { get; set; }
		public virtual Guid TT_DT_Target { get; set; }
		public virtual string TT_TransformationType { get; set; }
		#endregion

		#region Relationships
		[ForeignKey("TT_DT_Source")]
		public virtual eHubMessageType eHubMessageType_Source { get; set; }
		[ForeignKey("TT_DT_Target")]
		public virtual eHubMessageType eHubMessageType_Target { get; set; }
		[InverseProperty("eHubTransformationType")]
		public virtual List<eHubTransformationMapping> eHubTransformationMappings { get; set; }
		#endregion

		#region Default Constructor
		public eHubTransformationType()
		{

		}
		#endregion
	}
}
