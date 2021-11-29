namespace DeliveryOrderProccesor
{
    public class CatalogItemOrdered // ValueObject
    {
        public CatalogItemOrdered(int catalogItemId, string productName, string pictureUri)
        {


            CatalogItemId = catalogItemId;
            ProductName = productName;
            PictureUri = pictureUri;
        }

        private CatalogItemOrdered()
        {
            // required by EF
        }

        public int CatalogItemId { get; private set; }
        public string ProductName { get; private set; }
        public string PictureUri { get; private set; }
    }
}