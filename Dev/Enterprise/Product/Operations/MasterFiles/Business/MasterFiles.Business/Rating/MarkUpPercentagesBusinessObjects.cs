using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	#region MarkUpPercentage

	public class MarkUpPercentage : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string Mode = "Mode";
			public const string Location = "Location";
			public const string Percentage = "Percentage";
			public const string Minimum = "Minimum";
			public const string PerUnit = "PerUnit";
		}

		#endregion

		#region Constants

		public const string ALL = "ALL";
		public const string AIR = "AIR";
		public const string FCL = "FCL";
		public const string LCL = "LCL";
		public const string AIO = "AIO";
		public const string FCO = "FCO";
		public const string LCO = "LCO";
		public const string ORG = "ORG";
		public const string DST = "DST";

		#endregion

		public MarkUpPercentage(ZString mode, ZString location, ZDecimal percentage, ZDecimal minimum, ZDecimal perUnit, MarkUpPercentagesCollection parent) : base(new BusinessObjectFactory())
		{
			fMode = mode;
			fLocation = location;
			fPercentage = percentage;
			fMinimum = minimum;
			fPerUnit = perUnit;
			this.Parent = parent;
		}

		#region Properties

		#region Mode

		[MaxLength(3)]
		public ZString Mode
		{
			get { return fMode; }
			set
			{
				if (fMode != value)
				{
					CheckMaximumLength(ModeInfo, value);
					fMode = value;

					if (!IsValidationSuspended)
					{
						ValidateMode();
					}
					ModeInfo.RefreshBinding();
				}
			}
		}

		ZString fMode;

		public ZPropertyInfo ModeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.Mode); }
		}

		public void ValidateMode()
		{
			ModeInfo.ClearAllNotifications();
			if (fMode.IsEmpty)
			{
				MandatoryValidation.CheckEntered(ModeInfo);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(ModeInfo, Modes);
			}
		}

		public bool IsFreightCharge
		{
			get { return Mode == AIR || Mode == FCL || Mode == LCL; }
		}

		protected bool IsNotFreightCharge
		{
			get { return !IsFreightCharge; }
		}

		#endregion

		#region Location

		ZString fLocation;
		[MaxLength(5)]
		public ZString Location
		{
			get { return fLocation; }
			set
			{
				if (fLocation != value)
				{
					CheckMaximumLength(LocationInfo, value);
					fLocation = value;
					if (!IsValidationSuspended)
					{
						ValidateLocation();
					}
					LocationInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo LocationInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.Location); }
		}

		public void ValidateLocation()
		{
			LocationInfo.ClearAllNotifications();
			if (!Location.IsEmpty && LocationHelper.GetLocationFromString(Location, Factory) == null)
			{
				LocationInfo.AddError(Res.GetString("133c10ff-4b23-4161-8ad9-b8b9b3d9f863", "You have selected an invalid location. Please insert a valid value."));
			}
		}

		#endregion

		#region Percentage

		ZDecimal fPercentage;
		public ZDecimal Percentage
		{
			get { return fPercentage; }
			set
			{
				if (fPercentage != value)
				{
					fPercentage = value;
					if (!IsValidationSuspended)
					{
						ValidatePercentage();
					}
					PercentageInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo PercentageInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.Percentage); }
		}

		public void ValidatePercentage()
		{
			PercentageInfo.ClearAllNotifications();
			if (!Percentage.IsEmpty && (!Minimum.IsEmpty || !PerUnit.IsEmpty))
			{
				PercentageInfo.AddError(Res.GetString("3bca1b34-d6b1-4a0d-9374-6f7ef407da72", "You cannot specify a Percentage uplift if you have already specified a Minimum or Per Unit uplift."));
			}
		}

		#endregion

		#region Minimum

		[ReadOnlyMember(nameof(IsNotFreightCharge))]
		public ZDecimal Minimum
		{
			get { return IsFreightCharge ? fMinimum : (ZDecimal)0m; }
			set
			{
				if (fMinimum != value)
				{
					fMinimum = value;
					MinimumInfo.ClearAllNotifications();
					MinimumInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						ValidatePercentage();  
					}
				}
			}
		}

		ZDecimal fMinimum;

		public ZPropertyInfo MinimumInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.Minimum); }
		}

		#endregion

		#region PerUnit

		[ReadOnlyMember(nameof(IsNotFreightCharge))]
		public ZDecimal PerUnit
		{
			get { return IsFreightCharge ? fPerUnit : (ZDecimal)0m; }
			set
			{
				if (fPerUnit != value)
				{
					fPerUnit = value;
					PerUnitInfo.ClearAllNotifications();
					PerUnitInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						ValidatePercentage(); 
					}
				}
			}
		}

		ZDecimal fPerUnit;

		public ZPropertyInfo PerUnitInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.PerUnit); }
		}

		#endregion

		#endregion

		#region Collections

		public LocationCollection Locations
		{
			get { return Parent.Locations; }
		}

		public CodeDescriptionPairList Modes
		{
			get { return Parent.Modes; }
		}

		#endregion

		#region To/From String

		public static MarkUpPercentage FromString(string value, MarkUpPercentagesCollection parent)
		{
			try
			{
				if (!string.IsNullOrEmpty(value.Trim()))
				{
					string[] subStrings = value.Split('|');
					ZDecimal percentage = Decimal.Parse(subStrings[Math.Min(subStrings.Length - 1, 2)], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
					switch (subStrings.Length)
					{
						case 1:
							return new MarkUpPercentage(ALL, ZString.Empty, percentage, 0m, 0m, parent);

						case 2:
							return new MarkUpPercentage(ALL, subStrings[0], percentage, 0m, 0m, parent);

						case 3:
							return new MarkUpPercentage(subStrings[0], subStrings[1], percentage, 0m, 0m, parent);

						case 5:
							ZDecimal minimum = Decimal.Parse(subStrings[3], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
							ZDecimal perUnit = Decimal.Parse(subStrings[4], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
							return new MarkUpPercentage(subStrings[0], subStrings[1], percentage, minimum, perUnit, parent);
					}
				}
			}
			catch (ArgumentNullException)
			{
			}
			catch (FormatException)
			{
			}

			return null;
		}

		public override string ToString()
		{
			return String.Format("{0}|{1}|{2}|{3}|{4}", Mode, Location,
				Percentage.ToString("g", System.Globalization.CultureInfo.InvariantCulture.NumberFormat),
				Minimum.ToString("g", System.Globalization.CultureInfo.InvariantCulture.NumberFormat),
				PerUnit.ToString("g", System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
		}

		#endregion

		public ZBool IsValid
		{
			get { return !LocationInfo.HasErrors() && !ModeInfo.HasErrors() && !PercentageInfo.HasErrors(); }
		}

		readonly MarkUpPercentagesCollection Parent;
	}

	#endregion

	#region MarkUpPercentagesCollection

	public class MarkUpPercentagesCollection : NonPersistentBusinessObjectCollection<MarkUpPercentage>
	{
		public MarkUpPercentagesCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public MarkUpPercentagesCollection(string value) : this(new BusinessObjectFactory())
		{
			Load(value);
		}

		public void Load(string value)
		{
			string[] markUpPercentageStrings = value.Split(';');
			foreach (string markUpPercentageString in markUpPercentageStrings)
			{
				MarkUpPercentage markUpPercentage = MarkUpPercentage.FromString(markUpPercentageString, this);
				if (markUpPercentage != null)
				{
					Add(markUpPercentage);
				}
			}
		}

		public override string ToString()
		{
			string result = "";
			foreach (MarkUpPercentage elem in this)
			{
				if (elem.IsValid)
				{
					if (!string.IsNullOrEmpty(result))
					{
						result += ";";
					}

					result += elem.ToString();
				}
			}

			return result;
		}

		public MarkUpPercentage FindElement(ZString mode, ILocation location, bool isImport)
		{
			ZString uNLOCO = location == null || location.UNLOCO == null ? ZString.Empty : location.UNLOCO.Code;
			ZString country = location == null || location.Country == null ? ZString.Empty : location.Country.Code;

			MarkUpPercentage uNLOCOElem = null;
			MarkUpPercentage countryElem = null;
			MarkUpPercentage defaultElem = null;

			foreach (MarkUpPercentage elem in this)
			{
				if (elem.Mode != MarkUpPercentage.ALL && elem.Mode != mode)
				{
					continue;
				}

				if (!elem.Location.IsEmpty)
				{
					if (elem.Location == uNLOCO)
					{
						uNLOCOElem = SetElem(uNLOCOElem, elem);
					}
					else if (elem.Location == country)
					{
						countryElem = SetElem(countryElem, elem);
					}
				}
				else
				{
					defaultElem = SetElem(defaultElem, elem);
				}
			}

			if (uNLOCOElem != null)
			{
				return uNLOCOElem;
			}
			else if (countryElem != null)
			{
				return countryElem;
			}
			else if (defaultElem != null)
			{
				return defaultElem;
			}
			else
			{
				return null;
			}
		}

		MarkUpPercentage SetElem(MarkUpPercentage oldValue, MarkUpPercentage newValue)
		{
			if (oldValue != null && newValue.Mode == MarkUpPercentage.ALL)
			{
				return oldValue;
			}
			return newValue;
		}

		#region Locations

		public LocationCollection Locations
		{
			get { return Factory.GetCachedValue("LocationCollectionWithoutZones", () => new LocationCollection(Factory, false)); }
		}

		#endregion

		#region Modes

		CodeDescriptionPairList fModes;
		public CodeDescriptionPairList Modes
		{
			get
			{
				if (fModes == null)
				{
					fModes = new CodeDescriptionPairList();
					fModes.AddPair(MarkUpPercentage.ALL, Res.GetString("dfe42606-54d7-4542-8500-3e6ff5803266", "All Charges"));
					fModes.AddPair(MarkUpPercentage.AIR, Res.GetString("319f4bd9-bde6-4aff-bc00-36eb4e012a2e", "Air Freight Charge"));
					fModes.AddPair(MarkUpPercentage.FCL, Res.GetString("a617b590-57c6-4da0-ab1e-36936a7c7bf5", "FCL Freight Charge"));
					fModes.AddPair(MarkUpPercentage.LCL, Res.GetString("c49e2cad-b25d-4028-b66b-995c05ad44b9", "LCL Freight Charge"));
					fModes.AddPair(MarkUpPercentage.AIO, Res.GetString("8c50e38c-e73b-40d7-ae9e-d9f53bc6f5fe", "Air Freight Other Charges"));
					fModes.AddPair(MarkUpPercentage.FCO, Res.GetString("fe2238dd-e85f-4e74-b612-1cd7fb69cb44", "FCL Freight Other Charges"));
					fModes.AddPair(MarkUpPercentage.LCO, Res.GetString("89d56b96-6571-4cb8-9226-a8249be8e7ee", "LCL Freight Other Charges"));
					fModes.AddPair(MarkUpPercentage.ORG, Res.GetString("3750e45c-d36d-4d74-b0d0-94d6f81174a6", "Origin Charges"));
					fModes.AddPair(MarkUpPercentage.DST, Res.GetString("1f4d9b4d-3433-491d-8a03-d1c17c36f226", "Destination Charges"));
				}
				return fModes;
			}
		}

		#endregion

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new MarkUpPercentage(MarkUpPercentage.ALL, ZString.Empty, 0m, 0m, 0m, this);
		}
	}

	#endregion

	#region MarkUpPercentagesCollectionWrapper

	public class MarkUpPercentagesCollectionWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public MarkUpPercentagesCollectionWrapper(string value)
		{
			fMarkUpPercentages = new MarkUpPercentagesCollection(value);
		}

		readonly MarkUpPercentagesCollection fMarkUpPercentages;
		public MarkUpPercentagesCollection MarkUpPercentages
		{
			get { return fMarkUpPercentages; }
		}
	}

	#endregion
}
