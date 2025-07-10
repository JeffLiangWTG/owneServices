using WTG.StaticAnalysis.Annotation;
using static Enterprise.Integration.DocumentWrappers;

namespace Enterprise.Recruiter.Business
{
	[CodeAlive("Used via reflection")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IDocRecruitmentAutoRejectionEmail : IDocRecruitmentRejectionEmail
	{
	}
}
