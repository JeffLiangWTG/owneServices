using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Edifact.Utilities;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CBP7512Document : NonPersistentBusinessObject
		, IObsoleteValidation
		, IDocumentDeliveredLogSupporter
		, IParentDocManagerSupport
		, IVisualizerNoteSupporter
		, IBrokerSignatureProvider
	{
		public CBP7512Document(CusInBondMoveHeader moveHeader)
			: base(moveHeader.Factory)
		{
			this.moveHeader = moveHeader;
			this.header = moveHeader.Header;
		}

		readonly CusInBondMoveHeader moveHeader;
		readonly CusInBondHeader header;
		CusInBondMoveDetail FirstMoveDetail
		{
			get { return (CusInBondMoveDetail)moveHeader.FirstMoveDetail; }
		}

		CusInBondBill FirstBill
		{
			get
			{
				var moveDetail = FirstMoveDetail;
				return moveDetail != null ? moveDetail.Bill : null;
			}
		}

		bool IsAir
		{
			get { return header != null && header.IsAir; }
		}

		public ZString InBondVia
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				result.AppendIfNotEmpty(moveHeader.BM_InBondCarrierID);
				OrgAddress inBondCarrier = moveHeader.InBondCarrier;
				if (inBondCarrier != null)
				{
					result.AppendIfNotEmpty(inBondCarrier.EffectiveCompanyNameTruncated);
				}

				return result.ToStringWithDelimiterBetweenAppends(" ");
			}
		}

		public ZString ImportedOn
		{
			get
			{
				ZString result = ZString.Empty;
				var header = this.header;
				if (header != null)
				{
					if (IsAir)
					{
						var moveHeader = this.moveHeader;
						if (moveHeader != null)
						{
							if (moveHeader.EffectiveSplitFlightNo.IsNumbersOnlyOrEmpty)
							{
								result = moveHeader.EffectiveSplitCarrierSCAC + "," + moveHeader.EffectiveSplitFlightNo;
							}
							else
							{
								result = moveHeader.EffectiveSplitFlightNo;
							}
						}
					}
					else if (header.IsSea)
					{
						result = header.BH_ImportConveyanceName + "," + this.header.BH_VoyageNumber;
					}
					else
					{
						result = header.BH_ImportConveyanceName;
					}
				}
				result = result.TrimStart(',').TrimEnd(',');
				return result;
			}
		}

		public ZBool SubmittedElectronically
		{
			get { return moveHeader.LogManager.HasAClearLog && !moveHeader.LogManager.HasAWithdrawnLog; }
		}

		public ZString AuthorizedStatement
		{
			get
			{
				var result = string.Empty;
				if (SubmittedElectronically)
				{
					if (IsAir)
					{
						var submittedMessages = moveHeader.Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.USCustomsImport,
							new ZString[] { ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, ApplicationIdentifierCodeList.Codes.AirInbondResponse }, EDIMessage.Direction.Receive).OrderByDescending(x => x.EM_SystemCreateTimeUtc);

						foreach (MQEDIMessage message in submittedMessages)
						{
							if (message.OriginalMessage != null)
							{
								if (!message.MessageBlock.MessageBlocks.OfType<IINBQT95>().Any(block => block.NarrativeMessageTypeCode == InBondAcceptanceRejectionList.Codes.Rejected))
								{
									result = string.Format("** {0} IN-BOND AUTHORIZED **", message.OriginalMessage.EM_MessageType);
									break;
								}
							}
						}
					}
					else
					{
						result = "** QP IN-BOND AUTHORIZED **";
					}
				}
				return result;
			}
		}

		public ZString PreviousITType
		{
			get
			{
				var moveDetail = FirstMoveDetail;
				return moveDetail != null ? moveDetail.B9_PreviousITType : ZString.Empty;
			}
		}

		public ZString PreviousITNumber
		{
			get
			{
				var uniquePreviousITNumbers = moveHeader.MovementDetails.Select(x => x.B9_PreviousITNumber).Where(x => !x.IsEmpty).Distinct();
				return uniquePreviousITNumbers.Count() > 1 ? "MULTIPLE" : (uniquePreviousITNumbers.Count() == 1 ? uniquePreviousITNumbers.FirstOrDefault().ToString() : "");
			}
		}

		public ZString PreviousITPort
		{
			get
			{
				var moveDetail = FirstMoveDetail;
				return moveDetail != null ? moveDetail.B9_PreviousITPortDCode : ZString.Empty;
			}
		}

		public ZDateTime PreviousITDate
		{
			get
			{
				var moveDetail = FirstMoveDetail;
				return moveDetail != null ? moveDetail.B9_PreviousITDate : ZDateTime.Empty;
			}
		}

		public ZString FormattedEntryNumber
		{
			get { return moveHeader.InBondNumber; }
		}

		public ZString EntryTypeCode
		{
			get { return moveHeader.BM_InBondEntryType; }
		}

		public ZString EntryTypeAbbreviation
		{
			get
			{
				ZString result = ZString.Empty;

				switch (moveHeader.BM_InBondEntryType)
				{
					case EntryTypeList.Codes.ImmediateTransportation:
						result = "IT";
						break;
					case EntryTypeList.Codes.TransportationExportation:
						result = "T&E";
						break;
					case EntryTypeList.Codes.ImmediateExportation:
						result = "IE";
						break;
				}

				return result;
			}
		}

		public ZString PortCode
		{
			get { return moveHeader.BM_PortOfPresentationCode; }
		}

		public ZString PortName
		{
			get
			{
				var port = moveHeader.PortOfPresentationCode;
				return port == null ? ZString.Empty : port.ZZD_Description;
			}
		}

		public ZString FirstUSPortOfUnlading
		{
			get
			{
				var header = this.header;
				return header != null ? GetRegionPortCodeAndName(header.BH_PortUnladingDCode, true) : null;
			}
		}

		public ZDateTime EntryDate
		{
			get { return moveHeader.BM_EntryDate; }
		}

		public ZString EnteredOrImportedBy
		{
			get
			{
				ZString addressForDocument = ZString.Empty;
				var header = this.header;
				OrgAddress address = header != null ? header.Importer : null;
				if (address != null)
				{
					TextSplitElegantly splitter = new TextSplitElegantly(100, 2);
					splitter.Text = address.AddressAsASingleLine;
					addressForDocument = splitter[0];
					EnteredOrImportedByOverflow = splitter[1];
				}

				return addressForDocument;
			}
		}

		public ZString EnteredOrImportedByOverflow
		{
			get { return enteredOrImportedByOverflow; }
			set { enteredOrImportedByOverflow = value; }
		}
		ZString enteredOrImportedByOverflow;

		public ZString ImporterIRSNo => OrgHeaderWrapper.GetCustomsCode(header?.ImporterOrg, new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber });

		public ZString CustomsPortDirector
		{
			get { return GetRegionPortCodeAndName(moveHeader.BM_DestinationPortCode, true); }
		}

		public ZString FinalForeignDestination
		{
			get
			{
				var result = ZString.Empty;
				var moveHeader = this.moveHeader;
				if (moveHeader.BM_ForeignDestPortKCode.IsEmpty)
				{
					var foreignPort = moveHeader.ForeignDestPort;
					if (foreignPort != null)
					{
						result = foreignPort.RL_Code + " " + foreignPort.RL_PortName;
					}
				}
				else
				{
					result = GetForeignPortCodeAndName(moveHeader.BM_ForeignDestPortKCode);
				}
				return result;
			}
		}

		public ZString Consignee
		{
			get
			{
				ZString result = ZString.Empty;
				var firstBill = FirstBill;
				if (firstBill != null && !firstBill.Consignee.IsEmpty)
				{
					result = firstBill.Consignee.AddressAsASingleLine;
				}
				else
				{
					var header = this.header;
					OrgAddress importer = header != null ? header.Importer : null;
					if (importer != null)
					{
						result = importer.AddressAsASingleLine;
					}
				}

				return result;
			}
		}

		public ZString ForeignPortLading
		{
			get
			{
				var header = this.header;
				return header != null ? GetForeignPortCodeAndName(header.BH_ImportLoadPortKCode) : ZString.Empty;
			}
		}

		public ZString BLNo
		{
			get
			{
				CusInBondBill firstBill = null;
				var masterBill = ZString.Empty;
				var hasMultipleMasterBills = false;
				foreach (var bill in moveHeader.MovementDetails.Select(x => x.Bill)
					.Where(x => x != null && !x.B0_MasterBillNumber.IsEmpty).Distinct())
				{
					if (firstBill == null)
					{
						firstBill = bill;
					}

					if (masterBill.IsEmpty)
					{
						masterBill = bill.IssuerCodeAndMasterBillNumber;
					}
					else if (masterBill != bill.IssuerCodeAndMasterBillNumber)
					{
						hasMultipleMasterBills = true;
						break;
					}
				}

				var result = ZString.Empty;
				if (hasMultipleMasterBills)
				{
					result = "MULTIPLE";
				}
				else if (firstBill != null)
				{
					result = IsAir ? firstBill.B0_MasterBillNumber : firstBill.IssuerCodeAndMasterBillNumber;
				}
				return result;
			}
		}

		public ZDateTime DateOfSailing
		{
			get
			{
				var header = this.header;
				return header != null ? header.BH_SailingDate : ZDateTime.Empty;
			}
		}

		public ZString Flag
		{
			get
			{
				var header = this.header;
				return header != null && header.IsSea ? header.BH_ImportConveyanceCountry : ZString.Empty;
			}
		}

		public ZDateTime DateImported
		{
			get
			{
				var header = this.header;
				return header != null ? header.BH_ETA : ZDateTime.Empty;
			}
		}

		public ZString Via
		{
			get
			{
				var moveHeader = this.moveHeader;
				return moveHeader != null ? GetForeignPortCodeAndName(moveHeader.BM_Via) : ZString.Empty;
			}
		}

		public ZString ExportedFrom
		{
			get
			{
				ZString result = ZString.Empty;
				var header = this.header;
				if (header != null)
				{
					result = header.BH_RN_NKFirstExportCountry;
					var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, header.BH_RN_NKFirstExportCountry);
					if (country != null)
					{
						result = result + " (" + country.RN_DescMultilingual.GetUnresolvedString() + ")";
					}
				}

				return result;
			}
		}

		public ZDateTime DateExported
		{
			get
			{
				var header = this.header;
				return header != null ? header.BH_FirstExportDate : ZDateTime.Empty;
			}
		}

		public ZString GoodsNowAt
		{
			get
			{
				var builder = new ZStringBuilder();
				var header = this.header;
				if (header != null)
				{
					var firms = header.FIRMS;
					if (firms != null)
					{
						builder.AppendIfNotEmpty(firms.ZZD_Description);
					}
					builder.AppendIfNotEmpty(header.BH_FIRMS);
				}
				return builder.ToStringWithDelimiterBetweenAppends(" ");
			}
		}

		public CBP7512DocumentLineCollection Lines
		{
			get { return lines ?? (lines = new CBP7512DocumentLineCollection(moveHeader)); }
		}
		CBP7512DocumentLineCollection lines;

		public ZString ITNoBarCode
		{
			get { return !moveHeader.InBondNumber.IsEmpty ? (IsAir ? "QX10" : "QP01") + moveHeader.InBondNumber : ""; }
		}

		public OrgHeader InBondEnteredOrImportedBy
		{
			get
			{
				ZGuid organizationPK = ZGuid.Empty;
				var header = this.header;
				GlbBranch branch = header != null ? header.Branch : null;
				if (branch != null && !branch.GB_OH_OrgProxy.IsEmpty)
				{
					organizationPK = branch.GB_OH_OrgProxy;
				}
				else
				{
					organizationPK = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				}

				return Factory.Load<OrgHeader>(organizationPK);
			}
		}

		OrgAddress CustomsAddressOfRecord
		{
			get
			{
				if (fCustomsAddressOfRecord == null)
				{
					if (header != null && header.Branch.OrgProxy != null)
					{
						fCustomsAddressOfRecord = header.Branch.OrgProxy.GetCustomsAddressOfRecord();
					}
				}

				return fCustomsAddressOfRecord;
			}
		}
		OrgAddress fCustomsAddressOfRecord;

		OrgAddress CustomsAddressOfRecordOrMainAddress
		{
			get
			{
				OrgAddress result = null;

				if (CustomsAddressOfRecord != null)
				{
					result = CustomsAddressOfRecord;
				}
				else if (InBondEnteredOrImportedBy != null)
				{
					result = InBondEnteredOrImportedBy.MainAddress;
				}

				return result;
			}
		}

		public ZString EnteredOrWithdrawnBy
		{
			get
			{
				var address = CustomsAddressOfRecordOrMainAddress;
				return address != null ? address.EffectiveCompanyNameTruncated : ZString.Empty;
			}
		}

		public ZString EnteredOrWithdrawnByAddress1
		{
			get
			{
				var address = CustomsAddressOfRecordOrMainAddress;
				return address != null ? address.OA_Address1 : ZString.Empty;
			}
		}

		public ZString EnteredOrWithdrawnByAddress2
		{
			get
			{
				var address = CustomsAddressOfRecordOrMainAddress;
				var result = ZString.Empty;

				if (address != null)
				{
					if (address.OA_Address2.IsEmpty)
					{
						result = address.OA_City + " " + address.OA_State + " " + address.OA_PostCode;
					}
					else
					{
						result = address.OA_Address2;
					}
				}

				return result;
			}
		}

		public ZString EnteredOrWithdrawnByAddress3
		{
			get
			{
				var result = ZString.Empty;
				var address = CustomsAddressOfRecordOrMainAddress;

				if (address != null)
				{
					if (!address.OA_Address2.IsEmpty)
					{
						result = new ZString(address.OA_City + " " + address.OA_State + " " + address.OA_PostCode);
					}
				}

				return result;
			}
		}

		public ZString AttorneyInFact
		{
			get
			{
				var result = ZString.Empty;
				var broker = BrokerProvider.Signatory;
				if (broker != null)
				{
					result = broker.GS_FullName;
				}

				var header = this.header;
				if (header != null && header.IsAttorneyInFact)
				{
					result += " ATTY-IN-FACT";
				}

				return result;
			}
		}

		public ZString DeclarationReference
		{
			get
			{
				var header = this.header;
				return "Job Ref: " + (header != null ? header.BH_JobReference : ZString.Empty);
			}
		}

		public ZString CertificateOfLadingPort
		{
			get { return GetRegionPortCodeAndName(moveHeader.BM_DestinationPortCode, true); }
		}

		public ZString USSeal
		{
			get { return moveHeader.BM_Seals; }
		}

		public ZString ExportLadenOn
		{
			get { return moveHeader.BM_ExportLadenOn; }
		}

		public ZString ForPortDirector
		{
			get { return ZString.Empty; }
		}

		public ZString ClearedFor
		{
			get { return IsEntryTypeExportation ? GetForeignPortCodeAndName(moveHeader.BM_ForeignDestPortKCode) : GetRegionPortCodeAndName(moveHeader.BM_DestinationPortCode, false); }
		}

		public bool IsEntryTypeExportation => moveHeader.BM_InBondEntryType == InbondCommonTypeList.Codes._2TransportandExport || moveHeader.BM_InBondEntryType == InbondCommonTypeList.Codes._3ImmediateExport;

		public ZDateTime ClearedOn
		{
			get { return moveHeader.BM_ExportDate; }
		}

		public ZString Inspector1
		{
			get { return ZString.Empty; }
		}

		public ZString Inspector2
		{
			get { return ZString.Empty; }
		}

		public ZString Inspector1Date
		{
			get { return ZString.Empty; }
		}

		public ZString Inspector2Date
		{
			get { return ZString.Empty; }
		}

		public ZString AttorneyOrAgentOfCarrier
		{
			get { return InBondVia; }
		}

		public ZString GONumber
		{
			get { return moveHeader.BM_GONumber; }
		}

		public ZString PedimentoNumber
		{
			get
			{
				var result = ZString.Empty;
				var pedimentoNumberFromBill = moveHeader.PedimentoNumber;
				if (!pedimentoNumberFromBill.IsEmpty)
				{
					result = "Pedimento: " + pedimentoNumberFromBill;
				}
				else
				{
					result = moveHeader.BM_PedimentoNumber;
				}

				return result;
			}
		}

		public ZString ExpDate => new RefSysConfig.Loader(Factory).GetStringValue("CBP7512ED");

		public ZString RevDate => new RefSysConfig.Loader(Factory).GetStringValue("CBP7512RD");

		ZString GetRegionPortCodeAndName(ZString code, bool printCode)
		{
			ZStringBuilder result = new ZStringBuilder();
			if (printCode)
			{
				result.AppendIfNotEmpty(code);
			}
			var port = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, code, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
			if (port != null)
			{
				result.AppendIfNotEmpty(port.ZZD_Description);
			}

			return result.ToStringWithDelimiterBetweenAppends(" ");
		}

		ZString GetForeignPortCodeAndName(ZString code)
		{
			return Factory.GetCachedValue("CBP7512Document|" + code, () =>
			{
				ZStringBuilder result = new ZStringBuilder();
				result.AppendIfNotEmpty(code);
				ZZRefCusCodeListCombined port = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, code,
					Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today,
					attributeFilters:
					new[] {
					new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.PortValidType, SQLComparisonOperator.Equal,
					new ZString[] { ForeignPortTypeList.Codes.Common, ForeignPortTypeList.Codes.InBond })
					});
				if (port != null)
				{
					result.AppendIfNotEmpty(port.ZZD_Description);
				}

				return result.ToStringWithDelimiterBetweenAppends(" ");
			});
		}

		#region BackPageFields For Modifying if required

		#region Record of Cartage or Lighterage

		public ZString Conveyance1
		{
			get { return ZString.Empty; }
		}

		public ZString CartageQty1
		{
			get { return ZString.Empty; }
		}

		public ZString CartageDate1
		{
			get { return ZString.Empty; }
		}

		public ZString Cartman1
		{
			get { return ZString.Empty; }
		}

		public ZString Conveyance2
		{
			get { return ZString.Empty; }
		}

		public ZString CartageQty2
		{
			get { return ZString.Empty; }
		}

		public ZString CartageDate2
		{
			get { return ZString.Empty; }
		}

		public ZString Cartman2
		{
			get { return ZString.Empty; }
		}

		public ZString Conveyance3
		{
			get { return ZString.Empty; }
		}

		public ZString CartageQty3
		{
			get { return ZString.Empty; }
		}

		public ZString CartageDate3
		{
			get { return ZString.Empty; }
		}

		public ZString Cartman3
		{
			get { return ZString.Empty; }
		}

		public ZString CartageQtyTotal
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region Certificate of Transfer 1

		public ZString COT1_Reason
		{
			get { return ZString.Empty; }
		}

		public ZString COT1_To
		{
			get { return ZString.Empty; }
		}

		public ZString COT1_On
		{
			get { return ZString.Empty; }
		}

		public ZString COT1_At
		{
			get { return ZString.Empty; }
		}

		public ZString COT1_SealedWith
		{
			get { return ZString.Empty; }
		}

		public ZString COT1_SealNumbers
		{
			get { return ZString.Empty; }
		}

		public ZString COT1_Exceptionline1
		{
			get { return ZString.Empty; }
		}

		public ZString COT1_Exceptionline2
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region Certificate of Transfer 2

		public ZString COT2_Reason
		{
			get { return ZString.Empty; }
		}

		public ZString COT2_To
		{
			get { return ZString.Empty; }
		}

		public ZString COT2_On
		{
			get { return ZString.Empty; }
		}

		public ZString COT2_At
		{
			get { return ZString.Empty; }
		}

		public ZString COT2_SealedWith
		{
			get { return ZString.Empty; }
		}

		public ZString COT2_SealNumbers
		{
			get { return ZString.Empty; }
		}

		public ZString COT2_Exceptionline1
		{
			get { return ZString.Empty; }
		}

		public ZString COT2_Exceptionline2
		{
			get { return ZString.Empty; }
		}

		#endregion

		#endregion

		#region IDocumentDeliveredLogSupporter Members

		Type IDocumentDeliveredLogSupporter.BusinessObjectTypeToLogAgainst
		{
			get
			{
				var header = this.header;
				return header != null ? header.GetType() : null;
			}
		}

		ZGuid IDocumentDeliveredLogSupporter.Identifier
		{
			get
			{
				var header = this.header;
				return header != null ? header.PK : ZGuid.Empty;
			}
		}

		#endregion

		#region IParentDocManagerSupport Members

		ZGuid IParentDocManagerSupport.ParentGuid
		{
			get
			{
				var header = this.header;
				return header != null ? header.PK : ZGuid.Empty;
			}
		}

		ZString IParentDocManagerSupport.ParentTableName
		{
			get
			{
				var header = this.header;
				return header != null ? header.TableName : string.Empty;
			}
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(header, Core.Constants.DocManagerCodes.InBond)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IVisualizerNoteSupporter Members

		ZGuid IVisualizerNoteSupporter.PK
		{
			get
			{
				var moveHeader = this.moveHeader;
				return moveHeader != null ? moveHeader.PK : ZGuid.Empty;
			}
		}

		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;

		string IVisualizerNoteSupporter.TableCode
		{
			get
			{
				var moveHeader = this.moveHeader;
				return moveHeader != null ? moveHeader.TablePrefix : string.Empty;
			}
		}

		#endregion

		#region IBrokerSignatureProvider Members

		Guid IBrokerSignatureProvider.RegistryBranchPK
		{
			get { return moveHeader.RegistryBranchPK; }
		}

		Guid IBrokerSignatureProvider.RegistryCompanyPK
		{
			get { return moveHeader.RegistryCompanyPK; }
		}

		bool IBrokerSignatureProvider.HasCurrentElectronicRelease
		{
			get { return false; }
		}

		GlbStaff IBrokerSignatureProvider.CusAgent
		{
			get { return moveHeader.CusAgent; }
		}

		BusinessObjectFactory IBrokerSignatureProvider.Factory
		{
			get { return Factory; }
		}

		PrintBrokerSignatureProvider BrokerProvider
		{
			get { return fBrokerProvider ?? (fBrokerProvider = new PrintBrokerSignatureProvider(this)); }
		}
		PrintBrokerSignatureProvider fBrokerProvider;

		#endregion
	}
}
