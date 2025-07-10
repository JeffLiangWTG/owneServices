using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Business
{
	public interface IFormPresenter
	{
		void ShowNew(ControllerID controllerId, BusinessObject bo);

		void ShowEdit(ControllerID controllerId, BusinessObject bo);

		void ShowView(ControllerID controllerId, BusinessObject bo);

		void ShowError(string message, string caption);
	}
}
