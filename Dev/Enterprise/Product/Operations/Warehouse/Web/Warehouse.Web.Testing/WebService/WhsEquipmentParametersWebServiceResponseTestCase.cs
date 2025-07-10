using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsEquipmentParametersWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Test Cases

		public void TestEquipmentParameters_Null()
		{
			AssertNotNull(Response.EquipmentParameters);
		}

		public void TestEquipmentParameters()
		{
			var pickMethods = new WhsPickMethodInfoCollection
			{
				new WhsPickMethodInfo { Code = "PICK1", Description = "Manual Picking", IsDefault = true },
				new WhsPickMethodInfo { Code = "PICK2", Description = "AMR Picking", IsDefault = false },
			};
			var pickAreas = new WhsAreaInfoCollection
			{
				new WhsAreaInfo { Name = "S-A1-0010", Description = "Zone1" },
				new WhsAreaInfo { Name = "S-A2-0011", Description = "Zone2" },
			};
			var pickGroups = new WhsPickGroupInfoCollection
			{
				new WhsPickGroupInfo { PickSequence = 1, Description = "Area" },
				new WhsPickGroupInfo { PickSequence = 2, Description = "Row" },
			};
			var printers = new[]
			{
				new PrinterInfo { Name = "Floor 1" },
				new PrinterInfo { Name = "Floor 2" },
			};

			var parameters = new WhsEquipmentParameters
			{
				PickMethods = pickMethods,
				PickAreas = pickAreas,
				PickGroups = pickGroups,
				Printers = printers
			};

			Response.EquipmentParameters = parameters;
			AssertCollectionContains(pickMethods[0], Response.EquipmentParameters.PickMethods);
			AssertCollectionContains(pickAreas[0], Response.EquipmentParameters.PickAreas);
			AssertCollectionContains(pickGroups[0], Response.EquipmentParameters.PickGroups);
			AssertCollectionContains(printers[0], Response.EquipmentParameters.Printers);
		}

		public void TestEquipmentParameters_DifferentInstances()
		{
			var params1 = new WhsEquipmentParameters();
			var params2 = new WhsEquipmentParameters();
			AssertNotEquals(params1, params2);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsEquipmentParametersWebServiceResponse();
		}

		protected new WhsEquipmentParametersWebServiceResponse Response => (WhsEquipmentParametersWebServiceResponse)base.Response;

		#endregion
	}
}
