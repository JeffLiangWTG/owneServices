using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbDepartment))]
	sealed class GlbDepartmentTest : EnterpriseBusinessObjectTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[StressTest]
		public override void TestBizObjectFields()
		{
			var testDepartment = Factory.Load<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_SystemCode, false));
			if (testDepartment.Length > 0)
			{
				testDepartment.DeleteAll();
				Factory.Save();
			}
			base.TestBizObjectFields();
		}

		public void TestModuleDirectionModeHasList()
		{
			// These are used to StmNote to determine the correct Context to map to. If you really, really want to remove the list please ensure contexts can still be found
			AssertHasCustomAttribute<ListAttribute>(typeof(GlbDepartment), "GE_Activity", true, _ => true);
			AssertHasCustomAttribute<ListAttribute>(typeof(GlbDepartment), "GE_Direction", true, _ => true);
			AssertHasCustomAttribute<ListAttribute>(typeof(GlbDepartment), "GE_Mode", true, _ => true);
		}

		public void TestGE_DescMultilingual()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			using (var mockData = Res.UseMockData())
			{
				var key = department.GE_DescInfo.CustomizableDataResourceStrings.Source.GetKey(null, "Freight Services");
				mockData.Put(key, new ResourceStringData(key, "货运服务"));
				mockData.Put(department.GE_DescInfo.CustomizableDataResourceStrings.Source.GetKey(null, "Air Freight Services"), new ResourceStringData(key, "空运服务"));

				department.GE_Desc = "Freight Services";
				AssertEquals("货运服务", department.GE_DescMultilingual);

				department.GE_Desc = "Air Freight Services";
				AssertEquals("空运服务", department.GE_DescMultilingual);
			}
		}

		public void TestIDepartmentImplementation()
		{
			var glbDepartment = Factory.New<GlbDepartment>();
			glbDepartment.GE_Code = "WOW";
			glbDepartment.GE_Desc = "WILD ON WOW";

			IDepartment department = glbDepartment;
			AssertEquals("WOW", department.Code);
			AssertEquals("WILD ON WOW", department.Description);
			AssertEquals(glbDepartment.PK, department.PK);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return testFactory.New(typeof(GlbDepartment));
		}

		public void TestChargeCodesAreOrderedProperly()
		{
			// Step 1: create data
			GlbDeptCharges[] charges = new GlbDeptCharges[8];
			AccChargeCode[] chargeCodes = Factory.Load<AccChargeCode>(new ZQuery());
			Dictionary<ZGuid, int> chargeCodeMapping = new Dictionary<ZGuid, int>();

			for (int i = 0; i < charges.Length; i++)
			{
				charges[i] = testGlbDepartment.DeptCharges.AddNew();
				charges[i].GD_SequenceNumber = (byte)(i + 1);
				charges[i].GD_GC = GlbCompany.CurrentCompany.PK;
				charges[i].GD_AC = chargeCodes[i].PK;
				chargeCodeMapping.Add(charges[i].GD_AC, charges[i].GD_SequenceNumber);
			}

			testFactory.Save();

			// Step 2: reload data in new factory and assert it was loaded correctly...then flip around the sequence numbers and assert they are flipped properly
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			GlbDepartment testDepartment2 = factory2.Load<GlbDepartment>(testGlbDepartment.PK);
			AssertEquals("PRE: There should be 8 charges loaded for same object in new factory", 8, testDepartment2.DeptCharges.Count);
			for (int i = 0; i < testDepartment2.DeptCharges.Count; i++)
			{
				AssertEquals(String.Format("PRE: Expecting element {0} in the array to have sequence number {1}", i, i + 1), i + 1, testDepartment2.DeptCharges[i].GD_SequenceNumber);
				AssertEquals(String.Format("PRE: Expecting element {0}'s Sequence in the array to match the chargeCode Mapper's sequence", i), chargeCodeMapping[testDepartment2.DeptCharges[i].GD_AC], testDepartment2.DeptCharges[i].GD_SequenceNumber);
			}
			int middle = testDepartment2.DeptCharges.Count / 2;
			int opposed = testDepartment2.DeptCharges.Count - 1;

			var deptCharges2 = testDepartment2.DeptCharges.ToArray();
			// reverse the sequence order by swapping opposed objects
			for (int i = 0; i < middle; i++)
			{
				deptCharges2[i].GD_SequenceNumber = (byte)(testDepartment2.DeptCharges.Count - i);
				deptCharges2[opposed - i].GD_SequenceNumber = (byte)(i + 1);
			}

			testFactory.Save();

			for (int i = 0; i < testDepartment2.DeptCharges.Count - 1; i++)
			{
				int expectedResult = testDepartment2.DeptCharges.Count - i;
				AssertEquals(String.Format("Expecting element {0}'s Sequence in the array to match the chargeCode Mapper's original sequence", i), chargeCodeMapping[testDepartment2.DeptCharges[i].GD_AC], testDepartment2.DeptCharges[expectedResult - 1].GD_SequenceNumber);
			}

			// Step 3: reload flipped data in new factory and assert data is still flipped
			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			GlbDepartment testDepartment3 = factory3.Load<GlbDepartment>(testGlbDepartment.PK);
			AssertEquals("PRE: There should be 8 charges loaded for same object in new factory", 8, testDepartment3.DeptCharges.Count);
			for (byte i = 0; i < testDepartment3.DeptCharges.Count; i++)
			{
				AssertEquals(String.Format("Expecting element {0} in the array to have sequence number {1}", i, i + 1), i + 1, testDepartment3.DeptCharges[i].GD_SequenceNumber);
				AssertEquals(String.Format("Expecting element {0}'s Sequence in the array to match the chargeCode Mapper's original sequence", i), chargeCodeMapping[testDepartment2.DeptCharges[i].GD_AC], testDepartment2.DeptCharges[testDepartment3.DeptCharges.Count - i - 1].GD_SequenceNumber);
			}
		}

		[ExpectNoExceptions()]
		public void TestSetAttributes()
		{
			GlbDepartment parent = Factory.New<GlbDepartment>();
			parent.GE_Code = "12";

			GlbDepartment child = Factory.New<GlbDepartment>();
			child.GE_GE = parent.PK;
		}

		public void TestCurrentDepartment()
		{
			AssertEquals(Env.CurrentDepartment.PK, GlbDepartment.CurrentDepartment.PK.ToGuid());
		}

		public void TestTransportMode()
		{
			GlbDepartment department = Factory.New<GlbDepartment>();
			department.GE_Air = ZBool.True;
			AssertEquals(Enterprise.Core.Constants.TransportModes.Air, department.TransportMode);
			department.GE_Air = ZBool.False;
			department.GE_Sea = ZBool.True;
			AssertEquals(Enterprise.Core.Constants.TransportModes.Sea, department.TransportMode);
			department.GE_Sea = ZBool.False;
			department.GE_Road = ZBool.True;
			AssertEquals(Enterprise.Core.Constants.TransportModes.Road, department.TransportMode);
		}

		public void TestMode()
		{
			GlbDepartment department1 = Factory.New<GlbDepartment>();
			department1.GE_Code = "FEL";
			AssertEquals("Mode is a transport mode for forwarding", "Rail", department1.GE_Mode);

			GlbDepartment department2 = Factory.New<GlbDepartment>();
			department2.GE_Code = "SEL";
			AssertEquals("Mode is a container mode for ships agency", "Liquid Bulk", department2.GE_Mode);
		}

		public void TestLoadAndCreateWorkTime()
		{
			GlbDepartment tDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			AssertNotNull(tDepartment.WorkTimes.ParentID);
			AssertEquals(tDepartment.PK, tDepartment.WorkTimes.ParentID);
			AssertEquals("GE", tDepartment.WorkTimes.ParentTableCode);

			tDepartment.WorkTimes.MondayWorkingHours = "**              ***";
			tDepartment.WorkTimes.TuesdayWorkingHours = "***********************************************";
			tDepartment.WorkTimes.WednesdayWorkingHours = "      *****     ";
			tDepartment.WorkTimes.ThursdayWorkingHours = "      *";
			tDepartment.WorkTimes.FridayWorkingHours = "           *********";
			tDepartment.WorkTimes.SaturdayWorkingHours = "           ";
			tDepartment.WorkTimes.SundayWorkingHours = "           ";
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			GlbDepartment tDepartment2 = factory2.Load<GlbDepartment>(tDepartment.PK);
			factory2.Save();
			AssertNotNull(tDepartment2.WorkTimes.ParentID);
			AssertEquals(tDepartment2.WorkTimes.ParentID, tDepartment2.PK);
			AssertEquals(tDepartment2.WorkTimes.ParentTableCode, "GE");
			AssertEquals(tDepartment2.WorkTimes.MondayWorkingHours, "**              ***");
			AssertEquals(tDepartment2.WorkTimes.TuesdayWorkingHours, "***********************************************");
			AssertEquals(tDepartment2.WorkTimes.WednesdayWorkingHours, "      *****");
			AssertEquals(tDepartment2.WorkTimes.ThursdayWorkingHours, "      *");
			AssertEquals(tDepartment2.WorkTimes.FridayWorkingHours, "           *********");
			AssertEquals(tDepartment2.WorkTimes.SaturdayWorkingHours, "");
			AssertEquals(tDepartment2.WorkTimes.SundayWorkingHours, "");

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			GlbDepartment childDepartment = factory3.NewWithValidTestData<GlbDepartment>();
			childDepartment.GE_Code = "XXX";
			childDepartment.GE_GE = tDepartment2.PK;
			factory3.Save();
			AssertNotNull(childDepartment.WorkTimes.ParentID);
			AssertEquals(childDepartment.WorkTimes.ParentID, childDepartment.PK);
			AssertEquals(childDepartment.WorkTimes.ParentTableCode, "GE");
			AssertNotNull(childDepartment.ParentDepartment.FindByPK(tDepartment2.PK).PK);
			AssertEquals(childDepartment.WorkTimes.MondayWorkingHours, "**              ***");
			AssertEquals(childDepartment.WorkTimes.TuesdayWorkingHours, "***********************************************");
			AssertEquals(childDepartment.WorkTimes.WednesdayWorkingHours, "      *****");
			AssertEquals(childDepartment.WorkTimes.ThursdayWorkingHours, "      *");
			AssertEquals(childDepartment.WorkTimes.FridayWorkingHours, "           *********");
			AssertEquals(childDepartment.WorkTimes.SaturdayWorkingHours, "");
			AssertEquals(childDepartment.WorkTimes.SundayWorkingHours, "");
		}

		public void TestDeleteLastActiveDepartment()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(typeof(GlbDepartment)));

			GlbDepartment[] depts = Factory.Load<GlbDepartment>(new ZQuery());
			depts.ToList().ForEach(dept => dept.GE_IsActive = false);

			GlbDepartment dept1 = Factory.NewWithValidTestData<GlbDepartment>();
			dept1.GE_IsActive = true;
			dept1.GE_Code = "UT1";
			dept1.GE_Desc = "Test1";
			dept1.GE_SystemCode = false;

			GlbDepartment dept2 = Factory.NewWithValidTestData<GlbDepartment>();
			dept2.GE_IsActive = true;
			dept2.GE_Code = "UT2";
			dept2.GE_Desc = "Test2";
			dept2.GE_SystemCode = false;

			Factory.Save();

			AssertEquals("No Error expected", true, dept1.CanDelete);
			dept1.Delete();

			AssertEquals("Error expected - Last active branch cannot be deleted", false, dept2.CanDelete);
		}

		public void TestDeleteDepartmentDeletesUnusedStoredDefaults()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var data1ForDeparment = Factory.New<StmData>();
			data1ForDeparment.SD_DepartmentGuid = department.PK;
			var data2ForDepartment = Factory.New<StmData>();
			data2ForDepartment.SD_DepartmentGuid = department.PK;
			data2ForDepartment.SD_Owner = ZGuid.NewZGuid();

			var otherDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			var data1ForOtherDepartment = Factory.New<StmData>();
			data1ForOtherDepartment.SD_DepartmentGuid = otherDepartment.PK;
			var data2ForOtherDepartment = Factory.New<StmData>();
			data2ForOtherDepartment.SD_DepartmentGuid = otherDepartment.PK;
			data2ForOtherDepartment.SD_Owner = ZGuid.NewZGuid();

			Factory.Save();

			department.Delete();

			AssertNull(Factory.Load<StmData>(data1ForDeparment.PK));
			AssertNull(Factory.Load<StmData>(data2ForDepartment.PK));
			AssertNotNull(Factory.Load<StmData>(data1ForOtherDepartment.PK));
			AssertNotNull(Factory.Load<StmData>(data2ForOtherDepartment.PK));
		}

		#region ICancellable

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(typeof(GlbDepartment)));
		}

		#endregion

		#region Implementation

		BusinessObjectFactory testFactory;
		GlbDepartment testGlbDepartment;

		protected override void SetUp()
		{
			base.SetUp();
			testFactory = new BusinessObjectFactory();
			testGlbDepartment = testFactory.New<GlbDepartment>();
		}

		#endregion
	}
}
