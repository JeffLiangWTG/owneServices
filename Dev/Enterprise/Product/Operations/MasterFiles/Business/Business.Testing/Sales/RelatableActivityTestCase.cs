using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(IRelatableActivity), RequireTestOnlyInFirstSubLevel = true)]
	public abstract class RelatableActivityTestCase<TActivity> : TestCaseWithFactory
		where TActivity : IRelatableActivity
	{
		public virtual void TestCodePropertyAttribute()
		{
			Assert(string.Format("Should have {0}", nameof(CodePropertyAttribute)), typeof(TActivity).IsDefined(typeof(CodePropertyAttribute), true));
		}

		public virtual void TestDescriptionPropertyAttribute()
		{
			Assert(string.Format("Should have {0}", nameof(DescriptionPropertyAttribute)), typeof(TActivity).IsDefined(typeof(DescriptionPropertyAttribute), true));
		}

		public virtual void TestRelatedPivotsDeletedOnDeletion()
		{
			var activity = GetNewActivity();
			var childActivity = GetNewActivity();
			var parentActivity = GetNewActivity();
			var childPivot = activity.RelatedChildActivityPivotCollection.AddNew();
			childPivot.ChildActivity = childActivity;
			var parentPivot = activity.RelatedParentActivityPivotCollection.AddNew();
			parentPivot.ParentActivity = parentActivity;
			activity.Delete();

			AssertEquals("Should have deleted all child related activity pivots", 0, activity.RelatedChildActivityPivotCollection.Count());
			AssertEquals("Should have deleted all parent related activity pivots", 0, activity.RelatedParentActivityPivotCollection.Count());
		}

		[TestDate(2000, 1, 1, 0, 0, 0)]
		public virtual void TestSystemLastEditTime()
		{
			var plus10TimeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			plus10TimeZoneSet.StandardZone.R2_OffsetMinutesFromUTC = 600;
			var plus10TimeZoneUnloco = Factory.NewWithValidTestData<RefUNLOCO>();
			plus10TimeZoneUnloco.RL_R3 = plus10TimeZoneSet.PK;
			plus10TimeZoneUnloco.RL_Code = "TESTX";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "TESTX";
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var activity = GetNewActivity();
				SetActivityLastEditTimeIfExists(activity, new ZDateTime(2000, 1, 1, 0, 0, 0));
				Factory.Save();

				if (!activity.SystemLastEditTimeUtc.IsEmpty)
				{
					AssertEquals("Should return last edit time in Local time", new ZDateTime(2000, 1, 1, 0, 0, 0), activity.SystemLastEditTimeUtc);
				}
				else
				{
					Assert(true);
				}
			}
		}

		[TestDate(2000, 1, 1, 0, 0, 0)]
		public virtual void TestSystemCreateTime()
		{
			var plus10TimeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			plus10TimeZoneSet.StandardZone.R2_OffsetMinutesFromUTC = 600;
			var plus10TimeZoneUnloco = Factory.NewWithValidTestData<RefUNLOCO>();
			plus10TimeZoneUnloco.RL_R3 = plus10TimeZoneSet.PK;
			plus10TimeZoneUnloco.RL_Code = "TESTX";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "TESTX";
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var activity = GetNewActivity();
				SetActivityCreateTimeIfExists(activity, new ZDateTime(2000, 1, 1, 0, 0, 0));
				Factory.Save();

				if (!activity.SystemCreateTimeUtc.IsEmpty)
				{
					AssertEquals("Should return create time in Local time", new ZDateTime(2000, 1, 1, 0, 0, 0), activity.SystemCreateTimeUtc);
				}
				else
				{
					Assert(true);
				}
			}
		}

		#region Implementation

		protected abstract TActivity GetNewActivity();

		protected void SetActivityLastEditTimeIfExists(TActivity activity, ZDateTime lastEditTime)
		{
			var bizObj = activity as BusinessObject;
			var column = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(bizObj.TableName).All[bizObj.TablePrefix + "_SystemLastEditTimeUtc"];
			if (column != null)
			{
				bizObj[column] = lastEditTime;
			}
		}

		protected void SetActivityCreateTimeIfExists(TActivity activity, ZDateTime createTime)
		{
			var bizObj = activity as BusinessObject;
			var column = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(bizObj.TableName).All[bizObj.TablePrefix + "_SystemCreateTimeUtc"];
			if (column != null)
			{
				bizObj[column] = createTime;
			}
		}

		#endregion
	}
}
