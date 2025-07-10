using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.AES;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AES
{
	public abstract class AESPrint : NonPersistentBusinessObject,
		IObsoleteValidation,
		IDocumentDeliveredLogSupporter,
		IParentDocManagerSupport,
		IVisualizerNoteSupporter
	{
		protected AESPrint(CusEntryHeader entry)
			: base(entry.Factory)
		{
			this.entry = entry;
		}
		public readonly CusEntryHeader entry;

		public abstract ZString TransmittedVia { get; }
		public abstract ZString ShipmentReferenceNumber { get; }
		public abstract ZString ITN { get; }
		public abstract ZDate DepartureDate { get; }
		public abstract ZString TransportationReferenceNo { get; }
		public abstract ZString StateOfOrigin { get; }
		public abstract ZString CountryOfDestination { get; }
		public abstract ZString PortOfExport { get; }
		public abstract ZString ModeOfTransportation { get; }
		public abstract ZString CarrierSCAC { get; }
		public abstract ZString ConveyanceName { get; }
		public abstract ZString RoutedTransactionIndicator { get; }
		public abstract ZString RelatedCompaniesIndicator { get; }
		public abstract ZString HazardousIndicator { get; }
		public abstract ZString InBondType { get; }
		public abstract ZString FTZ { get; }
		public abstract ZString ImportEntryNumber { get; }

		public abstract ZString USPPIName { get; }
		public abstract ZString USPPIIDNumber { get; }
		public abstract ZString USPPIContactName { get; }
		public abstract ZString USPPIPhone { get; }
		public abstract ZString USPPICargoOriginLine1 { get; }
		public abstract ZString USPPICargoOriginLine2 { get; }
		public abstract ZString USPPICargoOriginLine3 { get; }

		public abstract ZString UltimateConsigneeName { get; }
		public abstract ZString UltimateConsigneeContact { get; }
		public abstract ZString UltimateConsigneePhone { get; }
		public abstract ZString UltimateConsigneeAddress1 { get; }
		public abstract ZString UltimateConsigneeAddress2 { get; }
		public abstract ZString UltimateConsigneeAddress3 { get; }
		public abstract ZString UltimateConsigneeType { get; }

		public abstract ZString FreightForwarderName { get; }
		public abstract ZString FreightForwarderIDNumber { get; }
		public abstract ZString FreightForwarderContact { get; }
		public abstract ZString FreightForwarderPhone { get; }
		public abstract ZString FreightForwarderAddress1 { get; }
		public abstract ZString FreightForwarderAddress2 { get; }
		public abstract ZString FreightForwarderAddress3 { get; }

		public abstract ZString IntermediateConsigneeName { get; }
		public abstract ZString IntermediateConsigneeContact { get; }
		public abstract ZString IntermediateConsigneePhone { get; }
		public abstract ZString IntermediateConsigneeAddress1 { get; }
		public abstract ZString IntermediateConsigneeAddress2 { get; }
		public abstract ZString IntermediateConsigneeAddress3 { get; }

		public abstract AESPrintCommodityLinesCollection Commodities { get; }

		public ZString FilerID
		{
			get { return USCustomsDataRegistry.Instance.ExportEntryFilerID.GetFallBackValueAtAllLevels(entry.Declaration.RegistryCompanyPK, entry.Declaration.RegistryBranchPK, Guid.Empty).EntryFilerID.KeepAlphanumericCharacters(); }
		}

		public ZString GetFormattedEntryNumber(ZString code)
		{
			ZString result = code;
			if (result.Length > 3)
			{
				result = result.Substring(0, 3) + " " + result.Substring(3);
			}
			return result;
		}

		public ZString GetInBondTypeAndDescription(ZString code)
		{
			ZString result = code;
			if (result != "")
			{
				result += " " + entry.AddInfoLookups.US_InbondType_List.GetDescriptionFromCode(code);
			}
			return result;
		}

		public ZString UltimateConsigneeTypeFormat
		{
			get
			{
				return UltimateConsigneeType.IsEmpty ? string.Empty
					: Factory.GetCachedValue<UltimateConsigneeTypeList>().GetDescriptionFromCode(UltimateConsigneeType);
			}
		}

		public ZString CurrentDateTimeInSpecificFormat
		{
			get
			{
				var monthFormatted = ZDateTime.Now.Month.ToString().Length < 2 ? "0" + ZDateTime.Now.Month.ToString() : ZDateTime.Now.Month.ToString();
				return ZDateTime.Now.DayOfWeek.ToString().Substring(0, 3) + " " +
						new MonthList().GetDescriptionFromCode(monthFormatted) + " " +
						ZDateTime.Now.Day + " " +
						ZDateTime.Now.Hour + ":" + ZDateTime.Now.Minute + ":" + ZDateTime.Now.Second + " " +
						ZDateTime.Now.Year + " EDT";
			}
		}

		internal ZString GetModeOfTransportationDescription(string modeOfTransportationCode)
		{
			var result = ZString.Empty;
			if (!string.IsNullOrEmpty(modeOfTransportationCode))
			{
				var description = (ZString)new TransportModeCodes().GetDescriptionFromCode(modeOfTransportationCode);
				result = !description.IsEmpty ? description + " (" + modeOfTransportationCode + ")" : modeOfTransportationCode;
			}
			return result;
		}

		internal ZString GetCountryOfDestinationName(ZString countryCode)
		{
			var countryLoadedByCode = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, countryCode));
			if (countryLoadedByCode != null)
			{
				return countryLoadedByCode.RN_DescMultilingual.GetUnresolvedString().ToUpper() + " (" + countryCode.ToUpper() + ")";
			}
			return ZString.Empty;
		}

		internal ZString GetOriginIndicatorDescription(ZString code)
		{
			return code != "" ? (ZString)entry.AddInfoLookups.US_AESOriginIndicator_List.GetDescriptionFromCode(code).ToUpper() : ZString.Empty;
		}

		internal ZString GetStateDescription(string stateCode)
		{
			var result = ZString.Empty;
			if (!string.IsNullOrEmpty(stateCode))
			{
				ZString stateDescription = (entry.AddInfoLookups.USStateList.GetDescriptionFromCode(stateCode) ?? "").ToUpper();
				result = !stateDescription.IsEmpty ? stateDescription + " (" + stateCode + ")" : stateCode;
			}
			return result;
		}

		internal ZString GetPortOfExportDescription(ZString portOfExportCode)
		{
			var portLoadedByPortCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, portOfExportCode, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
			if (portLoadedByPortCode != null)
			{
				return portLoadedByPortCode.ZZD_Description.ToUpper() + " (" + portOfExportCode.ToUpper() + ")";
			}
			return ZString.Empty;
		}

		internal ZString GetIDTypeFormatted(ZString iDType)
		{
			var result = ZString.Empty;
			if (iDType == AESConstants.IDTypes.EmployerIdentificationNumber)
			{
				result = " (" + OrgCusCode.USACodeTypes.EmployerIdentificationNumber + ")";
			}
			else if (iDType == AESConstants.IDTypes.SocialSecurityNumber)
			{
				result = " (" + OrgCusCode.USACodeTypes.SocialSecurityNumber + ")";
			}
			else if (iDType == AESConstants.IDTypes.DUNS)
			{
				result = " (" + OrgCusCode.CodeTypes.DataUniversalNumberingSystem + ")";
			}
			else if (iDType == AESConstants.IDTypes.Foreign)
			{
				result = " (" + OrgCusCode.USACodeTypes.ForeignRegistrationNumber + ")";
			}
			return result;
		}

		#region IDocumentDeliveredLogSupporter Members

		Type IDocumentDeliveredLogSupporter.BusinessObjectTypeToLogAgainst
		{
			get { return entry.GetType(); }
		}

		ZGuid IDocumentDeliveredLogSupporter.Identifier
		{
			get { return entry.PK; }
		}

		#endregion

		#region IParentDocManagerSupport Members

		ZGuid IParentDocManagerSupport.ParentGuid
		{
			get { return entry.PK; }
		}

		ZString IParentDocManagerSupport.ParentTableName
		{
			get { return CusEntryHeaderSchema.Constants.TableName; }
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.CustomsEntry)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IVisualizerNoteSupporter members

		ZGuid IVisualizerNoteSupporter.PK => entry.PK;

		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;

		string IVisualizerNoteSupporter.TableCode => CusEntryHeaderSchema.Constants.Prefix;

		#endregion
	}
}
