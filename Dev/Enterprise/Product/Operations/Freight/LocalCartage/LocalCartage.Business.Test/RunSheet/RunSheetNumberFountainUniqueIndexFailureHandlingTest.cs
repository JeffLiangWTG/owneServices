using System;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class RunSheetNumberFountainUniqueIndexFailureHandlingTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		protected override Type BizOTypeToTest
		{
			get
			{
				return typeof(CommonWorkSheet);
			}
		}

		protected override SchemaColumn ColumnThatUsesNumberFountain
		{
			get
			{
				return JobCartageRunSheetSchema.EY_RunSheetNumber;
			}
		}

		protected override INumberFountainProxy NumberFountainToTest
		{
			get
			{
				return Env.NumberFountains.JobCartageRunSheetNumber;
			}
		}
	}
}
