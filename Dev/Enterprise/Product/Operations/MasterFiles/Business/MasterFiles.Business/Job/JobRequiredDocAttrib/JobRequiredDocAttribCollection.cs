using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class JobRequiredDocAttribCollection : ActiveBusinessObjectCollection<JobRequiredDocAttrib>
	{
		public JobRequiredDocAttribCollection(JobRequiredDocument master)
			: base(master)
		{
		}

		public JobRequiredDocAttrib this[ZString type, ZString value]
		{
			get
			{
				JobRequiredDocAttrib result = null;
				foreach (JobRequiredDocAttrib attrib in this)
				{
					if (attrib.D0_AttribName == type && (attrib.D0_AttribDisplayValue.IsEmpty || attrib.D0_AttribDisplayValue == value))
					{
						result = attrib;
						break;
					}
				}
				return result;
			}
		}

		public JobRequiredDocAttrib this[string type]
		{
			get
			{
				return this.FirstOrDefault(x => x.D0_AttribName == type);
			}
		}

		public bool HasAttributeType(ZString type)
		{
			bool result = false;
			foreach (JobRequiredDocAttrib attrib in this)
			{
				if (attrib.D0_AttribName == type)
				{
					result = true;
					break;
				}
			}
			return result;
		}
	}
}
