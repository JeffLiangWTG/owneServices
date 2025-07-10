using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ForwardingConsolMatcher : ConsolMatcher<ForwardingConsol>
	{
		public ForwardingConsolMatcher(BusinessObjectFactory factory, CommonConsolReferences references, IXmlImportLogger logger, IUniversalFreightHelper helper)
			: base(factory, references, logger, helper)
		{
		}

		protected override void BuildMatchingQueryAndMatchDelegates(CommonConsolReferences referencesParent)
		{
			if (referencesParent.IsConsolidationAdvice)
			{
				AddPossibleMatch(JobConsolSchema.JK_CoLoadMasterBill, referencesParent.CoLoadMasterBillNumber, consol => GetMatchCount(consol.JK_CoLoadMasterBill, referencesParent.CoLoadMasterBillNumber));
				AddPossibleMatch(JobConsolSchema.JK_CoLoadBookingReference, referencesParent.CoLoadBookingConfirmationReference, consol => GetMatchCount(consol.JK_CoLoadBookingReference, referencesParent.CoLoadBookingConfirmationReference));

				var hirNumbers = referencesParent.AdditionalReferences.Where(x => x.Key == CustomsReferenceNumberType.eHubInterchangeReference.HIR).ToList();
				if (referencesParent.DocumentaryPurpose == MessagePurposes.Codes.Amendment && hirNumbers.Any())
				{
					var helper = new AdditionalReferencesMatchingHelper<ForwardingConsol>(factory);
					var consolPKsMatchingAdditionalReferences = helper.GetParentsMatchingAdditionalReferences(hirNumbers, null);

					AddPossibleMatch(JobConsolSchema.PK, consolPKsMatchingAdditionalReferences, consol => GetMatchCount(consol.Numbers, CusEntryNumSchema.CE_EntryType, CusEntryNumSchema.CE_EntryNum, hirNumbers));
				}
			}
			else
			{
				base.BuildMatchingQueryAndMatchDelegates(referencesParent);
			}
		}
	}
}
