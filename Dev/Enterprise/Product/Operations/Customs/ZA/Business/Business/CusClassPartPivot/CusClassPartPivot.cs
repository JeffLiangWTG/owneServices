using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class CusClassPartPivot : AutoZACusClassPartPivot, Integration.Customs.ZA.ICusClassPartPivot
	{
		public CusClassPartPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			this.CI_NewUsed = GoodsTypeList.Codes.N;
		}

		#region Override Properties

		public override ZString CI_TariffNum
		{
			get => base.CI_TariffNum;
			set
			{
				var oldValue = CI_TariffNum;
				base.CI_TariffNum = value;
				if (!IsCopying && oldValue != CI_TariffNum)
				{
					DefaultPreference();
				}
			}
		}

		public override ZString CI_RN_NKCountryOfOrigin
		{
			get => base.CI_RN_NKCountryOfOrigin;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CI_RN_NKCountryOfOrigin))
				{
					var oldValue = CI_RN_NKCountryOfOrigin;
					base.CI_RN_NKCountryOfOrigin = value;
					if (!IsCopying && oldValue != CI_RN_NKCountryOfOrigin)
					{
						DefaultPreference();
					}
				}
			}
		}

		#region CI_PrimaryPreference

		public ZString TradeAgreementLabel => Res.GetString("BB586890-B752-490F-977F-B40300CCBE09", "Trade Agreement: {0}", TradeAgreementDescription);

		public ZString TradeAgreementDescription => TradeAgreement.Value;

		public ZString TradeAgreementCode => TradeAgreement.Key;

		public KeyValuePair<ZString, ZString> TradeAgreement => UniversalReferenceDataHelper.GetTradeGroupByPreference(UniversalTariff, AllApplicableRatesSelectionCriteria);

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.ROOTypeOrPreferenceList))]
		public override ZString CI_PrimaryPreference
		{
			get { return base.CI_PrimaryPreference; }
			set { base.CI_PrimaryPreference = value; }
		}

		#endregion

		[ReadOnlyMember(nameof(CI_CC_ReadOnly))]
		public override ZGuid CI_CC
		{
			get { return base.CI_CC; }
			set { base.CI_CC = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.VehicleFormats))]
		public override ZString CI_VehicleFormat { get => base.CI_VehicleFormat; set => base.CI_VehicleFormat = value; }

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.VehicleTypes))]
		public override ZString CI_VehicleType { get => base.CI_VehicleType; set => base.CI_VehicleType = value; }

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.GoodsTypeList))]
		public override ZString CI_NewUsed { get => base.CI_NewUsed; set => base.CI_NewUsed = value; }

		#endregion

		#region CusLineTariffDetail

		[ChildEditable(true)]
		public new ICusLineTariffDetailCollection<CusLineTariffDetail> CusLineTariffDetails => (CusLineTariffDetailCollection<CusLineTariffDetail>)base.CusLineTariffDetails;

		protected override ICusLineTariffDetailCollection<Customs.Business.CusLineTariffDetail> GetCusLineTariffDetails() => new CusLineTariffDetailCollection<CusLineTariffDetail>(this);

		protected override bool SupportsAdditionalTariffs => true;

		#endregion

		#region TariffView

		protected override ZString UniversalTariffType => UniversalReferenceConstants.CusTariffCode.Schedule1Part1;

		#endregion

		public void DefaultPreference()
		{
			if (!CI_RN_NKCountryOfOrigin.IsEmpty && !CI_TariffNum.IsEmpty && CI_ChildType == ClassificationTypeList.Codes.HTI)
			{
				PreferenceListHelper.DefaultPreference(Lookups.ROOTypeOrPreferenceList, CI_PrimaryPreferenceInfo);
			}
		}

		protected override TariffFormatter GetTariffFormatter()
		{
			return new NumberOnlyTariffFormatter();
		}

		protected override Customs.Business.CusClassPartPivotValidation GetNewValidation()
		{
			return new CusClassPartPivotValidation(this);
		}

		protected override Customs.Business.CusClassPartPivotLookups GetNewLookups()
		{
			return new CusClassPartPivotLookups(this);
		}

		public new CusClassPartPivotLookups Lookups => (CusClassPartPivotLookups)base.Lookups;
	}
}
