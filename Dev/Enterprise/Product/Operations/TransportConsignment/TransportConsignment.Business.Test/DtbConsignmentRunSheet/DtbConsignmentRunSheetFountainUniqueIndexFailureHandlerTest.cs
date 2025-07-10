using System;
using System.Collections.Specialized;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentRunSheetFountainUniqueIndexFailureHandlerTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		protected override Type BizOTypeToTest
		{
			get { return typeof(DtbConsignmentRunSheet); }
		}

		protected override SchemaColumn ColumnThatUsesNumberFountain
		{
			get { return DtbConsignmentRunSheetSchema.KG_RunSheetNumber; }
		}

		protected override INumberFountainProxy NumberFountainToTest
		{
			get { return Env.NumberFountains.DtbConsignmentRunSheetID; }
		}

		protected override NameValueCollection AdditionalInsertValues
		{
			get
			{
				var collection = new NameValueCollection();
				collection.Add(DtbConsignmentRunSheetSchema.Constants.KG_GB_Branch, $"'{GlbBranch.CurrentBranch.PK.ToString()}'");
				collection.Add(DtbConsignmentRunSheetSchema.Constants.KG_SystemCreateTimeUtc, "GETUTCDATE()");
				collection.Add(DtbConsignmentRunSheetSchema.Constants.KG_SystemLastEditTimeUtc, "GETUTCDATE()");
				collection.Add(DtbConsignmentRunSheetSchema.Constants.KG_SystemCreateUser, "'~BP'");
				collection.Add(DtbConsignmentRunSheetSchema.Constants.KG_SystemLastEditUser, "'~BP'");
				return collection;
			}
		}
	}
}
