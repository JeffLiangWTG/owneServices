using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport
{
	public class OrgsToLinkDataTransferProcessor : DataTransferProcessor
	{
		readonly BusinessObjectFactory factory;
		readonly Dictionary<OrgFlattened, OrgHeader> orgsToLinkDict;

		public OrgsToLinkDataTransferProcessor(OrgFlattenedCollection orgCollection, Dictionary<OrgFlattened, OrgHeader> orgsToLinkDict)
		{
			this.factory = orgCollection.Factory;
			this.orgsToLinkDict = orgsToLinkDict;
		}

		public override void Import()
		{
			OnProgressChanged(0, Res.GetString("625ad4a0-71b8-4e9a-acb6-f53d0ab0891c", "Checking if there are related organizations to be linked."));
			if (orgsToLinkDict.Count > 0)
			{
				var originalRefreshValue = factory.RefreshEnabled;

				try
				{
					factory.SuspendValidation();
					factory.RefreshEnabled = false;

					int i = 1;
					foreach (KeyValuePair<OrgFlattened, OrgHeader> orgToLink in orgsToLinkDict)
					{
						if (IsCanceled)
						{
							break;
						}

						if (!OnProgressChanged(i * 100 / orgsToLinkDict.Count, Res.GetString("95a0e1f3-c5b3-4e97-9fc3-72197a17055c", "Linking related organizations ({0} of {1}) ...", i++, orgsToLinkDict.Count)))
						{
							break;
						}

						OrgFlattened org = orgToLink.Key;
						OrgHeader newOrg = orgToLink.Value;

						try
						{
							if (!org.CustomsAgent.IsEmpty)
							{
								OrgHeader relatedOrg = FindOrg(org.CustomsAgent);
								if (relatedOrg != null)
								{
									newOrg.SetRelatedParty(relatedOrg, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, ZString.Empty);
									newOrg.SetRelatedParty(relatedOrg, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
									newOrg.SetRelatedParty(relatedOrg, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Sea, ZString.Empty);
									newOrg.SetRelatedParty(relatedOrg, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Air, ZString.Empty);
									OrgsLinked++;
								}
								else
								{
									Log += Res.GetString("272f1db2-df11-41fb-b437-abfcaa1a9057", "Organization [Code: {0}] not found on linking parse", org.CustomsAgent) + "\n";
								}
							}

							if (!org.Debtor.IsEmpty)
							{
								OrgHeader relatedOrg = FindOrg(org.Debtor);
								if (relatedOrg != null)
								{
									newOrg.SetRelatedParty(relatedOrg, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.All, ZString.Empty);
									newOrg.SetRelatedParty(relatedOrg, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.All, ZString.Empty);
									newOrg.SetRelatedParty(relatedOrg, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.All, ZString.Empty);
									newOrg.SetRelatedParty(relatedOrg, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.All, ZString.Empty);
									OrgsLinked++;
								}
								else
								{
									Log += Res.GetString("272f1db2-df11-41fb-b437-abfcaa1a9057", "Organization [Code: {0}] not found on linking parse", org.Debtor) + "\n";
								}
							}

							if (!org.DebtorSettlementGroup.IsEmpty)
							{
								OrgHeader relatedOrg = FindOrg(org.DebtorSettlementGroup);
								if (relatedOrg != null)
								{
									newOrg.ARSettlementGroupPK = relatedOrg.PK;
									OrgsLinked++;
								}
								else
								{
									Log += Res.GetString("272f1db2-df11-41fb-b437-abfcaa1a9057", "Organization [Code: {0}] not found on linking parse", org.DebtorSettlementGroup) + "\n";
								}
							}
						}
						catch (Exception e) when (!e.IsCriticalException())
						{
							Log += Res.GetString("4d63dd5b-e149-4e69-8e93-f1ab98cea6e3", "Organization [Code: {0}] linking failed... data is inconsistent with required format ", org.OH_Code) + "\n";
						}
					}
				}
				finally
				{
					factory.ResumeValidation();
					factory.RefreshEnabled = originalRefreshValue;
				}
			}
			else
			{
				OnProgressChanged(100, Res.GetString("3b692ad3-846e-4f1c-a230-e4c2b0994a5c", "No related organizations to set."));
			}
		}

#if DEBUG
		public
#endif
 OrgHeader FindOrg(string orgCode)
		{
			OrgHeader relatedOrg = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, orgCode));
			if (relatedOrg == null)
			{
				ZQuery query = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.LegacySystemCode);
				query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, orgCode);
				OrgCusCode cusCode = factory.LoadTop1<OrgCusCode>(query);
				if (cusCode != null)
				{
					relatedOrg = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, cusCode.Organisation.OH_Code));
				}
			}
			return relatedOrg;
		}

		public override void Rollback()
		{
		}

		public string Log { get; set; }

		public int OrgsLinked { get; set; }
	}
}
