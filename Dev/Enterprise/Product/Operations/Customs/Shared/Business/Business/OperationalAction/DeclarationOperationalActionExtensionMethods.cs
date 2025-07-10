using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Business
{
	public static class DeclarationOperationalActionExtensionMethods
	{
		public static LogControllerLink GetDeclarationIdLink(this BaseJobDeclaration declaration)
		{
			var controllerID = declaration.JE_JS.IsValid ? ControllerIDs.JobShipment : ControllerIDs.Customs.JobDeclaration;
			var pK = declaration.JE_JS.IsValid ? declaration.JE_JS : declaration.PK;

			return new LogControllerLink(declaration.JE_DeclarationReference, controllerID, pK);
		}
	}
}
