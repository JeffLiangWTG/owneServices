using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusProfileQuestionAnswerListMappingFixture
	{
		[Test]
		public void Mapping()
		{
			var mapping = RefCusProfileQuestionAnswerListMapping.Mapping;
			Assert.AreEqual("#TempRefCusProfileQuestionAnswerList", mapping.TableName);
			Assert.AreEqual(1, mapping.RelatedFKColumnNames.Count);
			Assert.True(mapping.RelatedFKColumnNames.ContainsKey("RefCusProfileQuestionAnswerListLanguages"));
			Assert.AreEqual("XAL_XQ4_QuestionAnswer", mapping.RelatedFKColumnNames["RefCusProfileQuestionAnswerListLanguages"]);
			Assert.AreEqual(1, mapping.RelatedTableNames.Count);
			Assert.True(mapping.RelatedTableNames.ContainsKey("RefCusProfileQuestionAnswerListLanguages"));
		}
	}
}
