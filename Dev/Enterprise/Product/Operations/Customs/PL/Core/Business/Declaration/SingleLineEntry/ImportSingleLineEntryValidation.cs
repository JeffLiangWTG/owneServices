using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Declaration;

public sealed class ImportSingleLineEntryValidation : SingleLineEntryValidation
{
	public ImportSingleLineEntryValidation(AutoSingleLineEntry parent) : base(parent)
	{
	}

	public new ImportSingleLineEntry Parent
	{
		[System.Diagnostics.DebuggerStepThrough]
		get { return (ImportSingleLineEntry)base.Parent; }
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateGoodsOrigin();
		ValidatePreviousDocument();
	}

	public void ValidateGoodsOrigin()
	{
		ValidateCalculatedProperty(Parent.GoodsOriginInfo);
	}
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Suppressing as this method is called using reflection in the test TestCheckGoodsOrigin.")]
	void CheckGoodsOrigin()
	{
		ListValidation.MessageErrorIfInvalidCode(Parent.GoodsOriginInfo);
	}

	public void ValidatePreviousDocument()
	{
		ValidateCalculatedProperty(Parent.PreviousDocumentInfo);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Suppressing this as this method is called using reflection in the test TestCheckPreviousDocument.")]
	void CheckPreviousDocument()
	{
		ListValidation.MessageErrorIfInvalidCode(Parent.PreviousDocumentInfo);
	}
}
