using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubCodeMapValue
	{
		#region Fields
		[Key, Column(Order = 1)]
		public virtual Guid CV_CK { get; set; }
		[Key, Column(Order = 2)]
		public virtual Guid CV_CR { get; set; }
		public virtual string CV_OutputCode { get; set; }
		public virtual Nullable<int> CV_PassThroughKey { get; set; }
		#endregion

		#region Relationships
		[ForeignKey("CV_CK")]
		public virtual eHubCodeMapKey eHubCodeMapKey { get; set; }
		[ForeignKey("CV_CR")]
		public virtual eHubCodeSetResult eHubCodeSetResult { get; set; }
		#endregion

		#region Default Constructor
		public eHubCodeMapValue()
		{

		}
		#endregion
	}
}
