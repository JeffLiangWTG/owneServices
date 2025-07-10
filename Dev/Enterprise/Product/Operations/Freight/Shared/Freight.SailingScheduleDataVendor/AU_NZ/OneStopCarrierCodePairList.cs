using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Freight.SailingScheduleDataVendor.Res;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	public class OneStopCarrierCodePairList : IOneStopCarrierCodePairListProvider
	{
		public OneStopCarrierCodePairList()
			: this(null)
		{
		}

		public OneStopCarrierCodePairList(BusinessObjectFactory factory)
		{
			Factory = factory ?? new BusinessObjectFactory();
		}

		readonly BusinessObjectFactory Factory;

		public CodeDescriptionPairList GetOneStopCarrierCodePairListForFilter()
		{
			CodeDescriptionPairList oneStopCarrierCodePairList = new CodeDescriptionPairList();

			ZQuery query = new ZQuery();
			query.AddToFilter(JobVesselScheduleSchema.EV_RL_NKPortCode, SQLComparisonOperator.StartsWith, Core.Constants.CountryCodes.Australia);
			query.OrderBy = JobVesselScheduleSchema.EV_LineOperator.Name;

			JobVesselScheduleBase[] ports = Factory.Load<JobVesselScheduleBase>(query);
			foreach (JobVesselScheduleBase port in ports)
			{
				if (!oneStopCarrierCodePairList.ContainsCode(port.EV_LineOperator))
				{
					AddToLineOperatorExternalCodeList(oneStopCarrierCodePairList, port.EV_LineOperator, port.EV_OperatorsDescription);
				}
			}
			return oneStopCarrierCodePairList;
		}

		void AddToLineOperatorExternalCodeList(CodeDescriptionPairList list, ZString oneStopCode, ZString oneStopDescription)
		{
			if (!oneStopCode.IsEmpty && !list.ContainsCode(oneStopCode))
			{
				OrgHeader lineOperatorOrg = GetLineOperatorFromOneStopCode(oneStopCode);
				ZString description = oneStopDescription;
				if (lineOperatorOrg == null)
				{
					description += " " + Res.GetString("a322ca46-9930-4f7e-b573-ad38753421eb", "(not on file)");
				}
				list.AddPair(oneStopCode, description);
			}
		}

		OrgHeader GetLineOperatorFromOneStopCode(ZString oneStopCode)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.OneStopCode);
			query.AddToFilter(JoinCondition.And, OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.Australia);
			query.AddToFilter(JoinCondition.And, OrgCusCodeSchema.OK_CustomsRegNo, oneStopCode);

			OrgCusCode oneStopCusCode = Factory.LoadTop1<OrgCusCode>(query);
			ZGuid orgPK = oneStopCusCode == null ? ZGuid.Empty : oneStopCusCode.OK_OH;

			return !orgPK.IsEmpty ? Factory.Load<OrgHeader>(orgPK) : null;
		}
	}
}
