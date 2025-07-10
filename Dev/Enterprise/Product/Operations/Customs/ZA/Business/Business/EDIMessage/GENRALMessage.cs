using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public class GENRALMessage : SARSEDIMessage
	{
		#region Constructor
		public GENRALMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypes.GENRAL;
		}

		public ZString AgentCode => EM_MessageOwner.Left(8);

		public ZString CustomsDualProfileCode => EM_MessageOwner.SubstringSafe(8);

		public ZString? LocalProfileName
		{
			get
			{
				var agentCodeQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
				agentCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.AgentCode);
				agentCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, AgentCode);
				agentCodeQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.SouthAfrica);

				var dualProfileCodeQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
				dualProfileCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode);
				dualProfileCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, CustomsDualProfileCode);
				dualProfileCodeQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.SouthAfrica);

				var zQuery = new ZDBOnlyQuery(typeof(OrgHeader));
				zQuery.AddSubQuery(agentCodeQuery, JoinCondition.And);
				zQuery.AddSubQuery(dualProfileCodeQuery, JoinCondition.And);

				var result = Factory.Load<OrgHeader>(zQuery);
				return result.FirstOrDefault()?.OH_Code;
			}
		}
	}
}
