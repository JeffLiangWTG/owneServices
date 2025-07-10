using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Edifact;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Messaging
{
	class StowPlanInterchangeProvider : InterchangeProviderBase
	{
		public StowPlanInterchangeProvider(NonDependentEDIMessageCollection messages)
			: base(messages)
		{
		}

		protected override string InstructionHowToSetInterchangeSenderID
		{
			get { return ""; }
		}

		protected override string GetCollationKey(EDIMessage message)
		{
			return message.EM_GB.ToStringKey() + base.GetCollationKey(message);
		}

		protected override Type InterchangeType
		{
			get { return typeof(CBPEDIInterchange); }
		}

		internal static string GetMissingCustomsInterchangeSenderIDMessage(ZString messageNum, ZString orgCode) => Res.GetString("{ADAC01D3-D104-4B5F-B8B8-7747CB6470B1}", @"Message {0} will be discarded for the following reason:\r\nThere is no Customs Interchange Sender ID set up, please configure a Carrier Code (CCC) for Organization Proxy ({1}) > Details > Config > Registration Numbers/Code.", messageNum, orgCode);

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			if (messages.Count > 0)
			{
				(ZString scac, OrgHeader orgProxy) = GetOrgProxyData(messages[0].EM_GB);
				if (scac.IsEmpty)
				{
					foreach (EDIMessage message in messages)
					{
						message.EM_Status = EDIMessage.Status.Discarded;
						message.Notes.AddNew(true, ProcessingLogDescription, GetMissingCustomsInterchangeSenderIDMessage(message.EM_MessageNum, orgProxy.OH_Code));
					}
					interchange.Delete();
				}
				else
				{
					var message = messages[0];
					SetInterchangeValuesForTransmit(interchange, messages, message.EM_MessageType, "USC", scac);

					var uNB = GetUNB(PreparedTime.ToZDateTime(), interchange.EI_To, ZString.Empty, interchange.EI_From, ZString.Empty, ZString.Empty, "UNOA", "2", ZString.Empty,
						false, false, ZString.Empty).ToString(new UNOACharacterSet());
					interchange.EI_HeaderText = uNB;
					interchange.EI_GB = messages[0].EM_GB;
					interchange.EI_Status = EDIInterchange.Status.Queued;
				}
			}
		}

		(ZString, OrgHeader) GetOrgProxyData(ZGuid messageBranchPK)
		{
			if (!CompanyDictionary.TryGetValue(GlbCompany.CurrentCompany.PK, out var data))
			{
				SwitchEnvironment(messageBranchPK);
				data = AddToCompanyDictionary(GlbCompany.CurrentCompany);
			}
			else if (!data.branchPKs.Contains(messageBranchPK))
			{
				SwitchEnvironment(messageBranchPK);
				if (!CompanyDictionary.TryGetValue(GlbCompany.CurrentCompany.PK, out data))
				{
					data = AddToCompanyDictionary(GlbCompany.CurrentCompany);
				}
			}
			return (data.scac, data.orgProxy);
		}

		(HashSet<ZGuid> branchPKs, ZString scac, OrgHeader orgProxy) AddToCompanyDictionary(GlbCompany currentCompany)
		{
			var orgProxy = currentCompany.OrgProxy;
			var scac = orgProxy.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.UnitedStates);
			var result = (currentCompany.Branches.GetPKs().ToHashSet(), scac, orgProxy);
			CompanyDictionary.Add(currentCompany.PK, result);
			return result;
		}

		void SwitchEnvironment(ZGuid messageBranchPK)
		{
			environmentSwitchDisposable?.Dispose();
			environmentSwitchDisposable = DisposableEnvironment.ForBranch(messageBranchPK.ToGuid());
		}

		Dictionary<ZGuid, (HashSet<ZGuid> branchPKs, ZString scac, OrgHeader orgProxy)> CompanyDictionary => companyDictionary ?? (companyDictionary = new Dictionary<ZGuid, (HashSet<ZGuid> branchPKs, ZString scac, OrgHeader orgProxy)>());
		Dictionary<ZGuid, (HashSet<ZGuid> branchPKs, ZString scac, OrgHeader orgProxy)> companyDictionary;

		protected override void DisposeCore()
		{
			base.DisposeCore();
			environmentSwitchDisposable?.Dispose();
		}
		IDisposable environmentSwitchDisposable;
	}
}
