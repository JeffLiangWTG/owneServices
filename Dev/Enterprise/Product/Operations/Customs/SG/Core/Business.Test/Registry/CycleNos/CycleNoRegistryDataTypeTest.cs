using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Registry.Testing
{
	[TestedType(typeof(CycleNoRegistryDataType))]
	sealed class CycleNoRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CycleNoRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get
			{
				return "CycleNoRegistryItemEditor";
			}
		}

		protected override CycleNoRegistryDataType GetNewDataType()
		{
			return new CycleNoRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			CycleNoCollection collection = new CycleNoCollection();
			CycleNo cycleNo = collection.AddNew();
			cycleNo.CycleNum = 1;
			cycleNo.SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(23, 40, 0));
			CycleNoCollection collection2 = new CycleNoCollection();
			CycleNo cycleNo2 = collection.AddNew();
			cycleNo2.CycleNum = 2;
			cycleNo2.SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(10, 20, 0));
			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(collection, new CycleNoRegistryDataType().Serialise(collection)), new ValidSampleAndBinaryValueInDB(collection2, new CycleNoRegistryDataType().Serialise(collection2)) };
		}
	}
}
