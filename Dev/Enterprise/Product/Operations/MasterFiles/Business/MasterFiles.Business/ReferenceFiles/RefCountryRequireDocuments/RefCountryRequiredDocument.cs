using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefCountryRequiredDocument : AutoRefCountryRequiredDocument
	{
		public RefCountryRequiredDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[List("Lookups.RT_ReferenceType_List")]
		public override ZString RD_DocType
		{
			get
			{
				return base.RD_DocType;
			}
			set
			{
				base.RD_DocType = value;
			}
		}

		[List("Lookups.RefCountry_List")]
		public override ZString RD_RN_NKOrigin
		{
			get
			{
				return base.RD_RN_NKOrigin;
			}
			set
			{
				base.RD_RN_NKOrigin = value;
			}
		}

		[List("Lookups.RefCountry_List")]
		public override ZString RD_RN_NKDestination
		{
			get
			{
				return base.RD_RN_NKDestination;
			}
			set
			{
				base.RD_RN_NKDestination = value;
			}
		}

		[List("Lookups.DocUsage_List")]
		public override ZString RD_DocUsage
		{
			get
			{
				return base.RD_DocUsage;
			}
			set
			{
				base.RD_DocUsage = value;
			}
		}

		[List("Lookups.TransportMode_List")]
		public override ZString RD_TransportMode
		{
			get
			{
				return base.RD_TransportMode;
			}
			set
			{
				base.RD_TransportMode = value;
			}
		}

		#endregion

		#region Schema

		public new class Schema : AutoRefCountryRequiredDocument.Schema
		{
			public const string DocTypeDescription = "DocTypeDescription";
		}

		#endregion

		[MaxLength(200)]
		public ZString DocTypeDescription
		{
			get { return DocType != null ? DocType.RT_DescMultilingual : ZString.Empty; }
		}

		public ZPropertyInfo DocTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.DocTypeDescription); }
		}

		public RefDocType DocType
		{
			get
			{
				ZQuery query = new ZQuery(RefDocTypeSchema.RT_ReferenceType, Constants.ReferenceTypes.SupplyChainLogistics);
				query.AddToFilter(JoinCondition.And, RefDocTypeSchema.RT_DocType, RD_DocType);
				query.AddToFilter(JoinCondition.And, RefDocTypeSchema.RT_IsActive, true);
				return Factory.LoadTop1<RefDocType>(query);
			}
		}

		public ZString SelectedCountryCode { get; set; }
	}
}
