using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class UNDGAttributeZZCollection : ActiveBusinessObjectCollection<UNDGAttributeZZ>
	{
		public UNDGAttributeZZCollection(BusinessObjectFactory factory, IUNDGAttributeParent undgAttributeParent, string type)
			: base(factory, undgAttributeParent as BusinessObject, GetRelationshipQuery(undgAttributeParent, type), UNDGAttributeZZSchema.DAZ_ParentPK)
		{
			this.undgAttributeParent = undgAttributeParent;
			this.type = type;

			if (!Typeless)
			{
				undgAttributeParent.DetailsLanguageInfo.ValueChanged += delegate
				{
					RefreshForLanguage();
				};

				RefreshForLanguage();
			}
		}

		readonly IUNDGAttributeParent undgAttributeParent;
		readonly string type;

		static ZQuery GetRelationshipQuery(IUNDGAttributeParent refSubstance, string type)
		{
			var typeQuery = new ZQuery(UNDGAttributeZZSchema.DAZ_Type, type);
			var parentQuery = new ZQuery(UNDGAttributeZZSchema.DAZ_ParentCode, refSubstance.TableCode);

			return typeQuery.AddToFilter(parentQuery);
		}

		#region Implementation

		protected override void SetDefaultsForNewElementCore(UNDGAttributeZZ newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.DAZ_Type = type;
			newElement.DAZ_ParentCode = undgAttributeParent.TableCode;

			if (!undgAttributeParent.DetailsLanguage.IsEmpty)
			{
				newElement.DAZ_Language = undgAttributeParent.DetailsLanguage;
			}
		}

		bool Typeless => string.IsNullOrEmpty(type);

		void RefreshForLanguage()
		{
			if (!undgAttributeParent.DetailsLanguage.IsEmpty && !Typeless)
			{
				var languageQuery = new ZQuery(UNDGAttributeZZSchema.DAZ_Language, undgAttributeParent.DetailsLanguage);
				languageQuery.AddToFilter(JoinCondition.Or, UNDGAttributeZZSchema.DAZ_Language, ZString.Empty);

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
