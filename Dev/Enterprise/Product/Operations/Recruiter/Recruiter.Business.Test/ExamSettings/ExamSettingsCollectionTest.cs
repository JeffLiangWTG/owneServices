using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(ExamSettingCollection))]
	sealed class ExamSettingsCollectionTest : ActiveBusinessObjectCollectionTestCase<ExamSettingCollection>
	{
	}
}
