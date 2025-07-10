using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public static class ContainerPackLineRelationshipHelper
	{
		public static void PackLineAddedToContainer(CommonContainer container, PackLine packLine)
		{
			if (!container.IsDeleted && !packLine.IsDeleted)
			{
				PackLineAddedToContainerCore(container, packLine);
				EnsurePackLineIsInOneContainerPerConsol(container, packLine);
			}
#if DEBUG
			RegisterCall("PackLineAddedToContainer");
#endif
		}

		static void PackLineAddedToContainerCore(CommonContainer container, PackLine packLine)
		{
			var shipment = packLine.Shipment;
			if (shipment != null)
			{
				UpdateAvailabilityAndStorageDates(shipment);
				AddContainerToShipment(shipment, container);
				SetHighestContainerPackingOrder(container, packLine);
			}

			UpdateContainerGrossWeight(container);
		}

		static void EnsurePackLineIsInOneContainerPerConsol(CommonContainer packingContainer, PackLine packLine)
		{
			var otherContainersPackingPackLineOnSameConsol = packLine.Containers.Cast<CommonContainer>()
				.Where(container =>
					container.PK != packingContainer.PK
					&& container.JC_JK.IsValid
					&& container.JC_JK == packingContainer.JC_JK)
				.ToArray();

			foreach (var otherContainer in otherContainersPackingPackLineOnSameConsol)
			{
				packLine.Containers.Remove(otherContainer);
			}
		}

		public static void PackLineRemovedFromContainer(CommonContainer container, PackLine packLine)
		{
			if (!container.IsDeleted && !packLine.IsDeleted)
			{
				PackLineRemovedFromContainerCore(container, packLine);
			}

#if DEBUG
			RegisterCall("PackLineRemovedFromContainer");
#endif
		}

		static void PackLineRemovedFromContainerCore(CommonContainer container, PackLine packLine)
		{
			if (packLine.Shipment != null)
			{
				UpdateAvailabilityAndStorageDates(packLine.Shipment);
				ResetContainerPackingOrder(packLine);
			}

			UpdateContainerGrossWeight(container);
		}

		public static IEnumerable<JobContainerPackPivot> GetContainerPackPivotsWithAbsentConShipLink(BusinessObjectFactory factory, CommonContainer container, PackLine packLine)
		{
			if (container == null && packLine == null)
			{
				return Enumerable.Empty<JobContainerPackPivot>();
			}

			var query = new ZQuery { FetchOnlyFromLocalCache = true };

			if (container != null)
			{
				query.AddToFilter(JobContainerPackPivotSchema.J6_JC, container.PK);
			}

			if (packLine != null)
			{
				query.AddToFilter(JobContainerPackPivotSchema.J6_JL, packLine.PK);
			}

			var containerPackPivots = factory.Load<JobContainerPackPivot>(query);
			return containerPackPivots.Where(pivot =>
				!pivot.J6_JL.IsEmpty
				&& pivot.PackLine.Shipment != null
				&& !pivot.J6_JC.IsEmpty
				&& pivot.Container.Consol != null
				&& IsConShipLinkAbsent(pivot.PackLine, pivot.Container, factory));
		}

		public static void ReportAbsentConShipLink(string key, ZGuid packLinePK, ZGuid containerPK, BusinessObjectFactory factory)
		{
			if (!ErrorReporter.HasBeenReported(key) && packLinePK.IsValid && containerPK.IsValid)
			{
				var packLine = factory.Load<PackLine>(packLinePK);

				if (packLine != null)
				{
					var container = factory.Load<CommonContainer>(containerPK);
					if (container != null)
					{
						ReportAbsentConShipLink(key, packLine, container, factory);
					}
				}
			}
		}

		public static bool IsConShipLinkAbsent(PackLine packLine, CommonContainer container, BusinessObjectFactory factory)
		{
			var query = new ZQuery(JobConShipLinkSchema.JN_JS, packLine.Shipment.PK);
			query.AddToFilter(JobConShipLinkSchema.JN_JK, container.Consol.PK);

			return factory.LoadTop1<JobConShipLink>(query) == null;
		}

		#region SuppressResourceStringsCheckRegion

		public static void ReportAbsentConShipLink(string key, PackLine packLine, CommonContainer container, BusinessObjectFactory factory)
		{
			if (!ErrorReporter.HasBeenReported(key)
				&& container.Consol != null
				&& !container.Consol.IsMultiAWBMaster
				&& packLine.Shipment != null
				&& (packLine.Shipment.JS_IsForwardRegistered || packLine.Shipment.JS_IsCFSRegistered))
			{
				if (IsConShipLinkAbsent(packLine, container, factory))
				{
					var shipmentNumber = GetUniqueConsignRefAndPK(packLine.Shipment.JS_UniqueConsignRef, packLine.Shipment.PK);
					var consolNumber = GetUniqueConsignRefAndPK(container.Consol.JK_UniqueConsignRef, container.Consol.PK);

					var msg = new ZStringBuilder();
					msg.AppendFormat("Shipment {0}'s PackLine {{{3}}} is being allocated to the Container {1} belonging to the Consol {2} however no corresponding JobConShipLink exists. Please inform IL team. WI00035124",
						shipmentNumber,
						container.ContainerCode,
						consolNumber,
						packLine.PK.ToString()).AppendLine();

					msg.AppendLine("Packline Factory Instance:" + packLine.Factory._Instance.ToString());
					msg.AppendLine("Container Factory Instance:" + container.Factory._Instance.ToString());
					msg.AppendLine("Base Factory Instance:" + factory._Instance.ToString());

					foreach (var f in new[] { packLine.Factory, container.Factory, factory }.Distinct())
					{
						msg.AppendLine("Validating factory instance:" + f._Instance.ToString());
						msg.Append(LogShipmentAndConsolInFactory(packLine.Shipment, container, f));
					}

					msg.Append(GetCoLoadMasterShipmentSummary(packLine.Shipment));
					msg.Append(GetRecordInfoFromDB(packLine, container));
					msg.AppendLine();

					if (!string.IsNullOrEmpty(packLine.CurrentConsolSetterStackTrace))
					{
						msg.Append(packLine.CurrentConsolSetterStackTrace);
					}

					ErrorReporter.ReportOnce(key, msg.ToString());
				}
			}
		}

		static ZString LogShipmentAndConsolInFactory(CommonShipment shipment, CommonContainer container, BusinessObjectFactory factory)
		{
			var msg = new ZStringBuilder();
			var shipmentNumber = GetUniqueConsignRefAndPK(shipment.JS_UniqueConsignRef, shipment.PK);
			shipment = factory.Load<CommonShipment>(shipment.PK);
			msg.AppendFormat("Shipment {0} does {1}exist in factory {2};",
				shipmentNumber,
				shipment == null ? "not " : "",
				factory._Instance.ToString()).AppendLine();

			if (shipment != null)
			{
				msg.Append(shipmentNumber + "'s Consols:");
				shipment.Consols.Cast<CommonConsol>().ForEach(c => msg.AppendFormat(" {0};", GetUniqueConsignRefAndPK(c.JK_UniqueConsignRef, c.PK)));
				msg.AppendLine();
			}

			var consolNumber = GetUniqueConsignRefAndPK(container.Consol.JK_UniqueConsignRef, container.Consol.PK);
			container = factory.Load<CommonContainer>(container.PK);
			msg.AppendFormat("Consol {0} does {1}exist in factory {2};",
				consolNumber,
				container == null ? "not " : "",
				factory._Instance.ToString()).AppendLine();

			if (container != null)
			{
				msg.Append(consolNumber + "'s Shipments:");
				container.Consol.Shipments.Cast<CommonShipment>().ForEach(s => msg.AppendFormat(" {0};", GetUniqueConsignRefAndPK(s.JS_UniqueConsignRef, s.PK)));
				msg.AppendLine();
				msg.Append(consolNumber + "'s Containers:");
				container.Consol.Containers.Cast<CommonContainer>().ForEach(c => msg.AppendFormat(" {0} {{{1}}};", c.ContainerCode, c.PK.ToString()));
				msg.AppendLine();
			}

			return msg.ToString();
		}

		static ZString GetUniqueConsignRefAndPK(ZString uniqueConsignRef, ZGuid pk)
		{
			return ZString.Format("{0} {{{1}}}", uniqueConsignRef, pk);
		}

		static ZString GetCoLoadMasterShipmentSummary(CommonShipment shipment)
		{
			var msg = new ZStringBuilder();

			var shipmentNumber = GetUniqueConsignRefAndPK(shipment.JS_UniqueConsignRef, shipment.PK);
			msg.AppendFormat("Shipment {0} Type: {1}, JS_JS_ColoadMasterShipment: {2};",
				shipmentNumber,
				shipment.JS_ShipmentType,
				shipment.JS_JS_ColoadMasterShipment.ToString()).AppendLine();

			var masterShipment = shipment.CoLoadMasterShipment;
			if (masterShipment != null)
			{
				msg.AppendLine(GetCoLoadMasterShipmentSummary(masterShipment));
			}

			return msg.ToString();
		}

		static string GetRecordInfoFromDB(PackLine packLine, CommonContainer container)
		{
			var shipment = packLine.Shipment;
			var consol = container.Consol;

			var msg = new ZStringBuilder();
			msg.AppendLine("Record information from DB:");

			var factory = new BusinessObjectFactory();

			msg.AppendFormat("Shipment {{{0}}}:", shipment.PK.ToString()).AppendLine();
			foreach (DynamicBusinessObject consolBo in GetLinkedConsolCollection(factory, shipment.PK))
			{
				msg.AppendFormat("   linked consol: JN_JK = {{{0}}};",
					consolBo[JobConShipLinkSchema.JN_JK.Name].ToString()).AppendLine();
			}
			foreach (DynamicBusinessObject shipmentBo in GetMasterShipmentCollection(factory, shipment.PK))
			{
				msg.AppendFormat("   master shipment: {0} {{{1}}} Type: {2}, JS_JS_ColoadMasterShipment: {3};",
					shipmentBo[JobShipmentSchema.JS_UniqueConsignRef.Name].ToString(),
					shipmentBo[JobShipmentSchema.PK.Name].ToString(),
					shipmentBo[JobShipmentSchema.JS_ShipmentType.Name].ToString(),
					shipmentBo[JobShipmentSchema.JS_JS_ColoadMasterShipment.Name].ToString()).AppendLine();
			}
			foreach (DynamicBusinessObject packLineBo in GetPackLineCollectionFromShipment(factory, shipment.PK))
			{
				msg.AppendFormat("   packline: {{{0}}};",
					packLineBo[JobPackLinesSchema.PK.Name].ToString()).AppendLine();
			}

			msg.AppendFormat("Consol {{{0}}}:", consol.PK.ToString()).AppendLine();
			foreach (DynamicBusinessObject shipmentBo in GetLinkedShipmentCollection(factory, consol.PK))
			{
				msg.AppendFormat("   linked shipment: JN_JS = {{{0}}};",
					shipmentBo[JobConShipLinkSchema.JN_JS.Name].ToString()).AppendLine();
			}
			foreach (DynamicBusinessObject containerBo in GetContainerCollectionFromConsol(factory, consol.PK))
			{
				msg.AppendFormat("   containers: {0} {{{1}}} JC_ContainerMode: {2};",
					containerBo[JobContainerSchema.JC_ContainerNum.Name].ToString(),
					containerBo[JobContainerSchema.PK.Name].ToString(),
					containerBo[JobContainerSchema.JC_ContainerMode.Name].ToString()).AppendLine();
			}

			msg.AppendFormat("PackLine {{{0}}}:", packLine.PK.ToString()).AppendLine();
			foreach (DynamicBusinessObject containerBo in GetLinkedContainerCollection(factory, packLine.PK))
			{
				msg.AppendFormat("   linked containers: J6_JC = {{{0}}};",
					containerBo[JobContainerPackPivotSchema.J6_JC.Name].ToString()).AppendLine();
			}

			msg.AppendFormat("Container {{{0}}}:", container.PK.ToString()).AppendLine();
			foreach (DynamicBusinessObject packLineBo in GetLinkedPackLineCollection(factory, shipment.PK))
			{
				msg.AppendFormat("   linked packline: J6_JL = {{{0}}};",
					packLineBo[JobContainerPackPivotSchema.J6_JL.Name].ToString()).AppendLine();
			}

			return msg.ToString();
		}

		static DynamicBusinessObjectCollection GetLinkedConsolCollection(BusinessObjectFactory factory, ZGuid shipmentPK)
		{
			var sql = @"SELECT JN_JK
FROM dbo.JobConShipLink
WHERE JN_JS = @ShipmentPK";

			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sql, new[] { ZSqlParameter.New("@ShipmentPK", shipmentPK, JobShipmentSchema.PK) });
			return collection;
		}

		static DynamicBusinessObjectCollection GetMasterShipmentCollection(BusinessObjectFactory factory, ZGuid shipmentPK)
		{
			var sql = @"WITH ShipmentMasters AS
(
	SELECT JS_PK, JS_JS_ColoadMasterShipment, JS_UniqueConsignRef, JS_ShipmentType
	FROM dbo.JobShipment
	WHERE JS_PK = @ShipmentPK

	UNION ALL

	SELECT s1.JS_PK, s1.JS_JS_ColoadMasterShipment, s1.JS_UniqueConsignRef, s1.JS_ShipmentType
	FROM dbo.JobShipment s1
	JOIN ShipmentMasters s2 ON s1.JS_PK = s2.JS_JS_ColoadMasterShipment
)
SELECT JS_PK, JS_JS_ColoadMasterShipment, JS_UniqueConsignRef, JS_ShipmentType
FROM ShipmentMasters";

			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sql, new[] { ZSqlParameter.New("@ShipmentPK", shipmentPK, JobShipmentSchema.PK) });
			return collection;
		}

		static DynamicBusinessObjectCollection GetPackLineCollectionFromShipment(BusinessObjectFactory factory, ZGuid shipmentPK)
		{
			var sql = @"SELECT JL_PK
FROM dbo.JobPackLines
WHERE JL_JS = @ShipmentPK";

			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sql, new[] { ZSqlParameter.New("@ShipmentPK", shipmentPK, JobShipmentSchema.PK) });
			return collection;
		}

		static DynamicBusinessObjectCollection GetLinkedShipmentCollection(BusinessObjectFactory factory, ZGuid consolPK)
		{
			var sql = @"SELECT JN_JS
FROM dbo.JobConShipLink
WHERE JN_JK = @ConsolPK";

			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sql, new[] { ZSqlParameter.New("@ConsolPK", consolPK, JobConsolSchema.PK) });
			return collection;
		}

		static DynamicBusinessObjectCollection GetContainerCollectionFromConsol(BusinessObjectFactory factory, ZGuid consolPK)
		{
			var sql = @"SELECT JC_ContainerNum, JC_PK, JC_ContainerMode
FROM dbo.JobContainer
WHERE JC_JK = @ConsolPK";

			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sql, new[] { ZSqlParameter.New("@ConsolPK", consolPK, JobConsolSchema.PK) });
			return collection;
		}

		static DynamicBusinessObjectCollection GetLinkedContainerCollection(BusinessObjectFactory factory, ZGuid packLinePK)
		{
			var sql = @"SELECT J6_JC
FROM dbo.JobContainerPackPivot
WHERE J6_JL = @PackLinePK";

			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sql, new[] { ZSqlParameter.New("@PackLinePK", packLinePK, JobPackLinesSchema.PK) });
			return collection;
		}

		static DynamicBusinessObjectCollection GetLinkedPackLineCollection(BusinessObjectFactory factory, ZGuid containerPK)
		{
			var sql = @"SELECT J6_JL
FROM dbo.JobContainerPackPivot
WHERE J6_JC = @ContainerPK";

			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sql, new[] { ZSqlParameter.New("@ContainerPK", containerPK, JobContainerSchema.PK) });
			return collection;
		}

		#endregion

		#region UpdateAvailabilityAndStorageDates

		static void UpdateAvailabilityAndStorageDates(CommonShipment shipment)
		{
			shipment.DocsAndCartage.UpdateAvailabilityAndStorageDatesForContainers();
		}

		#endregion

		#region UpdateContainerGrossWeight

		static void UpdateContainerGrossWeight(CommonContainer container)
		{
			container.SetGrossWeightFromCombinedWeights();
		}

		#endregion

		#region AddContainerToShipment

		static void AddContainerToShipment(CommonShipment shipment, CommonContainer container)
		{
			shipment.DocsAndCartage.Services.SetServicesOfThisToMatchOther(container.Services);
			shipment.OnContainerAdded(container);
		}

		#endregion

		#region ContainerPackingOrder

		static void SetHighestContainerPackingOrder(CommonContainer container, PackLine packLine)
		{
			if (packLine.Containers.Count == 1 && !container.CreatedFromCusContainer)
			{
				int nextOrder = 1;
				foreach (PackLine existingPackLine in container.PackLines)
				{
					if (existingPackLine.PK != packLine.PK &&
							existingPackLine.JL_ContainerPackingOrder >= nextOrder)
					{
						nextOrder = existingPackLine.JL_ContainerPackingOrder + 1;
					}
				}

				packLine.JL_ContainerPackingOrder = nextOrder;
			}
		}

		static void ResetContainerPackingOrder(PackLine packLine)
		{
			if (packLine.Containers.Count == 0)
			{
				packLine.JL_ContainerPackingOrder = 0;
			}
		}

		#endregion

#if DEBUG

		static void RegisterCall(string key)
		{
			if (counter != null)
			{
				counter.Register(key);
			}
		}

		internal static Counter GetNewCallCounter()
		{
			counter = new Counter();
			return counter;
		}

		[ThreadStatic]
		static Counter counter;

		internal class Counter : IDisposable
		{
			readonly Dictionary<string, int> counterDictionary = new Dictionary<string, int>();

			public void Register(string key)
			{
				if (counterDictionary.ContainsKey(key))
				{
					counterDictionary[key] = counterDictionary[key] + 1;
				}
				else
				{
					counterDictionary.Add(key, 1);
				}
			}

			public int GetTotalCalls(string key)
			{
				return counterDictionary.ContainsKey(key)
					? counterDictionary[key]
					: 0;
			}

			void IDisposable.Dispose()
			{
				ContainerPackLineRelationshipHelper.counter = null;
			}
		}

#endif
	}
}
