using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public class UpdateRateLookups : ZLookups
	{
		public UpdateRateLookups(UpdateRate parent)
			: base(parent) { }

		public OrgHeaderCollection Clients
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		#region Implementation

		protected new UpdateRate Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (UpdateRate)base.Parent; }
		}

		#endregion
	}
}

