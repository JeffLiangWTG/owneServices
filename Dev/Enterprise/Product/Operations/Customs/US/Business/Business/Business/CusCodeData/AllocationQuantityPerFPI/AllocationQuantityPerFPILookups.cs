using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class AllocationQuantityPerFPILookups : Customs.Business.CusCodeDataLookups
	{
		public AllocationQuantityPerFPILookups(AllocationQuantityPerFPI parent)
			: base(parent)
		{
		}

		new AllocationQuantityPerFPI Parent => (AllocationQuantityPerFPI)base.Parent;

		public ConsignorCollection Organizations
		{
			get { return new ConsignorCollection(Factory); }
		}

		public CodeDescriptionPairList ForeignProducerIdentifiers
		{
			get
			{
				return Factory.GetCachedValue($"ForeignProducerIdentifiers|{Parent.US_OA_ManufacturerAddress}", () =>
				{
					void GetFPIFromManufacturer(OrgAddressCusCodeCollection customsCodes, ZString fpiCodeType, CodeDescriptionPairList list)
					{
						var fpiNumber = customsCodes.GetCustomsRegNo(fpiCodeType, Core.Constants.CountryCodes.UnitedStates).Left(AllocationQuantityPerFPI.Schema.US_ForeignProducerIdentifierMaxLength);
						if (!fpiNumber.IsEmpty)
						{
							list.AddPairIfNotExist(fpiNumber, fpiNumber);
						}
					}

					var result = new CodeDescriptionPairList();
					if (Parent.ManufacturerAddress is OrgAddress manufacturerAddress)
					{
						GetFPIFromManufacturer(manufacturerAddress.CustomsCodes, OrgCusCode.USACodeTypes.ForeignProducerIdentifierBeer, result);
						GetFPIFromManufacturer(manufacturerAddress.CustomsCodes, OrgCusCode.USACodeTypes.ForeignProducerIdentifierSpirits, result);
						GetFPIFromManufacturer(manufacturerAddress.CustomsCodes, OrgCusCode.USACodeTypes.ForeignProducerIdentifierWine, result);
					}

					return result;
				});
			}
		}
	}
}
