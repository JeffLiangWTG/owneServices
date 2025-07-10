using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	[TestedType(typeof(QueryUserSelectVesselFromLloydsNumber))]
	sealed class QueryUserSelectVesselFromLloydsNumberTest : NonPersistentBusinessObjectTestCase
	{
		public void TestVesselName()
		{
			AssertEquals("VesselName", BusinessEntity.VesselName);
		}

		public void TestLloydsNumber()
		{
			AssertEquals("Lloyds", BusinessEntity.LloydsNumber);
		}

		public void TestAvailableVessels()
		{
			RefVessel ambiguousVessel1 = NewVessel("Ambiguous Vessel 1", "Lloyds");
			RefVessel ambiguousVessel2 = NewVessel("Ambiguous Vessel 2", "Lloyds");
			RefVessel ambiguousVessel3 = NewVessel("VesselName", "Ambig.");

			AssertEquals("Expect 3 ambiguous vessels to select from", 3, BusinessEntity.AvailableVessels.Count);
			AssertEquals("ReadOnly", true, BusinessEntity.AvailableVessels.ReadOnly);
		}

		#region Implementation

		QueryUserSelectVesselFromLloydsNumber BusinessEntity
		{
			get
			{
				if (fBusinessEntity == null)
				{
					fBusinessEntity = (QueryUserSelectVesselFromLloydsNumber)GetNewBusinessObject();
				}
				return fBusinessEntity;
			}
		}
		QueryUserSelectVesselFromLloydsNumber fBusinessEntity;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new QueryUserSelectVesselFromLloydsNumber(Factory, "VesselName", "Lloyds");
		}

		RefVessel NewVessel(ZString vesselName, ZString lloydsNumber)
		{
			RefVessel result = Factory.New<RefVessel>();
			result.RV_Name = vesselName;
			result.RV_LloydsNumber = lloydsNumber;
			return result;
		}

		#endregion
	}
}
