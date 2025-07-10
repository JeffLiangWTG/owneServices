using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class SupervisorOverridesLookups : ZLookups
	{
		public SupervisorOverridesLookups(SupervisorOverrides parent)
			: base(parent)
		{
		}

		#region Users

		public GlbStaffCollection Users
		{
			get
			{
				if (users == null)
				{
					users = new GlbStaffCollection(Factory);
				}

				return users;
			}
		}
		GlbStaffCollection users;

		#endregion

		#region Implementation

		protected new SupervisorOverrides Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (SupervisorOverrides)base.Parent; }
		}

		#endregion
	}
}
