//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCommissionAgreementItemLookups
//
//    This class should be used for overriding collections in AutoOrgCommissionAgreementItemLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionAgreementItemLookups : AutoOrgCommissionAgreementItemLookups
	{
		public OrgCommissionAgreementItemLookups(AutoOrgCommissionAgreementItem parent)
			: base(parent)
		{
		}

		new OrgCommissionAgreementItem Parent
		{
			get { return (OrgCommissionAgreementItem)base.Parent; }
		}

		#region Codes

		public ICodeDescriptionPairList Codes
		{
			get
			{
				var itemPath = Parent.GetItemPath().ToArray();
				if (itemPath.Length <= 1)
				{
					return GetProducts();
				}
				else if (itemPath.Length == 2)
				{
					return GetServices(itemPath[0].Item2);
				}
				else if (itemPath.Length == 3)
				{
					return GetSubModules(itemPath[0].Item2, itemPath[1].Item2);
				}
				else
				{
					return GetProducts();
				}
			}
		}

		#endregion

		#region Products

		public const string AllProductsCode = "ALL";

		public static ICodeDescription AllProductsItem
		{
			get { return new CodeDescriptionPair(AllProductsCode, Res.GetString("7366a961-1491-4cbb-8c62-8f8e78e237a3", "All Products")); }
		}

		public ReadOnlyCodeDescriptionPairList GetProducts()
		{
			var codes = new CodeDescriptionPairList();
			codes.Add(AllProductsItem);
			codes.AddRange(CommissionLookups.New(Factory).GetProducts());
			return codes;
		}

		#endregion

		#region Services

		public const string AllServicesCode = "ALL";

		public static ICodeDescription AllServicesItem
		{
			get { return new CodeDescriptionPair(AllServicesCode, Res.GetString("e2d5b55d-4933-441d-bf0f-2f0540f2798d", "All Services")); }
		}

		public ReadOnlyCodeDescriptionPairList GetServices(ZString product)
		{
			var codes = new CodeDescriptionPairList();
			codes.Add(AllServicesItem);
			codes.AddRange(CommissionLookups.New(Factory).GetServices(product));
			return codes;
		}

		#endregion

		#region SubModules

		public const string AllSubModulesCode = "ALL";

		public static ICodeDescription AllSubModulesItem
		{
			get { return new CodeDescriptionPair(AllSubModulesCode, Res.GetString("4e5731ae-8565-4b57-9edf-33ed2b6e6b0e", "All Sub-Modules")); }
		}

		public ReadOnlyCodeDescriptionPairList GetSubModules(ZString product, ZString service)
		{
			var codes = new CodeDescriptionPairList();
			codes.Add(AllSubModulesItem);
			codes.AddRange(CommissionLookups.New(Factory).GetSubModules(product, service));
			return codes;
		}

		#endregion

		#region Modes

		public const string AllModesCode = "ALL";

		#endregion
	}
}
