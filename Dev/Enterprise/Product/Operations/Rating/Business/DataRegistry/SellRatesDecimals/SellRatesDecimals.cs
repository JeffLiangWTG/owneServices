using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	public static class NumberOfDecimals
	{
		public const string Zero = "0";
		public const string One = "1";
		public const string Two = "2";
		public const string Three = "3";
		public const string Four = "4";
	}

	public class SellRatesDecimalsList : CodeDescriptionPairList
	{
		public SellRatesDecimalsList()
		{
			AddPair(NumberOfDecimals.Zero, "0");
			AddPair(NumberOfDecimals.One, "0.0");
			AddPair(NumberOfDecimals.Two, "0.00");
			AddPair(NumberOfDecimals.Three, "0.000");
			AddPair(NumberOfDecimals.Four, "0.0000");
		}
	}

	[XmlSerializerAssembly("Enterprise.Rating.Business.XmlSerializers")]
	public class SellRatesDecimals : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Code = "Code";
			public const string Description = "Description";
			public const string Decimals = "Decimals";

			public const int CodeMaxLength = 3;
			public const int DescriptionMaxLength = 100;
			public const int DecimalsMaxLength = 1;
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SellRatesDecimals();
		}

		[MaxLength(SellRatesDecimals.Schema.CodeMaxLength)]
		[List("RateCategoriesAndGroupsList")]
		[ResourceStringData("SellRatesDecimalsControl|0307fa99-6c33-4c5c-92b4-187ccb46db52", Caption = "Rate")]
		public ZString Code
		{
			get { return fCode; }
			set
			{
				CheckMaximumLength(CodeInfo, value);
				fCode = value;
				CodeInfo.RefreshBinding();
				ValidateCode();
			}
		}

		ZString fCode;

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(SellRatesDecimals.Schema.Code); }
		}

		public void ValidateCode()
		{
			if (!IsValidationSuspended)
			{
				CodeInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(CodeInfo);
				ListValidation.ErrorIfInvalidCode(CodeInfo, RateCategoriesAndGroupsList);
			}
		}

		[ResourceStringData("SellRatesDecimalsControl|f57e62f2-cb06-4e41-a18a-f245fa0124dc", Caption = "Rate Type")]
		public ZString Description
		{
			get { return Res.GetString("16999f50-70b4-43c7-b997-c2aa0756bd88", "Number of decimals allowed for {0}", RateCategoriesAndGroupsList.GetDescriptionFromCode(Code)); }
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(SellRatesDecimals.Schema.Description); }
		}

		[MaxLength(SellRatesDecimals.Schema.DecimalsMaxLength)]
		[List("DecimalsList")]
		[ResourceStringData("SellRatesDecimalsControl|04ac5f28-272e-400e-90d7-5766c354cfc3", Caption = "Number of decimals")]
		public ZString Decimals
		{
			get { return fDecimals; }
			set
			{
				CheckMaximumLength(DecimalsInfo, value);
				fDecimals = value;
				DecimalsInfo.RefreshBinding();
				ValidateDecimals();
			}
		}

		ZString fDecimals;

		public ZPropertyInfo DecimalsInfo
		{
			get { return GetZPropertyInfo(SellRatesDecimals.Schema.Decimals); }
		}

		public void ValidateDecimals()
		{
			if (!IsValidationSuspended)
			{
				DecimalsInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(DecimalsInfo);
				ListValidation.ErrorIfInvalidCode(DecimalsInfo, DecimalsList);
			}
		}

		public void SetDefaults(string code)
		{
			Code = code;
			Decimals = NumberOfDecimals.Four;
		}

		public SellRatesDecimalsList DecimalsList
		{
			get { return new SellRatesDecimalsList(); }
		}

		public CodeDescriptionPairList RateCategoriesAndGroupsList
		{
			get { return RateCategoriesList.GetRateCategoriesAndGroupsList(); }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCode();
			ValidateDecimals();
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Code, Code);
			writer.WriteElementString(Schema.Decimals, Decimals);
		}

		protected override void ReadElements(XmlReaderWrapper wrapper)
		{
			Code = wrapper.ReadElementString(Schema.Code);
			SetDefaults(Code);

			XmlReader reader = wrapper.Reader;
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				switch (reader.LocalName)
				{
					case Schema.Decimals:
						Decimals = reader.ReadElementString();
						break;

					default:
						reader.ReadElementString();
						break;
				}
			}
		}

		#endregion
	}
}

