using System.Collections.Generic;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class ContainerMovementTypesTest : TestCaseWithFactory
	{
		public void TestGetEmptyReturnMovements()
		{
			AssertContainsExactElementsInAnyOrder("GetEmptyReturnMovements", new string[] { ContainerMovementTypes.Codes.YardGateIn, ContainerMovementTypes.Codes.ReturnToWharf, }, ContainerMovementTypes.EmptyReturnMovements);
		}

		public void TestGetFCLReturnToWharfMovements()
		{
			AssertContainsExactElementsInAnyOrder("GetFCLReturnToWharfMovements", new string[] { ContainerMovementTypes.Codes.WharfGateIn, ContainerMovementTypes.Codes.ReturnToWharf, }, ContainerMovementTypes.FCLReturnToWharfMovements);
		}

		public void TestGetDirection()
		{
			string[] expectedDirections = { MovementDirection.Codes.Arrival, MovementDirection.Codes.Departure, string.Empty, };
			string[] expectedArrival = new string[] { ContainerMovementTypes.Codes.WharfGateOut, ContainerMovementTypes.Codes.YardGateIn, ContainerMovementTypes.Codes.Discharge, ContainerMovementTypes.Codes.ReturnToWharf, };
			string[] expectedDeparture = new string[] { ContainerMovementTypes.Codes.WharfGateIn, ContainerMovementTypes.Codes.YardGateOut, ContainerMovementTypes.Codes.Load, ContainerMovementTypes.Codes.ReShipRequested, };
			Dictionary<string, List<string>> actual = new Dictionary<string, List<string>>();
			foreach (CodeDescriptionPair pair in new ContainerMovementTypes())
			{
				string code = pair.Code;
				string direction = ContainerMovementTypes.GetDirection(code) ?? string.Empty;
				List<string> list;
				if (!actual.TryGetValue(direction, out list))
				{
					list = new List<string>();
					actual.Add(direction, list);
				}

				list.Add(code);
			}

			AssertContainsExactElementsInAnyOrder("Direction Types", expectedDirections, actual.Keys);
			AssertContainsExactElementsInAnyOrder("Departure", expectedDeparture, actual[MovementDirection.Codes.Departure]);
			AssertContainsExactElementsInAnyOrder("Arrival", expectedArrival, actual[MovementDirection.Codes.Arrival]);
		}

		public void TestGetDepotLookupCollection()
		{
			string[] wharfMovementCodes = { ContainerMovementTypes.Codes.Load, ContainerMovementTypes.Codes.Discharge, ContainerMovementTypes.Codes.WharfGateIn, ContainerMovementTypes.Codes.WharfGateOut, ContainerMovementTypes.Codes.ReturnToWharf, };
			string[] depotMovementCodes = { ContainerMovementTypes.Codes.DepotGateIn, ContainerMovementTypes.Codes.DepotGateOut, };
			string[] yardMovementCodes = { ContainerMovementTypes.Codes.YardGateIn, ContainerMovementTypes.Codes.YardGateOut, ContainerMovementTypes.Codes.RePositionIntoYard, ContainerMovementTypes.Codes.RePositionOutOfYard, ContainerMovementTypes.Codes.ReturnedUnshipped, };
			string[] clientMovementCodes = { ContainerMovementTypes.Codes.ReShipRequested, };
			const string expectedWharf = @"
** Collection Type **
Enterprise.Freight.Agency.Business.CTODepotContainerYardConsigneeForwarderCollection

** Filters **
[Organisation Types].[Property9] = [Y]
[Secondary Type].[Property] = [Services - CTO]

** Defaults **
OH_IsMiscFreightServices = Y
OH_IsSeaCTO = Y
";
			const string expectedDepot = @"
** Collection Type **
Enterprise.Freight.Agency.Business.CTODepotContainerYardConsigneeForwarderCollection

** Filters **
[Organisation Types].[Property9] = [Y]
[Secondary Type].[Property] = [Services - Depot]

** Defaults **
OH_IsMiscFreightServices = Y
OH_IsPackDepot = Y
OH_IsUnpackDepot = Y
";
			const string expectedYard = @"
** Collection Type **
Enterprise.Freight.Agency.Business.CTODepotContainerYardConsigneeForwarderCollection

** Filters **
[Organisation Types].[Property9] = [Y]
[Secondary Type].[Property] = [Services - Container Yard]

** Defaults **
OH_IsContainerYard = Y
OH_IsMiscFreightServices = Y
";
			const string expectedClient = @"
** Collection Type **
Enterprise.Freight.Agency.Business.CTODepotContainerYardConsigneeForwarderCollection

** Filters **
[Organisation Types].[AndJoinCondition] = [N]
[Organisation Types].[OrJoinCondition] = [Y]
[Organisation Types].[Property2] = [Y]
[Organisation Types].[Property5] = [Y]

** Defaults **
OH_IsConsignee = Y
";
			ContainerMovementTypes allTypes = new ContainerMovementTypes();
			foreach (string movementType in wharfMovementCodes)
			{
				string label = string.Format("{0} - {1}", movementType, allTypes.GetDescriptionFromCode(movementType));
				OrganisationsFindBoxCollection collection = ContainerMovementTypes.GetDepotLookupCollection(Factory, movementType);
				AssertMultilineASCIIEquals(label, expectedWharf, Render(collection));
			}

			foreach (string movementType in depotMovementCodes)
			{
				string label = string.Format("{0} - {1}", movementType, allTypes.GetDescriptionFromCode(movementType));
				OrganisationsFindBoxCollection collection = ContainerMovementTypes.GetDepotLookupCollection(Factory, movementType);
				AssertMultilineASCIIEquals(label, expectedDepot, Render(collection));
			}

			foreach (string movementType in yardMovementCodes)
			{
				string label = string.Format("{0} - {1}", movementType, allTypes.GetDescriptionFromCode(movementType));
				OrganisationsFindBoxCollection collection = ContainerMovementTypes.GetDepotLookupCollection(Factory, movementType);
				AssertMultilineASCIIEquals(label, expectedYard, Render(collection));
			}

			foreach (string movementType in clientMovementCodes)
			{
				string label = string.Format("{0} - {1}", movementType, allTypes.GetDescriptionFromCode(movementType));
				OrganisationsFindBoxCollection collection = ContainerMovementTypes.GetDepotLookupCollection(Factory, movementType);
				AssertMultilineASCIIEquals(label, expectedClient, Render(collection));
			}
		}

		public void TestGetDefaultAddressType()
		{
			AddressType[] expectedTypes = new AddressType[] { AddressType.NoDefault, AddressType.DLV, AddressType.PIC, };
			string[] expectedDLV = new string[] { ContainerMovementTypes.Codes.DepotGateIn, ContainerMovementTypes.Codes.RePositionIntoYard, ContainerMovementTypes.Codes.ReturnedUnshipped, ContainerMovementTypes.Codes.WharfGateIn, ContainerMovementTypes.Codes.YardGateIn, ContainerMovementTypes.Codes.Load, ContainerMovementTypes.Codes.ReturnToWharf, };
			string[] expectedPIC = new string[] { ContainerMovementTypes.Codes.DepotGateOut, ContainerMovementTypes.Codes.RePositionOutOfYard, ContainerMovementTypes.Codes.WharfGateOut, ContainerMovementTypes.Codes.YardGateOut, ContainerMovementTypes.Codes.Discharge, };
			string[] expectedNoDefault = new string[] { ContainerMovementTypes.Codes.OffHire, ContainerMovementTypes.Codes.OnHire, ContainerMovementTypes.Codes.ReShipRequested, };
			Dictionary<AddressType, List<string>> actual = new Dictionary<AddressType, List<string>>();
			foreach (CodeDescriptionPair pair in new ContainerMovementTypes())
			{
				string code = pair.Code;
				AddressType type = ContainerMovementTypes.GetDefaultAddressType(code);
				List<string> list;
				if (!actual.TryGetValue(type, out list))
				{
					list = new List<string>();
					actual.Add(type, list);
				}

				list.Add(code);
			}

			AssertContainsExactElementsInAnyOrder("Address Types", expectedTypes, actual.Keys);
			AssertContainsExactElementsInAnyOrder("No Default", expectedNoDefault, actual[AddressType.NoDefault]);
			AssertContainsExactElementsInAnyOrder("DLV", expectedDLV, actual[AddressType.DLV]);
			AssertContainsExactElementsInAnyOrder("PIC", expectedPIC, actual[AddressType.PIC]);
		}

		public void TestCompareToDatabase()
		{
			const string sql = "select * from ContainerMoveStateMappings()";
			const string format = "Type: {0}\r\nDescription:{1}\r\nTo Location: {2}";
			List<string> expected = new List<string>();
			List<string> actual = new List<string>();
			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(sql);
			foreach (DynamicBusinessObject dbo in collection)
			{
				string movementType = new ZString(dbo["MovementType"]);
				string description = new ZString(dbo["Description"]);
				string toLocation = new ZString(dbo["ToLocation"]);
				expected.Add(string.Format(format, movementType, description, toLocation));
			}

			foreach (CodeDescriptionPair pair in new ContainerMovementTypes())
			{
				string movementType = pair.Code;
				string description = pair.Description;
				string toLocation = ContainerMovementTypes.GetToLocationCategory(movementType);
				actual.Add(string.Format(format, movementType, description, toLocation));
			}

			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestGetDetentionCalculation()
		{
			List<string> triggerImportDetention = new List<string>();
			List<string> triggerExportDetention = new List<string>();
			foreach (CodeDescriptionPair pair in new ContainerMovementTypes())
			{
				switch (ContainerMovementTypes.GetDetentionCalculation(pair.Code))
				{
					case DetentionInvoiceType.Codes.Export:
						triggerExportDetention.Add(pair.Code);
						break;
					case DetentionInvoiceType.Codes.Import:
						triggerImportDetention.Add(pair.Code);
						break;
				}
			}

			AssertContainsExactElementsInAnyOrder("types triggering export detention", ContainerMovementTypes.GetMovementCodesForDetention(DetentionInvoiceType.Codes.Export), triggerExportDetention);
			AssertContainsExactElementsInAnyOrder("types triggering import detention", ContainerMovementTypes.GetMovementCodesForDetention(DetentionInvoiceType.Codes.Import), triggerImportDetention);
		}

		public void TestGetMovementCodesForDetention()
		{
			AssertContainsExactElementsInAnyOrder("Export Detention", new string[] { ContainerMovementTypes.Codes.WharfGateIn, ContainerMovementTypes.Codes.ReturnedUnshipped, }, ContainerMovementTypes.GetMovementCodesForDetention(DetentionInvoiceType.Codes.Export));
			AssertContainsExactElementsInAnyOrder("Import Detention", new string[] { ContainerMovementTypes.Codes.YardGateIn, ContainerMovementTypes.Codes.ReShipRequested, ContainerMovementTypes.Codes.ReturnToWharf, }, ContainerMovementTypes.GetMovementCodesForDetention(DetentionInvoiceType.Codes.Import));
			AssertContainsExactElementsInAnyOrder("Error", System.Array.Empty<string>(), ContainerMovementTypes.GetMovementCodesForDetention("XXX"));
		}

		public void TestGetMovementCodesThatStartExportDetention()
		{
			AssertContainsExactElementsInAnyOrder("Starts Export Detention", new string[] { ContainerMovementTypes.Codes.YardGateOut, ContainerMovementTypes.Codes.ReShipRequested, }, ContainerMovementTypes.GetMovementsCodesThatStartExportDetention());
		}

		public void TestRequiresClientAndPrincipal()
		{
			List<string> returnedTrue = new List<string>();
			List<string> returnedFalse = new List<string>();
			foreach (CodeDescriptionPair pair in new ContainerMovementTypes())
			{
				List<string> list = ContainerMovementTypes.RequiresClientAndPrincipal(pair.Code) ? returnedTrue : returnedFalse;
				list.Add(pair.Code);
			}

			AssertContainsExactElementsInAnyOrder("types returning true", new string[] { ContainerMovementTypes.Codes.ReShipRequested, ContainerMovementTypes.Codes.ReturnedUnshipped, ContainerMovementTypes.Codes.WharfGateIn, ContainerMovementTypes.Codes.WharfGateOut, ContainerMovementTypes.Codes.YardGateIn, ContainerMovementTypes.Codes.YardGateOut, ContainerMovementTypes.Codes.ReturnToWharf, }, returnedTrue);
			AssertContainsExactElementsInAnyOrder("types returning false", new string[] { ContainerMovementTypes.Codes.DepotGateIn, ContainerMovementTypes.Codes.DepotGateOut, ContainerMovementTypes.Codes.Discharge, ContainerMovementTypes.Codes.Load, ContainerMovementTypes.Codes.OffHire, ContainerMovementTypes.Codes.OnHire, ContainerMovementTypes.Codes.RePositionIntoYard, ContainerMovementTypes.Codes.RePositionOutOfYard, }, returnedFalse);
		}

		public void TestGetMovementCodesLeadingToLocation()
		{
			CombineAssertions(delegate
			{
				Dictionary<string, List<string>> lookup = new Dictionary<string, List<string>>();
				foreach (CodeDescriptionPair pair in new ContainerMovementTypes())
				{
					string lc = ContainerMovementTypes.GetToLocationCategory(pair.Code);
					if (string.IsNullOrEmpty(lc))
					{
						continue;
					}

					List<string> list;
					if (!lookup.TryGetValue(lc, out list))
					{
						list = new List<string>();
						lookup.Add(lc, list);
					}

					list.Add(pair.Code);
				}

				foreach (KeyValuePair<string, List<string>> group in lookup)
				{
					AssertContainsExactElementsInAnyOrder(group.Key, group.Value, ContainerMovementTypes.GetMovementCodesLeadingToLocation(group.Key));
				}
			});
		}

		public void TestGetMovementsCodesThatRestrictDetentionDaysDefaulting()
		{
			var expected = new[] { ContainerMovementTypes.Codes.Discharge, ContainerMovementTypes.Codes.Load, ContainerMovementTypes.Codes.ReturnedUnshipped, ContainerMovementTypes.Codes.ReturnToWharf, ContainerMovementTypes.Codes.WharfGateIn, ContainerMovementTypes.Codes.WharfGateOut, ContainerMovementTypes.Codes.YardGateIn };
			var actual = ContainerMovementTypes.MovementsCodesThatRestrictDetentionDaysDefaulting;
			AssertContainsExactElementsInAnyOrder("Movement codes to check before the start detention date when defaulting", expected, actual);
		}

		#region Implementation
		static string Render(OrganisationsFindBoxCollection collection)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine();
			builder.AppendLine("** Collection Type **");
			builder.AppendLine(collection == null ? "<null>" : collection.GetType().FullName);
			builder.AppendLine();
			builder.AppendLine("** Filters **");
			List<string> lines = new List<string>();
			foreach (FilterBusinessObjectDefault def in collection.FilterBusinessObjectDefaults)
			{
				lines.Add(string.Format("[{0}].[{1}] = [{2}]", def.FilterName, def.PropertyName, def.Value));
			}

			lines.Sort();
			foreach (string line in lines)
			{
				builder.AppendLine(line);
			}

			lines.Clear();
			builder.AppendLine();
			builder.AppendLine("** Defaults **");
			foreach (OrgFieldDefault def in collection.DefaultsForNewChild)
			{
				lines.Add(string.Format("{0} = {1}", def.FieldName, def.Value));
			}

			lines.Sort();
			foreach (string line in lines)
			{
				builder.AppendLine(line);
			}

			return builder.ToString();
		}
		#endregion
	}
}
