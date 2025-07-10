using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class ACEACCaseQueryValidation : AutoACEACCaseQueryValidation
	{
		public ACEACCaseQueryValidation(AutoACEACCaseQuery parent)
			: base(parent) { }

		protected override void CheckUS_CompanyCaseStatus()
		{
			base.CheckUS_CompanyCaseStatus();
			if (Parent.CaseNumbers.Count == 0)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_CompanyCaseStatusInfo, Parent.Lookups.CompanyCaseStatusList);
			}

			if (!Parent.US_CompanyCaseStatus.IsEmpty)
			{
				if (Parent.US_CountryCode.IsEmpty
					&& Parent.US_DateSinceLastUpdate.IsEmpty
					&& Parent.US_ForeignShipperMID.IsEmpty
					&& Parent.US_HTSNumber.IsEmpty
					&& Parent.US_ManufacturerMID.IsEmpty
					&& Parent.US_TSUSA.IsEmpty)
				{
					Parent.US_CompanyCaseStatusInfo.AddMessageError(EnterAnotherCriteria);
				}

				if (Parent.CaseNumbers.Count > 0)
				{
					Parent.US_CompanyCaseStatusInfo.AddMessageError(CaseNumbersExist);
				}
			}
		}
		internal const string EnterAnotherCriteria = "Another criteria should be entered in addition to the case status.";
		internal const string CaseNumbersExist = "You have already entered Case Number(s). All other criteria will be ignored in the message.";

		protected override void CheckUS_CountryCode()
		{
			base.CheckUS_CountryCode();
			ValidateUS_CompanyCaseStatus();

			if (!Parent.US_CountryCode.IsEmpty && Parent.CaseNumbers.Count > 0)
			{
				Parent.US_CountryCodeInfo.AddMessageError(CaseNumbersExist);
			}
		}

		protected override void CheckUS_DateSinceLastUpdate()
		{
			base.CheckUS_DateSinceLastUpdate();
			ValidateUS_CompanyCaseStatus();

			if (!Parent.US_DateSinceLastUpdate.IsEmpty)
			{
				if (Parent.US_DateSinceLastUpdate < ZDate.Today.AddDays(-6))
				{
					Parent.US_DateSinceLastUpdateInfo.AddMessageError(MustBeWithin7Days);
				}
				else if (Parent.US_DateSinceLastUpdate.IsInTheFutureDatePartOnly)
				{
					Parent.US_DateSinceLastUpdateInfo.AddMessageError(MustBeWithin7Days);
				}

				if (Parent.CaseNumbers.Count > 0)
				{
					Parent.US_DateSinceLastUpdateInfo.AddMessageError(CaseNumbersExist);
				}
			}
		}
		internal const string MustBeWithin7Days = "This date, when used, must be within 7 days in the past.";

		protected override void CheckUS_ForeignShipperMID()
		{
			base.CheckUS_ForeignShipperMID();
			ValidateUS_CompanyCaseStatus();

			if (!Parent.US_ForeignShipperMID.IsEmpty && Parent.CaseNumbers.Count > 0)
			{
				Parent.US_ForeignShipperMIDInfo.AddMessageError(CaseNumbersExist);
			}
		}

		protected override void CheckUS_HTSNumber()
		{
			base.CheckUS_HTSNumber();
			ValidateUS_CompanyCaseStatus();

			if (!Parent.US_HTSNumber.IsEmpty)
			{
				var unformattedNumber = new TariffFormatter().Format(Parent.US_HTSNumber);
				if (unformattedNumber.Length != 8 && unformattedNumber.Length != 10)
				{
					Parent.US_HTSNumberInfo.AddMessageError(TariffNumberShouldBe8Or10);
				}
				else
				{
					var tariff = Parent.Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, unformattedNumber));
					if (tariff == null)
					{
						Parent.US_HTSNumberInfo.AddMessageError(NoTariffWithThisNumber);
					}
				}

				if (Parent.CaseNumbers.Count > 0)
				{
					Parent.US_HTSNumberInfo.AddMessageError(CaseNumbersExist);
				}
			}
		}
		internal const string TariffNumberShouldBe8Or10 = "HTS number must be a 8 or 10 digit number.";
		internal const string NoTariffWithThisNumber = "No HTS record exists with this number.";

		protected override void CheckUS_ManufacturerMID()
		{
			base.CheckUS_ManufacturerMID();
			ValidateUS_CompanyCaseStatus();

			if (!Parent.US_ManufacturerMID.IsEmpty && Parent.CaseNumbers.Count > 0)
			{
				Parent.US_ManufacturerMIDInfo.AddMessageError(CaseNumbersExist);
			}
		}

		protected override void CheckUS_TSUSA()
		{
			base.CheckUS_TSUSA();
			ValidateUS_CompanyCaseStatus();

			if (!Parent.US_TSUSA.IsEmpty && Parent.CaseNumbers.Count > 0)
			{
				Parent.US_TSUSAInfo.AddMessageError(CaseNumbersExist);
			}
		}

		#region Implementation

		public new ACEACCaseQuery Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ACEACCaseQuery)base.Parent; }
		}

		#endregion
	}
}
