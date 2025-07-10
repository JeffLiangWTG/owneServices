using System.Collections;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Business.TariffValidation;

namespace Enterprise.Customs.NZ.Business.MasterFiles
{
	public class CusClassPartPivotLookups : Customs.Business.CusClassPartPivotLookups
	{
		public CusClassPartPivotLookups(CusClassPartPivot parent) : base(parent)
		{
		}

		public override BusinessObjectCollection Tariffs
		{
			get { return tariffs ?? (tariffs = new NonDependentNZCClassificationCollection(Factory)); }
		}
		NonDependentNZCClassificationCollection tariffs;

		[SuppressWeaklyTypedCollectionMessage]
		public IList ConcessionList
		{
			get
			{
				return UniversalTariffHelper.GetConcessionList(Factory, Parent.CI_TariffNum, ((ITariffValidationData)Parent).DateForDutyRate, ((CusClassPartPivot)Parent).CI_RN_NKCountryOfOrigin);
			}
		}
	}
}
