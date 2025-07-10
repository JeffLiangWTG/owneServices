using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondMoveHeader : US.Business.CusInBondMoveHeader,
		Integration.Customs.US.USAMS.ICusInBondMoveHeader,
		IManifestMessageAttachee,
		IManifestPendingMessagesAttachee,
		ICBPEDIMessageMessageTextNumberPlaceHolderFiller,
		IMessageAttacheeInHeader,
		IValidationModesSupporter,
		ICusCodeDataTypeSupporter,
		IMessageResponseNotificator,
		IVesselArrivalMessageAttachee
	{
		public new class Schema : US.Business.CusInBondMoveHeader.Schema
		{
			public const string BM_ManifestSequenceNumber = "BM_ManifestSequenceNumber";
			public const string BM_IsSubsequentInBond = "BM_IsSubsequentInBond";
			public const string BM_SubApplicationCodeDescription = "BM_SubApplicationCodeDescription";
			public const string InBondDepartureStatus = "InBondDepartureStatus";
			public const string InBondDepartureStatusDesc = "InBondDepartureStatusDesc";
			public const string InBondArrivalStatus = "InBondArrivalStatus";
			public const string InBondArrivalStatusDesc = "InBondArrivalStatusDesc";
			public const string InBondExportationStatus = "InBondExportationStatus";
			public const string InBondExportationStatusDesc = "InBondExportationStatusDesc";
			public const string InBondTransferOfLiabilityStatus = "InBondTransferOfLiabilityStatus";
			public const string InBondTransferOfLiabilityStatusDesc = "InBondTransferOfLiabilityStatusDesc";

			public const int BM_ManifestSequenceNumberMaxLength = 6;
		}

		public CusInBondMoveHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusInBondMoveHeader);
			}

			/// <summary>
			/// Finds Movement that match Carrier Code and Manifest Sequence Number in the current company
			/// </summary>
			/// <param name="carrierCode"></param>
			/// <param name="manifestSequenceNumber"></param>
			/// <returns></returns>
			public CusInBondMoveHeader FindByCarrierCodeAndManifestSequenceNumberAndCurrentCompany(ZString carrierCode, ZString manifestSequenceNumber)
			{
				CusInBondMoveHeader result = null;
				if (manifestSequenceNumber != "000001" && manifestSequenceNumber != "000000") // "000001" and "000000" are default values that are used by Carrier which do not use Manifest Sequence Number
				{
					var entryNumberFilter = new ZQuery(CusEntryNumSchema.CE_ParentTable, CusInBondMoveHeaderSchema.Constants.TableName);
					entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_EntryNum, manifestSequenceNumber);
					entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, ManifestSequenceNumberEntryType);
					entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_EntryLineReference, carrierCode);

					var createTimeFilter = new ZQuery();
					createTimeFilter.AddToFilter(CusEntryNumSchema.CE_IssueDate, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddMonths(-FreightDataRegistry.Instance.MAWBRecyclePeriod.Value));
					createTimeFilter.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_IssueDate, SQLComparisonOperator.Equal, null);

					entryNumberFilter.AddToFilter(createTimeFilter);

					foreach (var bizO in Factory.Load<CusEntryNumber>(entryNumberFilter))
					{
						var moveHeader = Factory.Load<CusInBondMoveHeader>(bizO.CE_ParentID);
						if (moveHeader != null)
						{
							var header = moveHeader.Header;
							if (header != null && (header.RegistryCompanyPK == GlbCompany.CurrentCompany.PK))
							{
								result = moveHeader;
								break;
							}
						}
					}
				}
				return result;
			}

			public IManifestMessageAttachee FindByManifestDataAndBillOfLading(ZString carrierCode, ZString vesselName, ZString voyageNumber, ZString districtPortOfUnladingCode, ZDate estimatedDate, ZString billOfLadingIssuerCode, ZString billOfLadingNumber, ZString refNumQualifier, ZString refNum, ZString inBondNumber, Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>> movementMatch, IComparer<IBaseBillOfLading> matchComparer = null)
			{
				IManifestMessageAttachee result = null;
				if (!carrierCode.IsEmpty && movementMatch != null)
				{
					if (!billOfLadingIssuerCode.IsEmpty || !billOfLadingNumber.IsEmpty)
					{
						result = FindByBillOfLadingData(carrierCode, vesselName, voyageNumber, districtPortOfUnladingCode, estimatedDate, billOfLadingIssuerCode, billOfLadingNumber, refNumQualifier, refNum, inBondNumber, movementMatch, matchComparer);
					}
					else
					{
						result = FindByManifestData(carrierCode, vesselName, voyageNumber, districtPortOfUnladingCode, estimatedDate, inBondNumber, movementMatch, matchComparer);
					}
				}
				return result;
			}

			#region Implemenation

			IManifestMessageAttachee FindByManifestData(ZString carrierCode, ZString vesselName, ZString voyageNumber, ZString districtPortOfUnladingCode, ZDate estimatedDate, ZString inBondNumber, Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>> movementMatch, IComparer<IBaseBillOfLading> matchComparer)
			{
				var headerQuery = new ZQuery(CusInBondHeaderSchema.BH_ApplicationCode, new ZString[] { CusInBondApplicationCodeList.Codes.AMS, CusInBondApplicationCodeList.Codes.InBond });
				headerQuery.AddToFilter(CusInBondHeaderSchema.BH_CarrierSCAC, carrierCode);
				headerQuery.AddToFilter(CusInBondHeaderSchema.BH_ImportConveyanceName, SQLComparisonOperator.StartsWith, vesselName);
				headerQuery.AddToFilter(CusInBondHeaderSchema.BH_VoyageNumber, voyageNumber);
				headerQuery.AddToFilter(CusInBondHeaderSchema.BH_PortUnladingDCode, districtPortOfUnladingCode);
				headerQuery.OrderBy = CusInBondHeaderSchema.BH_SystemCreateTimeUtc.Name;
				Tuple<long, IManifestMessageAttachee> differentETAMatched = null;
				var estimatedDateTicks = estimatedDate.IsValid ? estimatedDate.ToZDateTime().Ticks : 0;
				foreach (var header in Factory.Load<Customs.Business.CusInBondHeader>(headerQuery))
				{
					var eta = header.BH_ETA;
					var isETAMatched = eta.Date == estimatedDate;
					var matchedData = new List<IBaseBillOfLading>();
					foreach (var moveHeader in Factory.Load<Customs.Business.CusInBondMoveHeader>(new ZQuery(CusInBondMoveHeaderSchema.BM_BH, header.PK)))
					{
						foreach (var billOfLading in moveHeader.MovementDetails.OfType<IBaseBillOfLading>())
						{
							var messageAttachee = billOfLading.MessageAttachee;
							if (messageAttachee != null && movementMatch(new Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>(billOfLading, messageAttachee, inBondNumber)))
							{
								matchedData.Add(billOfLading);
							}
						}
					}

					if (matchComparer != null)
					{
						matchedData.Sort(matchComparer);
					}
					foreach (var billOfLading in matchedData)
					{
						var messageAttachee = billOfLading.MessageAttachee;
						if (isETAMatched) // Have found a MessageAttachee that match all criteria
						{
							return messageAttachee;
						}
						else
						{
							KeepMessageAttacheeClosestToETA(ref differentETAMatched, messageAttachee, estimatedDateTicks, eta);
						}
					}
				}
				return differentETAMatched == null ? null : differentETAMatched.Item2;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
			IManifestMessageAttachee FindByBillOfLadingData(ZString carrierCode, ZString vesselName, ZString voyageNumber, ZString districtPortOfUnladingCode, ZDate estimatedDate, ZString billOfLadingIssuerCode, ZString billOfLadingNumber, ZString refNumQualifier, ZString refNum, ZString inBondNumber, Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>> movementMatch, IComparer<IBaseBillOfLading> matchComparer)
			{
				var billQuery = new ZQuery(CusInBondBillSchema.B0_IssuerCode, billOfLadingIssuerCode);
				billQuery.AddToFilter(CusInBondBillSchema.B0_MasterBillNumber, billOfLadingNumber);
				IManifestMessageAttachee differentCarrierMatched = null;
				Tuple<long, IManifestMessageAttachee> differentETAMatched = null;
				Tuple<long, IManifestMessageAttachee> differentCarrierAndETAMatched = null;
				var bills = Factory.Load<Customs.Business.CusInBondBill>(billQuery).Where(b => b.Header != null && (b.Header.BH_ApplicationCode == CusInBondApplicationCodeList.Codes.AMS || b.Header.BH_ApplicationCode == CusInBondApplicationCodeList.Codes.InBond));
				var estimatedDateTicks = estimatedDate.IsValid ? estimatedDate.ToZDateTime().Ticks : 0;

				if (bills.Any())
				{
					var hasOnlyOneBill = bills.Count() == 1;
					var hasOceanBillRefNumber = BillReferenceList.Codes.OB == refNumQualifier && !refNum.IsEmpty;
					var hasBillWithActiveHeader = bills.Any(b => b.Header.BH_IsActive);
					if (!hasBillWithActiveHeader)
					{
						bills = bills.OrderByDescending(b => b.Header.BH_SystemCreateTimeUtc);
					}

					foreach (var bill in bills)
					{
						var header = bill.Header;
						if (header.BH_IsActive || !hasBillWithActiveHeader)
						{
							var isBillMatched = false;
							if (hasOnlyOneBill)
							{
								isBillMatched = true;
							}
							else
							{
								if (hasOceanBillRefNumber)
								{
									var referenceQuery = new ZQuery(CusInbondBillAddRefSchema.BR_B0, bill.PK);
									referenceQuery.AddToFilter(CusInbondBillAddRefSchema.BR_Qualifier, refNumQualifier);
									referenceQuery.AddToFilter(CusInbondBillAddRefSchema.BR_ReferenceNum, refNum);
									referenceQuery.FetchOnlyFromLocalCache = !bill.IsInDatabase;
									if (header.BH_ApplicationCode == CusInBondApplicationCodeList.Codes.AMS)
									{
										isBillMatched = Factory.Load<Integration.Customs.US.USAMS.ICusInbondBillAddRef>(referenceQuery).Length > 0;
									}
									else
									{
										isBillMatched = Factory.Load<Integration.Customs.US.InBond.ICusInbondBillAddRef>(referenceQuery).Length > 0;
									}
								}

								if (!isBillMatched)
								{
									var aMSHeader = header as CusInBondHeader;
									if (aMSHeader != null && aMSHeader.IsNVOCCHeader && aMSHeader.OceanBill == bill)
									{
										isBillMatched = true;
									}
									else if (header.BH_ImportConveyanceName.StartsWith(vesselName, StringComparison.OrdinalIgnoreCase) &&
										header.BH_VoyageNumber.Trim().EqualsIgnoringCase(voyageNumber) &&
										GetDistrictPortOfUnladingCode(header, bill) == districtPortOfUnladingCode)
									{
										isBillMatched = true;
									}
								}
							}

							if (isBillMatched)
							{
								var eta = header.BH_ETA;
								var isETAMatched = eta.Date == estimatedDate;
								var matchedData = new List<IBaseBillOfLading>();
								foreach (var billOfLading in Factory.Load<Customs.Business.CusInBondMoveDetail>(new ZQuery(CusInBondMoveDetailSchema.B9_B0, bill.PK)).OfType<IBaseBillOfLading>())
								{
									var messageAttachee = billOfLading.MessageAttachee;
									if (messageAttachee != null && movementMatch(new Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>(billOfLading, messageAttachee, inBondNumber)))
									{
										matchedData.Add(billOfLading);
									}
								}

								if (matchComparer != null)
								{
									matchedData.Sort(matchComparer);
								}
								foreach (var billOfLading in matchedData)
								{
									var messageAttachee = billOfLading.MessageAttachee;

									if (messageAttachee.CarrierCode == carrierCode)
									{
										if (isETAMatched) // Have found a MessageAttachee that match all criteria
										{
											return messageAttachee;
										}
										else
										{
											KeepMessageAttacheeClosestToETA(ref differentETAMatched, messageAttachee, estimatedDateTicks, eta);
										}
									}
									else if (isETAMatched && differentCarrierMatched == null)
									{
										differentCarrierMatched = messageAttachee;
									}
									else
									{
										KeepMessageAttacheeClosestToETA(ref differentCarrierAndETAMatched, messageAttachee, estimatedDateTicks, eta);
									}
								}
							}
						}
					}
				}
				return (differentETAMatched != null) ? differentETAMatched.Item2 : differentCarrierMatched ?? (differentCarrierAndETAMatched != null ? differentCarrierAndETAMatched.Item2 : null);
			}

			ZString GetDistrictPortOfUnladingCode(Customs.Business.CusInBondHeader header, Customs.Business.CusInBondBill bill)
			{
				var amsHeader = header as CusInBondHeader;
				return amsHeader != null && !amsHeader.IsNVOCCHeader ? bill.B0_InBondPortOfDestDCode : header.BH_PortUnladingDCode;
			}

			static void KeepMessageAttacheeClosestToETA(ref Tuple<long, IManifestMessageAttachee> messageAttacheeWithClosestETA, IManifestMessageAttachee messageAttachee, long estimatedDateTicks, ZDateTime eta)
			{
				var etaTicks = eta.IsValid ? eta.Ticks : 0;
				var differentInTime = Math.Abs(etaTicks - estimatedDateTicks);
				if (messageAttacheeWithClosestETA == null || messageAttacheeWithClosestETA.Item1 > differentInTime)
				{
					messageAttacheeWithClosestETA = new Tuple<long, IManifestMessageAttachee>(differentInTime, messageAttachee);
				}
			}

			#endregion
		}

		public IDisposable SuspendUpdatingContainerDetail()
		{
			return new DisposableAction(
				() => IsUpdatingContainerDetailSuspended = true,
				() => IsUpdatingContainerDetailSuspended = false);
		}
		public bool IsUpdatingContainerDetailSuspended { get; private set; }

		#region New Properties

		public ZBool IsNVOCCHeader
		{
			get
			{
				var header = Header;
				return header != null && header.IsNVOCCHeader;
			}
		}

		public bool IsAMSMovement
		{
			get { return BM_SubApplicationCode == SubApplicationCodeList.Codes.AMS; }
		}

		public bool IsInBondMovement
		{
			get { return BM_SubApplicationCode == SubApplicationCodeList.Codes.MasterInBond || BM_SubApplicationCode == SubApplicationCodeList.Codes.SubsequentInBond; }
		}

		public bool IsPTTMovement
		{
			get { return BM_SubApplicationCode == SubApplicationCodeList.Codes.PermitToTransfer; }
		}

		public bool IsImmediateTransportEntryType
		{
			get { return BM_InBondEntryType == InbondCommonTypeList.Codes._1ImmediateTransport; }
		}

		public bool IsTransportandExportEntryType
		{
			get { return BM_InBondEntryType == InbondCommonTypeList.Codes._2TransportandExport; }
		}

		public bool IsImmediateExportEntryType
		{
			get { return BM_InBondEntryType == InbondCommonTypeList.Codes._3ImmediateExport; }
		}

		CusInBondMoveHeader RelatedAMSMovement
		{
			get
			{
				CusInBondMoveHeader result = null;
				if (!IsAMSMovement)
				{
					var header = Header;
					result = header == null ? null : header.MovementHeader;
				}
				return result;
			}
		}

		#region BM_ManifestSequenceNumber

		[BusinessObjectTestExclude]
		[ResourceStringData("Enterprise.Customs.US.AMS.Business.CusInBondMoveHeader|BM_ManifestSequenceNumber", Caption = "Manifest Sequence Number", MediumCaption = "Man. Seq. No.", ShortCaption = "MSN")]
		[MaxLength(Schema.BM_ManifestSequenceNumberMaxLength)]
		public ZString BM_ManifestSequenceNumber
		{
			get
			{
				var result = ZString.Empty;
				if (IsAMSMovement)
				{
					var manifestSequenceNumber = MSNCusEntryNumber;
					result = manifestSequenceNumber == null ? ZString.Empty : manifestSequenceNumber.CE_EntryNum.Left(Schema.BM_ManifestSequenceNumberMaxLength);
				}
				else
				{
					var amsMovement = RelatedAMSMovement;
					if (amsMovement != null)
					{
						result = amsMovement.BM_ManifestSequenceNumber;
					}
				}
				return result;
			}
			set
			{
				if (IsAMSMovement)
				{
					if (!value.IsEmpty)
					{
						value = value.KeepNumericCharacters();
						if (!value.IsEmpty)
						{
							value = value.PadLeft(Schema.BM_ManifestSequenceNumberMaxLength, '0');
						}
					}
					CheckMaximumLength(BM_ManifestSequenceNumberInfo, value);
					var oldValue = BM_ManifestSequenceNumber;
					if (oldValue != value)
					{
						if (value.IsEmpty)
						{
							ThrowAwayManifestSequenceNumber();
						}
						else
						{
							CreateMSNCusEntryNumberIfNeeded();
							MSNCusEntryNumber.CE_EntryNum = value.Left(Schema.BM_ManifestSequenceNumberMaxLength);
							MSNCusEntryNumber.CE_IssueDate = ZDateTime.Now;
						}
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidateBM_ManifestSequenceNumber();
					}
					BM_ManifestSequenceNumberInfo.RefreshBinding(oldValue);
				}
				else
				{
					var amsMovement = RelatedAMSMovement;
					if (amsMovement != null)
					{
						amsMovement.BM_ManifestSequenceNumber = value;
					}
					else
					{
						throw new InvalidOperationException(Schema.BM_ManifestSequenceNumber + " should only be set for AMS Movement");
					}
				}
			}
		}

		public void ThrowAwayManifestSequenceNumber()
		{
			if (MSNCusEntryNumber != null)
			{
				MSNCusEntryNumber.Delete();
			}
		}

		public ZPropertyInfo BM_ManifestSequenceNumberInfo
		{
			get { return GetZPropertyInfo(Schema.BM_ManifestSequenceNumber); }
		}

		public CusEntryNumber MSNCusEntryNumber
		{
			get
			{
				if (cusEntryNumberCache == null)
				{
					cusEntryNumberCache = new CachedProperty<CusEntryNumber>(Factory, delegate
					{ return LoadCusEntryNumber(); });
				}
				return cusEntryNumberCache.Value;
			}
		}
		CachedProperty<CusEntryNumber> cusEntryNumberCache;

		public const string ManifestSequenceNumberEntryType = "MSN";

		void CreateMSNCusEntryNumberIfNeeded()
		{
			if (MSNCusEntryNumber == null)
			{
				CreateMSNCusEntryNumber();
			}
		}

		CusEntryNumber CreateMSNCusEntryNumber()
		{
			var result = Factory.New<CusEntryNumber>();
			result.CE_EntryIsSystemGenerated = true;
			result.CE_ParentID = PK;
			result.CE_ParentTable = TableName;
			result.CE_EntryType = ManifestSequenceNumberEntryType;
			return result;
		}

		CusEntryNumber LoadCusEntryNumber()
		{
			var cusEntryNumberQuery = new ZQuery(CusEntryNumSchema.CE_ParentID, PK);
			cusEntryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, ManifestSequenceNumberEntryType);
			cusEntryNumberQuery.FetchOnlyFromLocalCache = !IsInDatabase;
			return Factory.LoadTop1<CusEntryNumber>(cusEntryNumberQuery);
		}

		#endregion

		#region BM_IsSubsequentInBond

		[ResourceStringData("Enterprise.Customs.US.AMS.Business.CusInBondMoveHeader|BM_IsSubsequentInBond", Caption = "Is Subsequent?")]
		public ZBool BM_IsSubsequentInBond
		{
			get { return BM_SubApplicationCode == SubApplicationCodeList.Codes.SubsequentInBond; }
			set
			{
				var oldValue = BM_IsSubsequentInBond;
				if (value && !oldValue)
				{
					BM_SubApplicationCode = SubApplicationCodeList.Codes.SubsequentInBond;
				}
				else if (!value && oldValue)
				{
					BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
				}
				BM_IsSubsequentInBondInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo BM_IsSubsequentInBondInfo
		{
			get { return GetZPropertyInfo(Schema.BM_IsSubsequentInBond); }
		}

		#endregion

		#region InBond Status

		#region InBond Departure Status

		[ResourceStringData("Enterprise.Customs.US.AMS.Business.CusInBondMoveHeader|InBondDepartureStatus", Caption = "In-Bond Departure Status", MediumCaption = "Departure Status", ShortCaption = "Dept. Status")]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.InBondStatusList))]
		public ZString InBondDepartureStatus
		{
			get
			{
				if (inBondDepartureStatusCached == null)
				{
					inBondDepartureStatusCached = new CachedProperty<ZString>(Factory, () =>
					{
						return IsInBondMovement ? MovementDetails.GetStatus(CusInBondMoveDetail.Schema.InBondDepartureStatus) : ZString.Empty;
					});
				}
				return inBondDepartureStatusCached.Value;
			}
		}
		CachedProperty<ZString> inBondDepartureStatusCached;

		public ZPropertyInfo InBondDepartureStatusInfo
		{
			get { return GetZPropertyInfo(Schema.InBondDepartureStatus); }
		}

		[ResourceStringData("Enterprise.Customs.US.AMS.Business.CusInBondMoveHeader|InBondDepartureStatusDesc", Caption = "In-Bond Departure Status Description", MediumCaption = "Departure Status Desc.", ShortCaption = "Dept. Status  Desc.")]
		public ZString InBondDepartureStatusDesc
		{
			get { return Lookups.InBondStatusList.GetDescriptionFromCode(InBondDepartureStatus); }
		}

		public ZPropertyInfo InBondDepartureStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.InBondDepartureStatusDesc); }
		}

		#endregion

		#region InBond Arrival Status

		[ResourceStringData("Enterprise.Customs.US.AMS.Business.CusInBondMoveHeader|InBondArrivalStatus", Caption = "In-Bond Arrival Status", MediumCaption = "Arrival Status", ShortCaption = "Arr. Status")]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.InBondStatusList))]
		public ZString InBondArrivalStatus
		{
			get
			{
				if (inBondArrivalStatusCached == null)
				{
					inBondArrivalStatusCached = new CachedProperty<ZString>(Factory, () =>
					{
						return IsInBondMovement ? MovementDetails.GetStatus(CusInBondMoveDetail.Schema.InBondArrivalStatus) : ZString.Empty;
					});
				}
				return inBondArrivalStatusCached.Value;
			}
		}
		CachedProperty<ZString> inBondArrivalStatusCached;

		public ZPropertyInfo InBondArrivalStatusInfo
		{
			get { return GetZPropertyInfo(Schema.InBondArrivalStatus); }
		}

		[ResourceStringData("Enterprise.Customs.US.AMS.Business.CusInBondMoveHeader|InBondArrivalStatusDesc", Caption = "In-Bond Arrival Status Description", MediumCaption = "Arrival Status Desc.", ShortCaption = "Arr. Status Desc.")]
		public ZString InBondArrivalStatusDesc
		{
			get { return Lookups.InBondStatusList.GetDescriptionFromCode(InBondArrivalStatus); }
		}

		public ZPropertyInfo InBondArrivalStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.InBondArrivalStatusDesc); }
		}

		#endregion

		#region InBond Exportation Status

		[ResourceStringData("Enterprise.Customs.US.AMS.Business.CusInBondMoveHeader|InBondExportationStatus", Caption = "In-Bond Exportation Status", MediumCaption = "Exportation Status", ShortCaption = "Exp. Status")]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.InBondStatusList))]
		public ZString InBondExportationStatus
		{
			get
			{
				if (inBondExportationStatusCached == null)
				{
					inBondExportationStatusCached = new CachedProperty<ZString>(Factory, () =>
					{
						return IsInBondMovement ? MovementDetails.GetStatus(CusInBondMoveDetail.Schema.InBondExportationStatus) : ZString.Empty;
					});
				}
				return inBondExportationStatusCached.Value;
			}
		}
		CachedProperty<ZString> inBondExportationStatusCached;

		public ZPropertyInfo InBondExportationStatusInfo
		{
			get { return GetZPropertyInfo(Schema.InBondExportationStatus); }
		}

		[ResourceStringData("Enterprise.Customs.US.AMS.Business.CusInBondMoveHeader|InBondExportationStatusDesc", Caption = "In-Bond Exportation Status Description", MediumCaption = "Exportation Status Desc.", ShortCaption = "Exp. Status Desc.")]
		public ZString InBondExportationStatusDesc
		{
			get { return Lookups.InBondStatusList.GetDescriptionFromCode(InBondExportationStatus); }
		}

		public ZPropertyInfo InBondExportationStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.InBondExportationStatusDesc); }
		}

		#endregion

		#region InBond TransferOfLiability Status

		[ResourceStringData("Enterprise.Customs.US.AMS.Business.CusInBondMoveHeader|InBondTransferOfLiabilityStatus", Caption = "In-Bond Transfer Of Liability Status", MediumCaption = "Transfer Of Liability Status", ShortCaption = "TOL Status")]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.InBondStatusList))]
		public ZString InBondTransferOfLiabilityStatus
		{
			get
			{
				if (inBondTransferOfLiabilityStatusCached == null)
				{
					inBondTransferOfLiabilityStatusCached = new CachedProperty<ZString>(Factory, () =>
					{
						return IsInBondMovement ? MovementDetails.GetStatus(CusInBondMoveDetail.Schema.InBondTransferOfLiabilityStatus) : ZString.Empty;
					});
				}
				return inBondTransferOfLiabilityStatusCached.Value;
			}
		}
		CachedProperty<ZString> inBondTransferOfLiabilityStatusCached;

		public ZPropertyInfo InBondTransferOfLiabilityStatusInfo
		{
			get { return GetZPropertyInfo(Schema.InBondTransferOfLiabilityStatus); }
		}

		[ResourceStringData("Enterprise.Customs.US.AMS.Business.CusInBondMoveHeader|InBondTransferOfLiabilityStatusDesc", Caption = "In-Bond Transfer Of Liability Status Description", MediumCaption = "Transfer Of Liability Status Desc.", ShortCaption = "TOL Status Desc.")]
		public ZString InBondTransferOfLiabilityStatusDesc
		{
			get { return Lookups.InBondStatusList.GetDescriptionFromCode(InBondTransferOfLiabilityStatus); }
		}

		public ZPropertyInfo InBondTransferOfLiabilityStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.InBondTransferOfLiabilityStatusDesc); }
		}

		#endregion

		#endregion

		#region ShouldSendManifestAmendmentMessage

		public ZBool ShouldSendManifestAmendmentMessage
		{
			get
			{
				if (shouldSendManifestAmendmentMessage == null)
				{
					shouldSendManifestAmendmentMessage = false;
					if (Messages.Count > 0 && MovementDetails.Count > 0 && MovementDetails.Any(x => x.IsMessageRejectedByError))
					{
						var mostRecentMRMessage = MostRecentManifestCreateTransmissionResponse(Messages);
						if (mostRecentMRMessage != null)
						{
							shouldSendManifestAmendmentMessage = mostRecentMRMessage.MessageBlock.MessageBlocks.OfType<TARW01>().Any(block => block.ErrorMessage.StartsWith("059"));
						}
					}
				}

				return shouldSendManifestAmendmentMessage.Value;
			}
		}
		ZBool? shouldSendManifestAmendmentMessage;

		CBPEDIMessage MostRecentManifestCreateTransmissionResponse(CBPEDIMessageCollection messages)
		{
			CBPEDIMessage mostRecentMRMessage = null;
			var lastTransmittedMessage = messages.OfType<CBPEDIMessage>()
										.Where(message => message.EM_ApplicationCode == EDIMessage.ApplicationCodes.AMS && message.EM_ReceiveTransmit == AMSEDIMessage.Direction.Transmit)
										.OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();

			if (lastTransmittedMessage != null && lastTransmittedMessage.EM_MessageType == AMSApplicationIdentifierCodeList.Codes.ManifestCreate)
			{
				mostRecentMRMessage = (CBPEDIMessage)messages.GetMatchingMessage(EDIMessage.ApplicationCodes.AMS, AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, lastTransmittedMessage.EM_MessageNum, AMSEDIMessage.Direction.Receive);
			}

			return mostRecentMRMessage;
		}

		#endregion

		#endregion

		#region Override Properties

		protected override ZString HumanReadableNameCore
		{
			get
			{
				if (humanReadableNameCoreCached == null)
				{
					humanReadableNameCoreCached = new CachedProperty<ZString>(Factory, delegate
					{
						var result = ZString.Empty;

						if (BM_SubApplicationCode == SubApplicationCodeList.Codes.AMS)
						{
							result = "AMS Movement Header";
						}
						else if (BM_SubApplicationCode == SubApplicationCodeList.Codes.PermitToTransfer)
						{
							result = ZString.Format("Permit Movement Header {0}", BM_InBondCarrierID);
						}
						else if (BM_SubApplicationCode == SubApplicationCodeList.Codes.MasterInBond || BM_SubApplicationCode == SubApplicationCodeList.Codes.SubsequentInBond)
						{
							result = ZString.Format("In-Bond Movement Header {0}", InBondNumber);
						}

						return result;
					});
				}
				return humanReadableNameCoreCached.Value;
			}
		}
		CachedProperty<ZString> humanReadableNameCoreCached;

		public override ZString BM_InBondEntryType
		{
			get { return base.BM_InBondEntryType; }
			set
			{
				var oldValue = BM_InBondEntryType;
				base.BM_InBondEntryType = value;
				if (!IsCopying && oldValue != BM_InBondEntryType)
				{
					Header?.RefreshCachedValue();
					MovementDetails.MarkAsNeedingValidation();
				}
			}
		}

		#region BM_BH

		public new CusInBondHeader Header
		{
			get { return (CusInBondHeader)base.Header; }
		}

		#endregion

		#region InBondNumber

		[BusinessObjectTestExclude]
		public override ZString InBondNumber
		{
			get { return base.InBondNumber; }
			set
			{
				if (!IsInBondMovement)
				{
					throw new InvalidOperationException("In-Bond Number should only be set for In-Bond Movement");
				}
				base.InBondNumber = value;
			}
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.ImporterCodes))]
		public override ZString BM_InBondCarrierID
		{
			get { return base.BM_InBondCarrierID; }
			set { base.BM_InBondCarrierID = value; }
		}

		#region BM_SubApplicationCode

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.SubApplicationCodeList))]
		public override ZString BM_SubApplicationCode
		{
			get { return base.BM_SubApplicationCode; }
			set
			{
				var oldValue = BM_SubApplicationCode;
				base.BM_SubApplicationCode = value;
				if (!IsCopying && oldValue != BM_SubApplicationCode)
				{
					EnsureOceanBillNVOCCMoveDetailAreValid();
				}
				MovementDetails.MarkAsNeedingValidation();
			}
		}

		[ResourceStringData("Enterprise.Customs.US.AMS.Business.CusInBondMoveHeader|BM_SubApplicationCodeDescription", Caption = "Movement Type")]
		public ZString BM_SubApplicationCodeDescription
		{
			get { return Lookups.SubApplicationCodeList.GetDescriptionFromCode(BM_SubApplicationCode) ?? ZString.Empty; }
		}

		public ZPropertyInfo BM_SubApplicationCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.BM_SubApplicationCodeDescription); }
		}

		#endregion

		public override ZDateTime BM_ExportDate
		{
			get { return base.BM_ExportDate; }
			set
			{
				var oldValue = BM_ExportDate;
				base.BM_ExportDate = value;
				if (!IsCopying)
				{
					var newValue = BM_ExportDate;
					if (oldValue != newValue)
					{
						ClearMoveDetailValuesIfSame(newValue, CusInBondMoveDetail.Schema.B9_ExportDate);
						MovementDetails.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZString BM_ExportLadenOn
		{
			get { return base.BM_ExportLadenOn; }
			set
			{
				var oldValue = BM_ExportLadenOn;
				base.BM_ExportLadenOn = value;
				if (!IsCopying)
				{
					var newValue = BM_ExportLadenOn;
					if (oldValue != newValue)
					{
						ClearMoveDetailValuesIfSame(newValue, CusInBondMoveDetail.Schema.B9_ExportLadenOn);
						MovementDetails.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZString BM_ForeignDestPortKCode
		{
			get { return base.BM_ForeignDestPortKCode; }
			set
			{
				var oldValue = BM_ForeignDestPortKCode;
				base.BM_ForeignDestPortKCode = value;
				if (!IsCopying)
				{
					var newValue = BM_ForeignDestPortKCode;
					if (oldValue != newValue)
					{
						ClearMoveDetailValuesIfSame(newValue, CusInBondMoveDetail.Schema.B9_ForeignDestPortKCode);
						MovementDetails.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override bool IsWaitingForResponse
		{
			get { return AMSBillMessageStatusList.IsMessagingInProgressType(BM_CustomsStatus); }
		}

		public override bool IsAcceptedByCustoms
		{
			get { return AMSBillMessageStatusList.IsAcceptedByCustoms(BM_CustomsStatus); }
		}

		public override bool IsWithdrawn
		{
			get { return AMSBillMessageStatusList.IsWithdrawn(BM_CustomsStatus); }
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				if (IsDeleted)
				{
					return base.BusinessObjectsWithRelatedEventsCore;
				}
				return MovementDetails.OfType<BusinessObject>().ToArray();
			}
		}

		public new CusInBondMoveHeaderLookups Lookups
		{
			get { return (CusInBondMoveHeaderLookups)base.Lookups; }
		}

		public new CusInBondMoveHeaderValidation Validation
		{
			get { return (CusInBondMoveHeaderValidation)base.Validation; }
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				Header?.RefreshCachedValue();
				MessageAttacheesAddedOrDeletedEvent.InvokeMessageAttacheeAddedOrDeletedService(Factory, this, MessageAttacheeActionType.Deleted);
			}
			base.Delete();
		}

		#endregion

		#region Override Methods

		public override void OnSaving()
		{
			base.OnSaving();
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(BM_CustomsStatus), ConcurrencyPolicy.Strict);
		}

		public override bool CanDelete
		{
			get
			{
				var result = !IsInDatabase || base.CanDelete;
				return result && !ActiveInMessaging;/// && !CopyParentDefault;
			}
		}

		public bool ActiveInMessaging
		{
			get { return Messages.Count > 0; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var result = base.ReasonForNotAbleToDelete;
				if (result.IsEmpty && !CanDelete)
				{
					result = ValidationConstants.MoveHeader.CannotDeleteMovementWhenMessageExists;
				}
				return result;
			}
		}

		#endregion

		#region Related Objects

		public CusInBondMoveDetail OceanBillNVOCCMoveDetail
		{
			get
			{
				if (oceanBillNVOCCMoveDetail == null || oceanBillNVOCCMoveDetail.IsDeleted || oceanBillNVOCCMoveDetail.B9_BM != PK)
				{
					oceanBillNVOCCMoveDetail = null;
					if (IsPTTMovement || IsInBondMovement)
					{
						var header = Header;
						if (header != null && header.IsNVOCCHeader)
						{
							var oceanBillPK = header.OceanBill.PK;
							oceanBillNVOCCMoveDetail = MovementDetails.FirstOrDefault(x => x.B9_B0 == oceanBillPK) ?? MovementDetails.AddNew(oceanBillPK);
						}
					}
				}
				return oceanBillNVOCCMoveDetail;
			}
		}
		CusInBondMoveDetail oceanBillNVOCCMoveDetail;

		[ChildEditable]
		public new CusInBondMoveDetailCollection MovementDetails
		{
			get { return (CusInBondMoveDetailCollection)base.MovementDetails; }
		}

		protected override Customs.Business.ICusInBondMoveDetailCollection CreateMovementDetails()
		{
			return new CusInBondMoveDetailCollection(this);
		}

		[ChildEditable]
		public AMSEDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new AMSEDIMessageCollection(this);
					fMessages.Load();
					fMessages.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(fMessages);
				}
				return fMessages;
			}
		}
		AMSEDIMessageCollection fMessages;

		public ForwardingConsol Consol
		{
			get
			{
				var header = Header;
				return header == null ? null : header.Consol;
			}
		}

		#endregion

		#region Validation Modes

		public ValidationModes ValidationModes
		{
			get
			{
				if (!fValidationModes.HasValue)
				{
					fValidationModes = ValidationModes.UseParentValidateMode;
				}

				var result = fValidationModes.Value;
				if (result == ValidationModes.UseParentValidateMode)
				{
					result = HeaderValidationModes;
				}
				return result;
			}
			set
			{
				var hasChanges = ValidationModes != value;
				var headerValidationModes = HeaderValidationModes;
				fValidationModes = value == headerValidationModes ? ValidationModes.UseParentValidateMode : value;
				if (hasChanges && !IsMarkingAsNeedingValidationSuspended)
				{
					MarkAsNeedingValidationIncludingChildren();
				}
			}
		}
		ValidationModes? fValidationModes;

		public ValidationModes HeaderValidationModes
		{
			get
			{
				var result = ValidationModes.None;
				var header = Header;
				if (header != null)
				{
					result = header.ValidationModes;
				}
				return result;
			}
		}

		public bool IsPermitToTransferValidationMode
		{
			get
			{
				var result = false;
				if (IsPTTMovement)
				{
					result = ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.PermitToTransfer);
				}
				return result;
			}
		}

		public bool IsSubsequentInBondValidationMode
		{
			get
			{
				var result = false;
				if (IsInBondMovement)
				{
					result = ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.SubsequentInBond);
				}
				return result;
			}
		}

		public bool IsInBondArrivalValidationMode
		{
			get
			{
				var result = false;
				if (IsInBondMovement)
				{
					result = ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.InBondArrival);
				}
				return result;
			}
		}

		public bool IsInBondExportationValidationMode
		{
			get
			{
				var result = false;
				if (IsInBondMovement)
				{
					result = ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.InBondExportation);
				}
				return result;
			}
		}

		public bool IsInBondTOLValidationMode
		{
			get
			{
				var result = false;
				if (IsInBondMovement)
				{
					result = ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.InBondTOL);
				}
				return result;
			}
		}

		public bool IsInBondDiversionValidationMode
		{
			get
			{
				var result = false;
				if (IsInBondMovement)
				{
					result = ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.InBondDiversion);
				}
				return result;
			}
		}

		#endregion

		#region Implementation

		void EnsureOceanBillNVOCCMoveDetailAreValid()
		{
			var header = Header;
			if (header != null)
			{
				if (header.IsNVOCCHeader && (IsPTTMovement || IsInBondMovement))
				{
					var oceanBillMoveDetail = OceanBillNVOCCMoveDetail;
				}
				else
				{
					var oceanBill = header.OceanBill;
					if (oceanBill != null)
					{
						foreach (var oceanBillMoveDetail in MovementDetails.Where(x => x.B9_B0 == oceanBill.PK))
						{
							oceanBillMoveDetail.Delete();
						}
					}
				}
			}
		}

		ZString GetJobNumber()
		{
			ZString result = "Unknown";
			var header = Header;
			if (header != null)
			{
				result = header.BH_JobReference;
				if (result.IsEmpty)
				{
					var consol = header.Consol;
					if (consol != null)
					{
						result = consol.JK_UniqueConsignRef;
					}
				}
			}
			return result;
		}

		protected override Type MovementDetailTypeCore
		{
			get { return typeof(CusInBondMoveDetail); }
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		new internal class Strategy : US.Business.CusInBondMoveHeader.Strategy
		{
			public Strategy(CusInBondMoveHeader moveHeader)
				: base(moveHeader)
			{
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();

				Factory.AddFetchHint(typeof(CusInBondMoveHeader), BusinessObject.PK);
				Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, BusinessObject.PK);
			}
		}

		protected override Customs.Business.CusInBondMoveHeaderLookups GetNewLookups()
		{
			return new CusInBondMoveHeaderLookups(this);
		}

		protected override Customs.Business.CusInBondMoveHeaderValidation GetNewValidation()
		{
			return new CusInBondMoveHeaderValidation(this);
		}

		protected override IEnumerable<US.Business.CusInBondMoveHeader> GetMovementHeaders(Customs.Business.CusInBondHeader header)
		{
			var result = new List<US.Business.CusInBondMoveHeader>();
			var amsHeader = (CusInBondHeader)header;
			if (IsAMSMovement)
			{
				result.Add(amsHeader.MovementHeader);
			}
			else if (IsInBondMovement)
			{
				result.AddRange(amsHeader.InBondMovementHeaders.OfType<US.Business.CusInBondMoveHeader>());
			}
			else if (IsPTTMovement)
			{
				result.AddRange(amsHeader.PTTMovements.OfType<US.Business.CusInBondMoveHeader>());
			}
			return result.ToArray();
		}

		void ClearMoveDetailValuesIfSame(IZType moveHeaderValue, string moveDetailFieldName)
		{
			foreach (var moveDetail in MovementDetails)
			{
				var moveDetailValue = (IZType)moveDetail[moveDetailFieldName];

				if (moveDetailValue.Equals(moveHeaderValue))
				{
					using (moveDetail.SuspendEffectiveValue(moveDetailFieldName, moveHeaderValue))
					{
						moveDetail[moveDetailFieldName] = moveDetailValue.Default;
					}

					var infoToRefresh = moveDetail.ZPropertyInfoHash[moveDetailFieldName];
					if (infoToRefresh != null)
					{
						infoToRefresh.RefreshBinding();
					}
				}
			}
		}

		#endregion

		#region IManifestMessageAttachee Members

		ZString IManifestMessageAttachee.ApplicationCode
		{
			get
			{
				var header = Header;
				return header == null ? ZString.Empty : header.BH_ApplicationCode;
			}
		}

		ZString IManifestMessageAttachee.UniqueVoyageIdentifier
		{
			get
			{
				var header = Header;
				return header == null ? ZString.Empty : header.BH_UniqueVoyageIdentifier;
			}
		}

		ZString IManifestMessageAttachee.SupApplicationCode
		{
			get { return BM_SubApplicationCode; }
		}

		ZString IManifestMessageAttachee.JobNumber
		{
			get { return GetJobNumber(); }
		}

		ZString IManifestMessageAttachee.CarrierCode
		{
			get { return Header.BH_CarrierSCAC; }
		}

		public ZString CarrierCodeForPTT
		{
			get
			{
				var result = ZString.Empty;
				if (Header.BH_TransitDirection == DirectionTypeList.Codes.NVOCC)
				{
					if (Header.Branch is GlbBranch branch && UseFIRMSCodeAsFilerForPTTMessages(branch))
					{
						ZString GetFIRMSCodeFromOrgProxy(ZGuid orgProxyPK)
						{
							var orgProxy = Factory.Load<OrgHeader>(orgProxyPK);
							return orgProxy?.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.USACodeTypes.FIRMSCode) ?? ZString.Empty;
						}

						result = GetFIRMSCodeFromOrgProxy(branch.GB_OH_OrgProxy);
						if (result.IsEmpty && branch.Company is GlbCompany company && branch.GB_OH_OrgProxy != company.GC_OH_OrgProxy)
						{
							result = GetFIRMSCodeFromOrgProxy(company.GC_OH_OrgProxy);
						}
					}
				}

				if (result.IsEmpty && this is IManifestMessageAttachee messageAttachee)
				{
					result = messageAttachee.CarrierCode;
				}

				return result;
			}
		}

		bool UseFIRMSCodeAsFilerForPTTMessages(GlbBranch branch) => FreightDataRegistry.Instance.UseFIRMSCodeAsFilerForPTTMessages.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);

		ZString IManifestMessageAttachee.ModeOfTransportationCode
		{
			get { return Header.BH_ImportTransportMode; }
		}

		ZString IManifestMessageAttachee.ConveyanceCountryCode
		{
			get { return Header.BH_ImportConveyanceCountry; }
		}

		ZString IManifestMessageAttachee.ConveyanceName
		{
			get { return Header.BH_ImportConveyanceName; }
		}

		ZString IManifestMessageAttachee.VoyageNumber
		{
			get { return Header.BH_VoyageNumber; }
		}

		ZString IManifestMessageAttachee.ManifestSequenceNumber
		{
			get { return BM_ManifestSequenceNumber; }
			set { BM_ManifestSequenceNumber = value; }
		}

		ZBool IManifestMessageAttachee.IsPaperlessMIBParticipant
		{
			get { return Header.BH_IsPaperlessMIBParticipant; }
		}

		ZString IManifestMessageAttachee.ConveyanceCode
		{
			get { return Header.BH_LloydsNumber; }
		}

		ZBool IManifestMessageAttachee.IsOutboundCargo
		{
			get { return Header.BH_IsOutboundCargo; }
		}

		void IManifestMessageAttachee.UpdatConveyanceEventInformation(ZString eventCode, ZDateTime eventDate)
		{
			var header = Header;
			if (header != null)
			{
				var dispositionCodes = header.DispositionCodes;
				var dispositionCodesCount = dispositionCodes.Count;

				dispositionCodes.AddNewIfNotExist(eventCode, eventDate);

				if (dispositionCodes.Count > dispositionCodesCount)
				{
					header.Logs.AddNew(Events.StatusChange, ZString.Format("{0} - {1}", eventCode, header.DispositionCodeDescriptionList.GetDescriptionFromCode(eventCode)));
				}
			}
		}

		void IManifestMessageAttachee.UpdateDispositionInformation(ZString billOfLadingIssuerCode, ZString billOfLadingNumber, ZString dispositionCode, ZDateTime dispositionDate)
		{
			var moveDetail = MovementDetails.FindBillOfLading(billOfLadingIssuerCode, billOfLadingNumber);
			if (moveDetail != null)
			{
				var bill = moveDetail.Bill;
				if (bill != null)
				{
					var dispositionCodes = bill.DispositionCodes;
					var dispositionCodesCount = dispositionCodes.Count;

					dispositionCodes.AddNewIfNotExist(dispositionCode, dispositionDate);

					if (dispositionCodes.Count > dispositionCodesCount)
					{
						bill.Logs.AddNew(Events.StatusChange, ZString.Format("{0} - {1}", dispositionCode, bill.DispositionCodeDescriptionList.GetDescriptionFromCode(dispositionCode)));
					}
				}
			}
		}

		void IManifestMessageAttachee.UpdateIncomingBillStatus(ZString issuerCode, ZString billOfLading, ZString subtype, bool isFailure, CBPEDIMessage responseMessage)
		{
			var actionCode = AMSMessageSubTypeList.GetActionCodeFromSubType(subtype);
			if (ActionCodeTool.IsVesselEvent(actionCode))
			{
				if (isFailure)
				{
					BM_CustomsStatus = AMSBillMessageStatusList.Codes.Error;
				}
				else
				{
					switch (actionCode)
					{
						case ActionCode.VesselArrival:
							BM_CustomsStatus = AMSBillMessageStatusList.Codes.ClearArrival;
							break;
						case ActionCode.VesselDeparture:
							BM_CustomsStatus = AMSBillMessageStatusList.Codes.ClearDeparture;
							break;
						case ActionCode.ChangeEstDateOfArrival:
							BM_CustomsStatus = AMSBillMessageStatusList.Codes.Updated;
							break;
					}
				}
			}
			else
			{
				var moveDetail = MovementDetails.FindBillOfLading(issuerCode, billOfLading);
				if (moveDetail != null)
				{
					if (isFailure)
					{
						moveDetail.B9_MessageStatus = AMSBillMessageStatusList.Codes.Error;
					}
					else
					{
						var oldCustomsStatus = moveDetail.B9_CustomsStatus;
						switch (actionCode)
						{
							case ActionCode.Creating:
							case ActionCode.AmendingAdd:
							case ActionCode.AmendingUpdate:
								moveDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
								break;
							case ActionCode.AmendingDelete:
								moveDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.NotOnFile;
								break;
							case ActionCode.InBondArrival:
							case ActionCode.SubsequentInBondOriginal:
							case ActionCode.SubsequentInBondDelete:
							case ActionCode.SubsequentInBondAmendment:
							case ActionCode.InBondDiversion:
							case ActionCode.InBondExportation:
							case ActionCode.InBondTransferOfLiability:
							case ActionCode.PermitToTransfer:
								moveDetail.B9_CustomsStatus = AMSBillMessageStatusList.GetTypeFromActionCode(actionCode);
								break;
						}
						moveDetail.B9_MessageStatus = AMSBillMessageStatusList.GetTypeFromActionCode(actionCode);

						var customsStatus = moveDetail.B9_CustomsStatus;
						if (responseMessage != null && oldCustomsStatus != customsStatus && IsAMSMovement && Header?.OceanBill is CusInBondBill oceanBill)
						{
							if (customsStatus == AMSBillCustomsStatusList.Codes.OnFile)
							{
								var firstAcceptedTime = GetFirstAcceptedTimeFromMessage(oceanBill.B0_IssuerCode + oceanBill.B0_MasterBillNumber, issuerCode, billOfLading, responseMessage);
								if (firstAcceptedTime.IsValid)
								{
									moveDetail.B9_FirstAcceptedTime = firstAcceptedTime;
								}
							}
							else
							{
								moveDetail.B9_FirstAcceptedTime = ZDateTime.Empty;
							}
						}
					}
				}
			}
		}

		void IManifestMessageAttachee.UpdateEstimatedDateOfArrival(ZDateTime estimatedDateOfArrival)
		{
			if (Header != null && !estimatedDateOfArrival.IsEmpty)
			{
				Header.BH_ETA = estimatedDateOfArrival;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		ZDateTime GetFirstAcceptedTimeFromMessage(string oceanBillNumberWithIssuerCode, string houseBillIssuerCode, string houseBillNumber, CBPEDIMessage responseMessage)
		{
			var firstAcceptedTime = ZDateTime.Empty;
			var sqlQueryText = FormattableString.Invariant($@"SELECT TOP 1 O.EM_SystemCreateTimeUtc
FROM dbo.EDIMessage O
INNER JOIN dbo.EDIMessage T ON T.EM_LinkUniqueID = O.EM_LinkUniqueID AND T.EM_MessageNum = O.EM_MessageNum
WHERE O.EM_LinkUniqueID = @LinkUniqueID
AND O.EM_PK != @ResponseMessagePK
AND O.EM_SystemCreateTimeUtc < @SystemCreateTimeUtc
AND O.EM_ApplicationCode = '{EDIMessage.ApplicationCodes.AMS}'
AND O.EM_ReceiveTransmit = 'RCV'
AND O.EM_Status = 'RCV'
AND O.EM_MessageType IN ('{AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse}', '{AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse}')
AND O.EM_MessageText like '%M02' + @HouseBillNumber + '_%'
AND (CHARINDEX('W01', O.EM_MessageText) % 80) != 1
AND T.EM_ApplicationCode = '{EDIMessage.ApplicationCodes.AMS}'
AND T.EM_ReceiveTransmit = 'TRX'
AND T.EM_MessageText LIKE '%M02' + @HouseBillNumber + '_%J01' + @HouseBillIsserCode + '%B04OB ' + @OceanBillNumberWithIssuerCode + '%'
ORDER BY O.EM_SystemCreateTimeUtc");

			var responseMessageCreateTime = responseMessage.EM_SystemCreateTimeUtc.ToDateTime();
			using (var cmd = Db.Connection.Command(sqlQueryText)) // Reduce DB hits in service task
			{
				cmd.AddParameter("@SystemCreateTimeUtc", SqlDbType.DateTime, responseMessageCreateTime);
				cmd.AddParameter("@LinkUniqueID", SqlDbType.UniqueIdentifier, this.PK.ToGuid());
				cmd.AddParameter("@ResponseMessagePK", SqlDbType.UniqueIdentifier, responseMessage.PK.ToGuid());
				cmd.AddParameter("@HouseBillNumber", SqlDbType.VarChar, houseBillNumber);
				cmd.AddParameter("@HouseBillIsserCode", SqlDbType.VarChar, houseBillIssuerCode);
				cmd.AddParameter("@OceanBillNumberWithIssuerCode", SqlDbType.VarChar, oceanBillNumberWithIssuerCode);
				if (cmd.ExecuteScalar() is DateTime systemCreateTime)
				{
					firstAcceptedTime = systemCreateTime;
				}
				else
				{
					firstAcceptedTime = responseMessageCreateTime;
				}
			}

			return firstAcceptedTime;
		}

		void IManifestMessageAttachee.LinkMessageToBill(ZString issuerCode, ZString billOfLading, CBPEDIMessage responseMessage)
		{
			var moveDetail = MovementDetails.FindBillOfLading(issuerCode, billOfLading);
			if (moveDetail != null)
			{
				if (moveDetail.Bill is CusInBondBill bill)
				{
					responseMessage.EM_ApplicationReference = bill.PK.ToString();
				}
			}
		}

		#endregion

		#region IVesselArrivalMessageAttachee Members

		void IVesselArrivalMessageAttachee.UpdateActualArrivalDate(ZString portOfUnlading, ZDateTime dateTime, ZString messageSubType)
		{
			if (messageSubType == AMSMessageSubTypeList.Codes.VesselArrival)
			{
				Header.PortArrivalDetails.AddNewIfNotExist(portOfUnlading, dateTime);
			}
			else if (messageSubType == AMSMessageSubTypeList.Codes.ChangeEstimatedDateOfArrival)
			{
				var matchedBills = Header.Bills.Where(x => x.B0_InBondPortOfDestDCode == portOfUnlading);
				foreach (var bill in matchedBills)
				{
					bill.B0_DateOfDischarge = dateTime.Date;
				}
			}
		}

		#endregion

		#region IControllerIDProvider Members

		ControllerID IControllerIDProvider.ControllerID
		{
			get
			{
				var consol = Consol;
				return consol == null ? ControllerIDs.Customs.US.AMS : ControllerIDs.JobConsol;
			}
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get
			{
				var consol = Consol;
				return consol == null ? BM_BH.ToGuid() : consol.PK.ToGuid();
			}
		}

		#endregion

		#region IMessageAttachee Members

		GlbBranch IMessageAttachee.Branch
		{
			get { return HeaderBranch; }
		}

		ZString IMessageAttachee.MessageStatus
		{
			get { return BM_CustomsStatus; }
			set { BM_CustomsStatus = value; }
		}

		CBPEDIMessageCollection IMessageAttachee.Messages
		{
			get { return Messages; }
		}

		BusinessObject IMessageAttachee.TopLevelBusinessObject
		{
			get { return Consol; }
		}

		string IMessageAttachee.TopLevelBizObjReferenceNumber
		{
			get { return Consol.JK_UniqueConsignRef; }
		}

		Logs IMessageAttachee.TopLevelBusinessObjectLogs
		{
			get
			{
				var consol = Consol;
				return consol == null ? null : consol.Logs;
			}
		}

		#endregion

		#region IMessageAttacheeInHeader Members

		ZString IMessageAttacheeInHeader.RecordIdentifier
		{
			get
			{
				if (IsPTTMovement)
				{
					return "Carrier: " + BM_InBondCarrierID.ToString();
				}
				else if (IsInBondMovement)
				{
					return "In-Bond Number: " + MovementUniqueCode + ", Entry Type: " + BM_InBondEntryType;
				}
				else
				{
					return GetJobNumber();
				}
			}
		}

		MessageAttacheeRecordType IMessageAttacheeInHeader.RecordType
		{
			get { return IsPTTMovement ? MessageAttacheeRecordType.PermitToTransferMovement : IsInBondMovement ? MessageAttacheeRecordType.InBondMovement : MessageAttacheeRecordType.VesselMovement; }
		}

		ZString IMessageAttacheeInHeader.RecordTypeDescription
		{
			get { return BM_SubApplicationCodeDescription; }
		}

		ZGuid IMessageAttacheeInHeader.HeaderPK
		{
			get { return Header.PK; }
		}

		ZGuid IMessageAttacheeInHeader.PK
		{
			get { return PK; }
		}

		#endregion

		#region ICBPEDIMessageMessageTextNumberPlaceHolderFiller Members

		string ICBPEDIMessageMessageTextNumberPlaceHolderFiller.Fill(CBPEDIMessage message)
		{
			var information = string.Empty;
			CusInBondMoveHeader inBondMovement = null;
			if (IsInBondMovement)
			{
				inBondMovement = this;
			}
			else if (IsAMSMovement && message.EM_MessageText.Contains(AMSEDIMessage.InBondNumberPlaceHolder))
			{
				inBondMovement = GetRelatedInBondMovement(message);
			}
			if (inBondMovement != null)
			{
				inBondMovement.FillInInBondNumberDetailsIfNeeded();
				var inBondPlaceHolder = AMSEDIMessage.InBondNumberPlaceHolder;
				message.EM_MessageText = message.EM_MessageText.Replace(inBondPlaceHolder, inBondMovement.InBondNumber.PadRight(inBondPlaceHolder.Length));
				information = string.Format(CultureInfo.InvariantCulture, "{0}InBondNumberPlaceHolder {1} is replaced with InBond Number {2}. ",
					information, inBondPlaceHolder, InBondNumber);
			}
			return information;
		}

		CusInBondMoveHeader GetRelatedInBondMovement(CBPEDIMessage message)
		{
			CusInBondMoveHeader inBondMovement = null;
			try
			{
				var inpm02 = message.MessageBlock.MessageBlocks.OfType<IINPM02>().FirstOrDefault();
				if (inpm02 != null)
				{
					var billOfLading = CarrierAssignedBatchNumberCreator.GetBillOfLading(inpm02);
					if (!billOfLading.IsEmpty)
					{
						var inpj02 = message.MessageBlock.MessageBlocks.OfType<IINPJ01>().FirstOrDefault();
						if (inpj02 != null)
						{
							var moveDetail = MovementDetails.FindBillOfLading(inpj02.IssuerCode, billOfLading);
							if (moveDetail != null)
							{
								var bill = moveDetail.Bill;
								if (bill != null)
								{
									inBondMovement = bill.MasterInBondMovement;
								}
							}
						}
					}
				}
			}
			catch (InvalidMessageFormatException)
			{
				// do nothing
			}
			return inBondMovement;
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(IssuerAndBillOfLading.IssuerAndBillOfLadingTypeCode, typeof(IssuerAndBillOfLading));
			return result;
		}

		#endregion

		#region IManifestPendingMessagesAttachee Members

		AMSEDIMessageCollection IManifestPendingMessagesAttachee.PendingMessages
		{
			get
			{
				var additionalQuery = new ZQuery();
				additionalQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
				additionalQuery.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Pending);
				additionalQuery.TableIndexHints.Add(new TableIndexHint("NR_RX__EM_LinkUniqueID")); // there is no existing constant for this index name

				fPendingMessages = new AMSEDIMessageCollection(this);
				fPendingMessages.LoadWithMoreFiltering(additionalQuery);
				return fPendingMessages;
			}
		}
		AMSEDIMessageCollection fPendingMessages;

		#endregion

		ZString IMessageResponseNotificator.GetFallbackEmailAddressRecipient()
		{
			var result = GetEmailOfLastOutgoingMessageSender(Messages);
			if (result.IsEmpty)
			{
				var relatedAMSMovement = RelatedAMSMovement;
				if (relatedAMSMovement != null)
				{
					result = GetEmailOfLastOutgoingMessageSender(relatedAMSMovement.Messages);
				}
			}
			return result;
		}

		ZString GetEmailOfLastOutgoingMessageSender(AMSEDIMessageCollection messages)
		{
			var result = ZString.Empty;
			if (messages != null)
			{
				var lastOutgoingMessage = messages.LastOutgoingMessage;
				if (lastOutgoingMessage != null)
				{
					var userWhoQueuedThisRecord = lastOutgoingMessage.UserWhoQueuedThisRecord;
					if (userWhoQueuedThisRecord != null)
					{
						result = userWhoQueuedThisRecord.GS_EmailAddress;
					}
				}
			}
			return result;
		}

		internal void RefreshCustomsStatus()
		{
			if (IsPTTMovement || IsInBondMovement)
			{
				BM_CustomsStatus = MovementDetails.GetStatus(CusInBondMoveDetail.Schema.B9_CustomsStatus);
			}
		}
	}
}
