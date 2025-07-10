using System;

namespace Enterprise.Customs.PL.Business.Declaration;

public class DeclarationValueChangedAnnouncer : Customs.Business.DeclarationValueChangedAnnouncer
{
	public DeclarationValueChangedAnnouncer(JobDeclaration declaration)
		: base(declaration)
	{
		Declaration.JE_LocationOfGoodsInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
		Declaration.JE_LocationQualifierInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
		Declaration.JE_OfficeOfEntryExitInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
	}

	JobDeclaration Declaration
	{
		get { return (JobDeclaration)base.declaration; }
	}

	public override void Dispose()
	{
		base.Dispose();
		Declaration.JE_LocationOfGoodsInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
		Declaration.JE_LocationQualifierInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
		Declaration.JE_OfficeOfEntryExitInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
	}
}
