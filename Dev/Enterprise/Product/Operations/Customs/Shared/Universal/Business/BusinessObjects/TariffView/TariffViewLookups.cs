//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTariffViewLookups
//
//    This class should be used for overriding collections in AutoTariffViewLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class TariffViewLookups : AutoTariffViewLookups
	{
		public TariffViewLookups(AutoTariffView parent) : base(parent)
		{
		}

		public ICodeDescriptionPairList TariffTypeList
		{
			get
			{
				if (Parent.ZZ1_IsSystem)
				{
					return RefCusTariffTypeList.GetCachedList(Factory, Parent.ZZ1_ZZZ_NKDataGrouping);
				}
				else
				{
					return ManualTariffTypeList;
				}
			}
		}

		public ICodeDescriptionPairList ManualTariffTypeList => Factory.GetCachedValue("TariffView|ManualTariffTypeList", () =>
		{
			var result = new CodeDescriptionPairList();
			result.Add(new CodeDescriptionPair(Constants.TariffTypes.HarmonizedSystem, Res.GetString("3322735d-a531-44f8-a1b2-1fd0f00aef43", "Harmonized System Nomenclature")));
			return result;
		});

		public RefCountryCollection CountryCodeList => Parent.ZZ1_IsSystem
			? new RefCountryCollection(Factory)
			: Factory.GetCachedValue($"{nameof(TariffView)}|{nameof(CountryCodeList)}|!{nameof(Parent.ZZ1_IsSystem)}", () =>
			{
				var selfManagedTariffCountryCodes = ObjectFactory
					.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>()
					.GetSelfManagedTariffCountryCodes();
				var list = new RefCountryCollection(Factory, new ZQuery(RefCountrySchema.RN_Code, selfManagedTariffCountryCodes));
				list.ApplySort(nameof(RefCountry.Code), System.ComponentModel.ListSortDirection.Ascending);
				return list;
			});

		public CusRefTariffVersionCollection TariffVersionList => new CusRefTariffVersionCollection(Factory, Parent.ZZ1_ZZZ_NKDataGrouping);

		public CodeDescriptionPairList TaxOrFeeCodeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var parent = Parent;
				if (!parent.ZZ1_IsSystem)
				{
					var dataGrouping = parent.ZZ1_ZZZ_NKDataGrouping;
					var parentStartDate = parent.ZZ1_StartDate;
					result = Factory.GetCachedValue(string.Join("_", "TariffViewLookups.TaxOrFeeCodeList", dataGrouping, parentStartDate), () =>
					{
						var list = new CodeDescriptionPairList();

						var query = new ZQuery(RefCusTaxOrFeeSchema.ZZF_ZZZ_NKDataGrouping, dataGrouping);
						query.AddToFilter(RefCusTaxOrFeeSchema.ZZF_ZX0_NKTaxOrFeeType, Constants.RefCusTaxOrFeeTypes.VAT);
						var cusTaxOrFeeList = Factory.Load<RefCusTaxOrFee>(query).OrderBy(x => x.ZZF_Code).ToArray();
						if (cusTaxOrFeeList.Any())
						{
							var filteredList = cusTaxOrFeeList.GroupBy(e => new
							{
								e.ZZF_Code
							})
							.Select(g => new
							{
								Code = g.Key.ZZF_Code,
								Description = g.First().ZZF_Description,
								MinStartDate = g.Min(x => x.ZZF_StartDate)
							})
							.Where(x => x.MinStartDate <= parentStartDate).OrderBy(x => x.Code);
							filteredList.ForEach(x => list.AddPairIfNotExist(x.Code, x.Description));
						}
						return list;
					});
				}
				return result;
			}
		}

		protected new TariffView Parent => (TariffView)base.Parent;
	}
}
