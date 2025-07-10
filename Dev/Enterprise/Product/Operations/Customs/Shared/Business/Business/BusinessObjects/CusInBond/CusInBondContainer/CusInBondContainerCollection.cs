using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface ICusInBondContainerCollection : IActiveBusinessObjectCollection
	{
		BusinessObject this[ZString containerNum] { get; }

		new BusinessObject this[int index] { get; }
	}

	public abstract class CusInBondContainerCollection<T> : ActiveBusinessObjectCollection<T>, ICusInBondContainerCollection
		where T : CusInBondContainer
	{
		protected CusInBondContainerCollection(BusinessObject master, ZQuery filter)
			: base(master.Factory, master, filter, CusInBondContainerSchema.BC_ParentID)
		{
		}

		protected CusInBondContainerCollection(BusinessObject master)
			: base(master.Factory, master, new ZQuery(), CusInBondContainerSchema.BC_ParentID)
		{
		}

		public T this[ZString containerNum]
		{
			get
			{
				T result = null;
				foreach (T container in this)
				{
					if (!container.IsDeleted && container.BC_ContainerNum == containerNum)
					{
						result = container;
						break;
					}
				}
				return result;
			}
		}

		BusinessObject ICusInBondContainerCollection.this[ZString containerNum]
		{
			get { return this[containerNum]; }
		}

		BusinessObject ICusInBondContainerCollection.this[int index] => this[index];
	}
}
