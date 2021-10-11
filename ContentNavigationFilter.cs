using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.ContentNavigation
{
    public class ContentNavigationFilter: IAsyncResultFilter
    {
        private readonly ILayoutAccessor _layoutAccessor;
        private readonly IShapeFactory _shapeFactory;
        private readonly IContentManager _contentManager;

        public ContentNavigationFilter(
            ILayoutAccessor layoutAccessor,
            IShapeFactory shapeFactory,
            IContentManager contentManager)
        {
            _layoutAccessor = layoutAccessor;
            _shapeFactory = shapeFactory;
            _contentManager = contentManager;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            // Should only run on the front-end (or optionally also on the admin) for a full view.
            if ((context.Result is ViewResult || context.Result is PageResult))
            {
                var area = Convert.ToString(context.RouteData.Values["area"]);
                var controller = Convert.ToString(context.RouteData.Values["controller"]);
                var action = Convert.ToString(context.RouteData.Values["action"]);
                var contentItemId = Convert.ToString(context.RouteData.Values["contentItemId"]);
                if((String.Equals("OrchardCore.Contents", area, StringComparison.OrdinalIgnoreCase)
                    && String.Equals("Admin", controller, StringComparison.OrdinalIgnoreCase)
                    && String.Equals("Display", action, StringComparison.OrdinalIgnoreCase)
                    ) || (String.Equals("OrchardCore.ContentNavigation", area, StringComparison.OrdinalIgnoreCase)
                    && String.Equals("Admin", controller, StringComparison.OrdinalIgnoreCase)
                    && String.Equals("Display", action, StringComparison.OrdinalIgnoreCase)
                    ))
                {
                    var layout = await _layoutAccessor.GetLayoutAsync();
                    var tabsZone = layout.Zones["Tabs"];

                    if (tabsZone is Shape shape)
                    {
                        var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);
                        await shape.AddAsync(await _shapeFactory.CreateAsync("ContentNavigation",
                            Arguments.From(new { ContentItem = contentItem })));
                    }
                }
            }

            await next.Invoke();
        }
    }
}
