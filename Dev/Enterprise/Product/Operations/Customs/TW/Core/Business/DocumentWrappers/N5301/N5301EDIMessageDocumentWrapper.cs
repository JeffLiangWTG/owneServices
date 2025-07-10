using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5301;
using Enterprise.Customs.TW.Messaging;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business.DocumentWrappers
{
	internal class N5301EDIMessageDocumentWrapper : NonPersistentBusinessObject, IBODocDataProvider
	{
		public N5301EDIMessageDocumentWrapper(CusInBondHeader header) : base(header.Factory)
		{
			this.header = header;
			iN5301Declaration = new N5301MessageSendingObject(header);
			tWMessage = (TWMessage)this.header.Messages.GetLastMessage(ApplicationCodeList.Codes.TWCustoms, MessageTypeList.Codes.TRA);
		}

		readonly IN5301Declaration iN5301Declaration;
		readonly TWMessage tWMessage;
		readonly CusInBondHeader header;

		#region Fields
		public ZString DeclarationID => iN5301Declaration.ID;

		public ZString TranshipmentNo => DocumentWrapperHelper.GetDeclarationIDFormat(DeclarationID);

		ITransportMeans ITransportMeans => iN5301Declaration?.BorderTransportMeans;

		IConsignment IConsignment => iN5301Declaration?.Consignment;

		public ZString RegistrationNo => ITransportMeans?.Registration ?? ZString.Empty;

		public ZString ManifestSerialNumber => IConsignment?.ManifestSerialNumber ?? ZString.Empty;

		IConsignmentItem IConsignmentItem => IConsignment?.ConsignmentItem;

		IEnumerable<ITransportContractDocument> ImportBillTransportContractDocuments => IConsignment?.TransportContractDocuments;

		public ZString MasterBill => ImportBillTransportContractDocuments?.FirstOrDefault(x => x.TypeCode == MessageConstants.TransportContractDocumentTypeCodes._704 || x.TypeCode == MessageConstants.TransportContractDocumentTypeCodes._741)?.ID ?? ZString.Empty;

		public ZString HouseBill => ImportBillTransportContractDocuments?.FirstOrDefault(x => x.TypeCode == MessageConstants.TransportContractDocumentTypeCodes._703 || x.TypeCode == MessageConstants.TransportContractDocumentTypeCodes._714)?.ID ?? ZString.Empty;

		ITransportMeans DepartureTransportMeans => IConsignment?.DepartureTransportMeans;

		public ZString VesselRegistrationNumber => DepartureTransportMeans?.Registration ?? ZString.Empty;

		public ZString ShippingOrderNumber => IConsignment?.ShippingOrderNumber ?? ZString.Empty;

		IEnumerable<ITransportContractDocument> ExportBillTransportContractDocuments => IConsignmentItem.TransportContractDocuments;

		public ZString TransportMasterBill => ExportBillTransportContractDocuments?.FirstOrDefault(x => x.TypeCode == MessageConstants.TransportContractDocumentTypeCodes._704 || x.TypeCode == MessageConstants.TransportContractDocumentTypeCodes._741)?.ID ?? ZString.Empty;

		public ZString TransportHouseBill => ExportBillTransportContractDocuments?.FirstOrDefault(x => x.TypeCode == MessageConstants.TransportContractDocumentTypeCodes._703 || x.TypeCode == MessageConstants.TransportContractDocumentTypeCodes._714)?.ID ?? ZString.Empty;

		IPartyDetails DeclarationAgent => iN5301Declaration?.Agent;

		public ZString AgentID => DeclarationAgent?.ID ?? ZString.Empty;

		public ZString SubBoxID => DeclarationAgent?.SubBoxID ?? ZString.Empty;

		public ZString RepresentativePersonName => iN5301Declaration?.RepresentativePersonName ?? ZString.Empty;

		public ZString BrokerStaffName => Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, header.BH_GS_NKCusAgent)?.GS_FullName ?? ZString.Empty;

		IPartyDetails IPartyDetails => iN5301Declaration?.Applicant;

		public ZString DeclarationChineseName => GlbCompany.CurrentCompany.OrgProxy.MainAddress.GetChineseName();

		public ZString ApplicantName => IPartyDetails?.Name ?? ZString.Empty;

		public ZString ApplicantID => IPartyDetails?.ID ?? ZString.Empty;

		public ZString BorderTransportMeans => ITransportMeans?.TypeCode ?? ZString.Empty;

		public ZString TranshipmentType => iN5301Declaration?.TypeCode ?? ZString.Empty;

		public ZDateTime ArrivalDateTime => ITransportMeans?.ArrivalDateTime ?? ZDateTime.Invalid;

		public ZDateTime CloseDateTime => tWMessage?.EM_MessageDateTime ?? ZDateTime.Invalid;

		public ZString LoadingLocationID => iN5301Declaration?.LoadingLocation ?? ZString.Empty;

		public ZString LoadingLocationName => ((IFindBoxListProvider)TWRefCusCodeListTypes.GetGoodsLocationCollection(Factory, null)).DescriptionFromCode(LoadingLocationID);

		public ZString UnloadingLocationID => iN5301Declaration?.UnloadingLocation ?? ZString.Empty;

		public ZString UnloadingLocationName =>
			new RefUNLOCO.Loader(Factory).Load(UnloadingLocationID)?.RL_PortName ??
			new ZString(((IFindBoxListProvider)TWRefCusCodeListTypes.GetGoodsLocationCollection(Factory, null)).DescriptionFromCode(UnloadingLocationID));

		public ZString BorderTransportMeansName => ITransportMeans?.Name ?? ZString.Empty;

		public ZString BorderTransportMeansID => ITransportMeans?.ID ?? ZString.Empty;

		public ZString BorderTransportMeansJourneyID => ITransportMeans?.JourneyID ?? ZString.Empty;

		public ZString DepartureTransportMeansName => DepartureTransportMeans?.Name ?? ZString.Empty;

		public ZString DepartureTransportMeansID => DepartureTransportMeans?.ID ?? ZString.Empty;

		public ZString DepartureTransportMeansJourneyID => DepartureTransportMeans?.JourneyID ?? ZString.Empty;

		public ZInt TotalPackageQuantity => iN5301Declaration?.TotalPackageQuantity ?? ZInt.Zero;

		public ZString Packaging => IConsignmentItem?.Packaging?.TypeCode ?? ZString.Empty;

		public ZString MarksNumbersAndContainer => string.Format(CultureInfo.CurrentCulture, "{0}{1}{2}", MarksNumbers, System.Environment.NewLine, Container);

		public ZString MarksNumbers => IConsignmentItem?.Packaging?.MarksNumbers ?? ZString.Empty;

		public ZString Container
		{
			get
			{
				var builder = new ZStringBuilder();
				var iTransportEquipmentCollection = IConsignment?.TransportEquipments;
				if (iTransportEquipmentCollection != null)
				{
					foreach (var iTransportEquipment in iTransportEquipmentCollection)
					{
						builder.Append(string.Format(CultureInfo.CurrentCulture, "{0}/{1}/{2}", iTransportEquipment.ID, iTransportEquipment.CharacteristicCode, iTransportEquipment.UsedCapacityCode));
					}
				}
				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		public ZString OtherRecordedItems
		{
			get
			{
				var docNumber = ZString.Empty;
				var importJobRequiredDocumentCollection = header.Importer?.Header?.RequiredDocuments?.Cast<JobRequiredDocument>()?.
						Where(x => x.EQ_RN_NKRelatedCountry == Core.Constants.CountryCodes.Taiwan
						&& x.EQ_DocUsage == JobRequiredDocument.DocUsage.Broker
						&& x.EQ_ValidToDate >= ZDateTime.Today
						&& x.Attributes.Cast<JobRequiredDocAttrib>().Any(y => y.D0_AttribName == JobRequiredDocAttribTypeList.Codes.CustomsDistrict && y.D0_AttribValue == header.ReceiptOffice.Left(1))
						&& x.Attributes.Cast<JobRequiredDocAttrib>().Any(y => y.D0_AttribName == JobRequiredDocAttribTypeList.Codes.BoxNumber && y.D0_AttribValue == header.TW_BoxNumber));

				if (importJobRequiredDocumentCollection != null)
				{
					var specifiedDocTypesToLookFor = new string[] { Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, Core.Constants.RefDocTypes.PowerOfAttorney, Core.Constants.RefDocTypes.PowerOfAttorneyForwarding };
					foreach (var docType in specifiedDocTypesToLookFor)
					{
						var theBestCandidate = importJobRequiredDocumentCollection.FirstOrDefault(x => x.EQ_DocType == docType);
						if (theBestCandidate != null)
						{
							var theBestCandidateDocNumber = theBestCandidate.EQ_DocNumber;
							if (!theBestCandidateDocNumber.IsEmpty)
							{
								docNumber = FormattableString.Invariant($"委任書號:{theBestCandidateDocNumber}");
							}

							break;
						}
					}
				}

				return docNumber;
			}
		}

		public ZString Barcode
		{
			get
			{
				ZString barCode;
				if (DeclarationID.SubstringSafe(2, 2).IsEmpty)
				{
					barCode = string.Format(CultureInfo.CurrentCulture, "{0}{1}", DeclarationID.SubstringSafe(0, 2), DeclarationID.SubstringSafe(4, 10));
				}
				else
				{
					barCode = DeclarationID;
				}
				return string.Format(CultureInfo.CurrentCulture, "*{0}*", barCode);
			}
		}

		public ZString BarcodeSplitWithSpace
		{
			get
			{
				var stringBuilder = new ZStringBuilder();
				var barcode = Barcode;
				foreach (char barcodeChar in barcode)
				{
					stringBuilder.Append(barcodeChar.ToString());
					stringBuilder.Append(" ");
				}
				return stringBuilder.ToString().TrimEnd(' ');
			}
		}

		public ZString CargoDescription => IConsignmentItem?.Commodity?.CargoDescription ?? ZString.Empty;

		public ZDecimal GoodsMeasureTariffQuantity => IConsignmentItem?.GoodsMeasure?.TariffQuantity ?? ZDecimal.Zero;

		public ZString GoodsMeasureUnitCode => IConsignmentItem?.GoodsMeasure?.UnitCode ?? ZString.Empty;

		public ZDecimal TotalGrossMassMeasure => iN5301Declaration?.TotalGrossMassMeasure ?? ZDecimal.Zero;
		#endregion

		#region IBODocDataProvider

		DocWrapperCopyInfo IBODocDataProvider.AdditionalCopyInfo => BasicBODocDataProvider.AdditionalCopyInfo;

		BusinessObject IBODocDataProvider.BusinessObjectToLogAgainst => header;

		BusinessObject IBODocDataProvider.ParentBusinessObject => BasicBODocDataProvider.ParentBusinessObject;

		void IBODocDataProvider.SetDocWrapperContext(Dictionary<string, object> constants) => BasicBODocDataProvider.SetDocWrapperContext(constants);

		string IBODocDataProvider.ToString() => header.HumanReadableName;

		ZString IBODocDataProvider.GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue) => BasicBODocDataProvider.GetDocDataValue(docDataIdentifier, formatStringForFallbackValue);

		IZType IBODocDataProvider.GetCustomField(string fieldName, string typeName) => BasicBODocDataProvider.GetCustomField(fieldName, typeName);

		string IBODocDataProvider.GetCustomFieldCodeDescription(string fieldName, string typeName) => BasicBODocDataProvider.GetCustomFieldCodeDescription(fieldName, typeName);

		ZDateTime IBODocDataProvider.GetEventLastDateTime(string eventCode) => BasicBODocDataProvider.GetEventLastDateTime(eventCode);

		string[] IBODocDataProvider.ImageNamesToRemove => BasicBODocDataProvider.ImageNamesToRemove;

		IBODocDataProvider BasicBODocDataProvider => basicBODocDataProvider ?? (basicBODocDataProvider = BODocDataProvider.GetDefault(this));
		IBODocDataProvider basicBODocDataProvider;

		#endregion
	}
}
