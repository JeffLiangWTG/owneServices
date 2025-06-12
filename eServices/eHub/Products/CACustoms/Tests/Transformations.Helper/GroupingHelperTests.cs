using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;
using CargoWise.eHub.Products.CACustoms.Transformations.Helper;

namespace CargoWise.eHub.Products.CACustoms.Tests.Transformations.Helper
{
    [TestClass]
    public class GroupingHelperTests
    {
        #region resources
        string strPackingLineCollectionForGrouping = @"
<PackingLineCollection>
  <PackingLine>
    <ID>ID1</ID>
    <PackedItemCollection>
      <PackedItem>
        <CommercialInvoiceLineLink>1</CommercialInvoiceLineLink>
      </PackedItem>
      <PackedItem>
        <CommercialInvoiceLineLink>2</CommercialInvoiceLineLink>
      </PackedItem>
      <PackedItem>
        <CommercialInvoiceLineLink>3</CommercialInvoiceLineLink>
      </PackedItem>
    </PackedItemCollection>
    <PackingLineCollection>
      <PackingLine>
        <ID>ID1.1</ID>
        <PackedItemCollection>
          <PackedItem>
            <CommercialInvoiceLineLink>1</CommercialInvoiceLineLink>
          </PackedItem>
        </PackedItemCollection>
        <PackingLineCollection>
        </PackingLineCollection>
      </PackingLine>
      <PackingLine>
        <ID>ID1.2</ID>
        <PackedItemCollection>
          <PackedItem>
            <CommercialInvoiceLineLink>2</CommercialInvoiceLineLink>
          </PackedItem>
          <PackedItem>
            <CommercialInvoiceLineLink>3</CommercialInvoiceLineLink>
          </PackedItem>
          <PackedItem>
            <CommercialInvoiceLineLink>1</CommercialInvoiceLineLink>
          </PackedItem>
        </PackedItemCollection>
        <PackingLineCollection>
        </PackingLineCollection>
      </PackingLine>
      <PackingLine>
        <ID>ID1.3</ID>
        <PackingLineCollection>
        </PackingLineCollection>
      </PackingLine>
      <PackingLine>
        <ID>ID1.4</ID>
        <PackedItemCollection>
        </PackedItemCollection>
      </PackingLine>
      <PackingLine>
        <ID>ID1.5</ID>
        <PackedItemCollection>
            <PackedItem>
          </PackedItem>
        </PackedItemCollection>
      </PackingLine>
    </PackingLineCollection>
  </PackingLine>
  <PackingLine>
    <ID>ID2</ID>
    <PackedItemCollection>
      <PackedItem>
        <CommercialInvoiceLineLink>1</CommercialInvoiceLineLink>
      </PackedItem>
      <PackedItem>
        <CommercialInvoiceLineLink>2</CommercialInvoiceLineLink>
      </PackedItem>
      <PackedItem>
        <CommercialInvoiceLineLink>3</CommercialInvoiceLineLink>
      </PackedItem>
    </PackedItemCollection>
    <PackingLineCollection>
      <PackingLine>
        <ID>ID2.1</ID>
        <PackedItemCollection>
          <PackedItem>
            <CommercialInvoiceLineLink>2</CommercialInvoiceLineLink>
          </PackedItem>
          <PackedItem>
            <CommercialInvoiceLineLink>3</CommercialInvoiceLineLink>
          </PackedItem>
          <PackedItem>
            <CommercialInvoiceLineLink>1</CommercialInvoiceLineLink>
          </PackedItem>
        </PackedItemCollection>
        <PackingLineCollection>
        </PackingLineCollection>
      </PackingLine>
      <PackingLine>
        <ID>ID2.2</ID>
        <PackedItemCollection>
          <PackedItem>
            <CommercialInvoiceLineLink>2</CommercialInvoiceLineLink>
          </PackedItem>
          <PackedItem>
            <CommercialInvoiceLineLink>3</CommercialInvoiceLineLink>
          </PackedItem>
          <PackedItem>
            <CommercialInvoiceLineLink>1</CommercialInvoiceLineLink>
          </PackedItem>
        </PackedItemCollection>
      </PackingLine>
    </PackingLineCollection>
  </PackingLine>
</PackingLineCollection>
";

string strCommercialInvoiceLinesForGrouping = @"
<CommercialInvoiceLineCollection>
    <CommercialInvoiceLine>
        <ID>ID1</ID>
        <Link>1</Link>
    </CommercialInvoiceLine>
    <CommercialInvoiceLine>
        <ID>ID2</ID>
        <Link>2</Link>
    </CommercialInvoiceLine>
    <CommercialInvoiceLine>
        <ID>ID3</ID>
        <Link>3</Link>
    </CommercialInvoiceLine>
</CommercialInvoiceLineCollection>";
        #endregion

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void Group()
        {
            var PackingLineCollection = XDocument.Parse(strPackingLineCollectionForGrouping).CreateNavigator().Select("PackingLineCollection");
            var CommercialInvoiceLineCollection = XDocument.Parse(strCommercialInvoiceLinesForGrouping).CreateNavigator().Select("CommercialInvoiceLineCollection");

            var groups = CACustomsGroupingHelper.GroupByPackingLine(CommercialInvoiceLineCollection, PackingLineCollection);
            Assert.AreEqual(@"<Groups>
  <Group>
    <CommercialInvoiceLineID>ID1</CommercialInvoiceLineID>
  </Group>
  <Group>
    <CommercialInvoiceLineID>ID2</CommercialInvoiceLineID>
    <CommercialInvoiceLineID>ID3</CommercialInvoiceLineID>
  </Group>
</Groups>", groups.OuterXml);
        }
    }
}
