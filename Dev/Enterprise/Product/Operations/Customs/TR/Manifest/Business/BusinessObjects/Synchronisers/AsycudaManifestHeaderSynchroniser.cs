using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class AsycudaManifestHeaderSynchroniser : ASYCUDA.Business.AsycudaManifestHeaderSynchroniser
	{
		public AsycudaManifestHeaderSynchroniser(AsycudaManifestHeader destination, ForwardingConsol sourceConsol)
			: base(destination, sourceConsol)
		{
		}

		protected override void SynchronisersAMA_VesselName()
		{
			if (!IsOnlyForSeaAndGrupaj)
			{
				base.SynchronisersAMA_VesselName();
			}
		}

		protected override void SynchronisersAMA_RL_NKPortOfLoading()
		{
			if (!IsOnlyForSeaAndGrupaj)
			{
				base.SynchronisersAMA_RL_NKPortOfLoading();
			}
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			Synchronisers.Add(new FieldSynchroniser(Destination.AMA_E_ARVInfo, Source.MostInterestingTransportForBinding[0].JW_ETDForBindingInfo));
			if (!IsOnlyForSeaAndGrupaj)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.AMA_CustomsLoadPortInfo, Source.JK_JX_JA_RL_NKPortOfLoadingInfo));
			}
			Synchronisers.Add(new FieldSynchroniser(Destination.AMA_CustomsOfficeInfo, () => GetOfficeCode(Source), () => GetSourceValuesAffectingMoveInOfficeCode(Source)));
		}

		IZType GetOfficeCode(ForwardingConsol consol)
		{
			var newOfficeCode = Destination.CalculateCustomsOfficeCode(consol.JK_JX_JB_RL_NKPortOfDischarge, consol.TransportMode);
			return newOfficeCode.Left(AsycudaManifestHeader.Schema.AMA_CustomsOfficeMaxLength);
		}

		IEnumerable<ZPropertyInfo> GetSourceValuesAffectingMoveInOfficeCode(ForwardingConsol consol)
		{
			yield return consol.JK_JX_JB_RL_NKPortOfDischargeInfo;
		}

		ZBool IsOnlyForSeaAndGrupaj => Source.JK_TransportMode == Core.Constants.TransportModes.Sea && Destination.AMA_ManifestType == TRManifestTypes.Codes.GRUPAJ;

		protected override BusinessObjectCollectionSynchroniser GetNewBillCollectionSynchroniser()
		{
			return new AsycudaBillCollectionSynchroniser((AsycudaManifestHeader)Destination);
		}
	}
}
