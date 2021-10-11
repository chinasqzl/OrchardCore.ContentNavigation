using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Lists.Models;

namespace OrchardCore.ContentNavigation.Handlers
{
    public class ListPartGroupHandler : ContentPartHandler<ListPart>
	{
		private readonly IContentDefinitionManager _contentDefinitionManager;
		private readonly IUpdateModelAccessor _updateModelAccessor;
		private readonly IServiceProvider _serviceProvider;

		private readonly IStringLocalizer T;

		public ListPartGroupHandler(IContentDefinitionManager contentDefinitionManager,
			IUpdateModelAccessor updateModelAccessor,
			IServiceProvider serviceProvider,
			IStringLocalizer<ListPartGroupHandler> stringLocalizer) {
			_contentDefinitionManager = contentDefinitionManager;
			_updateModelAccessor = updateModelAccessor;
			_serviceProvider = serviceProvider;

			T = stringLocalizer;
		}

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, ListPart part)
        {
            return context.ForAsync<ContentItemMetadata>(aspect =>
            {
				var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
				var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(ListPart));
				var settings = contentTypePartDefinition.GetSettings<ListPartSettings>();
				foreach(var contentType in settings.ContainedContentTypes)
                {
					var ctd = _contentDefinitionManager.GetTypeDefinition(contentType);
                    if (ctd != null)
                    {
						aspect.DisplayGroupInfo.Add(new GroupInfo(T[ctd.DisplayName]) { Id="List-"+contentType,Position="9" });
                    }
                }
				return Task.CompletedTask;
            });
        }
    }
}