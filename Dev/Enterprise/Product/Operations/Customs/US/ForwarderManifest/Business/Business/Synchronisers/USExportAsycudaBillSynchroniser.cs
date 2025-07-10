using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class USExportAsycudaBillSynchroniser : ASYCUDA.Business.AsycudaBillSynchroniser
	{
		public USExportAsycudaBillSynchroniser(USExportAsycudaBill destination, ForwardingShipment source)
			: base(destination, source)
		{
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (Source.CustomsEntryNumberType == CusEntryNumberTypeList.Codes.ITN)
			{
				Synchronisers.Add(new FieldSynchroniser(((USExportAsycudaBill)Destination).AESITNNumbersInfo, Source.CustomsEntryNumberInfo));
			}
			Synchronisers.Add(new FieldSynchroniser(((USExportAsycudaBill)Destination).InBondNumbersInfo, GetInBondNumbers, GetInfosAffectingContainerType));
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingContainerType()
		{
			if (Source.InBondHeader is InBond.Business.CusInBondHeader header)
			{
				foreach (var moveHeader in header.MovementHeaders.Cast<InBond.Business.CusInBondMoveHeader>())
				{
					yield return moveHeader.InBondNumberInfo;
				}
			}
		}

		IZType GetInBondNumbers()
		{
			var result = ZString.Empty;
			if (Source.InBondHeader is InBond.Business.CusInBondHeader header)
			{
				var inBondNumbers = header.MovementHeaders.Cast<InBond.Business.CusInBondMoveHeader>().Where(x => !x.InBondNumber.IsEmpty).Select(x => x.InBondNumber);
				result = string.Join(",", inBondNumbers.ToArray());
			}
			return result;
		}
	}
}
