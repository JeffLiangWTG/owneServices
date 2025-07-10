using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class OrgSupplierPartDataLoad : GlobalOrgSupplierPartDataLoad, Integration.Customs.ZA.IOrgSupplierPartDataLoad
	{
		protected override IEnumerable<string> GetFieldNames()
		{
			List<String> fieldNames = new List<string>();

			fieldNames.AddRange(base.GetFieldNames());

			fieldNames.Add("AdditionalTariffPart");
			fieldNames.Add("AdditionalTariffItemCode");

			return fieldNames;
		}

		ZAPartsDataToLoad PartsDataToLoad => partsDataToLoad ?? (partsDataToLoad = new ZAPartsDataToLoad());
		ZAPartsDataToLoad partsDataToLoad;

		protected override PartsDataToLoad GetPartsDataToLoad()
		{
			return PartsDataToLoad;
		}

		protected override void AddDataToPivot(BaseCusClassPartPivot pivot, PartsDataToLoad partsData)
		{
			base.AddDataToPivot(pivot, partsData);
			AddCountrySpecificDataToPivot((CusClassPartPivot)pivot, (ZAPartsDataToLoad)partsData);
		}

		void AddCountrySpecificDataToPivot(CusClassPartPivot pivot, ZAPartsDataToLoad partsData)
		{
			var part = partsData.AdditionalTariffPart;
			var tariffCode = partsData.AdditionalTariffItemCode;
			if (part.IsEmpty && !tariffCode.IsEmpty)
			{
				var tariff = new Universal.TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, tariffCode, ZDateTime.Now);
				if (tariff != null)
				{
					part = tariff.ZZ1_ZZI_TariffTypeCode;
				}
			}

			var cusLineTariffDetails = pivot.CusLineTariffDetails;
			if (!part.IsEmpty)
			{
				var tariffDetails = cusLineTariffDetails.OfType<CusLineTariffDetail>();
				var tariffDetail = tariffDetails.FirstOrDefault(x => x.BZ_Type == part);
				if (tariffDetail != null)
				{
					tariffDetail.BZ_Tariff = tariffCode;
				}
				else
				{
					cusLineTariffDetails.AddNew(part, tariffCode);
				}
			}
			else if (!tariffCode.IsEmpty)
			{
				cusLineTariffDetails.AddNew(ZString.Empty, tariffCode);
			}
		}

		protected override BaseCusClassification GetClassification(string partClassCode, string classType)
			=> base.GetClassification(partClassCode, classType)
				?? base.GetClassification(partClassCode, CusClassification.ClassificationType.Both);

		public class ZAPartsDataToLoad : GlobalPartsDataToLoad
		{
			public ZString AdditionalTariffPart;
			public ZString AdditionalTariffItemCode;
		}
	}
}
