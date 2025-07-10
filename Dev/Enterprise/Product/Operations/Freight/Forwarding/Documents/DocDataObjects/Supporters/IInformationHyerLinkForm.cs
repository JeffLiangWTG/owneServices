using System;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public interface IInformationHyerLinkForm : IDisposable
	{
		ZDialogResult ShowDialogAndGetResult();
	}
}
