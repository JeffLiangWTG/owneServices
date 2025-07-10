using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.Customs.ZA.Business.ResString;

namespace Enterprise.Customs.ZA.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.ZA.Business.XmlSerializers")]
	public class CPCAcquitByDate : RegistryBusinessObjectTemplate
	{
		public CPCAcquitByDate()
		{
		}

		public CPCAcquitByDate(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public static class Schema
		{
			public const string Quantity = "Quantity";
			public const string Unit = "Unit";
		}

		#region Quantity

		[ResourceStringData("Enterprise.Customs.ZA.DataRegistry.Business.CPCAcquitByDate|Quantity", Caption = "Quantity")]
		public ZInt Quantity
		{
			get => quantity;
			set
			{
				SetNonPersistentPropertyValue(QuantityInfo, ref quantity, value);

				if (!IsValidationSuspended)
				{
					ValidateQuantity();
				}
			}
		}

		void ValidateQuantity()
		{
			QuantityInfo.ClearAllNotifications();
			if (Quantity < 0)
			{
				QuantityInfo.AddError(ValidQuantityRequired);
			}
		}
		public const string ValidQuantityRequired = "Please enter a positive integer for Quantity or choose one from the list";

		ZInt quantity;

		public ZPropertyInfo QuantityInfo => GetZPropertyInfo(Schema.Quantity);

		#endregion

		#region "Unit"

		public IEnumerable<CPCAcquitByDateData> ValidUnits => new List<CPCAcquitByDateData>
		{
			new CPCAcquitByDateData(1, "MONTH(S)", ResString.GetMultilingualString("3E549970-7A46-47A2-B6E0-50117C53C008", "Month(s)")),
			new CPCAcquitByDateData(2, "DAY(S)", ResString.GetMultilingualString("28959771-4141-45AB-9803-F21018FF700A", "Day(s)"))
		};

		[List(nameof(ValidUnits))]
		[ResourceStringData("Enterprise.Customs.ZA.DataRegistry.Business.CPCAcquitByDate|Unit", Caption = "Unit")]
		public ZString Unit
		{
			get => unit;
			set
			{
				value = value.ToUpper();
				SetNonPersistentPropertyValue(UnitInfo, ref unit, value);

				if (!IsValidationSuspended)
				{
					ValidateUnit();
				}
			}
		}
		ZString unit;

		public ZPropertyInfo UnitInfo => GetZPropertyInfo(Schema.Unit);

		void ValidateUnit()
		{
			UnitInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(UnitInfo, "Unit");
			if (ValidUnits.All(d => d.Code != Unit))
			{
				UnitInfo.AddError(ValidUnitRequired);
			}
		}
		public const string ValidUnitRequired = "Please enter a valid Unit or choose one from the list";

		#endregion

		#region Implements

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CPCAcquitByDate(fallbackLevel, factory);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Quantity = reader.ReadElementStringAsZInt(Schema.Quantity);
			Unit = reader.ReadElementString(Schema.Unit);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Quantity, Quantity.ToString());
			writer.WriteElementString(Schema.Unit, Unit);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateUnit();
			ValidateQuantity();
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			Quantity = 24;
			Unit = ValidUnits.First().Code;
		}

		#endregion
	}
}
