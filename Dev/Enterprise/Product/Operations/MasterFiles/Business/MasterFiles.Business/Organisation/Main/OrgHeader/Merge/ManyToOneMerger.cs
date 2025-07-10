using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ManyToOneMerger
	{
		public ManyToOneMerger(string[] oldOrgCodes, string newOrgCode)
		{
			Argument.NotNull(oldOrgCodes, "oldOrgCodes");
			Argument.NotNullOrEmpty(newOrgCode, "newOrgCode");
			OldOrgCodes = oldOrgCodes;
			NewOrgCode = newOrgCode;
		}

		public string Process()
		{
			StringCollectionX errors = new StringCollectionX();
			OrgHeader newOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, NewOrgCode));
			ZGuid newOrgPk = ZGuid.Empty;
			if (newOrg == null)
			{
				errors.Add(GetOrgNotExistsMessage(NewOrgCode));
			}
			else
			{
				newOrgPk = newOrg.PK;
				foreach (string code in OldOrgCodes)
				{
					if (code == NewOrgCode)
					{
						errors.Add(CantMergeToItself);
					}
					else
					{
						BusinessObjectFactory localFactory = new BusinessObjectFactory();
						OrgHeader orgToMerge = localFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, code));
						if (orgToMerge == null)
						{
							errors.Add(GetOrgNotExistsMessage(code));
						}
						else
						{
							MergeOrgHeader merge = new MergeOrgHeader(localFactory, orgToMerge);
							merge.NewOrganisationPk = newOrgPk;
							merge.RunPreSaveValidation();
							if (merge.HasErrors)
							{
								foreach (INotification notif in merge.Notifications)
								{
									errors.Add(string.Format("{0}: {1}", code, notif.Message));
								}
							}
							else
							{
								if (OrgHeaderMergingChecker.IsAllowedToMergeOrgs(merge.NewOrganisation.PK, merge.OldOrganisation.PK, out var reasons))
								{
									BusinessObjectFactory.SaveTogether(merge.SaveFactories);

									if (!string.IsNullOrEmpty(merge.DeleteError))
									{
										errors.Add(merge.DeleteError);
									}
								}
								else
								{
									errors.Add(merge.OldOrganisation.OH_FullName + ": " + reasons);
								}
							}
						}
					}
				}
			}

			return string.Join("\r\n", errors.ToArray());
		}

		#region Error Messages

		string GetOrgNotExistsMessage(string code)
		{
			return ResString.GetMultilingualString("a2a2c786-992f-4258-9863-d02010b8be6a", "Organization with code '{0}' does not exist. No merging is possible.", code);
		}

		static string CantMergeToItself
		{
			get { return ResString.GetMultilingualString("32432ecc-de3c-4bd1-92e0-2b3d80966aa3", "Cannot merge Organization with itself."); }
		}

		#endregion

		BusinessObjectFactory factory;
		BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}
				return factory;
			}
		}

		string[] OldOrgCodes { get; set; }
		string NewOrgCode { get; set; }
	}
}
