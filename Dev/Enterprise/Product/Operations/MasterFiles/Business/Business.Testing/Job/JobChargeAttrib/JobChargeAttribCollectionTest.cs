using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobChargeAttribCollection))]
	sealed class JobChargeAttribCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetValueFromName()
		{
			var collection = (JobChargeAttribCollection)GetCollectionToTest();

			AddJobChargeAttrib(collection, JobChargeAttribTypeList.Codes.Product, "PRO");
			AddJobChargeAttrib(collection, JobChargeAttribTypeList.Codes.Commodity, "COM");
			AddJobChargeAttrib(collection, JobChargeAttribTypeList.Codes.LocationDesc, "LOC");

			AssertEquals("PRO", collection.GetValueFromName(JobChargeAttribTypeList.Codes.Product));
			AssertEquals("COM", collection.GetValueFromName(JobChargeAttribTypeList.Codes.Commodity));
			AssertEquals("LOC", collection.GetValueFromName(JobChargeAttribTypeList.Codes.LocationDesc));
			AssertEquals("", collection.GetValueFromName(JobChargeAttribTypeList.Codes.Attrib1));
		}

		public void TestGetAllValuesFromName()
		{
			var collection = (JobChargeAttribCollection)GetCollectionToTest();

			AddJobChargeAttrib(collection, JobChargeAttribTypeList.Codes.CalculatorDescription, "CLC1");
			AddJobChargeAttrib(collection, JobChargeAttribTypeList.Codes.CalculatorDescription, "CLC2");
			AddJobChargeAttrib(collection, JobChargeAttribTypeList.Codes.CalculatorDescription, "CLC3");

			AssertContainsExactElementsInAnyOrder(new[] { "CLC1", "CLC2", "CLC3" }, collection.GetAllValuesFromName(JobChargeAttribTypeList.Codes.CalculatorDescription));
		}

		void AddJobChargeAttrib(JobChargeAttribCollection collection, string name, string value)
		{
			var attrib = collection.AddNew();
			attrib.EC_Name = name;
			attrib.EC_Value = value;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new JobChargeAttribCollection(Factory.New<JobCharge>());
		}
	}
}
