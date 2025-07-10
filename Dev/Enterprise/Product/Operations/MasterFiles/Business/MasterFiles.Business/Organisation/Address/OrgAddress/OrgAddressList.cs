using System.Collections;

using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgAddressList : IEnumerable
	{
		public OrgAddressList()
		{
			fOrgAddressList = new ArrayList();
		}

		public void Add(OrgAddress address)
		{
			fOrgAddressList.Add(address);
		}

		public void AddRange(OrgAddressList list)
		{
			foreach (OrgAddress address in list)
			{
				Add(address);
			}
		}

		public void AddRange(OrgAddress[] list)
		{
			fOrgAddressList.AddRange(list);
		}

		public OrgAddress this[int index]
		{
			get { return (OrgAddress)fOrgAddressList[index]; }
		}

		public ZInt Count
		{
			get { return fOrgAddressList.Count; }
		}

		protected ArrayList fOrgAddressList;

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)fOrgAddressList).GetEnumerator();
		}

		#endregion
	}
}
