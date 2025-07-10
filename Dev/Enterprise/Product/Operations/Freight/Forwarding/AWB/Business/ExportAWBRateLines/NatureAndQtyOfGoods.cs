using System;
using System.ComponentModel;
using System.Drawing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class NatureAndQtyOfGoods : NonPersistentBusinessObject
	{
		public NatureAndQtyOfGoods(ExportAWBRateLine rateLine)
			: base(rateLine.Factory)
		{
			ParentRateLine = rateLine;

			if (!ParentRateLine.IsDeleted)
			{
				using (GetValidationSuspender())
				using (SuspendSettingHasChanges())
				{
					Text = ParentRateLine.ER_NatureAndQtyOfGoods;
				}
			}
		}

		public ExportAWBRateLine ParentRateLine { get; private set; }

		public static Font Font
		{
			get { return natureAndQtyOfGoodsFont ?? (natureAndQtyOfGoodsFont = new Font("Arial", 9)); }
		}
		[ThreadStatic]
		static Font natureAndQtyOfGoodsFont;

		public int TextSize
		{
			get
			{
				TextSizeCalculator textSizeCalculator = new TextSizeCalculator(Font);
				return textSizeCalculator.GetLengthInMillimeter(Text);
			}
		}

		public static class Schema
		{
			public const string Text = "Text";
		}

		[MaxLength(35)]
		[BusinessObjectTestExclude]
		public ZString Text
		{
			get
			{
				return TextCore;
			}
			set
			{
				if (Text != value)
				{
					TextCore = value;
				}
			}
		}
		ZString text;

		protected virtual ZString TextCore
		{
			get
			{
				return text;
			}
			set
			{
				if (SetNonPersistentPropertyValue(TextInfo, ref text, value.SubstringSafe(0, 35)) && !IsValidationSuspended)
				{
					Validation.ValidateText();
				}
			}
		}

		public ZPropertyInfo TextInfo
		{
			get { return GetZPropertyInfo(Schema.Text); }
		}

		public NatureAndQtyOfGoodsValidation Validation
		{
			get { return GetValidation(); }
		}

		protected virtual NatureAndQtyOfGoodsValidation GetValidation()
		{
			return new NatureAndQtyOfGoodsValidation(this);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return ParentRateLine?.Master != null && !ParentRateLine.Master.EH_AreRateLinesOverridden;
		}
	}
}
