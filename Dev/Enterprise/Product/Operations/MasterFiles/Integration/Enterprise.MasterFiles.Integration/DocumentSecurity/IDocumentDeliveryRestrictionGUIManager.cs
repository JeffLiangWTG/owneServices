using System;

namespace Enterprise.MasterFiles.Integration
{
	public interface IDocumentDeliveryRestrictionGUIManager : IDisposable
	{
		void Initialise(ICreditControlledBusinessObject creditControlledBusinessObject);
		void SetDescription(string description);
		void SetCaption(string caption);
		bool IsAuthenticationDeclined { get; }
	}
}
