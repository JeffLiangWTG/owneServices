using CargoWise.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Universal
{
	internal class TranslatableZZBusinessObjectFetchStrategy<T>
		: EnterpriseBusinessObjectFetchStrategy where T : EnterpriseBusinessObject, ITranslatableZZBusinessObject
	{
		public TranslatableZZBusinessObjectFetchStrategy(T businessObject)
			: base(businessObject)
		{
			Argument.NotNull(businessObject, nameof(businessObject));
			parent = businessObject;
		}

		readonly T parent;

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			if (parent.LanguageTableSchema != null && parent.LanguageTableType != null)
			{
				Factory.AddFetchHint(parent.LanguageTableType, TranslationHelper.GetWorkingLanguageQuery(parent, parent.LanguageTableSchema));
			}
		}
	}
}
