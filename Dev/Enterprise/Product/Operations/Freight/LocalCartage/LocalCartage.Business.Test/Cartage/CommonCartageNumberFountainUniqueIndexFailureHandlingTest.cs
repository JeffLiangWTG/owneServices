using System;
using System.Collections.Specialized;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CommonCartageNumberFountainUniqueIndexFailureHandlingTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		protected override Type BizOTypeToTest
		{
			get
			{
				return typeof(CommonCartage);
			}
		}

		protected override SchemaColumn ColumnThatUsesNumberFountain
		{
			get
			{
				return JobCartageSchema.JJ_ConsignmentID;
			}
		}

		protected override INumberFountainProxy NumberFountainToTest
		{
			get
			{
				return Env.NumberFountains.JobCartageNumber;
			}
		}

		protected override NameValueCollection AdditionalInsertValues
		{
			get
			{
				var result = base.AdditionalInsertValues;
				result.Add(JobCartageSchema.Constants.JJ_GB, string.Format("'{0}'", GlbBranch.CurrentBranch.PK));
				return result;
			}
		}
	}
}
