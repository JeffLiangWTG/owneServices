using System;
using CargoWise.EntityFramework;
namespace Enterprise.MasterFiles.Business
{
	public interface IJobDocAddressOverrideSupporter
	{
		Type ZDocAddressControlType { get; }
		JobDocAddressCollectionForPlugin GetJobDocAddressCollectionForPlugin(BusinessObjectFactory factory);
	}
}
