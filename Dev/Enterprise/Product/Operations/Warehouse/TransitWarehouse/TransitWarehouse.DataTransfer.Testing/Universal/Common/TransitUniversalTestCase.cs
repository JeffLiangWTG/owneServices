using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class TransitUniversalTestCase : OrganizationAddressTestHelper
	{
		protected TestDataForUniversal Data
		{
			get { return data ?? (data = new TestDataForUniversal(Factory, Logger)); }
		}

		protected void AssertUniversalJobLink(StmUniversalJobLink[] links, OrgHeader organisation, ZGuid parentID, string tablePrefix, DataContextType dataContextType, string sourceKey, string enterpriseCode, string serverCode, string companyCode)
		{
			var matchingLink = links.Single(l => l.UCL_ParentID == parentID && l.UCL_SourceType == dataContextType.ToString());
			AssertEquals(organisation?.PK ?? Guid.Empty, matchingLink.UCL_OH_Owner);
			AssertEquals(tablePrefix, matchingLink.UCL_ParentTableCode);
			AssertEquals(sourceKey, matchingLink.UCL_SourceKey);
			AssertEquals(enterpriseCode, matchingLink.UCL_EnterpriseCode);
			AssertEquals(serverCode, matchingLink.UCL_ServerCode);
			AssertEquals(companyCode, matchingLink.UCL_CompanyCode);
		}

		protected T[] AssertEntityCount<T>(int? count, UniversalObjectFactory factory = null) where T : BusinessObject
		{
			if (count == null)
			{
				return null;
			}
			var entities = (factory ?? Factory).Load<T>(new ZQuery());
			AssertEquals($"There should be {count} {typeof(T).Name}(s) created.", count, entities.Length);
			return entities;
		}

		TestDataForUniversal data;
	}
}
