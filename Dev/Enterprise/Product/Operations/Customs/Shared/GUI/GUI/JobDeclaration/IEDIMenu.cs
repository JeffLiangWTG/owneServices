using System;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI
{
	public interface IEDIMenu : IDisposable
	{
		BaseJobDeclaration Declaration { get; set; }
	}
}
