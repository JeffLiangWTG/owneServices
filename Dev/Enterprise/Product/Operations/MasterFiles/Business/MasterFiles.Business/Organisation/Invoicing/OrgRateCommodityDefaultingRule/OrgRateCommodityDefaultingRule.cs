using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgRateCommodityDefaultingRule : AutoOrgRateCommodityDefaultingRule
	{
		public OrgRateCommodityDefaultingRule(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return Header != null &&
				(!Header.SecurityProvider.HasModifyDetailsRatingAndTariffsSecurity ||
				!Header.SecurityProvider.HasModifyDetailsSecurity ||
				MetaData.GetReadOnlyExcludingMethodProvider(this, property));
		}

		#endregion

		#region properties

		[List("Lookups.ContainerModeList")]
		public override ZString ORC_ContainerMode { get => base.ORC_ContainerMode; set => base.ORC_ContainerMode = value; }

		[List("Lookups.TransportModeList")]
		public override ZString ORC_TransportMode { get => base.ORC_TransportMode; set => base.ORC_TransportMode = value; }

		[List("Lookups.DirectionList")]
		public override ZString ORC_Direction { get => base.ORC_Direction; set => base.ORC_Direction = value; }

		[List("Lookups.Locations")]
		public override ZString ORC_Origin { get => base.ORC_Origin; set => base.ORC_Origin = value; }

		[List("Lookups.Locations")]
		public override ZString ORC_Destination { get => base.ORC_Destination; set => base.ORC_Destination = value; }

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ORC_ContainerMode = Constants.ContainerModes.Loose;
			ORC_TransportMode = Constants.TransportModes.Air;
			ORC_Direction = Constants.FreightShipmentDirection.Code.Export;
		}

#endif
	}
}
