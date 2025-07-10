using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class EDIMessage(BusinessObjectFactory factory, DataRow row) : BaseEDIMessage(factory, row), Integration.Customs.PLExitControl.IEDIMessage
{
	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EM_ApplicationCode = ApplicationCodes.PLCustomsExitControl;
	}
}
