using System;
using System.Collections.Specialized;
using CargoWise.Application;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusUnderbondNumberFountainUniqueIndexFailureHandlingTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		protected override Type BizOTypeToTest => ObjectFactory.GetType<Integration.Customs.AU.ICusUnderbond>();

		protected override SchemaColumn ColumnThatUsesNumberFountain
		{
			get { return CusUnderbondSchema.C4_SendersMessageReference; }
		}

		protected override INumberFountainProxy NumberFountainToTest
		{
			get { return Env.NumberFountains.CusUnderbondNumberFountain; }
		}

		protected override NameValueCollection AdditionalInsertValues
		{
			get
			{
				if (fInsertValues == null)
				{
					fInsertValues = base.AdditionalInsertValues;
					fInsertValues.Add(CusUnderbondSchema.C4_ParentID.Name, string.Format("'{0}'", ZGuid.NewZGuid().ToString()));
					fInsertValues.Add(CusUnderbondSchema.C4_ApplicationCode.Name, string.Format("'{0}'", CusUnderbondApplicationCodeList.Codes.AUUnderbond));
				}
				return fInsertValues;
			}
		}
		NameValueCollection fInsertValues;
	}
}
