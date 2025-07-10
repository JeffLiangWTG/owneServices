using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business
{
	public class USInBondMoveHeader : AutoUSInBondMoveHeader, IDocumentSupportable, IDocumentDeliveredLogSupporter, IControllerIDProvider, IStmALogParent, IParentDocManagerSupport
	{
		public USInBondMoveHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		#region New Properties

		public CusInBondHeader Header => Factory.Load<CusInBondHeader>(BMH_BH);

		public CusInBondMoveHeader MoveHeader => Factory.Load<CusInBondMoveHeader>(PK);

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.USInBondMoveHeader|BMH_InBondNumber", Caption = "InBond Number")]
		public ZString BMH_InBondNumber => MoveHeader.InBondNumber;

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.USInBondMoveHeader|BMH_CustomsStatusDescription", Caption = "QP Status Desc.")]
		public ZString BMH_CustomsStatusDescription => Factory.GetCachedValue<MessageStatusListIT>().GetDescriptionFromCode(BMH_CustomsStatus);

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.USInBondMoveHeader|BMH_MessageStatusDescription", Caption = "WP Status Desc.")]
		public ZString BMH_MessageStatusDescription => Factory.GetCachedValue<MessageStatusListIT>().GetDescriptionFromCode(BMH_MessageStatus);

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.USInBondMoveHeader|BMH_ImporterName", Caption = "Importer")]
		public ZString BMH_ImporterName => Header.ImporterName;

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.USInBondMoveHeader|BMH_InBondCarrier", Caption = "In-Bond Carrier (org.)", ShortCaption = "In-Bond Carrier")]
		public ZString BMH_InBondCarrier => MoveHeader.InBondCarrierOrg?.OH_Code ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.USInBondMoveHeader|BMH_InBondCarrierName", Caption = "In-Bond Carrier Name")]
		public ZString BMH_InBondCarrierName
		{
			get
			{
				var carrierSCAC = Factory.LoadFromNaturalKey<USCarrierCombined>(USCarrierCombinedSchema.UI_Code, BMH_InBondCarrierSCAC);
				return carrierSCAC?.UI_Name ?? ZString.Empty;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.USInBondMoveHeader|BMH_USDestinationPortName", Caption = "US Destination (Port Name)", ShortCaption = "US Dest. Port Name")]
		public ZString BMH_USDestinationPortName
		{
			get
			{
				var destinationPortDCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, BMH_DestinationPortCode, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
				return destinationPortDCode?.ZZD_Description ?? ZString.Empty;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.USInBondMoveHeader|BMH_ForeignDestinationPortName", Caption = "Foreign Destination (Port Name)", ShortCaption = "Foreign Dest. Port Name")]
		public ZString BMH_ForeignDestinationPortName
		{
			get
			{
				return Factory.GetCachedValue("USInBondMoveHeader|BMH_ForeignDestinationPortName|" + BMH_ForeignDestPortKCode, () =>
				{
					var foreignDestPortKCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, BMH_ForeignDestPortKCode, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
					return foreignDestPortKCode?.ZZD_Description ?? ZString.Empty;
				});
			}
		}

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.USInBondMoveHeader|BMH_InBondQTY", Caption = "In-Bond QTY")]
		public ZInt BMH_InBondQTY
		{
			get
			{
				var result = ZInt.Zero;

				foreach (var moveDetail in MoveHeader.MovementDetails)
				{
					result += moveDetail.B9_InBoundQty;
				}

				return result;
			}
		}

		#endregion

		#region Override Properties

		[List(nameof(Lookups) + "." + nameof(USInBondMoveHeaderLookups.CarrierCollection))]
		public override ZString BMH_CarrierSCAC { get => base.BMH_CarrierSCAC; set => base.BMH_CarrierSCAC = value; }

		[List(nameof(Lookups) + "." + nameof(USInBondMoveHeaderLookups.InbondQPMessageStatusList))]
		public override ZString BMH_CustomsStatus { get => base.BMH_CustomsStatus; set => base.BMH_CustomsStatus = value; }

		[List(nameof(Lookups) + "." + nameof(USInBondMoveHeaderLookups.RegionDistrictPorts))]
		public override ZString BMH_DestinationPortCode { get => base.BMH_DestinationPortCode; set => base.BMH_DestinationPortCode = value; }

		[List(nameof(Lookups) + "." + nameof(USInBondMoveHeaderLookups.FIRMSCollection))]
		public override ZString BMH_FIRMS { get => base.BMH_FIRMS; set => base.BMH_FIRMS = value; }

		[List(nameof(Lookups) + "." + nameof(USInBondMoveHeaderLookups.ForeignPorts))]
		public override ZString BMH_ForeignDestPortKCode { get => base.BMH_ForeignDestPortKCode; set => base.BMH_ForeignDestPortKCode = value; }

		[List(nameof(Lookups) + "." + nameof(USInBondMoveHeaderLookups.Branches))]
		public override ZGuid BMH_GB { get => base.BMH_GB; set => base.BMH_GB = value; }

		[List(nameof(Lookups) + "." + nameof(USInBondMoveHeaderLookups.Countries))]
		public override ZString BMH_ImportConveyanceCountry { get => base.BMH_ImportConveyanceCountry; set => base.BMH_ImportConveyanceCountry = value; }

		[List(nameof(Lookups) + "." + nameof(USInBondMoveHeaderLookups.ImportingConveyanceList))]
		public override ZString BMH_ImportConveyanceName { get => base.BMH_ImportConveyanceName; set => base.BMH_ImportConveyanceName = value; }

		[List(nameof(Lookups) + "." + nameof(USInBondMoveHeaderLookups.ScheduleKCodes))]
		public override ZString BMH_ImportLoadPortKCode { get => base.BMH_ImportLoadPortKCode; set => base.BMH_ImportLoadPortKCode = value; }

		[List(nameof(Lookups) + "." + nameof(USInBondMoveHeaderLookups.TransportModeCodes))]
		public override ZString BMH_ImportTransportMode { get => base.BMH_ImportTransportMode; set => base.BMH_ImportTransportMode = value; }

		[List(nameof(Lookups) + "." + nameof(USInBondMoveHeaderLookups.CarrierCollection))]
		public override ZString BMH_InBondCarrierSCAC { get => base.BMH_InBondCarrierSCAC; set => base.BMH_InBondCarrierSCAC = value; }

		[List(nameof(Lookups) + "." + nameof(USInBondMoveHeaderLookups.EntryTypeList))]
		public override ZString BMH_InBondEntryType { get => base.BMH_InBondEntryType; set => base.BMH_InBondEntryType = value; }

		[List(nameof(Lookups) + "." + nameof(USInBondMoveHeaderLookups.InbondWPMessageStatusList))]
		public override ZString BMH_MessageStatus { get => base.BMH_MessageStatus; set => base.BMH_MessageStatus = value; }

		[List(nameof(Lookups) + "." + nameof(USInBondMoveHeaderLookups.Importers))]
		public override ZGuid BMH_OA_Importer { get => base.BMH_OA_Importer; set => base.BMH_OA_Importer = value; }

		[List(nameof(Lookups) + "." + nameof(USInBondMoveHeaderLookups.InBondCarriers))]
		public override ZGuid BMH_OA_InBondCarrier { get => base.BMH_OA_InBondCarrier; set => base.BMH_OA_InBondCarrier = value; }

		protected override ZString HumanReadableNameCore => Header.HumanReadableName;

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var stringBuilder = new ZStringBuilder();
				stringBuilder.Append(HumanReadableName);

				if (!BMH_InBondNumber.IsEmpty)
				{
					stringBuilder.Append(" (");
					stringBuilder.Append(BMH_InBondNumber);
					stringBuilder.Append(")");
				}

				if (!BMH_ImporterName.IsEmpty)
				{
					stringBuilder.Append(" - ");
					stringBuilder.Append(BMH_ImporterName);
				}

				return stringBuilder.ToString();
			}
		}

		#endregion

		#region IDocumentSupportable Members

		DocumentSupporter IDocumentSupportable.DocumentSupporter => new USInBondMoveHeaderDocumentSupporter(this);

		#endregion

		#region IDocumentDeliveredLogSupporter Members

		Type IDocumentDeliveredLogSupporter.BusinessObjectTypeToLogAgainst => typeof(CusInBondMoveHeader);

		ZGuid IDocumentDeliveredLogSupporter.Identifier => PK;

		#endregion

		#region IControllerIDProvider Members

		Guid IControllerIDProvider.BusinessObjectPK => PK.ToGuid();

		ControllerID IControllerIDProvider.ControllerID => ControllerIDs.Customs.US.InBondMoveHeader;

		#endregion

		#region IStmALogParent Members

		string IStmALogParent.LogsParentTableName => CusInBondMoveHeader.Schema.TableName;

		#endregion

		#region IParentDocManagerSupport Members

		ZGuid IParentDocManagerSupport.ParentGuid
		{
			get
			{
				return MoveHeader?.PK ?? ZGuid.Empty;
			}
		}

		ZString IParentDocManagerSupport.ParentTableName
		{
			get
			{
				return MoveHeader?.TableName ?? ZString.Empty;
			}
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(MoveHeader, Core.Constants.DocManagerCodes.InBond)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion
	}
}
