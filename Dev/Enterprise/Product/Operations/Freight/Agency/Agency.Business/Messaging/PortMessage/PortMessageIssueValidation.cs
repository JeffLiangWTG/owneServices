namespace Enterprise.Freight.Agency.Business
{
	public class PortMessageIssueValidation : AutoPortMessageIssueValidation
	{
		public PortMessageIssueValidation(AutoPortMessageIssue parent)
			: base(parent) { }

		#region Implementation

		public new PortMessageIssue Parent
		{
			get { return (PortMessageIssue)base.Parent; }
		}

		#endregion
	}
}
