using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	public class USFSISLot : AutoUSFSISLot, IFSISLot
	{
		public USFSISLot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new USInvoiceLineFSISLine Parent
		{
			get { return (USInvoiceLineFSISLine)base.Parent; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "Lot"; }
		}

		#region Override Properties

		[MeasureUnit(Schema.US_WeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal US_NetWeight
		{
			get { return base.US_NetWeight; }
			set { base.US_NetWeight = value; }
		}

		public override ZString US_ProductDescription
		{
			get => base.US_ProductDescription.IsEmpty ? Parent.US_CommercialDescription : base.US_ProductDescription;
			set => base.US_ProductDescription = value;
		}

		#endregion

		#region IFSISLot

		ZString IFSISLot.LotNumber
		{
			get { return US_LotNumber; }
		}

		ZDateTime IFSISLot.StartDate
		{
			get { return US_StartDate; }
		}

		ZDateTime IFSISLot.EndDate
		{
			get { return US_EndDate; }
		}

		ZString IFSISLot.ShippingMarks
		{
			get { return US_ShippingMarks; }
		}

		ZInt IFSISLot.Quantity1
		{
			get { return US_NoOfUnit1; }
		}

		ZInt IFSISLot.Quantity2
		{
			get { return US_NoOfUnit2; }
		}

		ZDecimal IFSISLot.NetWeightInLB
		{
			get { return NetWeightInPounds; }
		}

		ZString IFSISLot.UQ1
		{
			get { return US_UQ1; }
		}

		ZString IFSISLot.UQ2
		{
			get { return US_UQ2; }
		}

		ZString IFSISLot.ProducingEstNo
		{
			get { return US_ProducingEstNo; }
		}

		ZString IFSISLot.Species
		{
			get { return US_Species; }
		}

		ZString IFSISLot.ProductQualifierCode
		{
			get { return US_ProductQualifierCode; }
		}

		ZString IFSISLot.ProductCharacteristic
		{
			get { return US_ProductCharacteristicQualifier; }
		}

		ZString IFSISLot.SourceEstNo
		{
			get { return US_SourceEstNo; }
		}

		ZString IFSISLot.SourceCountry
		{
			get { return US_SourceCountry; }
		}
		#endregion

		public ZString SpeciesName
		{
			get { return AddInfoLookups.ProductSpeciesNames.GetDescriptionFromCode(US_Species); }
		}

		public ZString ProductCategory
		{
			get
			{
				var result = US_ProductCharacteristicQualifier;
				ZString descr = AddInfoLookups.ProductCharacteristics.GetDescriptionFromCode(US_ProductCharacteristicQualifier);
				if (!descr.IsEmpty)
				{
					var colonPosition = descr.IndexOf(":");
					result = result + " " + (colonPosition > 0 ? descr.SubstringSafe(0, colonPosition) : descr);
				}
				return result;
			}
		}

		public ZString ProductGroup
		{
			get
			{
				ZString descr = AddInfoLookups.ProductCharacteristics.GetDescriptionFromCode(US_ProductCharacteristicQualifier);
				if (!descr.IsEmpty)
				{
					var colonPosition = descr.IndexOf(":");
					return colonPosition > 0 ? descr.SubstringSafe(colonPosition + 1).TrimStart() : ZString.Empty;
				}
				return ZString.Empty;
			}
		}

		public ZString ShippingUnitPackageTypeName
		{
			get { return AddInfoLookups.PackingTypes.GetDescriptionFromCode(US_UQ1); }
		}

		public ZString ImmediateUnitPackageTypeName
		{
			get { return AddInfoLookups.PackingTypes.GetDescriptionFromCode(US_UQ2); }
		}

		public ZDecimal NetWeightInPounds
		{
			get
			{
				var result = ZDecimal.Zero;
				if (Core.Constants.Weight.ContainsCode(US_WeightUQ))
				{
					result = ((ZDecimal)Core.Constants.Weight.Convert(US_NetWeight, US_WeightUQ, Core.Constants.Weight.Pounds)).Round(2);
				}
				return result;
			}
		}

		public ZString ProcessCategory
		{
			get { return AddInfoLookups.ProductQualifierCodes.GetDescriptionFromCode(US_ProductQualifierCode); }
		}

		internal void ResetValueAfterClone()
		{
			US_EndDate = ZDateTime.Empty;
			US_LotNumber = ZString.Empty;
			US_NetWeight = ZDecimal.Zero;
			US_NoOfUnit1 = ZInt.Zero;
			US_NoOfUnit2 = ZInt.Zero;
			US_ShippingMarks = ZString.Empty;
			US_StartDate = ZDateTime.Empty;
			US_UQ1 = ZString.Empty;
			US_UQ2 = ZString.Empty;
			US_WeightUQ = ZString.Empty;
		}
	}
}
