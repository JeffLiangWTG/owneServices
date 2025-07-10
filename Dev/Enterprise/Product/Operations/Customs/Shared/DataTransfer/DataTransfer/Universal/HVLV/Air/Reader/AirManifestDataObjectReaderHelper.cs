using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DataTransfer.Universal.AirManifest
{
	public class AirManifestDataObjectReaderHelper : UniversalCommonReaderHelper
	{
		public AirManifestDataObjectReaderHelper(UniversalObjectFactory factory, ZString targetCountryCode, string dataProviderForCodeMapping = null)
			: base(factory, targetCountryCode, dataProviderForCodeMapping)
		{
		}

		public void MarkUnprocessedExistingBillsFor(CusMAWB mawb)
		{
			foreach (var hawb in mawb.ChildBills.OfType<CusHAWB>())
			{
				if (!ExistingBillProcessingDictionary.ContainsKey(hawb) && ShouldMarkBillAsUnprocessed(hawb))
				{
					ExistingBillProcessingDictionary.Add(hawb, false);
				}
			}
		}

		protected virtual bool ShouldMarkBillAsUnprocessed(CusHAWB hawb) => true;

		public void MarkProcessed(CusHAWB hawb)
		{
			if (ExistingBillProcessingDictionary.ContainsKey(hawb))
			{
				ExistingBillProcessingDictionary[hawb] = true;
			}
		}

		public void DeleteUnprocessedBillsFor(CusMAWB mawb, IXmlImportLogger logger)
		{
			foreach (var pair in ExistingBillProcessingDictionary.ToArray())
			{
				if (!pair.Value && pair.Key.CS_CM == mawb.PK)
				{
					var hawb = pair.Key;
					if (hawb.CanDelete)
					{
						ExistingBillProcessingDictionary.Remove(hawb);
						logger.Log(LogType.Information, Res.GetString("DB4291A8-D094-4780-BED5-309DEC291660", "Deleted {0} from {1}.", hawb.HumanReadableName, "UniversalShipment"));
						mawb.ChildBills.RemoveAndDelete(hawb);
					}
					else
					{
						HandleBillNotAbleToDelete(hawb, logger);
					}
				}
			}
		}

		protected virtual void HandleBillNotAbleToDelete(CusHAWB hawb, IXmlImportLogger logger)
		{
			var reason = (ZString)hawb.ReasonForNotAbleToDelete;
			var reasonToShow = reason.Left(1).ToLower() + reason.SubstringSafe(1);

			logger.Log(LogType.Warning, ZString.Format((NoResString)"{0} does not appear in UniveralShipment, but ", hawb.HumanReadableName) + reasonToShow);
			MarkProcessed(hawb);
		}

		Dictionary<CusHAWB, bool> ExistingBillProcessingDictionary
		{
			get { return existingBillProcessingDictionary ?? (existingBillProcessingDictionary = new Dictionary<CusHAWB, bool>()); }
		}
		Dictionary<CusHAWB, bool> existingBillProcessingDictionary;
	}
}
