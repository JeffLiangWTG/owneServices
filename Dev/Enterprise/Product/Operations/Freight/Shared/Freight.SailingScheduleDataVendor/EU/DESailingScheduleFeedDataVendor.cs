using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	class DESailingScheduleFeedDataVendor : SailingScheduleFeedDataVendor, Integration.SailingDataVendor.IDakosySailingScheduleDataVendor
	{
		protected override void UpdateVoyageOriginCore(VoyageOrigin origin)
		{
			base.UpdateVoyageOriginCore(origin);

			var lineOperatorCodes = new List<ZString>();

			if (origin.Voyage != null && origin.Voyage.Line != null)
			{
				var carrierCode = origin.Voyage.Line.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, ZString.Empty);
				var dakosyCode = origin.Voyage.Line.CustomsCodes.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, Core.Constants.CountryCodes.Germany);

				if (!carrierCode.IsEmpty)
				{
					lineOperatorCodes.Add(carrierCode);
				}

				if (!dakosyCode.IsEmpty)
				{
					lineOperatorCodes.Add(dakosyCode);
				}
			}

			var port = new VesselRoutingPort.Loader(origin.Factory).Load(origin, lineOperatorCodes);

			if (port != null)
			{
				if (!port.E7_DataProviderReference.IsEmpty)
				{
					origin.JA_DepartReference = port.E7_DataProviderReference;
					origin.FetchSailings().ForEach(sailing => sailing.JX_DeparturePortRouteId = origin.JA_DepartReference);
				}

				if (!port.E7_TerminalID.IsEmpty)
				{
					var cusCode = GetCusCode(origin.Factory, port.E7_TerminalID, GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, Core.Constants.CountryCodes.Germany);
					if (cusCode != null)
					{
						var cto = origin.Factory.Load<OrgHeader>(cusCode.OK_OH);
						if (cto != null)
						{
							origin.JA_Calc_DepartureCTOAddressOrg = cto.PK;
						}
					}
					else
					{
						origin.JA_Berth = port.E7_TerminalID;
						cusCode = GetCusCode(origin.Factory, port.E7_TerminalID, GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode, Core.Constants.CountryCodes.Germany);
						if (cusCode != null)
						{
							var cto = origin.Factory.Load<OrgHeader>(cusCode.OK_OH);
							if (cto != null)
							{
								origin.JA_Calc_DepartureCTOAddressOrg = cto.PK;
								if (cusCode.PremisesAddress != null)
								{
									origin.JA_Calc_DepartureCTOAddressCode = cusCode.PremisesAddress.OA_Code;
								}
							}
						}
					}
				}
			}
		}

		#region Implementation

		OrgCusCode GetCusCode(BusinessObjectFactory factory, ZString code, ZString codeType, ZString country)
		{
			var query = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, code);
			query.AddToFilter(OrgCusCodeSchema.OK_CodeType, codeType);
			query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, country);

			return factory.Load<OrgCusCode>(query).FirstOrDefault();
		}

		protected override ZString GetLineOperator(JobVoyage voyage)
		{
			var lineOperator = ZString.Empty;

			if (voyage != null && voyage.Line != null)
			{
				lineOperator = voyage.Line.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, ZString.Empty);

				if (lineOperator.IsEmpty)
				{
					lineOperator = voyage.Line.CustomsCodes.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, Core.Constants.CountryCodes.Germany);
				}
			}

			return lineOperator;
		}

		#endregion

	}
}
