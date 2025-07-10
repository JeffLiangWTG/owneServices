using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(VGMMessagingContainer))]
	sealed class VGMMessagingContainerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPopulateContainerGenset()
		{
			var container = new Container
			{
				Number = "AAAA000007",
				Genset = false,
				HasControlledAtmosphere = true
			};
			var vgmMessagingContainer = new VGMMessagingContainer(container, null);

			Assert(!vgmMessagingContainer.Genset);

			container.Genset = true;

			Assert(vgmMessagingContainer.Genset);

			container.HasControlledAtmosphere = false;

			Assert(!vgmMessagingContainer.Genset);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var context = new CommonContext(Factory);
			var sealPartyTypes = new CodeDescriptionPairList();

			var container = new Container
			{
				Number = "AAAA000007",
				Type = new ContainerType(context.ContainerTypes)
				{
					Code = "20GP"
				},
				ContainerCount = 1,
				IsEmpty = false,
				IsPartOf = true,
				Seal = "SEAL1",
				SealPartyType = new CodeDescription(sealPartyTypes)
				{
					Code = "AAA"
				},
				SecondSeal = "SEAL2",
				SecondSealPartyType = new CodeDescription(sealPartyTypes)
				{
					Code = "BBB"
				},
				ThirdSeal = "SEAL3",
				ThirdSealPartyType = new CodeDescription(sealPartyTypes)
				{
					Code = "CCC"
				},
				GoodsWeight = new Measurement
				{
					Value = 1000,
					Unit = new CodeDescription(context.WeightUnits)
					{
						Code = Core.Constants.Weight.Kilograms
					}
				},
				TareWeight = new Measurement
				{
					Value = 2000,
					Unit = new CodeDescription(context.WeightUnits)
					{
						Code = Core.Constants.Weight.Kilograms
					}
				},
				Dunnage = new Measurement
				{
					Value = 3000,
					Unit = new CodeDescription(context.WeightUnits)
					{
						Code = Core.Constants.Weight.Kilograms
					}
				},
				NetWeight = new Measurement
				{
					Value = 1000,
					Unit = new CodeDescription(context.WeightUnits)
					{
						Code = Core.Constants.Weight.Kilograms
					}
				},
				GrossWeight = new Measurement
				{
					Value = 6000,
					Unit = new CodeDescription(context.WeightUnits)
					{
						Code = Core.Constants.Weight.Kilograms
					}
				},
				PackingLines = System.Array.Empty<PackingLine>(),
				Numbers = System.Array.Empty<ReferenceNumber>(),
				AdditionalServices = System.Array.Empty<AdditionalService>(),
				Milestones = System.Array.Empty<Milestone>()
			};

			return new VGMMessagingContainer(container, null)
			{
				Statement = "Statement"
			};
		}

		#endregion
	}
}
