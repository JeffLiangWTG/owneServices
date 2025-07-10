using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobInvoicingEDocsProviderSupporterTest : EDocsProviderSupporterTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1089:DoNotUseAssemblyGetEntryAssemblyAnalyzer", Justification = "Baseline")]
		[RequiresSoftware(RequiredSoftware.VisualStudio)]
		public void TestJobInvoicingPlugInConsumersThatHaveDocumentsUsesThisProvider()
		{
			StringBuilder sb = new StringBuilder();
			string entryDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

			foreach (string assemblyName in BuildXml.Instance.GetAllTestedAssemblies())
			{
				if (IsNotTargetPrefix(assemblyName))
				{
					continue;
				}

				Assembly assembly = Assembly.LoadFrom(Path.Combine(entryDirectory, assemblyName));
				if (!IsClientAssembly(Path.GetFileNameWithoutExtension(assemblyName)))
				{
					foreach (Type type in assembly.GetTypes())
					{
						if (!type.IsAbstract &&
							typeof(IJobInvoicingPlugIn).IsAssignableFrom(type) &&
							typeof(IDocumentSupportable).IsAssignableFrom(type) &&
							!typeof(NonPersistentBusinessObject).IsAssignableFrom(type) &&
							!typeof(DummyWithWorkflow).IsAssignableFrom(type) &&
							IsNotTestClass(type))
						{
							try
							{
								using (IsCountryForEDocsProvider(type, out var countryCode) ? GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode) : DisposableAction.NoAction)
								{
									BusinessObject businessObject = TypesThatNeedNewWithValidTestData.Contains(type.FullName) ? Factory.NewWithValidTestData(type) : Factory.New(type);
									IEDocsProvider provider = businessObject as IEDocsProvider;
									if ((provider == null) || ((provider.GetEDocsProviderSupporter() as JobInvoicingEDocsProviderSupporter) == null))
									{
										sb.AppendLine(type.FullName);
									}
								}
							}
							catch
							{
								sb.AppendLine(type.FullName);
							}
						}
					}
				}
			}

			if (sb.Length > 0)
			{
				Fail("The following types must implement IEDocsProvider and return a JobInvoicingEDocsProviderSupporter:" +
					System.Environment.NewLine + System.Environment.NewLine + sb.ToString());
			}
			else
			{
				AssertionCount++;
			}
		}

		public bool IsNotTargetPrefix(string assemblyName)
		{
#pragma warning disable CS0436 // Type conflicts with imported type - due to InternalsVisibleTo
			bool isNetCoreTargetFrameworkPrefix = assemblyName.StartsWith(CommonAssemblyInfo.CWNetCoreSubfolder, StringComparison.OrdinalIgnoreCase);
#pragma warning restore CS0436 // Type conflicts with imported type
#if NETFRAMEWORK
			return isNetCoreTargetFrameworkPrefix;
#elif NET
			return !isNetCoreTargetFrameworkPrefix;
#else
#error Unexpected target platform
#endif
		}

		static bool IsNotTestClass(Type type)
		{
			const string TestNamespace = ".Test.";
			const string TestingNamespace = ".Testing.";
			var fullName = type.FullName;
			return !fullName.Contains(TestingNamespace) && !fullName.Contains(TestNamespace);
		}

		bool IsCountryForEDocsProvider(Type type, out ZString result)
		{
			if (typeof(FR.IJobDeclaration).IsAssignableFrom(type))
			{
				result = Core.Constants.CountryCodes.France;
			}
			else
			{
				result = default;
			}

			return !result.IsEmpty;
		}

		bool IsClientAssembly(string fileName)
		{
			return ClientHookLoader.Instance.IsAnyClientOverrideAssembly(fileName);
		}

		public void TestConsumersForOldStyleInvoice()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			MenuItemIdentifier[] consumers = Supporter.GetConsumers();
			AssertEquals("GetConsumers().Length", 1, consumers.Length);
			AssertEquals("GetConsumers()[0].BusinessContext", BusinessContext.ARInvoice, consumers[0].BusinessContext);
			AssertEquals("GetConsumers()[0].Name", JobInvoicingEDocsProviderSupporter.OldStyleInvoiceName, consumers[0].Name);
		}

		public void TestConsumersForNewStyleInvoice()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			MenuItemIdentifier[] consumers = Supporter.GetConsumers();
			AssertEquals("GetConsumers().Length", 0, consumers.Length);
		}

		public void TestOldStyleInvoiceMenuItemExists()
		{
			MenuItemIdentifierTest.TestMenuItemExists(BusinessContext.ARInvoice, JobInvoicingEDocsProviderSupporter.OldStyleInvoiceName);
		}

		public void TestDocBuilderInvoiceMenuItemExists()
		{
			MenuItemIdentifierTest.TestMenuItemExists(BusinessContext.ARInvoice, JobInvoicingEDocsProviderSupporter.DocBuilderInvoiceName);
		}

		public void TestClassAInvoiceName()
		{
			MenuItemIdentifierTest.TestMenuItemExists(BusinessContext.ARInvoice, JobInvoicingEDocsProviderSupporter.ClassAInvoiceName);
		}

		#region Implementation

		protected override EDocsProviderSupporter GetNewSupporter(IEDocsProvider eDocsProvider)
		{
			return new JobInvoicingEDocsProviderSupporter(eDocsProvider);
		}

		readonly string[] TypesThatNeedNewWithValidTestData = {
		  "Enterprise.Warehouse.Invoicing.Business.PeriodicInvoicing"
		};

		#endregion
	}
}
