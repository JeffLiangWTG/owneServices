using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Universal.Testing
{
	class CusRulingFindBoxCollectionForTest : CusRulingFindBoxCollection
	{
		public CusRulingFindBoxCollectionForTest(BusinessObjectFactory factory, ZString rulingNumber) : base(factory, rulingNumber)
		{
		}

		public CusRulingFindBoxCollectionForTest(BusinessObjectFactory factory, ZString rulingType, ZString rulingNumber, OrgHeader org, IEnumerable<ZGuid> validOrganizations) : base(factory, rulingType, rulingNumber, org, validOrganizations)
		{
		}

		public new void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
		}
	}
}
