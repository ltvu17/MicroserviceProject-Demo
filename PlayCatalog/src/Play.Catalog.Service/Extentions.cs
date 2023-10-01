namespace Play.Catalog.Service
{
    public static class Extentions
    {
        public static ItemDto Mapping(this Item item)
        {
            return new ItemDto(item.Id, item.Name,item.Description, item.Price, item.CreateDate);
        } 
    }
}