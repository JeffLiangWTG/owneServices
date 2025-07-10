using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing
{
	using Enterprise.ZArchitecture.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(NZCClassificationChapter))]
	public class NZCClassificationChapterTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			NZCClassificationSection section = Factory.New<NZCClassificationSection>();
			NZCClassificationChapter chapter = Factory.New<NZCClassificationChapter>();
			chapter.Q2_Q1_Section = section.PK;
			return chapter;
		}
	}
}
