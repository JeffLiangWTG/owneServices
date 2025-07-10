using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(ExportAWBHeaderDependentCollection))]
	sealed class ExportAWBHeaderDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRightDefaultValuesGetFilledInOnAddNew()
		{
			var header = Factory.New<ExportAWBHeader>();
			var collection = new ExportAWBHeaderDependentCollection(header);

			AssertEquals("header.EH_EH_Parent", ZGuid.Empty, header.EH_EH_Parent);
			AssertEquals("header.EH_ParentID", header.PK, header.EH_ParentID);
			AssertEquals("header.EH_Table", ExportAWBHeaderSchema.Constants.TableName, header.EH_Table);
			AssertEquals("header.EH_AWBType", AWBTypeList.Codes.AgentMaster, header.EH_AWBType);

			var child = collection.AddNew();
			AssertEquals("child.EH_EH_Parent", header.PK, child.EH_EH_Parent);
			AssertEquals("child.EH_ParentID", child.PK, child.EH_ParentID);
			AssertEquals("child.EH_Table", ExportAWBHeaderSchema.Constants.TableName, child.EH_Table);
			AssertEquals("child.EH_AWBType", AWBTypeList.Codes.House, child.EH_AWBType);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ExportAWBHeaderDependentCollection(Factory.New<ExportAWBHeader>());
		}

		#endregion
	}
}
