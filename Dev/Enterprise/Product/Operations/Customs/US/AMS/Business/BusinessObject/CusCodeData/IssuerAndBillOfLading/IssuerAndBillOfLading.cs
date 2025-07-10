using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business
{
	public class IssuerAndBillOfLading : Customs.Business.CusCodeData
	{
		public IssuerAndBillOfLading(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : Customs.Business.CusCodeData.Schema
		{
			public const string CY_IssuerCode = "CY_IssuerCode";
			public const int CY_IssuerCodeMaxLength = 4;

			public const string CY_BillOfLading = "CY_BillOfLading";
			public const int CY_BillOfLadingMaxLength = 12;

			public const string CY_CodeDescription = "CY_CodeDescription";
		}

		public new CusInBondMoveHeader Parent
		{
			get { return (CusInBondMoveHeader)base.Parent; }
			set { base.Parent = value; }
		}

		#region New Properties

		#region CY_IssuerCode

		[CargoWise.ComponentModel.MaxLength(Schema.CY_IssuerCodeMaxLength)]
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

		#endregion

		#region CY_BillOfLading

		[CargoWise.ComponentModel.MaxLength(Schema.CY_BillOfLadingMaxLength)]
		public ZString CY_BillOfLading
		{
			get
			{
				if (!billNumberLoaded)
				{
					billNumberLoaded = true;
					fBillNumber = CY_Data.SubstringSafe(Schema.CY_IssuerCodeMaxLength, Schema.CY_BillOfLadingMaxLength);
				}
				return fBillNumber;
			}
			set
			{
				SetNonPersistentPropertyValue(CY_BillOfLadingInfo, ref fBillNumber, value);
				PersistToCY_Data();
			}
		}
		ZString fBillNumber;
		bool billNumberLoaded;

		public ZPropertyInfo CY_BillOfLadingInfo
		{
			get { return GetZPropertyInfo(Schema.CY_BillOfLading); }
		}

		#endregion

		#region CY_CodeDescription

		public ZString CY_CodeDescription
		{
			get { return CodeList.GetDescriptionFromCode(CY_Code) ?? ZString.Empty; }
		}

		public ZPropertyInfo CY_CodeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CY_CodeDescription); }
		}

		CodeDescriptionPairList CodeList
		{
			get { return codeList ?? (codeList = Lookups.CY_CodeList); }
		}
		CodeDescriptionPairList codeList;

		#endregion

		#endregion

		#region Implementation

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(CusInBondMoveHeader)); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = IssuerAndBillOfLadingTypeCode;
		}
		public const string IssuerAndBillOfLadingTypeCode = "BOL";

		protected override Customs.Business.CusCodeDataValidation GetNewValidation()
		{
			return new IssuerAndBillOfLadingValidation(this);
		}

		protected override Customs.Business.CusCodeDataLookups GetNewLookups()
		{
			return new IssuerAndBillOfLadingLookups(this);
		}

		void PersistToCY_Data()
		{
			CY_Data = CY_IssuerCode.PadRight(Schema.CY_IssuerCodeMaxLength, ' ') + CY_BillOfLading;
		}

		#endregion
	}
}
