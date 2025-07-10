using Enterprise.Customs.GUI;
using Enterprise.Customs.PL.Business.Declaration;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.PL.GUI;

public partial class JobDeclarationForm : EU.GUI.JobDeclarationForm
{
	public JobDeclarationForm()
		: base()
	{
	}

	public JobDeclarationForm(JobDeclaration declaration)
		: base(declaration)
	{
	}

	protected override IEDIMenu GetNewTopLevelMenuCore() => new EDIMenu();

	protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl() => new CustomsBrokerageUserControl();
}
