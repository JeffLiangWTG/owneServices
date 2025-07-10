using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DummySuperRelatableActivity : DummyRelatableActivity, ISuperRelatableActivity
	{
		public DummySuperRelatableActivity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ISubRelatableActivity GetMatchingSubActivity(IRelatableActivity activity)
		{
			ISubRelatableActivity matchingSubActivity;
			if (MatchingSubActivityLookup.TryGetValue(activity, out matchingSubActivity))
			{
				return matchingSubActivity;
			}
			return null;
		}

		public ISubRelatableActivity AddNewSubRelatableActivity()
		{
			var result = Factory.New<DummySubRelatableActivity>();
			result.SuperActivity = this;
			SubRelatabeActivities.Add(result);

			return result;
		}
		readonly List<ISubRelatableActivity> SubRelatabeActivities = new List<ISubRelatableActivity>();

		public void StubMatchingSubActivityForTesting(IRelatableActivity activity, ISubRelatableActivity matchingActivity)
		{
			MatchingSubActivityLookup[activity] = matchingActivity;
		}
		readonly Dictionary<IRelatableActivity, ISubRelatableActivity> MatchingSubActivityLookup = new Dictionary<IRelatableActivity, ISubRelatableActivity>();

		public IRelatableActivity CurrentChildActivity
		{
			get;
			set;
		}
	}
}
