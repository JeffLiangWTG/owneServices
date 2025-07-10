using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business;

public sealed class PreviousDocumentMaster(BusinessObjectFactory factory, IPreviousDocumentsProvider parent) : NonPersistentBusinessObject(Argument.NotNull(factory, nameof(factory)))
{
	internal IPreviousDocumentsProvider Parent { get; } = Argument.NotNull(parent, nameof(parent));

	public ICusSupportingInfoCollection<PreviousDocument> PreviousDocuments { get; } = Argument.NotNull(parent.PreviousDocuments, nameof(IPreviousDocumentsProvider.PreviousDocuments));

	[List($"{nameof(Lookups)}.{nameof(PreviousDocumentMasterLookups.ProcedureList)}")]
	[MaxLength(PreviousDocument.Schema.CSI_ProcedureMaxLength)]
	[ResourceStringData("NO.PreviousDocumentMaster|CSI_Procedure", Caption = "Previous Procedure")]
	public ZString CSI_Procedure
	{
		get => MasterDocument?.CSI_Procedure ?? ZString.Empty;
		set
		{
			if (CSI_Procedure != value)
			{
				CheckMaximumLength(CSI_ProcedureInfo, value);
				PreviousDocuments.RemoveAndDeleteAll();
				masterDocument = null;

				if (!value.IsEmpty)
				{
					var document = PreviousDocuments.AddNew();
					document.CSI_Procedure = value;
					masterDocument = document;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateCSI_Procedure();
				}

				CSI_ProcedureInfo.RefreshBinding();

				if (PreviousDocuments is BusinessObjectCollection businessObjectCollection)
				{
					businessObjectCollection.RefreshBinding();
				}
			}
		}
	}

	public ZPropertyInfo CSI_ProcedureInfo => GetZPropertyInfo(PreviousDocument.Schema.CSI_Procedure);

	public PreviousDocumentMasterLookups Lookups => lookups ??= new (this);
	PreviousDocumentMasterLookups lookups;

	public PreviousDocumentMasterValidation Validation => new (this);

	PreviousDocument MasterDocument => masterDocument ??= PreviousDocuments.FirstOrDefault();
	PreviousDocument masterDocument;
}
