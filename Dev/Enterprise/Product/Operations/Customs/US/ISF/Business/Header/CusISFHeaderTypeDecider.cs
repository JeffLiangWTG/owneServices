using System;
using System.ComponentModel;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.ISF.Business
{
	[ImmutableObject(true)]
	public class CusISFHeaderTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return GetCusISFHeaderType();
		}

		public override Type GetTypeForBinding()
		{
			return GetCusISFHeaderType();
		}

		public override Type GetTypeForNew()
		{
			return GetCusISFHeaderType();
		}

		Type GetCusISFHeaderType()
		{
			return Globals.IsWeb ? ObjectFactory.GetType<Integration.Customs.US.ISF.ITrackingCusISFHeader>() : typeof(CusISFHeader);
		}
	}
}
