using System;
using CargoWise.Common;
using CargoWise.Types;
using WTG.RTUS.Interface;

namespace Enterprise.Packing.Business
{
	public sealed class CancellationResponse
	{
		public CancellationResponse(ZGuid packageJobPK, IPackingParent packingParent, IRTUSResponse response, ZString packageID)
		{
			Response = Argument.NotNull(response, nameof(response));

			if (packageID.IsEmpty)
			{
				throw new ArgumentException("Package ID should not be empty.", nameof(packageID));
			}

			PackingParent = packingParent;
			PackageJobPK = packageJobPK;
			PackageID = packageID;
		}

		IRTUSResponse Response { get; }
		public bool IsSuccessful => Response.IsSuccessful;
		public string ErrorMessageForFailure => Response.ErrorMessageForFailure;

		public ZString PackageID { get; }
		public ZGuid PackageJobPK { get; }
		public IPackingParent PackingParent { get; }
	}
}
