using System;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface ICusSupportingInfoTypeSubTypeSupporter
	{
		Type GetTypeBySubType(ZString subType);
	}
}
