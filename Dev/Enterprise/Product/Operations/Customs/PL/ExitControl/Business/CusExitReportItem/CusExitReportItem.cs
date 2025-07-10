using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class CusExitReportItem : EU.ExitControl.Business.CusExitReportItem
{
	public CusExitReportItem(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	#region ICusSupportingInfoTypeSupporter

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var result = base.GetCusSupportingInfoTypes();
		result[CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
		return result;
	}

	#endregion

	protected override EU.ExitControl.Business.IAdditionalInfoCollection<EU.ExitControl.Business.AdditionalInfo> CreateNewAdditionalInfoCollection() => new EU.ExitControl.Business.AdditionalInfoCollection<AdditionalInfo>(this);

	protected override bool IsUCC6Core => Report?.IsUCC6 ?? true;
}
