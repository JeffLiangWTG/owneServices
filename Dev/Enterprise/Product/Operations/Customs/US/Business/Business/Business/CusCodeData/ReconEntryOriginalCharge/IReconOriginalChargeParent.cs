using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public interface IReconOriginalChargeParent
	{
		CodeDescriptionPairList FeeAndChargeList { get; }

		bool DefaultValueForOverridenForNewChild { get; }

		USCTariff Tariff { get; }

		void SynchroniseOnNonCommittedAdded(ReconEntryOriginalCharge charge);

		bool ShouldCalculateOrigDuty { get; }

		bool MonthlyFiling { get; }

		void UpdateChargeDetails();

		BusinessObject ParentAsBusinessObject { get; }
	}
}
