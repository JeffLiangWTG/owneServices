using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business
{
	public class SecondaryNotifyPartyCollection : ActiveBusinessObjectCollection<SecondaryNotifyParty>
	{
		public SecondaryNotifyPartyCollection(CusInBondBill master)
			: base(master.Factory, master, new ZQuery(CusCodeDataSchema.CY_Type, SecondaryNotifyParty.SNPType), CusCodeDataSchema.CY_ParentID)
		{
		}

		public SecondaryNotifyParty[] GetNonEmptySNPInSortOrder()
		{
			var secondNotifyParties = new List<SecondaryNotifyParty>(Find(new FindPredicate(snp => !snp.IsDeleted && !snp.CY_Data.IsEmpty)));
			secondNotifyParties.Sort(new System.Comparison<SecondaryNotifyParty>((x, y) =>
				{
					var result = 0;
					if (x != null || y != null)
					{
						if (x != null && y == null)
						{
							result = -1;
						}
						else if (x == null && y != null)
						{
							result = 1;
						}
						else
						{
							result = x.CY_Order.CompareTo(y.CY_Order);
							result = result == 0 ? x.CY_Data.CompareTo(y.CY_Data) : result;
							result = result == 0 ? x.PK.CompareTo(y.PK) : result;
						}
					}
					return result;
				}));
			return secondNotifyParties.ToArray();
		}

		public SecondaryNotifyParty AddNewIfNotExist(ZString value)
		{
			var result = this.FirstOrDefault(x => x.CY_Data == value);
			if (result == null)
			{
				result = AddNew();
				result.CY_Data = value;
			}
			return result;
		}

		protected override void OnLoadedIntoCollectionCore(SecondaryNotifyParty loadedObject)
		{
			base.OnLoadedIntoCollectionCore(loadedObject);
			loadedObject.Parent = Relationship.Master;
		}

		protected override void SetRelationshipDefaultsForElementCore(SecondaryNotifyParty newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.Parent = Relationship.Master;
		}

		protected override void SetDefaultsForNewElementCore(SecondaryNotifyParty newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.CY_Order = (ZShort)Count + 1;
		}
	}
}
