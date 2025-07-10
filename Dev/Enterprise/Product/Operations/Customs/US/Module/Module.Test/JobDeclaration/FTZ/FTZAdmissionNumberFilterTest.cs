using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(FTZAdmissionNumberFilter))]
	sealed class FTZAdmissionNumberFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAdmissionNumberFilter()
		{
			var ftzDeclaration1 = Factory.New<JobDeclaration>();
			ftzDeclaration1.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			ftzDeclaration1.FTZAdmissionNumber = "1530A00|15|00000006";
			var ftzDeclaration2 = Factory.New<JobDeclaration>();
			ftzDeclaration2.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			ftzDeclaration2.FTZAdmissionNumber = "1240503|15|00000564";
			var ftzDeclaration3 = Factory.New<JobDeclaration>();
			ftzDeclaration3.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			var ftzDeclaration4 = Factory.New<JobDeclaration>();
			ftzDeclaration4.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			ftzDeclaration4.FTZControlNumber = "00000005";
			ftzDeclaration3.FTZYear = "14";
			ftzDeclaration4.FTZYear = "14";
			var ftzDeclaration5 = Factory.New<JobDeclaration>();
			ftzDeclaration5.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			ftzDeclaration5.FTZAdmissionNumber = "1530A0012|15|00000006";
			Factory.Save();
			var filter = new JobDeclarationFilterBusinessObject();
			var textFilter = (FTZAdmissionNumberFilter)filter[DeclarationFilterConstants.FTZAdmissionNumber];
			textFilter.ZoneID = "1530A00";
			textFilter.Year = "15";
			textFilter.ControlNumber = "00";
			textFilter.IsActive = true;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals("ftzDeclaration1 should be found", ftzDeclaration1, coll[0]);
			textFilter.ZoneID = "1240503";
			textFilter.Year = "15";
			textFilter.ControlNumber = "00000564";
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals("ftzDeclaration2 should be found", ftzDeclaration2, coll[0]);
			textFilter.ZoneID = ZString.Empty;
			textFilter.Year = ZString.Empty;
			textFilter.ControlNumber = "00000005";
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals("ftzDeclaration3 should be found", ftzDeclaration4, coll[0]);
			textFilter.ZoneID = ZString.Empty;
			textFilter.Year = ZString.Empty;
			textFilter.ControlNumber = ZString.Empty;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals("all declarations should be found", 5, coll.Count);
			textFilter.ZoneID = ZString.Empty;
			textFilter.Year = "15";
			textFilter.ControlNumber = ZString.Empty;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals("declarations 1 and 2 and 5 should be found", 3, coll.Count);
			textFilter.ZoneID = "1240503";
			textFilter.Year = ZString.Empty;
			textFilter.ControlNumber = "00000564";
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals("ftzDeclaration2 should be found", ftzDeclaration2, coll[0]);
			textFilter.ZoneID = "1530A0012";
			textFilter.Year = "15";
			textFilter.ControlNumber = "00000006";
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals("ftzDeclaration5 should be found", ftzDeclaration5, coll[0]);
		}

		protected override BusinessObject GetNewBusinessObject() => new FTZAdmissionNumberFilter(DeclarationFilterConstants.FTZAdmissionNumber);
	}
}
