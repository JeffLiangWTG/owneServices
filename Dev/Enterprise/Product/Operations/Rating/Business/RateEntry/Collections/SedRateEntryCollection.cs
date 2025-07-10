using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.Business
{
	public class SEDRateEntryCollection : RateEntryCollection
	{
		public SEDRateEntryCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory) { }

		public override ZString RateEntryType
		{
			get { return RatingConstants.RateCategory.SED; }
		}

		protected override ZString DefaultRateEntryMode
		{
			get { return Core.Constants.RateMode.SEA; }
		}

		protected override Guid[] DefaultChargeCodes(RateEntry entry)
		{
			var chargeCodePK = LinerAgencyDataRegistry.Instance.ExportDetentionChargeCode.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);

			if (chargeCodePK == Guid.Empty)
			{
				return Array.Empty<Guid>();
			}
			else
			{
				return new Guid[] { chargeCodePK };
			}
		}

		protected override string DefaultFreightChargeCalculator
		{
			get { return CombinedCalculator.Code; }
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var rateEntryChild = (RateEntry)child;

			rateEntryChild.Unit = RatingConstants.Units.DY;
			rateEntryChild.TI_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		}
	}
}

