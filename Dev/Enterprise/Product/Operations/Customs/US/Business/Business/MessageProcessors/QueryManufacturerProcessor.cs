using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.US;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ManufacturerNameAndAddressQueryResponse)]
	public class ACEQueryManufacturerProcessor : QueryManufacturerProcessor<AABIOutputA, AABIOutputB, AABIOutputY>
	{
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public abstract class QueryManufacturerProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : ABIProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		OrgHeader linkedOrganisation;
		QMFDollar1 dollar1 = new QMFDollar1();
		QMFDollar2 dollar2 = new QMFDollar2();
		QMFDollar3 dollar3 = new QMFDollar3();
		QMFDollar4 dollar4 = new QMFDollar4();
		QMFDollar5 dollar5 = new QMFDollar5();
		QMFDollar7 dollar7;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		public override void Process()
		{
			foreach (MessageBlock block in messageBlocks)
			{
				if (block is QMFDollar1)
				{
					dollar1 = block as QMFDollar1;
				}

				if (block is QMFDollar2)
				{
					dollar2 = block as QMFDollar2;
				}

				if (block is QMFDollar3)
				{
					dollar3 = block as QMFDollar3;
				}

				if (block is QMFDollar4)
				{
					dollar4 = block as QMFDollar4;
				}

				if (block is QMFDollar5)
				{
					dollar5 = block as QMFDollar5;
				}

				if (block is QMFDollar7)
				{
					dollar7 = block as QMFDollar7;
				}
			}
			bool isFailure = dollar7 != null;
			StringBuilder htmlBody = new StringBuilder();

			htmlBody.Append("Manufacturer ID Code : " + dollar1.ManufacturerIDCode + "<br />");
			htmlBody.Append("<br />");
			if (isFailure)
			{
				htmlBody.Append("Error Code : " + dollar7.ErrorMessageIdentifier + "<br />");
				htmlBody.Append("Description : " + dollar7.NarrativeMessage + "<br />");
			}
			else
			{
				htmlBody.Append("Firm Name : " + dollar2.FirmName + " " + dollar3.FirmName + "<br />");
				htmlBody.Append("ISO Country Code : " + dollar2.ISOCountryCode + "<br />");
				htmlBody.Append("Street Address : " + dollar3.Street + " " + dollar4.Street + "<br />");
				htmlBody.Append("City : " + dollar4.City + " " + dollar5.City + "<br />");
				htmlBody.Append("Zip/Postal Code : " + dollar5.ZIPOrPostalCode + "<br />");
			}

			ZString organisationCode = "Unknown";
			ZString uri = ZString.Empty;

			linkedOrganisation = OriginalMessageLinker.Link<OrgHeader>(Message);
			if (linkedOrganisation != null)
			{
				bool orgDetailsShouldBeUpdatedFromMessage = !isFailure
					&& AutocreatefromMID.IsMIDOrganization(linkedOrganisation);

				if (orgDetailsShouldBeUpdatedFromMessage)
				{
					UpdateOrgDetailsFromMessage();
				}

				organisationCode = linkedOrganisation.OH_Code;
				uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Organisation, linkedOrganisation.PK.ToGuid());
			}
			else
			{
				if (!isFailure && Message.OriginalMessage != null && Message.OriginalMessage.EM_ApplicationReference == YesNoDefaultList.Codes.Yes)
				{
					var countryCode = CanadaProvinceTerritoryCodes.IsCanadianProvince(dollar2.ISOCountryCode)
						? Core.Constants.CountryCodes.Canada
						: dollar2.ISOCountryCode.ToString();
					var unloco = countryCode;
					var city = dollar4.City + " " + dollar5.City;

					var closestPortsQuery = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, countryCode);
					closestPortsQuery.AddToFilter(new ZQuery(RefUNLOCOSchema.RL_PortName, city));
					var closestPorts = Factory.Load<RefUNLOCO>(closestPortsQuery);
					if (closestPorts.Length == 1)
					{
						unloco = closestPorts[0].Code;
						htmlBody.Append("UNLOCO : " + unloco + "<br />");
					}

					OrgHeader newGeneratedOrganization = OrganisationCreator.CreateOrganisation(Factory, dollar2.FirmName + " " + dollar3.FirmName, dollar3.Street, dollar4.Street, city, "", dollar5.ZIPOrPostalCode, unloco, true, false);
					newGeneratedOrganization.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, dollar1.ManufacturerIDCode, Core.Constants.CountryCodes.UnitedStates);
					if (Message.OriginalMessage != null)
					{
						Message.OriginalMessage.EM_LinkedObject = newGeneratedOrganization;
					}

					organisationCode = newGeneratedOrganization.OH_Code;
					uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Organisation, newGeneratedOrganization.PK.ToGuid());

					htmlBody.Append("<br /><b>Auto create Organization option has been selected for this message.</b><br />");
					htmlBody.Append("<b>New Organisation with code: " + organisationCode + " has been generated.</b><br />");
				}
			}
			var branch = Message.OriginalMessage != null ? Message.OriginalMessage.Branch : GlbBranch.CurrentBranch;
			GenerateHtmlEmailAndSendToOriginalOrGroup(uri, organisationCode, "Query Manufacturer File", htmlBody.ToString(), isFailure, branch, linkedOrganisation);
		}

		void UpdateOrgDetailsFromMessage()
		{
			linkedOrganisation.OH_FullName = new ZString(dollar2.FirmName + " " + dollar3.FirmName).Left(linkedOrganisation.OH_FullNameInfo.MaxLength);
			linkedOrganisation.MainAddress.OA_Address1 = new ZString(dollar3.Street + " " + dollar4.Street).Left(linkedOrganisation.MainAddress.OA_Address1Info.MaxLength);
			linkedOrganisation.MainAddress.OA_Address2 = "";
			ZString city = dollar4.City + " " + dollar5.City;
			linkedOrganisation.MainAddress.OA_City = city.Left(linkedOrganisation.MainAddress.OA_CityInfo.MaxLength);
			linkedOrganisation.MainAddress.OA_PostCode = dollar5.ZIPOrPostalCode;
		}
	}
}
