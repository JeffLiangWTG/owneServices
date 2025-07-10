using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using AMFDollar1 = Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.AMFDollar1;
using AMFDollar4 = Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.AMFDollar4;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressAddResponse)]
	public class AddManufacturerProcessor : ACEABIProcessor
	{
		public override void Process()
		{
			AMFDollarA dollarA = new AMFDollarA();
			AMFDollar5 dollar5 = new AMFDollar5();
			AMFDollar6 dollar6 = new AMFDollar6();
			AMFDollar7 dollar7 = null;
			var errorCodes = new List<ZString>();

			foreach (MessageBlock block in messageBlocks)
			{
				if (block is AMFDollarA)
				{
					dollarA = block as AMFDollarA;
				}

				if (block is AMFDollar5)
				{
					dollar5 = block as AMFDollar5;
				}

				if (block is AMFDollar6)
				{
					dollar6 = block as AMFDollar6;
				}

				if (block is AMFDollar7)
				{
					dollar7 = block as AMFDollar7;
					if (!dollar7.ErrorMessageIdentifier.IsEmpty)
					{
						errorCodes.Add(dollar7.ErrorMessageIdentifier);
					}
				}
			}

			StringBuilder htmlBody = new StringBuilder();

			htmlBody.Append("Manufacturer ID Code : " + dollar6.ManufacturerIDCode + "<br />");
			htmlBody.Append("<br />");
			htmlBody.Append("Firm Name : " + dollar5.FirmName + " " + dollar6.FirmName + "<br />");
			htmlBody.Append("ISO Country Code : " + dollar5.ISOCountryCode + "<br />");

			if (dollar7 != null)
			{
				htmlBody.Append("<br />");
				htmlBody.Append("<b style=\"color:red\">");
				htmlBody.Append("Error Code : " + dollar7.ErrorMessageIdentifier + "<br />");
				htmlBody.Append("Description : " + dollar7.NarrativeMessage + "<br />");
				htmlBody.Append("</b>");
			}

			ZString organisationCode = "Unknown";
			ZString uri = ZString.Empty;
			OrgHeader organisation = OriginalMessageLinker.Link<OrgHeader>(Message);
			bool isFailure = dollar5.UpdateStatus != "U";
			bool hasWarning = !isFailure && errorCodes.Count > 0;
			bool isUpdate = false;
			if (organisation != null)
			{
				organisationCode = "Organization: " + organisation.OH_Code;
				var originalMessage = Message.OriginalMessage;
				if (originalMessage != null)
				{
					var dollar4 = originalMessage.GetMessageBlocks<AMFDollar4>(x => !x.ManufacturerIDCode.IsEmpty).FirstOrDefault();
					if (dollar4 != null)
					{
						organisationCode += ", MID: " + dollar4.ManufacturerIDCode;
					}

					var dollar1 = originalMessage.GetMessageBlocks<AMFDollar1>(x => !x.UpdateActionCode.IsEmpty).FirstOrDefault();
					if (dollar1 != null)
					{
						isUpdate = dollar1.UpdateActionCode == "U";
					}
				}

				uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Organisation, organisation.PK.ToGuid());
				ZGuid addressPK;
				if (!dollar6.ManufacturerIDCode.IsEmpty && !dollarA.UserData.IsEmpty && ZGuid.TryParse(dollarA.UserData, out addressPK))
				{
					OrgAddress address = (OrgAddress)organisation.Addresses.FindByPK(addressPK);
					if (address != null)
					{
						if (!dollar6.ZIPOrPostalCode.IsEmpty)
						{
							address.OA_PostCode = dollar6.ZIPOrPostalCode;
						}
						address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, dollar6.ManufacturerIDCode, Core.Constants.CountryCodes.UnitedStates);
					}
				}
			}

			var branch = Message.OriginalMessage != null ? Message.OriginalMessage.Branch : GlbBranch.CurrentBranch;
			var messageTypeInSubject = isUpdate ? "Update Manufacturer File" : "Add Manufacturer File";
			GenerateHtmlEmailAndSendToOriginalOrGroup(uri, organisationCode, messageTypeInSubject, htmlBody.ToString(), isFailure, branch, organisation, hasWarning, WarningMessage);
		}

		const string WarningMessage = "(Warning) ";

		protected override List<ZString> ExcludedErrorCodes
		{
			get
			{
				var result = base.ExcludedErrorCodes;
				result.Add("B22");
				return result;
			}
		}
	}
}
