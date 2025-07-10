using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V97Transform : ITransformStrategy
	{
		static V97Transform v97Transform;

		V97Transform() { }

		public static V97Transform Instance()
		{
			if (v97Transform == null)
			{
				v97Transform = new V97Transform();
			}
			return v97Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 97 && ToBeTransformedTypes.Contains(dataSetType);
		}

		public T Transform<T>(T data)
		{
			if (data is RefCusTariff tariff)
			{
				if (!tariff.Deleted)
				{
					if (tariff.RefCusTariffAdditionalCodes != null)
					{
						foreach (var code in tariff.RefCusTariffAdditionalCodes)
						{
							if (code.RefCusApplicabilities != null && code.RefCusApplicabilities.Length > 0)
							{
								code.RefCusApplicabilities = new RefCusApplicability[0];
							}
						}
					}
				}
			}
			return data;
		}

		public Type[] ToBeTransformedTypes
		{
			get
			{
				return new Type[]
				{
					typeof(RefCusTariff)
				};
			}
		}
	}
}
