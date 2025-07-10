using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	public abstract class UniversalDataObjectReaderHelperAbstractTest : OrganizationAddressTestHelper
	{
		public static void CreateOrgPatternMatchOverrideInSource(BusinessObjectFactory factory, ZGuid codeMapSourcePK,
			ZString foreignCode, ZString relationship, ZGuid localGuid, ZString localCode)
		{
			var matchOverride = factory.New<OrgPatternMatchOverride>();
			matchOverride.OO_OH = codeMapSourcePK;
			matchOverride.OO_ForeignCode = foreignCode;
			matchOverride.OO_Relationship = relationship;
			if (localGuid != ZGuid.Empty)
			{
				matchOverride.OO_LocalGuid = localGuid;
			}

			if (!string.IsNullOrEmpty(localCode))
			{
				matchOverride.OO_LocalCode = localCode;
			}
		}

		public static IColumnIndexer[] ConvertToColumnIndexer(DataRow[] rows)
		{
			var result = new List<IColumnIndexer>();
			foreach (var row in rows)
			{
				var columnIndexer = row as IColumnIndexer;
				if (columnIndexer != null)
				{
					result.Add(columnIndexer);
				}
			}

			return result.ToArray();
		}
	}
}
