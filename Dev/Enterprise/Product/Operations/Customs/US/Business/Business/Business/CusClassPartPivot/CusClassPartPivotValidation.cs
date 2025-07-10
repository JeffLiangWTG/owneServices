using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CusClassPartPivotValidation : Customs.Business.BaseCusClassPartPivotValidation
	{
		public CusClassPartPivotValidation(CusClassPartPivot parent)
			: base(parent)
		{
		}

		public new CusClassPartPivot Parent
		{
			get { return (CusClassPartPivot)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCD_ADDDepositRateDescription();
			ValidateCD_CVDDepositRateDescription();
			ValidateSupFormattedAdditionalTariff1();
			ValidateSupFormattedAdditionalTariff2();
			ValidateSupFormattedAdditionalTariff3();
			ValidateSupFormattedAdditionalTariff4();
			ValidateSupFormattedAdditionalTariff5();
		}

		protected override void CheckCI_ChildListOrder()
		{
			base.CheckCI_ChildListOrder();
			var thisPivot = Parent;
			if (!Parent.CI_CI_Parent.IsEmpty)
			{
				if ((from CusClassPartPivot brother in thisPivot.Parent.Children where brother != thisPivot && brother.CI_ChildListOrder == thisPivot.CI_ChildListOrder select brother).Any())
				{
					thisPivot.CI_ChildListOrderInfo.AddError("Value must be unique");
				}
			}
		}

		public override string DuplicateAttributeForNoneHTI
		{
			get { return Res.GetString("34105C05-5078-43DF-8486-0408BD5C1026", "The combination of Classification Type and Related Organization should be unique for HTE and SHB classifications."); }
		}

		protected override void CheckCI_TariffNum()
		{
			base.CheckCI_TariffNum();
			ValidateCI_SupplementalTariff();

			if (Parent.CI_TariffNum.IsEmpty)
			{
				MandatoryValidateTariffsAndClassTariff(Parent.CI_TariffNumInfo);
			}
			else if (Parent.IsTariffUSCTariff && Parent.ImportTariff == null || Parent.IsExportClassification && Parent.ExportTariff == null)
			{
				Parent.CI_TariffNumInfo.AddMessageError(InvalidTariff);
			}
		}

		protected override void CheckCI_SupplementalTariff()
		{
			base.CheckCI_SupplementalTariff();
			ValidateCI_TariffNum();
			ValidateCI_CC();

			if (Parent.CI_SupplementalTariff.IsEmpty)
			{
				MandatoryValidateTariffsAndClassTariff(Parent.CI_SupplementalTariffInfo);
			}
			else if (Parent.IsTariffUSCTariff && Parent.ImportSupTariff == null)
			{
				Parent.CI_SupplementalTariffInfo.AddMessageError(InvalidTariff);
			}
		}

		protected override void CheckCI_CC()
		{
			base.CheckCI_CC();
			ValidateCI_SupplementalTariff();
		}

		protected override void ValidateClassTariff(ZPropertyInfo info)
		{
			MandatoryValidateTariffsAndClassTariff(info);
		}

		void MandatoryValidateTariffsAndClassTariff(ZPropertyInfo info)
		{
			if (Parent.CI_CC.IsEmpty && Parent.CI_TariffNum.IsEmpty && Parent.CI_SupplementalTariff.IsEmpty && Parent.CI_SupAdditionalTariff1.IsEmpty && Parent.CI_SupAdditionalTariff2.IsEmpty && Parent.CI_SupAdditionalTariff3.IsEmpty && Parent.CI_SupAdditionalTariff4.IsEmpty && Parent.CI_SupAdditionalTariff5.IsEmpty)
			{
				info.AddError(Parent.Parent == null ? AtLeastOneTariffOrClassificationNeeded : AtLeastOneTariffNeeded);
			}
		}
		public void ValidateCD_ADDDepositRateDescription()
		{
			ValidateCalculatedProperty(Parent.CD_ADDDepositRateDescriptionInfo);
		}
		protected void CheckUS_ADDDepositRateDescription()
		{
			CheckDescription(Parent.CD_ADDDepositRateInd, Parent.CD_ADDDepositRateDescriptionInfo);
		}

		public void ValidateCD_CVDDepositRateDescription()
		{
			ValidateCalculatedProperty(Parent.CD_CVDDepositRateDescriptionInfo);
		}

		protected void CheckUS_CVDDepositRateDescription()
		{
			CheckDescription(Parent.CD_CVDDepositRateInd, Parent.CD_CVDDepositRateDescriptionInfo);
		}

		void CheckDescription(ZString indicator, ZPropertyInfo descriptionInfo)
		{
			var description = (ZString)descriptionInfo.Value;
			if (indicator == DepositRateIndicatorList.Codes.OverrideAdValorem)
			{
				if (!CheckDescriptionIsPercentage(description))
				{
					descriptionInfo.AddError(MustEnterInPercent);
				}
			}
			if (indicator == DepositRateIndicatorList.Codes.OverrideSpecific)
			{
				if (description.Contains("%"))
				{
					descriptionInfo.AddError(PercentIsNotAllowed);
				}
			}
		}

		bool CheckDescriptionIsPercentage(ZString description)
		{
			description = description.Trim();
			if (!description.EndsWith("%"))
			{
				return false;
			}
			var subString = description.Substring(0, description.Length - 1);
			return ZDecimal.ParseSafe(subString, new ZDecimal(-1m)) >= 0;
		}

		public const string PercentIsNotAllowed = "Percent is not allowed.";
		public const string MustEnterInPercent = "Must enter in percent format (% at the end).";
		internal static string InvalidTariff => Res.GetString("D2B859A9-09FD-4DD8-B823-3076164DA86D", "Tariff unable to be found.");
		internal static string AtLeastOneTariffOrClassificationNeeded => Res.GetString("75F36B02-FCFA-4A94-B6D1-37E886BDDD34", "One of Tariff or Prov/Prog. Tariff or Classification is Mandatory.");
		internal static string AtLeastOneTariffNeeded => Res.GetString("953004D4-5E91-4008-A97A-7F00E019392E", "One of Tariff or Prov/Prog. Tariff is Mandatory.");

		public void ValidateSupFormattedAdditionalTariff1()
		{
			ValidateCalculatedProperty(Parent.SupFormattedAdditionalTariff1Info);
		}

		protected void CheckSupFormattedAdditionalTariff1()
		{
			CheckSupFormattedAdditionalTariffs(Parent, Parent.SupFormattedAdditionalTariff1Info);
		}

		public void ValidateSupFormattedAdditionalTariff2()
		{
			ValidateCalculatedProperty(Parent.SupFormattedAdditionalTariff2Info);
		}

		protected void CheckSupFormattedAdditionalTariff2()
		{
			CheckSupFormattedAdditionalTariffs(Parent, Parent.SupFormattedAdditionalTariff2Info);
		}

		public void ValidateSupFormattedAdditionalTariff3()
		{
			ValidateCalculatedProperty(Parent.SupFormattedAdditionalTariff3Info);
		}

		protected void CheckSupFormattedAdditionalTariff3()
		{
			CheckSupFormattedAdditionalTariffs(Parent, Parent.SupFormattedAdditionalTariff3Info);
		}

		public void ValidateSupFormattedAdditionalTariff4()
		{
			ValidateCalculatedProperty(Parent.SupFormattedAdditionalTariff4Info);
		}

		protected void CheckSupFormattedAdditionalTariff4()
		{
			CheckSupFormattedAdditionalTariffs(Parent, Parent.SupFormattedAdditionalTariff4Info);
		}

		public void ValidateSupFormattedAdditionalTariff5()
		{
			ValidateCalculatedProperty(Parent.SupFormattedAdditionalTariff5Info);
		}

		protected void CheckSupFormattedAdditionalTariff5()
		{
			CheckSupFormattedAdditionalTariffs(Parent, Parent.SupFormattedAdditionalTariff5Info);
		}

		void CheckSupFormattedAdditionalTariffs(CusClassPartPivot pivot, ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.Value.IsEmpty && Parent.IsTariffUSCTariff)
			{
				var tariffNumber = ((ZString)propertyInfo.Value).KeepNumericCharacters();
				var matchedTariff = new USCTariff.Loader(Parent.Factory).LoadBestMatch(tariffNumber, Parent.EffectiveDate);
				if (matchedTariff == null)
				{
					propertyInfo.AddMessageError(InvalidTariff);
				}

				var supTariffs = new ZString[] { pivot.CI_SupplementalTariff, pivot.CI_SupAdditionalTariff1, pivot.CI_SupAdditionalTariff2, pivot.CI_SupAdditionalTariff3, pivot.CI_SupAdditionalTariff4, pivot.CI_SupAdditionalTariff5 };
				var numberOfNoneEmptyTariffs = supTariffs.Count(t => !t.IsEmpty);
				var validationError = numberOfNoneEmptyTariffs switch
				{
					1 when pivot.CI_SupplementalTariff.IsEmpty
						=> "Only 'Prov/Prog. Tariff' should be filled when there is one supplementary tariff on classification.",
					2 when pivot.CI_SupplementalTariff.IsEmpty || pivot.CI_SupAdditionalTariff1.IsEmpty
						=> "Only 'Prov/Prog. Additional Tariff 1' and 'Prov/Prog. Tariff' should be filled when there are two supplementary tariffs on classification.",
					3 when pivot.CI_SupplementalTariff.IsEmpty || pivot.CI_SupAdditionalTariff1.IsEmpty || pivot.CI_SupAdditionalTariff2.IsEmpty
						=> "Only 'Prov/Prog. Additional Tariff 1', 'Prov/Prog. Additional Tariff 2' and 'Prov/Prog. Tariff' should be filled when there are three supplementary tariffs on classification.",
					4 when pivot.CI_SupplementalTariff.IsEmpty || pivot.CI_SupAdditionalTariff1.IsEmpty || pivot.CI_SupAdditionalTariff2.IsEmpty || pivot.CI_SupAdditionalTariff3.IsEmpty
						=> "Only 'Prov/Prog. Additional Tariff 1', 'Prov/Prog. Additional Tariff 2', 'Prov/Prog. Additional Tariff 3' and 'Prov/Prog. Tariff' should be filled when there are four supplementary tariffs on classification.",
					5 when pivot.CI_SupplementalTariff.IsEmpty || pivot.CI_SupAdditionalTariff1.IsEmpty || pivot.CI_SupAdditionalTariff2.IsEmpty || pivot.CI_SupAdditionalTariff3.IsEmpty || pivot.CI_SupAdditionalTariff4.IsEmpty
						=> "Only 'Prov/Prog. Additional Tariff 1', 'Prov/Prog. Additional Tariff 2', 'Prov/Prog. Additional Tariff 3', 'Prov/Prog. Additional Tariff 4' and 'Prov/Prog. Tariff' should be filled when there are five supplementary tariffs on classification.",
					_ => ""
				};

				if (!string.IsNullOrEmpty(validationError))
				{
					propertyInfo.AddMessageError(validationError);
				}
			}
		}
	}
}
