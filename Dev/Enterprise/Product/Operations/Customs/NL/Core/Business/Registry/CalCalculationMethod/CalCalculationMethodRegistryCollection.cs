using System;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NL.Business;

[XmlSerializerAssembly("Enterprise.Customs.NL.Business.XmlSerializers")]
public class CalCalculationMethodRegistryCollection : RegistryBusinessObjectCollectionTemplate
{
	public CalCalculationMethodRegistryCollection()
	{
	}

	public CalCalculationMethodRegistryCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		: base(fallbackLevel, factory)
	{
	}

	public static CalCalculationMethodRegistryCollection DefaultCollection => new CalCalculationMethodRegistryCollection
	{
		new CalCalculationMethodRegistry { CalculationMethodName = CalculationMethodList.Codes.DUT, CalculationMethodValue = CalCalculationMethodRegistry.BasedOnDutiesVatValue, CalculationMethodDefault = CalCalculationMethodRegistry.BasedOnDutiesVatDefault },
		new CalCalculationMethodRegistry { CalculationMethodName = CalculationMethodList.Codes.WGT, CalculationMethodValue = CalCalculationMethodRegistry.BasedOnWeightValue, CalculationMethodDefault = CalCalculationMethodRegistry.BasedOnWeightDefault },
		new CalCalculationMethodRegistry { CalculationMethodName = CalculationMethodList.Codes.DEF, CalculationMethodValue = CalCalculationMethodRegistry.DefaultAmountPerEntryValue, CalculationMethodDefault = CalCalculationMethodRegistry.DefaultAmountPerEntryDefault },
	};

	public new CalCalculationMethodRegistry this[int i] => (CalCalculationMethodRegistry)Elements[i];

	public new CalCalculationMethodRegistry AddNew() => (CalCalculationMethodRegistry)base.AddNew();

	public override void Add(BusinessObject businessObject)
	{
		var methodRegistry = (CalCalculationMethodRegistry)businessObject;
		methodRegistry.CalculationMethodDefaultInfo.ValueChanged += CalculationMethodDefault_ValueChanged;
		base.Add(methodRegistry);
	}

	public override void RemoveAndDelete(BusinessObject elementToDelete)
	{
		var methodRegistry = (CalCalculationMethodRegistry)elementToDelete;
		methodRegistry.CalculationMethodDefaultInfo.ValueChanged -= CalculationMethodDefault_ValueChanged;
		base.RemoveAndDelete(methodRegistry);
	}

	void CalculationMethodDefault_ValueChanged(object sender, EventArgs e)
	{
		if (sender is CalCalculationMethodRegistry methodRegistry)
		{
			if (methodRegistry.CalculationMethodDefault)
			{
				var element = this.Cast<CalCalculationMethodRegistry>().FirstOrDefault(c => c.CalculationMethodName != methodRegistry.CalculationMethodName && c.CalculationMethodDefault);
				if (element != null)
				{
					element.CalculationMethodDefault = ZBool.False;
				}
				else if (methodRegistry.HasNotifications())
				{
					this.Cast<CalCalculationMethodRegistry>().ForEach(c => c.CalculationMethodDefaultInfo.ClearAllNotifications());
				}
			}
		}
	}

	protected override BusinessObject CreateNonPersistentBusinessObject() => new CalCalculationMethodRegistry(CurrentFallbackLevel, CurrentFactory);

	protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new CalCalculationMethodRegistryCollection(fallbackLevel, factory);

	protected override bool AllowNewCore => false;

	protected override bool AllowRemoveCore => false;

	protected override bool RunPreSaveValidationCore()
	{
		if (!this.Cast<CalCalculationMethodRegistry>().Any(c => c.CalculationMethodDefault))
		{
			this.Cast<CalCalculationMethodRegistry>().ForEach(c => c.CalculationMethodDefaultInfo.AddError(CalculationMethodDefaultError));
		}
		return base.RunPreSaveValidationCore();
	}

	public static MultilingualString CalculationMethodDefaultError = ResString.GetMultilingualString("F86330D9-DC01-4B67-8EE3-BDBD4A4AEDCA", "You must select a default.");
}
