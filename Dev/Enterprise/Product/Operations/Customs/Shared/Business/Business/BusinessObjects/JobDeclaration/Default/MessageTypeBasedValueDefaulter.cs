using CargoWise.Common;

namespace Enterprise.Customs.Business
{
	public class MessageTypeBasedValueDefaulter
	{
		public MessageTypeBasedValueDefaulter(BaseJobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, "BaseJobDeclaration declaration");
		}

		public void DefaultValuesFromLocalParty()
		{
			declaration.DefaultValuesFromLocalPartyWhenEntered(declaration.LocalParty);
		}

		public void DefaultAll()
		{
			DefaultAllCore();
		}

		protected virtual void DefaultAllCore()
		{
			DefaultValuesFromLocalParty();
		}

		protected readonly BaseJobDeclaration declaration;
	}
}
