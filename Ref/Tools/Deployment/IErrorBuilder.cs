using System.Management.Automation;

namespace CargoWise.RefDbRepo.Deployment
{
	public interface IErrorBuilder
	{
		string Build();
		void Append(ErrorRecord errorRecord);
	}
}
