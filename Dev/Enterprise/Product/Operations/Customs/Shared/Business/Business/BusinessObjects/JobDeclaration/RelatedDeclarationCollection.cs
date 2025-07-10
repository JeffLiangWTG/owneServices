using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class RelatedDeclarationCollection : ActiveBusinessObjectCollection<BaseJobDeclaration>
	{
		RelatedDeclarationCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		/// <summary>
		/// Establishes a RelatedDeclarationCollection where objects are joined by GenPivot using XX_Relation1ID and XX_Relation2ID
		/// </summary>
		/// <param name="master"></param>
		RelatedDeclarationCollection(BaseJobDeclaration master)
			: base(master, typeof(GenPivot), new ZQuery(), GenPivotSchema.XX_Relation1ID, GenPivotSchema.XX_Relation2ID)
		{
		}

		public static RelatedDeclarationCollection GetPivotRelatedCollection(BaseJobDeclaration parent)
		{
			return new RelatedDeclarationCollection(parent);
		}

		public static RelatedDeclarationCollection GetLooselyRelatedCollection(BaseJobDeclaration parent, ZQuery query)
		{
			return new RelatedDeclarationCollection(parent.Factory, query);
		}

		internal void Add(BaseJobDeclaration newDeclaration, string relationshipType)
		{
			base.Add(newDeclaration);
			if (relationshipType != null)
			{
				if (Relationship.Master is BaseJobDeclaration master && !master.RelatedDeclarationTypes.Contains(relationshipType))
				{
					ErrorReporter.ReportOnce("Invalid relationship type: " + relationshipType);
				}
				var pivot = (GenPivot)((ManyToManyRelationship)Relationship).GetPivotObject(newDeclaration);
				pivot.XX_RelationType = relationshipType;
			}
		}
	}
}
