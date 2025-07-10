using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry
{
	[XmlSerializerAssembly("Enterprise.Freight.Forwarding.Registry.XmlSerializers")]
	public class PreAllocationCheckCollection : RegistryBusinessObjectCollectionTemplate
	{
		#region Quick Access to Checks

		public PreAllocationCheck Weight
		{
			get { return this.Cast<PreAllocationCheck>().FirstOrDefault(check => check.Measure == PreAllocationCheck.Measures.Weight); }
		}

		public PreAllocationCheck Volume
		{
			get { return this.Cast<PreAllocationCheck>().FirstOrDefault(check => check.Measure == PreAllocationCheck.Measures.Volume); }
		}

		public PreAllocationCheck Chargeable
		{
			get { return this.Cast<PreAllocationCheck>().FirstOrDefault(check => check.Measure == PreAllocationCheck.Measures.Chargeable); }
		}

		public PreAllocationCheck ShipmentCount
		{
			get { return this.Cast<PreAllocationCheck>().FirstOrDefault(check => check.Measure == PreAllocationCheck.Measures.ShipmentCount); }
		}

		public PreAllocationCheck Dimensions
		{
			get { return this.Cast<PreAllocationCheck>().FirstOrDefault(check => check.Measure == PreAllocationCheck.Measures.Dimensions); }
		}

		#endregion

		#region Default Value

		public static PreAllocationCheckCollection GetDefault()
		{
			PreAllocationCheckCollection result = new PreAllocationCheckCollection();

			PreAllocationCheck check = result.AddNew();
			check.MeasureMultilingualString = ResString.GetMultilingualString("9500d0fd-0637-49ba-b3cb-d6a2308cf052", PreAllocationCheck.Measures.Weight);
			check.Measure = PreAllocationCheck.Measures.Weight;
			check.Action = PreAllocationCheck.Actions.None;

			check = result.AddNew();
			check.MeasureMultilingualString = ResString.GetMultilingualString("788993ce-8b9c-4303-884d-2d82636aab0f", PreAllocationCheck.Measures.Volume);
			check.Measure = PreAllocationCheck.Measures.Volume;
			check.Action = PreAllocationCheck.Actions.None;

			check = result.AddNew();
			check.MeasureMultilingualString = ResString.GetMultilingualString("2eba13eb-0361-481b-9a42-d2abfee83492", PreAllocationCheck.Measures.Chargeable);
			check.Measure = PreAllocationCheck.Measures.Chargeable;
			check.Action = PreAllocationCheck.Actions.None;

			check = result.AddNew();
			check.MeasureMultilingualString = ResString.GetMultilingualString("a14136ee-d3e3-4ebf-a14b-b309a10e90e9", PreAllocationCheck.Measures.ShipmentCount);
			check.Measure = PreAllocationCheck.Measures.ShipmentCount;
			check.Action = PreAllocationCheck.Actions.None;

			check = result.AddNew();
			check.MeasureMultilingualString = ResString.GetMultilingualString("97e21f7b-8944-427c-9482-9c75689f1e61", PreAllocationCheck.Measures.Dimensions);
			check.Measure = PreAllocationCheck.Measures.Dimensions;
			check.Action = PreAllocationCheck.Actions.None;
			check.Percentage = 100m;

			return result;
		}

		#endregion

		#region Implementation

		public new PreAllocationCheck AddNew()
		{
			return (PreAllocationCheck)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PreAllocationCheckCollection();
		}

		public new PreAllocationCheck this[int i]
		{
			get { return (PreAllocationCheck)base[i]; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PreAllocationCheck();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		#endregion
	}
}
