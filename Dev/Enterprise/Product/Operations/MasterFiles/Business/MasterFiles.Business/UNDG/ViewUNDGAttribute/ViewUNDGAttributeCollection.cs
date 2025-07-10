using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ViewUNDGAttributeCollection : ActiveBusinessObjectCollection<ViewUNDGAttribute>
	{
		public ViewUNDGAttributeCollection(UNDGSubstance substance, string type)
			: base(substance, string.IsNullOrEmpty(type) ? new ZQuery() : new ZQuery(ViewUNDGAttributeSchema.DA_Type, type))
		{
			this.Substance = substance;
			this.Type = type;

			if (!Typeless)
			{
				Substance.DetailsLanguageInfo.ValueChanged += delegate
				{
					RefreshForLanguage();
				};
			}
			RefreshForLanguage();
		}

		readonly UNDGSubstance Substance;
		readonly string Type;

		bool Typeless
		{
			get
			{
				return string.IsNullOrEmpty(Type);
			}
		}

		#region Implementation

		protected override void SetDefaultsForNewElementCore(ViewUNDGAttribute newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			if (!Typeless)
			{
				newElement.DA_Type = Type;
				if (!Substance.DetailsLanguage.IsEmpty)
				{
					newElement.DA_Language = Substance.DetailsLanguage;
				}
			}
		}

		void RefreshForLanguage()
		{
			if (!Substance.DetailsLanguage.IsEmpty && !Typeless)
			{
				ZQuery languageQuery = new ZQuery(ViewUNDGAttributeSchema.DA_Language, Substance.DetailsLanguage);
				languageQuery.AddToFilter(JoinCondition.Or, ViewUNDGAttributeSchema.DA_Language, ZString.Empty);
				AdditionalFilter = languageQuery;
			}
			else
			{
				AdditionalFilter = new ZQuery();
			}
		}

		#endregion
	}
}
