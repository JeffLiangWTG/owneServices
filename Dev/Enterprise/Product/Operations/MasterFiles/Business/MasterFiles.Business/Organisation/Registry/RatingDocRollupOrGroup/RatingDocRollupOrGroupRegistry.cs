using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class RatingDocRollupOrGroupRegistry
		: RegistryBusinessObjectTemplate
		, IRatingDocRollupOrSort
		, IDocRollupOrGroupForBestMatcher
	{
		#region Schema

		public abstract class Schema
		{
			public const string Module = "Module";
			public const string JobType = "JobType";
			public const string TransportMode = "TransportMode";
			public const string Display = "Display";
			public const string Style = "Style";
		}

		#endregion

		public RatingDocRollupOrGroupRegistry()
		{
		}

		public RatingDocRollupOrGroupRegistry(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			=> new RatingDocRollupOrGroupRegistry(fallbackLevel, factory);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateModule();
			ValidateJobType();
			ValidateTransportMode();
			ValidateDisplay();
			ValidateStyle();
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Module, Module.ToString());
			writer.WriteElementString(Schema.JobType, JobType.ToString());
			writer.WriteElementString(Schema.TransportMode, TransportMode.ToString());
			writer.WriteElementString(Schema.Display, Display.ToString());
			writer.WriteElementString(Schema.Style, Style.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Module = new ZString(reader.ReadElementString(Schema.Module));
			JobType = new ZString(reader.ReadElementString(Schema.JobType));
			TransportMode = new ZString(reader.ReadElementString(Schema.TransportMode));
			Display = new ZString(reader.ReadElementString(Schema.Display));
			Style = new ZString(reader.ReadElementString(Schema.Style));
		}

		#endregion

		#region Properties

		#region Module

		[List("ModuleList")]
		[MaxLength(3)]
		public ZString Module
		{
			get => fModule;

			set
			{
				SetNonPersistentPropertyValue(ModuleInfo, ref fModule, value);
				if (!IsValidationSuspended)
				{
					ValidateModule();
				}
			}
		}

		ZString fModule;

		public ZPropertyInfo ModuleInfo => GetZPropertyInfo(Schema.Module);

		public CodeDescriptionPairList ModuleList => Helper.ModuleList;

		void ValidateModule()
		{
			ModuleInfo.ClearAllNotifications();
			Helper.ValidateModule();
			CheckThatDefaultRecordExist(ModuleInfo);
		}

		#endregion

		#region JobType

		[List("JobTypeList")]
		[MaxLength(3)]
		public ZString JobType
		{
			get => fJobType;

			set
			{
				SetNonPersistentPropertyValue(JobTypeInfo, ref fJobType, value);

				if (!IsValidationSuspended)
				{
					ValidateJobType();
				}
			}
		}

		ZString fJobType;
		public ZPropertyInfo JobTypeInfo => GetZPropertyInfo(Schema.JobType);

		public CodeDescriptionPairList JobTypeList => Helper.JobTypeList;

		void ValidateJobType()
		{
			JobTypeInfo.ClearAllNotifications();
			Helper.ValidateJobType();
		}

		#endregion

		#region TransportMode

		[List("TransportModeList")]
		[MaxLength(3)]
		public ZString TransportMode
		{
			get => fTransportMode;

			set
			{
				SetNonPersistentPropertyValue(TransportModeInfo, ref fTransportMode, value);
				if (!IsValidationSuspended)
				{
					ValidateTransportMode();
				}
			}
		}

		ZString fTransportMode;

		public ZPropertyInfo TransportModeInfo => GetZPropertyInfo(Schema.TransportMode);

		public CodeDescriptionPairList TransportModeList => Helper.TransportModeList;

		void ValidateTransportMode()
		{
			TransportModeInfo.ClearAllNotifications();
			Helper.ValidateTransportMode();
		}

		#endregion

		#region Display

		[List("DisplayList")]
		[MaxLength(3)]
		public ZString Display
		{
			get => fDisplay;

			set
			{
				SetNonPersistentPropertyValue(DisplayInfo, ref fDisplay, value);
				if (!IsValidationSuspended)
				{
					ValidateDisplay();
				}
			}
		}

		ZString fDisplay;

		public ZPropertyInfo DisplayInfo => GetZPropertyInfo(Schema.Display);

		public CodeDescriptionPairList DisplayList => Helper.DisplayList;

		void ValidateDisplay()
		{
			DisplayInfo.ClearAllNotifications();
			Helper.ValidateDisplay();
		}

		#endregion

		#region Style

		[List("StyleList")]
		[MaxLength(3)]
		public ZString Style
		{
			get => fStyle;

			set
			{
				SetNonPersistentPropertyValue(StyleInfo, ref fStyle, value);
				if (!IsValidationSuspended)
				{
					ValidateStyle();
				}
			}
		}
		ZString fStyle;
		public ZPropertyInfo StyleInfo => GetZPropertyInfo(Schema.Style);

		public CodeDescriptionPairList StyleList => Helper.StyleList;

		void ValidateStyle()
		{
			StyleInfo.ClearAllNotifications();
			Helper.ValidateStyle();
		}

		#endregion
		public bool IsRegistry => true;

		#endregion

		#region IRollupOrGroupForBestMatch

		string IDocRollupOrGroupForBestMatcher.JobType => JobType;
		string IDocRollupOrGroupForBestMatcher.ServiceDirection => string.Empty;
		string IDocRollupOrGroupForBestMatcher.TransportMode => TransportMode;

		bool IDocRollupOrGroupForBestMatcher.HasServiceDirection => false;

		#endregion

		#region Implementation

		void CheckThatDefaultRecordExist(ZPropertyInfo propertyInfo)
		{
			if (ParentCollection != null)
			{
				bool isDefaultExist = false;
				foreach (RatingDocRollupOrGroupRegistry ratingDocumentsChargeGroupingAndRoll in ParentCollection)
				{
					if
						(
							ratingDocumentsChargeGroupingAndRoll.Module == DocRollupOrSortModuleList.Codes.All
							&& ratingDocumentsChargeGroupingAndRoll.JobType == DocRollupOrSortJobTypeList.Codes.All
							&& ratingDocumentsChargeGroupingAndRoll.TransportMode == DocRollupOrSortTransportModeList.Codes.All
						)
					{
						isDefaultExist = true;
						break;
					}
				}
				if (!isDefaultExist)
				{
					var errorMessage = Res.GetString("5ba484f5-6220-473f-aac5-32741dd75619", "You must always have a row with Module = All, Job Type = All, Mode = All.");
					propertyInfo.AddError(errorMessage);
				}
			}
		}

		[BusinessObjectTestExclude]
		public IBusinessObjectCollection ParentCollection
			=> ((IBusinessObjectInternals)this).ParentCollections.Length > 0
				? (RatingDocRollupOrGroupRegistryCollection)((IBusinessObjectInternals)this).ParentCollections[0]
				: null;

		OrgRatingDocRollupOrGroupHelper Helper => helper ??= new OrgRatingDocRollupOrGroupHelper(this);
		OrgRatingDocRollupOrGroupHelper helper;

		#endregion
	}
}
