using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	class AsycudaContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRelation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			container.Relation = "XXX";
			AssertHasMessageErrorContaining(container.RelationInfo, ListValidation.InvalidCodeMessageError);
			container.Relation = "Local";
			AssertNoMessageErrorContaining(container.RelationInfo, ListValidation.InvalidCodeMessageError);
		}

		[ExpectException(typeof(MaxLengthExceededException))]
		public void TestRelationMaxLength()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			try
			{
				container.Relation = "ABCDEXXX";
			}
			catch (MaxLengthExceededException)
			{
				ErrorReporter.Clear();
				throw;
			}
		}

		public void TestCheckACN_RC_ContainerType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_ManifestType = TRManifestTypes.Codes.HAVIHR;
			var container = header.Containers.AddNew();
			container.ACN_RC_ContainerType = ZGuid.Empty;
			CombineAssertions(() =>
			{
				header.AMA_ManifestType = TRManifestTypes.Codes.HAVITH;
				container.ACN_RC_ContainerType = ZGuid.NewZGuid();
				AssertNoMessageErrors(header.MasterBill.ABL_E_ARVInfo);
				container.ACN_RC_ContainerType = ZGuid.Empty;
				AssertNoMessageErrors(container.ACN_RC_ContainerTypeInfo);
			});
		}
	}
}
