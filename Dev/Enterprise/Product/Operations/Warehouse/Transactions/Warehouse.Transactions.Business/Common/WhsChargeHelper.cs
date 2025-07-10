using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class WhsChargeHelper
	{
		#region GetDocket

		public static WhsDocket GetDocket(JobCharge charge)
		{
			return charge.Factory.GetCachedValue(string.Format(Culture.Invariant, "DocketFromCharge-{0}", charge.PK), () => // Key used in factory cache
				{
					WhsDocket docket = null;

					var job = charge.Job;
					if (job != null && !charge.JobChargeAttrib_DocketReference.IsEmpty)
					{
						var jobsDocket = job.Parent as WhsDocket;
						if (jobsDocket != null && jobsDocket.WD_ExternalReference == charge.JobChargeAttrib_DocketReference)
						{
							docket = jobsDocket;
						}
						else
						{
							var query = new ZQuery(WhsDocketSchema.WD_ExternalReference, charge.JobChargeAttrib_DocketReference);
							query.AddToFilter(WhsDocketSchema.WD_OH_Client, job.LocalChargesPK);

							docket = charge.Factory.LoadTop1<WhsDocket>(query);
						}
					}

					return docket;
				});
		}

		#endregion
	}
}

// Tested in Enterprise.Warehouse.Transactions.Invoicing.Testing.WhsChargeHelperTest
