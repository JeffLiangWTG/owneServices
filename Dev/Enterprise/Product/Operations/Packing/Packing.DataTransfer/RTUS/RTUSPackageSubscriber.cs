using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Core;
using WTG.RTUS.Interface;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Packing.DataTransfer
{
	public class RTUSPackageSubscriber
	{
		public ReturnResult SubscribePackage<T>(PkgPackage package, RequestType<T> requestType)
			 where T : IRTUSResponse
		{
			Argument.NotNull(package, nameof(package));

			var error = new ZStringBuilder();
			var packageUniversalShipment = GetPackageUniversalShipment();
			RequestXMLsByPackage[package] = new RTUSRequestXml(package.KP_PackageID, package.KP_KJ_ParentPackageJob, package.PackageJob.ParentJob, packageUniversalShipment);

			UniversalShipment GetPackageUniversalShipment()
			{
				var writeManager = new DataWritingManager(new ActionInfo(null, package));

				try
				{
					return new PkgPackageUniversalShipmentDataObjectWriter(writeManager, requestType).GetDataObject(GetParentShipmentsByPackageJob, package);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					// Since this package job caused an error when generating the Universal Shipment we
					// set an entry in the dictionary for this job so that we don't throw an error again.
					GetParentShipmentsByPackageJob[package.KP_KJ_ParentPackageJob] = null;
					error.Append(ex.Message);
				}

				return null;
			}

			if (packageUniversalShipment == null)
			{
				if (!error.IsEmpty)
				{
					error.Prepend(Res.GetString("924d32fd-982b-4c3d-861c-12e9096845cd", "Details below:"));
				}

				error.Prepend(Res.GetString("4924c4c2-138d-4f84-a392-7f221a3dcac5", "Failed to generate Universal Shipment from Package '{0}'.", package.KP_PackageID));
			}

			return new ReturnResult { Success = error.IsEmpty, Message = error.ToStringWithNewLineBetweenAppends() };
		}

		public bool IsPackageSubscribed(PkgPackage package) => RequestXMLsByPackage.ContainsKey(package);

		public RTUSRequestXml GetRequestXML(PkgPackage package) => RequestXMLsByPackage.TryGetValue(package, out var rtusRequestXML) ? rtusRequestXML : new RTUSRequestXml(package.KP_PackageID, package.KP_KJ_ParentPackageJob, null, null);

		Dictionary<ZGuid, Func<UniversalShipment>> GetParentShipmentsByPackageJob { get; } = new Dictionary<ZGuid, Func<UniversalShipment>>();
		Dictionary<PkgPackage, RTUSRequestXml> RequestXMLsByPackage { get; } = new Dictionary<PkgPackage, RTUSRequestXml>();
	}
}
