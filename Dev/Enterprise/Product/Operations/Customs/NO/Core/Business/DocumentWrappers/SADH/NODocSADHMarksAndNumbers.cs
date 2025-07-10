using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.NO.Business;

[DefaultField("Value")]
public sealed class NODocSADHMarksAndNumbers : DocBaseWrapper
{
	public NODocSADHMarksAndNumbers(ZString value, BusinessObjectFactory factory) : base(value, factory)
	{
		this.value = Argument.NotNullOrEmpty(value, nameof(value));
	}

	public static NODocSADHMarksAndNumbers New(ZString value, BusinessObjectFactory factory)
	{
		return new NODocSADHMarksAndNumbers(value, factory);
	}

	public static NODocSADHMarksAndNumbers New(object value, BusinessObjectFactory factory)
	{
		return new NODocSADHMarksAndNumbers(value.ToString(), factory);
	}

	public ZString Value => value;

	readonly ZString value;
}
