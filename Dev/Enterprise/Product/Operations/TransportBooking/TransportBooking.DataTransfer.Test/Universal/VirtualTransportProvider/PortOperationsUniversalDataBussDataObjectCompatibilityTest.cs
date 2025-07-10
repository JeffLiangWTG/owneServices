using System.Reflection;
using CargoWise.ComponentModel;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.TransportBookings.DataTransfer.Universal.VirtualTransportProvider.Testing
{
	sealed class PortOperationsUniversalDataBussDataObjectCompatibilityTest : TestCase
	{
		public void TestMaxLength()
		{
			AssertMaxLengthFor<AdditionalReference>(
				new PropertyDetail
				{
					Name = nameof(AdditionalReference.ReferenceNumber),
					MaxLength = 35
				}
			);

			AssertMaxLengthFor<ContainerType>(
				new PropertyDetail
				{
					Name = nameof(ContainerType.ISOCode),
					MaxLength = 4
				}
			);

			AssertMaxLengthFor<CodeDescriptionPair>(
				new PropertyDetail
				{
					Name = nameof(CodeDescriptionPair.Code),
					MaxLength = 3
				}
			);

			AssertMaxLengthFor<Commodity>(
				new PropertyDetail
				{
					Name = nameof(Commodity.Code),
					MaxLength = 4
				}
			);

			AssertMaxLengthFor<Container>(new[]
			{
				new PropertyDetail
				{
					Name = nameof(Container.ContainerNumber),
					MaxLength = 20
				},
				new PropertyDetail
				{
					Name = nameof(Container.Seal),
					MaxLength = 20
				},
			});
			
			AssertMaxLengthFor<OrganizationAddress>(new[]
			{
				new PropertyDetail
				{
					Name = nameof(OrganizationAddress.CompanyName),
					MaxLength = 200
				},
				new PropertyDetail
				{
					Name = nameof(OrganizationAddress.Address1),
					MaxLength = 50
				},
				new PropertyDetail
				{
					Name = nameof(OrganizationAddress.Address2),
					MaxLength = 50
				},
				new PropertyDetail
				{
					Name = nameof(OrganizationAddress.City),
					MaxLength = 50
				},
				new PropertyDetail
				{
					Name = nameof(OrganizationAddress.Postcode),
					MaxLength = 10
				},
			});

			AssertMaxLengthFor<OrganizationAddressState>(
				new PropertyDetail
				{
					Name = nameof(OrganizationAddressState.Code),
					MaxLength = 25
				}
			);

			AssertMaxLengthFor<DropMode>(
				new PropertyDetail
				{
					Name = nameof(DropMode.Code),
					MaxLength = 3
				}
			);

			AssertMaxLengthFor<CodeDescriptionPair>(
				new PropertyDetail
				{
					Name = nameof(CodeDescriptionPair.Code),
					MaxLength = 3
				}
			);
		}

		struct PropertyDetail
		{
			public string Name { get; set; }
			public int MaxLength { get; set; }
		}

		void AssertMaxLengthFor<T>(params PropertyDetail[] propertiesToTest) where T : IDataObject
		{
			foreach (var testProperty in propertiesToTest)
			{
				var propertyInfo = typeof(T).GetProperty(testProperty.Name, BindingFlags.Public | BindingFlags.Instance);

				if (propertyInfo == null)
				{
					Fail($"Property {testProperty.Name} does not exist on {typeof(T).Name}");
				}

				var attribute = propertyInfo.GetCustomAttribute<MaxLengthAttribute>();

				if (attribute == null)
				{
					Fail($"Property Name: [{testProperty.Name}] is missing the {nameof(MaxLengthAttribute)}");
				}

				AssertEquals($"Property {testProperty.Name} Expected Max Length: [{testProperty.MaxLength}] Actual Max Length of Xml Property: [{attribute.MaxLength}]", testProperty.MaxLength, attribute.MaxLength);
			}
		}
	}
}
