using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessTaskRequiredSkillCollection))]
	sealed class ProcessTaskRequiredSkillCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ProcessTask task = Factory.New<ProcessTask>();
			return new ProcessTaskRequiredSkillCollection(task);
		}

		public void TestAddAspect()
		{
			var guid1 = Guid.NewGuid();
			var guid2 = Guid.NewGuid();
			var guid3 = Guid.NewGuid();
			var task = Factory.New<ProcessTask>();
			AssertPivots();
			task.SkillsPivots.AddAspect(guid1);
			AssertPivots(guid1);
			task.SkillsPivots.AddAspect(guid2);
			AssertPivots(guid1, guid2);
			task.SkillsPivots.AddAspect(guid2);
			AssertPivots(guid1, guid2);

			void AssertPivots(params Guid[] expectedAspects)
			{
				AssertSequencesEqual(expectedAspects, task.SkillsPivots.Cast<ProcessTaskRequiredSkill>().Select(p => p.P9S_Aspect.ToGuid()));
			}
		}
	}
}
