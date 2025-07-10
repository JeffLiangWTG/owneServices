using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	abstract class USOrganisationWrapperTestCase : DocumentWrapperTestCase
	{
		protected OrgHeader Supplier => helper.Supplier;

		protected OrgAddress SupplierAddress => helper.SupplierAddress;

		protected OrgContact SupplierContact => helper.SupplierContact;

		protected USOrganisation USOrganisation => helper.USOrganisation;

		USOrganisationWrapperTestHelper helper;
		protected override void SetUp()
		{
			Env.Registry.SetOrgUsePhoneNumberFormatting(true);
			helper = new USOrganisationWrapperTestHelper(Factory);

			base.SetUp();
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			DocumentWrapper result = null;
			MethodInfo[] methods = GetExpectedBusinessObjectType().GetMethods(BindingFlags.Static | BindingFlags.Public);

			foreach (MethodInfo method in methods)
			{
				if (method.Name == "New" && method.GetParameters().Length <= 2)
				{
					ParameterInfo[] parameters = method.GetParameters();

					if (parameters.Length == 2 && typeof(USOrganisation).IsAssignableFrom(parameters[0].ParameterType) && (parameters[1].ParameterType == typeof(BusinessObjectFactory)))
					{
						USOrganisation wrappedObject = helper.CreateNewUSOrganisation();
						result = (DocumentWrapper)method.Invoke(null, new object[] { wrappedObject, Factory });
					}

					if (result != null)
					{
						break;
					}
				}
			}

			return result;
		}
	}
}
