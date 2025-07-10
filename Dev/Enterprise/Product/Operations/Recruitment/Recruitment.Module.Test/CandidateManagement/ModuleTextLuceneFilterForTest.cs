using CargoWise.Types;
using Enterprise.Recruitment.Module.CandidateManagement;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Recruitment.Testing.Module
{
	sealed class ModuleTextLuceneFilterForTest : ModuleTextLuceneFilter
	{
		public ModuleTextLuceneFilterForTest(ZString description, GetTextQueryWithOperator queryDelegate) : base(description, queryDelegate)
		{ }

		public bool ShouldReevaluateQuery_Exposed()
			=> ShouldReevaluateQuery();
	}
}
