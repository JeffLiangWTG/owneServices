using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.BusinessObjects.CusPermit
{
	public interface IAllowPermitProcessing
	{
		ZString GetPermitReference();
		ZInt GetPermitReferenceNumberLine();
		EDIMessageCollection Messages { get; }
		ZInt PermitValueDecimalPlaceCount { get; }
		ZInt PermitQuantityDecimalPlaceCount { get; }
		IList<PermitRecord> GetPermitRecords();
		ZString GetPermitComment(PermitRecord permitRecord);
		ZInt PackageCount { get; }
	}
}
