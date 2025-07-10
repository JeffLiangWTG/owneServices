using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public interface IRelatedDeclarationGenPivotCollection : IBusinessObjectCollection, IBusinessObjectCollectionInternals, IBusinessObjectFilterFactory, IEnumerable<BusinessObject>
	{
		bool Contains(BaseJobDeclaration declaration);
		void AddPivotForDeclaration(BaseJobDeclaration declaration);
		void DeletePivotFor(BaseJobDeclaration declaration);
		void RemoveAndDeleteAll();
	}

	#region InvoiceRelatedDeclarationGenPivotCollection

	public class InvoiceRelatedDeclarationGenPivotCollection : CustomsGenPivotCollection<InvoiceRelatedDeclarationGenPivot, BaseJobComInvoiceHeader, BaseJobDeclaration>, IRelatedDeclarationGenPivotCollection
	{
		public InvoiceRelatedDeclarationGenPivotCollection(BaseJobComInvoiceHeader master)
			: base(master)
		{
		}

		protected override string RelationType
		{
			get { return GenPivotTypeDecider.Types.InvoiceRelatedDeclarationGenPivot; }
		}

		#region IRelatedDeclarationGenPivotCollection Members

		public void AddPivotForDeclaration(BaseJobDeclaration declaration)
		{
			base.AddPivotFor(declaration);
		}

		#endregion
	}

	#endregion

	#region GroupRelatedDeclarationGenPivotCollection

	public class GroupRelatedDeclarationGenPivotCollection : CustomsGenPivotCollection<GroupRelatedDeclarationGenPivot, BaseJobComInvoiceGroupHeader, BaseJobDeclaration>, IRelatedDeclarationGenPivotCollection
	{
		public GroupRelatedDeclarationGenPivotCollection(BaseJobComInvoiceGroupHeader master)
			: base(master)
		{
		}

		protected override string RelationType
		{
			get { return GenPivotTypeDecider.Types.GroupRelatedDeclarationGenPivot; }
		}

		#region IRelatedDeclarationGenPivotCollection Members

		public void AddPivotForDeclaration(BaseJobDeclaration declaration)
		{
			base.AddPivotFor(declaration);
		}

		#endregion
	}

	#endregion
}
