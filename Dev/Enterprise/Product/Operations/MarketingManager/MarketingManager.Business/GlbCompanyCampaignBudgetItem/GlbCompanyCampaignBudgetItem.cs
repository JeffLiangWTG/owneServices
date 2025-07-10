using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignBudgetItem : AutoGlbCompanyCampaignBudgetItem
	{
		public GlbCompanyCampaignBudgetItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		#region G9_RX_NKCurrency

		public override ZString G9_RX_NKCurrency
		{
			get { return base.G9_RX_NKCurrency; }
			set
			{
				base.G9_RX_NKCurrency = value;
				if (Currency != null)
				{
					if (Currency.RX_Code == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						G9_ExchangeRate = 1;
					}
					else
					{
						G9_ExchangeRate = Currency.CurrentBuyRate;
					}
				}
			}
		}

		#endregion

		#region G9_AC

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public override ZGuid G9_AC
		{
			get { return base.G9_AC; }
			set
			{
				base.G9_AC = value;
				if (ChargeCode != null)
				{
					G9_ChargeDescription = ChargeCode.AC_Desc;
				}
			}
		}

		#endregion

		#region Local Flat Amount

		public ZDecimal LocalFlatAmount
		{
			get { return Env.CurrentCompany.ExchangeRate.ForeignToLocal(G9_FlatAmount, G9_ExchangeRate); }
		}

		public ZPropertyInfo LocalFlatAmountInfo
		{
			get { return GetZPropertyInfo(nameof(LocalFlatAmount)); }
		}

		#endregion

		#region Local Per Unit Amount

		public ZDecimal LocalPerUnitAmount
		{
			get { return Env.CurrentCompany.ExchangeRate.ForeignToLocal(G9_PerUnitAmount, G9_ExchangeRate); }
		}

		public ZPropertyInfo LocalPerUnitAmountInfo
		{
			get { return GetZPropertyInfo(nameof(LocalPerUnitAmount)); }
		}

		#endregion

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion
	}
}
