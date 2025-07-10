using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CensusWarningQuery))]
	sealed class CensusWarningQueryTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var entryFiler = new EntryFiler();
			entryFiler.EntryFilerCode = "SV9";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, entryFiler);

			var bizObj = new CensusWarningQuery(Factory);
			AssertEquals("Entry Filer should be defaulted", "SV9", bizObj.EntryFilerCode);
			AssertEquals("Entry Number should be empty", ZString.Empty, bizObj.EntryNumber);
			AssertEquals("Entry Number should not be readonly", false, bizObj.EntryNumberInfo.ReadOnly);

			bizObj.DateFrom = ZDateTime.Today.AddDays(-1);
			bizObj.DateTo = ZDateTime.Today;
			AssertEquals("Date From", ZDateTime.Today.AddDays(-1), bizObj.DateFrom);
			AssertEquals("Date To", ZDateTime.Today, bizObj.DateTo);

			bizObj.DistrictPortCode = "3901";
			AssertEquals("DistrictPortCode", "3901", bizObj.DistrictPortCode);
		}

		protected override BusinessObject GetNewBusinessObject() => new CensusWarningQuery(Factory);
	}
}
