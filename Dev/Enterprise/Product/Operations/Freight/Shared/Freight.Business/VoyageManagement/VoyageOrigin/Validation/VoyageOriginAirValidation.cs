using CargoWise.ComponentModel;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.Business
{
	public class VoyageOriginAirValidation : BaseJobVoyOriginValidation
	{
		public VoyageOriginAirValidation(AutoJobVoyOrigin parent)
			: base(parent)
		{
		}

		protected override void CheckJA_RL_NKPortOfLoading()
		{
			base.CheckJA_RL_NKPortOfLoading();
			if (Origin.PortOfLoading != null && !Origin.PortOfLoading.RL_HasAirport)
			{
				Origin.JA_RL_NKPortOfLoadingInfo.AddWarning(Res.GetString("3de0fb95-c0de-48b0-a669-3ba381a09d64", "{0} does not have an air port.", Origin.JA_RL_NKPortOfLoading));
			}
		}

		protected override bool AllowETAsMoreThanOneDayBeforeETD
		{
			get { return false; }
		}

		protected override void CheckJA_E_ARV()
		{
			base.CheckJA_E_ARV();
			if (!Parent.JA_E_ARVInfo.HasErrors())
			{
				ValidateJA_S_ARV();
			}
		}

		protected override void CheckJA_E_DEP()
		{
			base.CheckJA_E_DEP();
			if (!Parent.JA_E_DEPInfo.HasErrors())
			{
				ValidateJA_S_DEP();
			}
		}

		protected override void CheckJA_S_ARV()
		{
			base.CheckJA_S_ARV();
			ValidateScheduleArrivalDate();
		}

		protected override void CheckJA_S_DEP()
		{
			base.CheckJA_S_DEP();
			ValidateScheduleDepartureDate();
		}

		void ValidateScheduleArrivalDate()
		{
			if (!Parent.JA_S_ARV.IsEmpty && !Parent.JA_E_ARV.IsEmpty)
			{
				if (Parent.JA_S_ARV > Parent.JA_E_ARV && Parent.JA_E_ARV.AddDays(1) < Parent.JA_S_ARV)
				{
					Parent.JA_S_ARVInfo.AddWarning(Res.GetString("ffb3f707-8167-4f8f-81b2-7f800bd2ea86", "STA is more than a day after ETA"));
				}
				else if (Parent.JA_S_ARV < Parent.JA_E_ARV && Parent.JA_E_ARV.AddDays(-1) > Parent.JA_S_ARV)
				{
					Parent.JA_S_ARVInfo.AddWarning(Res.GetString("971ce1e6-a486-4395-b5bf-910626a1de22", "ETA is more than a day after STA"));
				}
			}
		}

		void ValidateScheduleDepartureDate()
		{
			if (!Parent.JA_S_DEP.IsEmpty && !Parent.JA_E_DEP.IsEmpty)
			{
				if (Parent.JA_S_DEP > Parent.JA_E_DEP && Parent.JA_E_DEP.AddDays(1) < Parent.JA_S_DEP)
				{
					Parent.JA_S_DEPInfo.AddWarning(Res.GetString("791c523f-dfe1-477a-8086-39097418cb29", "STD is more than a day after ETD"));
				}
				else if (Parent.JA_S_DEP < Parent.JA_E_DEP && Parent.JA_E_DEP.AddDays(-1) > Parent.JA_S_DEP)
				{
					Parent.JA_S_DEPInfo.AddWarning(Res.GetString("a7033ae4-13ed-4e79-bc97-aaa5bd7c7391", "ETD is more than a day after STD"));
				}
			}
		}
	}
}
