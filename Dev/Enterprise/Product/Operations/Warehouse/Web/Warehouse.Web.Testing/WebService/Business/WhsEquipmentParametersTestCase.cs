using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsEquipmentParameters))]
	public class WhsEquipmentParametersTestCase : DataObjectInfoTestCase<WhsEquipmentParameters>
	{
		#region Test Cases

		public void TestAdditionalConstructors()
		{
			var parameters = new WhsEquipmentParameters
			{
				PickMethods = new WhsPickMethodInfoCollection
				{
					new WhsPickMethodInfo { Code = "PICK1", Description = "Manual Picking", IsDefault = true },
				},
				PickAreas = new WhsAreaInfoCollection
				{
					new WhsAreaInfo { Name = "S-A1-0010", Description = "Zone1" },
				},
				PickGroups = new WhsPickGroupInfoCollection
				{
					new WhsPickGroupInfo { PickSequence = 1, Description = "Area" },
				},
				Printers = new[]
				{
					new PrinterInfo { Name = "Floor 1" },
				},
			};

			Assertion.AssertEquals("PICK1", parameters.PickMethods[0].Code);
			Assertion.AssertEquals("Manual Picking", parameters.PickMethods[0].Description);
			Assertion.AssertEquals(true, parameters.PickMethods[0].IsDefault);

			Assertion.AssertEquals("S-A1-0010", parameters.PickAreas[0].Name);
			Assertion.AssertEquals("Zone1", parameters.PickAreas[0].Description);

			Assertion.AssertEquals(1, (int)parameters.PickGroups[0].PickSequence);
			Assertion.AssertEquals("Area", parameters.PickGroups[0].Description);

			Assertion.AssertEquals("Floor 1", parameters.Printers[0].Name);
		}

		public void TestEquipmentParameters_PickMethods()
		{
			var parameters = new WhsEquipmentParameters();

			parameters.PickMethods.Add(new WhsPickMethodInfo() { Code = "UpdatedPICK", Description = "Updated Picking", IsDefault = true });

			Assertion.AssertEquals("UpdatedPICK", parameters.PickMethods[0].Code);
			Assertion.AssertEquals("Updated Picking", parameters.PickMethods[0].Description);
			Assertion.AssertEquals(true, parameters.PickMethods[0].IsDefault);
		}

		public void TestEquipmentParameters_PickAreas()
		{
			var parameters = new WhsEquipmentParameters();

			parameters.PickAreas.Add(new WhsAreaInfo { Name = "S-C0-0001", Description = "Zone5" });

			Assertion.AssertEquals("S-C0-0001", parameters.PickAreas[0].Name);
			Assertion.AssertEquals("Zone5", parameters.PickAreas[0].Description);
		}

		public void TestEquipmentParameters_PickGroups()
		{
			var parameters = new WhsEquipmentParameters();

			parameters.PickGroups.Add(new WhsPickGroupInfo() { PickSequence = 3, Description = "Column" });

			Assertion.AssertEquals(3, (int)parameters.PickGroups[0].PickSequence);
			Assertion.AssertEquals("Column", parameters.PickGroups[0].Description);
		}

		public void TestEquipmentParameters_Printers()
		{
			var parameters = new WhsEquipmentParameters();

			parameters.Printers = new[]
			{
				new PrinterInfo { Name = "Floor 4" }
			};

			Assertion.AssertEquals("Floor 4", parameters.Printers[0].Name);
		}

		#endregion

		#region Implementation

		protected new WhsEquipmentParameters Parent => (WhsEquipmentParameters)base.Parent;

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsEquipmentParameters();
		}

		#endregion
	}
}

