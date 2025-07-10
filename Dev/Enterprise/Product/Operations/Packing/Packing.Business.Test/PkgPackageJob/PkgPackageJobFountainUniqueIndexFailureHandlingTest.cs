using System;
using System.Collections.Specialized;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business.Testing
{
	public class PkgPackageJobFountainUniqueIndexFailureHandlingTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		protected override Type BizOTypeToTest => typeof(PkgPackageJob);

		protected override SchemaColumn ColumnThatUsesNumberFountain => PkgPackageJobSchema.KJ_JobID;

		protected override INumberFountainProxy NumberFountainToTest => Env.NumberFountains.PackingID;

		protected override void SetExtraPropertyValuesAfterCreatingBizO(BusinessObject testBizO)
		{
			base.SetExtraPropertyValuesAfterCreatingBizO(testBizO);
			var job = (PkgPackageJob)testBizO;

			var parent = GetParent();
			job.KJ_ParentID = parent.PK;
			job.KJ_ParentTableCode = parent.TablePrefix;
		}

		protected override NameValueCollection AdditionalInsertValues
		{
			get
			{
				var values = base.AdditionalInsertValues;

				var parent = GetParent();
				Factory.Save();

				values.Add(PkgPackageJobSchema.Constants.KJ_ParentTableCode, string.Format(Culture.Invariant, "'{0}'", parent.TablePrefix));
				values.Add(PkgPackageJobSchema.Constants.KJ_ParentID, string.Format(Culture.Invariant, "'{0}'", parent.PK));
				values.Add(PkgPackageJobSchema.Constants.KJ_IsFinalized, "0");
				return values;
			}
		}

		DummyWithPacking GetParent()
		{
			var parent = Factory.New<DummyWithPacking>();
			parent.JobNoForPackingParent = "abc";
			return parent;
		}
	}
}
