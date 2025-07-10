using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.Universal
{
	public class CusRefTradeGroupCountry : AutoCusRefTradeGroupCountry
	{
		public new class Schema : AutoCusRefTradeGroupCountry.Schema
		{
			public new const int CRA_DescriptionMaxLength = 100;
		}

		public CusRefTradeGroupCountry(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		[MaxLength(Schema.CRA_RN_NKTradeGroupCountryCodeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusRefTradeGroupCountryLookups.TradeGroupCountryCodes))]
		[ResourceStringData("Enterprise.Customs.Universal.CusRefTradeGroupCountry|CRA_RN_NKTradeGroupCountryCode", Caption = "Country/Region Code", ShortCaption = "Ctry./Rgn.")]
		public override ZString CRA_RN_NKTradeGroupCountryCode
		{
			get => base.CRA_RN_NKTradeGroupCountryCode;
			set
			{
				var oldValue = CRA_RN_NKTradeGroupCountryCode;
				base.CRA_RN_NKTradeGroupCountryCode = value;
				if (!IsCopying && oldValue != CRA_RN_NKTradeGroupCountryCode)
				{
					CRA_Description = TradeGroupCountryCode?.Description ?? ZString.Empty;
				}
			}
		}

		[MaxLength(Schema.CRA_DescriptionMaxLength)]
		[ResourceStringData("Enterprise.Customs.Universal.CusRefTradeGroupCountry|CRA_Description", Caption = "Description")]
		public override ZString CRA_Description { get => base.CRA_Description; set => base.CRA_Description = value; }

		[ResourceStringData("Enterprise.Customs.Universal.CusRefTradeGroupCountry|CRA_StartDate", Caption = "Start Date")]
		public override ZDate CRA_StartDate { get => base.CRA_StartDate; set => base.CRA_StartDate = value; }

		[ResourceStringData("Enterprise.Customs.Universal.CusRefTradeGroupCountry|CRA_EndDate", Caption = "End Date")]
		public override ZDate CRA_EndDate { get => base.CRA_EndDate; set => base.CRA_EndDate = value; }

		[RelatedBusinessObject("TradeGroup")]
		public override ZGuid CRA_CR9_TradeGroup { get => base.CRA_CR9_TradeGroup; set => base.CRA_CR9_TradeGroup = value; }

		public CusRefTradeGroup TradeGroup => Factory.Load<CusRefTradeGroup>(CRA_CR9_TradeGroup);

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore => Res.GetString("B5FF83F9-D14F-416E-AA63-D9642F191BA0", "Trade Group Country/Region {0}", CRA_RN_NKTradeGroupCountryCode);
	}
}
