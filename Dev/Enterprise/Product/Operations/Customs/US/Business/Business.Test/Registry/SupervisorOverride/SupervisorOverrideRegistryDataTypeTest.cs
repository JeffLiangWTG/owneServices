using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(SupervisorOverrideRegistryDataType))]
	sealed class SupervisorOverrideRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<SupervisorOverrideRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "SupervisorOverridesRegistryItemEditor"; }
		}

		protected override SupervisorOverrideRegistryDataType GetNewDataType()
		{
			return new SupervisorOverrideRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var supervisorOverrideData1 = new SupervisorOverrideData();
			supervisorOverrideData1.FillWithValidTestData();

			var supervisorOverrideData2 = new SupervisorOverrideData();
			var nominatedMessageError = supervisorOverrideData2.NominatedMessageErrors.AddNew();
			nominatedMessageError.FieldName = "US_ADDCaseNo";
			nominatedMessageError.MessageErrorText = "This invoice line may be subject to ADD";

			return
			[
				new ValidSampleAndBinaryValueInDB(supervisorOverrideData1, new SupervisorOverrideRegistryDataType().Serialise(supervisorOverrideData1)),
				new ValidSampleAndBinaryValueInDB(supervisorOverrideData2, new SupervisorOverrideRegistryDataType().Serialise(supervisorOverrideData2))
			];
		}

		protected override void SetUp()
		{
			base.SetUp();
			var factory = new BusinessObjectFactory();
			var groupCollection = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<MasterFiles.Integration.IGlbGroupCollection>(), new object[] { factory });
			groupCollection.AddNew();
		}
	}
}
