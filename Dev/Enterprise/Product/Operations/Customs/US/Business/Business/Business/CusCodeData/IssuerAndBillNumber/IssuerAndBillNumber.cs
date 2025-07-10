using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// CY_Data has four-letter issuer code and master bill number concatenated.
	/// Each AMS Bills of lading message has a record of this for fast SQL load
	/// and CargoManifestStatusQuery uses this to have issuer codes and bill numbers persisted.
	/// </summary>
	public class IssuerAndBillNumber : Customs.Business.CusCodeData
	{
		#region Loader

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : Customs.Business.CusCodeData.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public IssuerAndBillNumber[] Load(MQEDIMessage message)
			{
				return Factory.Load<IssuerAndBillNumber>(GetQuery(message.PK));
			}

			public IssuerAndBillNumber LoadTop1(MQEDIMessage message)
			{
				ZQuery query = GetQuery(message.PK);
				query.FetchOnlyFromLocalCache = !message.IsInDatabase;
				return Factory.LoadTop1<IssuerAndBillNumber>(query);
			}

			public IssuerAndBillNumber LoadTop1(MQEDIMessage message, ZString issuerCodeAndBillNumber)
			{
				ZQuery query = GetQuery(message.PK);
				query.AddToFilter(CusCodeDataSchema.CY_Data, issuerCodeAndBillNumber);
				query.FetchOnlyFromLocalCache = !message.IsInDatabase;
				return Factory.LoadTop1<IssuerAndBillNumber>(query);
			}

			ZQuery GetQuery(ZGuid parentPK)
			{
				ZQuery query = new ZQuery(CusCodeDataSchema.CY_ParentID, parentPK);
				query.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.IssuerAndBillNumber);
				return query;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(IssuerAndBillNumber);
			}
		}
		#endregion

		public IssuerAndBillNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : Customs.Business.CusCodeData.Schema
		{
			public const string CY_IssuerCode = "CY_IssuerCode";
			public const int CY_IssuerCodeMaxLength = 4;

			public const string CY_BillNumber = "CY_BillNumber";
			public const int CY_BillNumberMaxLength = 31;
		}

		public new MQEDIMessage Parent
		{
			get { return (MQEDIMessage)base.Parent; }
			set { base.Parent = value; }
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(MQEDIMessage)); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.IssuerAndBillNumber;
		}

		protected override Customs.Business.CusCodeDataValidation GetNewValidation()
		{
			return new IssuerAndBillNumberValidation(this);
		}

		[MaxLength(Schema.CY_IssuerCodeMaxLength)]
		public ZString CY_IssuerCode
		{
			get
			{
				if (!issuerCodeLoaded)
				{
					issuerCodeLoaded = true;
					fIssuerCode = CY_Data.Left(Schema.CY_IssuerCodeMaxLength).Trim();
				}
				return fIssuerCode;
			}
			set
			{
				SetNonPersistentPropertyValue(CY_IssuerCodeInfo, ref fIssuerCode, value);
				PersistToCY_Data();
			}
		}
		ZString fIssuerCode;
		bool issuerCodeLoaded;

		public ZPropertyInfo CY_IssuerCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CY_IssuerCode); }
		}

		ZString IssuerCodeToPersist
		{
			get { return CY_IssuerCode.PadRight(Schema.CY_IssuerCodeMaxLength, ' '); }
		}

		[MaxLength(Schema.CY_BillNumberMaxLength)]
		public ZString CY_BillNumber
		{
			get
			{
				if (!billNumberLoaded)
				{
					billNumberLoaded = true;
					fBillNumber = CY_Data.SubstringSafe(Schema.CY_IssuerCodeMaxLength);
				}
				return fBillNumber;
			}
			set
			{
				SetNonPersistentPropertyValue(CY_BillNumberInfo, ref fBillNumber, value);
				PersistToCY_Data();
			}
		}
		ZString fBillNumber;
		bool billNumberLoaded;

		public ZPropertyInfo CY_BillNumberInfo
		{
			get { return GetZPropertyInfo(Schema.CY_BillNumber); }
		}

		void PersistToCY_Data()
		{
			CY_Data = IssuerCodeToPersist + CY_BillNumber;
		}
	}
}
