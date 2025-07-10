using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class CommissionRuleLookups
	{
		#region New

		public static CommissionRuleLookups New(ICommissionRule commissionRule)
		{
			return new CommissionRuleLookups(commissionRule);
		}

		#endregion

		#region Constructor

		protected CommissionRuleLookups(ICommissionRule commissionRule)
		{
			CommissionRule = commissionRule;
		}

		protected readonly ICommissionRule CommissionRule;

		#endregion

		protected BusinessObjectFactory Factory
		{
			get { return CommissionRule.Factory; }
		}

		#region Products

		public ReadOnlyCodeDescriptionPairList Products
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.Add(AnyProductsItem);
				result.AddRange(CommissionLookups.New(Factory).GetProducts());
				result.SortByDescription();

				return result;
			}
		}

		public const string AnyProductsCode = "ANY";

		public static ICodeDescription AnyProductsItem
		{
			get { return new CodeDescriptionPair(AnyProductsCode, Res.GetString("941ba63d-4e31-4e40-a85d-6ad29e72e66b", "Any Product")); }
		}

		#endregion

		#region Services

		public ReadOnlyCodeDescriptionPairList Services
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.Add(AnyServicesItem);
				result.AddRange(CommissionLookups.New(Factory).GetServices(CommissionRule.Product));
				result.SortByDescription();

				return result;
			}
		}

		public const string AnyServicesCode = "ANY";

		public static ICodeDescription AnyServicesItem
		{
			get { return new CodeDescriptionPair(AnyServicesCode, Res.GetString("2a54bb57-da6b-48ec-8d3b-fa3f3854d6af", "Any Services")); }
		}

		#endregion

		#region SubModules

		public ReadOnlyCodeDescriptionPairList SubModules
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.Add(AnySubModulesItem);
				result.AddRange(CommissionLookups.New(Factory).GetSubModules(CommissionRule.Product, CommissionRule.Service));
				result.SortByDescription();

				return result;
			}
		}

		public const string AnySubModulesCode = "ANY";

		public static ICodeDescription AnySubModulesItem
		{
			get { return new CodeDescriptionPair(AnySubModulesCode, Res.GetString("b57c7976-0228-4f88-b659-f8642fc19379", "Any Sub-Modules")); }
		}

		#endregion

		#region Modes

		public ReadOnlyCodeDescriptionPairList Modes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(CommissionLookups.New(Factory).GetModes(CommissionRule.Product));
				result.SortByDescription();

				return result;
			}
		}

		#endregion

		public static bool ProductSupportsTradeLane(ZString product)
		{
			switch (product)
			{
				case JobInvoicingConsumerTypes.ShipmentCode:
				case JobInvoicingConsumerTypes.BrokerageCode:
				case JobInvoicingConsumerTypes.AgencyBillOfLadingCode:
				case JobInvoicingConsumerTypes.AgencyBookingCode:
					return true;
				default:
					return false;
			}
		}
	}
}