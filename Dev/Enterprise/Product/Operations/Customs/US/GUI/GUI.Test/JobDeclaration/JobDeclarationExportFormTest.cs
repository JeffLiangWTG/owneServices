using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class JobDeclarationExportFormTest : JobDeclarationFormAbstractTest
	{
		public override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Export;

		protected override Customs.Business.BaseJobDeclaration CreateDeclarationForPerformanceTest(BusinessObjectFactory factory)
		{
			var declaration = base.CreateDeclarationForPerformanceTest(factory);
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			return declaration;
		}
	}
}
