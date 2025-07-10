using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business
{
	public static class CusDispositionCollectionExtensionMethod
	{
		public static void AddOrUpdateCusDisposition(this CusDispositionCollection cusDispositions, Dictionary<ZString, IPGADispositionProvider> pgaEntryStatusMapping)
		{
			foreach (var element in pgaEntryStatusMapping)
			{
				AddOrUpdateCusDisposition(cusDispositions, element);
			}
		}

		public static void AddOrUpdateCusDisposition(this CusDispositionCollection cusDispositions, KeyValuePair<ZString, IPGADispositionProvider> dataProvider)
		{
			var agencyCode = dataProvider.Key;
			var dispositionProvider = dataProvider.Value;
			AddOrUpdateCusDisposition(cusDispositions, agencyCode, dispositionProvider.EntryDispositionCode, dispositionProvider.DispositionDateTime);
		}

		public static void AddQuotaDisposition(this CusDispositionCollection cusDispositions, ZString quotaStatusCode, ZString messageType, ZDateTime dispositionDate, ZString dispositionNote)
		{
			var quotaDisposition = cusDispositions.AddNew();
			quotaDisposition.CDI_Status = quotaStatusCode;
			quotaDisposition.CDI_StatusDate = dispositionDate;
			quotaDisposition.CDI_StatusKey = messageType;
			quotaDisposition.CDI_Notes = dispositionNote;
			quotaDisposition.CDI_Sequence = (short)cusDispositions.Count;
		}

		public static void AddCusDisposition(this CusDispositionCollection cusDispositions, ZString statusKey, ZDateTime dispositionDate, ZString status)
		{
			var quotaDisposition = cusDispositions.AddNew();
			quotaDisposition.CDI_Status = status;
			quotaDisposition.CDI_StatusDate = dispositionDate;
			quotaDisposition.CDI_StatusKey = statusKey;
			quotaDisposition.CDI_Sequence = (short)cusDispositions.Count;
		}

		static CusDisposition AddOrUpdateCusDisposition(this CusDispositionCollection cusDispositions, ZString agencyCode, ZString status, ZDateTime dispositionDateTime)
		{
			var cusDisposition = AddNewIfNotExist(cusDispositions, agencyCode);
			if (cusDisposition.CDI_StatusDate.IsEmpty || cusDisposition.CDI_StatusDate <= dispositionDateTime)
			{
				cusDisposition.CDI_Status = status;
				cusDisposition.CDI_StatusDate = dispositionDateTime;
			}
			return cusDisposition;
		}

		public static void AddOrUpdateCusDisposition(this CusDispositionCollection cusDispositions, IPGADispositionProvider dispositionProvider, ZInt lineNumber)
		{
			var cusDisposition = AddOrUpdateCusDisposition(cusDispositions, dispositionProvider.OtherAgencyQuotaIdentifier, dispositionProvider.PGALineDispositionCode, dispositionProvider.DispositionDateTime);
			cusDisposition.CDI_Sequence = (ZShort)lineNumber;
		}

		static CusDisposition AddNewIfNotExist(CusDispositionCollection cusDispositions, ZString agencyCode)
		{
			var result = cusDispositions.Cast<CusDisposition>().FirstOrDefault(x => x.CDI_StatusKey == agencyCode);
			if (result == null)
			{
				result = cusDispositions.AddNew();
				result.CDI_StatusKey = agencyCode;
			}
			return result;
		}
	}
}
