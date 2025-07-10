using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface ICusEquipmentCollection<out T> : IActiveBusinessObjectCollection<T>
	where T : CusEquipment
	{
		new T this[int index] { get; }
	}

	public class CusEquipmentCollection<T> : ActiveBusinessObjectCollection<T>, ICusEquipmentCollection<T>
		where T : CusEquipment
	{
		public CusEquipmentCollection(BaseJobDeclaration parent)
			: base(parent.Factory, parent, new ZQuery(), CusEquipmentSchema.CEQ_JE_Declaration)
		{
		}
	}
}
