using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class JobSailingCollection : BusinessObjectCollection<JobSailing>
	{
		public JobSailingCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public JobSailingCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#region Helper Methods

		/// <summary>
		/// Finds a sailing based on the supplied ports.
		/// </summary>
		/// <param name="load">UNLOCO code of the load port.</param>
		/// <param name="discharge">UNLOCO code of the discharge port.</param>
		/// <returns>The sailing that matches the given ports, null if not found.</returns>
		public JobSailing GetSailingFromLoadAndDischarge(ZString load, ZString discharge)
		{
			JobSailing result = null;
			foreach (JobSailing sailing in this)
			{
				if (sailing.JX_JB_RL_NKPortOfDischarge == discharge && sailing.JX_JA_RL_NKPortOfLoading == load)
				{
					result = sailing;
					break;
				}
			}
			return result;
		}

		#endregion
	}
}
