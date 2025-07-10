using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG
{
	public class SGCPCCollection : ActiveBusinessObjectCollection<SGCPC>
	{
		public SGCPCCollection(JobDeclaration master)
			: base(master.Factory, master, GetFilter(master.PK), CusAddInfoSchema.B7_ParentID)
		{
		}

		public JobDeclaration Master
		{
			get { return (JobDeclaration)Relationship.Master; }
		}

		static ZQuery GetFilter(ZGuid jobDeclarationPK)
		{
			ZQuery query = new ZQuery(CusAddInfoSchema.B7_ParentID, jobDeclarationPK);
			query.AddToFilter(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.SGCustomsProcedureCode);
			return query;
		}

		protected override bool AllowNew
		{
			get
			{
				if (Count < 5)
				{
					return base.AllowNew;
				}
				else
				{
					return false;
				}
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "Customs Procedure Code"; }
		}

		#region Implementation

		protected override void SetRelationshipDefaultsForElementCore(SGCPC newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.B7_Type = CusAddInfoTypeAttribute.Codes.SGCustomsProcedureCode;
		}

		#endregion
	}
}
