#define CODE_ANALYSIS
using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.ISF.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public class ImporterSecurityFilingMessageBuilder<T, TBlockB, TBlockY> : MessageBuilder<T>
		where T : ABIInputBlockControlGenerator<TBlockB, TBlockY>
		where TBlockB : MessageBlock, IABIControlMessageBlockB, new()
		where TBlockY : MessageBlock, IABIControlMessageBlockY, new()
	{
		public ImporterSecurityFilingMessageBuilder(IImporterSecurityFiling iSFData, UpdateActionCode action)
			: base(iSFData, action)
		{
		}

		IImporterSecurityFiling ISFData
		{
			get { return (IImporterSecurityFiling)messageAttachee; }
		}

		bool IsISF10Types
		{
			get
			{
				return ISFData.SFSubmissionType == SubmissionTypeList.Codes.ISF10 ||
				ISFData.SFSubmissionType == SubmissionTypeList.Codes.ISF5ToISF10 ||
				ISFData.SFSubmissionType == SubmissionTypeList.Codes.LateISF10;
			}
		}

		bool IsISF5Types
		{
			get
			{
				return ISFData.SFSubmissionType == SubmissionTypeList.Codes.ISF5 ||
					ISFData.SFSubmissionType == SubmissionTypeList.Codes.ISF10ToISF5 ||
					ISFData.SFSubmissionType == SubmissionTypeList.Codes.LateISF5;
			}
		}

		protected override string ApplicationIdentifier
		{
			get { return ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling; }
		}

		protected override void SetMessageSubType(MQEDIMessage message)
		{
			switch (action)
			{
				case UpdateActionCode.Delete:
					message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ISFDelete;
					break;
				case UpdateActionCode.Replace:
					message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ISFReplace;
					break;
				default:
					message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ISFAdd;
					break;
			}

			new ImporterSecurityFilingMessageStatusCalculator(ISFData).CalculateStatus(message, ABIResponseStatus.Undefined);
		}

		protected override List<UpdateActionCode> GetSupportedUpdateActionCodeList()
		{
			List<UpdateActionCode> supportedList = base.GetSupportedUpdateActionCodeList();
			supportedList.Add(UpdateActionCode.Add);
			supportedList.Add(UpdateActionCode.Delete);
			supportedList.Add(UpdateActionCode.Replace);
			return supportedList;
		}

		protected override void UpdateMessageBlocks(T block)
		{
			GenerateSF10(block);
			if (action != UpdateActionCode.Delete)
			{
				GenerateSF13(block);
				GenerateSF15(block);
				GenerateSF20(block);
				GenerateSF25(block);
				GenerateRelatedOrganizationData(block);
				if (IsISF5Types)
				{
					GenerateISF5(block);
				}
				else if (IsISF10Types)
				{
					GenerateISF10(block);
				}
			}
		}

		#region Generate Message Blocks

		void GenerateISF10(T block)
		{
			// may be repeated up to 999 times
			foreach (IManufacturerData manufacturerData in ISFData.ManufacturerData)
			{
				Generate30XBlocks(DocAddressTo30BlocksMapper.New(manufacturerData.Manufacturer), block);

				// may be repeated up to 999 times too
				foreach (ITariffData tariff in manufacturerData.Tariffs)
				{
					ISFSF40 sf40 = new ISFSF40();
					sf40.HarmonizedNumber = tariff.HarmonizedTariffNumber;
					sf40.CountryOfOrigin = tariff.CountryOfOrigin;
					block.AddMessageBlock(sf40);
				}
			}
		}
		void GenerateISF5(T block)
		{
			// may be repeated up to 999 times too
			foreach (ITariffData tariff in ISFData.Tariffs)
			{
				ISFSF40 sf40 = new ISFSF40();
				sf40.HarmonizedNumber = tariff.HarmonizedTariffNumber;
				block.AddMessageBlock(sf40);
			}

			GenerateSF50(block);
		}

		void GenerateSF50(T block)
		{
			ISFSF50 sf50 = new ISFSF50();
			sf50.CodeQualifier = ISFData.CodeQualifier1;
			sf50.CodeQualifier1 = ISFData.CodeQualifier2;
			sf50.ForeignPortOfUnlading = ISFData.ForeignPortOfUnlading;
			sf50.PlaceOfDelivery = ISFData.PlaceOfDelivery;
			block.AddMessageBlock(sf50);
		}

		void GenerateSF10(T block)
		{
			ISFSF10 sf10 = new ISFSF10();
			sf10.ActionCode = UpdateActionCodeConverter.ConvertToString(action);

			if (ISFData.IORNumberQualifier == EntityIdentifierQualifierList.Codes.PassportNumber)
			{
				sf10.CountryOfIssuance = ISFData.CountryOfIssuance;
			}

			if (ISFData.IORNumberQualifier == EntityIdentifierQualifierList.Codes.SocialSecurityNumber ||
				ISFData.IORNumberQualifier == EntityIdentifierQualifierList.Codes.PassportNumber)
			{
				sf10.DateOfBirth = ISFData.DateOfBirth;
			}

			sf10.ISFImporterNumber = ISFData.IORNumber;
			sf10.ISFImporterNumberQualifier = ISFData.IORNumberQualifier;
			sf10.ActionReasonCode = ISFData.ActionReasonCode;
			if (ISFData.ShipmentTypeCode != ShipmentTypeList.Codes.Informal)
			{
				sf10.BondHolder = ISFData.ISFImporterBondHolder;
				sf10.BondActivityCode = ISFData.ISFBondActivityCode;
				sf10.BondType = ISFData.ISFBondType;
			}
			sf10.ModeOfTransportationCode = ISFData.ModeOfTransportation;
			sf10.SCACIdentifier = ISFData.SCAC;
			sf10.ISFSubmissionType = ISFData.SFSubmissionType;
			if (action != UpdateActionCode.Add)
			{
				sf10.ISFTransactionNumber = ISFData.SFTransactionNumber;
			}
			sf10.ShipmentTypeCode = ISFData.ShipmentTypeCode.Trim();
			block.AddMessageBlock(sf10);
		}

		void GenerateSF13(T block)
		{
			var shipmentSubType = ISFData.ShipmentSubType;
			var estimatedValue = ISFData.EstimatedValue;
			var estimatedQuantity = ISFData.EstimatedQuantity;
			var estimatedWeight = ISFData.EstimatedWeight;
			var weightQualifier = ISFData.WeightQualifier;

			if (!shipmentSubType.IsEmpty || !estimatedQuantity.IsEmpty || !estimatedValue.IsEmpty || !estimatedWeight.IsEmpty || !weightQualifier.IsEmpty)
			{
				var sf13 = new ISFSF13();
				sf13.ShipmentSubType = shipmentSubType;
				sf13.EstimatedValue = estimatedValue;
				sf13.EstimatedQuantity = estimatedQuantity;
				sf13.UnitOfMeasure = ISFData.UnitOfMeasure;
				sf13.EstimatedWeight = estimatedWeight;
				sf13.WeightQualifier = weightQualifier == Enterprise.Core.Constants.Weight.Kilograms ? "K" : weightQualifier == Enterprise.Core.Constants.Weight.Pounds ? "L" : weightQualifier.ToString();
				block.AddMessageBlock(sf13);
			}
		}

		void GenerateSF15(T block)
		{
			foreach (IShipmentReferenceID shipmentData in ISFData.ShipmentIDs)
			{
				ISFSF15 sf15 = new ISFSF15();
				sf15.CodeQualifier = shipmentData.CodeQualifier;
				sf15.ShipmentReferenceIdentifier = shipmentData.ShipmentReferenceIdentifier;
				block.AddMessageBlock(sf15);
			}
		}

		void GenerateSF20(T block)
		{
			foreach (IReferenceData refData in ISFData.ReferenceData)
			{
				ISFSF20 sf20 = new ISFSF20();
				sf20.ReferenceIdentifierQualifier = refData.CodeQualifier;
				sf20.ReferenceIdentifier = refData.ReferenceData;
				block.AddMessageBlock(sf20);
			}
		}

		void GenerateSF25(T block)
		{
			foreach (IContainerData containerData in ISFData.ContainerData)
			{
				ISFSF25 sf25 = new ISFSF25();
				sf25.EquipmentDescriptionCode = containerData.EquipmentDescriptionCode;
				sf25.EquipmentInitial = containerData.EquipmentInitial;
				sf25.EquipmentNumber = containerData.EquipmentNumber;
				sf25.EquipmentNumberCheckDigit = containerData.EquipmentNumberCheckDigit;
				sf25.EquipmentSizeTypeCode = containerData.EquipmentSizeTypeCode;
				block.AddMessageBlock(sf25);
			}
		}

		void GenerateRelatedOrganizationData(T block)
		{
			if (IsISF10Types)
			{
				ISFSF30 sf30ImporterOfRecord = new ISFSF30();
				sf30ImporterOfRecord.EntityName = ISFData.ImporterFullName;
				sf30ImporterOfRecord.EntityCode = CommercialEntityTypeList.Codes.ImporterOfRecord;
				sf30ImporterOfRecord.EntityIdentifierQualifier = ISFData.IORNumberQualifier;
				sf30ImporterOfRecord.EntityIdentifier = ISFData.IORNumber;
				sf30ImporterOfRecord.CountryCode = ISFData.CountryOfIssuance;
				sf30ImporterOfRecord.DOB = GetDOBFormat(ISFData.DateOfBirth);
				block.AddMessageBlock(sf30ImporterOfRecord);

				if (!ISFData.ConsigneeNumber.IsEmpty)
				{
					ISFSF30 sf30Consignee = new ISFSF30();
					sf30Consignee.EntityName = ISFData.ConsigneeFullName;
					sf30Consignee.EntityCode = CommercialEntityTypeList.Codes.Consignee;
					sf30Consignee.EntityIdentifierQualifier = ISFData.ConsigneeNumberQualifier;
					sf30Consignee.EntityIdentifier = ISFData.ConsigneeNumber;
					sf30Consignee.CountryCode = ISFData.ConsigneePassportCountryOfIssue;
					sf30Consignee.DOB = GetDOBFormat(ISFData.ConsigneePassportDateOfBirth);
					block.AddMessageBlock(sf30Consignee);
				}
			}

			// may be repeated up to 999 times
			foreach (var orgData in ISFData.RelatedOrganizationData)
			{
				I30Blocks organization = DocAddressTo30BlocksMapper.New(orgData);
				if (organization != null)
				{
					switch (organization.EntityCode)
					{
						case CommercialEntityTypeList.Codes.SellingParty:
						case CommercialEntityTypeList.Codes.BuyingParty:
						case CommercialEntityTypeList.Codes.StuffingLocation:
						case CommercialEntityTypeList.Codes.Consolidator:
						case CommercialEntityTypeList.Codes.Consignee:
							if (IsISF10Types)
							{
								Generate30XBlocks(organization, block);
							}
							break;
						case CommercialEntityTypeList.Codes.BookingParty:
							if (IsISF5Types)
							{
								Generate30XBlocks(organization, block);
							}
							break;
						case CommercialEntityTypeList.Codes.ShipToParty:
							Generate30XBlocks(organization, block);
							break;
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		ZString GetDOBFormat(ZDateTime dateOfBirth)
		{
			return dateOfBirth.ToString("MMddyyyy", CultureInfo.CurrentCulture); // dateformat
		}

		protected void Generate30XBlocks(I30Blocks organization, T block)
		{
			if (organization != null)
			{
				ISFSF30 sf30 = new ISFSF30();
				sf30.EntityCode = organization.EntityCode;

				if (!organization.EntityIdentifier.IsEmpty && !organization.EntityIdentifierQualifier.IsEmpty && !IsDUNS(organization.EntityIdentifierQualifier))
				{
					sf30.EntityIdentifierQualifier = organization.EntityIdentifierQualifier;
					sf30.EntityIdentifier = organization.EntityIdentifier;
					if (sf30.EntityIdentifierQualifier == EntityIdentifierQualifierList.Codes.SocialSecurityNumber)
					{
						sf30.DOB = GetDOBFormat(organization.SocialSecurityNumberDateOfBirth);
						sf30.EntityName = organization.LegalName;
					}
					else if (sf30.EntityIdentifierQualifier == EntityIdentifierQualifierList.Codes.PassportNumber)
					{
						sf30.CountryCode = organization.PassportCountryOfIssue;
						sf30.DOB = GetDOBFormat(organization.PassportDateOfBirth);
						sf30.EntityName = organization.LegalName;
					}

					block.AddMessageBlock(sf30);
				}

				if (ShouldReportNameAndAddressData(organization))
				{
					sf30.EntityName = organization.EntityName;

					block.AddMessageBlock(sf30);

					if (!organization.SecondaryEntityName.IsEmpty)
					{
						ISFSF31 sf31 = new ISFSF31();
						sf31.EntityCode = organization.SecondaryEntityCode;
						sf31.EntityName = organization.SecondaryEntityName;
						block.AddMessageBlock(sf31);
					}

					if (ShouldReportAddressData(sf30))
					{
						//may be repeated up to 3 times
						foreach (IAddressingInformation address in organization.AddressingInformation)
						{
							ISFSF35 sf35 = new ISFSF35();
							sf35.AddressComponentQualifier = address.AddressComponentQualifier1;
							sf35.AddressComponentQualifier1 = address.AddressComponentQualifier2;
							sf35.AddressInformation = address.AddressInformation1;
							sf35.AddressInformation1 = address.AddressInformation2;
							block.AddMessageBlock(sf35);
						}

						ISFSF36 sf36 = new ISFSF36();
						sf36.CityName = organization.City;
						sf36.CountrySubEntityCode = organization.CountrySubEntityCode;
						sf36.PostalCode = organization.PostalCode;
						sf36.CountryCode = organization.Country;
						block.AddMessageBlock(sf36);
					}
				}
			}
		}

		bool IsDUNS(ZString entityIdentifierQualifier)
		{
			return entityIdentifierQualifier == EntityIdentifierQualifierList.Codes.DUNSNumber || entityIdentifierQualifier == EntityIdentifierQualifierList.Codes.DUNSPlust4Number;
		}

		bool ShouldReportNameAndAddressData(I30Blocks organization)
		{
			return organization.EntityCode != CommercialEntityTypeList.Codes.Consignee && (organization.EntityIdentifier.IsEmpty || organization.EntityIdentifierQualifier.IsEmpty || IsDUNS(organization.EntityIdentifierQualifier));
		}

		bool ShouldReportAddressData(ISFSF30 sf30)
		{
			return sf30.EntityIdentifierQualifier != EntityIdentifierQualifierList.Codes.FIRMSCode;
		}

		#endregion

		protected override T GetNewInputBlockControlGenerator()
		{
			return (T)Activator.CreateInstance(typeof(T), ISFData.Branch);
		}

		protected override bool ShouldSetMessageBranchToAttacheeBranch
		{
			get { return true; }
		}
	}
}
