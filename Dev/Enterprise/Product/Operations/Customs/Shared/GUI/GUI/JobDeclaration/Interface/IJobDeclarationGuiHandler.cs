using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI
{
	public interface IJobDeclarationGuiHandler
	{
		BaseJobDeclaration JobDeclaration { get; set; }
		void InitializeGridLayout();
	}
}
