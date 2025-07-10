using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class CalculationExhibitDocLineCollection : NonPersistentBusinessObjectCollection<CalculationExhibitDocLine>
	{
		public CalculationExhibitDocLineCollection(JobDeclarationDrawbackSupporter supporter, Func<JobComInvoiceLine, DrawbackClaims.IndividualClaim> getClaim)
		{
			foreach (var line in (from JobComInvoiceLine line in supporter.ImportSectionInvoiceLines
								  where getClaim(line).ClaimedDuty > 0
								  select new CalculationExhibitDocLine(getClaim(line))))
			{
				Add(line);
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}
	}
}
