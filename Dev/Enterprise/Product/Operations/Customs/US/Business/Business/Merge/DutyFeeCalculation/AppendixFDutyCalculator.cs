using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class AppendixFDutyCalculator : AppendixFCalculator
	{
		public AppendixFDutyCalculator(IDutyData dutyData, BusinessObjectFactory factory)
			: base(dutyData, factory)
		{
		}

		public static AppendixFDutyCalculator NewWithCombinedCustomsValue(IDutyData dutyData, IDutyData parentDutyData)
		{
			var combinedDutyData = new DutyDataProxy(dutyData);
			if (parentDutyData != null)
			{
				combinedDutyData.CustomsValue += parentDutyData.CustomsValue;
			}

			return new AppendixFDutyCalculator(combinedDutyData, dutyData.Factory);
		}

		#region Implementation

		protected override IDutyResult Calculate()
		{
			IDutyResult result;
			if (dutyData.IsSetVLine)
			{
				result = new DutyResult();
			}
			else
			{
				var computationCode = dutyData.ImportTariff?.UE_DutyComputationCode ?? ZString.Empty;
				IRateWrapper wrapper = DutyRateWrapper.GetWrapper(dutyData);
				result = Calculate(wrapper, computationCode);
			}
			return result;
		}

		#endregion
	}
}
