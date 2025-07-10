using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class DeliveryOrderLine : AutoDeliveryOrderLine
	{
		public DeliveryOrderLine(BusinessObjectFactory factory, DataRow row)
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
					throw new NotSupportedException("Setting DeliveryOrderLine.B7_ParentTableCode is not supported.");
				}
				base.B7_ParentTableCode = value;
			}
		}

		public new DeliveryOrderHeader Parent
		{
			get
			{
				if (B7_Type != CusAddInfoTypeAttribute.Codes.USDeliveryOrderLine)
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

		public ZDecimal US_WeightInPounds
		{
			get
			{
				if (uS_WeightInPoundsCached == null)
				{
					uS_WeightInPoundsCached = new CachedProperty<ZDecimal>(Factory, delegate
						{
							return new ZDecimal(Core.Constants.Weight.Convert(US_WeightInKilograms, Core.Constants.Weight.Kilograms, Core.Constants.Weight.Pounds)).Round(0);
						});
				}
				return uS_WeightInPoundsCached.Value;
			}
		}
		CachedProperty<ZDecimal> uS_WeightInPoundsCached;

		#region Implementation
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			base.B7_ParentTableCode = CusAddInfoSchema.Constants.Prefix;
			B7_Type = CusAddInfoTypeAttribute.Codes.USDeliveryOrderLine;
		}

		DeliveryOrderHeader fParent;
		#endregion
	}
}
