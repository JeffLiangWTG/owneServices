using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.DocumentWrappers;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public class SAD501DocumentPageWrapper : NonPersistentBusinessObject
	{
		public SAD501DocumentPageWrapper(SADLineDetailWrapper first, SADLineDetailWrapper second, SADLineDetailWrapper third, BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper> startingTotalDutiesAndFees)
		{
			FirstLine = first ?? new SADLineDetailWrapper(null, null);
			SecondLine = second ?? new SADLineDetailWrapper(null, null);
			ThirdLine = third ?? new SADLineDetailWrapper(null, null);
			this.startingTotalDutiesAndFees = startingTotalDutiesAndFees;
		}

		public SADLineDetailWrapper FirstLine { get; }
		public SADLineDetailWrapper SecondLine { get; }
		public SADLineDetailWrapper ThirdLine { get; }

		readonly BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper> startingTotalDutiesAndFees;

		public BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper> RunningTotalDutiesAndFeesOfThisPage
		{
			get
			{
				if (runningTotalDutiesAndFeesOfThisPage == null)
				{
					runningTotalDutiesAndFeesOfThisPage = new BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper>(null);
					AggregateDutiesAndFeesOfThisPage(runningTotalDutiesAndFeesOfThisPage, startingTotalDutiesAndFees);
					AggregateDutiesAndFeesOfThisPage(runningTotalDutiesAndFeesOfThisPage, FirstLine?.CalcDutiesAndFees);
					AggregateDutiesAndFeesOfThisPage(runningTotalDutiesAndFeesOfThisPage, SecondLine?.CalcDutiesAndFees);
					AggregateDutiesAndFeesOfThisPage(runningTotalDutiesAndFeesOfThisPage, ThirdLine?.CalcDutiesAndFees);

					runningTotalDutiesAndFeesOfThisPage = SADDocumentWrapperHelper.ConsolidateDutiesIfNeeded(runningTotalDutiesAndFeesOfThisPage);
				}
				return runningTotalDutiesAndFeesOfThisPage;
			}
		}

		BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper> runningTotalDutiesAndFeesOfThisPage;

		static void AggregateDutiesAndFeesOfThisPage(BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper> result, BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper> sourceCollection)
		{
			if (sourceCollection != null)
			{
				foreach (DutyFeeInformationDocWrapper item in sourceCollection)
				{
					var resultArray = result.ToArray<DutyFeeInformationDocWrapper>();
					var target = resultArray.FirstOrDefault(a => a.Code == item.Code);
					if (target != null)
					{
						target.AddAmountToValue(item.Value);
					}
					else
					{
						result.Add(new DutyFeeInformationDocWrapper(item));
					}
				}
			}
		}
	}
}
