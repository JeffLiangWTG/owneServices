using Enterprise.Customs.GUI;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.GUI;

public class JobDeclarationForm : EU.GUI.JobDeclarationForm
{
	public JobDeclarationForm()
	{
	}

	public JobDeclarationForm(JobDeclaration declaration) : base(declaration)
	{
	}

	protected override IEDIMenu GetNewTopLevelMenuCore() => new EDIMenu();

	protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl() => new CustomsBrokerageUserControl();
}
