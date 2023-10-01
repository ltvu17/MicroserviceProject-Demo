using MassTransit;
using Play.Catalog.Contracts;
using Play.Common;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemCreatedConsumer : IConsumer<CatalogItemCreated>
    {
        private readonly IRepoBase<CatalogItem> _repoBase;
        public CatalogItemCreatedConsumer(IRepoBase<CatalogItem> repoBase)
        {   
            _repoBase = repoBase;
            
        }
        public async Task Consume(ConsumeContext<CatalogItemCreated> context)
        {
            var message = context.Message;
            var item = await _repoBase.GetAsync(message.ItemId);

            if(item != null){
                return;
            }

            item = new CatalogItem
            {
                Id = message.ItemId,
                Name = message.Name,
                Description = message.Description
            };
            await _repoBase.CreateAsync(item);
        
        }
    }
}