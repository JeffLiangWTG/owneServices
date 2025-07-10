using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding
{
	public class ForwardingContainerSplitter : IDisposable
	{
		#region Ctor

		public ForwardingContainerSplitter(ForwardingContainer multiContainer)
		{
			Argument.NotNull(multiContainer, "multiContainer cannot be null");
			MultiContainer = multiContainer;
		}

		#endregion

		#region Nested Types

		public enum SplitMethod
		{
			None,
			PackAllIntoFirstContainer,
			DistributeEvenly
		}

		class Check
		{
			public Func<ForwardingContainer, bool> Method { get; set; }
			public ZString ErrorMessage { get; set; }
		}

		#endregion

		#region Properties

		public BusinessObjectFactory Factory { get { return MultiContainer != null ? MultiContainer.Factory : null; } }
		public ForwardingContainer MultiContainer { get; private set; }

		public Func<SplitMethod> SplitMethodProvider
		{
			get { return splitMethodProvider ?? (() => SplitMethod.None); }
			set { splitMethodProvider = value; }
		}
		Func<SplitMethod> splitMethodProvider;

		#endregion

		#region Preconditions

		ZString CheckIfContainerCanBeSplit(ForwardingContainer container)
		{
			List<Check> checks = new List<Check>
			{
				new Check { Method = ContainerIsMultiContainer, ErrorMessage = Res.GetString("7dc20b6e-05bf-4b40-89fa-3a310850df3e", "Selected container is not a multi-container") },
				new Check { Method = ContainerPackLinesHaveNoProducts, ErrorMessage = Res.GetString("ea5b82b8-d464-44c1-9051-e593ccc5237e", "Container cannot have any products packed") },
				new Check { Method = ContainerDangerousGoodsFillTheWholePackLine, ErrorMessage = Res.GetString("47aa7fca-259f-4bcd-b8b9-ac1cd0ef5872", "The Split functionality is not allowed due to insufficient information on dangerous goods packline/s") }
			};

			return checks.Where(check => !check.Method(container)).Select(check => check.ErrorMessage).FirstOrDefault();
		}

		bool ContainerIsMultiContainer(ForwardingContainer container)
		{
			return container.JC_ContainerCount > 1;
		}

		bool ContainerPackLinesHaveNoProducts(ForwardingContainer container)
		{
			return container.PackLines.Cast<ForwardingPackLine>().All(packLine => packLine.Products.Count == 0);
		}

		bool ContainerDangerousGoodsFillTheWholePackLine(ForwardingContainer container)
		{
			return container.PackLines.Cast<ForwardingPackLine>().All(DangerousGoodsFillTheWholePackLine);
		}

		bool DangerousGoodsFillTheWholePackLine(ForwardingPackLine packLine)
		{
			switch (packLine.UNDGs.Count)
			{
				case 0:
					return true;
				case 1:
					bool dngFillsTheWholePackLine = false;

					ZVolume dngVolume = new ZVolume(packLine.UNDGs[0].DI_DGVolume, packLine.UNDGs[0].DI_UnitOfVolume);
					ZWeight dngWeight = new ZWeight(packLine.UNDGs[0].DI_DGWeight, packLine.UNDGs[0].DI_UnitOfWeight);

					if (dngVolume.IsEmpty && dngWeight.IsEmpty)
					{
						dngFillsTheWholePackLine = true;
					}
					else
					{
						ZVolume packLineVolume = new ZVolume(packLine.JL_ActualVolume, packLine.JL_ActualVolumeUQ);
						ZWeight packLineWeight = new ZWeight(packLine.JL_ActualWeight, packLine.JL_ActualWeightUQ);

						dngFillsTheWholePackLine = packLineVolume.IsValid && dngVolume.IsValid && packLineVolume == dngVolume && packLineWeight == dngWeight;
					}

					return dngFillsTheWholePackLine;
				default:
					return false;
			}
		}

		#endregion

		#region Split

		public ZString Split()
		{
			ZString checkErrorMessage = CheckIfContainerCanBeSplit(MultiContainer);

			if (checkErrorMessage != ZString.Empty)
			{
				return checkErrorMessage;
			}

			SplitMethod splitMethod = SplitMethodProvider();

			if (splitMethod == SplitMethod.PackAllIntoFirstContainer)
			{
				ForwardingContainer[] newContainers = CreateContainers(MultiContainer, MultiContainer.JC_ContainerCount - 1, splitMethod);
				PackAllIntoFirstContainer(newContainers, MultiContainer);
			}
			else if (splitMethod == SplitMethod.DistributeEvenly)
			{
				ForwardingContainer[] newContainers = CreateContainers(MultiContainer, MultiContainer.JC_ContainerCount, splitMethod);
				PackContainersEvenly(newContainers, MultiContainer);
			}

			return ZString.Empty;
		}

		ForwardingContainer[] CreateContainers(ForwardingContainer templateContainer, int numberOfContainersToCreate, SplitMethod splitMethod)
		{
			var result = new List<ForwardingContainer>();
			var propertiesToBeExcluded = new List<string> { ForwardingContainer.Schema.JC_ContainerCount, ForwardingContainer.Schema.JC_TareWeight };

			for (int i = 0; i < numberOfContainersToCreate; i++)
			{
				if (splitMethod == SplitMethod.PackAllIntoFirstContainer || i > 0)
				{
					propertiesToBeExcluded.Add(ForwardingContainer.Schema.JC_ContainerJobID);
				}

				var containerCloneArgs = new BusinessObjectCloneArgs(propertiesToBeExcluded);
				var container = (ForwardingContainer)templateContainer.Clone(containerCloneArgs);

				result.Add(container);
			}

			return result.ToArray();
		}

		void PackContainersEvenly(ForwardingContainer[] containers, ForwardingContainer multiContainer)
		{
			ForwardingPackLine[] multiContainerPackLines = multiContainer.PackLines.Cast<ForwardingPackLine>().ToArray();

			BusinessObjectCloneArgs packLineCloneArgs = new BusinessObjectCloneArgs(new[]
			{
				ForwardingPackLine.Schema.JL_ActualWeight, ForwardingPackLine.Schema.JL_ActualVolume, ForwardingPackLine.Schema.JL_PackageCount,
				ForwardingPackLine.Schema.DangerousGoodsCollection, ForwardingPackLine.Schema.ProductsCollection
			});

			BusinessObjectCloneArgs dangerousGoodCloneArgs = new BusinessObjectCloneArgs(new[]
			{
				UNDGDataItem.Schema.DI_ParentID, UNDGDataItem.Schema.DI_ParentTableCode, UNDGDataItem.Schema.DI_DGWeight, UNDGDataItem.Schema.DI_DGVolume
			});

			ForwardingConsol consol = multiContainer.Consol;

			foreach (ForwardingPackLine multiContainerPackLine in multiContainerPackLines)
			{
				ForwardingShipment shipment = multiContainerPackLine.Shipment;

				decimal totalValue = 0m;
				int valuePrecission = 0;
				decimal minAllowedValue = 0m;

				if (multiContainerPackLine.JL_PackageCount > 0)
				{
					totalValue = multiContainerPackLine.JL_PackageCount;
					valuePrecission = 0;
					minAllowedValue = 1m;
				}
				else if (multiContainerPackLine.JL_ActualWeight > 0)
				{
					totalValue = multiContainerPackLine.JL_ActualWeight;
					valuePrecission = 3;
					minAllowedValue = 0.001m;
				}
				else
				{
					totalValue = multiContainerPackLine.JL_ActualVolume;
					valuePrecission = 3;
					minAllowedValue = 0.001m;
				}

				int totalNumberOfDistributions = containers.Length;
				decimal allocatedValue = 0;
				int allocatedNumberOfDistributions = 0;

				int packagesPerUnit = totalValue > 0m ? Convert.ToInt32(multiContainerPackLine.JL_PackageCount / totalValue) : 0;
				decimal weightPerUnit = totalValue > 0m ? multiContainerPackLine.JL_ActualWeight / totalValue : 0m;
				decimal volumePerUnit = totalValue > 0m ? multiContainerPackLine.JL_ActualVolume / totalValue : 0m;

				foreach (ForwardingContainer container in containers)
				{
					ForwardingPackLine packLine = (ForwardingPackLine)multiContainerPackLine.Clone(packLineCloneArgs);

					int reminingNumberOfDistributions = totalNumberOfDistributions - allocatedNumberOfDistributions;

					decimal units = reminingNumberOfDistributions > 0m ? Utilities.Round((totalValue - allocatedValue) / reminingNumberOfDistributions, valuePrecission) : 0m;

					if (units * reminingNumberOfDistributions < totalValue - allocatedValue)
					{
						units += minAllowedValue;
					}

					allocatedValue += units;
					allocatedNumberOfDistributions++;

					packLine.JL_PackageCount = (int)Utilities.Round(packagesPerUnit * units, 0);
					packLine.JL_ActualWeight = Utilities.Round(weightPerUnit * units, 3);
					packLine.JL_ActualVolume = Utilities.Round(volumePerUnit * units, 3);

					packLine.JL_OriginTransitWarehouseStatus = multiContainerPackLine.JL_OriginTransitWarehouseStatus;
					packLine.JL_DepartureTransitWarehouseExcluded = multiContainerPackLine.JL_DepartureTransitWarehouseExcluded;
					packLine.JL_OA_LastKnownTransitWarehouseAddress = multiContainerPackLine.JL_OA_LastKnownTransitWarehouseAddress;
					packLine.JL_LastKnownTransitWarehouseStatus = multiContainerPackLine.JL_LastKnownTransitWarehouseStatus;
					packLine.JL_LastKnownTransitWarehouseStatusDateTime = multiContainerPackLine.JL_LastKnownTransitWarehouseStatusDateTime;

					if (multiContainerPackLine.UNDGs.Count == 1)
					{
						UNDGDataItem dangerousGood = (UNDGDataItem)multiContainerPackLine.UNDGs[0].Clone(dangerousGoodCloneArgs);

						decimal dgVolumeSplitFactor = multiContainerPackLine.JL_ActualVolume > 0 ? packLine.JL_ActualVolume / multiContainerPackLine.JL_ActualVolume : 0m;
						decimal dgWeightSplitFactor = multiContainerPackLine.JL_ActualWeight > 0 ? packLine.JL_ActualWeight / multiContainerPackLine.JL_ActualWeight : 0m;

						dangerousGood.DI_DGWeight = Utilities.Round(multiContainerPackLine.UNDGs[0].DI_DGWeight * dgWeightSplitFactor, 3);
						dangerousGood.DI_DGVolume = Utilities.Round(multiContainerPackLine.UNDGs[0].DI_DGVolume * dgVolumeSplitFactor, 3);

						packLine.UNDGs.Add(dangerousGood);
					}

					shipment.OuterPackLines.Add(packLine);

					if (consol != null)
					{
						packLine.SetContainer(consol, null);
					}

					packLine.SetContainer(container.PK);
				}

				shipment.OuterPackLines.RemoveAndDelete(multiContainerPackLine);
			}

			SplitTareWeight(containers, multiContainer.JC_TareWeight);

			if (consol != null)
			{
				consol.Containers.AddRange(containers);
				consol.Containers.RemoveAndDelete(multiContainer);
			}
		}

		void PackAllIntoFirstContainer(ForwardingContainer[] containers, ForwardingContainer multiContainer)
		{
			PackLine[] multiContainerPackLines = multiContainer.PackLines.Cast<PackLine>().ToArray();

			BusinessObjectCloneArgs packLineCloneArgs = new BusinessObjectCloneArgs(new[]
			{
				ForwardingPackLine.Schema.JL_ActualWeight, ForwardingPackLine.Schema.JL_ActualVolume, ForwardingPackLine.Schema.JL_PackageCount,
				ForwardingPackLine.Schema.DangerousGoodsCollection, ForwardingPackLine.Schema.ProductsCollection
			});

			ForwardingConsol consol = multiContainer.Consol;

			foreach (PackLine multiContainerPackLine in multiContainerPackLines)
			{
				ForwardingShipment shipment = (ForwardingShipment)multiContainerPackLine.Shipment;

				foreach (ForwardingContainer container in containers)
				{
					ForwardingPackLine packLine = (ForwardingPackLine)multiContainerPackLine.Clone(packLineCloneArgs);

					shipment.OuterPackLines.Add(packLine);

					if (consol != null)
					{
						packLine.SetContainer(consol, null);
					}

					packLine.SetContainer(container.PK);
				}
			}

			ZDecimal tareWeight = multiContainer.JC_TareWeight;
			multiContainer.JC_ContainerCount = 1;
			SplitTareWeight(containers.Concat(new[] { multiContainer }).ToArray(), tareWeight);

			if (consol != null)
			{
				consol.Containers.AddRange(containers);
			}
		}

		void SplitTareWeight(ForwardingContainer[] containers, ZDecimal tareWeightToSplit)
		{
			foreach (ForwardingContainer container in containers)
			{
				container.JC_TareWeight = tareWeightToSplit / containers.Length;
			}
		}

		#endregion

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				SplitMethodProvider = null;
			}
		}
	}
}
