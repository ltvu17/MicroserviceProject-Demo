using System.ComponentModel.DataAnnotations;
using Play.Common;

namespace Play.Catalog.Service
{
    public class Item : IEntity
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTimeOffset CreateDate { get; set; }
    }
}