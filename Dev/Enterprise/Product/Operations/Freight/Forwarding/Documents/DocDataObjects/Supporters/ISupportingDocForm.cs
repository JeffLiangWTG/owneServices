using System;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Supporters
{
	public interface ISupportingDocForm : IDisposable
	{
		ZDialogResult ShowDialogAndGetResult();
	}
}
