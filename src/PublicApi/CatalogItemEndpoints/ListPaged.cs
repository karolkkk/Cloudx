using Ardalis.ApiEndpoints;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints
{
    public class ListPaged : BaseAsyncEndpoint
        .WithRequest<ListPagedCatalogItemRequest>
        .WithResponse<ListPagedCatalogItemResponse>
    {
        private readonly IAsyncRepository<CatalogItem> _itemRepository;
        private readonly IUriComposer _uriComposer;
        private readonly IMapper _mapper;
        ILogger<ListPaged> _logger { get; set; }

        public ListPaged(IAsyncRepository<CatalogItem> itemRepository,
            IUriComposer uriComposer,
            IMapper mapper,
            ILogger<ListPaged> logger)
        {
            _itemRepository = itemRepository;
            _uriComposer = uriComposer;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("api/catalog-items")]
        [SwaggerOperation(
            Summary = "List Catalog Items (paged)",
            Description = "List Catalog Items (paged)",
            OperationId = "catalog-items.ListPaged",
            Tags = new[] { "CatalogItemEndpoints" })
        ]
        public override async Task<ActionResult<ListPagedCatalogItemResponse>> HandleAsync([FromQuery] ListPagedCatalogItemRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Hey, I'm working!1");
            var response = new ListPagedCatalogItemResponse(request.CorrelationId());
            _logger.LogInformation("Hey, I'm working!");
            var filterSpec = new CatalogFilterSpecification(request.CatalogBrandId, request.CatalogTypeId);
            int totalItems = await _itemRepository.CountAsync(filterSpec, cancellationToken);

            var pagedSpec = new CatalogFilterPaginatedSpecification(
                skip: request.PageIndex * request.PageSize,
                take: request.PageSize,
                brandId: request.CatalogBrandId,
                typeId: request.CatalogTypeId);

            var items = await _itemRepository.ListAsync(pagedSpec, cancellationToken);

            response.CatalogItems.AddRange(items.Select(_mapper.Map<CatalogItemDto>));
            foreach (CatalogItemDto item in response.CatalogItems)
            {
                item.PictureUri = _uriComposer.ComposePicUri(item.PictureUri);
            }
            response.PageCount = int.Parse(Math.Ceiling((decimal)totalItems / request.PageSize).ToString());

            return Ok(response);
        }
    }
}
