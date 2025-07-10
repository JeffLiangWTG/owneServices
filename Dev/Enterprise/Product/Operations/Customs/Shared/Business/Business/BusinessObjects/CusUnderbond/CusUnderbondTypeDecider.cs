using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusUnderbondTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var applicationCode = row[CusUnderbondSchema.Constants.C4_ApplicationCode].ToString();
			switch (applicationCode)
			{
				case CusUnderbondApplicationCodeList.Codes.AUUnderbond:
					return ObjectFactory.GetType<Integration.Customs.AU.ICusUnderbond>();
				case CusUnderbondApplicationCodeList.Codes.GBFallback:
					return ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICusUnderbond_Fallback>();
				case CusUnderbondApplicationCodeList.Codes.GBInterAirportRemoval:
					return ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICusUnderbond_InterAirportRemoval>();
				case CusUnderbondApplicationCodeList.Codes.GBInterShedRemoval:
					return ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICusUnderbond_InterShedRemoval>();
				case CusUnderbondApplicationCodeList.Codes.GBTranshipmentRemoval:
					return ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICusUnderbond_TranshipmentRemoval>();
				case CusUnderbondApplicationCodeList.Codes.NZTranshipmentRequest:
					return ObjectFactory.GetType<Integration.Customs.NZ.ICusUnderbond>();
				default:
					return typeof(CusUnderbond);
			}
		}

		public override Type GetTypeForBinding() => null;
		public override Type GetTypeForNew() => null;
	}
}
