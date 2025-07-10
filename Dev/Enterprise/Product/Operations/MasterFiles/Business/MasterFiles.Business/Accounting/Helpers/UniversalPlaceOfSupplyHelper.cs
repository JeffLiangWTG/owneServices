using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class UniversalPlaceOfSupplyHelper
	{
		public UniversalPlaceOfSupplyHelper(GlbCompany company)
		{
			PlacesOfSupply = LoadPlacesOfSupply(company);
		}

		public UniversalPlaceOfSupplyHelper(string companyCode, BusinessObjectFactory factory = null)
			: this(GetCompanyFromCode(companyCode, factory))
		{
		}

		public PlaceOfSupply GetPlaceOfSupply(string placeOfSupply, string placeOfSupplyType)
		{
			PlaceOfSupply result = null;

			if (!string.IsNullOrEmpty(placeOfSupply) && !string.IsNullOrEmpty(placeOfSupplyType))
			{
				PlacesOfSupply.TryGetValue(new Tuple<string, string>(placeOfSupply, placeOfSupplyType), out result);
			}

			return result;
		}

		#region Implementation

		Dictionary<Tuple<string, string>, PlaceOfSupply> PlacesOfSupply { get; }

		static GlbCompany GetCompanyFromCode(string companyCode, BusinessObjectFactory factory = null)
		{
			Argument.NotNullOrEmpty(companyCode, nameof(companyCode));

			factory = factory ?? new BusinessObjectFactory();
			return factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCode);
		}

		static Dictionary<Tuple<string, string>, PlaceOfSupply> LoadPlacesOfSupply(GlbCompany company)
		{
			Argument.NotNull(company, nameof(company));

			var result = new Dictionary<Tuple<string, string>, PlaceOfSupply>();

			if (PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(company))
			{
				var placeOfSupplyTypeList = PlaceOfSupplyListProvider.GetPlaceOfSupplyTypeList(company);

				foreach (CodeDescriptionPair placeOfSupply in PlaceOfSupplyListProvider.GetPlaceOfSupplyList(company))
				{
					var placeOfSupplyTypeCode = PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(company, placeOfSupply.Code);
					var placeOfSupplyTypeDescription = placeOfSupplyTypeList.GetDescriptionFromCode(placeOfSupplyTypeCode);

					result.Add(new Tuple<string, string>(placeOfSupply.Code, placeOfSupplyTypeCode),
						new PlaceOfSupply()
						{
							LocationType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair { Code = placeOfSupplyTypeCode, Description = placeOfSupplyTypeDescription },
							Location = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair5Char { Code = placeOfSupply.Code, Description = placeOfSupply.Description }
						});
				}
			}

			return result;
		}

		#endregion
	}
}
