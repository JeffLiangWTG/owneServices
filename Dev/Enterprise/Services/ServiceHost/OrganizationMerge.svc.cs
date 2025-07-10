using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Threading;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Utilities.Environment;

namespace Enterprise.Services.ServiceHost
{
	#region SuppressResourceStringsCheckRegion

	// NOTE: If you change the class name "OrganizationMergeService" here, you must also update the reference to "OrganizationMergeService" in Web.config.
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	[ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.PerCall)]
	public class OrganizationMerge : IOrganizationMerge
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public OrganizationMerge()
		{
			Log("Constructor", "", MethodStart);
			InitialiseWebService();
			SetWebAssemblyLoader();
			Log("Constructor", "", MethodEnd);
		}

		#region Initialization

		void InitialiseWebService()
		{
			try
			{
				Log("InitialiseWebService", "", MethodStart);
				Globals.IsUserInteractive = false;
				WebInitialiser.Initialise(enableErrorReport: false);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Log("InitialiseWebService", ex.Message, Error);
				throw;
			}
			Log("InitialiseWebService", "", MethodEnd);
		}

		ILicensedComponent CoreLicenceComponent
		{
			get { return fCoreLicensedComponent ?? (fCoreLicensedComponent = new CoreLicensedComponent()); }
		}
		CoreLicensedComponent fCoreLicensedComponent;

		class CoreLicensedComponent : ILicensedComponent
		{
			public LicensedComponentManager LicensedComponentManager
			{
				get { return ((ILicensedComponent)this).LicensedComponentManager as LicensedComponentManager; }
			}

			IDisposable ILicensedComponent.LicensedComponentManager
			{
				get { return fLicensedComponentManager ?? (fLicensedComponentManager = new LicensedComponentManager(this)); }
			}

			LicensedComponentManager fLicensedComponentManager;
		}

		protected virtual void SetDefaultAssemblyLoader()
		{
			Log("SetDefaultAssemblyLoader", "", MethodStart);

			var originalProvider = Env.GetCurrentProvider();
			new WinFormsEnvironmentProvider().Enable();
			AssemblyLoader.Instance = new DefaultAssemblyLoader();

			if (originalProvider != null)
			{
				originalProvider.Dispose();
			}

			Log("SetDefaultAssemblyLoader", "", MethodEnd);
		}

		protected virtual void SetWebAssemblyLoader()
		{
			Log("SetWebAssemblyLoader", "", MethodStart);

			var originalProvider = Env.GetCurrentProvider();
			new WebServiceEnvironmentProvider().Enable();
			AssemblyLoader.Instance = new WebAssemblyLoader();

			if (originalProvider != null)
			{
				originalProvider.Dispose();
			}

			Log("SetWebAssemblyLoader", "", MethodEnd);
		}

		#endregion

		#region Logging

		// To enable logging add following code after </system.web> in web.config
		//    <system.diagnostics>
		//		<trace autoflush="true">
		//			<listeners>
		//				<add name="OrgMergeListener" type="System.Diagnostics.TextWriterTraceListener" initializeData="c:\TextWriterOutput.log" />
		//				<remove name="Default" />
		//			</listeners>
		//		</trace>
		//	</system.diagnostics>

		const string Input1 = "Input ->Service";
		const string Input2 = "Input ->Business";
		const string Output1 = "Output ->Service";
		const string Output2 = "Method Output";
		const string InternalData = "Internal Data";
		const string InternalProcess = "Internal Process";
		const string MethodStart = "Method Start";
		const string MethodEnd = "Method End";
		const string Error = "Error";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		void Log(string parameter, string message, string category)
		{
			Trace.WriteLineIf(Trace.Listeners.Count > 0, DateTime.UtcNow.ToString(Culture.Invariant) + ", " + parameter + ": " + (message ?? "null"), category);
		}

		void Log(string parameter, object item, string category)
		{
			var message = item == null ? "null" : item.ToString();
			Log(parameter, message, category);
		}

		#endregion

		#region Merging

		internal string Merge(string[] oldOrgCodes, string newOrgCode)
		{
			string result = string.Empty;
			try
			{
				Log("Merge", "", MethodStart);
				Log("oldOrgCodes", string.Join(",", oldOrgCodes), Input1);
				Log("newOrgCode", newOrgCode, Input1);

				var orgMergingCheckpoint = Env.Security.OrgDuplicateDetectionMerge;

				if (orgMergingCheckpoint.IsAllowed)
				{
					Log("Merging Allowed", bool.TrueString, InternalProcess);
					Log("Process Call", string.Join(",", oldOrgCodes), Input2);
					Log("Process Call", newOrgCode, Input2);
					result = new ManyToOneMerger(oldOrgCodes, newOrgCode).Process();
					Log("Process Result", result, Output1);
				}
				else
				{
					Log("Merging Allowed", bool.FalseString, InternalProcess);
					result = orgMergingCheckpoint.ErrorMessageForNotAllowed;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Log("Merge", ex.Message, Error);
				throw;
			}
			Log("Merge Result", result, Output2);
			Log("Merge", "", MethodEnd);
			return result;
		}

		internal string MergeFromXml(string xml)
		{
			string result = string.Empty;
			try
			{
				Log("MergeFromXml", "", MethodStart);
				Log("xml", xml, Input1);
				List<string> responses = new List<string>();
				Dictionary<string, string[]> codes = ParseXml(xml);

				if (codes == null)
				{
					Log("ParseXml", "null", InternalProcess);
					responses.Add("Xml is in incorrect format.");
				}
				else
				{
					foreach (var entry in codes)
					{
						Log("New Organization (from xml)", entry.Key, InternalData);
						Log("Old Organizations (from xml)", string.Join(",", entry.Value), InternalData);
						string mergeResult = Merge(entry.Value, entry.Key);
						responses.Add(mergeResult);
						Log("Merge Call Result", mergeResult, InternalProcess);
					}
				}
				result = string.Join("\r\n", responses.ToArray());
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Log("MergeFromXml", ex.Message, Error);
				throw;
			}
			Log("MergeFromXml", result, Output2);
			Log("MergeFromXml", "", MethodEnd);
			return result;
		}

		public Dictionary<string, string[]> ParseXml(string xml)
		{
			Dictionary<string, string[]> result = new Dictionary<string, string[]>();
			try
			{
				Log("ParseXml", "", MethodStart);
				Log("xml", xml, Input1);
				if (string.IsNullOrEmpty(xml))
				{
					result = null;
				}
				else
				{
					XmlDocument doc = new XmlDocument();
					bool fail = false;
					try
					{
						Log("LoadXml", xml, Input2);
						doc.LoadXml(xml);
						Log("LoadXml", "success", InternalProcess);
					}
					catch (XmlException ex)
					{
						Log("LoadXml", ex.Message, InternalProcess);
						result = null;
						fail = true;
					}

					if (!fail)
					{
						XmlNodeList newOrgs = doc.GetElementsByTagName("MergeInto");
						Log("newOrgs count", newOrgs.Count.ToString(CultureInfo.InvariantCulture), InternalData);
						foreach (XmlNode org in newOrgs)
						{
							string newOrgCode = org.Attributes["Value"].Value;
							Log("newOrgCode", newOrgCode, InternalData);
							List<string> oldOrgCodes = new List<string>();
							Log("org.ChildNodes Count", org.ChildNodes.Count.ToString(CultureInfo.InvariantCulture), InternalData);
							foreach (XmlNode oldOrg in org.ChildNodes)
							{
								Log("oldOrg node name (should be MergeFrom)", oldOrg.Name, InternalData);
								if (oldOrg.Name == "MergeFrom")
								{
									Log("Actual Org Code", oldOrg.InnerText, InternalData);
									if (!string.IsNullOrEmpty(oldOrg.InnerText))
									{
										oldOrgCodes.Add(oldOrg.InnerText);
									}
								}
							}
							if (oldOrgCodes.Count > 0)
							{
								result.Add(newOrgCode, oldOrgCodes.ToArray());
							}
						}
					}
				}
				if (result != null)
				{
					foreach (var entry in result)
					{
						Log("New Organization", entry.Key, Output2);
						Log("Old Organizations", string.Join(",", entry.Value), Output2);
					}
				}
				else
				{
					Log("ParseXml", null, Output2);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Log("ParseXml", ex.Message, Error);
				throw;
			}
			Log("ParseXml", "", MethodEnd);
			return result;
		}

		#endregion

		#region logging in

		protected Environment.UserContext GetLogin(string username, string password)
		{
			Environment.UserContext result;
			try
			{
				Log("GetLogin", "", MethodStart);
				Log("username", username, Input1);

				result = GetUserContext(username, password);
			}
			catch (FormatException ex)
			{
				Log("GetLogin", ex.Message, InternalProcess);
				result = null;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Log("GetLogin", ex.Message, Error);
				throw;
			}

			Log("User Context", result, Output2);
			Log("GetLogin", "", MethodEnd);

			return result;
		}

		Environment.UserContext GetUserContext(string login, string password)
		{
			Environment.UserContext result = null;
			try
			{
				Log("GetUserContext", "", MethodStart);
				Log("GetLogin", login, Input1);
				Log("Password", password, Input1);

				login = DecryptData(login);
				Log("Decrypted GetLogin", login, InternalData);
				password = DecryptData(password);

				var branchPK = EnvProxy.Instance.Registry.WebBranch;
				var departmentPK = EnvProxy.Instance.Registry.WebDepartment;

				Log("BranchPK", branchPK.ToString(), InternalData);
				Log("DepartmentPK", departmentPK.ToString(), InternalData);

				var loginController = (IUserLoginController)Activator.CreateInstance(ObjectFactory.GetType<IUserLoginController>());

				var auth = loginController.ValidateUserLoginAndPassword(login, password);
				if (auth.LoginValidated)
				{
					Log("ValidateUserLoginAndPassword", bool.TrueString, InternalProcess);

					SetDefaultAssemblyLoader();
					var userContext = new Environment.UserContext(auth.User, branchPK, departmentPK, loginAuthenticationInfo: auth);
					SetWebAssemblyLoader();

					if (userContext.Licence.Core.Login(CoreLicenceComponent) != LicenceLoginResponse.Denied)
					{
						userContext.Licence.Core.Logout(CoreLicenceComponent);
						Log("Licence GetLogin", bool.TrueString, InternalProcess);
						result = userContext;
						Log("Set User Context", bool.TrueString, InternalProcess);
					}
					else
					{
						Log("Licence GetLogin", bool.FalseString, InternalProcess);
					}
				}
				else
				{
					Log("Validate User GetLogin And Password", bool.FalseString, InternalProcess);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Log("GetUserContext", ex.Message, Error);
				throw;
			}

			Log("Setup User Context", result, Output2);
			Log("GetUserContext", "", MethodEnd);

			return result;
		}

		#endregion

		#region IOrganizationMergeService Members

		public string MergeFromXmlAsUser(string username, string password, string xml)
		{
			return RunAsUser(username, password, () => MergeFromXml(xml));
		}

		public string MergeAsUser(string username, string password, string[] oldOrgCodes, string newOrgCode)
		{
			return RunAsUser(username, password, () => Merge(oldOrgCodes, newOrgCode));
		}

		protected string RunAsUser(string username, string password, Func<string> method)
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (Monitor.TryEnter(mutex, TimeSpan.FromSeconds(1)))
				{
					try
					{
						var userContext = GetLogin(username, password);
						if (userContext != null)
						{
							using (Env.SetTemporaryUserContext(userContext))
							{
								return method();
							}
						}
						else
						{
							return "Could not log in with the given credentials";
						}
					}
					finally
					{
						Monitor.Exit(mutex);
					}
				}
				else
				{
					return "Another user is currently using this service. Please try again later.";
				}
			}
		}

		static readonly object mutex = new object();

		#endregion

		#region Cryptography

		RSACryptoServiceProvider rsaProvider;
		RSACryptoServiceProvider RsaProvider
		{
			get { return rsaProvider ?? (rsaProvider = new RSACryptoServiceProvider()); }
		}

		protected virtual string DecryptData(string data)
		{
			byte[] databytes = Convert.FromBase64String(data);
			using (StreamReader reader = new StreamReader(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "privatekey.xml")))
			{
				string publicPrivateKeyXML = reader.ReadToEnd();
				RsaProvider.FromXmlString(publicPrivateKeyXML);

				//read ciphertext, decrypt it to plaintext
				byte[] plain = RsaProvider.Decrypt(databytes, false);
				return System.Text.Encoding.UTF8.GetString(plain);
			}
		}

		#endregion
	}

	#endregion
}
