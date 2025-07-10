using System;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class GlobalBusinessIdentifierMessageBuilder
	{
		public GlobalBusinessIdentifierMessageBuilder(GlobalBusinessIdentifierData messageData, GlobalBusinessIdentifierMessageType messageType)
		{
			this.messageData = messageData;
			this.messageType = messageType;

			var portCode = ProcessingPortCodeAndFilerFinder.GetProcessingPortCodeFromRegistry(GlbBranch.CurrentBranch);
			var officeCode = USCustomsDataRegistry.Instance.BRecordOfficeCode.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			block = new ACEInputBlockControlGenerator(GlbCompany.CurrentCompany.PK.ToGuid(), portCode, officeCode);
		}
		readonly GlobalBusinessIdentifierData messageData;
		readonly GlobalBusinessIdentifierMessageType messageType;
		readonly BlockControlGenerator block;

		public MQEDIMessage Generate()
		{
			block.B.ApplicationIdentifier = ACEApplicationIdentifierCodeList.Codes.GBIReferenceCreateUpdateDelete;
			var messageSubType = ZString.Empty;
			switch (messageType)
			{
				case GlobalBusinessIdentifierMessageType.Original:
					messageSubType = EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierAdd;
					GenerateBlocks(UpdateActionCode.Add);
					break;
				case GlobalBusinessIdentifierMessageType.Update:
					messageSubType = EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierUpdate;
					GenerateBlocks(UpdateActionCode.Update);
					break;
				case GlobalBusinessIdentifierMessageType.Delete:
					messageSubType = EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierDelete;
					GenerateBlocks(UpdateActionCode.Delete);
					break;
				default:
					break;
			}

			var message = block.CreateMessage<MQEDIMessage>(messageData.Factory);
			message.EM_LinkedObject = messageData.Organization;
			message.EM_MessageSubType = messageSubType;

			var statusCalculator = new GlobalBusinessIdentifierMessageStatusCalculator(messageData);
			statusCalculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			messageData.Wrapper.Messages.Add(message);

			return message;
		}

		void GenerateBlocks(UpdateActionCode actionCode)
		{
			block.AddMessageBlock(new AGE10() { ActionCode = UpdateActionCodeConverter.ConvertToString(actionCode), EntityName = messageData.US_FirmName });

			block.AddMessageBlock(new AGE20() { GBIIdentifierQualifier = OrganisationDetails.DataUniversalNumberingSystemCodeInGlobalBusinessIdentifier, GBIIdentifier = messageData.US_DUNS });
			block.AddMessageBlock(new AGE20() { GBIIdentifierQualifier = OrgCusCode.USACodeTypes.GlobalLocationNumber, GBIIdentifier = messageData.US_GLN });
			block.AddMessageBlock(new AGE20() { GBIIdentifierQualifier = OrgCusCode.USACodeTypes.LegalEntityIdentifier, GBIIdentifier = messageData.US_LEI });

			if (actionCode != UpdateActionCode.Delete)
			{
				block.AddMessageBlock(new AGE21()
				{
					ManufacturerRole = messageData.US_IsManufacturer ? EntityCodeList.Codes.ManufacturerSupplier : string.Empty,
					ShipperRole = messageData.US_IsShipper ? EntityCodeList.Codes.Shipper : string.Empty,
					SellerRole = messageData.US_IsSeller ? EntityCodeList.Codes.SellingParty : string.Empty,
					ExporterRole = messageData.US_IsExporter ? EntityCodeList.Codes.Exporter : string.Empty,
					PackagerRole = messageData.US_IsPackager ? EntityCodeList.Codes.Packager : string.Empty,
					DistributorRole = messageData.US_IsDistributor ? EntityCodeList.Codes.Distributor : string.Empty
				});

				if (!messageData.US_Address1.IsEmpty || !messageData.US_Address2.IsEmpty)
				{
					if (!messageData.US_Address1.IsEmpty)
					{
						block.AddMessageBlock(new AGE30() { EntityAddressLine1 = messageData.US_Address1 });
					}

					if (!messageData.US_Address2.IsEmpty)
					{
						block.AddMessageBlock(new AGE31() { EntityAddressLine2 = messageData.US_Address2 });
					}

					block.AddMessageBlock(new AGE32()
					{
						City = messageData.US_City,
						StateProvince = messageData.US_State,
						Country = messageData.US_Country,
						PostalCode = messageData.US_PostCode
					});
				}

				if (!messageData.US_Phone.IsEmpty)
				{
					block.AddMessageBlock(new AGE40() { PhoneNumber = messageData.US_Phone });
				}

				if (!messageData.US_WebsiteURL.IsEmpty)
				{
					block.AddMessageBlock(new AGE41() { Website = messageData.US_WebsiteURL });
				}

				var manufacturerID = messageData.ManufacturerID;
				if (!manufacturerID.IsEmpty)
				{
					block.AddMessageBlock(new AGE50()
					{
						ReferenceIdentifierType = OrgCusCode.USACodeTypes.ManufacturerID,
						ReferenceIdentifierValue = manufacturerID
					});
				}

				var authorisedEconomicOperator = messageData.AuthorisedEconomicOperator;
				if (!authorisedEconomicOperator.IsEmpty)
				{
					block.AddMessageBlock(new AGE50()
					{
						ReferenceIdentifierType = OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator,
						ReferenceIdentifierValue = authorisedEconomicOperator
					});
				}
			}
		}
	}
}
