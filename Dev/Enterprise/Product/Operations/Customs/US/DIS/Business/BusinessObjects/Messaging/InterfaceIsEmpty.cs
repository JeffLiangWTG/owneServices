using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.DIS.Business
{
	public static class InterfaceIsEmpty
	{
		public static bool IsEmpty(this IDISData record)
		{
			if (record != null)
			{
				foreach (var typeOfInterface in record.GetType().GetInterfaces())
				{
					var properties = typeOfInterface.GetProperties();

					foreach (PropertyInfo info in properties)
					{
						var propertyType = info.PropertyType;
						var interfaces = propertyType.GetInterfaces();
						var value = info.GetValue(record, null);

						if (interfaces.Contains(typeof(IEnumerable)))
						{
							if (value != null)
							{
								var enumerator = ((IEnumerable)value).GetEnumerator();

								while (enumerator.MoveNext())
								{
									var element = enumerator.Current;

									var interfacesOfElement = element.GetType().GetInterfaces();

									if (!IsEmptyCore(element, interfacesOfElement))
									{
										return false;
									}
								}
							}
						}
						else if (!IsEmptyCore(value, interfaces))
						{
							return false;
						}
					}
				}
			}

			return true;
		}

		static bool IsEmptyCore(object value, Type[] interfaces)
		{
			if (interfaces.Contains(typeof(IZType)))
			{
				if (!((IZType)value).IsEmpty)
				{
					return false;
				}
			}
			else if (interfaces.Contains(typeof(IDISData)))
			{
				if (!IsEmpty((IDISData)value))
				{
					return false;
				}
			}
			return true;
		}
	}
}
