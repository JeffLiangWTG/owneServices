using System.Diagnostics;

namespace Enterprise.MasterFiles.Business
{
	public interface IHaveConstructorStackTrace
	{
		StackTrace ConstructorStackTrace { get; set; }
	}
}
