using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class AsycudaManifestHeaderSynchroniser : ASYCUDA.Business.AsycudaManifestHeaderSynchroniser
	{
		public AsycudaManifestHeaderSynchroniser(AsycudaManifestHeader destination, ForwardingConsol sourceConsol)
			: base(destination, sourceConsol)
		{
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			var transports = Source.MostInterestingTransportForBinding;
			if (transports != null && transports.Count > 0)
			{
				if (transports[0].JW_RL_NKDiscPortForBinding.SubstringSafe(0, 2) == Core.Constants.CountryCodes.Uruguay)
				{
					Synchronisers.Add(new FieldSynchroniser(Destination.AMA_DateAtCustomsOfficeInfo, transports[0].JW_ETAInfo));
				}
				else
				{
					Synchronisers.Add(new FieldSynchroniser(Destination.AMA_DateAtCustomsOfficeInfo, GetETA, GetSourceInfosAffectingETA));
				}
			}
		}

		#region ETA

		IZType GetETA()
		{
			IZType result;
			var transport = GetRelevantTransportByNature();
			if (transport != null)
			{
				result = transport.JW_ATA.IsEmpty ? transport.JW_ETA : transport.JW_ATA;
			}
			else
			{
				result = ZDateTime.Empty;
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetSourceInfosAffectingETA()
		{
			foreach (var info in ConsolDataCalculator.GetInfosAffectingTransportsOrder())
			{
				yield return info;
			}

			foreach (var info in ConsolDataCalculator.GetTransportsInfos(JobConsolTransportSchema.Constants.JW_ETA, JobConsolTransportSchema.Constants.JW_ATA))
			{
				yield return info;
			}

			yield return Destination.AMA_NatureInfo;
		}

		#endregion
	}
}
