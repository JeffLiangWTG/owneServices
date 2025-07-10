using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.Business.Declaration;

public class SupportingDocument : EU.Business.Declaration.MultiLineAddInfos.SupportingDocument
{
	public SupportingDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override CusSupportingInfoLookups GetNewLookups() => new SupportingDocumentLookups(this);

	public new SupportingDocumentLookups Lookups => (SupportingDocumentLookups)base.Lookups;

	protected override CusSupportingInfoValidation GetNewValidation() => new SupportingDocumentValidation(this);

	public new SupportingDocumentValidation Validation => (SupportingDocumentValidation)base.Validation;

	public new class Schema : EU.Business.Declaration.MultiLineAddInfos.SupportingDocument.Schema
	{
		public new const int CSI_ReferenceNumber2MaxLength = 70;
	}

	[ResourceStringData("NLSupportingDocument|IssuingAuthority", ShortCaption = "Authority", Caption = "Issuing Authority")]
	[MaxLength(Schema.CSI_ReferenceNumber2MaxLength)]
	public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

	public override bool IsEffectiveSupportingDocumentsForLine => !IsHeaderOnly;
}
