using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Contracts;
using Play.Common;


namespace Play.Catalog.Service
{   
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : Controller
    {
        private readonly IRepoBase<Item> _itemRepo;
        private readonly IPublishEndpoint _publishEndpoint;
        public ItemsController(IRepoBase<Item> itemRepo, IPublishEndpoint publishEndpoint)
        {
            _itemRepo = itemRepo;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
        {
            var items = (await _itemRepo.GetAllAsync()).Select(items => items.Mapping());
            return Ok(items);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var item = await _itemRepo.GetAsync(id);
            if(item == null)
            {
                return NotFound();
            }
            return Ok(item.Mapping());
        }
        [HttpPost]
        public async Task<IActionResult> PostAsync(CreateItemDto createItemDto)
        {
            var item = new Item{
                Id = Guid.NewGuid(),
                Name = createItemDto.Name,
                Price = createItemDto.Price,
                Description = createItemDto.Description,
                CreateDate = DateTimeOffset.UtcNow
            };
            await _publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));
            await _itemRepo.CreateAsync(item);
            return CreatedAtAction(nameof(GetByIdAsync), new {id = item.Id}, item);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto updateItemDto)
        {
            var existingItem = await _itemRepo.GetAsync(id);
            if(existingItem == null)
            {
                return NotFound();
            }
            existingItem.Name = updateItemDto.Name;
            existingItem.Description = updateItemDto.Description;
            existingItem.Price = updateItemDto.Price;
            await _publishEndpoint.Publish(new CatalogItemCreated(existingItem.Id, existingItem.Name, existingItem.Description));
            await _itemRepo.UpdateAsync(existingItem);
            return Ok(new {
                mesage = "Updated"
            });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var item = await _itemRepo.GetAsync(id);
            if(item == null)
            {
                return NotFound();
            }
            await _itemRepo.RemoveAsync(item.Id);
            await _publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));
            return Ok(new {
                mesage = "Removed"
            }); 
        }
    }
}