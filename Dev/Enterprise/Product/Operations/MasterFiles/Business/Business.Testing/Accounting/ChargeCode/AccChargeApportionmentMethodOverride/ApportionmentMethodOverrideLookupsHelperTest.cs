using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using static NUnit.Framework.Assertion;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ApportionmentMethodOverrideLookupsTest : TestCaseWithFactory
	{
		public void TestConsolTypeList()
		{
			ApportionmentMethodOverrideLookupsTestBase.AssertTestConsolTypeList(
				() => ApportionmentMethodOverrideLookupsHelper.ModuleList,
				(module) => ApportionmentMethodOverrideLookupsHelper.TransportModeList(module),
				(transport, module) => ApportionmentMethodOverrideLookupsHelper.ConsolTypeList(transport, module)
			);
		}

		public void TestContainerModeList()
		{
			ApportionmentMethodOverrideLookupsTestBase.AssertTestContainerModeList(
				() => ApportionmentMethodOverrideLookupsHelper.ModuleList,
				(module) => ApportionmentMethodOverrideLookupsHelper.TransportModeList(module),
				(transport, module) => ApportionmentMethodOverrideLookupsHelper.ConsolTypeList(transport, module),
				(consolType, transport, module) => ApportionmentMethodOverrideLookupsHelper.ContainerModeList(consolType, transport, module)
			);
		}

		public void TestModuleList()
		{
			ApportionmentMethodOverrideLookupsTestBase.AssertTestModuleList(
				() => ApportionmentMethodOverrideLookupsHelper.ModuleList
			);
		}

		public void TestTransportModeList()
		{
			ApportionmentMethodOverrideLookupsTestBase.AssertTestTransportModeList(
				() => ApportionmentMethodOverrideLookupsHelper.ModuleList,
				(module) => ApportionmentMethodOverrideLookupsHelper.TransportModeList(module)
			);
		}

		public void TestDirectionList()
		{
			ApportionmentMethodOverrideLookupsTestBase.AssertTestDirectionList(
				() => ApportionmentMethodOverrideLookupsHelper.ModuleList,
				(module) => ApportionmentMethodOverrideLookupsHelper.DirectionList(module)
			);
		}

		public void TestApportionmentMethodList()
		{
			ApportionmentMethodOverrideLookupsTestBase.AssertTestApportionmentMethodList(
				() => ApportionmentMethodOverrideLookupsHelper.ModuleList,
				(module) => ApportionmentMethodOverrideLookupsHelper.ApportionmentList(module)
			);
		}
	}

	public static class ApportionmentMethodOverrideLookupsTestBase
	{
		public static void AssertTestConsolTypeList(
			Func<CodeDescriptionPairList> moduleList,
			Func<string, CodeDescriptionPairList> transportModeList,
			Func<string, string, CodeDescriptionPairList> consolTypeList)
		{
			foreach (var module in moduleList().GetAllCodes())
			{
				switch (module)
				{
					case ApportionmentMethod.AllCode:
					case ApportionmentMethodModules.Forwarding:
						AssertNormal(module);
						break;
					case ApportionmentMethodModules.TransitWarehouse:
					case ApportionmentMethodModules.TransportBooking:
						AssertConsolTypeAll(module);
						break;
					default:
						throw new NotImplementedException();
				}
			}

			void AssertNormal(string module)
			{
				foreach (CodeDescriptionPair transportMode in transportModeList(module))
				{
					switch (transportMode.Code)
					{
						case Constants.TransportModes.Air:
							var validCodesWhenIsAir = new string[] { "ALL", "DRT", "CLD", "AGT", "CHT", "COU", "OTH", "CLA", "CLM" };
							AssertContainsExactElementsInAnyOrder(validCodesWhenIsAir, consolTypeList(transportMode.Code, module).GetAllCodes());
							break;
						default:
							var validCodesWhenIsNotAir = new string[] { "ALL", "DRT", "CLD", "AGT", "CHT", "COU", "OTH" };
							AssertContainsExactElementsInAnyOrder(validCodesWhenIsNotAir, consolTypeList(transportMode.Code, module).GetAllCodes());
							break;
					}
				}
			}

			void AssertConsolTypeAll(string module)
			{
				var expectedValues = new string[] {
					ApportionmentMethod.AllCode,
				};

				foreach (CodeDescriptionPair transportMode in transportModeList(module))
				{
					AssertContainsExactElementsInAnyOrder(expectedValues, consolTypeList(transportMode.Code, module).GetAllCodes());
				}
			}
		}

		public static void AssertTestContainerModeList(
			Func<CodeDescriptionPairList> moduleList,
			Func<string, CodeDescriptionPairList> transportModeList,
			Func<string, string, CodeDescriptionPairList> consolTypeList,
			Func<string, string, string, CodeDescriptionPairList> containerModeList)
		{
			var conditions = moduleList().GetAllCodes()
				.SelectMany(module =>
				{
					var transportModes = transportModeList(module).GetAllCodes();
					var transportsConditions = transportModes
						.SelectMany(transportMode =>
						{
							return consolTypeList(transportMode, module)
								.GetAllCodes().Select(consolType => new
								{
									TransportMode = transportMode,
									ConsolType = consolType
								});
						});
					return transportsConditions.Select(subCondition => new
					{
						Module = module,
						subCondition.TransportMode,
						subCondition.ConsolType
					});
				});

			foreach (var condition in conditions)
			{
				var valueList = containerModeList(condition.ConsolType, condition.TransportMode, condition.Module).GetAllCodes();
				switch (condition.Module)
				{
					case ApportionmentMethod.AllCode:
					case ApportionmentMethodModules.TransportBooking:
					case ApportionmentMethodModules.Forwarding:
						AssertContainsExactElementsInAnyOrder(ExpectedValuesNormal(condition.ConsolType, condition.TransportMode), valueList);
						break;
					case ApportionmentMethodModules.TransitWarehouse:
						AssertContainsExactElementsInAnyOrder(ExpectedValuesContainerModeAll(), valueList);
						break;
					default:
						throw new NotImplementedException();
				}
			}

			string[] ExpectedValuesNormal(string agentType, string transportMode)
			{
				var result = new List<string>();
				switch (transportMode)
				{
					case Constants.TransportModes.Air:
						if (agentType != Constants.AgentType.Other)
						{
							return new[] {
								ApportionmentMethod.AllCode,
								Constants.ContainerModes.Loose,
								Constants.ContainerModes.ULD,
								Constants.ContainerModes.BuyersConsol,
								Constants.ContainerModes.ShippersConsol,
								Constants.ContainerModes.Other,
							};
						}
						else
						{
							return new[] {
								ApportionmentMethod.AllCode,
								Constants.ContainerModes.Other,
							};
						}
					case Constants.TransportModes.Sea:
						if (agentType != Constants.AgentType.Other)
						{
							if (agentType != Constants.AgentType.Direct)
							{
								return new[] {
									ApportionmentMethod.AllCode,
									Constants.ContainerModes.Groupage,
									Constants.ContainerModes.FCL,
									Constants.ContainerModes.LCL,
									Constants.ContainerModes.Bulk,
									Constants.ContainerModes.Liquid,
									Constants.ContainerModes.BreakBulk,
									Constants.ContainerModes.BuyersConsol,
									Constants.ContainerModes.ShippersConsol,
									Constants.ContainerModes.RollOnRollOff,
									Constants.ContainerModes.Other,
								};
							}
							else
							{
								return new[] {
									ApportionmentMethod.AllCode,
									Constants.ContainerModes.FCL,
									Constants.ContainerModes.LCL,
									Constants.ContainerModes.Bulk,
									Constants.ContainerModes.Liquid,
									Constants.ContainerModes.BreakBulk,
									Constants.ContainerModes.BuyersConsol,
									Constants.ContainerModes.ShippersConsol,
									Constants.ContainerModes.RollOnRollOff,
									Constants.ContainerModes.Other,
								};
							}
						}
						else
						{
							return new[] {
								ApportionmentMethod.AllCode,
								Constants.ContainerModes.Other,
							};
						}
					case Constants.TransportModes.Road:
						if (agentType != Constants.AgentType.Other)
						{
							return new[] {
								ApportionmentMethod.AllCode,
								Constants.ContainerModes.FCL,
								Constants.ContainerModes.FTL,
								Constants.ContainerModes.LCL,
								Constants.ContainerModes.LTL,
								Constants.ContainerModes.BuyersConsol,
								Constants.ContainerModes.ShippersConsol,
								Constants.ContainerModes.Groupage,
								Constants.ContainerModes.Other,
							};
						}
						else
						{
							return new[] {
								ApportionmentMethod.AllCode,
								Constants.ContainerModes.Other,
							};
						}
					case Constants.TransportModes.Rail:
						if (agentType != Constants.AgentType.Other)
						{
							return new[] {
								ApportionmentMethod.AllCode,
								Constants.ContainerModes.FCL,
								Constants.ContainerModes.LCL,
								Constants.ContainerModes.Bulk,
								Constants.ContainerModes.Liquid,
								Constants.ContainerModes.BreakBulk,
								Constants.ContainerModes.BuyersConsol,
								Constants.ContainerModes.ShippersConsol,
								Constants.ContainerModes.Groupage,
								Constants.ContainerModes.RollOnRollOff,
								Constants.ContainerModes.Other,
							};
						}
						else
						{
							return new[] {
								ApportionmentMethod.AllCode,
								Constants.ContainerModes.Other,
							};
						}
					case Constants.TransportModes.Courier:
						if (agentType != Constants.AgentType.Other)
						{
							return new[] {
								ApportionmentMethod.AllCode,
								Constants.ContainerModes.OnBoardCourier,
								Constants.ContainerModes.Unaccompanied,
								Constants.ContainerModes.Other,
							};
						}
						else
						{
							return new[] {
								ApportionmentMethod.AllCode,
								Constants.ContainerModes.Other,
							};
						}
					case Constants.TransportModes.Other:
						return new[] {
								ApportionmentMethod.AllCode,
							Constants.ContainerModes.Other,
						};
					case ApportionmentMethod.AllCode:
					case "":
						return new string[] {
							ApportionmentMethod.AllCode,
							Core.Constants.ContainerModes.LCL,
							Core.Constants.ContainerModes.FCL,
							Core.Constants.ContainerModes.Groupage,
							Core.Constants.ContainerModes.BuyersConsol,
							Core.Constants.ContainerModes.ShippersConsol,
							Core.Constants.ContainerModes.Loose,
							Core.Constants.ContainerModes.ULD,
							Core.Constants.ContainerModes.BreakBulk,
							Core.Constants.ContainerModes.Bulk,
							Core.Constants.ContainerModes.Liquid,
							Core.Constants.ContainerModes.RollOnRollOff,
							Core.Constants.ContainerModes.LTL,
							Core.Constants.ContainerModes.FTL,
							Core.Constants.ContainerModes.Other,
						};
					default:
						throw new NotImplementedException();
				}
			}

			string[] ExpectedValuesContainerModeAll()
			{
				return new string[] {
					ApportionmentMethod.AllCode,
				};
			}
		}

		public static void AssertTestModuleList(Func<CodeDescriptionPairList> moduleList)
		{
			var validCodes = new string[] { "ALL", "FOR", "DTB", "TRW" };
			AssertContainsExactElementsInAnyOrder(validCodes, moduleList().GetAllCodes());
		}

		public static void AssertTestTransportModeList(
			Func<CodeDescriptionPairList> moduleList,
			Func<string, CodeDescriptionPairList> transportModeList)
		{
			foreach (var module in moduleList().GetAllCodes())
			{
				switch (module)
				{
					case ApportionmentMethod.AllCode:
					case ApportionmentMethodModules.TransportBooking:
					case ApportionmentMethodModules.Forwarding:
						AssertNormal(module);
						break;
					case ApportionmentMethodModules.TransitWarehouse:
						AssertTransportModeAll(module);
						break;
					default:
						throw new NotImplementedException();
				}
			}

			void AssertNormal(string moudle)
			{
				var expectedValues = new string[] {
					Constants.TransportModes.All,
					Constants.TransportModes.Air,
					Constants.TransportModes.Sea,
					Constants.TransportModes.Road,
					Constants.TransportModes.Rail,
				};

				AssertContainsExactElementsInAnyOrder(expectedValues, transportModeList(moudle).GetAllCodes());
			}

			void AssertTransportModeAll(string moudle)
			{
				var expectedValues = new string[] {
					Constants.TransportModes.All,
				};

				AssertContainsExactElementsInAnyOrder(expectedValues, transportModeList(moudle).GetAllCodes());
			}
		}

		public static void AssertTestDirectionList(
			Func<CodeDescriptionPairList> moduleList,
			Func<string, CodeDescriptionPairList> directionList)
		{
			foreach (var module in moduleList().GetAllCodes())
			{
				switch (module)
				{
					case ApportionmentMethod.AllCode:
					case ApportionmentMethodModules.Forwarding:
						AssertNormal(module);
						break;
					case ApportionmentMethodModules.TransportBooking:
					case ApportionmentMethodModules.TransitWarehouse:
						AssertDirectionAll(module);
						break;
					default:
						throw new NotImplementedException();
				}
			}

			void AssertNormal(string module)
			{
				var expectedValues = new string[] {
					Core.Constants.FreightShipmentDirection.Code.All,
					Core.Constants.FreightShipmentDirection.Code.Export,
					Core.Constants.FreightShipmentDirection.Code.Import,
					Core.Constants.FreightShipmentDirection.Code.Domestic,
					Core.Constants.FreightShipmentDirection.Code.Other,
				};

				AssertContainsExactElementsInAnyOrder(expectedValues, directionList(module).GetAllCodes());
			}

			void AssertDirectionAll(string module)
			{
				var expectedValues = new string[] {
					Core.Constants.FreightShipmentDirection.Code.All,
				};

				AssertContainsExactElementsInAnyOrder(expectedValues, directionList(module).GetAllCodes());
			}
		}

		public static void AssertTestApportionmentMethodList(
			Func<CodeDescriptionPairList> moduleList,
			Func<string, CodeDescriptionPairList> apportionMethodList)
		{
			foreach (var module in moduleList().GetAllCodes())
			{
				switch (module)
				{
					case ApportionmentMethod.AllCode:
					case ApportionmentMethodModules.Forwarding:
					case ApportionmentMethodModules.TransportBooking:
						AssertNormal(module);
						break;
					case ApportionmentMethodModules.TransitWarehouse:
						AssertTransitWarehouse(module);
						break;
					default:
						throw new NotImplementedException();
				}
			}

			void AssertNormal(string module)
			{
				var expectedValues = new string[] { "MAN", "SHP", "REV", "CHG", "GWT", "GVT", "CNT", "OPT", "TEU", "CAP", "FSC" };

				AssertContainsExactElementsInAnyOrder(expectedValues, apportionMethodList(module).GetAllCodes());
			}

			void AssertTransitWarehouse(string module)
			{
				var expectedValues = new[] {
					AllocationMethod.Manual,
					AllocationMethod.Shipment,
					AllocationMethod.GrossWeight,
					AllocationMethod.GrossVolume,
					AllocationMethod.OuterPackTotal,
				};

				AssertContainsExactElementsInAnyOrder(expectedValues, apportionMethodList(module).GetAllCodes());
			}
		}
	}
}
