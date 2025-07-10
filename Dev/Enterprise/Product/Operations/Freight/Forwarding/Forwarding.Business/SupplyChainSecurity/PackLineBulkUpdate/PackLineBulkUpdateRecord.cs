using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public class PackLineBulkUpdateRecord : NonPersistentBusinessObject
	{
		public PackLineBulkUpdateRecord(ForwardingPackLine packLine)
		{
			Argument.NotNull(packLine, nameof(packLine));
			wrappedPackLine = packLine;
		}

		readonly ForwardingPackLine wrappedPackLine;

		public ZString ShipmentId
		{
			get { return wrappedPackLine.Shipment.JS_UniqueConsignRef; }
		}

		public ZString ShipmentInspectionType
		{
			get { return wrappedPackLine.Shipment.JS_InspectionTypeCode; }
		}

		public ZString PackLineId
		{
			get { return wrappedPackLine.JL_PackLineId; }
		}

		[List(nameof(PackLineInspectionTypes))]
		public ZString PackLineInspectionType
		{
			get { return wrappedPackLine.JL_InspectionTypeCode; }
			set { wrappedPackLine.JL_InspectionTypeCode = value; }
		}
		public ZWrappedPropertyInfo PackLineInspectionTypeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(PackLineInspectionType), x => wrappedPackLine.JL_InspectionTypeCodeInfo); }
		}

		public ZString PackageType
		{
			get { return wrappedPackLine.JL_F3_NKPackType; }
		}

		public ZInt PackageCount
		{
			get { return wrappedPackLine.JL_PackageCount; }
		}

		public ZDecimal ActualVolume
		{
			get { return wrappedPackLine.JL_ActualVolume; }
		}

		public ZString ActualVolumeUQ
		{
			get { return wrappedPackLine.JL_ActualVolumeUQ; }
		}

		public ZDecimal ActualWeight
		{
			get { return wrappedPackLine.JL_ActualWeight; }
		}

		public ZString ActualWeightUQ
		{
			get { return wrappedPackLine.JL_ActualWeightUQ; }
		}

		protected override void RunPreSaveValidationCore()
		{
			wrappedPackLine.Validation.ValidateJL_InspectionTypeCode();
			base.RunPreSaveValidationCore();
		}

		public CodeDescriptionPairList PackLineInspectionTypes
		{
			get { return wrappedPackLine.InspectionTypes; }
		}

		protected override ZString HumanReadableNameCore => wrappedPackLine.HumanReadableName;
	}
}
