using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public abstract class RefListProvider : FindBoxListProvider
	{
		protected RefListProvider(IBusinessObjectCollection collection)
							: base(collection)
		{
			this.collection = collection;
		}

		protected RefListProvider(IBusinessObjectCollection collection, string countryCode, string postcode)
						 : this(collection)
		{
			Helper = new RefCityTownPostcodeHelper(collection, countryCode, postcode, "", "");
		}

		protected RefListProvider(IBusinessObjectCollection collection, string countryCode, string cityTown, string state)
						 : this(collection)
		{
			Helper = new RefCityTownPostcodeHelper(collection, countryCode, "", cityTown, state);
		}

		readonly IBusinessObjectCollection collection;

		ICityTownPostcodeUserInteraction CityTownPostcodeUserInteraction => (ICityTownPostcodeUserInteraction)collection;

		protected RefCityTownPostcodeHelper Helper { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		protected ZGuid GetPrimaryKeyFromCode<T>(string code, SchemaStringColumn schemaColumn, Func<string, ZGuid> getPKFromCode) where T : BusinessObject
		{
			var result = ZGuid.Invalid;

			if (!string.IsNullOrWhiteSpace(code))
			{
				if (CompleteFilter.IsEmpty)
				{
					CompleteFilter.AddToFilter(schemaColumn, Helper.CountryCode);
				}

				var bizOs = BizObjsFromCodeWithCompleteFilter(code);
				if (bizOs != null && bizOs.Any())
				{
					result = bizOs.First().PK;
					if (bizOs.Count() > 1)
					{
						result = CityTownPostcodeUserInteraction.OnSelectionNeeded(result);
					}
				}
				else
				{
					result = getPKFromCode(code);
					if (result.IsValid)
					{
						collection.Factory.Load<T>(result);
					}
				}
			}

			return result;
		}
	}
}

