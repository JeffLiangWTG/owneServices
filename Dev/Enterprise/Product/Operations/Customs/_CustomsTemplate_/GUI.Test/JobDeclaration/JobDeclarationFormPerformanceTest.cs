using CargoWise.EntityFramework;
using Enterprise.Customs._CustomsTemplate_.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs._CustomsTemplate_.GUI.Testing
{
	sealed class JobDeclarationFormPerformanceTest : Customs.GUI.Testing.JobDeclarationFormPerformanceAbstractTest
	{
		protected override ZForm GetForm(BusinessObject bizO) => new JobDeclarationForm((JobDeclaration)bizO);
	}
}
