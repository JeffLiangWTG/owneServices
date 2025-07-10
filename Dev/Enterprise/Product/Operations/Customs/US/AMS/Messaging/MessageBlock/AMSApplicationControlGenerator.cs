using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	class AMSApplicationControlGenerator : ApplicationControlGenerator<APLACR, APLZCR>
	{
		public AMSApplicationControlGenerator(string applicationIdentifier, GlbBranch branch)
			: base(branch)
		{
			A.ApplicationIdentifier = applicationIdentifier;
			var company = branch != null ? branch.Company : null;
			if (company != null)
			{
				A.AMSUserCode = company.OrgProxy.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.UnitedStates);
			}

			Z.ApplicationIdentifier = A.ApplicationIdentifier;
			Z.AMSUserCode = A.AMSUserCode;
		}

		protected override ZString GetMessageText(ZString messageText)
		{
			if (messageText.Left(3) == "ACR")
			{
				messageText = messageText.SubstringSafe(80);
			}
			if (messageText.SubstringSafe(messageText.Length - 80, 3) == "ZCR")
			{
				messageText = messageText.Left(messageText.Length - 80);
			}
			return messageText;
		}

		protected override string GetSettingDetails(GlbBranch branch)
		{
			var company = branch.Company;
			return Res.GetString("{CA6866A8-D85A-4DFC-9629-ED39BA1F4B6C}", "Please configure a Carrier Code (CCC) for US in Organization Proxy '{0}' (Maintain > User Admin > Companies > {1} - {2} > Company Info. > Organization Proxy > Details > Config > Registration Numbers / Codes)", company.OrgProxy.OH_Code, company.GC_Code, company.GC_Name);
		}
	}
}
