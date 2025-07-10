using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[GlowDataDefinition("IUSAPHISInspections")]
	public class APHISInspection : AutoAPHISInspection, IAPHISInspection
	{
		public APHISInspection(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoAPHISInspection.Schema
		{
			public const string US_PortTypeDesc = "US_PortTypeDesc";
			public const string US_TestingStatusDesc = "US_TestingStatusDesc";
		}

		public APHISHeader Header
		{
			get { return (APHISHeader)Parent; }
		}

		internal JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					var invoiceLine = Header != null ? Header.Parent : null;
					declaration = invoiceLine != null ? invoiceLine.Declaration : null;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		#region Override Properties

		[ResourceStringData("Enterprise.Customs.US.Business.APHISInspection|US_Date", Caption = "Inspection Date", ShortCaption = "Date")]
		public override ZDateTime US_Date
		{
			get { return base.US_Date; }
			set { base.US_Date = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISInspection|US_PortTypeDesc", Caption = "Type", ShortCaption = "Type")]
		public ZString US_PortTypeDesc
		{
			get
			{
				var result = ZString.Empty;
				if (IsUNLOCOApplicable)
				{
					result = InspectionLocationCodeList.Descriptions.UNLOCO;
				}
				else if (IsScheduleDApplicable)
				{
					result = InspectionLocationCodeList.Descriptions.ScheduleD;
				}
				return result;
			}
		}

		public bool IsUNLOCOApplicable
		{
			get { return US_TestingStatus == InspectionStatusList.Codes.PreviouslyPerformed; }
		}

		bool IsScheduleDApplicable
		{
			get
			{
				return US_TestingStatus == InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			}
		}

		public ZPropertyInfo US_PortTypeDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_PortTypeDesc); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISInspection|US_PortCode", Caption = "Port Code", ShortCaption = "Port Code")]
		public override ZString US_Location
		{
			get { return base.US_Location; }
			set { base.US_Location = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISInspection|US_TestingStatus", Caption = "Inspection Testing Status", ShortCaption = "Testing Status")]
		public override ZString US_TestingStatus
		{
			get { return base.US_TestingStatus; }
			set
			{
				var hasChange = US_TestingStatus != value;
				base.US_TestingStatus = value;

				if (hasChange && !IsCopying)
				{
					if (US_Location.IsEmpty && Declaration != null)
					{
						if (IsScheduleDApplicable)
						{
							US_Location = Declaration.US_SchDArrival;
						}
						else if (IsUNLOCOApplicable && Declaration.Consignee != null)
						{
							US_Location = Declaration.Consignee.UNLOCO?.Code ?? ZString.Empty;
						}
					}

					US_PortTypeDescInfo.RefreshBinding();
					US_LocationInfo.RefreshBinding();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISInspection|US_TestingStatusDesc", Caption = "Inspection Testing Status Description", ShortCaption = "Testing Status Desc.")]
		public ZString US_TestingStatusDesc
		{
			get { return AddInfoLookups.InspectionStatusList.GetDescriptionFromCode(US_TestingStatus); }
		}

		public ZPropertyInfo US_TestingStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_TestingStatusDesc); }
		}

		#endregion

		#region IAPHISInspection Members

		ZString IAPHISInspection.InspectionTestingStatus
		{
			get { return US_TestingStatus; }
		}

		ZDate IAPHISInspection.InspectionDate
		{
			get { return US_Date.IsValid ? US_Date.Date : ZDate.Empty; }
		}

		ZString IAPHISInspection.InspectionLocationQualifier
		{
			get
			{
				return IsScheduleDApplicable
					? (ZString)InspectionLocationCodeList.Codes.ScheduleD
					: IsUNLOCOApplicable ? (ZString)InspectionLocationCodeList.Codes.UNLOCO : ZString.Empty;
			}
		}

		ZString IAPHISInspection.InspectionLocation
		{
			get { return US_Location; }
		}

		#endregion
	}
}
