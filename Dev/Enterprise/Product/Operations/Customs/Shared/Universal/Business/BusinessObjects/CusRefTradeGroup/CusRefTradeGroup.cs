using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Universal
{
	[CodeProperty(nameof(CR9_TradeGroup)), DescriptionProperty(nameof(CR9_Description))]
	public class CusRefTradeGroup : AutoCusRefTradeGroup
	{
		public new class Schema : AutoCusRefTradeGroup.Schema
		{
			public new const int CR9_DescriptionMaxLength = 100;
		}

		public CusRefTradeGroup(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		[MaxLength(Schema.CR9_TradeGroupMaxLength)]
		[ResourceStringData("Enterprise.Customs.Universal.CusRefTradeGroup|CR9_TradeGroup", Caption = "Trade Group")]
		public override ZString CR9_TradeGroup { get => base.CR9_TradeGroup; set => base.CR9_TradeGroup = value; }

		[MaxLength(Schema.CR9_DescriptionMaxLength)]
		[ResourceStringData("Enterprise.Customs.Universal.CusRefTradeGroup|CR9_Description", Caption = "Description")]
		public override ZString CR9_Description { get => base.CR9_Description; set => base.CR9_Description = value; }

		[ReadOnly(true)]
		[MaxLength(Schema.CR9_RN_NKCountryCodeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusRefTradeGroupLookups.Countries))]
		[ResourceStringData("Enterprise.Customs.Universal.CusRefTradeGroup|CR9_RN_NKCountryCode", Caption = "Country/Region", ShortCaption = "Ctry/Rgn.")]
		public override ZString CR9_RN_NKCountryCode { get => base.CR9_RN_NKCountryCode; set => base.CR9_RN_NKCountryCode = value; }

		[ResourceStringData("Enterprise.Customs.Universal.CusRefTradeGroup| CR9_StartDate", Caption = "Start Date")]
		public override ZDate CR9_StartDate { get => base.CR9_StartDate; set => base.CR9_StartDate = value; }

		[ResourceStringData("Enterprise.Customs.Universal.CusRefTradeGroup|CR9_EndDate", Caption = "End Date")]
		public override ZDate CR9_EndDate { get => base.CR9_EndDate; set => base.CR9_EndDate = value; }

		#region TradeGroupCountries

		[ChildEditable]
		public CusRefTradeGroupCountryCollection TradeGroupCountries
		{
			get
			{
				if (tradeGroupCountries == null)
				{
					tradeGroupCountries = new CusRefTradeGroupCountryCollection(this);
					RegisterEditableChildObject(tradeGroupCountries);
				}
				return tradeGroupCountries;
			}
		}
		CusRefTradeGroupCountryCollection tradeGroupCountries;

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CR9_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
			CR9_StartDate = ZDate.Today;
			CR9_EndDate = (ZDate)ZDateTime.MaxSmallDateTime;
		}

		public override void Delete()
		{
			TradeGroupCountries.DeleteAll();
			base.Delete();
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore => Res.GetString("51C0F2E7-5D5A-456D-8228-88CA042EC038", "Trade Group {0}", CR9_TradeGroup);

		public void OnTradeGroupCountriesSelected(ICodeDescription[] selectedTradeGroupCountries)
		{
			foreach (ICodeDescription selectedTradeGroupCountry in selectedTradeGroupCountries)
			{
				var tradeGroupCountry = TradeGroupCountries.AddNew();
				tradeGroupCountry.CRA_RN_NKTradeGroupCountryCode = selectedTradeGroupCountry.Code;
				tradeGroupCountry.CRA_Description = selectedTradeGroupCountry.Description;
			}
		}
	}
}
