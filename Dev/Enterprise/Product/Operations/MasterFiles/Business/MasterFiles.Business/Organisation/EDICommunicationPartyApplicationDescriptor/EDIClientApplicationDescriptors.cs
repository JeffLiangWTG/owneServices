using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;

namespace Enterprise.MasterFiles.Business
{
	public class EDIClientApplicationDescriptors : IEDIClientApplicationDescriptors
	{
		public IEDIClientApplicationDescriptor GetValue(string code)
		{
			IEDIClientApplicationDescriptor descriptor;
			if (!ApplicationDescriptorsDictionaryUnsafe.TryGetValue(code, out descriptor))
			{
				ApplicationDescriptorCreator creator;
				if (ApplicationDescriptorCreatorDictionaryUnsafe.TryGetValue(code, out creator))
				{
					descriptor = GetApplicationDescriptorFromCreatorUnsafe(code, creator);
				}
			}
			return descriptor;
		}

		public IEnumerable<IEDIClientApplicationDescriptor> Values
		{
			get
			{
				var applicationDescriptors = new List<IEDIClientApplicationDescriptor>();
				foreach (var entry in ApplicationDescriptorCreatorDictionaryUnsafe.OrderBy(x => x.Key))
				{
					var descriptor = GetValue(entry.Key);
					if (descriptor != null)
					{
						applicationDescriptors.Add(descriptor);
					}
					else
					{
						applicationDescriptors.Add(GetApplicationDescriptorFromCreatorUnsafe(entry.Key, entry.Value));
					}
				}
				return applicationDescriptors;
			}
		}

		Dictionary<string, IEDIClientApplicationDescriptor> ApplicationDescriptorsDictionaryUnsafe
		{
			get { return applicationDescriptorsDictionary ?? (applicationDescriptorsDictionary = new Dictionary<string, IEDIClientApplicationDescriptor>(ApplicationDescriptorCreatorDictionaryUnsafe.Count)); }
		}

		Dictionary<string, ApplicationDescriptorCreator> ApplicationDescriptorCreatorDictionaryUnsafe
		{
			get
			{
				if (applicationDescriptorCreatorDictionary == null || applicationDescriptorCreatorDictionary.Count == 0)
				{
					var objectHandleHashtable = (Hashtable)ObjectFactory.Get("EDIClientApplicationDescriptors");
					applicationDescriptorCreatorDictionary = new Dictionary<string, ApplicationDescriptorCreator>(objectHandleHashtable.Count);

					foreach (DictionaryEntry entry in objectHandleHashtable)
					{
						var key = (string)entry.Key;

						var handle = (ObjectHandle)entry.Value;
						applicationDescriptorCreatorDictionary.Add((string)entry.Key, () => (IEDIClientApplicationDescriptor)handle.GetObject());
					}
				}

				return applicationDescriptorCreatorDictionary;
			}
		}
		IEDIClientApplicationDescriptor GetApplicationDescriptorFromCreatorUnsafe(string code, ApplicationDescriptorCreator creator)
		{
			var result = creator();

			//Creator delegate may already have added to dict, so override
			if (ApplicationDescriptorsDictionaryUnsafe.ContainsKey(code))
			{
				ApplicationDescriptorsDictionaryUnsafe.Remove(code);
				ErrorReporter.ReportOnce("DuplicateApplicationAdded", "A duplicate application with code " + code + " was attempted to be added to ApplicationDescriptors dictionary.");
			}

			ApplicationDescriptorsDictionaryUnsafe.Add(code, result);
			return result;
		}

#if DEBUG
		public IEDIClientApplicationDescriptor GetApplicationDescriptorFromAnyCreator(string code, Func<IEDIClientApplicationDescriptor> creator)
		{
			return GetApplicationDescriptorFromCreatorUnsafe(code, () => creator());
		}
#endif

		//lazy init for Properties
		Dictionary<string, ApplicationDescriptorCreator> applicationDescriptorCreatorDictionary;
		Dictionary<string, IEDIClientApplicationDescriptor> applicationDescriptorsDictionary;

		delegate IEDIClientApplicationDescriptor ApplicationDescriptorCreator();
	}
}
