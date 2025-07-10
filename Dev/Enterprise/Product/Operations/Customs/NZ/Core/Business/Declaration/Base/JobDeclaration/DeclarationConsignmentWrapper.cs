using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class DeclarationConsignmentWrapper : ICREConsignment, ICREConsignmentItem, IICRConsignment, IICRConsignmentItem
	{
		public DeclarationConsignmentWrapper(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, "Declaration cannot be null");
		}
		readonly JobDeclaration declaration;

		#region ICREConsignment Implementation

		ZBool ICREConsignment.WriteOffRequest => true; //Cargo Report Export is a WriteOffRequest

		ZShort ICREConsignment.SequenceNumber => 0;

		ITranshipmentDetails ICREConsignment.TranshipmentDetails => declaration.TranshipmentRequest;

		ZString ICREConsignment.HandlingInfo
		{
			get { return GoodsHandling; }
		}

		ZDecimal ICREConsignment.ConsignmentValueInNZD => ECIWriteOffValueInNZD;

		IPartyInformation ICREConsignment.Consignee => new OrgHeaderWrapper(declaration.Consignee);

		IEnumerable<ICREConsignmentItem> ICREConsignment.ConsignmentItems
		{
			get { return GetConsignmentItems; }
		}

		protected virtual IEnumerable<ICREConsignmentItem> GetConsignmentItems => ConsignmentItems;

		IPartyInformation ICREConsignment.Consignor => new OrgHeaderWrapper(declaration.Consignor);

		IOrganisation ICREConsignment.DeliverToParty
		{
			// Must be transmitted if different to consignee
			// State the name of the party that the goods will be delivered to if different to the Consignee
			get { return OrgHeaderWrapper.New(declaration.DeliveryDestinationPartyDocAddress.Organisation, declaration.DeliveryDestinationPartyDocAddress); }
		}

		ZString ICREConsignment.FreightPaymentMethod
		{
			get { return declaration.JE_PaymentMethod; }
		}

		ZString ICREConsignment.GoodsLocation
		{
			//	Air - Must be transmitted to state the Cargo Terminal Operator / consolidator / freight forwarder responsible for export loading
			//	Sea - Must be transmitted to state any consolidator / freight forwarder responsible for export loading. Not required when goods are delivered direct to the Port company 
			//			TSW Premises code - A list of codes will be published in due course
			get { return declaration.JE_Cal_GoodsLocation; }
		}

		ZString ICREConsignment.PortOfLoading
		{
			get { return declaration.JE_RL_NKPortOfLoading; }
		}

		IEnumerable<IPartyInformation> ICREConsignment.NotifyParties
		{
			get { yield return OrgHeaderWrapper.New(declaration.NotifyPartyDocumentaryAddress.Organisation); }
		}

		IEnumerable<IOrganisationSimple> ICREConsignment.DeliveryNotifyParties
		{
			get { yield return OrgHeaderWrapper.New(declaration.NotifyParty); }
		}

		IEnumerable<ZString> ICREConsignment.NotifyPartyCodes
		{
			get
			{
				yield return declaration.NotifyParty2DocumentaryAddress.GetCCPOrATFCode();
				yield return declaration.JE_RL_NKPortOfDeliveryNotify;
			}
		}

		IAssociatedTransportDocument ICREConsignment.BillNumber
		{
			get
			{
				var bill = declaration.PrimaryHouseBill ?? declaration.PrimaryMasterBill;
				return bill != null ? new BillNumberWrapper((Bill)bill) : null;
			}
		}

		IOrganisationSimple ICREConsignment.Consolidator
		{
			get { return OrgHeaderWrapper.New(declaration.Forwarder); }
		}

		ZBool ICREConsignment.HasContainers => ConsignmentHasContainers;

		IEnumerable<ITransportEquipment> ICREConsignment.Containers => ConsignmentContainers;

		ZString ICREConsignment.PortOfDischarge
		{
			get { return declaration.JE_RL_NKFinalDestination; }
		}

		#endregion

		#region ICREConsignmentItem Implementation

		ZShort ICREConsignmentItem.SequenceNumber => 1;

		ZBool ICREConsignmentItem.IsEmptyContainer
		{
			get { return false; }
		}

		ZString ICREConsignmentItem.GoodsDescription
		{
			get { return declaration.JE_GoodsDescription; }
		}

		IEnumerable<ICommodity> ICREConsignmentItem.Identifiers => Enumerable.Empty<ICommodity>(); // Only available when using Invoice Line not from Declaration only write-off

		ZDecimal ICREConsignmentItem.Value
		{
			get { return declaration.JE_ECI_InvoiceAmount; }
		}

		ZString ICREConsignmentItem.Currency
		{
			get { return declaration.ECI_InvoiceCurrency?.RX_Code ?? ZString.Empty; }
		}

		IEnumerable<IClassification> ICREConsignmentItem.Classifications => Enumerable.Empty<IClassification>(); // Standard write-off using Dec details only - no invoice line data

		ZDecimal ICREConsignmentItem.GrossWeightInKg
		{
			get { return declaration.JE_DeclaredWeight; }
		}

		ZString ICREConsignmentItem.GoodsOriginCountry
		{
			get { return declaration.JE_RL_NKOrigin.Left(2); }
		}

		ZInt ICREConsignmentItem.PackageQty
		{
			get { return declaration.JE_TotalNoOfPacks; }
		}

		ZString ICREConsignmentItem.PackageType
		{
			get { return declaration.JE_TotalNoOfPacksPackType; }
		}

		ZString ICREConsignmentItem.ContainerNumber
		{
			get
			{
				return declaration.CusContainers.Cast<CusContainer>().FirstOrDefault()?.CO_ContainerNumber ?? ZString.Empty; // Will only ever be 1 container at most in this case
			}
		}

		ZString ICREConsignmentItem.UNDGHazardousGoodsCode
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IICRConsignment Implementation

		ZBool IICRConsignment.WriteOffRequest
		{
			get
			{
				var deminimusValue = UniversalReferenceHelper.GetTaxOrFee(declaration.Factory, RateTypes.Deminimus, ZDateTime.Today);
				return ECIWriteOffValueInNZD > 0 && ECIWriteOffValueInNZD < deminimusValue;
			}
		}

		ZBool IICRConsignment.IsLinkEmptyContainer => declaration.IsTSWEmptyContainerWriteOff;

		ITranshipmentDetails IICRConsignment.TranshipmentDetails => declaration.TranshipmentRequest;

		ZBool IICRConsignment.IsConsolidation
		{
			get { return false; }
		}

		ZInt IICRConsignment.SequenceNumber => 0;

		ZDecimal IICRConsignment.ConsignmentValueInNZD
		{
			get { return ECIWriteOffValueInNZD; }
		}

		IEnumerable<ZString> IICRConsignment.Permits
		{
			get
			{
				foreach (PermitCode permit in declaration.PermitCodes)
				{
					yield return permit.ZO_Data;
				}
			}
		}

		ZBool IICRConsignment.MAFContainerDeclaration => declaration.JE_SendMCDContainerQuarantineDeclaration;

		//format container number, MCD Response eg ABCU1234560,YYNNN
		IEnumerable<ZString> IICRConsignment.MAFContainerStatements
		{
			get
			{
				if (((IICRConsignment)this).MAFContainerDeclaration)
				{
					var formattedMAFContainerDeclaration = declaration.MCDOtherInfoValue;
					foreach (CusContainer container in declaration.CusContainers)
					{
						yield return container.CO_ContainerNumber + "," + formattedMAFContainerDeclaration;
					}
				}
			}
		}

		//format Container number,MAS Approved System number (e.g. ABCU1234567,10635)
		IEnumerable<ZString> IICRConsignment.MPIApprovedSystemNumbers
		{
			get
			{
				if (NZCustomsDataRegistry.Instance.EnableSeaICRFields.Value)
				{
					foreach (CusContainer container in declaration.CusContainers)
					{
						if (!container.CO_MPIApprovedSystemNumber.IsEmpty)
						{
							yield return container.CO_ContainerNumber + "," + container.CO_MPIApprovedSystemNumber;
						}
					}
				}
			}
		}

		ZString IICRConsignment.MasterBill
		{
			get { return declaration.JE_MasterBill; }
		}

		IPartyInformation IICRConsignment.Consignee => new OrgHeaderWrapper(declaration.Consignee);

		IPartyInformation IICRConsignment.Consignor => new OrgHeaderWrapper(declaration.Consignor);

		IOrganisation IICRConsignment.DeliverToParty
		{
			get { return OrgHeaderWrapper.New(declaration.DeliveryDestinationPartyDocAddress.Organisation, declaration.DeliveryDestinationPartyDocAddress); }
		}

		ZString IICRConsignment.FreightPaymentMethod
		{
			get { return ZString.Empty; }   //TODO: Declaration.Invoices.InvoiceIncoterms
		}

		ZString IICRConsignment.PortOfOrigin
		{
			get { return declaration.JE_RL_NKOrigin; }
		}

		ZString IICRConsignment.GoodsLocation
		{
			get { return GoodsLocation; }
		}

		ZString IICRConsignment.PortOfLoading
		{
			get { return declaration.JE_RL_NKPortOfLoading; }
		}

		IPartyInformation IICRConsignment.NotifyParty
		{
			get { return null; }
		}

		IEnumerable<IOrganisationSimple> IICRConsignment.DeliveryNotifyParties
		{
			get { yield return OrgHeaderWrapper.New(declaration.NotifyParty); }
		}

		IEnumerable<ZString> IICRConsignment.NotifyPartyCodes
		{
			get
			{
				yield return declaration.NotifyParty2DocumentaryAddress.GetCCPOrATFCode();
				yield return declaration.JE_RL_NKPortOfDeliveryNotify;
			}
		}

		IEnumerable<IOrganisation> IICRConsignment.ContainerPackingLocations
		{
			get
			{
				foreach (CusContainer container in declaration.CusContainers)
				{
					if (container.StuffingEstablishmentAddress != null)
					{
						yield return OrgAddressWrapper.New(container.StuffingEstablishmentAddress);
					}
				}
			}
		}

		IEnumerable<ZString> IICRConsignment.TranshipmentPorts
		{
			get
			{
				foreach (Transport routingLeg in declaration.Transports)
				{
					if (!routingLeg.JW_RL_NKLoadPort.IsEmpty)
					{
						yield return routingLeg.JW_RL_NKLoadPort;
					}

					if (!routingLeg.JW_RL_NKDiscPort.IsEmpty)
					{
						var dischargeCountry = routingLeg.JW_RL_NKDiscPort.Left(2);
						if (dischargeCountry != Core.Constants.CountryCodes.NewZealand)
						{
							yield return routingLeg.JW_RL_NKDiscPort;
						}
					}
				}
			}
		}

		ZString IICRConsignment.BillNumber
		{
			get { return declaration.JE_HouseBill; }
		}

		ZString IICRConsignment.BillType
		{
			get { return Enterprise.Customs.NZ.TradeSingleWindow.BillTypeList.Codes.HWB; }
		}

		IOrganisationSimple IICRConsignment.Deconsolidator
		{
			get { return null; }
		}

		ZBool IICRConsignment.HasContainers => ConsignmentHasContainers;

		IEnumerable<ITransportEquipment> IICRConsignment.Containers => ConsignmentContainers;

		ZString IICRConsignment.PortOfDischarge
		{
			get { return declaration.JE_RL_NKPortOfArrival; }
		}

		ZString IICRConsignment.HandlingInformation
		{
			get { return GoodsHandling; }
		}

		ZString IICRConsignment.MPIAccountDetails
		{
			get
			{
				var result = ZString.Empty;
				if (declaration != null && !declaration.AccountNumberToBeSent.IsEmpty)
				{
					result = declaration.AccountNumberToBeSent + "," + declaration.AccountHolderNameToBeSent;
				}

				return result;
			}
		}

		IEnumerable<IICRConsignmentItem> IICRConsignment.ConsignmentItems
		{
			get { return GetICRConsignmentItems; }
		}

		protected virtual IEnumerable<IICRConsignmentItem> GetICRConsignmentItems
		{
			get
			{
				if (declaration.IsTSWEmptyContainerWriteOff)
				{
					foreach (CusContainer container in declaration.CusContainers)   // A separate consignment item is required for each empty container
					{
						yield return new EmptyContainerConsignmentItemWrapper(container);
					}
				}
				else
				{
					if (declaration.InvoiceLines.Count > 0)
					{
						foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
						{
							yield return new ICRJobComInvoiceLineWrapper(invoiceLine);   // line information has been used for this write-off entry
						}
					}
					else if (declaration.IsECIManifestDeclarationReference)
					{
						yield return this;
					}
					else
					{
						yield return new TSWEntryHeaderWrapper((ECIWriteOff.CusEntryHeader)declaration.CusEntryHeader, null);
					}
				}
			}
		}

		ZString GetGSTValue(Func<JobComInvoiceHeader, ZString> valueSector)
		{
			var gstValue = ZString.Empty;
			if (declaration.IsECIWriteoffAndGSTIsApplicable && declaration.HasTheSameGSTDetailsOnAllInvoices)
			{
				gstValue = declaration.Invoices.Cast<JobComInvoiceHeader>().Select(valueSector).FirstOrDefault();
			}
			return gstValue;
		}

		ZString IICRConsignment.IsGSTPrePaid => GetGSTValue(i => i.JZ_IsGSTPrePaid);
		ZString IICRConsignment.VendorIdentifier => GetGSTValue(i => i.JZ_SupplierGSTNumber).KeepAlphanumericCharacters();
		ZString IICRConsignment.ApprovedTransitionalFacilityCode
		{
			get
			{
				var atfCode = ZString.Empty;
				var deliveryDestination = declaration.DeliveryDestinationPartyDocAddress.Organisation;
				if (deliveryDestination != null)
				{
					atfCode = GetCustomsCodeForAddress(deliveryDestination, declaration.DeliveryDestinationPartyDocAddress.E2_OA_Address, OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility);
				}

				return atfCode;
			}
		}

		IEnumerable<ITSWAttachment> IICRConsignment.SupportingDocuments => Array.Empty<ITSWAttachment>();

		ZString IICRConsignmentItem.MPIApprovedSystemNumber => ZString.Empty;

		#endregion

		#region IICRConsignmentItem Implementation

		ZShort IICRConsignmentItem.SequenceNumber => 1;

		ZBool IICRConsignmentItem.IsEmptyContainer
		{
			get { return false; }
		}

		ZString IICRConsignmentItem.GoodsDescription
		{
			get { return declaration.JE_GoodsDescription; }
		}

		ZString IICRConsignmentItem.IdentityNumber
		{
			get { return ZString.Empty; }   // Conditional - not used business rule
		}

		ZDecimal IICRConsignmentItem.Value
		{
			get { return declaration.JE_ECI_InvoiceAmount; }
		}

		ZString IICRConsignmentItem.Currency
		{
			get { return declaration.ECI_InvoiceCurrency?.RX_Code ?? ZString.Empty; }
		}

		ZString IICRConsignmentItem.IdentityType
		{
			get { return ZString.Empty; }   // Conditional - not used business rule
		}

		IEnumerable<IClassification> IICRConsignmentItem.Classifications => Enumerable.Empty<IClassification>();

		ZBool IICRConsignmentItem.SendFlashpointTemp => false;

		ZDecimal IICRConsignmentItem.FlashpointTempInCelsius
		{
			get { return ZDecimal.Zero; } // TODO: FlashpointTempInCelsius
		}

		ITemperatureRequirements IICRConsignmentItem.Temperatures
		{
			get { return null; } // TODO: Temperatures
		}

		ZDecimal IICRConsignmentItem.GrossWeightInKg
		{
			get { return declaration.JE_DeclaredWeight; }
		}

		ZString IICRConsignmentItem.GoodsOriginCountry
		{
			get { return declaration.JE_RL_NKOrigin.Left(2); }
		}

		ZInt IICRConsignmentItem.PackageQty
		{
			get { return declaration.JE_TotalNoOfPacks; }
		}

		ZString IICRConsignmentItem.PackageType
		{
			get { return declaration.JE_TotalNoOfPacksPackType; }
		}

		ZString IICRConsignmentItem.ContainerNumber
		{
			get
			{
				var result = ZString.Empty;

				if (declaration.CusContainers.Count > 0)
				{
					result = declaration.CusContainers[0].CO_ContainerNumber;
				}

				return result;
			}
		}

		#endregion

		#region Implementation

		IEnumerable<ICREConsignmentItem> ConsignmentItems
		{
			get
			{
				if (declaration.IsTSWEmptyContainerWriteOff)
				{
					foreach (CusContainer container in declaration.CusContainers)   // A separate consignment item is required for each empty container - CRE used for empty containers only
					{
						yield return new EmptyContainerConsignmentItemWrapper(container);
					}
				}
				else
				{
					if (declaration.InvoiceLines.Count > 0)
					{
						foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
						{
							yield return new CREJobComInvoiceLineWrapper(invoiceLine);   // line information like dangerous goods code or classification has been used for this entry
						}
					}
					else if (declaration.IsECIManifestDeclarationReference)
					{
						yield return this;
					}
					else
					{
						yield return new TSWEntryHeaderWrapper((ECIWriteOff.CusEntryHeader)declaration.CusEntryHeader, null);
					}
				}
			}
		}

		ZDecimal ECIWriteOffValueInNZD
		{
			get
			{
				var result = ZDecimal.Zero;

				if (declaration.JE_ECI_InvoiceAmount > 0)
				{
					var currency = declaration.ECI_InvoiceCurrency;

					if (currency != null)
					{
						if (currency.RX_Code == Core.Constants.CurrencyCodes.NewZealand)
						{
							result = declaration.JE_ECI_InvoiceAmount;
						}
						else
						{
							var entryHeader = declaration.CusEntryHeader;
							var eCIValue = new Money(declaration.JE_ECI_InvoiceAmount, currency);
							result = entryHeader.CurrencyConverter.ConvertExact(eCIValue, entryHeader.CurrencyConverter.LocalCurrency).Amount;
						}
					}
				}

				return result;
			}
		}

		ZString GoodsLocation
		{
			//	Air - Must be transmitted to state the Cargo Terminal Operator / consolidator / freight forwarder responsible for export loading
			//	Sea - Must be transmitted to state any consolidator / freight forwarder responsible for export loading. Not required when goods are delivered direct to the Port company 
			//			TSW Premises code - A list of codes will be published in due course
			get { return declaration.JE_Cal_GoodsLocation; }
		}

		#region Goods Handling

		public ZString GoodsHandling
		{
			get
			{
				StmNote goodsHandlingNote = GetGoodsHandlingNote();
				return goodsHandlingNote != null ? new ZString(new string(goodsHandlingNote.ST_NoteText.Replace("\r\n", " ").Replace("\r", "").Replace("\n", "").ToString().Take(512).ToArray())) : ZString.Empty;
			}
		}

		protected StmNote GetGoodsHandlingNote()
		{
			StmNote[] handlingInstructionsNotes = declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
			return handlingInstructionsNotes.Length > 0 ? handlingInstructionsNotes[0] : null;
		}

		#endregion

		#region Containers

		ZBool ConsignmentHasContainers => PackingGroupInContainers.Any();

		IEnumerable<ITransportEquipment> ConsignmentContainers
		{
			get
			{
				foreach (var packGroup in PackingGroupInContainers)
				{
					yield return new ContainerWrapper(packGroup);
				}
			}
		}

		IEnumerable<PackingGroup> PackingGroupInContainers => declaration.PackingGroups.Cast<PackingGroup>().Where(x => x.Container != null);

		#endregion

		public ZString GetCustomsCodeForAddress(OrgHeader organisation, ZGuid addressToMatch, ZString customsCode)
		{
			var result = ZString.Empty;
			if (organisation != null)
			{
				foreach (OrgCusCode cusCode in organisation.CustomsCodes.GetOrgCusCodesForCodeAndCountry(customsCode, Core.Constants.CountryCodes.NewZealand))
				{
					if (cusCode.OK_OA_PremisesAddress == addressToMatch)
					{
						result = cusCode.OK_CustomsRegNo;
						break;
					}
					else if (cusCode.OK_OA_PremisesAddress.IsEmpty || cusCode.OK_OA_PremisesAddress == organisation.MainAddress.PK)    // default code for all non specific addresses
					{
						result = cusCode.OK_CustomsRegNo;
					}
				}
			}

			return result;
		}

		#endregion
	}
}
