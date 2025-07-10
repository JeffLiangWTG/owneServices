using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class ScreeningPartiesProviderHelper : IScreeningPartiesProviderHelper
	{
		public ScreeningParty[] GetJobInvoicingScreeningParties(IJobHeaderParent parent, IEnumerable<JobCharge> charges)
		{
			var screeningParties = new List<ScreeningParty>();
			if (!charges.IsNullOrEmpty()
				&& parent is BusinessObject parentBizo
				&& parentBizo is Forwarding.IForwardingShipment shipment
				&& shipment is IShouldUpdateScreeningStatus)
			{
				var descriptionCreditor = Res.GetString("4c2e3d6a-137e-406d-8c2c-2f336c46c71a", "Creditor");
				var descriptionDebtor = Res.GetString("1f38b935-1ca3-4dcd-9c40-fd001157f2a6", "Debtor");
				var descriptionCreditorAndDebtor = Res.GetString("98035541-71bc-49f6-a500-0a749afcdf1f", "Creditor And Debtor");
#if NETFRAMEWORK
				var debtors = charges.Select(x => x.SellAccount).WhereNotNull().DistinctBy(o => o.PK);
				var creditors = charges.Select(x => x.CostAccount).WhereNotNull().DistinctBy(o => o.PK);
#else
				var debtors = IEnumerableExtensions.DistinctBy(charges.Select(x => x.SellAccount).WhereNotNull(), o => o.PK);
				var creditors = IEnumerableExtensions.DistinctBy(charges.Select(x => x.CostAccount).WhereNotNull(), o => o.PK);
#endif
				var creditorsAndDebtors = debtors.Where(d => creditors.Any(c => c.PK == d.PK));
				AddScreeningParty(parentBizo, screeningParties, creditorsAndDebtors, descriptionCreditorAndDebtor);
				AddScreeningParty(parentBizo, screeningParties, creditors.Except(creditorsAndDebtors), descriptionCreditor);
				AddScreeningParty(parentBizo, screeningParties, debtors.Except(creditorsAndDebtors), descriptionDebtor);
			}

			return screeningParties.ToArray();
		}

		void AddScreeningParty(BusinessObject parent, List<ScreeningParty> screeningParties, IEnumerable<OrgHeader> orgHeaders, string description)
		{
			if (orgHeaders != null && orgHeaders.Any())
			{
				foreach (var orgHeader in orgHeaders)
				{
					screeningParties.Add(new ScreeningParty(parent, description, orgHeader));
				}
			}
		}
	}
}
