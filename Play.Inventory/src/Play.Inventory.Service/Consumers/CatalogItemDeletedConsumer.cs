using MassTransit;
using Play.Catalog.Contracts;
using Play.Common;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemDeletedConsumer : IConsumer<CatalogItemCreated>
    {
        private readonly IRepoBase<CatalogItem> _repoBase;
        public CatalogItemDeletedConsumer(IRepoBase<CatalogItem> repoBase)
        {   
            _repoBase = repoBase;
            
        }
        public async Task Consume(ConsumeContext<CatalogItemCreated> context)
        {
            var message = context.Message;
            var item = await _repoBase.GetAsync(message.ItemId);

            if(item == null){
                return;
            }
            else
            {
                await _repoBase.RemoveAsync(message.ItemId);
            }
            
        
        }
    }
}