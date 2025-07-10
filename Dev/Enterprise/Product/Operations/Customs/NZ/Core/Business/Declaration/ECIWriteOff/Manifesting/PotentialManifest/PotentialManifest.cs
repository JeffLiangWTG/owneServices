using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting
{
	public class PotentialManifest : DynamicBusinessObject, IObsoleteValidation
	{
		public PotentialManifest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		public static class Schema
		{
			public const string JE_MasterBill = "JE_MasterBill";
			public const string JE_FormattedMasterBill = "JE_FormattedMasterBill";
			public const string JE_VoyageFlightNo = "JE_VoyageFlightNo";
			public const string JE_OH_ShippingLine = "JE_OH_ShippingLine";
			public const string ShippingLineFullName = "ShippingLineFullName";
			public const string JE_MessageType = "JE_MessageType";
			public const string MessageTypeDescription = "MessageTypeDescription";
			public const string BarrierDate = "BarrierDate";
			public const string BarrierPort = "BarrierPort";
			public const string DeclarationCount = "DeclarationCount";
		}
		#endregion

		#region JE_MasterBill
		[MaxLength(JobDeclaration.Schema.JE_MasterBillMaxLength)]
		public ZString JE_MasterBill
		{
			get { return new ZString(this[Schema.JE_MasterBill]); }
		}

		public ZPropertyInfo JE_MasterBillInfo
		{
			get { return GetZPropertyInfo(Schema.JE_MasterBill); }
		}
		#endregion

		#region JE_FormattedMasterBill
		[MaxLength(JobDeclaration.Schema.JE_MasterBillMaxLength + 1)]
		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.PotentialManifest|JE_FormattedMasterBill", Caption = "Master Bill")]
		public ZString JE_FormattedMasterBill
		{
			get { return JE_MasterBill.Left(3) + "-" + JE_MasterBill.SubstringSafe(3); }
		}

		public ZPropertyInfo JE_FormattedMasterBillInfo
		{
			get { return GetZPropertyInfo(Schema.JE_FormattedMasterBill); }
		}
		#endregion

		#region JE_VoyageFlightNo
		[MaxLength(JobDeclaration.Schema.JE_VoyageFlightNoMaxLength)]
		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.PotentialManifest|JE_VoyageFlightNo", Caption = "Flight No")]
		public ZString JE_VoyageFlightNo
		{
			get { return new ZString(this[Schema.JE_VoyageFlightNo]); }
		}

		public ZPropertyInfo JE_VoyageFlightNoInfo
		{
			get { return GetZPropertyInfo(Schema.JE_VoyageFlightNo); }
		}
		#endregion

		#region JE_OH_ShippingLine
		public ZGuid JE_OH_ShippingLine
		{
			get { return new ZGuid(this[Schema.JE_OH_ShippingLine]); }
		}

		public ZPropertyInfo JE_OH_ShippingLineInfo
		{
			get { return GetZPropertyInfo(Schema.JE_OH_ShippingLine); }
		}
		#endregion

		#region ShippingLineFullName
		[MaxLength(OrgHeader.Schema.OH_FullNameMaxLength)]
		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.PotentialManifest|ShippingLineFullName", Caption = "Carrier")]
		public ZString ShippingLineFullName
		{
			get
			{
				var shippingLine = Factory.Load<OrgHeader>(JE_OH_ShippingLine);
				return shippingLine != null ? shippingLine.OH_FullNameTruncated : ZString.Empty;
			}
		}

		public ZPropertyInfo ShippingLineFullNameInfo
		{
			get { return GetZPropertyInfo(Schema.ShippingLineFullName); }
		}
		#endregion

		#region JE_MessageType
		[MaxLength(JobDeclaration.Schema.JE_MessageTypeMaxLength)]
		public ZString JE_MessageType
		{
			get { return new ZString(this[Schema.JE_MessageType]); }
		}

		public ZPropertyInfo JE_MessageTypeInfo
		{
			get { return GetZPropertyInfo(Schema.JE_MessageType); }
		}
		#endregion

		#region MessageTypeDescription
		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.PotentialManifest|MessageTypeDescription", Caption = "Message Type")]
		public ZString MessageTypeDescription
		{
			get { return Factory.GetCachedValue<JobMessageTypeList>().GetDescriptionFromCode(JE_MessageType); }
		}

		public ZPropertyInfo MessageTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.MessageTypeDescription); }
		}
		#endregion

		#region BarrierDate
		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.PotentialManifest|BarrierDate", Caption = "Local Xfer. Date")]
		public ZDateTime BarrierDate
		{
			get { return new ZDateTime(this[Schema.BarrierDate]); }
		}

		public ZPropertyInfo BarrierDateInfo
		{
			get { return GetZPropertyInfo(Schema.BarrierDate); }
		}
		#endregion

		#region BarrierPort
		[MaxLength(JobDeclaration.Schema.JE_RL_NKPortOfArrivalMaxLength)]
		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.PotentialManifest|BarrierPort", Caption = "Local Xfer. Port")]
		public ZString BarrierPort
		{
			get { return new ZString(this[Schema.BarrierPort]); }
		}

		public ZPropertyInfo BarrierPortInfo
		{
			get { return GetZPropertyInfo(Schema.BarrierPort); }
		}
		#endregion

		#region DeclarationCount
		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.PotentialManifest|DeclarationCount", Caption = "Decs. Ready")]
		public ZInt DeclarationCount
		{
			get { return new ZInt(this[Schema.DeclarationCount]); }
		}

		public ZPropertyInfo DeclarationCountInfo
		{
			get { return GetZPropertyInfo(Schema.DeclarationCount); }
		}
		#endregion

		#region Delete
		public override void Delete()
		{
			throw new NotSupportedException("You cannot delete a Dynamic Business Object");
		}
		#endregion

		#region OrganisationsList
		public OrgHeaderCollection OrganisationsList
		{
			get { return new OrgHeaderCollection(Factory); }
		}
		#endregion

		#region UNLOCOsList
		public RefUNLOCOCollection UNLOCOsList
		{
			get { return new RefUNLOCOCollection(Factory); }
		}
		#endregion

		#region GetDeclarationsForManifest
		public JobDeclarationCollectionECIWriteOff GetDeclarationsForManifest(BusinessObjectFactory factory, ZGuid companyPkToFilterOn)
		{
			var result = new JobDeclarationCollectionECIWriteOff(factory, companyPkToFilterOn);

			var filter = new ZQuery(JobDeclarationSchema.JE_EntryStatus, LowValueConsignmentStatusList.Codes.ReadyForManifesting);
			filter.AddToFilter(JobDeclarationSchema.JE_MasterBill, JE_MasterBill);
			filter.AddToFilter(JobDeclarationSchema.JE_VoyageFlightNo, JE_VoyageFlightNo);
			filter.AddToFilter(JobDeclarationSchema.JE_OH_ShippingLine, JE_OH_ShippingLine.IsEmpty ? DBNull.Value : JE_OH_ShippingLine);

			if (JE_MessageType == JobMessageTypeList.Codes.Import)
			{
				filter.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Import);
				filter.AddToFilter(JobDeclarationSchema.JE_RL_NKPortOfArrival, BarrierPort);
				filter.AddToFilter(JobDeclarationSchema.JE_DateOfArrival, SQLComparisonOperator.EqualToDatePartOnly, BarrierDate);
			}
			else if (JE_MessageType == JobMessageTypeList.Codes.Export)
			{
				filter.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Export);
				filter.AddToFilter(JobDeclarationSchema.JE_RL_NKPortOfLoading, BarrierPort);
				filter.AddToFilter(JobDeclarationSchema.JE_ExportDate, SQLComparisonOperator.EqualToDatePartOnly, BarrierDate);
			}
			else
			{
				filter.IsNoResultQuery = true;
			}

			result.Load(filter);
			return result;
		}
		#endregion
	}
}
