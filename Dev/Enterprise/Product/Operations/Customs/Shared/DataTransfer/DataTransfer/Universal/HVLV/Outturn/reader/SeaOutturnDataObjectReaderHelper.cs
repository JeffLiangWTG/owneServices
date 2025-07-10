using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn
{
	public class SeaOutturnDataObjectReaderHelper : UniversalCommonReaderHelper
	{
		public SeaOutturnDataObjectReaderHelper(UniversalObjectFactory factory, ZString targetCountryCode, string dataProviderForCodeMapping = null)
				: base(factory, targetCountryCode, dataProviderForCodeMapping)
		{
		}

		public void MarkUnprocessedExistingBillsFor(CusOutturnHeader outturnHeader, ZGuid? forwardingShipmentPK)
		{
			foreach (var outturn in outturnHeader.Outturns.OfType<CusOutturn>())
			{
				var sameShipment = !forwardingShipmentPK.HasValue
					|| outturn.C5_ParentID.IsEmpty
					|| !forwardingShipmentPK.Value.IsEmpty
					&& outturn.C5_ParentTableCode == JobShipmentSchema.Constants.Prefix
					&& outturn.C5_ParentID == forwardingShipmentPK;

				if (sameShipment && !ExistingOutturnProcessingDictionary.ContainsKey(outturn))
				{
					ExistingOutturnProcessingDictionary.Add(outturn, false);
				}
			}
		}

		public void MarkProcessed(CusOutturn outturn)
		{
			if (outturn != null && ExistingOutturnProcessingDictionary.ContainsKey(outturn))
			{
				ExistingOutturnProcessingDictionary[outturn] = true;
			}
		}

		public void DeleteUnprocessedBillsFor(CusOutturnHeader outturnHeader, IXmlImportLogger logger)
		{
			foreach (var pair in ExistingOutturnProcessingDictionary.ToArray())
			{
				if (!pair.Value && pair.Key.C5_C6 == outturnHeader.PK)
				{
					var outturn = pair.Key;
					if (outturn.CanDelete)
					{
						ExistingOutturnProcessingDictionary.Remove(outturn);
						logger.Log(LogType.Information, Res.GetString("951A1596-FF30-4BA7-8544-7123A6488E41", "Deleted {0} from {1}.", outturn.HumanReadableName, "UniversalShipment"));
						outturnHeader.Outturns.RemoveAndDelete(outturn);
					}
					else
					{
						var reason = (ZString)outturn.ReasonForNotAbleToDelete;
						var reasonToShow = reason.Left(1).ToLower() + reason.SubstringSafe(1);

						logger.Log(LogType.Warning, Invariant($"{outturn.HumanReadableName} does not appear in UniveralShipment, but {reasonToShow}"));
						MarkProcessed(outturn);
					}
				}
			}
		}

		Dictionary<CusOutturn, bool> ExistingOutturnProcessingDictionary => existingOutturnProcessingDictionary ?? (existingOutturnProcessingDictionary = new Dictionary<CusOutturn, bool>());
		Dictionary<CusOutturn, bool> existingOutturnProcessingDictionary;
	}
}
