using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public abstract class GenericCusUnderbondCollection<T> : DependentBusinessObjectCollection<T, BusinessObject> where T : CusUnderbond
	{
		protected GenericCusUnderbondCollection(ICusUnderbondDependentCollectionParent master)
			: base(master as BusinessObject, master.Factory)
		{
		}

		public T FindUnderbond(ZString originID, ZString destinationID)
		{
			foreach (T underbond in this)
			{
				if (underbond.C4_OriginPremiseID == originID && underbond.C4_DestinationPremiseID == destinationID)
				{
					return underbond;
				}
			}
			return null;
		}

		public bool AreAnyUnderbondsWaitingForAResponse
		{
			get
			{
				bool result = false;
				foreach (T underbond in this)
				{
					result |= underbond.Messages.IsWaitingForAResponse;
					if (result)
					{
						break;
					}
				}
				return result;
			}
		}

		#region Implementation

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			((T)child).LinkedObject = (ICusUnderbondDependentCollectionParent)Master;
			base.SetCollectionRelationships(child);
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusUnderbondSchema.C4_ParentID; }
		}

		#endregion
	}

	public abstract class CusUnderbondCollection : GenericCusUnderbondCollection<CusUnderbond>
	{
		protected CusUnderbondCollection(ICusUnderbondDependentCollectionParent master)
			: base(master)
		{
		}
	}
}
