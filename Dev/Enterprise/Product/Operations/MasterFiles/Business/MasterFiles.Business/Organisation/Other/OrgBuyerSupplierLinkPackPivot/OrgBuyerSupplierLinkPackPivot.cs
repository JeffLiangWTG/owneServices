using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgBuyerSupplierLinkPackPivot : AutoOrgBuyerSupplierLinkPackPivot
	{
		public OrgBuyerSupplierLinkPackPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ReadOnlySecurity

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		[List("Lookups.WeightUnitList")]
		public override ZString Q0_UnitOfWeight
		{
			get { return base.Q0_UnitOfWeight; }
			set { base.Q0_UnitOfWeight = value; }
		}

		[List("Lookups.LengthUnitList")]
		public override ZString Q0_UnitOfDimension
		{
			get { return base.Q0_UnitOfDimension; }
			set { base.Q0_UnitOfDimension = value; }
		}

		[List("Lookups.PackTypeList")]
		[RelatedBusinessObject("PackType")]
		public override ZGuid Q0_F3
		{
			get { return base.Q0_F3; }
			set { base.Q0_F3 = value; }
		}

		public RefPackType PackType
		{
			get { return Factory.Load<RefPackType>(Q0_F3); }
		}
	}
}
