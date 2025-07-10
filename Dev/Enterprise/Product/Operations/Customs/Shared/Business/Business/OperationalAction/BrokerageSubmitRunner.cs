using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.Business
{
	public class BrokerageSubmitRunner
	{
		public IOperationalActionSectionLog Log { get; set; }

		public void Execute(BaseJobDeclaration declaration)
		{
			Execute(declaration, new BusinessObjectFactory());
		}

		public void Execute(BaseJobDeclaration declaration, BusinessObjectFactory factory)
		{
			Argument.NotNull(declaration, "declaration");

			var declaration1 = factory.Load<BaseJobDeclaration>(declaration.PK);
			if (declaration1 == null || declaration1.IsDeleted)
			{
				Notify(declaration, OperationalActionLogErrorLevel.Warning, DeclarationNotExistsText);
			}
			else
			{
				var integrationProvider = GetCusIntegrationProvider(declaration1);
				if (integrationProvider == null)
				{
					Notify(declaration1, OperationalActionLogErrorLevel.Warning, DeclarationNotSubmittedText);
				}
				else
				{
					Notify(declaration1, OperationalActionLogErrorLevel.Informational, integrationProvider.Execute(declaration));
				}
			}
		}

		void Notify(BaseJobDeclaration declaration, OperationalActionLogErrorLevel level, string message)
		{
			if (Log != null)
			{
				Log.NotifyFormat(level, "{0}: {1}", declaration.GetDeclarationIdLink(), message);
			}
		}

#if DEBUG
		protected virtual
#endif
		ICusIntegration GetCusIntegrationProvider(BaseJobDeclaration declaration)
		{
			return CusIntegrationTypeDecider.CusIntegration(declaration);
		}

		#region Static Strings

		static ZString DeclarationNotExistsText
		{
			get { return Res.GetString("ed5b442d-5895-4d76-823b-10860ea0f129", "This Declaration does not exist or has already been deleted."); }
		}

		static ZString DeclarationNotSubmittedText
		{
			get { return ResString.GetMultilingualString("5eedbe78-730d-481c-af82-c7afa8622b5b", "Not Submitted."); }
		}

		#endregion
	}
}
