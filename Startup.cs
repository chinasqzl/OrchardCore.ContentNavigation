using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentNavigation.Drivers;
using OrchardCore.ContentNavigation.Handlers;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.ContentNavigation
{
    public class Startup : StartupBase {
        public override void ConfigureServices(IServiceCollection services) {
			services.AddNavigation();
			services.AddShapeAttributes<ContentNavigationShapes>();

			services.AddScoped<IContentDisplayDriver, ListPartGroupDisplayDriver>();
			services.AddScoped<IContentDisplayDriver, ContainedPartDisplayDriver>();
			services.AddScoped<IContentPartHandler, ListPartGroupHandler>();

			services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(ContentNavigationFilter));
            });
		}

		public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
		{
			routes.MapAreaControllerRoute(
				name: "AdminContentNavigation",
				areaName: "OrchardCore.ContentNavigation",
				pattern: "Admin/Contents/ContentItems/{contentItemId}/Display/{groupId}",
				defaults: new { controller = "Admin", action = "Display" }
			);

			routes.MapAreaControllerRoute(
				name: "AdminEditContentNavigation",
				areaName: "OrchardCore.ContentNavigation",
				pattern: "Admin/Contents/ContentItems/{contentItemId}/Edit/{groupId}",
				defaults: new { controller = "Admin", action = "Edit" }
			);
		}
	}
}
