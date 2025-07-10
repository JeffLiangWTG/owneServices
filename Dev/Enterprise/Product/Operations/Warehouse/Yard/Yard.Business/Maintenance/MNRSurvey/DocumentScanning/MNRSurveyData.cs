using System;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(MNRSurveyData),
	Enterprise.Core.Constants.DocManagerCodes.MNRSurvey)]

namespace Enterprise.Warehouse.Yard.Business
{
	public class MNRSurveyData : CYDAssemblyData<MNRSurvey>
	{
		protected override Type CollectionType => typeof(MNRSurveyCollection);

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("MNRSurveyData|HumanReadableName", "Survey");
	}
}
