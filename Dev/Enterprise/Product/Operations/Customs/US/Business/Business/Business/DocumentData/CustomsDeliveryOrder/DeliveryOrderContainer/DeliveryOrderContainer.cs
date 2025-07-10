using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class DeliveryOrderContainer : AutoDeliveryOrderContainer
	{
		public DeliveryOrderContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[BusinessObjectTestExclude]
		public override ZString B7_ParentTableCode
		{
			get { return base.B7_ParentTableCode; }
			set
			{
				if (!value.IsEmpty && value != CusAddInfoSchema.Constants.Prefix)
				{
					throw new NotSupportedException("Setting DeliveryOrderContainer.B7_ParentTableCode is not supported.");
				}
				base.B7_ParentTableCode = value;
			}
		}

		public new DeliveryOrderHeader Parent
		{
			get
			{
				if (B7_Type != CusAddInfoTypeAttribute.Codes.USDeliveryOrderContainer)
				{
					fParent = null;
				}
				else if ((fParent == null) || (fParent.PK != B7_ParentID))
				{
					fParent = base.Factory.Load<DeliveryOrderHeader>(B7_ParentID);
				}
				return fParent;
			}
		}

		public ZString WeightAndUQ
		{
			get { return US_Weight > 0 ? US_Weight.ToStringTrimZeros() + " " + US_WeightUQ : ""; }
		}

		[MeasureUnit(Schema.US_WeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal US_Weight
		{
			get { return base.US_Weight; }
			set { base.US_Weight = value; }
		}

		#region Implementation
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			base.B7_ParentTableCode = CusAddInfoSchema.Constants.Prefix;
			B7_Type = CusAddInfoTypeAttribute.Codes.USDeliveryOrderContainer;
		}

		DeliveryOrderHeader fParent;
		#endregion
	}
}
