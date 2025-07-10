using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;

namespace Enterprise.MasterFiles.Business
{
	public class TraceSourceSettings : NonPersistentBusinessObject
	{
		#region TraceSourceCategory

		[ReadOnly(true)]
		public ZString TraceSourceCategory
		{
			get { return traceSourceCategory; }
			set { SetNonPersistentPropertyValue(TraceSourceCategoryInfo, ref traceSourceCategory, value); }
		}

		public ZPropertyInfo TraceSourceCategoryInfo
		{
			get { return GetZPropertyInfo(nameof(TraceSourceCategory)); }
		}

		ZString traceSourceCategory;

		#endregion

		#region TraceSourceName

		[ReadOnly(true)]
		public ZString TraceSourceName
		{
			get { return traceSourceName; }
			set { SetNonPersistentPropertyValue(TraceSourceNameInfo, ref traceSourceName, value); }
		}

		public ZPropertyInfo TraceSourceNameInfo
		{
			get { return GetZPropertyInfo(nameof(TraceSourceName)); }
		}

		ZString traceSourceName;

		#endregion

		#region TraceSourceDescription

		[ReadOnly(true)]
		public ZString TraceSourceDescription
		{
			get { return traceSourceDescription; }
			set { SetNonPersistentPropertyValue(TraceSourceDescriptionInfo, ref traceSourceDescription, value); }
		}

		public ZPropertyInfo TraceSourceDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(TraceSourceDescription)); }
		}

		ZString traceSourceDescription;

		#endregion

		#region TraceLevel

		[List("TraceLevelList")]
		public ZString TraceLevel
		{
			get { return traceLevel; }
			set { SetNonPersistentPropertyValue(TraceLevelInfo, ref traceLevel, value); }
		}

		public ZPropertyInfo TraceLevelInfo
		{
			get { return GetZPropertyInfo(nameof(TraceLevel)); }
		}
		ZString traceLevel;

		public CodeDescriptionPairList TraceLevelList => traceLevelList ?? (traceLevelList = new TraceSourceLevels());
		CodeDescriptionPairList traceLevelList;

		public SourceLevels SourceLevel
		{
			get
			{
				if (CodeToSourceLevelMapping == null)
				{
					CodeToSourceLevelMapping = new Dictionary<ZString, SourceLevels>();
					CodeToSourceLevelMapping.Add(TraceSourceLevels.Codes.ActivityTracing, SourceLevels.ActivityTracing);
					CodeToSourceLevelMapping.Add(TraceSourceLevels.Codes.All, SourceLevels.All);
					CodeToSourceLevelMapping.Add(TraceSourceLevels.Codes.Critical, SourceLevels.Critical);
					CodeToSourceLevelMapping.Add(TraceSourceLevels.Codes.Error, SourceLevels.Error);
					CodeToSourceLevelMapping.Add(TraceSourceLevels.Codes.Information, SourceLevels.Information);
					CodeToSourceLevelMapping.Add(TraceSourceLevels.Codes.Off, SourceLevels.Off);
					CodeToSourceLevelMapping.Add(TraceSourceLevels.Codes.Verbose, SourceLevels.Verbose);
					CodeToSourceLevelMapping.Add(TraceSourceLevels.Codes.Warning, SourceLevels.Warning);
				}
				return CodeToSourceLevelMapping[TraceLevel];
			}
		}
		Dictionary<ZString, SourceLevels> CodeToSourceLevelMapping;

		#endregion

		#region TraceFilter

		public ZString TraceFilter
		{
			get { return traceFilterInfo; }
			set { SetNonPersistentPropertyValue(TraceFilterInfo, ref traceFilterInfo, value); }
		}

		public bool TraceFilter_ReadOnly { get; set; }

		public ZPropertyInfo TraceFilterInfo
		{
			get { return GetZPropertyInfo(nameof(TraceFilter)); }
		}

		ZString traceFilterInfo;

		#endregion

		public TraceSourceSettingsConfiguration ToTraceSourceSettingsConfiguration()
		{
			return new TraceSourceSettingsConfiguration
			{
				SourceLevel = SourceLevel,
				TraceSourceName = TraceSourceName,
				TraceSourceDescription = TraceSourceDescription,
				TraceFilter = TraceFilter
			};
		}
	}
}
