using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class AuditDetailsWrapper : NonPersistentBusinessObject, IAuditDetails
	{
		public AuditDetailsWrapper(BusinessObjectFactory factory, IAuditDetails details)
		{
			this.factory = factory;
			this.details = details;
		}

		readonly IAuditDetails details;
		readonly BusinessObjectFactory factory;

		public ZDateTime SystemCreateTimeUtc => details.SystemCreateTimeUtc;
		public ZString SystemCreateUser => details.SystemCreateUser;
		public ZDateTime SystemLastEditTimeUtc => details.SystemLastEditTimeUtc;
		public ZString SystemLastEditUser => details.SystemLastEditUser;
		public ZDateTime SystemCreateTime => details.SystemCreateTimeUtc.ToLocalBranchTime(factory);
		public ZDateTime SystemLastEditTime => details.SystemLastEditTimeUtc.ToLocalBranchTime(factory);
	}
}
