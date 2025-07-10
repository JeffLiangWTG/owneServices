using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry
{
	[XmlSerializerAssembly("Enterprise.Freight.Forwarding.Registry.XmlSerializers")]
	public class PreAllocationCheck : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string Measure = "Measure";
			public const string Action = "Action";
			public const string Percentage = "Percentage";
		}

		#endregion

		#region Constants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Code identifier")]
		public static class Measures
		{
			public const string Weight = "Weight";
			public const string Volume = "Volume";
			public const string Chargeable = "Chargeable";
			public const string ShipmentCount = "No. of Shipments";
			public const string Dimensions = "Dimensions";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Code identifier")]
		public static class Actions
		{
			public const string None = "None";
			public const string Warning = "Warning";
			public const string Restriction = "Restriction";
		}

		#endregion

		#region Properties

		#region MeasureMultilingualString

		[ReadOnly(true)]
		public MultilingualString MeasureMultilingualString
		{
			get { return measureMultilingualString ?? (NoResString)Measure; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}

				SetNonPersistentPropertyValue(MeasureMultilingualStringInfo, ref measureMultilingualString, value, false);
				MeasureMultilingualStringInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo MeasureMultilingualStringInfo
		{
			get { return GetZPropertyInfo(nameof(MeasureMultilingualString)); }
		}

		MultilingualString measureMultilingualString;

		#endregion

		#region Measure

		[ReadOnly(true)]
		public ZString Measure
		{
			get { return measure; }
			set
			{
				SetNonPersistentPropertyValue(MeasureInfo, ref measure, value);
			}
		}
		ZString measure;

		public ZPropertyInfo MeasureInfo
		{
			get { return GetZPropertyInfo(Schema.Measure); }
		}

		#endregion

		#region Action

		[List("ActionList")]
		public ZString Action
		{
			get { return action; }
			set
			{
				SetNonPersistentPropertyValue(ActionInfo, ref action, value);
				if (value == Actions.None)
				{
					Percentage = 0m;
				}

				if (!IsValidationSuspended)
				{
					ValidateAction();
				}
			}
		}
		ZString action;

		public ZPropertyInfo ActionInfo
		{
			get { return GetZPropertyInfo(Schema.Action); }
		}

		public void ValidateAction()
		{
			ActionInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(ActionInfo);
		}

		#endregion

		#region Percentage

		public ZDecimal Percentage
		{
			get { return percentage; }
			set
			{
				SetNonPersistentPropertyValue(PercentageInfo, ref percentage, value);
				if (!IsValidationSuspended)
				{
					ValidatePercentage();
				}
			}
		}
		ZDecimal percentage;

		public ZPropertyInfo PercentageInfo
		{
			get { return GetZPropertyInfo(Schema.Percentage); }
		}

		protected bool Percentage_ReadOnly
		{
			get { return IsNone; }
		}

		public void ValidatePercentage()
		{
			PercentageInfo.ClearAllNotifications();
			CompareValidation.CheckWithinRange(PercentageInfo, 0m, 100m);
			if (!IsNone)
			{
				CompareValidation.CheckNumberGreaterThanZero(PercentageInfo);
			}
		}

		#endregion

		#endregion

		#region Calculated Boolean Properties

		public bool IsNone
		{
			get { return Action == Actions.None; }
		}

		public bool IsWarning
		{
			get { return Action == Actions.Warning; }
		}

		public bool IsRestriction
		{
			get { return Action == Actions.Restriction; }
		}

		#endregion

		#region Lookups

		IEnumerable<ZString> ExcludedActions
		{
			get
			{
				yield return Measures.Dimensions;
			}
		}

		public CodeDescriptionPairList ActionList
		{
			get
			{
				if (actionList == null)
				{
					actionList = new CodeDescriptionPairList();
					actionList.AddPair(PreAllocationCheck.Actions.None, ResString.GetMultilingualString("b2d04c69-87b1-4f8d-8f07-2ec2f0a3927b", PreAllocationCheck.Actions.None));
					if (!ExcludedActions.Contains(Measure))
					{
						actionList.AddPair(PreAllocationCheck.Actions.Warning, ResString.GetMultilingualString("37bb7a47-e824-45c8-af8d-6264a21c2402", PreAllocationCheck.Actions.Warning));
					}
					actionList.AddPair(PreAllocationCheck.Actions.Restriction, ResString.GetMultilingualString("d331b8b8-0b08-4c90-b4e0-90bc3b767d3f", PreAllocationCheck.Actions.Restriction));
				}

				return actionList;
			}
		}
		CodeDescriptionPairList actionList;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAction();
			ValidatePercentage();
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Measure, Measure);
			writer.WriteElementString(Schema.Action, Action);
			writer.WriteElementString(Schema.Percentage, Percentage.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper wrapper)
		{
			Measure = wrapper.ReadElementString(Schema.Measure);
			Action = wrapper.ReadElementString(Schema.Action);
			Percentage = new ZDecimal(wrapper.ReadElementString(Schema.Percentage));
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PreAllocationCheck();
		}

		#endregion
	}
}
