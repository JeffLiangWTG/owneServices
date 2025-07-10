using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ReconInterestDataProviderReconDec : IReconInterestDataProviderHost
	{
		public ReconInterestDataProviderReconDec(ReconDeclaration reconDeclaration)
		{
			this.reconDeclaration = reconDeclaration;
		}

		readonly ReconDeclaration reconDeclaration;

		#region IReconInterestDataProviderHost Members

		public ZBool IsAggregate
		{
			get { return reconDeclaration.US_IsAggregate; }
		}

		public ZBool HasBeenUnderPaid
		{
			get
			{
				return OriginalPayable - ReconPayable < 0;
			}
		}

		public IEnumerable<IReconInterestDataProvider> ReconInterestData
		{
			get
			{
				foreach (ReconOriginalEntryHeader entry in reconDeclaration.OriginalEntries)
				{
					yield return new ReconInterestDataProviderReconOriginalEntry(entry);
				}
			}
		}

		#endregion

		#region IReconInterestDataProvider Members

		public ZDate OriginalPaymentDate
		{
			get
			{
				CalculateTotalIfNecessary();
				return originalPaymentDate;
			}
		}
		ZDate originalPaymentDate;

		public ZDate ReconPaymentDate
		{
			get { return reconDeclaration.ReconPaymentDate; }
		}

		public ZDecimal OriginalPayable
		{
			get
			{
				CalculateTotalIfNecessary();
				return originalPayable;
			}
		}
		ZDecimal originalPayable;

		public ZDecimal ReconPayable
		{
			get
			{
				CalculateTotalIfNecessary();
				return reconPayable;
			}
		}
		ZDecimal reconPayable;

		void CalculateTotalIfNecessary()
		{
			if (!hasBeenCalculated)
			{
				hasBeenCalculated = true;

				ZDate earliestOrgPayDate = ZDate.Empty;
				ZDate latestOrgPayDate = ZDate.Empty;

				foreach (IReconInterestDataProvider dataProvider in ReconInterestData)
				{
					if (earliestOrgPayDate.IsEmpty || earliestOrgPayDate > dataProvider.OriginalPaymentDate)
					{
						earliestOrgPayDate = dataProvider.OriginalPaymentDate;
					}

					if (latestOrgPayDate.IsEmpty || latestOrgPayDate < dataProvider.OriginalPaymentDate)
					{
						latestOrgPayDate = dataProvider.OriginalPaymentDate;
					}

					originalPayable += dataProvider.OriginalPayable;
					reconPayable += dataProvider.ReconPayable;
				}

				if (!earliestOrgPayDate.IsEmpty && !latestOrgPayDate.IsEmpty)
				{
					ZDecimal numberOfDays = (latestOrgPayDate - earliestOrgPayDate).Days / 2;
					originalPaymentDate = earliestOrgPayDate.AddDays(numberOfDays.ToZInt());
				}
			}
		}

		bool hasBeenCalculated;

		public void UpdateOrAddInterestCharge(ZDecimal amount)
		{
			reconDeclaration.US_R_AggregateInterest = amount;
		}

		#endregion
	}
}
