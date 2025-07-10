using System;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public interface IAlertHyperLinkForm : IDisposable
	{
		ZDialogResult ShowDialogAndGetResult();
	}
}
