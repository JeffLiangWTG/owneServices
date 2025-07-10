using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.US.USAMS;

namespace Enterprise.Customs.US.AMS.Business
{
	public class OriginatorCodeChangedHandler : IOriginatorCodeChangedHandler
	{
		public OriginatorCodeChangedHandler(BusinessObjectFactory factory, ZGuid organisationPK)
		{
			Factory = factory;
			this.organisationPK = organisationPK;
		}
		readonly ZGuid organisationPK;

		BusinessObjectFactory Factory { get; }

		GlbBranchCollection Branches
		{
			get
			{
				if (fBranches == null)
				{
					var query = new ZQuery(GlbBranchSchema.GB_OH_OrgProxy, organisationPK);
					query.AddToFilter(GlbBranchSchema.GB_IsActive, true);
					fBranches = new GlbBranchCollection(Factory, query);
					fBranches.Load();
				}
				return fBranches;
			}
		}
		GlbBranchCollection fBranches;

		const string ConfigurationName = "US Customs Registry";
		const string Current = "Current";
		const string CompanyType = "Company";
		const string GroupTypeAMA = "AMA";

		void IOriginatorCodeChangedHandler.OnUpdateAction()
		{
			var companyWithOriginatorCodes = new Dictionary<string, string>();
			foreach (GlbBranch branch in Branches)
			{
				var company = branch.Company;
				if (company != null && company.GC_IsActive && !companyWithOriginatorCodes.ContainsKey(company.GC_Code))
				{
					var companyOriginatorCodes = new HashSet<string>();

					company.Branches
						.Where(b => b.OrgProxy != null)
						.SelectMany(b => b.OrgProxy.CustomsCodes)
						.Cast<OrgCusCode>()
						.Where(x => x.OK_CodeType == OrgCusCode.USACodeTypes.AirAMSOriginatorCode)
						.ForEach(x => companyOriginatorCodes.Add(x.OK_CustomsRegNo));

					if (company.OrgProxy != null)
					{
						company.OrgProxy.CustomsCodes
							.Cast<OrgCusCode>()
							.Where(x => x.OK_CodeType == OrgCusCode.USACodeTypes.AirAMSOriginatorCode)
							.ForEach(x => companyOriginatorCodes.Add(x.OK_CustomsRegNo));
					}

					companyWithOriginatorCodes.Add(company.GC_Code, string.Join(",", companyOriginatorCodes));
				}
			}

			if (companyWithOriginatorCodes.Count > 0)
			{
				var credentialSender = new CredentialSender(ConfigurationName);
				var companyGroups = new List<object>();

				foreach (var item in companyWithOriginatorCodes)
				{
					var groupAMA = CredentialSender.CreateGroup(GroupTypeAMA, ZString.Empty, Constants.CredentialStatusList.Valid);

					var newValueString = item.Value;
					if (!string.IsNullOrWhiteSpace(newValueString))
					{
						var credential = CredentialSender.CreateCredential(Current, ZString.Empty, newValueString);
						groupAMA.Items = new[] { credential };
					}

					var groupCompany = CredentialSender.CreateGroup(CompanyType, item.Key, ZString.Empty);
					groupCompany.Items = new object[] { groupAMA };

					companyGroups.Add(groupCompany);
				}

				credentialSender.AddItems(companyGroups.ToArray());
				credentialSender.SendCredential(Factory);
			}
		}
	}
}
