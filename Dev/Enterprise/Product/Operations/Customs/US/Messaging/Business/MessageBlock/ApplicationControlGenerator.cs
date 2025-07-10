using System;
using System.Collections;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Messaging.Business
{
	public abstract class ApplicationControlGenerator<ControlMessageBlockA, ControlMessageBlockZ> : ApplicationControlGenerator
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockZ : MessageBlock, IControlMessageBlockZ, new()
	{
		protected ApplicationControlGenerator(GlbBranch branch)
			: base(branch, new ControlMessageBlockA(), new ControlMessageBlockZ())
		{
		}

		public new ControlMessageBlockA A
		{
			get { return (ControlMessageBlockA)base.A; }
		}

		public new ControlMessageBlockZ Z
		{
			get { return (ControlMessageBlockZ)base.Z; }
		}
	}

	public interface IApplicationControlGeneratorCreator
	{
		ApplicationControlGenerator New(string applicationIdentifier, GlbBranch branch);
	}

	public abstract class ApplicationControlGenerator
	{
		public static ApplicationControlGenerator New(string applicationCode, string applicationIdentifier, GlbBranch branch)
		{
#if DEBUG
			if (applicationCode == CBPEDIInterchange.ApplicationCodeForTesting)
			{
				return Enterprise.Customs.US.Messaging.Business.Testing.ApplicationControlGeneratorTestClass.New(branch);
			}
#endif
			Hashtable types = (Hashtable)ObjectFactory.Get("ApplicationControlGeneratorCreatorTypes");
			ObjectHandle objectHandle = (ObjectHandle)types[applicationCode];
			IApplicationControlGeneratorCreator creator = objectHandle != null ? (IApplicationControlGeneratorCreator)objectHandle.GetObject() : null;

			ApplicationControlGenerator result = null;
			if (creator == null)
			{
				ErrorReporter.ReportOnce("ApplicationControlGeneratorCreator for Application Code '" + applicationCode + "' is unknown", "Cannot determine the ApplicationControlGeneratorCreator object for Application Code '" + applicationCode + "'");
			}
			else
			{
				result = creator.New(applicationIdentifier, branch);
			}
			if (result == null)
			{
				ErrorReporter.ReportOnce("ApplicationControlGenerator for Application Code '" + applicationCode + "' and Application Identifier '" + applicationIdentifier + "' is unknown", "Cannot determine the ApplicationControlGenerator object for Application Code '" + applicationCode + "' and Application Identifier '" + applicationIdentifier + "'");
			}
			return result;
		}

		protected ApplicationControlGenerator(GlbBranch branch, IControlMessageBlockA a, IControlMessageBlockZ z)
		{
			if (branch == null)
			{
				throw new ArgumentNullException(nameof(branch));
			}
			if (a == null)
			{
				throw new ArgumentNullException(nameof(a));
			}
			if (z == null)
			{
				throw new ArgumentNullException(nameof(z));
			}
			A = a;

			builder = new StringBuilder();
			Z = z;
		}
		public readonly IControlMessageBlockA A;
		public readonly IControlMessageBlockZ Z;
		readonly StringBuilder builder;

		public void AddMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
			builder.Append(GetMessageText(BlockPadder.Pad(message.EM_MessageText)));
			AddExtraInfo(message);
		}

		protected virtual ZString GetMessageText(ZString messageText)
		{
			return messageText;
		}

		protected virtual void AddExtraInfo(Enterprise.Messaging.Business.EDIMessage message)
		{
		}

		public string GetBody()
		{
			return builder.ToString();
		}

		internal protected virtual string GetSettingDetails(GlbBranch branch) => "";
	}
}

#if DEBUG
namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	using System.Collections.Generic;

	class ApplicationControlGeneratorTestClass : ApplicationControlGenerator<ZZZA, ZZZZ>
	{
		ApplicationControlGeneratorTestClass(GlbBranch branch)
			: base(branch)
		{
			this.branch = branch;
		}
		readonly GlbBranch branch;

		public static ApplicationControlGeneratorTestClass New(GlbBranch branch)
		{
			ApplicationControlGeneratorTestClass result = null;
			generatorTestClasses?.TryGetValue(branch, out result);
			return result ?? new ApplicationControlGeneratorTestClass(branch);
		}
		[ThreadStatic]
		static Dictionary<GlbBranch, ApplicationControlGeneratorTestClass> generatorTestClasses;

		public class GeneratorDisposable : IDisposable
		{
			public GeneratorDisposable(GlbBranch branch)
			{
				generator = new ApplicationControlGeneratorTestClass(branch);
				generatorTestClasses = generatorTestClasses ?? new Dictionary<GlbBranch, ApplicationControlGeneratorTestClass>();
				generatorTestClasses.Add(branch, generator);
			}
			public readonly ApplicationControlGeneratorTestClass generator;

			public void Dispose()
			{
				generatorTestClasses.Remove(generator.branch);
				if (generatorTestClasses.Count == 0)
				{
					generatorTestClasses = null;
				}
			}
		}

		public static GeneratorDisposable TemporarySetup(GlbBranch branch) => new GeneratorDisposable(branch);

		public string settingDetailsForTesting;
		protected internal override string GetSettingDetails(GlbBranch branch)
		{
			return settingDetailsForTesting ?? base.GetSettingDetails(branch);
		}
	}
}
#endif
