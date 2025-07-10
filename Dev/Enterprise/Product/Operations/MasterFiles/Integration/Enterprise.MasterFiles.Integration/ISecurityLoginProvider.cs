using System;

namespace Enterprise.MasterFiles.Integration
{
	public interface ISecurityLoginProvider
	{
		void ShowDocumentLoginForDocuments(EventArgs args);
	}
}
