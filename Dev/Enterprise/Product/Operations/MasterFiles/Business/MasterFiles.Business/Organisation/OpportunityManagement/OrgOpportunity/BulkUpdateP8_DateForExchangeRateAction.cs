using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class BulkUpdateP8_DateForExchangeRateAction : AutoBulkUpdateP8_DateForExchangeRateAction
	{
		public BulkUpdateP8_DateForExchangeRateAction(IEnumerable<OrgOpportunity> opportunities)
		{
			this.opportunityPks = opportunities.Select(x => x.PK).ToArray();
		}

		public int Count
		{
			get { return opportunityPks.Length; }
		}

		readonly ZGuid[] opportunityPks;

		#region Execute

		public void Execute(ZArchitecture.Core.Progress progress)
		{
			var newFactory = new BusinessObjectFactory();
			var opportunities = newFactory.Load<OrgOpportunity>(new ZQuery(OrgOpportunitySchema.PK, opportunityPks));

			var total = opportunities.Length;
			var processed = 0;
			foreach (var opportunity in opportunities)
			{
				if (Cancelled)
				{
					break;
				}

				NotifyProgress(progress, Res.GetString("32E25C03-0A0B-40E3-A8B7-0547A5F12774", "Updating Exchange Rate Date: {0} of {1}", processed, total), 100 * processed / total);
				opportunity.P8_DateForExchangeRate = Date;
			}

			if (!Cancelled)
			{
				NotifyProgress(progress, Res.GetString("F79EC571-405A-4436-8F54-7FCA0AF4626C", "Saving.."), 100);
				newFactory.Save();
			}
		}

		static void NotifyProgress(ZArchitecture.Core.Progress progress, string status, int percentComplete)
		{
			if (progress != null)
			{
				progress(status, percentComplete);
			}
		}

		#endregion

		#region Cancel

		public void Cancel()
		{
			Cancelled = true;
		}

		public bool Cancelled
		{
			get;
			private set;
		}

		#endregion

		#region Validation

		public override void ValidateDate()
		{
			base.ValidateDate();
			MandatoryValidation.CheckEntered(DateInfo);
		}

		#endregion
	}
}
