using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class CommissionLookupsTestCase<T> : BusinessObjectLookupsTestCase
			where T : CommissionLookups
	{
		#region Products

		public void TestProducts_MaxLength()
		{
			var lookups = GetNewLookups();

			CombineAssertions("All product codes should have a length less than or equal to " + OrgCommissionAgreementItem.Schema.CAI_CodeMaxLength, () =>
			{
				foreach (ICodeDescription product in lookups.GetProducts())
				{
					Assert(product.Code, product.Code.Length <= OrgCommissionAgreementItem.Schema.CAI_CodeMaxLength);
				}
			});
		}

		public void TestAllProducts()
		{
			var lookups = GetNewLookups();

			var actualAllProducts = lookups.AllProducts.Cast<ICodeDescription>().Select(x => x.Code);

			foreach (ICodeDescription product in lookups.GetProducts())
			{
				AssertCollectionContains("Contains " + product.Code, product.Code, actualAllProducts);
			}
		}

		#endregion

		#region Services

		public void TestServices_MaxLength()
		{
			var lookups = GetNewLookups();

			foreach (ICodeDescription product in lookups.GetProducts())
			{
				CombineAssertions("All service codes for Product:[" + product.Code + "] should have a length less than or equal to " + OrgCommissionAgreementItem.Schema.CAI_CodeMaxLength, () =>
				{
					foreach (ICodeDescription service in lookups.GetServices(product.Code))
					{
						Assert(service.Code, service.Code.Length <= OrgCommissionAgreementItem.Schema.CAI_CodeMaxLength);
					}
				});
			}

			Assert("Prevent test failure when no services", true);
		}

		public void TestAllServices()
		{
			var lookups = GetNewLookups();

			var actualAllServices = lookups.AllServices.Cast<ICodeDescription>().Select(x => x.Code);

			foreach (ICodeDescription product in lookups.GetProducts())
			{
				foreach (ICodeDescription service in lookups.GetServices(product.Code))
				{
					AssertCollectionContains("Contains " + service.Code, service.Code, actualAllServices);
				}
			}

			Assert("Prevent test failure when no services", true);
		}

		#endregion

		#region SubModules

		public void TestSubModules_MaxLength()
		{
			var lookups = GetNewLookups();

			foreach (ICodeDescription product in lookups.GetProducts())
			{
				foreach (ICodeDescription service in lookups.GetServices(product.Code))
				{
					CombineAssertions("All sub-module codes for Product:[" + product.Code + "] Service:[" + service.Code + "] should have a length less than or equal to " + OrgCommissionAgreementItem.Schema.CAI_CodeMaxLength, () =>
					{
						foreach (ICodeDescription subModule in lookups.GetSubModules(product.Code, service.Code))
						{
							Assert(subModule.Code, subModule.Code.Length <= OrgCommissionAgreementItem.Schema.CAI_CodeMaxLength);
						}
					});
				}
			}

			Assert("Prevent test failure when no sub-modules", true);
		}

		public void TestAllSubModules()
		{
			var lookups = GetNewLookups();

			var actualAllSubModules = lookups.AllSubModules.Cast<ICodeDescription>().Select(x => x.Code);

			foreach (ICodeDescription product in lookups.GetProducts())
			{
				foreach (ICodeDescription service in lookups.GetServices(product.Code))
				{
					foreach (ICodeDescription subModule in lookups.GetSubModules(product.Code, service.Code))
					{
						AssertCollectionContains("Contains " + subModule.Code, subModule.Code, actualAllSubModules);
					}
				}
			}

			Assert("Prevent test failure when no sub-modules", true);
		}

		#endregion

		#region Modes

		public void TestModes_MaxLength()
		{
			var lookups = GetNewLookups();

			CombineAssertions("All mode codes should have a length less than or equal to " + OrgCommissionAgreementItemCondition.Schema.CIC_ModeMaxLength, () =>
			{
				foreach (ICodeDescription product in lookups.GetProducts())
				{
					foreach (ICodeDescription mode in lookups.GetModes(product.Code))
					{
						Assert(mode.Code, mode.Code.Length <= OrgCommissionAgreementItemCondition.Schema.CIC_ModeMaxLength);
					}
				}
			});
		}

		public void TestModes()
		{
			var lookups = GetNewLookups();

			var modes = lookups.GetModes("SHP");
			var expected = new CodeDescriptionPairList(OLookUpEditType.TransportType);
			expected.AddPair(Core.Constants.TransportModes.All, Core.Constants.TransportModeDescriptions.All);
			expected.RemoveCode("FAS");
			expected.RemoveCode("FSA");
			expected.RemoveCode("MMD");
			AssertContainsExactElementsInAnyOrder(expected.GetAllCodes(), modes.GetAllCodes());

			modes = lookups.GetModes("BRK");
			expected = new CodeDescriptionPairList();
			expected.AddPair(Core.Constants.FreightShipmentDirection.Code.All, Core.Constants.FreightShipmentDirection.Description.All);
			expected.AddPair(Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.FreightShipmentDirection.Description.Export);
			expected.AddPair(Core.Constants.FreightShipmentDirection.Code.Import, Core.Constants.FreightShipmentDirection.Description.Import);
			AssertContainsExactElementsInAnyOrder(expected.GetAllCodes(), modes.GetAllCodes());

			modes = lookups.GetModes("AGS");
			expected = new CodeDescriptionPairList();
			expected.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
			expected.AddPair(Constants.ContainerModes.Bulk, Constants.ContainerModeDescriptions.Bulk);
			expected.AddPair(Constants.ContainerModes.Liquid, Constants.ContainerModeDescriptions.Liquid);
			expected.AddPair(Constants.ContainerModes.BreakBulk, Constants.ContainerModeDescriptions.BreakBulk);
			expected.AddPair(Constants.ContainerModes.RollOnRollOff, Constants.ContainerModeDescriptions.RollOnRollOff);
			expected.AddPair(OrgCommissionAgreementItem.AllItemCode);
			AssertContainsExactElementsInAnyOrder(expected.GetAllCodes(), modes.GetAllCodes());

			modes = lookups.GetModes("AGB");
			AssertContainsExactElementsInAnyOrder(expected.GetAllCodes(), modes.GetAllCodes());

			modes = lookups.GetModes("ZZZ");
			expected = new CodeDescriptionPairList();
			expected.AddPair(OrgCommissionAgreementItem.AllItemCode);
			AssertContainsExactElementsInAnyOrder(expected.GetAllCodes(), modes.GetAllCodes());
		}

		#endregion

		protected abstract T GetNewLookups();
	}
}
