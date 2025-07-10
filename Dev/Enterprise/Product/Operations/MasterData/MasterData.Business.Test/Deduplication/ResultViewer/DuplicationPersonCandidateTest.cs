using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Test
{
	[TestedType(typeof(DuplicationPersonCandidate))]
	public class DuplicationPersonCandidateTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DuplicationPersonCandidate(null, null);
		}

		public void TestIsDissolvedChanged()
		{
			var changed = false;
			var handler = new PropertyChangedEventHandler((s, e) => changed = true);

			var candidate = new DuplicationPersonCandidate(null, null);
			candidate.IsDissolvedChanged += handler;

			candidate.IsDissolved = !candidate.IsDissolved;

			Assert(changed);
		}
	}
}
