using System;
using Enterprise.Messaging.Integration;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Packing.DataTransfer.Universal.Testing
{
	class PackageParentJobMapperTest : PackingTestCaseWithFactory
	{
		#region TestGetWriter_DoesNotAcceptNullWriteManager

		public void TestGetWriter_DoesNotAcceptNullWriteManager()
		{
			AssertExceptionThrown<ArgumentNullException>(() => PackageParentJobMapper.GetWriter(ParentJobType.None, null));
		}

		#endregion

		#region TestGetWriter_ParentTypeMapped

		public void TestGetWriter_ParentTypeMapped()
		{
			Data.CreatePackingData();
			var mockHandle = new DummyHandle();

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			{
				var writer = PackageParentJobMapper.GetWriter(ParentJobType.Dummy, GetWritingManager(Factory.New<PkgPackage>()));
				AssertNotNull(writer);
				AssertEquals(mockHandle.Writer, writer);
			}
		}

		#endregion

		#region TestGetWriter_ParentTypeNotProperlyMapped

		public void TestGetWriter_ParentTypeNotProperlyMapped()
		{
			AssertExceptionThrown(typeof(InvalidOperationException),
				"All ParentJobTypes must be mapped to UniversalPackingExporters in our Application Configuration. Missing Mapping for ParentJobType: -1.",
				() => PackageParentJobMapper.GetWriter((ParentJobType)(-1), GetWritingManager(Factory.New<PkgPackage>())));
		}

		#endregion

		#region TestGetWriter_ParentTypeOfNone

		public void TestGetWriter_ParentTypeOfNone()
		{
			var package = Factory.New<PkgPackage>();
			var writer = PackageParentJobMapper.GetWriter(ParentJobType.None, GetWritingManager(package));
			AssertNotNull(writer);
			AssertNotNull(writer.GetDataObject(package));

			var topLevelWriter = (ITopLevelDataObjectWriter)writer;
			AssertEquals("EDI Message Sub Type is correct.", EDIMessageSubTypeList.Codes.XmlUniversalShipment, topLevelWriter.EDIMessageSubType);
			AssertEquals("Data Context Type is correct.", DataContextType.PkgPackage, topLevelWriter.TopLevelDataContextType);
		}

		#endregion

		#region Implementation

		IDataWritingManager GetWritingManager(PkgPackage package) => new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, package));

		#endregion
	}
}
