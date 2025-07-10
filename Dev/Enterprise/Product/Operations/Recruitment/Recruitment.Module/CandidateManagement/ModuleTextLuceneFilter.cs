using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Recruitment.Module.CandidateManagement
{
	public class ModuleTextLuceneFilter : ModuleTextFilter
	{
		public ModuleTextLuceneFilter(ZString description, GetTextQueryWithOperator queryDelegate) : base(description, queryDelegate)
		{ }

		internal bool ShouldReevaluateQueryForTest()
			=> ShouldReevaluateQuery();

		protected override bool ShouldReevaluateQuery()
			=> false;
	}
}
