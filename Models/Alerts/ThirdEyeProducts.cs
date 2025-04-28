
public class ThirdEyeProductsPayload
{
    public Product[] products { get; set; }
}

public class Product
{
    public string body_html { get; set; } = string.Empty;
    public string body { get; set; } = string.Empty;
    public DateTime created_at { get; set; }
    public string handle { get; set; }
    public long id { get; set; }
    public Image[] images { get; set; }
    public Option[] options { get; set; }
    public string product_type { get; set; }
    public DateTime published_at { get; set; }
    public string[] tags { get; set; }
    public string title { get; set; }
    public DateTime updated_at { get; set; }
    public Variant[] variants { get; set; }
    public string vendor { get; set; }
}

public class Image
{
    public DateTime created_at { get; set; }
    public int height { get; set; }
    public long id { get; set; }
    public int position { get; set; }
    public long product_id { get; set; }
    public string src { get; set; }
    public DateTime updated_at { get; set; }
    public object[] variant_ids { get; set; }
    public int width { get; set; }
}

public class Option
{
    public string name { get; set; }
    public int position { get; set; }
    public string[] values { get; set; }
}

public class Variant
{
    public bool available { get; set; }
    public object compare_at_price { get; set; }
    public DateTime created_at { get; set; }
    public object featured_image { get; set; }
    public int grams { get; set; }
    public long id { get; set; }
    public string option1 { get; set; }
    public object option2 { get; set; }
    public object option3 { get; set; }
    public int position { get; set; }
    public string price { get; set; }
    public long product_id { get; set; }
    public bool requires_shipping { get; set; }
    public string sku { get; set; }
    public bool taxable { get; set; }
    public string title { get; set; }
    public DateTime updated_at { get; set; }
}



public class ThirdEyeSearchResponse
{
    public List<ThirdEyeSearchProducts> Products { get; set; } = new List<ThirdEyeSearchProducts>();
}

public class ThirdEyeSearchProducts
{
    public long id { get; set; }
    public string title { get; set; }
    public string handle { get; set; }
    public string price { get; set; }
    public string price_max { get; set; }
    public string price_min { get; set; }
    public string type { get; set; }
    public string created_at { get; set; }
    public string published_at { get; set; }
    public List<string> tags { get; set; } = new List<string>();
    public string vendor { get; set; }
    public string featured_image { get; set; }
    public string url { get; set; }
    public long?[] collections { get; set; }
    public string compare_at_price { get; set; }
    public string compare_at_price_max { get; set; }
    public string compare_at_price_min { get; set; }
    public ThirdEyeSearchResponseImages[] images { get; set; }
    public object first_available_variant { get; set; }
    public bool? available { get; set; }
    public ThirdEyeSearchResponseVariants[] variants { get; set; }
}

public class ThirdEyeSearchResponseImages
{
    public long id { get; set; }
    public string src { get; set; }
}

public class ThirdEyeSearchResponseVariants
{
    public long id { get; set; }
    public string title { get; set; }
    public object name { get; set; }
    public int price { get; set; }
    public string compare_at_price { get; set; }
    public bool available { get; set; }
    public string sku { get; set; }
    public string weight { get; set; }
    public string weight_unit { get; set; }
    public int inventory_quantity { get; set; }
}



public class ThirdEyeListingPageDetails
{
    public string context { get; set; }
    public string type { get; set; }
    public string id { get; set; }
    public Mainentityofpage mainEntityOfPage { get; set; }
    public Additionalproperty[] additionalProperty { get; set; }
    public Brand brand { get; set; }
    public string category { get; set; }
    public string color { get; set; }
    public string depth { get; set; }
    public string height { get; set; }
    public string itemCondition { get; set; }
    public string logo { get; set; }
    public string manufacturer { get; set; }
    public string material { get; set; }
    public string model { get; set; }
    public Offers offers { get; set; }
    public string productID { get; set; }
    public string productionDate { get; set; }
    public string purchaseDate { get; set; }
    public string releaseDate { get; set; }
    public string review { get; set; }
    public string sku { get; set; }
    public string mpn { get; set; }
    public Weight weight { get; set; }
    public string width { get; set; }
    public string description { get; set; }
    public string[] image { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public Itemlistelement[] itemListElement { get; set; }
}

public class Mainentityofpage
{
    public string type { get; set; }
    public string id { get; set; }
    public string sdDatePublished { get; set; }
    public Sdpublisher sdPublisher { get; set; }
}

public class Sdpublisher
{
    public string context { get; set; }
    public string type { get; set; }
    public string id { get; set; }
    public string name { get; set; }
}

public class Brand
{
    public string type { get; set; }
    public string name { get; set; }
    public string url { get; set; }
}

public class Offers
{
    public string type { get; set; }
    public string id { get; set; }
    public string availability { get; set; }
    public Inventorylevel inventoryLevel { get; set; }
    public float price { get; set; }
    public string priceCurrency { get; set; }
    public string description { get; set; }
    public string priceValidUntil { get; set; }
    public Seller seller { get; set; }
}

public class Inventorylevel
{
    public string type { get; set; }
    public string value { get; set; }
}

public class Seller
{
    public string type { get; set; }
    public string name { get; set; }
    public string id { get; set; }
}

public class Weight
{
    public string type { get; set; }
    public string unitCode { get; set; }
    public string value { get; set; }
}

public class Additionalproperty
{
    public string type { get; set; }
    public string name { get; set; }
    public string[] value { get; set; }
}

public class Itemlistelement
{
    public string type { get; set; }
    public int position { get; set; }
    public Item item { get; set; }
}

public class Item
{
    public string id { get; set; }
    public string name { get; set; }
}



public class ThirdEyeListingPageDetailsPayload
{
    public ThirdEyeListingPageDetails[] Details { get; set; }
}
