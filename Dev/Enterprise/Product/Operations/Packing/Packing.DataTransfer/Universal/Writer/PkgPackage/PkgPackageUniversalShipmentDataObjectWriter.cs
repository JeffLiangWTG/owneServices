using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using WTG.RTUS.Interface;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Packing.DataTransfer.Universal
{
	public class PkgPackageUniversalShipmentDataObjectWriter : DataObjectWriter<PkgPackage, UniversalShipment>
	{
		public PkgPackageUniversalShipmentDataObjectWriter(IDataWritingManager manager, RequestType rtusRequest)
			: base(manager)
		{
			RTUSRequest = Argument.NotNull(rtusRequest, nameof(rtusRequest));
		}

		readonly RequestType RTUSRequest;

		#region GetDataObject

		internal UniversalShipment GetDataObject(Dictionary<ZGuid, Func<UniversalShipment>> shipmentDataObjectsByPackageJob, PkgPackage package)
		{
			var parentShipmentWriter = GetParentShipmentWriter(package);
			if (!shipmentDataObjectsByPackageJob.TryGetValue(package.KP_KJ_ParentPackageJob, out var result))
			{
				var parentDataObject = parentShipmentWriter.GetDataObject(package);
				shipmentDataObjectsByPackageJob[package.KP_KJ_ParentPackageJob] = result = () => CloneDelegateCache.Value.Invoke(parentDataObject);
			}

			UniversalShipment shipmentDataObject;

			if (result != null)
			{
				shipmentDataObject = result();
				PopulatePackageData(shipmentDataObject, package, parentShipmentWriter);
			}
			else
			{
				shipmentDataObject = null;
			}

			return shipmentDataObject;
		}

		static Lazy<Func<UniversalShipment, UniversalShipment>> CloneDelegateCache { get; } = new Lazy<Func<UniversalShipment, UniversalShipment>>(GetCloneDelegate);

		static Func<UniversalShipment, UniversalShipment> GetCloneDelegate()
		{
			var cloneMethodInfo = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
			var cloneDelegate = (Func<UniversalShipment, object>)Delegate.CreateDelegate(typeof(Func<UniversalShipment, object>), null, cloneMethodInfo);
			return shipmentDataObject => (UniversalShipment)cloneDelegate(shipmentDataObject);
		}

		#endregion

		#region PopulateDataObject

		protected override UniversalShipment PopulateDataObject(PkgPackage package)
		{
			var parentShipmentWriter = GetParentShipmentWriter(package);
			var shipmentDataObject = parentShipmentWriter.GetDataObject(package);
			PopulatePackageData(shipmentDataObject, package, parentShipmentWriter);

			return shipmentDataObject;
		}

		DataObjectWriter<PkgPackage, UniversalShipment> GetParentShipmentWriter(PkgPackage package)
		{
			var parentType = package.PackageJob.ParentJob.ParentJobType;
			return PackageParentJobMapper.GetWriter(parentType, writeManager);
		}

		void PopulatePackageData(UniversalShipment shipmentDataObject, PkgPackage package, DataObjectWriter<PkgPackage, UniversalShipment> parentShipmentWriter)
		{
			var originalDataContext = shipmentDataObject.DataContext;
			shipmentDataObject.DataContext = DataContextFactory.New(writeManager.Schema.Namespace); // need to add a new data source due to caching of parent UXML
			new DataContextDataObjectWriter().PopulateDataObject(writeManager.Action, shipmentDataObject.DataContext);

			if (originalDataContext != null)
			{
				originalDataContext.DataSourceCollection.ForEach(x => shipmentDataObject.DataContext.AddDataSource((DataContextType)Enum.Parse(typeof(DataContextType), x.Type), x.Key.GetValueOrDefault()));
			}

			var parentJob = package.PackageJob.ParentJob;
			var isHandlingUnit = package.IsClosed && parentJob is PkgHandlingUnit;

			shipmentDataObject.SetPackingLineCollection(() =>
			{
				PackingLine[] packages;

				if (RTUSRequest == RequestType.Cancellation)
				{
					packages = Array.Empty<PackingLine>();
				}
				else
				{
					var orderLinesDictionary = parentShipmentWriter is IOrderLineDictionaryProvider orderLineDictionaryProvider
						? orderLineDictionaryProvider.GetOrderLineDictionary()
						: null;
					var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, parentJob, orderLineDictionary: orderLinesDictionary);
					helper.PopulatePackableItems(shipmentDataObject, package.PackedItems.Typed.Select(p => p.PackableItemParent)); // this needs to come first in case the parent Job uses PackableItems
					var packageDataObject = helper.PopulatePkgPackageDataObject(package, 0, isHandlingUnit: isHandlingUnit);
					packages = new[] { packageDataObject };
				}

				return new DataObjectList<PackingLine>(packages)
				{
					Content = CollectionContent.Partial
				};
			});

			shipmentDataObject.SetSubShipmentCollection(() =>
			{
				DataObjectList<UniversalShipment> packageShipments = null;

				if (isHandlingUnit)
				{
					var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, parentJob);

					var innerPackages = package.HandlingUnitPackedPackages;
					var shipmentDataObjectsByPackageJob = new Dictionary<ZGuid, Func<UniversalShipment>>();
					packageShipments = new DataObjectList<UniversalShipment>(innerPackages.Select(p => GetPackageUniversalShipment(p, shipmentDataObjectsByPackageJob)));

					var dataSources = packageShipments
						.SelectMany(ps => ps.DataContext.DataSourceCollection)
						.Where(d => d.Type.Value != nameof(DataContextType.PkgPackage));
#if NETFRAMEWORK
					var distinctOnes = dataSources.DistinctBy(d => d.Key);
#elif NET
					var distinctOnes = Enumerable.DistinctBy(dataSources, d => d.Key);

#else
#error Unexpected target platform
#endif
					distinctOnes.ForEach(x => shipmentDataObject.DataContext.AddDataSource((DataContextType)Enum.Parse(typeof(DataContextType), x.Type), x.Key.GetValueOrDefault()));
				}

				return packageShipments;
			});
		}

		UniversalShipment GetPackageUniversalShipment(PkgPackage package, Dictionary<ZGuid, Func<UniversalShipment>> shipmentDataObjectsByPackageJob)
		{
			var packageWriteManager = new DataWritingManager(new ActionInfo(null, package));
			return new PkgPackageUniversalShipmentDataObjectWriter(packageWriteManager, RTUSRequest).GetDataObject(shipmentDataObjectsByPackageJob, package);
		}

#endregion
	}
}
