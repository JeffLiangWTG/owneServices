using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusBrokerStaffRegistryDataType))]
	sealed class CusBrokerStaffRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CusBrokerStaffRegistryDataType>
	{
		protected override CusBrokerStaffRegistryDataType GetNewDataType() => new CusBrokerStaffRegistryDataType();
		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var item1 = new CusBrokerStaff(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), factory);
			item1.BrokerStaffCode = "CYO";
			item1.Mailbox = "TBK0461-0";
			var item2 = new CusBrokerStaff(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), factory);
			item2.BrokerStaffCode = "CYO";
			item2.Mailbox = ZString.Empty;
			return new[] { new ValidSampleAndBinaryValueInDB(item1, new CusBrokerStaffRegistryDataType().Serialise(item1)), new ValidSampleAndBinaryValueInDB(item2, new CusBrokerStaffRegistryDataType().Serialise(item2)) };
		}

		protected override string ExpectedEditorName => "CusBrokerStaffRegistryItemEditor";
		protected override void SetUp()
		{
			base.SetUp();
			factory = new BusinessObjectFactory();
			new TestTWCreator(factory).CreateBrokerStaff();
		}

		BusinessObjectFactory factory;
	}
}
