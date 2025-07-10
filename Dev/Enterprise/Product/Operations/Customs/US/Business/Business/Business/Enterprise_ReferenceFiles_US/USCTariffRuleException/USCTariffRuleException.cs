using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public sealed class USCTariffRuleException : AutoUSCTariffRuleException
	{
		public USCTariffRuleException(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region New Properties/Methods

		[List(nameof(Lookups) + "." + nameof(USCTariffRuleExceptionLookups.Tariffs))]
		[BusinessObjectTestExclude]
		[MaxLength(13)]
		public ZString FormattedTariff
		{
			get { return new TariffFormatter().DisplayFormat(U2_Tariff); }
			set
			{
				U2_Tariff = new TariffFormatter().Format(value);
				Validation.ValidateFormattedTariff();
				FormattedTariffInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FormattedTariffInfo
		{
			get { return GetZPropertyInfo(nameof(FormattedTariff)); }
		}

		[List(nameof(Lookups) + "." + nameof(USCTariffRuleExceptionLookups.Tariffs))]
		[BusinessObjectTestExclude]
		[MaxLength(13)]
		public ZString FormattedTariffTo
		{
			get { return new TariffFormatter().DisplayFormat(U2_TariffTo); }
			set
			{
				U2_TariffTo = new TariffFormatter().Format(value);
				Validation.ValidateFormattedTariffTo();
				FormattedTariffToInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FormattedTariffToInfo
		{
			get { return GetZPropertyInfo(nameof(FormattedTariffTo)); }
		}

		public bool Applies(ZString tariffNumber, ZDateTime dateOfException)
		{
			return IsValidForThisTariffNumber(tariffNumber) &&
					(U2_DateFrom.IsEmpty || U2_DateFrom <= dateOfException) &&
					(U2_DateTo.IsEmpty || U2_DateTo >= dateOfException);
		}

		bool IsValidForThisTariffNumber(ZString tariffNumber)
		{
			if (U2_TariffTo.IsEmpty)
			{
				return tariffNumber.StartsWith(U2_Tariff);
			}
			else
			{
				return tariffNumber.CompareTo(U2_Tariff) >= 0 && tariffNumber.CompareTo(U2_TariffTo) <= 0;
			}
		}

		internal bool HasBroaderTariffRangesThan(USCTariffRuleException passedException)
		{
			bool result = false;

			if (U2_TariffTo.IsEmpty)
			{
				result = passedException.U2_Tariff.StartsWith(U2_Tariff) &&
					(passedException.U2_TariffTo.IsEmpty || passedException.U2_TariffTo.StartsWith(U2_Tariff));
			}
			else
			{
				result = U2_Tariff.CompareTo(passedException.U2_Tariff) <= 0 &&
					U2_TariffTo.CompareTo(passedException.EffectiveTariffTo) >= 0;
			}

			return result;
		}

		internal ZString EffectiveTariffTo
		{
			get { return U2_TariffTo.IsEmpty ? U2_Tariff : U2_TariffTo; }
		}

		#endregion

		#region Overriden Properties/Methods

		[RelatedBusinessObject("TariffRule")]
		public override ZGuid U2_U1
		{
			get { return base.U2_U1; }
			set { base.U2_U1 = value; }
		}

		#endregion

		#region Related Objects

		public USCTariffRule TariffRule
		{
			get { return Factory.Load<USCTariffRule>(U2_U1); }
		}

		#endregion
	}
}
