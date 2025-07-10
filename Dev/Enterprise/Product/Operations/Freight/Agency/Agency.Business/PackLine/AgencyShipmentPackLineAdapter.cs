using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	[TestedAsNonPersistentBusinessObject]
	public class AgencyShipmentPackLineAdapter : AgencyShipmentPackLine
	{
		public AgencyShipmentPackLineAdapter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static AgencyShipmentPackLineAdapter New(AgencyShipmentContainer adaptee)
		{
			var adapter = adaptee.Factory.New<AgencyShipmentPackLineAdapter>();
			adapter.Adaptee = adaptee;
			return adapter;
		}

		protected override void SetDefaultValues()
		{
		}

		public override ZGuid JL_JC
		{
			get { return ZGuid.Empty; }
			set { }
		}

		public override ZString JL_RefNumber
		{
			get { return GetValue<ZString>(Schema.JL_RefNumber); }
			set { SetValue(Schema.JL_RefNumber, value); }
		}

		public override ZPropertyInfo JL_RefNumberInfo
		{
			get { return GetPropertyInfo(Schema.JL_RefNumber); }
		}

		public override ZInt JL_PackageCount
		{
			get { return GetValue<ZShort>(Schema.JL_PackageCount); }
			set { SetValue(Schema.JL_PackageCount, (ZShort)value); }
		}

		public override ZPropertyInfo JL_PackageCountInfo
		{
			get { return GetPropertyInfo(Schema.JL_PackageCount); }
		}

		public override ZString JL_DetailedDescription
		{
			get { return GetValue<ZString>(Schema.JL_DetailedDescription); }
			set { SetValue(Schema.JL_DetailedDescription, value); }
		}

		public override ZPropertyInfo JL_DetailedDescriptionInfo
		{
			get { return GetPropertyInfo(Schema.JL_DetailedDescription); }
		}

		public override ZString JL_MarksAndNumbers
		{
			get { return GetValue<ZString>(Schema.JL_MarksAndNumbers); }
			set { SetValue(Schema.JL_MarksAndNumbers, value); }
		}

		public override ZPropertyInfo JL_MarksAndNumbersInfo
		{
			get { return GetPropertyInfo(Schema.JL_MarksAndNumbers); }
		}

		public override ZDecimal JL_ActualWeight
		{
			get { return GetValue<ZDecimal>(Schema.JL_ActualWeight); }
			set { SetValue(Schema.JL_ActualWeight, value); }
		}

		public override ZPropertyInfo JL_ActualWeightInfo
		{
			get { return GetPropertyInfo(Schema.JL_ActualWeight); }
		}

		public override ZString JL_ActualWeightUQ
		{
			get { return GetValue<ZString>(Schema.JL_ActualWeightUQ); }
			set { SetValue(Schema.JL_ActualWeightUQ, value); }
		}

		public override ZPropertyInfo JL_ActualWeightUQInfo
		{
			get { return GetPropertyInfo(Schema.JL_ActualWeightUQ); }
		}

		public override ZDecimal JL_ActualVolume
		{
			get { return GetValue<ZDecimal>(Schema.JL_ActualVolume); }
			set { SetValue(Schema.JL_ActualVolume, value); }
		}

		public override ZPropertyInfo JL_ActualVolumeInfo
		{
			get { return GetPropertyInfo(Schema.JL_ActualVolume); }
		}

		public override ZString JL_ActualVolumeUQ
		{
			get { return GetValue<ZString>(Schema.JL_ActualVolumeUQ); }
			set { SetValue(Schema.JL_ActualVolumeUQ, value); }
		}

		public override ZPropertyInfo JL_ActualVolumeUQInfo
		{
			get { return GetPropertyInfo(Schema.JL_ActualVolumeUQ); }
		}

		public override ZDecimal JL_Length
		{
			get { return GetValue<ZDecimal>(Schema.JL_Length); }
			set { SetValue(Schema.JL_Length, value); }
		}

		public override ZPropertyInfo JL_LengthInfo
		{
			get { return GetPropertyInfo(Schema.JL_Length); }
		}

		public override ZDecimal JL_Width
		{
			get { return GetValue<ZDecimal>(Schema.JL_Width); }
			set { SetValue(Schema.JL_Width, value); }
		}

		public override ZPropertyInfo JL_WidthInfo
		{
			get { return GetPropertyInfo(Schema.JL_Width); }
		}

		public override ZDecimal JL_Height
		{
			get { return GetValue<ZDecimal>(Schema.JL_Height); }
			set { SetValue(Schema.JL_Height, value); }
		}

		public override ZPropertyInfo JL_HeightInfo
		{
			get { return GetPropertyInfo(Schema.JL_Height); }
		}

		public override ZString JL_UnitOfDimension
		{
			get { return GetValue<ZString>(Schema.JL_UnitOfDimension); }
			set { SetValue(Schema.JL_UnitOfDimension, value); }
		}

		public override ZPropertyInfo JL_UnitOfDimensionInfo
		{
			get { return GetPropertyInfo(Schema.JL_UnitOfDimension); }
		}

		public override ZString JL_RH_NKCommodityCode
		{
			get { return GetValue<ZString>(Schema.JL_RH_NKCommodityCode); }
			set { SetValue(Schema.JL_RH_NKCommodityCode, value); }
		}

		public override ZPropertyInfo JL_RH_NKCommodityCodeInfo
		{
			get { return GetPropertyInfo(Schema.JL_RH_NKCommodityCode); }
		}

		public override ZString JL_HarmonisedCode
		{
			get { return GetValue<ZString>(Schema.JL_HarmonisedCode); }
			set { SetValue(Schema.JL_HarmonisedCode, value); }
		}

		public override ZPropertyInfo JL_HarmonisedCodeInfo
		{
			get { return GetPropertyInfo(Schema.JL_HarmonisedCode); }
		}

		protected override UNDGDataItemCollection GetNewUNDGs()
		{
			return Adaptee != null ? Adaptee.UNDGs : null;
		}

		public override bool IsSavedByFactory
		{
			get { return false; }
		}

		public override void Delete()
		{
			base.Delete();

			if (Adaptee != null)
			{
				Adaptee.Delete();
			}
		}

		#region Implementation

		public AgencyShipmentContainer Adaptee { get; set; }

		readonly Dictionary<string, string> adapteeMapping = new Dictionary<string, string>
		{
			{ JobPackLinesSchema.JL_RefNumber.Name, JobContainerSchema.JC_ContainerNum.Name },
			{ JobPackLinesSchema.JL_PackageCount.Name, JobContainerSchema.JC_ContainerCount.Name },
			{ JobPackLinesSchema.JL_DetailedDescription.Name, JobContainerSchema.JC_Description.Name },
			{ JobPackLinesSchema.JL_MarksAndNumbers.Name, JobContainerSchema.JC_MarksAndNumbers.Name },
			{ JobPackLinesSchema.JL_ActualWeight.Name, JobContainerSchema.JC_GrossWeight.Name },
			{ JobPackLinesSchema.JL_ActualWeightUQ.Name, JobContainerSchema.JC_GrossWeightUQ.Name },
			{ JobPackLinesSchema.JL_ActualVolume.Name, JobContainerSchema.JC_GrossVolume.Name },
			{ JobPackLinesSchema.JL_ActualVolumeUQ.Name, JobContainerSchema.JC_GrossVolumeUQ.Name },
			{ JobPackLinesSchema.JL_Length.Name, JobContainerSchema.JC_TotalLength.Name },
			{ JobPackLinesSchema.JL_Width.Name, JobContainerSchema.JC_TotalWidth.Name },
			{ JobPackLinesSchema.JL_Height.Name, JobContainerSchema.JC_TotalHeight.Name },
			{ JobPackLinesSchema.JL_UnitOfDimension.Name, JobContainerSchema.JC_TotalUnitOfMeasure.Name },
			{ JobPackLinesSchema.JL_RH_NKCommodityCode.Name, JobContainerSchema.JC_RH_NKContainerCommodityCode.Name },
			{ JobPackLinesSchema.JL_HarmonisedCode.Name, JobContainerSchema.JC_HarmonisedCode.Name }
		};

		T GetValue<T>(string columnName)
		{
			if (Adaptee != null && adapteeMapping.ContainsKey(columnName))
			{
				return (T)Adaptee[adapteeMapping[columnName]];
			}

			return default(T);
		}

		void SetValue<T>(string columnName, T value)
		{
			if (Adaptee != null && adapteeMapping.ContainsKey(columnName))
			{
				Adaptee[adapteeMapping[columnName]] = value;
			}
		}

		ZPropertyInfo GetPropertyInfo(string columnName)
		{
			ZPropertyInfo propertyInfo = null;

			if (Adaptee != null && adapteeMapping.ContainsKey(columnName))
			{
				propertyInfo = Adaptee.ZPropertyInfoHash[adapteeMapping[columnName]];
			}

			return GetWrappedZPropertyInfo(columnName, x => propertyInfo);
		}

		#endregion
	}
}
