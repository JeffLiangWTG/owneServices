using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class DutyCalculationExhibitDocLineCollection : NonPersistentBusinessObjectCollection<DutyCalculationExhibitDocLine>
	{
		public DutyCalculationExhibitDocLineCollection(JobDeclarationDrawbackSupporter supporter)
			: base(supporter.Factory)
		{
			foreach (var line in (from JobComInvoiceLine line in supporter.ImportSectionInvoiceLines
								  where line.Claims.DutyClaim.ClaimedDuty > 0
								  select new DutyCalculationExhibitDocLine(line.Claims.DutyClaim)))
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
