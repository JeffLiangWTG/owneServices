using System;

namespace Enterprise.Freight.Agency.Business.Testing
{
	static internal class ExtentionMethodsForTesting
	{
		#region ReleaseHeader
		public static void Release(this ReleaseHeader header, AgencyShipmentContainer container, short releaseCount)
		{
			foreach (ReleaseDetail detail in header.Details)
			{
				if (detail.Container.PK == container.PK)
				{
					detail.ReleaseCount = releaseCount;
					return;
				}
			}

			throw new ArgumentOutOfRangeException();
		}
		#endregion
	}
}
