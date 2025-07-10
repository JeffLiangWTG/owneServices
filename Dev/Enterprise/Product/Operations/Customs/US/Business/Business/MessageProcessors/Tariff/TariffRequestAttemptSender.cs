using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.US.Business
{
	public class TariffRequestAttemptSender
	{
		public TariffRequestAttemptSender(ZInt lastHTSAttempt, BusinessObjectFactory factory)
		{
			this.lastHTSAttempt = lastHTSAttempt;
			this.factory = factory;
		}
		readonly ZInt lastHTSAttempt;
		readonly BusinessObjectFactory factory;

		int AttemptYear
		{
			get { return lastHTSAttempt / 100; }
		}

		public bool CurrentYearNotEqualAttemptYear
		{
			get { return ZDateTime.Today.Year % 100 != AttemptYear; }
		}

		public ZInt NextAttempt
		{
			get { return lastHTSAttempt + 1; }
		}

		public ZInt NextAttemptForYearCutover
		{
			get { return ((AttemptYear + 1) % 100) * 100 + 1; }
		}

		public void SendMessageIfAllowed(bool sendMessage, ZInt nextAttempt, USCDataVersion lastHTSAttemptObj)
		{
			if (sendMessage)
			{
				nextAttempt = GetAttemptVersionNotIgnored(nextAttempt);
				new ReferenceFileRequester().RequestTariffByUpdate(nextAttempt);

				// DO NOT try and do on a 'changed' basis - needs the status and time recorded of each attempt
				lastHTSAttemptObj.UZ_Version = nextAttempt;
				lastHTSAttemptObj.SetNoteWithDatabaseDetailAndUpdateTime(ExtractReferenceFilesResult.Pending);
			}
		}

		ZInt GetAttemptVersionNotIgnored(ZInt attemptVersion)
		{
			var isVersionIgnored = Enterprise.Customs.Universal.ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, attemptVersion.ToString(), Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.HTSAttemptVersionsIgnored, ZDateTime.Today) != null;
			if (isVersionIgnored)
			{
				attemptVersion = GetAttemptVersionNotIgnored(attemptVersion + 1);
			}

			return attemptVersion;
		}
	}
}
