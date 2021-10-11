using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.ContentNavigation
{
	public class ContentNavigationShapes : IShapeAttributeProvider
	{
		private readonly IStringLocalizer T;

		public ContentNavigationShapes(IStringLocalizer<ContentNavigationShapes> localizer)
		{
			T = localizer;
		}

		[Shape]
		public async Task<IHtmlContent> ContentNavigation(Shape Shape, dynamic DisplayAsync, dynamic New, IHtmlHelper Html, DisplayContext DisplayContext,
			IEnumerable<string> ItemClasses,
			IDictionary<string, string> ItemAttributes)
		{
			Shape.Classes.Add("nav");
			Shape.Metadata.Alternates.Clear();
			Shape.Metadata.Type = "List";
			Shape.Properties["ItemClasses"] = new string[] { "nav-item" };

			dynamic shape = Shape;
			ContentItem contentItem = shape.ContentItem;
			var contentManager = DisplayContext.ServiceProvider.GetRequiredService<IContentManager>();
			var contentItemMetadata = (await contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem));

			var routeData = new RouteValueDictionary(Html.ViewContext.RouteData.Values);
			var groupId = "";
			if (routeData.ContainsKey("GroupId"))
			{
				groupId = routeData["GroupId"].ToString();
			}

			var fn = await New.ContentNavigation_Link(Value: "详情", active: string.IsNullOrWhiteSpace(groupId), RouteValues: contentItemMetadata.AdminRouteValues);
			await Shape.AddAsync((object)fn);
			foreach (var group in contentItemMetadata.DisplayGroupInfo.OrderBy(g=>g.Position))
			{
				var active = group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase);
				var navItem = await New.ContentNavigation_Link(Value: group.Name, active: active, RouteValues: new RouteValueDictionary{
						{"Area", "OrchardCore.ContentNavigation"},
						{"Controller", "Admin"},
						{"Action", "Display"},
						{"ContentItemId", contentItem.ContentItemId},
						{"GroupId", group.Id}
					});
				await Shape.AddAsync((object)navItem);
			}
			return await DisplayAsync(shape);
		}

		[Shape]
		public Task<IHtmlContent> ContentNavigation_Link(Shape shape, IHtmlHelper Html, dynamic DisplayAsync, object Value, bool active)
		{
			shape.Metadata.Alternates.Clear();
			shape.Metadata.Type = "ActionLink";
			shape.Metadata.Alternates.Add("ActionLink__" + EncodeAlternateElement(Value.ToString()));
			shape.Classes.Add("nav-link");
			if (active) shape.Classes.Add("active");
			return DisplayAsync(shape);
		}

		/// <summary>
		/// Encodes dashed and dots so that they don't conflict in filenames
		/// </summary>
		/// <param name="alternateElement"></param>
		/// <returns></returns>
		private string EncodeAlternateElement(string alternateElement)
		{
			return alternateElement.Replace("-", "__").Replace(".", "_");
		}
	}
}
