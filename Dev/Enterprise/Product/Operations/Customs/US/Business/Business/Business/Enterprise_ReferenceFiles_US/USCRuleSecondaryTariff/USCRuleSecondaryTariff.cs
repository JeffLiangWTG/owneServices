using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	public sealed class USCRuleSecondaryTariff : AutoUSCRuleSecondaryTariff
	{
		public new class Schema : AutoUSCRuleSecondaryTariff.Schema
		{
			public const string FormattedTariffFrom = "FormattedTariffFrom";
			public const string FormattedTariffTo = "FormattedTariffTo";
			public const string FormattedTariff2 = "FormattedTariff2";
			public const string FormattedTariff3 = "FormattedTariff3";
		}

		public USCRuleSecondaryTariff(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region New Properties/Methods

		[List(nameof(Lookups) + "." + nameof(USCRuleSecondaryTariffLookups.Tariffs))]
		[BusinessObjectTestExclude]
		[MaxLength(13)]
		public ZString FormattedTariffFrom
		{
			get { return new TariffFormatter().DisplayFormat(U3_TariffFrom); }
			set
			{
				U3_TariffFrom = new TariffFormatter().Format(value);
				Validation.ValidateFormattedTariffFrom();
				FormattedTariffFromInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FormattedTariffFromInfo
		{
			get { return GetZPropertyInfo(Schema.FormattedTariffFrom); }
		}

		[List(nameof(Lookups) + "." + nameof(USCRuleSecondaryTariffLookups.Tariffs))]
		[BusinessObjectTestExclude]
		[MaxLength(13)]
		public ZString FormattedTariffTo
		{
			get { return new TariffFormatter().DisplayFormat(U3_TariffTo); }
			set
			{
				U3_TariffTo = new TariffFormatter().Format(value);
				Validation.ValidateFormattedTariffTo();
				FormattedTariffToInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FormattedTariffToInfo
		{
			get { return GetZPropertyInfo(Schema.FormattedTariffTo); }
		}

		[List(nameof(Lookups) + "." + nameof(USCRuleSecondaryTariffLookups.Tariffs))]
		[BusinessObjectTestExclude]
		[MaxLength(13)]
		public ZString FormattedTariff2
		{
			get { return new TariffFormatter().DisplayFormat(U3_Tariff2); }
			set
			{
				U3_Tariff2 = new TariffFormatter().Format(value);
				Validation.ValidateFormattedTariff2();
				FormattedTariff2Info.RefreshBinding();
			}
		}

		public ZPropertyInfo FormattedTariff2Info
		{
			get { return GetZPropertyInfo(Schema.FormattedTariff2); }
		}

		[List(nameof(Lookups) + "." + nameof(USCRuleSecondaryTariffLookups.Tariffs))]
		[BusinessObjectTestExclude]
		[MaxLength(13)]
		public ZString FormattedTariff3
		{
			get { return new TariffFormatter().DisplayFormat(U3_Tariff3); }
			set
			{
				U3_Tariff3 = new TariffFormatter().Format(value);
				Validation.ValidateFormattedTariff3();
				FormattedTariff3Info.RefreshBinding();
			}
		}

		public ZPropertyInfo FormattedTariff3Info
		{
			get { return GetZPropertyInfo(Schema.FormattedTariff3); }
		}

		public bool HasBroaderTariffRangesThan(USCRuleSecondaryTariff passedSecondaryTariff)
		{
			bool result = false;

			if (U3_TariffTo.IsEmpty)
			{
				result = passedSecondaryTariff.U3_TariffFrom.StartsWith(U3_TariffFrom) && (passedSecondaryTariff.U3_TariffTo.IsEmpty || passedSecondaryTariff.U3_TariffTo.StartsWith(U3_TariffFrom));
			}
			else
			{
				result = (U3_TariffFrom.CompareTo(passedSecondaryTariff.U3_TariffFrom) <= 0) && (U3_TariffTo.CompareTo(passedSecondaryTariff.EffectiveTariffTo) >= 0);
			}

			return result;
		}

		public ZString EffectiveTariffTo
		{
			get
			{
				if (U3_TariffTo.IsEmpty)
				{
					return U3_TariffFrom;
				}
				else
				{
					return U3_TariffTo;
				}
			}
		}

		public bool IsValid(ZString tariff, ZDateTime date)
		{
			return IsValidTariff(tariff) && U3_DateFrom <= date && (U3_DateTo.IsEmpty || U3_DateTo >= date);
		}

		bool IsValidTariff(ZString tariff)
		{
			bool isValidForFromTo = false;

			if (U3_TariffTo.IsEmpty)
			{
				isValidForFromTo = tariff.StartsWith(U3_TariffFrom);
			}
			else
			{
				isValidForFromTo = U3_TariffFrom.CompareTo(tariff) <= 0 && U3_TariffTo.CompareTo(tariff) >= 0;
			}

			return isValidForFromTo || !U3_Tariff2.IsEmpty && tariff.StartsWith(U3_Tariff2) || !U3_Tariff3.IsEmpty && tariff.StartsWith(U3_Tariff3);
		}

		public IEnumerable<ZString> GetASetOfAssociatedTariffNumbers()
		{
			if (!U3_TariffFrom.IsEmpty)
			{
				yield return U3_TariffFrom;
			}

			if (!U3_Tariff2.IsEmpty)
			{
				yield return U3_Tariff2;
			}

			if (!U3_Tariff3.IsEmpty)
			{
				yield return U3_Tariff3;
			}
		}

		#endregion

		#region Overriden Properties/Methods

		[RelatedBusinessObject("TariffRule")]
		public override ZGuid U3_U1
		{
			get { return base.U3_U1; }
			set { base.U3_U1 = value; }
		}

		#endregion

		#region Related Objects

		public USCTariffRule TariffRule
		{
			get { return Factory.Load<USCTariffRule>(U3_U1); }
		}

		[ChildEditable(true)]
		public USCRuleSecondaryTariffExceptionCollection Exceptions
		{
			get
			{
				if (exceptions == null)
				{
					exceptions = new USCRuleSecondaryTariffExceptionCollection(this);
					RegisterEditableChildObject(exceptions);
				}
				return exceptions;
			}
		}
		USCRuleSecondaryTariffExceptionCollection exceptions;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new USCRuleSecondaryTariffFetchStrategy(this);
		}

		#endregion
	}
}
