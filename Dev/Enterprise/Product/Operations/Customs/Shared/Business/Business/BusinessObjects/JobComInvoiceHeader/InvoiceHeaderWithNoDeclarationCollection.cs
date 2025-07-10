using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class InvoiceHeaderWithNoDeclarationCollection : ActiveBusinessObjectCollection<BaseJobComInvoiceHeader>
	{
		public InvoiceHeaderWithNoDeclarationCollection(BusinessObjectFactory factory) : base(factory)
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(AttachedToDeclarationFilterName, "Property", new ZString(NotAttachedToDeclarationCode), false));
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(JoinCondition.And, JobComInvoiceHeaderSchema.JZ_JE, SQLComparisonOperator.Equal, null);
			return result;
		}

		protected override void SetDefaultsForNewElementCore(BaseJobComInvoiceHeader newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (!((BaseJobComInvoiceHeader)selectedBusinessObject).JZ_JE.IsEmpty)
			{
				errors.Add(Res.GetString("E2726255-936A-4CF2-9051-456E979D7B0F", "The selected invoice is attached to a declaration job. Please select an unattached invoice."));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter Name, should not be translated")]
		public const string AttachedToDeclarationFilterName = "Attached to a Declaration";

		public const string NotAttachedToDeclarationCode = "NO";
	}
}
