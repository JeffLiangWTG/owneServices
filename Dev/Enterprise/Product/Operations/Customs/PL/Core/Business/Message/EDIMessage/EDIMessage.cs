using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.Business;

public class EDIMessage(BusinessObjectFactory factory, DataRow row) : BaseEDIMessage(factory, row), Integration.Customs.PL.IEDIMessage
{
	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EM_ApplicationCode = ApplicationCodes.PLCustoms;
	}
}
