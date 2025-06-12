using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubAsyncPollingRegistration
	{
		#region Fields

		[Key]
		public Guid PR_PK { get; set; }
		public Guid PR_RT { get; set; }
		public Guid? PR_CC { get; set; }
		public Guid? PR_EH { get; set; }
		public string PR_Text { get; set; }
		[Column(TypeName = "xml")]
		public string PR_XML { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public DateTime PR_CreatedUTC { get; set; }

		#endregion

		#region Relationships

		[ForeignKey("PR_CC")] public virtual eHubClient eHubClient { get; set; }
		[ForeignKey("PR_EH")] public virtual eHubClientSystem eHubClientSystem { get; set; }
		[ForeignKey("PR_RT")] public virtual eHubRegistrationType eHubRegistrationType { get; set; }

		#endregion

		#region DefaultConstructor

		public eHubAsyncPollingRegistration()
		{
			PR_PK = Guid.NewGuid();
		}

		#endregion
	}
}
