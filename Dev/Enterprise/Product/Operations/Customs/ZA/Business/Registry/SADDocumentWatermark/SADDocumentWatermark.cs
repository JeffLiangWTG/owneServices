using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.ZA.Business.XmlSerializers")]
	public class SADDocumentWatermark : RegistryBusinessObjectTemplate
	{
		public SADDocumentWatermark()
		{
		}

		public SADDocumentWatermark(FallbackLevel fallbackLevel, BusinessObjectFactory factory, SADDocumentWatermarkCollection collection) : base(fallbackLevel, factory)
		{
			this.collection = collection;
		}

		readonly SADDocumentWatermarkCollection collection;

		public static class Schema
		{
			public const string EntryStatusCode = "EntryStatusCode";
			public const string WatermarkText = "WatermarkText";
		}

		#region EntryStatusCode
		[List(nameof(EntryStatusCodeList))]
		public ZString EntryStatusCode
		{
			get { return entryStatusCode; }
			set
			{
				SetNonPersistentPropertyValue(EntryStatusCodeInfo, ref entryStatusCode, value);

				if (!IsValidationSuspended)
				{
					ValidateEntryStatusCode();
				}

				EntryStatusCodeInfo.RefreshBinding();
			}
		}
		ZString entryStatusCode;

		public ZPropertyInfo EntryStatusCodeInfo
		{
			get { return GetZPropertyInfo(Schema.EntryStatusCode); }
		}
		void ValidateEntryStatusCode()
		{
			EntryStatusCodeInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(EntryStatusCodeInfo, "Entry Status Code");
			ListValidation.ErrorIfInvalidCode(EntryStatusCodeInfo, EntryStatusCodeList);

			if (collection != null)
			{
				foreach (SADDocumentWatermark watermark in collection)
				{
					if (watermark != this && watermark.EntryStatusCode == EntryStatusCode)
					{
						EntryStatusCodeInfo.AddError(DuplicateEntryStatusCode);
						break;
					}
				}
			}
		}

		public const string DuplicateEntryStatusCode = "You have already entered this entry status code";

		public ZString EntryStatusDescription
		{
			get
			{
				return EntryStatusCode.IsEmpty ? string.Empty : EntryStatusCodeList.GetDescriptionFromCode(EntryStatusCode) ?? string.Empty;
			}
		}
		#endregion

		#region WatermarkText
		public ZString WatermarkText
		{
			get { return watermarkText; }
			set
			{
				SetNonPersistentPropertyValue(WatermarkTextInfo, ref watermarkText, value);

				WatermarkTextInfo.RefreshBinding();
			}
		}
		ZString watermarkText;

		public ZPropertyInfo WatermarkTextInfo
		{
			get { return GetZPropertyInfo(Schema.WatermarkText); }
		}
		#endregion

		#region Lists
		public CodeDescriptionPairList EntryStatusCodeList
		{
			get
			{
				return ZARefCusCodeListTypes.GetCustomsStatusList(CurrentFactory);
			}
		}
		#endregion

		#region Overrides
		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateEntryStatusCode();
		}
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SADDocumentWatermark(fallbackLevel, factory, null);
		}

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			EntryStatusCode = reader.ReadElementString(Schema.EntryStatusCode);
			WatermarkText = reader.ReadElementString(Schema.WatermarkText);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.EntryStatusCode, EntryStatusCode);
			writer.WriteElementString(Schema.WatermarkText, WatermarkText);
		}
		#endregion
	}
}
