using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class BaseCusSCAOceanBillTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var applicationCode = (row != null) ? new ZString(row[BaseCusSCAOceanBill.Schema.CB_ApplicationCode]) : ZString.Empty;
			switch (applicationCode)
			{
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIAir:
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea:
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRail:
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRoad:
					return ObjectFactory.GetType<Integration.Customs.CA.ICusSCAOceanBill>();
				case Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff:
					return ObjectFactory.GetType<Integration.Customs.NZ.ICusSCAOceanBill>();
				case Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages:
					return ObjectFactory.GetType<Integration.Customs.AU.ICusSCAOceanBill>();
#if DEBUG
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.BaseTesting:
					return typeof(TestCusSCAOceanBill);
#endif
				default:
					return typeof(DefaultCusSCAOceanBill);
			}
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForNew()
		{
			switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				case Core.Constants.CountryCodes.Australia:
					return ObjectFactory.GetType<Integration.Customs.AU.ICusSCAOceanBill>();
				case Core.Constants.CountryCodes.NewZealand:
					return ObjectFactory.GetType<Integration.Customs.NZ.ICusSCAOceanBill>();
				default:
					return null;
			}
		}

		public static string[] GetApplicationCodesForForwarding()
		{
			switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				case Core.Constants.CountryCodes.Australia:
					return new[] { Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages };
				case Core.Constants.CountryCodes.NewZealand:
					return new[] { Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff };
				default:
					return Array.Empty<string>();
			}
		}
	}
}
