using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : Controller
    {
        private readonly IRepoBase<InventoryItem> _inventoryRepo;
        private readonly IRepoBase<CatalogItem> _catalogRepo;
        public ItemsController(IRepoBase<InventoryItem> inventoryRepo, IRepoBase<CatalogItem> catalogRepo)
        {
            _catalogRepo = catalogRepo;
            _inventoryRepo = inventoryRepo;
        }
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAsync(Guid userId)
        {
            if(userId == Guid.Empty)
            {
                return BadRequest();
            }
            var catalogItems = await _catalogRepo.GetAllAsync();

            var items = await _inventoryRepo.GetAllAsync(item => item.UserId == userId);
            var inventoryItemDTO= items.Select(item => {
                            var catalogItem = catalogItems.Single(catalogItem=> catalogItem.Id == item.CatalogItemId);
                            return item.Mapping(catalogItem.Name, catalogItem.Description);
                        });
            return Ok(inventoryItemDTO);
        }
        [HttpPost]
        public async Task<IActionResult> PostAsync(GrantItemDto grantItemDto)
        {
            var inventoryItem = await _inventoryRepo.GetAsync(item=> item.UserId == grantItemDto.UserId 
                                && item.CatalogItemId == grantItemDto.CatalogItemId);
            if(inventoryItem == null)
            {
                inventoryItem = new InventoryItem
                {
                    CatalogItemId = grantItemDto.CatalogItemId,
                    UserId = grantItemDto.UserId,
                    Quantity= grantItemDto.Quantity,
                    AccquiredDate = DateTimeOffset.UtcNow
                };

                await _inventoryRepo.CreateAsync(inventoryItem);

            }
            else
            {
                inventoryItem.Quantity += grantItemDto.Quantity;
                await _inventoryRepo.UpdateAsync(inventoryItem);
            }
            return Ok();
        }
    }
}