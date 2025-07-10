using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module
{
	public class AuditClassificationLinesMethod : OperationalActionMethod
	{
		public AuditClassificationLinesMethod()
			: base(new ZGuid("FDC4E205-D07C-4E12-9183-B7F4D17275F5"))
		{
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new AuditClassificationLinesMethodApplicator(Name, factory);
		}

		public override string Name
		{
			get { return Res.GetString("CC9D521E-0685-47C5-96CF-7C03CDB8A8F2", "Audit Classification Lines"); }
		}

		public override string Description
		{
			get { return Res.GetString("CC9D521E-0685-47C5-96CF-7C03CDB8A8F2", "Audit Classification Lines"); }
		}
	}
}
