namespace Enterprise.Freight.Agency.Business
{
	public class PortMessageHostValidation : AutoPortMessageHostValidation
	{
		public PortMessageHostValidation(AutoPortMessageHost parent)
			: base(parent) { }

		#region Implementation

		public new PortMessageHost Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (PortMessageHost)base.Parent; }
		}

		#endregion
	}
}
