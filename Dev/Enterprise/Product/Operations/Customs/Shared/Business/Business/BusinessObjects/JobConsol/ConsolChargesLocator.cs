using System;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Business
{
	public class ConsolChargesLocator : IServiceLocator, Integration.Customs.IConsolChargesLocator
	{
		public ConsolChargesLocator(ForwardingConsol consol)
		{
			this.consol = consol;
		}
		readonly ForwardingConsol consol;

		bool IsNZExportAirOrSeaConsol
		{
			get
			{
				return consol.JK_JX_JA_RL_NKPortOfLoading.StartsWith(Core.Constants.CountryCodes.NewZealand)
				  && !consol.JK_JX_JB_RL_NKPortOfDischarge.StartsWith(Core.Constants.CountryCodes.NewZealand)
				  && (consol.IsAir || consol.IsSea);
			}
		}

		#region IServiceLocator Members

		object IServiceLocator.GetService(Type serviceType)
		{
			if (typeof(ICustomsCharges) == serviceType && IsNZExportAirOrSeaConsol)
			{
				return Activator.CreateInstance(ObjectFactory.GetType<Integration.Customs.NZ.IConsolCustomsCharges>(), consol);
			}
			return null;
		}

		#endregion
	}
}
