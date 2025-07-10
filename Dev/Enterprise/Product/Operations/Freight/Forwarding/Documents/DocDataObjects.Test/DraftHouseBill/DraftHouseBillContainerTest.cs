using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(DraftHouseBillContainer))]
	sealed class DraftHouseBillContainerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var context = new CommonContext(Factory);

			return new DraftHouseBillContainer
			{
				Number = "AAAA000007",
				Seal = "SEAL1",
				SecondSeal = "SEAL2",
				ThirdSeal = "SEAL3",
				Type = new ContainerType(context.ContainerTypes)
				{
					Code = "20GP"
				},
				GrossWeight = new Measurement
				{
					Value = 6000,
					Unit = new CodeDescription(context.WeightUnits)
					{
						Code = Core.Constants.Weight.Kilograms
					}
				},
				VolumeCapacity = new Measurement
				{
					Value = 6000,
					Unit = new CodeDescription(context.VolumeUnits)
					{
						Code = Core.Constants.Volume.CubicMetres
					}
				},
				PackType = new CodeDescription(context.WeightUnits)
				{
					Code = Core.Constants.PkgUnit.Package
				},
				PackCount = 11,
				MarksAndNumbers = "MarksAndNos123",
				GoodsDescription = "GoodsDescription123"
			};
		}
	}
}
