using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ZA.Business
{
	public class CusLineTariffDetailLookups : Customs.Business.CusLineTariffDetailLookups
	{
		public CusLineTariffDetailLookups(CusLineTariffDetail parent)
			: base(parent)
		{
		}

		public new CusLineTariffDetail Parent => (CusLineTariffDetail)base.Parent;

		public override ICollection QuantityUnitList => Factory.GetCachedValue<QuantityCodeList>();

		public override ICodeDescriptionPairList TariffTypeList => GetAdditionalDutiesTariffTypeList(Factory);

		public ChildTariffViewCollection TariffCollection
		{
			get
			{
				var tariffDetailParent = Parent.Parent;
				var type = Parent.BZ_Type;
				var schedule = type.Left(1);
				var effectiveAssessmentDate = tariffDetailParent?.EffectiveAssessmentDate ?? ZDateTime.Today;
				return ChildTariffViewCollection.GetNewCollection(Factory, Core.Constants.CountryCodes.SouthAfrica, schedule == UniversalReferenceConstants.Schedule._1 ? type : schedule, effectiveAssessmentDate, GetApplicableTariffDetail(tariffDetailParent, Parent.UniversalTariffType));
			}
		}

		public static KeyValuePair<ZString, ZString>[] GetApplicableTariffDetail(ICusLineTariffDetailParent tariffDetailParent, RefCusTariffType parentTariffType)
		{
			KeyValuePair<ZString, ZString>[] result = null;
			if (tariffDetailParent != null && parentTariffType != null)
			{
				var procedure = (tariffDetailParent as JobComInvoiceLine)?.CusProcedure;
				if (procedure == null || procedure.ZZ6_Concession != UniversalReferenceConstants.Schedule._6)
				{
					var tariffType = parentTariffType.ZZI_TariffType;
					var cusLineTariffDetails = (CusLineTariffDetailCollection<CusLineTariffDetail>)tariffDetailParent.CusLineTariffDetails;

					if (!tariffDetailParent.Tariff.IsEmpty && !tariffType.IsEmpty && cusLineTariffDetails != null)
					{
						result = GetApplicableTariffDetail(parentTariffType.Factory, tariffDetailParent.Tariff, tariffType, cusLineTariffDetails).ToArray();
					}
				}
			}
			return result;
		}

		static IEnumerable<KeyValuePair<ZString, ZString>> GetApplicableTariffDetail(BusinessObjectFactory factory, ZString parentTariff, ZString parentTariffType, CusLineTariffDetailCollection<CusLineTariffDetail> cusLineTariffDetails)
		{
			yield return new KeyValuePair<ZString, ZString>(UniversalReferenceConstants.CusTariffCode.Schedule1Part1, parentTariff);

			var validTariffTypes = GetAdditionalDutiesTariffTypeList(factory);
			foreach (CusLineTariffDetail siblingTariffDetail in cusLineTariffDetails)
			{
				var type = siblingTariffDetail.BZ_Type;
				var tariff = siblingTariffDetail.BZ_Tariff;

				if (!type.IsEmpty && !tariff.IsEmpty && type < parentTariffType && validTariffTypes.ContainsCode(type)) //Why the hell are we doing: 'type < parentTariffType'?? There are no tests broken if removed, can't think of any logical reason to have it...
				{
					yield return new KeyValuePair<ZString, ZString>(type, tariff);
				}
			}
		}

		internal static AdditionalDutiesTariffTypeList GetAdditionalDutiesTariffTypeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("ZA.CusLineTariffDetailLookups.GetAdditionalDutiesTariffTypeList", () => new AdditionalDutiesTariffTypeList(factory));
		}
	}
}
