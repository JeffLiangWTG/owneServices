using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NL.Business;

[XmlSerializerAssembly("Enterprise.Customs.NL.Business.XmlSerializers")]
[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
public class CalCalculationMethodRegistry : AutoCalCalculationMethodRegistry
{
	public CalCalculationMethodRegistry()
		: base()
	{
	}

	public CalCalculationMethodRegistry(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		: base(fallbackLevel, factory)
	{
	}

	public const decimal BasedOnDutiesVatValue = 0;
	public const bool BasedOnDutiesVatDefault = true;

	public const decimal BasedOnWeightValue = 0;
	public const bool BasedOnWeightDefault = false;

	public const decimal DefaultAmountPerEntryValue = 0;
	public const bool DefaultAmountPerEntryDefault = false;

	#region Overrides

	public override ZString CalculationMethodDescription => new CalculationMethodList().GetDescriptionFromCode(CalculationMethodName);

	protected override bool CalculationMethodValue_ReadOnly => CalculationMethodName == CalculationMethodList.Codes.DUT || CalculationMethodName == CalculationMethodList.Codes.WGT;

	public override void ValidateCalculationMethodValue()
	{
		base.ValidateCalculationMethodValue();
		if (!CalculationMethodValue_ReadOnly)
		{
			MandatoryValidation.MessageErrorIfNotEntered(CalculationMethodValueInfo);
		}
	}

	protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new CalCalculationMethodRegistry(fallbackLevel, factory);

	protected sealed override void WriteElements(XmlWriter writer)
	{
		base.WriteElements(writer);
		writer.WriteElementString(Schema.CalculationMethodName, CalculationMethodName);
		writer.WriteElementString(Schema.CalculationMethodValue, CalculationMethodValue.ToString());
		writer.WriteElementString(Schema.CalculationMethodDefault, CalculationMethodDefault.ToString());
	}

	protected override void ReadElements(XmlReaderWrapper reader)
	{
		CalculationMethodName = reader.ReadElementString(Schema.CalculationMethodName);
		CalculationMethodValue = reader.ReadElementStringAsZDecimal(Schema.CalculationMethodValue);
		CalculationMethodDefault = reader.ReadElementStringAsZBool(Schema.CalculationMethodDefault);
	}

	#endregion
}

