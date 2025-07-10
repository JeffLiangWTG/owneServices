using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class DpsComplianceListItem
	{
		public DpsComplianceListItem(ZString code, ZString name, ZString description, ZString publisher, ZString contact)
		{
			Code = code;
			Name = name;
			Description = description;
			Publisher = publisher;
			Contact = contact;
		}

		public ZString Code { get; }
		public ZString Name { get; }
		public ZString Description { get; }
		public ZString Publisher { get; }
		public ZString Contact { get; }
	}
}
