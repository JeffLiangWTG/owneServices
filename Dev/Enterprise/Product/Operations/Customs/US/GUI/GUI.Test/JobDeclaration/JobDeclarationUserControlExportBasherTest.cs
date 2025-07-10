using CargoWise.Types;
using NUnit.Framework;
using JobMessageTypeList = Enterprise.Customs.US.Business.JobMessageTypeList;

namespace Enterprise.Customs.US.GUI
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class JobDeclarationUserControlExportBasherTest : JobDeclarationUserControlBasherAbstractTest
	{
		public override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Export;
	}
}
