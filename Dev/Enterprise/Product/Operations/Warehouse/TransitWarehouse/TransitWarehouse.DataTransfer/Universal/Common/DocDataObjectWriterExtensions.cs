using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Document;
using Country = Enterprise.UniversalDataBuss.DataObjects.Universal.Country;

namespace Enterprise.Warehouse.Transit.DataTransfer.Document
{
	static class DocDataObjectWriterExtensions
	{
		#region AddDataProvider

		public static UniversalDataBuss.DataObjects.Universal._2012_11.DataContext AddDataProvider(this UniversalDataBuss.DataObjects.Universal._2012_11.DataContext dataContext)
		{
			if (dataContext == null)
			{
				return null;
			}

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var enterpriseCode = registrationKey.EnterpriseCode;
			var serverCode = registrationKey.ServerCode;

			var dataProvider = new UniversalDataBuss.DataObjects.Universal._2012_11.DataProvider()
			{
				Code = enterpriseCode + serverCode + GlbCompany.CurrentCompany.GC_Code,
				Type = UniversalDataBuss.DataObjects.Universal._2012_11.DataProviderType.EnterpriseID
			};

			if (dataContext.DataSource == null)
			{
				dataContext.DataSource = new UniversalDataBuss.DataObjects.Universal._2012_11.DataSource();
			}

			dataContext.DataSource.DataProvider = dataProvider;

			if (dataContext is IDataContextDataObject dataContextDataObject)
			{
				dataContextDataObject.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			}

			return dataContext;
		}

		#endregion

		#region CreateUXmlDataContext

		public static UniversalDataBuss.DataObjects.Universal._2012_11.DataContext CreateUXmlDataContext(this IDataSourceProvider dataSourceProvider)
		{
			var res = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext
			{
				DataSource = new UniversalDataBuss.DataObjects.Universal._2012_11.DataSource
				{
					Type = dataSourceProvider.SourceType,
					Key = dataSourceProvider.SourceID
				}
			};

			return res;
		}

		public static UniversalDataBuss.DataObjects.Universal._2012_11.DataContext CreateUXmlDataContext(ZString sourceType, ZString sourceID)
		{
			var res = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext
			{
				DataSource = new UniversalDataBuss.DataObjects.Universal._2012_11.DataSource
				{
					Type = sourceType,
					Key = sourceID
				}
			};

			return res;
		}

		#endregion

		#region AddUserBranchAndDepartment

		public static UniversalDataBuss.DataObjects.Universal._2012_11.DataContext AddUserBranchAndDepartment(this UniversalDataBuss.DataObjects.Universal._2012_11.DataContext dataContext)
		{
			if (dataContext.Workflow == null)
			{
				dataContext.Workflow = new UniversalDataBuss.DataObjects.Universal._2012_11.Workflow();
			}

			dataContext.Workflow.EventUser = new Staff() { Code = GlbStaff.CurrentUser.GS_Code, Name = GlbStaff.CurrentUser.GS_FullName };
			dataContext.Workflow.EventBranch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code, Name = GlbBranch.CurrentBranch.GB_BranchName };
			dataContext.Workflow.EventDepartment = new Department { Code = GlbDepartment.CurrentDepartment.GE_Code, Name = GlbDepartment.CurrentDepartment.HumanReadableName };

			return dataContext;
		}

		#endregion

		#region ToUXmlOrganizationAddress

		public static OrganizationAddress ToUXmlOrganizationAddress(this IAddress source, string addressType, IDataObjectWriterStrategy strategy, IEnumerable<UniversalDataBuss.DataObjects.Universal.RegistrationNumber> overridenRegistrationNumbers = null, bool isUpperCase = false)
		{
			if (source == null || string.IsNullOrWhiteSpace(source.CompanyName))
			{
				return null;
			}

			var address = new OrganizationAddress(strategy);
			address.CompanyName = isUpperCase ? source.CompanyName.ToUpperInvariant() : source.CompanyName;
			address.AddressType = addressType;
			address.Address1 = isUpperCase ? source.AddressLine1.ToUpperInvariant() : source.AddressLine1;
			address.Address2 = isUpperCase ? source.AddressLine2.ToUpperInvariant() : source.AddressLine2;
			address.AdditionalAddressInformation = source.AdditionalAddressInformation;
			address.GovRegNum = source.TaxNumber;

			if (source.TaxNumberType != null
				&& !source.TaxNumberType.Code.IsEmpty)
			{
				address.GovRegNumType = new RegistrationNumberType
				{
					Code = source.TaxNumberType.Code,
					Description = source.TaxNumberType.Description
				};
			}

			address.AddressOverride = ZBool.False;
			address.City = isUpperCase ? source.City.ToUpperInvariant() : source.City;
			address.Postcode = isUpperCase ? source.Postcode.ToUpperInvariant() : source.Postcode;
			address.State = isUpperCase ? source.State.ToUpperInvariant() : source.State;
			address.Country = source.Country?.ToUXmlCountry(isUpperCase);
			address.Port = source.Unloco?.ToUXmlUnloco();

			address.Contact = source.Contact;
			address.Email = source.Email;
			address.Fax = source.Fax;
			address.Phone = source.Phone;

			if (overridenRegistrationNumbers != null)
			{
				if (overridenRegistrationNumbers.Any())
				{
					address.SetRegistrationNumberCollection(() => overridenRegistrationNumbers.ToList());
				}
			}
			else if (source.RegistrationNumbers?.Count > 0)
			{
				var regNumbers = source
					.RegistrationNumbers
					.Select(ToUXmlRegistrationNumber)
					.ToList();

				address.SetRegistrationNumberCollection(() => regNumbers);
			}

			return address;
		}

		#endregion

		#region ToUXmlRegistrationNumber

		public static UniversalDataBuss.DataObjects.Universal.RegistrationNumber ToUXmlRegistrationNumber(this IRegistrationNumber source)
		{
			if (source == null)
			{
				return null;
			}

			var regNumber = new UniversalDataBuss.DataObjects.Universal.RegistrationNumber();
			regNumber.Type = new RegistrationNumberType
			{
				Code = source.Type?.Code,
				Description = source.Type?.Description
			};
			regNumber.CountryOfIssue = new Country
			{
				Code = source.CountryOfIssue?.Code,
				Name = source.CountryOfIssue?.Name
			};
			regNumber.Value = source.Value;

			return regNumber;
		}

		#endregion

		#region ToUXmlCountry

		public static Country ToUXmlCountry(this ICountry source, bool isUpperCase = false)
		{
			if (source == null)
			{
				return null;
			}

			return new Country
			{
				Code = isUpperCase ? source.Code.ToUpperInvariant() : source.Code,
				Name = source.Name
			};
		}

		#endregion

		#region ToUXmlUnloco

		public static UNLOCO ToUXmlUnloco(this IUnloco source)
		{
			if (source == null)
			{
				return null;
			}

			return new UNLOCO
			{
				Code = source.Code,
				Name = source.Name
			};
		}

		#endregion
	}
}
