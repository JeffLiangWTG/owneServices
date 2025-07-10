using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DeniedPartyScreening.Business
{
	public class DpsSourceWithParties : IDpsSourceWithParties
	{
		public BusinessObject SourceBizO { get; }
		public IEnumerable<ScreeningParty> ScreenParties { get; }

		public DpsSourceWithParties(BusinessObject sourceBizO, ScreeningParty[] screenParties)
		{
			SourceBizO = sourceBizO;
			ScreenParties = screenParties;
		}

		public static List<IDpsSourceWithParties> GetSingleSourceList(BusinessObject sourceBizO, ScreeningParty[] screenParties)
		{
			if (sourceBizO == null)
			{
				throw new ArgumentNullException(nameof(sourceBizO));
			}

			if (screenParties == null)
			{
				throw new ArgumentNullException(nameof(screenParties));
			}

			return new List<IDpsSourceWithParties>
			{
				new DpsSourceWithParties(sourceBizO, screenParties)
			};
		}
	}
}
