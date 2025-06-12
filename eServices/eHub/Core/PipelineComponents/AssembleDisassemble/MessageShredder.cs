using System;
using System.Collections;
using System.Runtime.InteropServices;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents
{
	[Serializable]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("04F235E2-EF0D-4A9E-830D-6A9567AEE3CF")]
	public class MessageShredder : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region properties

		public string Name
		{
			get
			{
				return "MessageShredder";
			}
		}

		public string Version
		{
			get
			{
				return "1.0.0";
			}
		}

		public string Description
		{
			get
			{
				return "Return no message after processing";
			}
		}

		public void GetClassID(out System.Guid classid)
		{
			classid = new System.Guid("32E79253-4E0E-4473-971E-620007561546");
		}

		#endregion

		public IEnumerator Validate(object obj)
		{
			return null;
		}

		public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage message)
		{
			return null;
		}

		#region Empty methods

		public IntPtr Icon
		{
			get { return IntPtr.Zero; }
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
		}

		#endregion
	}
}
