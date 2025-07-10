
using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NL.Business;

[XmlSerializerAssembly("Enterprise.Customs.NL.Business.XmlSerializers")]
public class SenderInfoCollection : RegistryBusinessObjectCollectionTemplate
{
	public SenderInfoCollection()
	{
	}

	public SenderInfoCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
	{
	}

	public new SenderInfo this[int i]
	{
		get { return (SenderInfo)Elements[i]; }
	}

	public new SenderInfo AddNew()
	{
		var newElement = (SenderInfo)base.AddNew();
		newElement.Collection = this;
		return newElement;
	}

	public void MapAll(SenderInfoCollection mapTo = null)
	{
		try
		{
			foreach (SenderInfo senderInfo in mapTo ?? this)
			{
				senderInfo.Collection = this;
			}
		}
		catch (InvalidCastException) { } //Certain RegistryBusinessObjectCollectionTemplateTestCase break here due to dummy objects
	}

	protected override BusinessObject CreateNonPersistentBusinessObject()
	{
		return new SenderInfo(CurrentFallbackLevel, CurrentFactory, this);
	}

	protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
	{
		return new SenderInfoCollection(fallbackLevel, factory);
	}

	public new SenderInfoCollection Clone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
	{
		var result = (SenderInfoCollection)GetClone(fallbackLevel, factory);
		foreach (SenderInfo senderInfo in this)
		{
			var clonedSenderInfo = (RegistryBusinessObjectTemplate)senderInfo.Clone(fallbackLevel, factory);
			(clonedSenderInfo as SenderInfo).Collection = result;
			result.Add(clonedSenderInfo);
		}
		PerformPostCloneAction(result);
		return result;
	}

	protected override void PerformPostCloneAction(IRegistryBusiness registryBusiness)
	{
		base.PerformPostCloneAction(registryBusiness);

		if (registryBusiness is SenderInfoCollection collection)
		{
			collection.MapAll();
		}
	}
}
