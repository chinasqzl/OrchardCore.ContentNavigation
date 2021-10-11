using System;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lists.Models;

namespace OrchardCore.ContentNavigation.Drivers
{
    public class ContainedPartDisplayDriver : ContentDisplayDriver
    {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        public ContainedPartDisplayDriver(
			IContentManager contentManager,
			IContentDefinitionManager contentDefinitionManager
			) {
            _contentManager = contentManager;
			_contentDefinitionManager = contentDefinitionManager;
		}

        public override IDisplayResult Display(ContentItem model, IUpdateModel updater)
        {
            var part = model.As<ContainedPart>();
            if (part != null)
            {
                return Dynamic(nameof(ContainedPart), async shape =>
                {
                    var parent = await _contentManager.GetAsync(part.ListContentItemId);
                    var contentDefinition = _contentDefinitionManager.GetTypeDefinition(parent.ContentType);
                    shape.ContentItem = parent;
                    shape.ContentTypeName = contentDefinition.DisplayName;
                }).Displaying(ctx =>
                    {
                        var displayType = ctx.Shape.Metadata.DisplayType;
                        if (!String.IsNullOrEmpty(displayType) && displayType != "Detail")
                        {
                            ctx.Shape.Metadata.Alternates.Add($"ContainedPart_{ctx.Shape.Metadata.DisplayType}");
                        }
                    })
                .Location("Detail", "Content")
                .Location("DetailAdmin", "Content");
            }

            return null;
        }
    }
}
