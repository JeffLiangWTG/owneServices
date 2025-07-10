using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NO.Manifest.Business;

public sealed class PreviousDocument : CusSupportingInfo
{
	public PreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : AutoCusSupportingInfo.Schema
	{
		public const string DocumentDescription = "DocumentDescription";
	}

	public new PreviousDocumentValidation Validation => (PreviousDocumentValidation)base.Validation;

	protected override CusSupportingInfoValidation GetNewValidation() => new PreviousDocumentValidation(this);

	public new PreviousDocumentLookups Lookups => (PreviousDocumentLookups)base.Lookups;

	protected override CusSupportingInfoLookups GetNewLookups() => new PreviousDocumentLookups(this);

	[MaxLength(4)]
	[ResourceStringData("NO.PreviousDocument.CSI_Code", Caption = "Type")]
	[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.CodeList))]
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

	[ResourceStringData("NO.PreviousDocument.DocumentDescription", Caption = "Description")]
	public ZString DocumentDescription => CachedValueHelper.GetValue(ref documentDescriptionCache, GetPreviousDocumentDescription);
	CachedValue<ZString> documentDescriptionCache;

	public ZPropertyInfo DocumentDescriptionInfo => GetZPropertyInfo(Schema.DocumentDescription);

	ZString GetPreviousDocumentDescription()
	{
		if (!CSI_Code.IsEmpty && Lookups.CodeList is BusinessObjectCollection codeListObjectCollection)
		{
			var query = codeListObjectCollection.CompleteFilter;
			query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, CSI_Code);
			return Factory.LoadTop1<ZZRefCusCodeListCombined>(query)?.ZZD_Description ?? ZString.Empty;
		}

		return ZString.Empty;
	}

	[MaxLength(70)]
	[ResourceStringData("NO.PreviousDocument.CSI_ReferenceNumber", Caption = "Number")]
	public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }
}
