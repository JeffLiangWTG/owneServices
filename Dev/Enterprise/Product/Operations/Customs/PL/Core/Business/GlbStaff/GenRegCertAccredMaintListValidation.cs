using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public class GenRegCertAccredMaintListValidation : MasterFiles.Business.GenRegCertAccredMaintListValidation
{
	public GenRegCertAccredMaintListValidation(AutoGenRegCertAccredMaintList parent) : base(parent)
	{
	}

	protected override void CheckXZ_IssueDateIsValidZDateTimeRange()
	{
		var type = Parent.XZ_Type;
		if (type != Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK && type != Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.CAR)
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.XZ_IssueDateInfo);
		}
	}
}
