using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class StampDutyFee : CusInBondFee
	{
		public StampDutyFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusInBondFee.Schema
		{
			public const string RegistrationDate = nameof(RegistrationDate);
		}

		public StampDutyCargoDesc StampDutyBFE => Factory.Load<StampDutyCargoDesc>(BFE_BY);

		#region Properties

		public override ZString BFE_MethodOfCalculation
		{
			get => base.BFE_MethodOfCalculation;
			set
			{
				var oldValue = BFE_MethodOfCalculation;
				base.BFE_MethodOfCalculation = value;
				if (oldValue != BFE_MethodOfCalculation && !IsCopying && BFE_MethodOfCalculation == StampDutyStatusCodeList.Codes.D3)
				{
					var cusTaxOrFee = new RefCusTaxOrFee.Loader(Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Turkey, "89", RegistrationDate);
					if (cusTaxOrFee != null)
					{
						BFE_ChargeAmount = cusTaxOrFee.ZZF_Value;
					}

					BFE_MethodOfCalculationInfo.RefreshBinding();
				}

				StampDutyBFE?.NctsHeader?.MarkAsNeedingValidation();
			}
		}

		public ZDate RegistrationDate => Factory.GetValue(ref cachedRegistrationDate, () =>
		{
			var result = ZDate.Today;
			var cusStatementLine = Factory.LoadTop1<CusStatementLine>(new ZQuery(CusStatementLineSchema.B3_BrokerReference, StampDutyBFE.NctsHeader.BH_JobReference));
			if (cusStatementLine != null)
			{
				result = cusStatementLine.B3_EntryDate;
			}
			return result;
		});

		CachedProperty<ZDate> cachedRegistrationDate;

		public ZPropertyInfo RegistrationDateInfo => GetZPropertyInfo(Schema.RegistrationDate);

		#endregion

		public override void OnSaving()
		{
			if (BFE_MethodOfCalculation.IsEmpty && BFE_ChargeAmount.IsEmpty && StampDutyBFE != null)
			{
				StampDutyBFE.Delete();
			}

			base.OnSaving();
		}
	}
}
