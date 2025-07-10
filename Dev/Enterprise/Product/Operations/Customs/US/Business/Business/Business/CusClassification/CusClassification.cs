using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public partial class CusClassification : AutoCusClassification
		, Integration.Customs.US.ICusClassification
		, IHaveAdditionalDataForBorderWise
	{
		#region Loader

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoCusClassification.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public CusClassification Load(string lookupCode, string classificationType)
			{
				ZQuery classFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, lookupCode);
				classFilter.AddToFilter(CusClassificationSchema.CC_ClassificationType, classificationType);
				classFilter.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);
				return (CusClassification)Factory.LoadTop1(GetTypeOfBusinessObjectToLoad(), classFilter);
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusClassification);
			}
		}

		#endregion

		public CusClassification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		#region Overriden

		protected override void OnTariffSet(ZString oldTariff)
		{
			base.OnTariffSet(oldTariff);
			var tariffType = IsImport ? Universal.Constants.TariffTypes.HarmonizedSystem : Universal.Constants.TariffTypes.ScheduleB;
			if (!CC_Description.IsEmpty)
			{
				var tariff = Factory.GetTariff(tariffType, oldTariff, ZDateTime.Today);
				if (tariff != null && CC_Description == tariff.Description.Left(CC_Description.Length))
				{
					CC_Description = ZString.Empty;
				}
			}

			if (CC_Description.IsEmpty)
			{
				var tariff = Factory.GetTariff(tariffType, CC_TariffNum, ZDateTime.Today);
				if (tariff != null)
				{
					CC_Description = tariff.Description.Left(CC_DescriptionInfo.MaxLength);
				}
			}
		}

		protected override Customs.Business.TariffFormatter GetTariffFormatter()
		{
			return new TariffFormatter();
		}

		protected override Customs.Business.CusClassificationLookups GetNewLookups()
		{
			return new CusClassificationLookups(this);
		}

		protected override Customs.Business.CusClassificationValidation GetNewValidation()
		{
			return new CusClassificationValidation(this);
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(CusClassification classification)
				: base(classification)
			{
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				CusClassification classification = (CusClassification)BusinessObject;

				if (classification.IsExport)
				{
					Factory.AddFetchHint(typeof(Universal.TariffView), TariffViewSchema.ZZ1_TariffCode, classification.CC_TariffNum);
				}
				else
				{
					Factory.AddFetchHint(typeof(USCTariff), USCTariffSchema.UE_Tariff, classification.CC_TariffNum);
				}
			}
		}

		public override ZString CC_TariffNum
		{
			get { return base.CC_TariffNum; }
			set
			{
				base.CC_TariffNum = value;

				if (fOGARequirementCalculator != null)
				{
					fOGARequirementCalculator.Initialise();
				}
			}
		}

		public USCTariff ImportTariff
		{
			get { return new USCTariff.Loader(Factory).LoadBestMatch(CC_TariffNum, ZDateTime.Today); }
		}

		#endregion
		#region Objects
		readonly OGARequirementCalculator fOGARequirementCalculator;
		#endregion

		#region IHaveAdditionalDataForBorderWise Members

		AdditionalDataForBorderWise IHaveAdditionalDataForBorderWise.GetAdditionalDataForBorderWise(string bindingProperty)
		{
			return new AdditionalDataForBorderWise(IsImport ? "I" : "E", ZDate.Today);
		}

		Type IHaveAdditionalDataForBorderWise.ExpectedBusinessObjectTypeForList
		{
			get { return Lookups.Tariffs.TypeOfElements; }
		}

		#endregion
	}
}
