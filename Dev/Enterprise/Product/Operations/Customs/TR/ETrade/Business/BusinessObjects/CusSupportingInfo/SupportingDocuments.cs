using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class SupportingDocuments : CusSupportingInfo
	{
		public SupportingDocuments(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const string DocumentDescription = "DocumentDescription";
		}

		public const string SupportingDocumentsType = "BIL";

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = SupportingDocumentsType;
			CSI_Status = SupportingDocumentStatusList.Codes.EXS;
		}

		protected override CusSupportingInfoValidation GetNewValidation() => new SupportingDocumentsValidation(this);
		public new SupportingDocumentsValidation Validation => (SupportingDocumentsValidation)base.Validation;

		public new SupportingDocumentsLookups Lookups => (SupportingDocumentsLookups)base.Lookups;
		protected override CusSupportingInfoLookups GetNewLookups() => new SupportingDocumentsLookups(this);

		[List(nameof(Lookups) + "." + nameof(SupportingDocumentsLookups.CodeList))]
		[MaxLength(4)]
		[ResourceStringData("SupportingDocuments.CSI_Code", Caption = "Code")]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_Code = value;
				if (oldValue != CSI_Code && !IsCopying)
				{
					documentDescriptionCache = null;
					DocumentDescriptionInfo.RefreshBinding();
				}
			}
		}

		[ResourceStringData("SupportingDocuments.DocumentDescription", Caption = "Description")]
		public ZString DocumentDescription => CachedValueHelper.GetValue(ref documentDescriptionCache, GetSupportingDocumentDescription);

		CachedValue<ZString> documentDescriptionCache;
		public ZPropertyInfo DocumentDescriptionInfo => GetZPropertyInfo(Schema.DocumentDescription);
		ZString GetSupportingDocumentDescription()
		{
			{
				var descrip = ZString.Empty;
				var code = CSI_Code;
				if (!CSI_Code.IsEmpty)
				{
					var query = ((BusinessObjectCollection)Lookups.CodeList).CompleteFilter;
					query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, code);
					descrip = Factory.LoadTop1<ZZRefCusCodeListCombined>(query)?.ZZD_Description ?? ZString.Empty;
				}
				return descrip;
			}
		}

		[ResourceStringData("SupportingDocuments.CSI_DateOfIssue", Caption = "Date")]
		public override ZDateTime CSI_DateOfIssue { get => base.CSI_DateOfIssue; set => base.CSI_DateOfIssue = value; }

		[MaxLength(100)]
		[ResourceStringData("SupportingDocuments.CSI_ReferenceNumber", Caption = "Reference No")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[MaxLength(3)]
		[ResourceStringData("SupportingDocuments.CSI_Status", Caption = "Exist?")]
		[List(nameof(Lookups) + "." + nameof(SupportingDocumentsLookups.StatusList))]
		public override ZString CSI_Status { get => base.CSI_Status; set => base.CSI_Status = value; }
	}
}
