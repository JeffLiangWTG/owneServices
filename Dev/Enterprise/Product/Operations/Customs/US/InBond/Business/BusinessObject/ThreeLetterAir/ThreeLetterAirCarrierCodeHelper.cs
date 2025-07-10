using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business
{
	public class ThreeLetterAirCarrierCodeHelper
	{
		public ThreeLetterAirCarrierCodeHelper(BusinessObject parent)
		{
			this.parent = parent;
		}
		readonly BusinessObject parent;

		public delegate void CheckMaximumLengthDelegate(ZPropertyInfo carrierCodeFieldInfo, ZString value);

		public void AddOnColumnCarrierCode(GenAddOnColumn addOnCarrier, ZString statusName, ZString value, ZPropertyInfo carrierCodeFieldInfo, CheckMaximumLengthDelegate checkMaximumLength)
		{
			if (value.IsEmpty)
			{
				if (addOnCarrier != null && !addOnCarrier.IsDeleted)
				{
					addOnCarrier.Delete();
				}
			}
			else
			{
				checkMaximumLength?.Invoke(carrierCodeFieldInfo, value);

				if (addOnCarrier != null)
				{
					addOnCarrier.XA_Data = value;
				}
				else
				{
					var addOn = parent.Factory.New<GenAddOnColumn>();
					addOn.XA_ParentID = parent.PK;
					addOn.XA_ParentTableCode = parent.TablePrefix;
					addOn.XA_Name = statusName;
					addOn.XA_Type = AddOnColumnDataType.Codes.String;
					addOn.XA_Data = value;
				}
			}
			carrierCodeFieldInfo.RefreshBinding();
		}

		public GenAddOnColumn GetAddOnCarrierCode(ZString carrierCodeFieldName)
		{
			var addOnCarrierCodeQuery = new ZQuery(GenAddOnColumnSchema.XA_ParentID, parent.PK);
			addOnCarrierCodeQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, parent.TablePrefix);
			addOnCarrierCodeQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, carrierCodeFieldName);
			addOnCarrierCodeQuery.FetchOnlyFromLocalCache = !parent.IsInDatabase;
			return parent.Factory.LoadTop1<GenAddOnColumn>(addOnCarrierCodeQuery);
		}

		public ZString GetRefAirlineThreeLetterCodeIfNecessary(ZString twoCharacterCode)
		{
			var result = ZString.Empty;
			if (IsThreeLetterAirCarrierCodeRequired(twoCharacterCode))
			{
				var refAirline = RefAirline.LoadFromAirline2LetterCode(parent.Factory, twoCharacterCode);
				if (refAirline != null)
				{
					result = refAirline.RM_ThreeLetterCode;
				}
			}
			return result;
		}

		public static bool IsThreeLetterAirCarrierCodeRequired(ZString scas) => scas.Length == 2 && scas.SubstringSafe(1, 1).IsNumbersOnlyOrEmpty;
	}
}
