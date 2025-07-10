using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public interface IPackingInformation
	{
		HouseBillContainer HouseBillContainer { get; set; }
		ZInt PackQty { get; set; }
		ZString PackType { get; set; }
		ZString MarksAndNumbers { get; set; }
		bool SupportMarksAndNumbers { get; }
	}

	public struct HouseBillContainer
	{
		public HouseBillContainer(Bill houseBill, BaseCusContainer container)
		{
			this.HouseBill = houseBill;
			this.Container = container;
		}

		public readonly Bill HouseBill;
		public readonly BaseCusContainer Container;
	}

	public interface IOneToOnePackingInformation : IPackingInformation
	{
		UNDGDataItemCollection UNDGs { get; }
	}

	public interface IPackingInformationCollection
	{
		IPackingInformation AddNew();
		IPackingInformation GetMatchingElement(Bill bill, BaseCusContainer container);
		IPackingInformation GetElementWithNoContainer();
		IPackingInformation GetElementWithNoHouseBill();
		void RemoveAndDeleteAll();
		int Count { get; }
		IPackingInformation GetElement(int index);
		void SetReadOnlyIncludingChildren(bool isReadOnly);
	}
}
