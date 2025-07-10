using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.Business
{
	public class CusGuaranteeHeader : EU.Business.CusGuaranteeHeader, Integration.Customs.TR.ICusGuaranteeHeader
	{
		public CusGuaranteeHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusGuaranteeHeaderLookups Lookups => (CusGuaranteeHeaderLookups)GetNewLookups();

		protected override CusPermitHeaderLookups GetNewLookups()
		{
			return new CusGuaranteeHeaderLookups(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SetUOMIfRequired();
		}

		public override ZString CPH_Type
		{
			get => base.CPH_Type;
			set
			{
				base.CPH_Type = value;
				SetUOMIfRequired();
			}
		}

		void SetUOMIfRequired()
		{
			if (CPH_UnitOfMeasure.IsEmpty)
			{
				CPH_UnitOfMeasure = Core.Constants.CurrencyCodes.Turkey;
			}
		}
	}
}
